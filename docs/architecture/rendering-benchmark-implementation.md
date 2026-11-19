# Rendering Benchmark Implementation Report

> **Status:** Implemented, not runtime-validated
>
> - **Implementation range:** `d8e1623` → `7c129d3` (7 commits)
> - **Reported branch state:** `main` synchronized; working tree clean at `7c129d3`
> - **Scope:** `benchmarks/` and benchmark documentation
> - **Runtime measurements:** none

This report records the implementation state of the rendering benchmark project. It complements, but does not replace, the [benchmark scene specification](rendering-benchmark-scenes.md), [benchmark methodology](rendering-benchmarks.md), and executable-project instructions in [`benchmarks/README.md`](../../benchmarks/README.md).

The implementation report describes authored code and recorded engineering decisions. It does **not** claim that the GDScript has passed the engine parser, that a template has been built, or that any performance result has been measured.

## 1. Implementation status

| Item | Status | Notes |
|---|---|---|
| Shared project `benchmarks/rd_3d/` | Implemented | Shared by S1–S4 and S6 |
| S1 — static geometry | Implemented | Opaque boxes |
| S2 — instancing | Implemented | One `MultiMesh` |
| S3a/S3b — material variety | Implemented | Material-count and shader-feature variants |
| S4 — lights and shadows | Implemented | Directional, spot, and omni lights |
| S5 — 2D/Canvas | Not authored | Separate project and harness intentionally deferred |
| S6 — transparency and post-processing | Implemented | Transparent quads and P0–P5 presets |
| S7 — editor viewport | Manual procedure documented | No automated harness scene |
| Report checker (`benchmarks/tools/check_report.py`) | Implemented | 24 tests executed; no engine run |
| Harness runtime validation | Not performed | No template build, GPU, or display was available |
| Performance measurements | Not available | All runtime values remain unmeasured |

“Implemented” means the scene and harness source were authored. It does not mean the implementation has been parsed or executed by the engine.

## 2. Change history

The implementation was delivered over seven commits:

| Commit | Delivery |
|---|---|
| `9ca2cec` | Consolidated the project into `rd_3d`, implemented S2, and registered scenes in the harness |
| `69032d0` | Added S3a/S3b, per-source PSO counters, and the `first_120` measurement window |
| `e6a3d01` | Added S4, `--bench-spots`, `--bench-omnis`, and the shadow variant |
| `3dc9bf8` | Added S6, screenshot capture, image diff, and `--bench-min-diff` |
| `811b02f` | Documented S7 as a manual procedure and recorded S5/S7 status |
| `1cfdcbc` | Corrected documentation wording about automation and implementation status |
| `7c129d3` | Implemented the report checker and its tests, and documented the automated part of the gate |

The aggregate change over this range is 22 files under `benchmarks/` and `docs/`, with approximately 2,345 insertions and 151 deletions. The benchmark project contains approximately 1,538 lines of GDScript.

## 3. Project architecture

S1–S4 and S6 share one Godot-style project because they use the same renderer and project settings. The harness selects the requested scene through `--bench-scene` and a scene registry near the top of `bench_harness.gd`.

```text
benchmarks/rd_3d/
  project.godot                  fixed benchmark settings
  scenes/
    bootstrap.tscn                empty startup scene; harness loads target scene
    s1_static.tscn                S1: opaque boxes
    s2_instancing.tscn            S2: MultiMesh instances
    s3_materials.tscn             S3: material variants
    s4_lighting.tscn              S4: lights and shadows
    s6_post.tscn                  S6: transparent quads and P0–P5
  scripts/
    bench_harness.gd              sampling, validation, and JSON reporting
    bench_scene_common.gd         shared deterministic scene helpers
    scene_s1.gd                   S1 content and frame-indexed camera
    scene_s2.gd                   S2 content and frame-indexed camera
    scene_s3.gd                   S3 content and frame-indexed camera
    scene_s4.gd                   S4 content and frame-indexed camera
    scene_s6.gd                   S6 content and frame-indexed camera
```

Scene content is generated procedurally. No binary assets are required. Integer seeds and hashes make the content reproducible, and a camera driven by frame index makes the 300-frame sequence independent of the machine's achieved FPS. The harness does not use `--fixed-fps` to enforce determinism.

The shared project pins its benchmark settings, including a 1920×1080 window, VSync disabled, MSAA disabled, prepass enabled, and SSAO/SSIL quality set to 2. The effective runtime configuration is recorded separately so command-line overrides are not confused with project defaults.

S5 remains deliberately separate in scope: a Canvas benchmark needs a 2D project and a harness path appropriate to that renderer. Copying the current 639-line harness would create two competing implementations of the measurement contract. The decision is to resolve harness sharing before authoring S5, not to duplicate the current harness by default.

## 4. Harness measurement contract

The harness is designed for exported template builds. It refuses `--headless`. A tools/editor build can be used during scene development only when `--bench-allow-editor` is supplied; such a report is explicitly invalid.

Default sampling:

| Parameter | Value |
|---|---:|
| Warm-up | 900 frames |
| Sampling window | 3,600 frames |
| Camera loop | 300 frames |
| Window alignment | Both counts are multiples of 300 |

The fixed frame-index windows ensure that repeated runs cover the same camera sequence regardless of achieved FPS.

The report records effective runtime information, including rendering method, driver, display server, VSync, MSAA/TAA/3D scaling, adapter identity, API version, and window size.

Per-frame samples include:

- `frame_us`, `process_ms`, and `physics_ms`;
- draw calls, objects, and primitives;
- texture, buffer, and video memory monitors;
- viewport CPU/GPU render time where the backend provides it;
- per-frame deltas for the five pipeline-compilation counters.

A single JSON report includes the content hash, frame-299 SHA-256 hash, timing summary (median, p95, p99, worst frame, and hitches), effective settings, and scene parameters. Exit codes distinguish valid completion (0), harness startup failure (2), invalid measurement (3), and report-write failure (4).

### 4.1 Pipeline compilation semantics

Pipeline compilation counters are cumulative from engine startup. Therefore, absolute totals are not a valid steady-state gate. The harness samples per-frame deltas.

- `pipeline_compilations.first_120` **starts before the target scene is loaded** and ends after the first 120 frames the harness renders, so the compiles caused by loading and first drawing that scene are included. This is the S3b observation window.
- `pipeline_compilations.total` is cumulative for the run and includes warm-up. A non-zero value is not by itself a failure.
- `timing.pso_in_sample` is the steady-state gate. It must be zero throughout the sampling window.

This distinction corrects an earlier documentation interpretation that treated the cumulative total as the validity gate.

### 4.2 Validity

The harness writes a report even when a run is suspect, with `valid` and `invalid_reason` describing the result. The exact validity contract is maintained in the [benchmark project README](../../benchmarks/README.md) and [benchmark methodology](rendering-benchmarks.md).

A valid published cell requires, among other conditions:

- an exported template, not a tools build;
- a non-headless run;
- disabled VSync as observed by the display server, with driver-level enforcement checked separately;
- no pipeline compilations during the sampling window;
- a successful frame-hash capture;
- backend GPU timing represented as unavailable when unsupported, never fabricated as zero.

The harness does not itself establish cross-run frame-hash equality. The mechanical part of this gate is implemented in `benchmarks/tools/check_report.py`, which also compares repeated runs of one cell (frame hash, draw-call median, and S3b `first_120`). Driver-forced VSync, thermal throttling, and wrong content on screen remain manual checks.

## 5. Scene-specific implementation decisions

### 5.1 S1 — static geometry and draw-call expectations

Forward+ can merge adjacent identical surfaces into an instanced draw through the renderer's repeat path (`RenderElementInfo::repeat`, bounded by `MAX_REPEATS = 2²⁰−1`). Consequently, the original expectation of approximately `box_count + 1` draw calls is not portable:

- Forward+ may produce approximately two draws for the scene, largely independent of box count.
- Mobile may report approximately one draw per instance.

The acceptance criterion is therefore repeatability and stability across equivalent runs, not a single fixed draw-call count. Primitive-count checks remain separate.

### 5.2 S3 — material and pipeline variants

S3a varies material count; S3b exercises eight shader-feature combinations. The `first_120` per-source PSO counters are used to observe pipeline creation caused by loading and first drawing the scene.

The harness records the counter breakdown but does not assume that a particular material feature maps to a particular PSO counter. That mapping must be established from measured results.

ORM material setup uses `ORMMaterial3D`, because `orm_texture` is registered on that class. Using `StandardMaterial3D` with separate texture properties would not exercise the same property path.

### 5.3 S4 — lights and shadows

S4 exposes spot and omni counts through `--bench-spots` and `--bench-omnis`, plus a shadow variant selected with `--bench-variant shadows`. The configured sweep is:

`(0,0), (8,0), (0,32), (8,32), (32,64)`

The draw-call response is recorded rather than assumed. A change across the light sweep is a renderer-path finding that must be documented.

### 5.4 S6 — transparency and post-processing

S6 combines transparent quads with post presets P0–P5. The harness can save the frame-299 screenshot, compare it with a reference image, and invalidate a run when `--bench-min-diff` requires a minimum image difference that is not reached.

The diff is a whole-frame byte comparison. It proves that a preset changed the image; it does not establish visual correctness or quality. P5 volumetric fog is not applicable on renderers that do not support it and must not be reported as a zero-cost effect.

### 5.5 S7 — editor viewport

S7 is documented as a manual editor procedure using the S1 scene in single-viewport and 2×2 viewport layouts. It has no runtime harness scene. Editor results must remain separate from template results because the editor has a different rendering/threading configuration and includes editor overlays and gizmos.

## 6. Engine API and scene-authoring corrections

The implementation work recorded these source/API constraints:

1. ORM texture assignment requires `ORMMaterial3D`; the property is registered by that class.
2. `Node3D.look_at()` requires the node to be inside the scene tree. Spot-light setup uses `look_at_from_position()` where the node is not yet in-tree.
3. `Basis.looking_at()` makes the negative Z axis point along the direction it is given. S6 quads pass the direction from the camera toward the quad, which leaves their positive-Z face pointing back at the camera.
4. S1 draw-call expectations must account for Forward+ surface repetition/instancing rather than assume one draw per node.
5. PSO monitors are cumulative and must be interpreted as deltas for per-frame and in-window checks.
6. The first-120 PSO baseline is captured before the target scene is loaded, so the window includes scene-load and first-draw compiles.

These are implementation notes, not substitutes for the upstream API contract. If the engine source changes, re-check the relevant call sites and property registration.

## 7. Verification performed and not performed

### Source and repository checks reported as completed

- Material, light, environment, image, and performance-monitor property names were checked against the engine source.
- Script structure, indentation, and whitespace were reviewed.
- `git diff --check` passed.
- Documentation consistency was reviewed for status labels, JSON fields, command-line arguments, and acceptance criteria.
- The report checker's 24 unit tests were executed in this environment and passed.
- The final reported repository state at `7c129d3` was `main` synchronized with a clean working tree.

### Runtime validation not performed

No engine build or benchmark execution was performed. In particular:

- GDScript has not been parsed by a built Kaizen executable.
- No template binary has been built for this benchmark.
- No GPU/display-backed run has taken place.
- No CPU, GPU, memory, or frame-time result exists.
- Metal parity, WebGL behavior, and browser performance remain unverified.

Accordingly, all runtime results remain **not measured**. Source inspection and structural checks do not establish runtime correctness.

## 8. First execution plan

The first real cell must be **S1/Q0** on one known configuration, preferably D1 or M1, before running the rest of the suite.

1. Build an exported template for the target platform and renderer.
2. Run S1/Q0 with the documented warm-up and sample window.
3. Confirm the report is valid, inspect `timing.pso_in_sample == 0`, and run it through `benchmarks/tools/check_report.py`.
4. Compare frame hashes across repeated S1 runs.
5. Check draw-call stability and primitive-count sanity.
6. Only after S1 validates the harness, proceed to S2, S3a/S3b, S4, and S6.
7. Keep S7 manual and decide the shared-harness design before implementing S5.

Do not publish performance claims until the reports come from real hardware, pass the validity gate, and include the full runtime configuration.

## 9. Related documents

- [Benchmark project instructions](../../benchmarks/README.md)
- [Benchmark scene specification](rendering-benchmark-scenes.md)
- [Benchmark methodology](rendering-benchmarks.md)
- [Rendering audit (R0)](rendering-audit.md)
- [Rendering architecture and roadmap](rendering-roadmap.md)
