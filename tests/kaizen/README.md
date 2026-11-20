# Kaizen Diagnostics Tests

This directory contains executable tests for Kaizen-owned subsystems. They are
independent of the engine build unless stated otherwise; the goal is that every
claim in `docs/architecture/resource-diagnostics.md` can be reproduced.

## Resource registry tests

`test_resource_registry.cpp` exercises `kaizen/diagnostics/rendering/resource_registry.h`.
The registry core depends only on the C++ standard library, so a full engine
build is not required.

```bash
./tests/kaizen/run_tests.sh          # correctness
./tests/kaizen/run_tests.sh --bench  # correctness + microbenchmark
```

Measured on the reference machine (Xeon E5-2680 v3, GCC, `-O2`):

| Metric | Value |
| --- | --- |
| Checks | 182 |
| Failures | 0 |
| Basic op (alloc+release+confirm) | 54.0 ns mean, p50 53.9 / p95 54.3 / p99 54.6 |
| Detailed op (event log) | 64.1 ns mean, 15.6 Mops/s |
| Near-capacity op | 54.0 ns (free list stays O(1)) |
| Registry footprint (65 536 slots) | 3.5 MiB (56 B per slot) |

Throughput on a shared registry (maximum contention) is 18.63 / 1.35 / 0.48 /
0.23 Mops/s at 1 / 2 / 4 / 8 threads. The registry serializes operations and is
intended for the render thread; the multi-threaded test proves safety, not
scalability. See `docs/architecture/resource-diagnostics.md` section 7.2.

Acceptance criteria covered:

- 100 000 allocations and releases with exact counters.
- Slot reuse increments the generation; stale handles are rejected.
- Double free is detected without corrupting counters.
- A full registry fails in a controlled way and recovers after a release.
- Byte accounting never underflows; peak tracks the live total.
- CPU and GPU registries stay independent (same index, different handles).
- Deferred release moves bytes from alive to pending and only then to released.
- Concurrent alloc/free from 8 threads (200 000 operations).
- Snapshot invariants hold while other threads allocate and release.
- The event ring records allocation, release request and release completion,
  and keeps the newest 1 024 entries when it overflows.
- Generation wrap skips zero and invalidates the previous handle.

Overflow of the 32-bit generation counter is not reachable in practice (it
would require 4.29 billion reuses of the same slot). On wrap the code skips
zero, because zero is reserved for null handles.

## Pipeline registry tests

`test_pipeline_registry.cpp` exercises the P2 core in
`kaizen/rendering/pipeline_cache/`: `PipelineRegistry`, `PipelineLRU`,
`PipelineRetirementQueue` and `PipelineSubmissionTracker`. Like the resource
registry, it depends only on the C++ standard library.

```bash
./tests/kaizen/run_tests.sh          # both suites
./tests/kaizen/run_tests.sh --bench  # both suites + microbenchmarks
```

Measured on the reference machine (Xeon E5-2680 v3, GCC, `-O2`; one
representative run, approximately ±10 % run-to-run):

| Operation | mean | p50 | p95 | p99 | Throughput |
| --- | --- | --- | --- | --- | --- |
| Hit: `acquire` + `release` | 27.9 ns | 27.8 ns | 27.9 ns | 28.2 ns | 35.83 Mops/s |
| Hit, near capacity | 27.6 ns | 27.4 ns | 27.4 ns | 27.7 ns | 36.24 Mops/s |
| Miss: new slot + `release` | 108.4 ns | 86.3 ns | 103.8 ns | 154.4 ns | 9.23 Mops/s |
| Evict → retire → destroy | 154.5 ns | 152.9 ns | 153.5 ns | 226.7 ns | 6.47 Mops/s |
| Full lifetime | 162.8 ns | 161.7 ns | 162.4 ns | 206.5 ns | 6.14 Mops/s |
| Multi-queue lifetime | 193.0 ns | 191.7 ns | 192.4 ns | 237.6 ns | 5.18 Mops/s |

Shared-registry throughput (one hot key, maximum contention, machine not idle):
1 thread 34.06–35.85, 2 threads 2.69–6.95, 4 threads 2.06–3.16, 8 threads
0.90–1.67 Mops/s. A slot is 64 B; with free-list and LRU links the fixed cost is
76 B/slot (4.75 MiB at 65 536 slots). The suite is also clean under ASan+UBSan and TSan; sanitizers are not
part of the default script.

These numbers exclude Vulkan pipeline creation; a "miss" only allocates a
registry slot and inserts a key. Acceptance criteria covered:

- No eviction at or below the byte budget; LRU selects the least recently used
  resident object.
- Host and recording references independently prevent destruction.
- Destruction waits for every queue that used the object; graphics finishing
  does not release an object that compute still uses, and queues may report
  completion in any order.
- A failed or aborted recording creates no completion dependency.
- `retire_now()` is refused while a reference is live and waits for recorded
  queue usage after it is accepted.
- Fence groups map to one serial: unfenced submissions join the open group, the
  fenced submission closes it, and a fence wait confirms exactly that serial.
- A key has at most one live object; acquiring it while it retires is refused
  instead of creating a duplicate.
- Reclaimed slots produce new generations; stale handles are rejected by every
  operation.
- Capacity exhaustion fails in a controlled way and recovers after a destroy.
- Late use of a retired object is counted and refused.
- Four concurrent eviction threads plus a completion/destruction thread run
  80 000 operations without freeing a referenced object; the registry drains to
  zero at shutdown.
- Statistics match the observed lifecycle (hits, misses, candidates, retired,
  destroyed, resident/pending bytes).

## End-to-end churn test (requires the engine)

`churn_project/` is a minimal project that creates two GPU textures per frame,
draws them, and frees them while the GPU may still reference them. It validates
that free requests enter `PendingRelease` and are confirmed by later frames
rather than by the teardown flush.

Build the template, then run:

```bash
KAIZEN_DIAGNOSTICS=validation \
  bin/kaizen.linuxbsd.template_release.x86_64 \
  --path tests/kaizen/churn_project --resolution 320x240
```

Reference result (600 churned textures, 300 frames):

```
churn: completed 300 frames, 600 textures
[Kaizen][Resources] GPU: allocs=685 releases=685 live=0 pending=0 live_bytes=0 B
  peak_bytes=4.45 MiB invalid=0 stale=0 double_free=0 full=0 | CPU: ...
  | runtime_releases=604 teardown_releases=81
```

`runtime_releases` must be large relative to `teardown_releases`: it is the
number of releases confirmed by frame epochs while the engine was running.
`teardown_releases` is the number of resources still alive when the device was
finalized (the scene and its default textures). `live=0` and `releases == allocs`
are the pass criteria.

## Vulkan device-level pipeline lifetime test (requires a GPU)

`vulkan/pipeline_lifetime_vulkan.cpp` validates the P2 model against real
`VkPipeline` objects. It creates a graphics and a compute queue, signals a
timeline semaphore per submission, and only destroys a pipeline after the
registry observed both queues completing.

```bash
./tests/kaizen/vulkan/run_vulkan_tests.sh
```

Requirements: Vulkan 1.2, a device with graphics + compute queues and timeline
semaphores, `VK_LAYER_KHRONOS_validation`, and `spirv-as`. The script exits 77
(skip) when they are missing; a skipped run is not a pass.

Reference result on the RX 550 (RADV POLARIS12): 70 checks, 0 failures, 0
validation errors. Scenarios: multi-queue completion in both orders, blocked
destruction while recording, aborted submission, device-idle drain, zero live
objects at shutdown, and key reuse after retirement.

## Vulkan image-buffer and texture-buffer callback tests

The tests previously sketched here as empty files were removed: they contained
no code and were never compiled, so they provided no coverage.

Validating `UNIFORM_TYPE_IMAGE_BUFFER` and texture buffers inside driver
callbacks requires a live Vulkan device plus a shader that binds and writes a
texel buffer. The benchmark harness cannot express that today. The remaining
work is tracked in `docs/architecture/resource-diagnostics.md` under
"Pending validation"; it must not be reported as tested until a device-level
harness exists.

The current evidence for those two patches is limited to: the engine builds,
the S1 benchmark renders correctly, and no tracked resource leaks. That is
sufficient to prove the normal path still works, not to prove the new paths are
exercised.

## In-engine pipeline destruction (opt-in)

The Vulkan driver defers `vkDestroyPipeline` through the registry only when
`KAIZEN_PIPELINE_LIFETIME=on` is set. With the variable absent, destruction is
the upstream immediate call. The hooks are documented in
`patches/rendering-device/004-pipeline-lifetime-integration.md`; the model and
the remaining work are in `docs/architecture/pipeline-lifetime.md`.
