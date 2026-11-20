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

### 5.1 Synchronization contract by backend

The tracker confirms a release when the ring slot's epoch is reached, not when a
GPU query reports completion. That is only sound because of how each backend
drives the device; this must be re-validated whenever a backend changes.

**Vulkan (D1, D2).** `_begin_frame(frame)` calls `_stall_for_frame(frame)`,
which calls `driver->fence_wait(frames[frame].fence)`, and only then calls
`_free_pending_resources(frame)`. That fence is signaled by the last command
submission of the same ring slot (`execute_chained_cmds`). All main work is
submitted to a single `main_queue`; transfer workers submit to `transfer_queue`
and signal semaphores that the main submission waits on before executing
(`_submit_transfer_worker` pushes them into `frames[frame].semaphores_to_wait_on`).
The main fence therefore transitively covers the transfer work of that slot.

Consequences:

- A resource freed during frame `F` is queued in slot `F % frame_count` and
  destroyed when that slot is processed again, i.e. after its fence is waited.
  At that point the last submission that could have used the resource has
  completed, because every submission goes through the same ordered queue.
- The model assumes no async-compute or second graphics queue uses tracked
  resources. It also assumes the resource is not used after it is freed; the
  RID is invalid from that moment.
- The epoch is a scheduling bound, not a GPU timestamp. It must not be
  presented as a fence query, and it is not sufficient for `VkPipeline`
  destruction, which must track the completion of every queue that used the
  pipeline.

**Metal.** Uses the same `RenderingDevice` frame-ring contract, but the Metal
driver's fence and cache behaviour has not been measured on this project (no
macOS hardware). The contract is inherited, not verified.

**Compatibility / OpenGL (D3).** Does not use `RenderingDevice`; the tracker is
inert and reports zero for every cell.

**GLES3 and future backends.** If a backend destroys resources without a
per-slot fence wait, the epoch assumption does not hold and the tracker would
overstate safety. Check the backend's `_free_pending_resources` path before
enabling the tracker there.

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

`./tests/kaizen/run_tests.sh`: 182 checks, 0 failures. Besides the 100 000
allocation/release run and 8-thread concurrency, the suite now covers:

- snapshot invariants sampled while workers allocate and release
  (`live == allocations - releases`, byte totals consistent, no underflow);
- generation wrap: starting at `UINT32_MAX`, the next reuse becomes 1 and the
  old handle is rejected;
- byte accounting with 1 TiB requests;
- event-ring overflow: 2 000 cycles (6 000 events) keep the newest 1 024.

### 7.2 Registry performance

One operation is `allocate + begin_release + confirm`. Latencies are measured
over 64-operation batches so clock overhead does not dominate; percentiles are
per-operation values within batches. 320 000 operations per configuration.

| Configuration | mean | p50 | p95 | p99 | Throughput |
| --- | --- | --- | --- | --- | --- |
| Basic | 54.0 ns | 53.9 ns | 54.3 ns | 54.6 ns | 18.51 Mops/s |
| Detailed (event log) | 64.1 ns | 64.0 ns | 64.3 ns | 64.5 ns | 15.59 Mops/s |
| Low occupancy (1 live) | 54.0 ns | 53.9 ns | 54.3 ns | 54.5 ns | 18.51 Mops/s |
| Near capacity (4 095/4 096 live) | 54.0 ns | 53.9 ns | 54.3 ns | 54.4 ns | 18.52 Mops/s |

Throughput on a **shared** registry (maximum contention, 50 000 operations per
thread):

| Threads | Throughput |
| --- | --- |
| 1 | 18.63 Mops/s |
| 2 | 1.35 Mops/s |
| 4 | 0.48 Mops/s |
| 8 | 0.23 Mops/s |

The registry serializes every operation behind one lock; it is designed for the
render thread, which is the only thread that creates and frees tracked
resources. The multi-threaded test exists to prove safety, not scalability. The
sharp drop reflects spinlock convoying plus rescanning the shared pending list,
and is not a production path. If a future design needs concurrent producers,
the pending queue must be sharded or made lock-free.

Memory: 3.5 MiB for 65 536 slots (56 B per slot). Production capacity is
16 384 GPU + 4 096 CPU slots, about 1.2 MiB, allocated only when enabled.

### 7.3 Runtime overhead (S1, D1, 400 boxes, 1920×1080)

| Mode | FPS (median of 3) |
| --- | --- |
| disabled | 380.30 |
| basic | 378.79 |

Difference: −0.40 %, within the ±0.5 % run-to-run spread. GPU median time was
identical (1.895 ms) in both modes. One additional disabled run reported
208.68 FPS while another GPU process was running concurrently; it was excluded
as contaminated. Three runs per mode are enough to show that no large regression
exists, not to characterize sub-1 % differences precisely; the CPU-side registry
cost is characterized by 7.2 instead.

### 7.4 Deferred release under GPU load

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

1. **Logical vs physical release.** Confirmation happens when the ring slot
   owning the deferred destruction is processed, after that slot's fence wait
   (see 5.1). It is a scheduling bound derived from the RenderingDevice
   contract, not a per-resource completion query. It is sufficient for buffers
   and textures under the single-queue assumption, but not for `VkPipeline`
   eviction, which must track every queue that used the pipeline.
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
