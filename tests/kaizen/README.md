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

### Optimization pass (2026-11-22)

Key lookup moved from `std::unordered_map<std::string, uint32_t>` to an
open-addressing table over slot indices (no per-entry heap allocation, no
pointer chasing on hits), the canonical-key hit path no longer copies the
whole key identity on every acquire, `PipelineKey` uses a compact field
encoding with a word-at-a-time hash, the LRU touch has a most-recently-used
fast path, and the spinlock pauses while spinning. A telemetry bug where
`release()` inflated `shared_acquires` was also fixed.

Measured before/after on the same sandbox machine (GCC, `-O2`; one
representative run each, approximately ±15 % run-to-run):

| Operation | Before | After | Δ |
| --- | --- | --- | --- |
| Hit: `acquire` + `release` | 63.5 ns | 37.2 ns | −41 % |
| Hit, near capacity | 85.2 ns | 45.0 ns | −47 % |
| Miss: new slot + `release` | 1206 ns | 485 ns | −60 % |
| Evict → retire → destroy | 326 ns | 136 ns | −58 % |
| Full lifetime | 285 ns | 177 ns | −38 % |
| Multi-queue lifetime | 303 ns | 294 ns | −3 % |
| Throughput, 8 threads (shared) | 1.30 Mops/s | 1.82–2.95 Mops/s | +40–127 % |

New benchmarks cover the canonical `PipelineKey` path used by
`PipelineCacheRD`: a realistic multi-field key hits at ~161 ns
(acquire+release, dominated by hashing and comparing ~700 B of identity)
and a unique-key miss at ~4.3 µs including key construction. The suite
grew to 1 747 checks (including a key-table churn/rehash test) and stays
clean under ASan+UBSan and TSan. Fixed cost is now 116 B/slot including
the key table (96 B slot + 4 B free-list + 8 B LRU + 8 B key table, worst
case), replacing the previous 108 B/slot plus an unaccounted hash map.

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
- Key-table churn: filling the registry, destroying half the entries
  (tombstones) and re-acquiring them yields new generations without merging
  identities, for both numeric and canonical keys.

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

## Pipeline churn project (requires the engine)

`pipeline_churn_project/` creates 8 `MeshInstance3D` per frame with
`StandardMaterial3D` variants that differ in transparency, culling, shading mode,
depth draw and vertex color, frees them with `queue_free()` and renders with a
camera. It validates that deferred pipeline destruction is exercised during
active frames, not only at teardown.

```bash
KAIZEN_PIPELINE_CACHE=on \
KAIZEN_PIPELINE_CACHE_VERIFY=on \
KAIZEN_PIPELINE_LIFETIME=on \
  bin/kaizen.linuxbsd.editor.x86_64 \
  --path tests/kaizen/pipeline_churn_project --resolution 1280x720
```

Latest runtime result (120 frames, 960 meshes, RX 550): `PipelineCache`
verification reports equivalent-key RID sharing and partial release, blocks a
probe acquire during retirement, and allows a new registry generation after the
physical-destruction callback. The verification deliberately adds test-only
acquisitions, so its cache hit/miss counters are not workload metrics. The
Vulkan lifetime registry retired and destroyed all 204 physical pipelines with
no resident, retiring, destroyable, or pending objects. Teardown still emits a
deferred shader-release warning and the generic warning for one shader RID;
independent multi-consumer reuse and clean renderer-owner teardown remain open.

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

`run_vulkan_tests.sh` now builds against the vendored `thirdparty/vulkan/include`
(falling back to system headers), falls back from `clang++` to `g++`/`c++` instead of
requiring the former, and accepts `KAIZEN_SANITIZERS=address,undefined` to build the test
under sanitizers and tell the report so. Without that variable the report says the
sanitizers were not built; it can no longer claim otherwise.

## C# glue and class-reference consistency

`api_glue/check_api_glue.py` compares four sources that have to agree for every enum the
fork extends: the C++ declaration order (which is the value), the `BIND_ENUM_CONSTANT`
list (which is what ClassDB exposes), `doc/classes/<Class>.xml` (which is the
documentation), and the generated C# enum in `modules/mono/glue/.../Generated/`. It exists
because `087edfa7` added four `RenderingInfo` counters, hand-edited the generated C# enum
to match, and added no doc XML and no `<summary>` blocks - a state in which C# callers get
four undocumented values and the next regeneration deletes them. Running the checker
against that state reports 8 failures; against the tree as it is now it reports none.

`test_check_api_glue.py` (18 tests) drives the checker over a synthetic four-file tree
with one source broken at a time, so a checker that passes for the wrong reason shows up
here: a doc value that no longer matches the C++ position, an entry inserted in the middle
without renumbering, a glue entry with nothing bound behind it, a glue entry with no
summary, doc text drifting from the XML, a `[constant X]` reference that was not
translated to a `<see cref>`, and a doc entry for an enumerator that is not bound (which
must *not* be reported). `run_tests.sh` runs both the checker and the tests.

The rule it enforces is worth stating, because it is a design constraint on whoever extends
one of these enums next: the C++ declaration order is the value, and the XML is the doc
source, so the correct sequence is enumerator -> bind -> XML entry -> regenerate the glue.
Hand-editing the glue is only acceptable because this fork tracks it, and then only with
this checker saying the four sources still agree.

## Pipeline lifetime report helpers

`vulkan/pipeline_lifetime_report.h` holds the summary printing for the device-level
Vulkan test, and `vulkan/test_pipeline_lifetime_report.cpp` unit-tests it with no loader,
no device and no `spirv-as` - which is the only way that logic can be checked here, since
the test it serves skips (exit 77) on any machine without a GPU.

It was extracted for a reason: the report used to print `PASS` for four things the harness
does not test (`Command buffer reuse`, `Simultaneous submissions`, `Reset and discard`,
`Shutdown`), `CLEAN` for ASan/UBSan/TSan that `run_vulkan_tests.sh` never enables, and
`(validated in phases)` where six numeric counters should have been - while its exit code
was computed without the `validation_errors` count that the line above it used. So a run
printing `Result: FAIL` exited 0. The helpers make each line come from its own
`Phase {label, ran, passed}`, render untested claims as `NOT COVERED (<reason>)`, derive
the sanitizer state from `-DKAIZEN_TEST_SANITIZERS` (which `KAIZEN_SANITIZERS` now passes
along with the matching `-fsanitize`), and compute the verdict and the exit code from the
same `run_succeeded()`.

`run_tests.sh` builds the report test twice - plain, and with `-DKAIZEN_TEST_SANITIZERS` -
because the two states print different words and both have to be checked. Neither of those
two builds links a sanitizer; only `run_vulkan_tests.sh` with `KAIZEN_SANITIZERS` does
that, and it is the only place the "built with address" line may ever appear.

## In-engine pipeline destruction (opt-in)

The Vulkan driver defers `vkDestroyPipeline` through the registry only when
`KAIZEN_PIPELINE_LIFETIME=on` is set. With the variable absent, destruction is
the upstream immediate call. The hooks are documented in
`patches/rendering-device/004-pipeline-lifetime-integration.md`; the model and
the remaining work are in `docs/architecture/pipeline-lifetime.md`.

## HZB occlusion CPU invariants

`hzb_occlusion/` covers the three CPU terms the occlusion patches touch: the pyramid
build in `servers/rendering/renderer_scene_occlusion_cull.cpp`, and the level-0 scatter
and the camera-ray generation in `modules/raycast/raycast_occlusion_cull.cpp`.

Those two translation units cannot be compiled here at all - they include
`platform_config.h` and the `*.gen.h` headers that only exist after a SCons build, which
is exactly what this suite is written to avoid. So `extract_source_bodies.py` lifts the
five function bodies out of the engine sources verbatim at build time, and the test
supplies the shims they need (`LocalVector`, `RID`, `Ref`, the `Engine`/`RenderingServer`
singletons, a `Vector3` that mirrors `core/math/vector3.h`). The bodies are unmodified;
they compile warning-free under the same `-Wall -Wextra -std=c++17` the other two suites
use, which is checked on every run rather than asserted here.

Each case compares the extracted function against a reference written in the naive
pre-patch form - max over the 2x2 block with a clamp per sampled offset, a scatter with
a bounds test per texel, one division and one field reload per ray - and the comparison is
bit-exact, because bit-exactness is the claim being protected. Destinations are poisoned
with values the inputs cannot produce, so a texel or a ray slot that stops being written
is a failure rather than an accident of allocation. `update_mips()` is checked against a
hand-computed 4x4 pyramid too, so a reference that is wrong the same way twice cannot pass.

Two things this buys beyond the obvious:

- It catches the specific shortcut someone will be tempted to repeat. Replacing the
  per-ray `(float(x) + 0.5f) / width` with a multiply by a hoisted reciprocal fails 17 of
  the 114 cases: the values are not equal in float, and a texel that changes by one ULP can
  flip which instances survive the cull. Dropping the `odd_w` gate in the mip build fails 30.
- It fails loudly when the engine moves. `extract_source_bodies.py` requires each
  definition to match exactly once, so a rename or a signature change stops the build with
  instructions instead of silently testing nothing. Update its table and the matching
  `patches/*/*.md` note together.

The suite is built by `run_tests.sh` and is deliberately not registered in
`tests/meson.build`: it needs the extraction step as a codegen target, and meson remains
unvalidated for Kaizen changes. Adding a plain entry for the `.cpp` there would break the
configure step, since `hzb_extracted.inc` is generated into the build directory.

Not covered, and not coverable here: the Embree traversal between generation and scatter,
the threading that splits the ray loop across workers, the GPU side of
`Viewport.use_occlusion_culling`, and anything that depends on real engine types rather
than on the shimmed members. Timing claims still come from the offline harnesses
described in the patch notes, not from this test - `run_tests.sh` measures nothing.
