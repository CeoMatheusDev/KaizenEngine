# Pipeline Object Lifetime and Eviction (P2)

> **Status:** Core implemented — unit and concurrency tests pass; **not yet wired into the Vulkan driver**.
> **Scope:** `kaizen/rendering/pipeline_cache/`
> **Related:** [Pipeline cache and shader variants](rendering-pipeline-cache.md), [Resource lifetime diagnostics](resource-diagnostics.md), [Rendering roadmap](rendering-roadmap.md)

This document describes the Kaizen subsystem that will bound the number of
live pipeline state objects. It is deliberately separate from the persistent
`VkPipelineCache` blob: that blob stores driver-side creation data, while this
subsystem owns **objects with references** and decides when one may be
destroyed.

## 1. What upstream already does

Godot destroys a freed pipeline on the frame ring, not immediately:
`RenderingDevice::_free_pending_resources()` walks
`frames[p_frame].render_pipelines_to_dispose_of` and calls
`driver->pipeline_free()` (`servers/rendering/rendering_device.cpp:8193-8207`).
That function runs from `_begin_frame()` after
`_stall_for_frame()` waited on the slot's fence
(`rendering_device.cpp:8509-8513`), so the deferred destruction is safe for the
same single-main-queue model described in
[Resource lifetime diagnostics](resource-diagnostics.md) §5.1.

What upstream does **not** do is bound the cache:

- `PipelineCacheRD` keeps every variant it ever compiled until the whole cache
  is cleared or destroyed (`servers/rendering/renderer_rd/pipeline_cache_rd.cpp:81-94`).
- Each variant is a linear-scan entry; there is no budget, LRU, or admission
  policy.
- The pipeline cache blob is allowed to grow without a limit; the FIXME in
  `drivers/vulkan/rendering_device_driver_vulkan.cpp` notes that no size cap is
  applied.

P2 therefore adds a policy layer above the existing deferred destruction. It
does not change the ownership rules of `PipelineCacheRD` or the device cache.

## 2. Model

`PipelineRegistry` is a standalone, backend-agnostic core. It knows nothing
about `VkPipeline`, `RID`, or the renderer; the backend supplies handles, byte
sizes, and completion serials.

### 2.1 States and references

| State | Meaning |
| --- | --- |
| `Free` | Slot never allocated or already destroyed. |
| `Resident` | Usable and acquirable; least-recently-used entries may be evicted. |
| `EvictionCandidate` | Selected by the budget policy. No new acquisitions or recording scopes. Existing references keep it alive. |
| `Retiring` | No host or recording references; waiting for the backend completion serial. |
| `Destroyable` | Completion reached; the backend may destroy the object and call `destroy()`. |

Each slot carries a 32-bit generation. A handle is `{index, generation}`;
after a slot is reused the old handle is rejected. Generation 0 is reserved for
null handles, and wrap from `UINT32_MAX` goes to 1.

Two independent reference counts exist:

- `host_refs` — owners that may hand the pipeline out (`acquire()` /
  `release()`).
- `recording_refs` — command buffers that have begun recording but have not
  been submitted (`begin_use()` / `end_use()`).

A pipeline may only enter `Retiring` when **both** are zero. This is what
prevents the common race where a pipeline is evicted while a command buffer is
still recording it.

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

### 2.3 Destruction condition

The registry only moves a slot to `Destroyable` when all of the following hold:

1. `host_refs == 0`
2. `recording_refs == 0`
3. `last_use_serial <= completed_serial`

`last_use_serial` is the greatest serial passed to `mark_use()` or `end_use()`.
`set_completed_serial()` is monotonic: lower values are ignored, and every call
re-evaluates entries even when the value did not change, so an entry that retires
after completion was already reported still drains on the next call.

A serial of 0 means "never submitted". Such an object becomes destroyable as
soon as it has no references.

### 2.4 Key identity

A key maps to at most one live object. While an object with that key is
`Retiring` or `Destroyable`, `acquire()` returns a null handle and increments
`blocked_by_retirement`; it does not create a duplicate. The mapping is erased
only by `destroy()`, after which a new object (new generation) may be created.
This trades a possible stall for a strict "one object per key" invariant.

Callers that cannot tolerate the stall must drain destroyable entries before
acquiring. In a rendering path, that means calling
`set_completed_serial()` + `collect_destroyable()` + `destroy()` once per frame,
before pipeline lookups.

### 2.5 Shutdown

At device teardown the owner waits for the device to idle, calls
`set_completed_serial(UINT64_MAX)`, drains all destroyable entries, and then
destroys the registry. Every slot reaches `Free`, and the statistics report zero
resident and pending bytes. The concurrency test exercises the same sequence
after concurrent workers stop.

## 3. Completion serial contract

The registry does not query a GPU. Correctness depends on the serial meaning
"every queue that could have used the object has completed all work up to this
point".

| Backend | What a valid serial is | Status |
| --- | --- | --- |
| Vulkan | A device-wide progress point. The safest source is one timeline semaphore per queue, reduced to the minimum completed value across every queue the pipeline was submitted to. A single queue's frame fence is sufficient only if the pipeline was used exclusively on that queue. | Contract defined, not wired. |
| Metal | `MTLCommandBuffer` completion (completed handler or `waitUntilCompleted` for teardown). A per-queue command-buffer index can serve as the serial if completion is observed before advancing it. | Inherited contract, unverified (no macOS hardware). |
| D3D12 | Fence values per command queue; again the minimum across queues that used the pipeline. | Not implemented. |
| OpenGL / GLES3 | Not applicable — no pipeline objects with deferred destruction in the `RenderingDevice` sense. The registry stays unused. | Not applicable. |

The current `RenderingDevice` ring contract is **not** a sufficient serial for
P2 by itself: the tracked resource epoch in
[Resource lifetime diagnostics](resource-diagnostics.md) §5.1 is a scheduling
bound on the main queue. Pipeline destruction must additionally account for any
async-compute or secondary queue that bound the pipeline. Until timeline
semaphores are plumbed through `RenderingDevice`, the subsystem must not be
enabled on a backend with more than one queue using the same pipeline.

`VkPipeline` objects and the `VkPipelineCache` blob have different retention
rules and must never share an eviction decision:

- A `VkPipelineCache` is a data container; destroying it does not destroy the
  pipelines created from it, and pipelines remain valid after it is gone.
- Pipeline objects have references and in-flight use; the blob does not.
- Evicting a pipeline from the object registry is not a reason to drop the
  blob, and trimming the blob is not a reason to destroy an object.

The subsystem therefore exposes no API that touches `VkPipelineCache`.

## 4. Integration plan

The core is not called by the driver yet. The intended integration is an
isolated patch (`patches/rendering-device/004-*`) rather than edits to the
upstream renderer:

1. Add an owner-level hook where `PipelineCacheRD` creates and frees variants.
   `acquire()` on a miss, `release()` when the variant is dropped.
2. Wrap command-buffer recording with `begin_use()` / `end_use(serial)`; a
   variant that is evicted during recording remains alive until submission.
3. Call `enforce_budget()` once per frame, outside the `PipelineCacheRD` spin
   lock, and drain `collect_destroyable()` through `driver->pipeline_free()`.
4. Feed `set_completed_serial()` from a backend-wide completion point (timeline
   semaphore reduction, see §3). Do not reuse the resource tracker's frame
   number without proving the single-queue assumption.
5. Keep `RD::free_rid()` semantics unchanged: the registry decides *when* a
   pipeline object can be destroyed, not how RIDs are exposed to callers.

Because the integration changes destruction timing, it needs a device-level
Vulkan test before it can be enabled by default. That harness does not exist
yet; until then the subsystem is compiled but inactive.

## 5. Acceptance matrix

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
serial and destroys ready objects. After the workers stop, the test drains the
registry and asserts zero resident, candidate, retiring and destroyable entries,
zero resident/pending bytes, and `late_use == 0`.

## 6. Measurements

Registry microbenchmark (`./tests/kaizen/run_tests.sh --bench`, Xeon E5-2680 v3,
GCC `-O2`). One operation is listed per row; latencies are per operation over
64-operation batches, 1 000 000 operations. These are one representative run:
on a shared machine, means varied by roughly 10 % between runs, and p95/p99 are
sensitive to scheduling noise.

| Operation | mean | p50 | p95 | p99 | Throughput |
| --- | --- | --- | --- | --- | --- |
| Hit: `acquire` + `release` | 28.4 ns | 27.4 ns | 28.3 ns | 48.5 ns | 35.25 Mops/s |
| Hit with 4 095 other resident objects | 27.8 ns | 27.4 ns | 27.4 ns | 42.7 ns | 36.00 Mops/s |
| Miss: `acquire` (new slot) + `release` | 108.7 ns | 87.6 ns | 118.0 ns | 164.3 ns | 9.20 Mops/s |
| Evict → retire → destroy | 154.1 ns | 149.1 ns | 179.7 ns | 266.5 ns | 6.49 Mops/s |
| Full lifetime (record, submit, release, evict, destroy) | 159.9 ns | 156.9 ns | 161.8 ns | 276.3 ns | 6.25 Mops/s |

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

Interpretation and limits:

- These numbers exclude pipeline creation and GPU work. **Vulkan pipeline
  creation time is not measured here**; a miss in this benchmark allocates a
  registry slot and inserts a hash-map entry. Real hit/miss rates and eviction
  counts require an in-engine run, which does not exist yet.
- A slot is 40 B; the free list adds 4 B and the LRU links 8 B per slot, so the
  fixed cost is 52 B/slot (52 B × 65 536 ≈ 3.25 MiB), excluding the hash map and
  the retirement queue, which scale with live entries. The benchmark process
  does not report resident set size. `PipelineRegistry::get_slot_size()`
  returns the 40 B value so reports can be computed from the actual build.
- The registry serializes operations behind a spin lock. It is designed for
  the render thread; the concurrency test proves safety, not scalability. The
  multi-thread throughput row is a contention measurement, not a production
  configuration.
- `PipelineStats` exposes hit/miss counts, resident/candidate/retiring/
  destroyable counts, resident and pending bytes, evictions and destroyed
  objects for a future in-engine run. No in-engine P2 values are published.

The pipeline suite also runs clean under AddressSanitizer + UBSan and under
ThreadSanitizer (Homebrew clang 23.1.2, `-O1 -g`). Sanitizers are not part of
the default script because the runtimes are not installed on every machine.

## 7. Limitations

1. **Not integrated.** The driver does not call the registry; there is no
   in-engine hit/miss or memory data yet.
2. **No device-level test.** The acceptance tests validate the model, not a live
   `VkPipeline` destruction. The same gap applies to the two renderer patches:
   they need a Vulkan harness.
3. **Single serial per object.** The API accepts one completion serial. The
   backend must reduce per-queue completions correctly; the registry cannot
   detect a serial that covers only one of several queues.
4. **Serial 0 is "never used".** A backend that uses 0 as a real submission
   serial would make unused objects look submitted and submitted objects look
   unused. Backends must start serials at 1.
5. **No persistence or size estimation of driver memory.** `bytes` is supplied
   by the caller; the registry never queries the driver for the real allocation
   size.
6. **No admission control.** Creation is still synchronous; the subsystem only
   bounds how many objects stay resident.
