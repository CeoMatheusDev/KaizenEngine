# Metal Rendering Driver (R0 follow-up)

> **Status:** Source-verified — **no runtime validation on Apple hardware**
> **Verified against revision:** `257040e` (2026-11-18)
> **Related:** [Rendering audit (R0)](rendering-audit.md), [Pipeline cache](rendering-pipeline-cache.md), [Rendering benchmark methodology](rendering-benchmarks.md), [Rendering roadmap](rendering-roadmap.md)

This document describes the Metal backend as it exists in the source tree: how it is built and selected, how shaders reach Metal, how memory and synchronization are handled, and what is stubbed. No Apple device was available, so all statements are code-level.

## 1. Selection and build gating

- On macOS, `rendering/rendering_device/driver.macos` defaults to `metal` (`main/main.cpp:2395`). iOS and visionOS default to `metal` as well; visionOS accepts only `metal` (`main.cpp:2393-2394`).
- The build option `metal` defaults to **disabled** (`SConstruct:209`). It must be enabled explicitly, is only supported on Apple arm64 (`platform/macos/detect.py:91`, `:333-338`), and on a non-arm64 target it is silently turned off with a warning.
- `METAL_ENABLED` is therefore defined **only in arm64 macOS builds** (`detect.py:337-338`). Consequences:
  - The Metal rendering driver is compiled only there (`drivers/SCsub:54-58`).
  - The Metal shader-baking export plugin is registered only there (`editor/editor_node.cpp:9708-9712`).
- The display server registers `metal` as a driver and branches on the driver name when creating a window's platform data (`platform/macos/display_server_macos.mm:202-204`, `:3629`).

## 2. Driver structure

- Two layers: a context driver (`rendering_context_driver_metal.cpp`) that owns the device, swapchain/surface and window integration, and a rendering-device driver (`rendering_device_driver_metal.cpp`) that implements `RenderingDeviceDriver`.
- The instance actually created is the Metal 3 subclass, `MTL3::RenderingDeviceDriverMetal` (`rendering_context_driver_metal.cpp:109`), which adds command queues, fences/semaphores, swapchain presentation and timestamp queries on top of the base driver.
- Copy work uses a dedicated command queue plus a 64 KiB shared scratch buffer; when the buffer fills, the copy queue is flushed (`rendering_device_driver_metal.cpp:2490-2506`).
- The driver has an internal, in-memory shader library cache keyed by SHA-256 of the generated shader source (`rendering_device_driver_metal.cpp:1122-1141`, `:1204-1258`). It is not persisted.

## 3. Shader path: SPIR-V to MSL to metallib

1. The engine produces SPIR-V for RD shaders; the Metal shader container translates it to Metal Shading Language through SPIRV-Cross and reads bindings through spirv-reflect (`drivers/metal/rendering_shader_container_metal.cpp:41-44`).
2. MSL version and feature use come from a `MetalDeviceProfile`: MSL 2.3 through 4.0 by minimum OS version, argument buffers enabled from macOS 13 / iOS 16 and GPU family Apple6+ (`drivers/metal/metal_device_profile.cpp:54-120`).
3. **At export time on an arm64 macOS editor**, the shader baker (`editor/shader/shader_baker/shader_baker_export_plugin_platform_metal.cpp:37-55`) builds containers in `export_mode`, compiles the MSL with `xcrun metal -O3` into a `.metallib`, and appends that binary after the source in the shader blob (`rendering_shader_container_metal.cpp:114-190`, `:725-745`). The bake profile is Apple7 for macOS/iOS and Apple8 for visionOS, with the preset's minimum OS version.
   - If the toolchain cannot be probed (no Xcode, non-macOS host), the source is still generated but no `.metallib` is embedded; the warning *"Metal shader baking limited to SPIR-V"* is printed (`:738`).
   - If a shader needs a higher GPU family or MSL version than the bake profile provides, the container is marked invalid so the device recompiles it at runtime (`:330-350`).
4. **At runtime**, if a baked library is present the driver loads it with `MTL::Device::newLibrary(dispatch_data)`; otherwise it compiles the embedded MSL source with `newLibrary(source)` (`rendering_device_driver_metal.cpp:1235-1258`, `drivers/metal/metal_objects_shared.cpp:918-958`). There is **no fallback from a failed binary library to the source**.
5. Compilation is asynchronous by default on macOS (`MDImmediateLibrary`, Metal's async compilation API); `KAIZEN_MTL_SHADER_LOAD_STRATEGY=lazy` switches to loading on first use, and iOS/tvOS/visionOS always use the lazy strategy (`rendering_device_driver_metal.cpp:2878-2885`, `metal_objects_shared.cpp:771-960`).
6. Runtime compatibility checks reject a container compiled for a newer MSL version, a newer GPU family, or with argument buffers on a device that lacks them (`rendering_device_driver_metal.cpp:1172-1195`). The driver returns an invalid `ShaderID`, and the shader-cache group is discarded because its variants become null (`servers/rendering/renderer_rd/shader_rd.cpp:756-800`); the exact recompile path above that layer was not traced.

**Consequence:** an exported game built on an arm64 Mac ships precompiled Metal libraries and only has to create pipeline state objects. The editor, and exports produced from a non-Mac host, compile MSL on the device.

## 4. Pipeline state cache remains disabled

This is the same finding as [Pipeline cache](rendering-pipeline-cache.md), restated here because it interacts with the shader path:

- `pipeline_cache_create()` returns `false`; the `MTLBinaryArchive` implementation is commented out with a TODO to re-enable it (`rendering_device_driver_metal.cpp:1640-1675`).
- The archive member exists and pipeline creation consults it if present (`:2255-2265`, `:2347-2357`), but nothing ever creates it, so it is always null. `KAIZEN_MTL_ARCHIVE_FAIL_ON_MISS` is therefore currently inert.
- Baked `.metallib` files remove MSL compilation, but **render/compute pipeline state creation still happens on every run** and is not cached across runs.

## 5. Memory and synchronization

- Default synchronization is `Barriers` (`rendering_device_driver_metal.h:164`). In this mode the driver uses a placement-heap suballocator built on OffsetAllocator instead of per-resource `MTL::Device` allocations (`rendering_device_driver_metal.cpp:3014-3023`).
- Heap pools are keyed by storage mode: private (including memoryless textures), shared, and shared write-combined; the allocator unit is 256 bytes and each block holds up to 16K allocations (`drivers/metal/metal_allocator.h:145-166`).
- `KAIZEN_MTL_SYNC_MODE=none` selects Metal's native hazard tracking, which switches allocation to the passthrough device allocator and disables the engine's barrier contract (`rendering_device_driver_metal.cpp:2871-2876`, `:2771-2780`).
- Reported API traits (`rendering_device_driver_metal.cpp:2773-2781`): barriers are honored only in barrier mode; buffer clears may use the copy engine; texture clears do not; textures do **not** require layout transitions.
- Residency: textures are added to and removed from a main residency set (`rendering_device_driver_metal.cpp:465-467`, `:584-586`), but the Metal 3 hooks that attach/detach residency sets on the main queue are empty (`rendering_device_driver_metal3.cpp:114-118`).
- Total memory used is reported as `MTL::Device::currentAllocatedSize()`; lazily allocated (memoryless) memory is reported as `0` with a TODO (`rendering_device_driver_metal.cpp:2646-2654`).

## 6. Feature support reported by the driver

From `has_feature()` (`rendering_device_driver_metal.cpp:2783-2822`):

| Feature | Reported |
|---|---|
| Half float, fragment shader with only side effects, point size, HDR output, Vulkan memory model | Yes |
| Buffer device address | Conditional on `supports_gpu_address` |
| 32-bit image atomics | Conditional on native image atomics and OS version |
| Framebuffer depth resolve | Conditional on `supports_msaa_depth_resolve` |
| Rasterization rate map | Conditional on device support (two layers on visionOS) |
| GPU-mappable buffer | Yes |
| MetalFX spatial / temporal | Conditional; temporal requires the `METAL_MFXTEMPORAL_ENABLED` build define |
| Ray query / ray-tracing pipeline | **Not reported** (default `false`) |

Multiview is advertised when `features.layeredRendering` is present, with `max_view_count` from the device limits (`rendering_device_driver_metal.cpp:3043-3053`).

Argument buffers are used only when the device is Tier 2 **and** no argument encoders are needed, which requires the Metal 3 GPU family and macOS 13/iOS 16 (`drivers/metal/metal_device_properties.h:126-140`, `metal_device_properties.cpp:175`). They can be disabled with `KAIZEN_MTL_DISABLE_ARGUMENT_BUFFERS=1` (`metal_device_properties.cpp:178-180`); otherwise the driver binds resources into individual slots.

## 7. Timestamps and profiling

- The Metal 3 driver enables GPU timestamp queries only in barrier synchronization mode and only when the device supports counter sampling at stage boundaries (`rendering_device_driver_metal3.cpp:90-101`).
- When unavailable, query-pool creation falls back to the base implementation, which does not provide GPU timings (`rendering_device_driver_metal3.cpp:284-320`).
- Practical consequence for [benchmarking](rendering-benchmarks.md): `--gpu-profile` on Metal may legitimately report no GPU data, depending on device and sync mode. This must be recorded, not worked around.

## 8. Known stubs and hard limits

- `shader_destroy_modules()` is an empty TODO (`rendering_device_driver_metal.cpp:1351-1353`); Metal has no separate shader-module object, so the library lifetime is tied to the shader object.
- `command_insert_breadcrumb()` is a TODO (`:2485-2487`); GPU crash breadcrumbs are unavailable.
- Line width other than `1.0` is rejected with an error (`:1843-1847`).
- Subpass dependencies are asserted to form a sequential chain (`:1692`).
- MSAA resolve of an unsupported texture format calls `CRASH_NOW_MSG("unimplemented: ...")` (`drivers/metal/metal3_objects.cpp:1311`, `:1346`).
- Minimum runtime GPU family is Apple4 / A11 (2017); below that, the engine lists missing capabilities (image cube arrays among them) and fails device creation after showing an error (`rendering_device_driver_metal.cpp:3056-3068`).
- Pixel-format mismatches are handled by the format table, with a "lossy texture support" idea left as a TODO (`:271`).

## 9. Environment switches

These are read directly from the process environment; they are developer switches, not project settings:

| Variable | Effect |
|---|---|
| `KAIZEN_MTL_TARGET_VERSION` | Overrides the MSL version used to translate shaders (`metal_device_properties.cpp:88-106`) |
| `KAIZEN_MTL_DISABLE_IMAGE_ATOMICS=1` | Disables native image atomics |
| `KAIZEN_MTL_DISABLE_ARGUMENT_BUFFERS=1` | Forces slot binding |
| `KAIZEN_MTL_SYNC_MODE=none` | Native hazard tracking instead of engine barriers |
| `KAIZEN_MTL_SHADER_LOAD_STRATEGY=lazy` | Lazy shader library compilation on macOS |
| `KAIZEN_MTL_OFF_SCREEN=1` | Uses an offscreen surface instead of a layer surface |
| `KAIZEN_MTL_ARCHIVE_FAIL_ON_MISS=1` | Intended to fail pipeline creation on archive miss; inert while the archive is disabled |

## 10. Implications for R1

1. **The RD abstraction already covers Metal**, including argument buffers, heaps, timestamps and MetalFX. R1 should extend this contract rather than add a parallel Metal path.
2. **Do not assume Vulkan semantics.** Textures have no layout transitions, barriers are only enforced in barrier mode, and line width is fixed.
3. **Cross-run pipeline caching is still missing.** A new renderer that relies on fast warm starts will behave differently on Metal until the binary archive is re-enabled.
4. **Shader delivery is two-tiered.** Baked libraries depend on being exported from an arm64 Mac; otherwise the device compiles MSL. Any startup budget must state which case it measured.
5. **GPU timing may be unavailable**, so Metal validation needs a CPU-side measurement fallback.

## 11. Open items

- Runtime quality and performance parity with Vulkan — **not measured**.
- Whether baked libraries ever load on Apple4–6 devices, and how the invalid-container recompile path behaves end to end — **not verified at runtime**.
- Whether re-enabling `MTLBinaryArchive` is viable with metal-cpp — **not investigated**.
- Memoryless/lazy memory accounting — **not implemented** in the source.
- MetalFX spatial/temporal image quality and cost — **not measured**.
- Residency-set behavior in the Metal 3 driver — **not verified**.
