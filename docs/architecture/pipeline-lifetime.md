# Pipeline Object Lifetime and Eviction (P2)

> **Status:** Core implemented and validated against a live Vulkan device.
> Deferred destruction is available in the Vulkan driver behind an opt-in
> switch; creation-side eviction is not wired.
> **Scope:** `kaizen/rendering/pipeline_cache/`, plus the documented Vulkan
> hooks in `patches/rendering-device/004-pipeline-lifetime-integration.md`.
> **Related:** [Pipeline cache and shader variants](rendering-pipeline-cache.md), [Resource lifetime diagnostics](resource-diagnostics.md), [Rendering roadmap](rendering-roadmap.md)

This document describes the Kaizen subsystem that owns pipeline objects and
decides when one may be destroyed. It is deliberately separate from the
persistent `VkPipelineCache` blob: that blob stores driver-side creation data,
while this subsystem owns **objects with references** and their completion
bookkeeping.

## 1. What upstream already does

Godot destroys a freed pipeline on the frame ring, not immediately:
`RenderingDevice::_free_pending_resources()` walks
`frames[p_frame].render_pipelines_to_dispose_of` and calls
`driver->pipeline_free()` (`servers/rendering/rendering_device.cpp:8193-8207`).
That function runs from `_begin_frame()` after
`_stall_for_frame()` waited on the slot's fence
(`rendering_device.cpp:8509-8513`), so deferred destruction is safe for the
single-main-queue, single-frame-ring model.

What upstream does **not** do is bound the cache:

- `PipelineCacheRD` keeps every variant it ever compiled until the whole cache
  is cleared or destroyed (`servers/rendering/renderer_rd/pipeline_cache_rd.cpp:81-94`).
- Each variant is a linear-scan entry; there is no budget, LRU, or admission
  policy.
- The pipeline cache blob is allowed to grow without a limit; the FIXME in
  `drivers/vulkan/rendering_device_driver_vulkan.cpp` notes that no size cap is
  applied.

P2 adds a lifetime policy layer. It does not change the ownership rules of
`PipelineCacheRD` or the device cache.

## 2. Model

`PipelineRegistry` is a standalone, backend-agnostic core. It knows nothing
about `VkPipeline`, `RID`, or the renderer; the backend supplies handles, byte
sizes, and completion serials.

### 2.1 States and references

| State | Meaning |
| --- | --- |
| `Free` | Slot never allocated or already destroyed. |
| `Resident` | Usable and acquirable; least-recently-used entries may be evicted. |
| `EvictionCandidate` | Selected by the budget policy or by `retire_now()`. No new acquisitions or recording scopes. Existing references keep it alive. |
| `Retiring` | No host or recording references; waiting for every queue's completion serial. |
| `Destroyable` | Completion reached; the backend may destroy the object and call `destroy()`. |

Each slot carries a 32-bit generation. A handle is `{index, generation}`;
after a slot is reused the old handle is rejected. Generation 0 is reserved for
null handles, and wrap from `UINT32_MAX` goes to 1.

Two independent reference counts exist:

- `host_refs` — owners that may hand the pipeline out (`acquire()` /
  `release()`).
- `recording_refs` — command buffers that have begun recording but have not
  been submitted (`begin_use()` / `end_use()` / `abort_use()`).

A pipeline may only enter `Retiring` when **both** are zero. This is what
prevents the race where a pipeline is evicted while a command buffer is still
recording it.

### 2.2 Budget and LRU

`enforce_budget()` evicts least-recently-used `Resident` entries until
`resident_bytes <= budget_bytes`. A candidate with live references stays in
`EvictionCandidate`; it is retired when the last reference drops. Candidate and
retiring bytes are tracked separately as `pending_bytes`, so the budget applies
to memory that can still be handed out, not to memory that is already on its way
out. No eviction happens while the resident bytes are at or below the budget.

There is no automatic eviction on `acquire()`. The owner calls
`enforce_budget()` at a point where it can tolerate a miss; this keeps the
policy explicit and avoids evicting while a lock is held by a caller.

`retire_now()` covers the other direction: the backend already knows it wants
the object gone. It skips the budget, requires zero references, and enters
`Retiring` with whatever usage was recorded through `mark_use()`/`end_use()`.
It is the entry point used by the Vulkan driver's deferred destruction.

### 2.3 Destruction condition

The registry only moves a slot to `Destroyable` when all of the following hold:

1. `host_refs == 0`
2. `recording_refs == 0`
3. for **every queue that used the object**, `completed_serial[queue] >=
   last_use_serial[queue]`

Usage is per queue (`PipelineQueueUsage`): a mask of queues that submitted work
using the object plus one serial per queue. A queue that never used the object
never gates its destruction, so a graphics-only pipeline does not wait for a
compute queue.

`set_completed_serial(queue, value)` is monotonic per queue: lower values are
ignored, and every call re-evaluates retirement even when the value did not
change. An entry that retires after completion was already reported therefore
drains on the next call.

A serial of 0 means "never submitted". Such an object becomes destroyable as
soon as it has no references.

### 2.4 Key identity

A key maps to at most one live object. While an object with that key is
`Retiring` or `Destroyable`, `acquire()` returns a null handle and increments
`blocked_by_retirement`; it does not create a duplicate. The mapping is erased
only by `destroy()`, after which a new object (new generation) may be created.
This trades a possible stall for a strict "one object per key" invariant.

Callers that cannot tolerate the stall must drain destroyable entries before
acquiring. In a rendering path, that means calling `set_completed_serial()` +
`collect_destroyable()` + `destroy()` once per frame, before pipeline lookups.

The current driver integration does not use keyed acquisition: it only defers
destruction of objects the renderer already freed. Two objects for the same
configuration may therefore coexist briefly. That is legal Vulkan; it is not a
deduplication cache.

### 2.5 Shutdown

At device teardown the owner waits for the device to idle, confirms every
outstanding submission, drains all destroyable entries, and then destroys the
registry. Every slot reaches `Free`, and the statistics report zero resident and
pending bytes. The concurrency test exercises the same sequence after
concurrent workers stop.

## 3. Completion serial contract

The registry does not query a GPU. Correctness depends on the serial meaning
"every queue that could have used the object has completed all work up to this
point".

| Backend | What a valid serial is | Status |
| --- | --- | --- |
| Vulkan | One monotonic serial per physical queue, confirmed by a fence wait. Timeline semaphores per queue are the direct source; the current driver derives group serials from submission fences (see §3.1). | Implemented behind an opt-in switch; validated on a two-queue device test. |
| Metal | `MTLCommandBuffer` completion (completed handler or `waitUntilCompleted` for teardown). A per-queue command-buffer index can serve as the serial if completion is observed before advancing it. | Inherited contract, unverified (no macOS hardware). |
| D3D12 | Fence values per command queue; again the minimum across queues that used the pipeline. | Not implemented. |
| OpenGL / GLES3 | Not applicable — no pipeline objects with deferred destruction in the `RenderingDevice` sense. The registry stays unused. | Not applicable. |

A single global submission counter is **not** sufficient: graphics completing
serial 100 does not imply that the compute queue finished 99, and a device-wide
counter would let destruction race with compute work.

`VkPipeline` objects and the `VkPipelineCache` blob have different retention
rules and must never share an eviction decision:

- A `VkPipelineCache` is a data container; destroying it does not destroy the
  pipelines created from it, and pipelines remain valid after it is gone.
- Pipeline objects have references and in-flight use; the blob does not.
- Evicting a pipeline from the object registry is not a reason to drop the
  blob, and trimming the blob is not a reason to destroy an object.

The subsystem therefore exposes no API that touches `VkPipelineCache`.

### 3.1 Fence groups in the Vulkan driver

`PipelineSubmissionTracker` maps the shape of the existing driver:

- `RenderingDevice` passes a fence only for the last command buffer of a batch;
  earlier ones are chained with semaphores.
- A submission without a fence joins the **open group**.
- A submission with a fence closes the group, and that fence confirms every
  submission in it.
- Queue serial spaces are independent.

This is sound because the fenced submission waits on the semaphore signaled by
the previous one, so its fence cannot signal before the group's earlier work is
done. A fence wait feeds the exact group serial to the registry; nothing assumes
that a global counter advances in lockstep across queues.

The engine creates at most three command queues (main, transfer, present). The
main queue is slot 0 and carries graphics and compute pipelines. A queue beyond
three is left untracked rather than merged into another completion domain:
untracked is conservative (objects stay pending until teardown), whereas merging
would be unsound.

## 4. Driver integration

The core is called from the Vulkan driver through isolated hooks documented in
`patches/rendering-device/004-pipeline-lifetime-integration.md`. The hooks are:

1. `pipeline_free()` asks the registry to retire the object instead of calling
   `vkDestroyPipeline` immediately.
2. `command_queue_execute_and_present()` records the submission serial after a
   successful `vkQueueSubmit` and stores the group serial in the fence.
3. `fence_wait()` confirms that serial and drains destroyable objects.
4. The destructor waits for the device, confirms everything, drains, and only
   then destroys the `VkDevice`.

Because the renderer already frees pipeline RIDs through the frame ring, the
driver's last submitted serial per queue is a conservative bound for
`mark_use()`. The object is retired with that bound, and destruction happens
only after every queue's fence reaches it.

The integration is **opt-in**:

```bash
KAIZEN_PIPELINE_LIFETIME=on ./bin/kaizen.linuxbsd.template_release.x86_64 ...
```

Disabled is the default and preserves upstream destruction timing exactly. With
the switch off, `pipeline_free()` is the upstream one-line function.

What this integration does **not** do:

- It does not deduplicate pipelines, evict during creation, or bound the
  `PipelineCacheRD` variant list. Those need owner-level changes and hit/miss
  measurements.
- It does not track raytracing pipelines (`raytracing_pipeline_free()` still
  destroys immediately).
- It does not replace the frame ring for other resources; only pipeline objects
  use the registry.

## 5. Acceptance matrix

### 5.1 Registry tests

All tests are in `tests/kaizen/test_pipeline_registry.cpp` and run without an
engine build (`./tests/kaizen/run_tests.sh`).

| Acceptance criterion | Test | Result |
| --- | --- | --- |
| No eviction below or at the budget | `no_eviction_below_or_at_budget` | Pass |
| LRU selects the least recently used resident object | `lru_evicts_least_recently_used`, `lru_basics` | Pass |
| Host references preserve an object | `active_host_reference_blocks_destruction` | Pass |
| Recording references preserve an object | `recording_reference_blocks_destruction` | Pass |
| Pending submissions delay destruction | `pending_submission_delays_destruction` | Pass |
| Destruction happens only after completion | `pending_submission_delays_destruction`, `recording_reference_blocks_destruction` | Pass |
| Object used on two queues waits for both | `multi_queue_waits_for_all` | Pass |
| Queues completing out of order are handled | `queues_complete_out_of_order` | Pass |
| Failed submission does not create a dependency | `abort_use_drops_protection` | Pass |
| Zero / invalid serials are rejected | `zero_and_invalid_serials_rejected` | Pass |
| Backend-requested retirement | `retire_now_requires_no_references`, `retire_now_waits_for_recorded_use`, `retire_now_from_candidate` | Pass |
| Fence groups map to serials | `submission_groups_and_fences` | Pass |
| Destroyable objects can be drained and destroyed | `shutdown_drains_everything` | Pass |
| No duplicate object for a key while it retires | `key_is_unique_while_retiring` | Pass |
| Reclaimed slots get new generations; stale handles are rejected | `stale_handle_rejected`, `generation_wrap_skips_zero` | Pass |
| Capacity exhaustion fails in a controlled way | `capacity_full_fails_controlled` | Pass |
| Late use of a retired object is rejected | `late_use_is_rejected` | Pass |
| Concurrent eviction/destruction does not free referenced objects | `concurrent_eviction_and_destruction` | Pass |
| Event/retirement bookkeeping survives limited collection | `retirement_queue_basics` | Pass |
| Statistics match the lifecycle | `stats_reflect_lifecycle`, `acquire_hit_and_miss` | Pass |

The concurrency test runs four worker threads (80 000 acquire/record/submit/
release/enforce cycles) against one collector thread that advances the completion
serial and destroys ready objects. Each returned handle is also checked to be
live: it must have a host reference and must not be `Retiring` or `Destroyable`.
After the workers stop, the test drains the registry and asserts zero resident,
candidate, retiring and destroyable entries, zero resident/pending bytes, and
`late_use == 0`.

### 5.2 Device test

`tests/kaizen/vulkan/pipeline_lifetime_vulkan.cpp` creates real `VkPipeline`
objects on a device with separate graphics and compute queues, signals a
timeline semaphore per submission, and destroys each pipeline only after the
registry observed both queues completing.

| Device scenario | Result |
| --- | --- |
| Pipeline used on graphics and compute, compute reported first | Pass — not destroyable until graphics is reported |
| Graphics and compute reported in reverse order | Pass |
| Recording not submitted yet | Pass — destruction blocked by the recording reference |
| `vkQueueSubmit` path fails / is aborted | Pass — no completion dependency is created |
| Device idle drains retirement | Pass |
| Shutdown leaves zero live objects | Pass |
| Same key re-acquired while the old object retires | Pass — rejected, then new generation after destroy |
| Khronos validation layer during the whole run | 0 errors |

Run it with `./tests/kaizen/vulkan/run_vulkan_tests.sh`. The harness needs
Vulkan 1.2, the Khronos validation layer and `spirv-as`; it skips with exit code
77 when they are missing. The test performs real dispatches, so a headless CI
without a GPU will skip rather than fake a pass.

## 6. Measurements

Registry microbenchmark (`./tests/kaizen/run_tests.sh --bench`, Xeon E5-2680 v3,
GCC `-O2`). One operation is listed per row; latencies are per operation over
64-operation batches, 1 000 000 operations. These are one representative run:
on a shared machine, means varied by roughly 10 % between runs, and p95/p99 are
sensitive to scheduling noise.

| Operation | mean | p50 | p95 | p99 | Throughput |
| --- | --- | --- | --- | --- | --- |
| Hit: `acquire` + `release` | 27.9 ns | 27.8 ns | 27.9 ns | 28.2 ns | 35.83 Mops/s |
| Hit with 4 095 other resident objects | 27.6 ns | 27.4 ns | 27.4 ns | 27.7 ns | 36.24 Mops/s |
| Miss: `acquire` (new slot) + `release` | 108.4 ns | 86.3 ns | 103.8 ns | 154.4 ns | 9.23 Mops/s |
| Evict → retire → destroy | 154.5 ns | 152.9 ns | 153.5 ns | 226.7 ns | 6.47 Mops/s |
| Full lifetime (record, submit, release, evict, destroy) | 162.8 ns | 161.7 ns | 162.4 ns | 206.5 ns | 6.14 Mops/s |
| Multi-queue lifetime (graphics + compute) | 193.0 ns | 191.7 ns | 192.4 ns | 237.6 ns | 5.18 Mops/s |

Throughput on a **shared** registry with one hot key (maximum contention,
50 000 operations per thread). The reference machine was not idle during these
runs (other engine processes were active), so the observed range is reported
instead of a single value:

| Threads | Observed throughput |
| --- | --- |
| 1 | 34.06 – 35.85 Mops/s |
| 2 | 2.69 – 6.95 Mops/s |
| 4 | 2.06 – 3.16 Mops/s |
| 8 | 0.90 – 1.67 Mops/s |

(The most recent run measured 35.58 / 4.32 / 2.09 / 0.99 Mops/s; the table keeps
the observed range because the reference machine is shared.)

Interpretation and limits:

- These numbers exclude pipeline creation and GPU work. **Vulkan pipeline
  creation time is not measured here**; a miss in this benchmark allocates a
  registry slot and inserts a hash-map entry. Real hit/miss rates and eviction
  counts require an in-engine run, which does not exist yet.
- A slot is 64 B: the key, byte count, three per-queue serials plus the usage
  mask, generation, both reference counts and the state. The free list adds
  4 B and the LRU links 8 B per slot, so the fixed cost is 76 B/slot
  (76 B × 65 536 ≈ 4.75 MiB), excluding the hash map and the retirement queue,
  which scale with live entries. The benchmark process does not report resident
  set size. `PipelineRegistry::get_slot_size()` returns the 64 B value plus the
  link overhead in the benchmark output, so reports can be computed from the
  actual build.
- The registry serializes operations behind a spin lock. It is designed for
  the render thread; the concurrency test proves safety, not scalability. The
  multi-thread throughput row is a contention measurement, not a production
  configuration.
- `PipelineStats` exposes hit/miss counts, resident/candidate/retiring/
  destroyable counts, resident and pending bytes, evictions and destroyed
  objects for a future in-engine run. No in-engine P2 values are published.
- The opt-in driver path changes destruction timing; no frame-time measurement
  of the enabled path is published yet.

The pipeline suite also runs clean under AddressSanitizer + UBSan and under
ThreadSanitizer (Homebrew clang 23.1.2, `-O1 -g`). Sanitizers are not part of
the default script because the runtimes are not installed on every machine.

## 8. Premises, invariants and failure paths (P2.2)

### 8.1 Premises

**P1 — Deferred release runs after the frame fence.**  
`RenderingDevice::_free_pending_resources()` is called from `_begin_frame()`,
after `_stall_for_frame()` waited on the slot's fence. Consequently, when a
`PipelineCacheRD` variant or shader is freed through the frame ring, every
submission that could reference it has already completed. This is a property
of the frame lifecycle, not of the registry; the registry only consumes the
serials the driver hands it.

**P2 — A freed RID is not referenced by command buffers under construction.**  
The renderer's rule is that a freed pipeline RID is never used again by a
recording command buffer. The registry's `recording_refs` is the mechanism that
would enforce this; in the driver it is **not yet wired** (P2.2 work). Until
it is, the protection of in-flight recording rests on P1 and on the renderer's
frame-ring ownership, not on the registry.

**P3 — Serial 0 means "never submitted".**  
Backends must start submission serials at 1. A backend that uses 0 as a real
serial would make unused objects look submitted and submitted objects look
unused.

**P4 — One queue completion does not imply another's.**  
Graphics finishing does not imply compute finished. The completion condition is
a conjunction over every queue in the object's usage mask.

### 8.2 Invariants

| ID | Invariant | Enforced by |
| --- | --- | --- |
| I1 | `host_refs >= 0` and `recording_refs >= 0` at all times. | Reference-count discipline in `acquire`/`release`/`begin_use`/`end_use`/`abort_use`. |
| I2 | A slot enters `Retiring` only when `host_refs == 0` and `recording_refs == 0`. | `_retire_locked()` is only called from paths that first check both counts. |
| I3 | `Retiring` becomes `Destroyable` only when every queue in the usage mask has `completed >= last_use`. | `_is_ready_locked()` / `PipelineQueueUsage::is_complete()`. |
| I4 | At most one live object per key; the mapping is erased only by `destroy()`. | `acquire()` returns null while the key is not `Resident`. |
| I5 | Generation 0 is reserved for null handles; wrap skips 0. | Allocation path in `acquire()`. |
| I6 | `set_completed_serial()` is monotonic per queue; equal values re-evaluate retirement. | `confirm()` / `set_completed_serial()`. |
| I7 | `completed_serial` never decreases. | Monotonic max in both the tracker and the registry. |
| I8 | `resident_bytes + pending_bytes` is conserved; no underflow. | `_move_to_pending_locked()` and `destroy()`. |
| I9 | Recording references block both eviction and retirement. | `_mark_candidate_locked()` and `release()`. |
| I10 | A failed or aborted submission records no queue usage. | `abort_use()` does not call `usage.record()`. |
| I11 | A key cannot be re-acquired while its object is `Retiring` or `Destroyable`. | `blocked_by_retirement` counter. |

### 8.3 Failure-path map

Every exit point of the submission chain must leave the registry in a consistent
state. The required cleanup is listed per exit.

```
acquire(handle)                       -- host_refs++
  |
  +-- begin_use(handle)              -- recording_refs++
  |     |
  |     +-- command recording        -- (no registry state change)
  |     |
  |     +-- end_use(handle, q, s)    -- recording_refs--; usage.record(q, s)
  |     |     |                        if refs == 0 and candidate: retire
  |     |     +-- submit succeeds    -- (fence stores the group serial)
  |     |     +-- submit fails       -- call abort_use (no usage recorded)
  |     |
  |     +-- abort_use(handle)        -- recording_refs--; no usage recorded
  |           |                        if refs == 0 and candidate: retire
  |           +-- recording abandoned -- (same cleanup as above)
  |
  +-- release(handle)                -- host_refs--
        |                              if refs == 0 and candidate: retire
        +-- object never used          -- serial 0; destroyable once refs == 0
        +-- object used on N queues    -- waits for all N completions
```

Exit points and their obligations:

| Exit point | Obligation |
| --- | --- |
| `begin_use` fails (not Resident) | No state change; the caller must not record. |
| Recording is abandoned | Call `abort_use()`; otherwise `recording_refs` stays pinned and the object can never retire. |
| `vkQueueSubmit` fails | Call `abort_use()`; do not store a serial in the fence; do not call `confirm()`. |
| Fence wait fails | Do not call `confirm()`; the object stays retiring. The destructor's `confirm_all()` is the escape hatch after device idle. |
| `set_completed_serial` is never called again | The object stays retiring until the next call or until `confirm_all()` at teardown. |
| `collect_destroyable` returns nothing | Not an error; re-evaluate on the next frame. |
| `destroy()` fails | The slot stays `Destroyable`; retry on the next drain. Never leak the VkPipeline handle. |

### 8.4 Premise audit checklist (P2.2)

Before P2.2 is considered closed, the following must be verified on the
reference machine:

- [ ] A pipeline freed through the frame ring is never referenced by a command
      buffer under construction. Verified by enabling the recording hook and
      asserting `recording_refs == 0` at `pipeline_free()` time.
- [ ] Every `vkQueueSubmit` failure path calls `abort_use()` (or never called
      `begin_use()`). Verified by inspection + a forced-failure test.
- [x] Fence reset and reuse never confirm a stale group serial. Verified by the
      device test's reuse scenario.
- [x] No path leaves a group open across a frame boundary without a later
      fenced submission closing it. Verified by the submission-group unit test
      and by the 120-frame churn run.

### 8.5 P2.2 step 2 — runtime churn (validated)

`tests/kaizen/pipeline_churn_project/` creates 8 `MeshInstance3D` per frame
with `StandardMaterial3D` variants that differ in the properties that affect
the shader variant (transparency, culling, shading mode, depth draw, vertex
color), frees them with `queue_free()` and renders with a camera. It ran 120
frames, 960 meshes created and freed, with `KAIZEN_PIPELINE_LIFETIME=on` and
the Khronos validation layer active.

Result on the RX 550: 204 distinct pipeline objects retired and destroyed,
`resident=0 retiring=0 destroyable=0 pending_bytes=0` at shutdown, one
pre-existing `VUID-VkShaderModuleCreateInfo-pCode-08740` and no new
validation message. The deferred path is therefore exercised during active
frames, not only at teardown, and no pipeline was destroyed while still in use.

The churn created ~204 distinct pipelines out of 960 meshes, which confirms that
the test genuinely stresses variant creation rather than reusing a single
cached pipeline. The registry's `blocked_by_retirement` path is not exercised
by this scene because each variant is freed only after it leaves the frame
ring; a scene that re-creates the same variant before the old one drains would
exercise it.

### 8.6 P2.2 step 3 — recording references: assessment

Wiring `begin_use()`/`end_use()`/`abort_use()` to the driver requires knowing
when a pipeline is bound to a command buffer that is being recorded and when
that command buffer is submitted. In this engine that binding happens inside
the render graph (`draw_graph.add_*_list_bind_pipeline`), not in
`RenderingDevice` or in the driver. Hooking it therefore requires render-graph
changes, which are larger than the documented "isolated hook" policy for
patches. The hook points that would be needed are:

- render graph execution: `vkCmdBindPipeline` during graph replay (begin),
- command buffer submission in `execute_chained_cmds` (end, with the group
  serial),
- graph reset / command buffer discard (abort).

Until those hooks exist, the driver relies on premise P1 (the frame fence) and
on the renderer's frame-ring ownership, exactly as upstream's deferred
destruction does. This is recorded as a limitation in section 7.

### 8.8 Runtime frame-time comparison (churn project)

`tests/kaizen/pipeline_churn_project/` ran 120 frames, 960 meshes created and
freed, with the Khronos validation layer active. Frame time is dominated by
shader compilation (8 distinct variants per frame); the deferred-destruction
path adds no measurable overhead during active rendering.

| Configuration | mean | p50 | p95 | p99 | worst |
| --- | --- | --- | --- | --- | --- |
| `KAIZEN_PIPELINE_LIFETIME=off` | 132.369 ms | 133.333 ms | 149.699 ms | 150.000 ms | 150.000 ms |
| `KAIZEN_PIPELINE_LIFETIME=on` | 132.369 ms | 133.107 ms | 150.000 ms | 150.000 ms | 150.000 ms |

Difference is within measurement noise. The deferred path's cost is paid at
frame boundaries and teardown, not during the sampled frame. A scene that
creates pipelines cheaper (cached variants) would isolate the drain cost
better; that is future work.

### 8.9 Submission-failure audit (P2.2)

The registry's invariants (I1-I11) are only as strong as the driver's
cleanup discipline. The following table audits every exit point of the path
that a pipeline object takes from `pipeline_free()` to `destroy()`, and states
the obligation that must hold at each point. Obligations marked `[verified]`
were checked by inspection and, where noted, by a test.

| # | Exit point | Location | Obligation | Status |
|---|---|---|---|---|
| 1 | `_kaizen_pipeline_retire` called with a live host ref | `pipeline_free` | Cannot happen: `pipeline_free` is only called from `_free_pending_resources`, after the RID is freed, so no owner holds the pipeline. | `[verified by construction]` |
| 2 | `acquire()` returns null (registry full) | `_kaizen_pipeline_retire` | Drain destroyables to free slots; if still full, wait for device idle and retry. Last resort: destroy immediately (device is idle, so safe). | `[verified]` (code path) |
| 3 | `mark_use()` fails (state not Resident/Candidate) | `_kaizen_pipeline_retire` | Cannot happen: the object was just acquired as `Resident`. | `[verified by construction]` |
| 4 | `release()` returns false | `_kaizen_pipeline_retire` | Cannot happen: `host_refs == 1` at this point. | `[verified by construction]` |
| 5 | `retire_now()` returns false | `_kaizen_pipeline_retire` | Unreachable (no refs held). Fallback: wait for device idle, `confirm_all`, `destroy`, then `vkDestroyPipeline`. | `[verified]` (fallback path) |
| 6 | `collect_destroyable` returns nothing after retire | `_kaizen_pipeline_retire` | Not an error: the object stays `Retiring` until a later `fence_wait` or teardown. `pending_bytes` accounts for it. | `[verified]` |
| 7 | `vkQueueSubmit` fails | `command_queue_execute_and_present` | The driver returns `FAILED` before recording a serial. No serial is stored in the fence. The group stays open; the next successful fenced submission closes it. | `[verified]` (code path) |
| 8 | `fence_wait` fails (`vkWaitForFences` error) | `fence_wait` | Returns early without calling `confirm()`. The pipeline stays retiring. The destructor's `confirm_all()` is the escape hatch after device idle. | `[verified]` (code path) |
| 9 | `fence_wait` succeeds but fence had no kaizen serial | `fence_wait` | No-op for the registry. Happens for fences that were never associated with a pipeline submission (e.g. empty submits). | `[verified]` |
| 10 | Fence is reused without being waited | driver lifecycle | Cannot happen: `_stall_for_frame` waits the fence before reuse; transfer workers are waited before reuse. | `[verified by construction]` |
| 11 | `set_completed_serial` is never called again | runtime | The object stays retiring until the next frame's fence wait or teardown. No premature destruction. | `[verified]` |
| 12 | `destroy()` fails | `_kaizen_pipeline_drain` | The slot stays `Destroyable`; the VkPipeline handle is retained in `kaizen_pending_pipelines[index]` and retried on the next drain. The loop breaks to avoid spinning. | `[verified]` (code path) |
| 13 | Destructor: `vkDeviceWaitIdle` fails | `~RenderingDeviceDriverVulkan` | Proceeds to `confirm_all` and drain anyway; the device is being destroyed regardless. | `[verified]` (code path) |
| 14 | Pipeline freed while a command buffer is still recording it | renderer contract | Not protected by the current driver: `recording_refs` is not wired. Relies on premise P2 (frame-ring ownership). This is the open gap. | `[open gap]` |

Entries 7-9 cover the submission chain that the registry does not directly
observe: the driver never calls `begin_use`/`end_use`/`abort_use` for a
pipeline, because recording refs are not wired (entry 14). The failure paths
that the registry *does* observe (1-6, 10-13) all leave the registry
consistent, either by construction or by an explicit fallback.

The one path that is genuinely unprotected is entry 14: a pipeline freed
through the frame ring while a command buffer under construction still
references it. The registry's `recording_refs` would catch this, but the
driver does not feed it. Premise P2 documents this as a renderer-level
contract; section 8.6 identifies the render-graph hooks that would close it.

### 8.10 Render-graph hook investigation (P2.2)

The candidate hook points identified in section 8.6 were investigated against
the actual call sites:

- `draw_graph.add_draw_list_bind_pipeline` / `add_compute_list_bind_pipeline` /
  `add_raytracing_list_bind_pipeline` (`rendering_device.cpp:5861, 6569, 6837`)
  only record a bind into the graph; they do not execute it. Hooking here
  would protect the pipeline for the whole frame, which is correct but
  conservative (it would pin every pipeline touched by the graph until the
  frame's fence is confirmed, which is already what the frame ring does).
- The actual `vkCmdBindPipeline` happens during `draw_graph.end()` inside
  `execute_chained_cmds` (`rendering_device.cpp:8425-8444`), which is where
  command buffers are recorded. Hooking here would be precise but requires the
  graph to call back into the driver or into a Kaizen-owned recorder.
- Submission and fence association happen in `command_queue_execute_and_present`
  (already hooked for serials).
- Command-buffer discard happens in `command_pool_reset` and in the frame
  ring's `_free_pending_resources`; a discarded buffer must call `abort_use`.

Conclusion: a single ownership point in the render graph is feasible only if
the graph exposes a "pipeline acquired for this frame" callback. The current
graph does not. Two options remain:

1. Add a Kaizen-owned recorder that the graph calls at bind time (begin) and
   that `execute_chained_cmds` drains at submission time (end/abort). This
   touches the graph's bind and end paths.
2. Keep premise P2 and add a regression test that asserts
   `recording_refs == 0` at `pipeline_free()` time in debug builds. This does
   not remove the fragility but makes a violation loud.

Option 2 is the smaller change and is consistent with the "isolated documented
hooks" policy. Option 1 is the correct long-term fix and is deferred to P2.3
alongside creation-side eviction, because both require render-graph changes.

### 8.7 P2.2 closure status

| Criterion | Status |
| --- | --- |
| Churn in runtime | Validated (120 frames, 960 meshes, 204 pipelines, 0 validation errors). |
| Recording refs | Not wired; render-graph hooks identified (see 8.6) but not implemented. |
| Submission failure paths | `abort_use` (unit) + device forced-failure test; failure-map audit pending. |
| Eviction | Not wired (creation-side; P2.3 territory). |
| Graphics + Compute | Validated on the device test. |
| Continuous rendering | No device-idle stall introduced by the deferred path. |
| Shutdown | 0 pipelines pending. |
| Performance | Off vs on comparison done (8.8): no measurable frame-time overhead. |

## 7. Limitations

1. **Opt-in, destruction-only.** The driver defers destruction safely when
   `KAIZEN_PIPELINE_LIFETIME=on`; it does not deduplicate or evict during
   creation, and the default path is unchanged.
2. **No in-engine performance data.** Hit/miss rates, memory reclaimed and the
   cost of the enabled path are not measured.
3. **Raytracing pipelines are not covered.** `raytracing_pipeline_free()` still
   destroys immediately.
4. **Conservative last use.** The driver retires an object with the highest
   serial submitted on each queue at free time, not the exact submission that
   used it. This can delay destruction by at most the in-flight batch; it
   cannot destroy too early.
5. **Recording references are not wired in the driver.** The renderer's
   existing rule still applies: a pipeline RID is freed after the frame that
   used it, so no command buffer under construction references it. The
   registry's `begin_use()`/`abort_use()` are tested but are not called by the
   driver yet. Wiring them requires a recording-level hook, not a destruction
   hook.
6. **Serial 0 is "never used".** A backend that uses 0 as a real submission
   serial would make unused objects look submitted and submitted objects look
   unused. Backends must start serials at 1.
7. **No persistence or size estimation of driver memory.** `bytes` is supplied
   by the caller; the registry never queries the driver for the real allocation
   size. The driver path passes 0 because it is a destruction queue, not a
   budgeted cache.
8. **Single node, single device.** The registry is process-local and assumes one
   Vulkan device. Multi-device or multi-GPU routing is out of scope.
