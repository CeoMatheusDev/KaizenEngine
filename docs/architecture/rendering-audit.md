# Rendering Audit (R0 — Initial Baseline)

> **Status:** Initial R0 result — **source-level verification only**
> **Verified against revision:** `eb7dd94` (2026-11-18)
> **Method:** direct reading of the current source tree. No runtime profiling, device testing, or mobile/Web execution was performed.
> **Scope:** renderer selection, initialization and ownership, threading model, resource lifetime entry points, shader pipeline, and the existing rendering device graph.

This document records only what was confirmed in the source. Items not confirmed are listed in section 10 as open questions and must not be treated as established behavior.

## 1. Method and limitations

- Every statement below cites the file where it was verified.
- Line numbers refer to revision `eb7dd94` and may drift as the tree changes.
- Runtime behavior (frame pacing, GPU timings, editor viewport specifics, mobile and Web execution) was **not** measured. The roadmap requires baselines with real measurements; this audit does not replace them.

## 2. Renderer selection flow (verified)

Selection is resolved in `main/main.cpp` before any rendering server is created:

1. `--rendering-method` and `--rendering-driver` command-line arguments override project settings (`main.cpp:1284`, `main.cpp:1292`).
2. Accepted methods are validated against `forward_plus`, `mobile`, `gl_compatibility`, and `dummy` (`main.cpp:2522-2537`).
3. When running the project manager without explicit flags, the default is `opengl3` + `gl_compatibility` (`main.cpp:2515-2520`).
4. Project settings provide the defaults:
   - `rendering/renderer/rendering_method` (`main.cpp:2660`);
   - `rendering/renderer/rendering_method.mobile` (`main.cpp:2661`);
   - `rendering/renderer/rendering_method.web` is forced to `gl_compatibility` (`main.cpp:2662`);
   - the driver comes from `rendering/gl_compatibility/driver` or `rendering/rendering_device/driver` depending on the method (`main.cpp:2671-2685`).
5. The resolved method and driver are stored on the OS singleton via `set_current_rendering_method()` / `set_current_rendering_driver_name()` (`main.cpp:2686-2687`).

**Implication:** on Web, the renderer method is not a free choice in this revision; it is pinned to `gl_compatibility`.

## 3. Initialization and ownership (verified)

Initialization is two-phase:

1. **Early bootstrap (dummy):** `RasterizerDummy::make_current()` followed by `memnew(RenderingServerDefault())` (no render thread) and `init()`, with the render loop disabled (`main.cpp:772-777`). This exists before the display server is created.
2. **Real server:** after display-server setup, `memnew(RenderingServerDefault(OS::is_separate_thread_rendering_enabled()))`, then `init()` (`main.cpp:3609-3616`).

Construction path:

- `RenderingServerDefault::_init()` allocates the core subsystems — `RendererCanvasCull`, `RendererViewport`, `RendererSceneCull`, `RendererCameraAttributes` — and creates the compositor with `RendererCompositor::create()` (`servers/rendering/rendering_server_default.cpp:250-260`).
- `RendererCompositor::create()` calls the registered factory `_create_func()` (`servers/rendering/renderer_compositor.cpp:53-55`).
- The factory is registered by the renderer implementation:
  - RD (Vulkan/Metal): `_create_func = _create_current` in `servers/rendering/renderer_rd/renderer_compositor_rd.h:172`;
  - GLES3 Compatibility: `RasterizerGLES3::make_current()` at `drivers/gles3/rasterizer_gles3.cpp:219-227`;
  - headless: `RasterizerDummy::make_current()` at `servers/display/display_server_headless.cpp:50`.
- `make_current()` is invoked by the platform display servers during initialization, e.g.:
  - X11: `platform/linuxbsd/x11/display_server_x11.cpp:7181`, `:7295`, `:7315`, `:7360`;
  - Wayland: `platform/linuxbsd/wayland/display_server_wayland.cpp:2361`, `:2485`, `:2510`, `:2559`;
  - Android: `platform/android/display_server_android.cpp:869`, `:875`.

**Implication:** renderer selection is ultimately executed by the display-server code path, not solely by `main.cpp`. The selected method must remain consistent with the display driver's `make_current()` branch.

## 4. Threading model (verified)

- The render thread is enabled by `OS::is_separate_thread_rendering_enabled()`, stored in `_separate_thread_render` (`core/os/os.h:131`, `:314`).
- Its value comes from `--render-thread separate` or the project setting `rendering/driver/threads/thread_model` (`main.cpp:1535-1546`, `main.cpp:2788-2789`).
- The editor and project manager are **forced to single-threaded rendering** (`main.cpp:2792-2795`), as are builds without thread support.
- When enabled, `RenderingServerDefault::init()` starts `_thread_loop()` as a **WorkerThreadPool task** (not a dedicated `Thread`) and records the task id as the command queue pump task (`servers/rendering/rendering_server_default.cpp` — `init()`).
- All rendering-server calls flow through `CommandQueueMT command_queue` (`servers/rendering/rendering_server_default.h:90`); `RSG::threaded = create_thread` in `_init()`.
- `sync()` blocks on the queue when threaded, otherwise flushes pending commands from other threads; `draw()` pushes `_draw` to the server thread when threaded (`rendering_server_default.cpp` — `sync()`, `draw()`).
- `free_rid()` defers `_free()` to the server thread (`rendering_server_default.h:1211-1219`).
- `RenderingDevice` enforces render-thread ownership with guard macros (`servers/rendering/rendering_device.cpp:67-68`).

**Implication for a future renderer:** any new renderer must document on which thread resources may be created/destroyed. The existing `ASYNC_COND_PUSH*` macros (`rendering_server_default.h:127-149`) show the accepted pattern.

## 5. Resource lifetime entry points (verified)

- Backend-agnostic RIDs are owned by `RID_Owner<T>` (`core/templates/rid_owner.h:517`, `free()` at `:548`).
- GPU objects live behind `RenderingDevice`; `RenderingDevice::make_current()` exists at `servers/rendering/rendering_device.cpp:10151`.
- The RD command graph tracks resource usage and transitions (section 7).
- `RenderingServerDefault::free_rid()` defers destruction to the server thread (section 4).

**Not yet mapped:** the exact point at which a freed RID stops being referenced by in-flight frames, and the per-resource-type ordering between `free_rid()`, renderer storage destructors, and `RenderingDevice` destruction. This requires a follow-up pass and is required before R1 defines new ownership rules.

## 6. Shader pipeline (verified)

- **Build time:** `SConstruct:1196-1212` registers the `RD_GLSL`, `GLSL_HEADER`, and `GLES3_GLSL` builders (`glsl_builders.py`, `gles3_builders.py`). These embed shader source text into `*.glsl.gen.h` files; they do not produce SPIR-V.
- **Runtime GLSL to SPIR-V:** `RenderingDevice::shader_compile_spirv_from_source()` calls the glslang module (`modules/glslang`, `compile_glslang_shader`) (`servers/rendering/rendering_device.cpp:288-294`; also `servers/rendering/rendering_device_binds.cpp:215`).
- **Godot shading language to backend code:** `ShaderCompiler::compile()` in `servers/rendering/shader_compiler.cpp:1580`, with preprocessor and language layers under `servers/rendering/`.
- **Pipeline variants:** `PipelineCacheRD` caches pipelines keyed by vertex format, framebuffer format, render pass, wireframe, and boolean specializations, guarded by a `SpinLock` (`servers/rendering/renderer_rd/pipeline_cache_rd.h`).
- **Includes:** `ShaderIncludeDB` resolves shader includes (`servers/rendering/shader_include_db.*`).

**Not yet mapped:** pipeline cache persistence across runs, cache invalidation rules on driver updates, and compilation cost measurements.

## 7. Existing rendering device graph (important finding)

The tree already contains `RenderingDeviceGraph` (`servers/rendering/rendering_device_graph.{h,cpp}`). It records draw, compute, and ray-tracing command lists; tracks resource usage with `ResourceTracker`; normalizes and emits buffer/texture/acceleration-structure barriers; manages render-pass creation callbacks, secondary command buffers per frame, and breadcrumb markers.

**Implication for R1/R2:** a scene-level render graph should be built **on top of** `RenderingDeviceGraph`, not as a parallel implementation. Rewriting barrier/transition logic independently would duplicate existing, tested functionality.

## 8. Platform notes (verified only)

| Platform | Verified facts |
|---|---|
| Linux X11 / Wayland | `RasterizerGLES3::make_current()` and `RendererCompositorRD::make_current()` are both reachable during display-server init (paths in section 3). |
| Android | Same two branches (`display_server_android.cpp:869`, `:875`). |
| Web | Renderer method forced to `gl_compatibility` (`main.cpp:2662`); the Compatibility path is `drivers/gles3/`. |
| Headless | Dummy rasterizer (`display_server_headless.cpp:50`). |
| macOS / Metal | Driver sources exist under `drivers/metal/`, but the selection and runtime path were **not** audited. |

## 9. Editor viewport frame path (verified)

- `Node3DEditorViewport` embeds a `SubViewportContainer` (stretch enabled) containing a `SubViewport` (input disabled) with a `Camera3D`; gizmo layers are configured on the camera's cull mask (`editor/scene/3d/node_3d_editor_viewport.cpp:6941-6953`).
- The main loop triggers rendering with `RenderingServer::get_singleton()->draw(wants_present, scaled_step)` (`main.cpp:5139`, `:5143`).
- `RenderingServerDefault::draw()` pushes `_draw` to the server thread when threaded; otherwise it runs directly (`rendering_server_default.cpp`, `draw()`).
- `_draw()` calls `RSG::rasterizer->begin_frame()`, then `RSG::viewport->draw_viewports(p_swap_buffers)`, then `RSG::rasterizer->end_frame(p_swap_buffers)` (`rendering_server_default.cpp`, `_draw()`).
- `RendererViewport::draw_viewports()` sorts active viewports (parent dependency order, rendered in reverse order) and renders each through the scene renderer (`servers/rendering/renderer_viewport.cpp:805`).
- Editor and project manager are forced single-threaded (`main.cpp:2792-2795`), so in the editor this entire path runs on the main thread; the 3D viewport's `SubViewport` is simply one more active viewport in the same pass.

## 10. Open questions (must not be treated as verified)

1. ~~Exact per-resource lifetime rules across multiple frames in flight.~~ **Documented** in [Rendering resource lifetime](rendering-resource-lifetime.md): RIDs are removed immediately; GPU objects are disposed when the frame ring slot is recycled, after the slot fence is waited.
2. ~~Editor viewport rendering path — **partially verified**~~ **Verified**: see section 9. The `SubViewport` is one of the active viewports rendered by `draw_viewports()`; the editor runs the whole path single-threaded.
3. Pipeline cache persistence and invalidation behavior — **verified**; see [Pipeline cache and shader variants](rendering-pipeline-cache.md). Note: Metal has no persistent PSO cache.
4. Metal renderer selection — **selection verified**: `rendering/rendering_device/driver.macos` defaults to `metal` (`main.cpp:2395`); the Metal build option defaults to enabled but is **disabled for non-arm64 targets** (`platform/macos/detect.py:333-335`); the display server registers `metal` and branches on the driver name (`platform/macos/display_server_macos.mm:3629`, `:202`). Quality parity and runtime behavior were **not** verified.
5. Runtime baselines: CPU/GPU frame times, memory, frame pacing — **not measured**.
6. Mobile device capability profiles and thermal behavior — **not measured**.
7. WebGL 2 feature limits as actually enforced by the Compatibility renderer at runtime — **not verified**.

## 11. Recommended next actions

Complete R0 before starting R1:

1. ~~Produce the resource-lifetime map required by R1.~~ **Done** — see [Rendering resource lifetime](rendering-resource-lifetime.md); remaining gaps are listed in its section 8.
2. ~~Trace the detailed editor viewport per-frame update path.~~ **Done** — see section 9; the remaining gap is the per-viewport scene-renderer internals, which belong to the R1 contract work.
3. ~~Establish a benchmark methodology.~~ **Done** — see [Rendering benchmark methodology](rendering-benchmarks.md): instruments, scene set, reference configurations, procedure, and report template. Scene authoring, the harness, and all measurements are still pending; no numbers have been produced.
4. Audit Metal runtime behavior (quality parity with Vulkan) and the Web export runtime.
5. Update the rendering roadmap with any finding that changes its assumptions — particularly the reuse of `RenderingDeviceGraph` and the linear variant lookup in `PipelineCacheRD`.
