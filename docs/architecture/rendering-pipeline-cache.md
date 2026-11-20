# Pipeline Cache and Shader Variants (R0 follow-up)

> **Status:** Source-verified — **no runtime measurements**
> **Verified against revision:** `eb7dd94` (2026-11-18)
> **Related:** [Rendering audit (R0)](rendering-audit.md), [Rendering resource lifetime](rendering-resource-lifetime.md)

> **Kaizen P2 status:** `PipelineCacheRD` uses full-key shared acquisition
> behind `KAIZEN_PIPELINE_CACHE=on`. Focused renderer/device/Vulkan runtime
> checks cover duplicate acquire, partial release, blocked retirement reacquire,
> and key reuse after physical destruction. Natural multi-consumer hits and
> clean shader teardown remain open; budget-driven eviction stays disabled. See
> [Pipeline object lifetime and eviction](pipeline-lifetime.md).

This document describes how pipeline state objects (PSOs) are cached at two levels, how the device cache is persisted between runs, and which backends actually support it.

## 1. Two cache layers

| Layer | Owner | Scope | Persistence |
|---|---|---|---|
| Variant table | `PipelineCacheRD` (renderer) | Per shader + material/pass combination | In memory only |
| Device PSO cache | `RenderingDevice` + RDD backend | Whole device | Serialized to disk between runs |

They are independent: the variant table decides **which** pipeline is needed; the device cache decides whether the GPU driver can reuse compiled state for it.

## 2. In-memory variant table (`PipelineCacheRD`)

- Key fields: shader, vertex format id, framebuffer format id, render pass, wireframe flag, and a boolean-specialization bitmask (`servers/rendering/renderer_rd/pipeline_cache_rd.h` — `struct Version`).
- Sample count is derived from the framebuffer format during generation (`pipeline_cache_rd.cpp` — `_generate_version()`), so the same key can produce different pipelines for different MSAA settings.
- Lookup is a **linear scan** over the version array under a `SpinLock` (`pipeline_cache_rd.h` — `get_render_pipeline()`).
- On a miss, `_generate_version()` calls `RD::render_pipeline_create_cached()` **while holding the lock**; the opt-in device path returns a shared RID for equivalent full keys.
- `setup()` clears all versions and resets state; `_clear()` frees pipelines through `RD::free_rid()` and contains a TODO noting that clearing probably should recompile variants instead of stalling.

**Implication:** per-draw lookups are O(number of variants). With many variants this becomes measurable, and compilation under the spin lock serializes threads. R1/R2 should consider a hash map keyed by a deterministic variant key and compilation outside the lock.

## 3. Device PSO cache (`RenderingDevice`)

Verified in `servers/rendering/rendering_device.cpp:8860-8940`:

- Enabled by the project setting `rendering/rendering_device/pipeline_cache/enable`; only the main (non-local) device manages it.
- Cache file path: `user://vulkan/pipelines.<rendering_method>.<device_name>.cache`, with a `.editor` suffix in editor builds. Note that the `vulkan/` directory name is hardcoded even when the driver is Metal or D3D12.
- At device init: `_load_pipeline_cache()` reads the file; `driver->pipeline_cache_create(data)` creates the backend cache; the initial size is reported as `Startup PSO cache (N MiB)` in verbose output.
- Saving: `update_pipeline_cache()` is called after pipeline creation (`rendering_device.cpp:5108`, `:5196`, `:5440`). It saves only when the cache grew by at least `rendering/rendering_device/pipeline_cache/save_chunk_size_mb` (or unconditionally on shutdown). Saving runs as a `WorkerThreadPool` native task named `PipelineCacheSave`; if a previous save is still running, the update is skipped and retried later. On closing, the save is synchronous.

## 4. Backend support

| Backend | PSO cache persistence | Notes |
|---|---|---|
| Vulkan | Yes | Header validated against magic, data hash, `vendor_id`, `device_id`, `driver_version`, device `uuid`, and `driver_abi`; mismatches discard the cache (`drivers/vulkan/rendering_device_driver_vulkan.cpp:5360-5405`). Only one caching instance at a time (`caching_instance_count`). |
| D3D12 | Yes | Implements create/free/query/serialize plus `get_pipeline_cache_uuid()` (`drivers/d3d12/rendering_device_driver_d3d12.cpp:4448-4460`, `:6163`). |
| Metal | **No** | `pipeline_cache_create()` returns `false`; serialization returns an empty buffer; the Metal binary archive implementation is commented out with a TODO to re-enable it (`drivers/metal/rendering_device_driver_metal.cpp:1640-1675`). |
| OpenGL / WebGL 2 (Compatibility) | Not applicable | Does not use `RenderingDevice`; no PSO cache layer. |

**Implication:** on Vulkan and D3D12 the cache is reused across runs; on Metal the `MTLBinaryArchive` path is disabled, so pipeline state objects are recreated every run. This is narrower than "everything is compiled at startup": an arm64 macOS export can embed precompiled `.metallib` shader libraries via the shader baker (`editor/shader/shader_baker/shader_baker_export_plugin_platform_metal.cpp`), so MSL compilation may be skipped, but PSO creation is not cached. The editor and exports produced from a non-Mac host compile MSL on the device. See [Metal rendering driver](rendering-metal-driver.md) §3-4.

## 5. Shader variant growth

- Boolean specializations are encoded as a bitmask; each additional boolean doubles the theoretical variant space.
- No variant budget, cap, or eviction policy exists in the variant table. Kaizen's P2 registry is connected to `PipelineCacheRD` for semantic deduplication when opted in. Budget-driven eviction is intentionally disabled; explicit final logical releases are retired. The integration still needs in-engine validation. See [Pipeline object lifetime and eviction](pipeline-lifetime.md).
- Base specialization constants are stored per cache instance and can be changed through `update_specialization_constants()` / `update_shader()`, which invalidate the pipelines created before the change (the caches are re-set up).

## 6. Rules implied for R1

1. Reuse the device PSO cache; do not build a second serialization layer.
2. Introduce a deterministic variant key (hash) rather than relying solely on the linear scan; keep the scan as a fallback if needed.
3. Do not compile pipelines while holding a global lock; queue compilation and publish the pipeline when ready.
4. Account for Metal having no persistent cache: measure cold-start compilation and consider re-enabling the binary archive.
5. Bound variant growth per material/pass and expose cache hit/miss counters.

## 7. Open items

- P2 tracking reports creation-side registry counters, but those historical
  measurements are not functional-cache hit/miss or measured driver-memory
  figures. Semantic `PipelineKey` acquisition is now wired through
  `PipelineCacheRD`; real renderer/Vulkan hit rates and shared-lifetime
  behavior have not yet been measured. Automatic budget eviction remains
  disabled.

- Cold vs. warm compilation time per backend and per scene — **not measured**.
- Real variant counts for representative scenes — **not measured**.
- Whether re-enabling the Metal binary archive is viable in the current metal-cpp integration — **not investigated**.
- The `user://vulkan/` path naming for non-Vulkan backends is cosmetic but should be confirmed as intentional before changing it.
