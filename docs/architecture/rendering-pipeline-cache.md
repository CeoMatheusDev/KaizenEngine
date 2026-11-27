# Pipeline Cache and Shader Variants (R0 follow-up)

> **Status:** Source-verified; persistent-cache performance remains unmeasured
> **Verified against revision:** `d7524ae` (2026-11-20)
> **Related:** [Rendering audit (R0)](rendering-audit.md), [Rendering resource lifetime](rendering-resource-lifetime.md)

> **Kaizen P2 status:** `PipelineCacheRD` uses full-key shared acquisition
> behind `KAIZEN_PIPELINE_CACHE=on`. Focused renderer/device/Vulkan runtime
> checks cover duplicate acquire, partial release, blocked retirement reacquire,
> and key reuse after physical destruction. Clean shader teardown is
> implemented (blocked free requests are deferred and completed, never
> dropped), and the independent multi-consumer scenario exists in
> `tests/kaizen/p251_dedup_project` pending a GPU run; natural multi-consumer
> hits remain unproven and budget-driven eviction stays disabled. See
> [Pipeline object lifetime and eviction](pipeline-lifetime.md).

This document describes renderer variant lookup, opt-in shared pipeline-object
ownership, driver PSO persistence, and which backends support the persistent
cache.

## 1. Cache layers and identities

| Layer | Owner | Scope | Persistence |
|---|---|---|---|
| Renderer variant table | `PipelineCacheRD` | Per shader + material/pass request; retains logical variants | In memory only |
| Shared pipeline-object cache | `RenderingDevice` + `PipelineRegistry` | Canonical render-state identity; opt-in shared RID ownership | In memory only |
| Driver PSO cache | `RenderingDevice` + RDD backend | Driver compilation data | Serialized to disk between runs on supported backends |

These layers answer different questions: `PipelineCacheRD` resolves a renderer
variant request, the shared object cache determines whether an equivalent
request can reuse a live pipeline RID, and the driver cache can reuse compiled
PSO data. The shared object cache is enabled with
`KAIZEN_PIPELINE_CACHE=on`; budget-driven eviction is disabled.

## 2. In-memory variant table (`PipelineCacheRD`)

- Key fields: shader, vertex format id, framebuffer format id, render pass, wireframe flag, and a boolean-specialization bitmask (`servers/rendering/renderer_rd/pipeline_cache_rd.h` — `struct Version`).
- Sample count is derived from the framebuffer format during generation (`pipeline_cache_rd.cpp` — `_generate_version()`), so the same key can produce different pipelines for different MSAA settings.
- Lookup is a **linear scan** over the version array under a `SpinLock` (`pipeline_cache_rd.h` — `get_render_pipeline()`).
- On a miss, `_generate_version()` calls `RD::render_pipeline_create_cached()` **while holding the lock**; the opt-in device path returns a shared RID for equivalent full keys.
- The device-level `PipelineKey` includes the resolved shader, format IDs, subpass and primitive, dynamic state, all fixed-function states, and specialization values. Its canonical byte identity is compared in full; the hash is only an index aid.
- `setup()` clears all versions and resets state; `_clear()` releases each version's logical shared-cache reference when the opt-in registry is active, otherwise it frees the owned RID. It retains a TODO noting that clearing probably should recompile variants instead of stalling.

**Implication:** per-draw lookups are O(number of variants). With many variants this becomes measurable, and compilation under the spin lock serializes threads. R1/R2 should consider a hash map keyed by a deterministic variant key and compilation outside the lock.

## 3. Device PSO cache (`RenderingDevice`)

Verified in `servers/rendering/rendering_device.cpp:9033-9125`:

- Enabled by the project setting `rendering/rendering_device/pipeline_cache/enable`; only the main (non-local) device manages it.
- Cache file path: `user://vulkan/pipelines.<rendering_method>.<device_name>.cache`, with a `.editor` suffix in editor builds. Note that the `vulkan/` directory name is hardcoded even when the driver is Metal or D3D12.
- At device init: `_load_pipeline_cache()` reads the file; `driver->pipeline_cache_create(data)` creates the backend cache; the initial size is reported as `Startup PSO cache (N MiB)` in verbose output.
- Saving: `update_pipeline_cache()` is called after pipeline creation (`rendering_device.cpp:5131`, `:5284`, `:5539`). It saves only when the cache grew by at least `rendering/rendering_device/pipeline_cache/save_chunk_size_mb` (or unconditionally on shutdown). Saving runs as a `WorkerThreadPool` native task named `PipelineCacheSave`; if a previous save is still running, the update is skipped and retried later. On closing, the save is synchronous.

## 4. Backend support

| Backend | PSO cache persistence | Notes |
|---|---|---|
| Vulkan | Yes | Header validated against magic, data hash, `vendor_id`, `device_id`, `driver_version`, device `uuid`, and `driver_abi`; mismatches discard the cache (`drivers/vulkan/rendering_device_driver_vulkan.cpp:5417-5431`). Only one caching instance at a time (`caching_instance_count`). |
| D3D12 | Yes | Implements create/free/query/serialize plus `get_pipeline_cache_uuid()` (`drivers/d3d12/rendering_device_driver_d3d12.cpp:4448-4460`, `:6163`). |
| Metal | **No** | `pipeline_cache_create()` returns `false`; serialization returns an empty buffer; the Metal binary archive implementation is commented out with a TODO to re-enable it (`drivers/metal/rendering_device_driver_metal.cpp:1640-1675`). |
| OpenGL / WebGL 2 (Compatibility) | Not applicable | Does not use `RenderingDevice`; no PSO cache layer. |

**Implication:** on Vulkan and D3D12 the cache is reused across runs; on Metal the `MTLBinaryArchive` path is disabled, so pipeline state objects are recreated every run. This is narrower than "everything is compiled at startup": an arm64 macOS export can embed precompiled `.metallib` shader libraries via the shader baker (`editor/shader/shader_baker/shader_baker_export_plugin_platform_metal.cpp`), so MSL compilation may be skipped, but PSO creation is not cached. The editor and exports produced from a non-Mac host compile MSL on the device. See [Metal rendering driver](rendering-metal-driver.md) §3-4.

## 5. Shader variant growth

- Boolean specializations are encoded as a bitmask; each additional boolean doubles the theoretical variant space.
- No variant budget, cap, or eviction policy exists in the variant table. The P2 registry is connected to `PipelineCacheRD` for semantic sharing when opted in. A focused Vulkan runtime check verifies equivalent-key RID sharing, partial release, acquire blocking during retirement, and acquire after physical destruction. Ordinary churn produced no natural cache hits, and independent renderer consumers plus clean shader teardown are still unverified. Budget-driven eviction remains disabled. See [Pipeline object lifetime and eviction](pipeline-lifetime.md).
- Base specialization constants are stored per cache instance and can be changed through `update_specialization_constants()` / `update_shader()`, which invalidate the pipelines created before the change (the caches are re-set up).

## 6. Rules implied for R1

1. Reuse the device PSO cache; do not build a second serialization layer.
2. Introduce a deterministic variant key (hash) rather than relying solely on the linear scan; keep the scan as a fallback if needed.
3. Do not compile pipelines while holding a global lock; queue compilation and publish the pipeline when ready.
4. Account for Metal having no persistent cache: measure cold-start compilation and consider re-enabling the binary archive.
5. Bound variant growth per material/pass and expose cache hit/miss counters.

## 7. Open items

- Historical P2 tracking counters (for example `hits=0 misses=204`) came from
  unique RID-keyed tracking and are not measurements of semantic cache reuse or
  driver-memory savings. The 120-frame churn run with the shared cache enabled
  also produced 0 natural hits; opt-in verification adds synthetic test
  acquisitions and its counters are not workload metrics. Automatic budget
  eviction remains disabled.

- Cold vs. warm compilation time per backend and per scene — **not measured**.
- Real variant counts for representative scenes — **not measured**.
- Whether re-enabling the Metal binary archive is viable in the current metal-cpp integration — **not investigated**.
- The `user://vulkan/` path naming for non-Vulkan backends is cosmetic but should be confirmed as intentional before changing it.
