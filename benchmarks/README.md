# Rendering benchmarks

Executable benchmark projects for the Kaizen rendering baseline. The measurement
contract is defined in [Benchmark scene specification](../docs/architecture/rendering-benchmark-scenes.md);
the instrument list and reference configurations are in
[Benchmark methodology](../docs/architecture/rendering-benchmarks.md). The implementation history,
engineering corrections, and current validation status are recorded in the
[benchmark implementation report](../docs/architecture/rendering-benchmark-implementation.md).

> **Status:** scenes **S1–S4 and S6 are authored; S5 is not written; S7 is a
> manual editor procedure without a harness scene**. Fifteen D1 cells have
> passed the validity gate in a template build at 1920×1080: S1/Q0 with 400
> boxes in [`results/2026-11-19_s1_d1_q0/`](results/2026-11-19_s1_d1_q0/README.md),
> S2, S3a, S3b and six S4 light cells in
> [`results/2026-11-19_d1/`](results/2026-11-19_d1/README.md), and S6 at four
> quads for P0–P5 in
> [`results/2026-11-19_s6_d1_q4/`](results/2026-11-19_s6_d1_q4/README.md).
> Cell counts outside those sessions have tools-build smoke runs only. See the
> [run log](../docs/architecture/rendering-benchmark-run-log.md) for what was
> observed and which defects the runs exposed.

## Layout

S1–S4 and S6 share one project, because they use the same renderer settings and
differ only in scene content. `--bench-scene` selects which scene the harness
loads.

```text
benchmarks/
  results/                       published cells, one directory per cell or batch
  tools/                         check_report.py and its tests
  rd_3d/
    project.kaizen
    scenes/bootstrap.tscn          empty main scene; the harness loads the real one
    scenes/s1_static.tscn          S1: separate MeshInstance3D boxes
    scenes/s2_instancing.tscn      S2: one MultiMesh
    scenes/s3_materials.tscn       S3: 64 meshes with separate materials
    scenes/s4_lighting.tscn        S4: boxes, a shadowed sun, spot/omni lights
    scenes/s6_post.tscn            S6: transparent quads and post presets P0-P5
    scripts/bench_harness.gd       autoload: scene registry, sampling, JSON report
    scripts/bench_scene_common.gd  shared deterministic material/light/environment
    scripts/scene_s1.gd            S1 content and frame-indexed camera
    scripts/scene_s2.gd            S2 content and frame-indexed camera
    scripts/scene_s3.gd            S3 content, variants, and camera
    scripts/scene_s4.gd            S4 content, light rings, shadow variant, camera
    scripts/scene_s6.gd            S6 content, post presets, screenshot camera
```

Adding a scene means adding its `.tscn`, its script, and one entry in the
`SCENES` registry at the top of `bench_harness.gd`.

## Requirements

- A Kaizen build with `rendering_device` enabled (Forward+ or Mobile). The
  scene also runs on Compatibility, but the report must record the renderer.
- An exported template for published measurements. Tools builds are accepted
  only with `--bench-allow-editor`, and the report is marked invalid.
- VSync disabled in the project **and** in the driver; `--disable-vsync` does not
  override driver-level enforcement.

## Running

Export the project with the preset for the target platform, then run the
template binary:

```text
<kaizen-template> --path benchmarks/rd_3d -- \
    --bench-scene S1 --bench-count 400 --bench-preset Q0 --bench-config D1 \
    --bench-frames 3600 --bench-warmup 900 \
    --bench-out /absolute/path/S1_D1_Q0.json
```

S2 uses the same command with `--bench-scene S2` and its own count, for example
`--bench-count 16384`.

S3 has two variants selected with `--bench-variant`:

```text
# S3a: uniform variety only. material_count sweep: 1, 16, 64.
<kaizen-template> --path benchmarks/rd_3d -- \
    --bench-scene S3 --bench-variant a --bench-count 64 \
    --bench-preset Q0 --bench-config D1 --bench-out /absolute/path/S3a_D1_Q0.json

# S3b: eight shader-feature combinations instead.
<kaizen-template> --path benchmarks/rd_3d -- \
    --bench-scene S3 --bench-variant b \
    --bench-preset Q0 --bench-config D1 --bench-out /absolute/path/S3b_D1_Q0.json
```

`--bench-count` is ignored in S3b, which always uses the eight features.

S4 takes the light counts directly; the specification's sweep is
(0,0), (8,0), (0,32), (8,32), (32,64) spots/omnis:

```text
<kaizen-template> --path benchmarks/rd_3d -- \
    --bench-scene S4 --bench-spots 8 --bench-omnis 0 \
    --bench-preset Q0 --bench-config D1 --bench-out /absolute/path/S4_8_0_D1_Q0.json
```

Add `--bench-variant shadows` to enable shadows on four spots.

S6 uses `--bench-preset` for the post preset and `--bench-count` for the quad
count. Capture a P0 reference, then diff each other preset against it:

```text
# P0 reference image
<kaizen-template> --path benchmarks/rd_3d -- \
    --bench-scene S6 --bench-count 4 --bench-preset P0 --bench-config D1 \
    --bench-screenshot /absolute/path/S6_P0.png \
    --bench-out /absolute/path/S6_P0_D1.json

# P2 records its difference from P0 in image_diff.mean_abs_diff
<kaizen-template> --path benchmarks/rd_3d -- \
    --bench-scene S6 --bench-count 4 --bench-preset P2 --bench-config D1 \
    --bench-reference /absolute/path/S6_P0.png \
    --bench-out /absolute/path/S6_P2_D1.json
```

`--bench-min-diff <value>` is optional. The gate is not a fixed threshold: an
`image_diff.mean_abs_diff` of exactly zero for a preset that enables an effect is
invalid because the effect did not run, and a small non-zero value is recorded
as measured. A per-preset threshold
belongs to whoever collects a cell, not to the specification.

The comparison runs once after sampling and reads every RGBA byte, so it can
take several seconds on a 1920×1080 frame; that cost is outside the sampled
window.

### S7 — editor viewport (manual)

S7 measures the editor's own viewport, so the runtime harness cannot drive it.
Use the editor binary and the S1 scene:

```text
<kaizen-editor> --editor --print-fps --path benchmarks/rd_3d
```

Open `scenes/s1_static.tscn`, keep `box_count = 400`, and measure two layouts:
S7a is one maximized 3D viewport, S7b is a 2×2 layout of the same scene.

Record `Editor FPS:` lines, the editor frame profiler, and a screenshot.
The screenshot must state the viewport pixel size and whether the Scene dock
preview, the inspector, and the 3D grid are visible; the grid and gizmos are
part of the measured frame.

Editor runs are reported as **editor**, never as engine performance. The editor
forces `separate_thread_render = 0`, so S7 numbers are not comparable with any
template cell, even on the same commit.

Without `--bench-out` the report goes to `user://bench/<scene>_<preset>.json`,
and its absolute path is printed on exit.

For scene development only, a tools build can run the same command with
`--bench-allow-editor`; the JSON then contains `"valid": false` and the reason.

**Q1 is not automated.** The project pins the Q0 settings. A Q1 cell requires
editing `project.kaizen` (MSAA 4×, which is `msaa_3d = 2`, and soft shadow
quality 4) and the scene environment (SSAO medium, SSIL medium, glow on at
intensity 0.8), then recording the resulting values. The harness writes whatever
settings are in effect and uses `--bench-preset` only as a label; it cannot prove
a run labelled Q1 matches the Q1 definition.

### Arguments

| Argument | Default | Meaning |
|---|---|---|
| `--bench-scene` | `S1` | Scene to load and report label: `S1`–`S4` or `S6` |
| `--bench-count` | scene default (400 for S1, 4096 for S2, 64 for S3) | Forwarded to the scene as its content count |
| `--bench-variant` | scene default (`a` for S3) | Forwarded to the scene as its variant label |
| `--bench-spots` | scene default (8 for S4) | S4 spot-light count |
| `--bench-omnis` | scene default (0 for S4) | S4 omni-light count |
| `--bench-preset` | `Q0` | `Q0`/`Q1` are report labels only (presets are project settings); on S6 it selects the post preset `P0`–`P5` |
| `--bench-config` | empty | Configuration label recorded in the report (`D1`, `M1`, ...) |
| `--bench-frames` | `3600` | Sampled frames, rounded up to a multiple of 300 |
| `--bench-warmup` | `900` | Frames before sampling, rounded up to a multiple of 300 |
| `--bench-out` | `user://bench/...` | Absolute report path |
| `--bench-no-hash` | off | Skip the frame-hash capture |
| `--bench-screenshot` | off | Save the frame-299 image as PNG |
| `--bench-reference` | off | Diff the captured frame against this PNG |
| `--bench-min-diff` | off | Mark invalid when `image_diff.mean_abs_diff` is below this value |
| `--bench-allow-editor` | off | Allow a tools build (report marked invalid) |

With the defaults, sampling starts at frame 900 and covers exactly 3600 frames,
so every run covers the same camera frames. Both windows are rounded up to a
multiple of the 300-frame loop.

The project pins a 1920×1080 window, and the report records the size the engine
actually got. A window manager can shrink the client area: the same request
measured 1600×834 on a 1600×900 primary display and 1920×1052 when moved to a
1080p output, because decorations took 28 px. `--resolution` does not win in
either case. Run fullscreen on an output that can show the pinned size — that is
how the published cell got exactly 1920×1080 — or record the decision to pin a
different size. The checker fails a mismatch because the frame would have been
rendered at another resolution.

### Renderer selection

The project pins Forward+, but the CLI overrides it per run:

```text
<kaizen-template> --path benchmarks/rd_3d --rendering-method mobile \
    --rendering-driver metal -- --bench-scene S1 --bench-count 400 ...
```

The report records the **effective** method, driver, display server, vsync
setting, viewport MSAA/TAA/3D scaling, adapter name, adapter vendor, and API
version as reported by `RenderingServer`, `DisplayServer`, and `Viewport`, so a
run started with an override cannot be mistaken for the project setting.
`window_get_vsync_mode` reports what the display server believes it is using; it
cannot prove the driver is not forcing a swap interval, which is why the
specification still treats a flat frame-time histogram pinned to the refresh
rate as a validity failure.

## What the harness records

One row per sampled frame:

- `frame_us`: wall-clock interval between consecutive `_process` callbacks. This
  includes presentation blocking; it is not a pure CPU measurement.
- `process_ms`, `physics_ms`: not sampled. `Performance.TIME_PROCESS` and
  `TIME_PHYSICS_PROCESS` are published as the maximum over 1,000,000 frames
  (`main/main.cpp:5186`), so inside a run they repeat the startup frame and say
  nothing about the sampled frames.
- `draw_calls`, `objects`, `primitives`.
- `texture_mem`, `buffer_mem`, `video_mem`.
- `pso_compilations`: **per-frame delta** of the cumulative
  `Performance.PIPELINE_COMPILATIONS_*` counters. Those counters never return to
  zero, so a delta is the only meaningful steady-state signal.
- `viewport_cpu_ms`, `viewport_gpu_ms`: from
  `RenderingServer.viewport_set_measure_render_time`.

One value per run:

- `timing.load_to_sample_ms`: wall time from the scene-load call to the first
  sampled frame. It is the cold-run measure the protocol asks for; on a warm run
  it is dominated by the 900-frame warm-up.

Monitor values are read once per frame, at the start of frame *n*. Draw-call and
memory monitors describe the frame that was just rendered (*n − 1*). The camera
for frame *n* is applied before that frame is rendered.

At the end the harness writes a single JSON document with the frame arrays, a
summary (median / p95 / p99 / worst / hitches), the engine version and hash, the
pinned project settings, the scene parameters, a content hash, and a frame hash
of rendered frame 299.

`screenshot` is the absolute path of the saved PNG (empty when none was
requested). `image_diff` is `{}` unless `--bench-reference` was used; otherwise
it holds `reference`, `width`, `height`, `mean_abs_diff` and `max_abs_diff`.

It also writes `pipeline_compilations.first_120` and
`pipeline_compilations.total`, both broken down by counter (`canvas`, `mesh`,
`surface`, `draw`, `specialization`). `first_120` starts **before the scene is
loaded** and ends after the first 120 frames the harness renders, so it includes
the compiles caused by loading and first drawing that scene. `total` runs from
the moment the scene is current to the end of the run, so it includes the warm-up
and is normally non-zero even in a valid cell; the steady-state signal is
`timing.pso_in_sample`, which counts only the sampling window and must be zero.

## Scene checks

The harness does not reject a cell on geometry grounds; it reports the numbers
and the person collecting results applies the acceptance criteria from the
specification:

- **S1** — `primitives ≈ 12·box_count + 2` (±1%). `draw_calls` is
  renderer-dependent: Forward+ merges the identical boxes into one instanced
  draw (expect ≈ 2 draw calls regardless of `box_count`), while Mobile reports
  one draw per instance (≈ `box_count + 1`). S1 therefore checks stability
  across repeated runs, not a fixed draw-call target.
- **S2** — `draw_calls` must stay constant (±1) across the whole
  `instance_count` sweep, and `primitives` must scale linearly (±1%). Draw calls
  that grow with the instance count mean the MultiMesh is misconfigured.
- **S3a** — `timing.pso_in_sample` must be zero and every
  `frames.pso_compilations` entry must be zero, even at `material_count = 64`.
  `draw_calls` follows the same renderer rule as S1: with
  one material the 64 identical spheres merge into one instanced draw on
  Forward+, while 16 or 64 distinct materials cannot merge. Compare runs, not a
  fixed number.
- **S3b** — `pipeline_compilations.first_120` must be recorded exactly and be
  identical across repeated runs of the same configuration. The per-counter
  breakdown says which compile path moved; do not assume a feature maps to a
  specific counter.
- **S4** — `draw_calls` should not change across the (0,0), (8,0), (0,32),
  (8,32), (32,64) light sweep of one configuration. Record what actually
  happens; a change indicates a non-clustered fallback and must be documented.
  The directional shadow atlas size and soft-shadow quality are in
  `project_settings`, and `scene.parameters.shadowed_spots` states how many
  spots cast shadows.
- **S6** — each preset must change the image. Save P0 with `--bench-screenshot`,
  then run P1–P5 with `--bench-reference` and check
  `image_diff.mean_abs_diff`. A preset whose diff is ~0 did not run; a diff of
  exactly 0 with `--bench-min-diff` set makes the report invalid. A preset that
  asks for an effect the active renderer does not provide (all three on Mobile;
  SSIL and volumetric fog on Compatibility) is marked invalid with the effect
  named, and the cell is published as not applicable — never as 0 ms. `scene.parameters.post.support`
  records what the renderer reported.

All four scenes must show no increase in the PSO counters during sampling and
the same `frame_hash` for repeated runs of one configuration.

## Validity

A report is written even when the run is suspect; `"valid"` and
`"invalid_reason"` state why. Exit code is `0` for a valid report, `3` for an
invalid one, `2` when the harness cannot start, and `4` when the report cannot
be written. A cell is invalid when:

- the build is a tools build (unless explicitly allowed, and then still invalid);
- the process runs under `--headless` (the harness refuses to start, exit code 2);
- the display server reports a vsync mode other than disabled;
- pipeline compilations occur during the sampling window or within 120 frames
  before it;
- the frame hash is missing because capture failed;
- the report shows a GPU time that the backend never produced (the harness
  reports an empty GPU array and `null` summary instead of zero);
- `--bench-preset Q1` is passed to a scene that does not apply Q1 settings. The
  S1–S4 scenes pin Q0, so the report is refused instead of being published as a
  Q1 frame that is actually Q0.

Frame-hash equality across runs is **not** checked by the harness; the checker
compares it when several reports of the same cell are passed together.

## Checking reports

`benchmarks/tools/check_report.py` implements the gate in code:

```text
python3 benchmarks/tools/check_report.py runs/*.json
python3 benchmarks/tools/check_report.py benchmarks/results/2026-11-19_s1_d1_q0/*.json.gz
python3 benchmarks/tools/check_report.py benchmarks/results/2026-11-19_d1/*/*.json.gz
```

Reports may be gzip-compressed; the checker reads `.json` and `.json.gz` alike.
Published cells are committed gzipped because one full report is about 540 KB
uncompressed and about 23 KB compressed.

It checks that a report came from a template build with a rendering surface, that
VSync was disabled, that no pipeline compilation happened in the sampling window
or within 120 frames before it, that the frame arrays match the declared window,
and that the viewport and project settings agree. Reports with the same scene,
scene parameters, preset, configuration, runtime and content hash are treated as
repetitions: differing `frame_hash` values, draw-call medians, or (for S3b)
`first_120` compilations fail the cell. On S6, a P1–P5 cell whose
`image_diff.mean_abs_diff` is exactly zero fails, because the effect demonstrably
did not run; the preset is read from `scene.parameters.post.preset`, not from the
label, and a label that disagrees with the applied preset is reported as a
warning.

Exit code is `0` when everything passes, `1` when a check fails, and `2` when an
input cannot be read. The tests run with
`python3 benchmarks/tools/test_check_report.py`.

The script never runs the engine and cannot replace reading the report: it
cannot detect a driver-forced swap interval from a flat histogram, thermal
throttling, or a scene that is not the one the label claims.

## Limitations

- `frame_us` comes from a script callback timer, not the engine's internal frame
  profiler. It measures the interval the main loop observes, which is what a
  game experiences, but it cannot separate submission from presentation.
- The harness itself performs ~15 monitor reads per frame. That cost is identical
  in every configuration and is disclosed, not subtracted.
- S3b builds its normal and ORM textures on the CPU at startup (512², smooth
  analytic patterns). That is content setup, not sampling work, and it is
  identical in every configuration.
- The S6 image diff is a GDScript byte loop over a full frame. It runs once,
  after sampling, and is not part of any timing number; on 1920×1080 it may take
  several seconds.
- GPU timestamps are unavailable on some backends (WebGL 2, and Metal without
  barrier mode and stage-boundary counter sampling). Empty means unavailable.
- Nothing here validates image quality. S1–S4 have no post-processing; S6 proves
  that an effect changed the frame, not that it looks right. Parity and quality
  comparisons need scenes and tooling that are not written yet.

## Related documents

- [Benchmark implementation report](../docs/architecture/rendering-benchmark-implementation.md)
- [Benchmark scene specification](../docs/architecture/rendering-benchmark-scenes.md)
- [Benchmark methodology](../docs/architecture/rendering-benchmarks.md)
- [Rendering audit (R0)](../docs/architecture/rendering-audit.md)
