# Rendering benchmarks

Executable benchmark projects for the Kaizen rendering baseline. The measurement
contract is defined in [Benchmark scene specification](../docs/architecture/rendering-benchmark-scenes.md);
the instrument list and reference configurations are in
[Benchmark methodology](../docs/architecture/rendering-benchmarks.md).

> **Status:** scene **S1 is authored; S2–S7 are not**. The harness has never been
> executed in this environment (no build, GPU, or display). Treat every number it
> produces as unvalidated until it has been run on real hardware and checked
> against the validity rules below.

## Layout

```text
benchmarks/
  s1_static/
    project.godot
    scenes/s1_static.tscn
    scripts/bench_harness.gd   autoload: sampling, monitors, JSON report
    scripts/s1_static.gd       deterministic content + frame-indexed camera
```

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
<kaizen-template> --path benchmarks/s1_static -- \
    --bench-scene S1 --bench-count 400 --bench-preset Q0 \
    --bench-frames 3600 --bench-warmup 900 \
    --bench-out /absolute/path/S1_D1_Q0.json
```

Without `--bench-out` the report goes to `user://bench/<scene>_<preset>.json`,
and its absolute path is printed on exit.

For scene development only, a tools build can run the same command with
`--bench-allow-editor`; the JSON then contains `"valid": false` and the reason.

### Arguments

| Argument | Default | Meaning |
|---|---|---|
| `--bench-scene` | `S1` | Report label |
| `--bench-count` | scene default (400) | Forwarded to the scene as its content count |
| `--bench-preset` | `Q0` | Report label only; presets are project settings |
| `--bench-frames` | `3600` | Sampled frames, rounded up to a multiple of 300 |
| `--bench-warmup` | `900` | Frames before sampling, rounded up to a multiple of 300 |
| `--bench-out` | `user://bench/...` | Absolute report path |
| `--bench-no-hash` | off | Skip the frame-hash capture |
| `--bench-allow-editor` | off | Allow a tools build (report marked invalid) |

Sampling always starts at a fixed frame index (default 900) and the reported
window is exactly 3600 frames, so every run covers the same camera frames.

### Renderer selection

The project pins Forward+, but the CLI overrides it per run:

```text
<kaizen-template> --path benchmarks/s1_static --rendering-method mobile \
    --rendering-driver metal -- --bench-scene S1 --bench-count 400 ...
```

The report records the **effective** method, driver, adapter name, adapter
vendor, and API version as reported by `RenderingServer`, so a run started with
an override cannot be mistaken for the project setting.

## What the harness records

One row per sampled frame:

- `frame_us`: wall-clock interval between consecutive `_process` callbacks. This
  includes presentation blocking; it is not a pure CPU measurement.
- `process_ms`, `physics_ms`: `Performance` monitors.
- `draw_calls`, `objects`, `primitives`.
- `texture_mem`, `buffer_mem`, `video_mem`.
- `pso_compilations`: **per-frame delta** of the cumulative
  `Performance.PIPELINE_COMPILATIONS_*` counters. Those counters never return to
  zero, so a delta is the only meaningful steady-state signal.
- `viewport_cpu_ms`, `viewport_gpu_ms`: from
  `RenderingServer.viewport_set_measure_render_time`.

Monitor values are read once per frame, at the start of frame *n*. Draw-call and
memory monitors describe the frame that was just rendered (*n − 1*); the process
and physics timers describe the most recent main-loop step. The camera for frame
*n* is applied before that frame is rendered.

At the end the harness writes a single JSON document with the frame arrays, a
summary (median / p95 / p99 / worst / hitches), the engine version and hash, the
pinned project settings, the scene parameters, a content hash, and a frame hash
of rendered frame 299.

## Validity

A report is written even when the run is suspect; `"valid"` and
`"invalid_reason"` state why. Exit code is `0` for a valid report, `3` for an
invalid one, and `2` when the harness cannot start. A cell is invalid when:

- the build is a tools build (unless explicitly allowed, and then still invalid);
- pipeline compilations occur during the sampling window or within 120 frames
  before it;
- the frame hash is missing because capture failed;
- the report shows a GPU time that the backend never produced (the harness
  reports an empty GPU array and `null` summary instead of zero).

Frame-hash equality across runs is **not** checked by the harness; compare
`frame_hash` fields when collecting results.

## Limitations

- `frame_us` comes from a script callback timer, not the engine's internal frame
  profiler. It measures the interval the main loop observes, which is what a
  game experiences, but it cannot separate submission from presentation.
- The harness itself reads ~13 monitors per frame. That cost is identical in
  every configuration and is disclosed, not subtracted.
- GPU timestamps are unavailable on some backends (WebGL 2, and Metal without
  barrier mode and stage-boundary counter sampling). Empty means unavailable.
- Nothing here validates image quality. S1 has no post-processing; parity and
  quality comparisons need the scenes that are not authored yet.

## Related documents

- [Benchmark scene specification](../docs/architecture/rendering-benchmark-scenes.md)
- [Benchmark methodology](../docs/architecture/rendering-benchmarks.md)
- [Rendering audit (R0)](../docs/architecture/rendering-audit.md)
