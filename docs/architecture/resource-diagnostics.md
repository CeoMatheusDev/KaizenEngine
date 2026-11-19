# Resource Lifetime Diagnostics (R1)

> **Status:** implemented and runtime-verified on D1 hardware.
> **Scope:** GPU buffers and textures created through `RenderingDevice`.
> **Not covered:** pipelines, uniform sets, framebuffers, samplers, GLES3
> resources (Compatibility renderer does not use `RenderingDevice`).

This document describes Kaizen's own resource lifetime instrumentation. It is
separate from upstream Godot: the upstream tree is not modified beyond the
small, documented integration hooks listed in section 5.

## 1. Why it exists

Godot destroys rendering resources asynchronously. `RenderingDevice::free_rid()`
only pushes a copy of the resource into the current ring frame; the driver
object is destroyed later, in `_free_pending_resources()`, when that ring frame
is processed again. A logical free is therefore not a physical free, and the
existing `buffer_memory`/`texture_memory` counters cannot tell the two apart.

The tracker makes the distinction explicit. It is the foundation for the next
milestone (safe pipeline object eviction), which needs exactly this information:
which objects are still referenced by in-flight frames.

## 2. Architecture

```
kaizen/diagnostics/rendering/
├── resource_registry.h/.cpp   pure C++ registry (unit-tested outside the engine)
└── resource_tracker.h/.cpp    engine wrapper, RID map, diagnostics modes
```

`resource_registry` depends only on the C++ standard library. It can be tested
with `g++` and no engine build, which is why the correctness tests actually run
(see `tests/kaizen/`).

The engine wrapper owns two registries — CPU and GPU — and the mapping from
`RID` to registry handle. They are never mixed: a handle encodes its kind, so
looking up a GPU handle in the CPU registry fails instead of silently matching
an unrelated slot with the same index.

## 3. Lifetime states

```
        allocate()                  begin_release()
 Free ──────────────► Alive ──────────────────────► PendingRelease
   ▲                   │                                  │
   │                   │                                  │ confirm_releases_until(epoch)
   │                   └──────── double free ─────────┐   ▼
   └──────────────────────── slot reuse ◄────────── Released
```

| State | Meaning | Counts as live | Bytes |
| --- | --- | --- | --- |
| `Free` | Slot never used. | No | – |
| `Alive` | Usable by the renderer. | Yes | `alive_*_bytes` |
| `PendingRelease` | Free requested, GPU may still read it. | Yes | `pending_*_bytes` |
| `Released` | Backend confirmed; slot reusable. | No | `released_*_bytes` (cumulative) |

The generation is incremented on **allocation**, not on release. A handle from a
previous lifetime therefore keeps a stable, comparable value until the slot is
reused; after reuse it is rejected as stale instead of aliasing the new
resource.

## 4. Accounting

The registry keeps logical and backend memory separate:

- `requested_bytes` — what `RenderingDevice` asked for (`p_size_bytes`).
- `backend_bytes` — what the driver actually allocated. For textures this is
  `driver->texture_get_allocation_size()`, which is not derivable from the
  width/height/mip count.

Totals are split by state, so it is possible to see memory that is requested but
not yet released (`pending_*_bytes`). `peak_live_*` is the maximum of
`alive + pending`, which is the number that matters when diagnosing stalls.
Released totals are cumulative and never decremented; this makes underflow
impossible by construction.

Counters distinguish:

- `invalid_handles` — handle not owned by this registry (wrong kind or index).
- `generation_mismatches` — handle from a previous lifetime.
- `double_frees` — release requested for an already released resource.
- `registry_full` — capacity exhausted; allocation returns a null handle and the
  renderer continues, never asserts.
- `backend_bytes_overrides` — times the driver-reported size replaced the
  requested size.

## 5. Integration points

All hooks are in `servers/rendering/rendering_device.cpp` and are no-ops unless
diagnostics are enabled. They are documented in
`patches/rendering-device/003-resource-tracker-integration.md`.

| Where | Hook | Purpose |
| --- | --- | --- |
| `storage_buffer_create` | `KAIZEN_TRACK_GPU_RESOURCE` | Register buffer. |
| `uniform_buffer_create` | `KAIZEN_TRACK_GPU_RESOURCE` | Register buffer. |
| `vertex_buffer_create` | `KAIZEN_TRACK_GPU_RESOURCE` | Register buffer. |
| `index_buffer_create` | `KAIZEN_TRACK_GPU_RESOURCE` | Register buffer. |
| `texture_buffer_create` | `KAIZEN_TRACK_GPU_RESOURCE` | Register texel buffer. |
| `texture_create` | `KAIZEN_TRACK_GPU_RESOURCE` | Register texture with driver allocation size. |
| `_free_internal` | `notify_logical_free` | `Alive → PendingRelease`, scheduled at `frames_drawn + frame_count`. |
| `_begin_frame` | `notify_frame_completed` | Confirms releases whose epoch has arrived. |
| `finalize` | `notify_device_teardown` | GPU is idle; flushes the remainder and prints the summary. |

Shared textures and slices are intentionally not registered: they share the
owner's memory and would double count it.

## 6. Diagnostics modes

Selected once per process with `KAIZEN_DIAGNOSTICS`:

| Value | Behaviour |
| --- | --- |
| unset / `off` | One predictable branch per hook. Default for all published benchmarks. |
| `basic` | Counters and byte accounting. |
| `detailed` | Basic plus a preallocated 1024-entry event ring per registry. |
| `validation` | Detailed plus strict handle checks at release time. |

The runtime check is a plain field read; there is no atomic load on the disabled
path. The registry storage (about 1.2 MiB for 16 384 GPU + 4 096 CPU slots) is
allocated lazily on the first enabled call, so a disabled engine allocates
nothing. A compile-time switch is possible but was not added: the measured cost
was below run-to-run noise (section 7).

## 7. Verification

All numbers below were measured on the reference machine (Xeon E5-2680 v3,
Radeon RX 550 / RADV, Mesa 26.2.5).

### 7.1 Registry unit tests

`./tests/kaizen/run_tests.sh`: 140 checks, 0 failures, including 100 000
allocations/releases and 8-thread concurrency.

Microbenchmark (`--bench`): 19.5 ns per allocation+release+confirmation cycle;
3.5 MiB for 65 536 slots.

### 7.2 Runtime overhead (S1, D1, 400 boxes, 1920×1080)

| Mode | FPS (median of 3) |
| --- | --- |
| disabled | 380.30 |
| basic | 378.79 |

Difference: −0.40 %, within the ±0.5 % run-to-run spread. GPU median time was
identical (1.895 ms) in both modes. One additional disabled run reported
208.68 FPS while another GPU process was running concurrently; it was excluded
as contaminated.

### 7.3 Deferred release under GPU load

`tests/kaizen/churn_project` creates two 32×32 textures per frame, draws them,
and frees them in the same frame, for 300 frames:

```
churn: completed 300 frames, 600 textures
[Kaizen][Resources] GPU: allocs=685 releases=685 live=0 pending=0
  live_bytes=0 B peak_bytes=4.45 MiB invalid=0 stale=0 double_free=0 full=0
  | runtime_releases=604 teardown_releases=81
```

600 of the 604 runtime confirmations are the churned textures; the remaining
81 resources are the scene and default textures still alive at shutdown. This
is the end-to-end evidence that a free requested while the GPU is working is
confirmed by a later frame and not by the teardown flush.

## 8. Limitations

1. **Logical vs physical release.** The tracker observes the ring-frame model,
   not driver completion. Confirmation happens when the ring frame is processed,
   which is the point at which upstream considers destruction safe. It does not
   query Vulkan fences directly; the future pipeline eviction work needs that,
   because `VkPipeline` destruction has stricter requirements.
2. **Coverage.** Only storage/uniform/vertex/index buffers, texel buffers and
   non-shared textures are tracked. Pipelines, uniform sets, framebuffers,
   samplers and acceleration structures are not.
3. **GLES3 is invisible.** The Compatibility renderer does not use
   `RenderingDevice`; only D1/D2 cells produce tracker data.
4. **No device-level test for the two renderer patches.** Validating
   `UNIFORM_TYPE_IMAGE_BUFFER` or texture buffers inside callbacks needs a live
   Vulkan device and a shader that binds a texel buffer. The current evidence
   (engine builds, S1 renders, no leaks) proves the normal path, not those
   paths. They must not be reported as fully validated.
5. **RID map growth.** The map holds one entry per live tracked resource and is
   cleared at teardown. It is not sharded; contention is irrelevant today
   because creation/free are render-thread only, but this must be revisited if
   transfer workers ever create resources.

## 9. Next steps

1. **P2 — pipeline object eviction.** Distinguish `VkPipeline` objects from the
   persistent `VkPipelineCache` blob, add reference counting and fence-based
   deferred destruction, and use the tracker's pending-release accounting as
   the safety signal.
2. **P3 — GLES3 initialization.** Guarantee a destructible state after a failed
   constructor path.
3. **GPU profiling.** Timestamp queries and frame pacing on top of the same
   subsystem.
4. **Device-level rendering tests.** A Vulkan harness that can create an image
   buffer, dispatch a compute shader and verify results, plus the same for
   texture buffers in `driver_callback_add`.
