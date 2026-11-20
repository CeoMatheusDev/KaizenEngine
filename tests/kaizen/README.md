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
