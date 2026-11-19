# Benchmark Run Log

> **Status:** Smoke validation only — **not a baseline**, no publishable number
> **Engine:** Kaizen `4.8.beta.custom_build.92482158f`, `target=editor`
> **Date:** 2026-11-19

This document records executions of the benchmark project. It exists so that a
run is never described from memory: what was run, on what, and what the report
said. Every number here is a smoke-run observation on one machine. None of it is
a published cell, because the runs break the validity gate on purpose: they come
from a tools build and from a window the compositor clamped below the pinned
resolution.

The contract is in [Benchmark scene specification](rendering-benchmark-scenes.md)
and [Benchmark methodology](rendering-benchmarks.md). The code is in
[benchmark projects](../../benchmarks/README.md).

## 1. What was executed

```text
<kaizen-editor> --path benchmarks/rd_3d -- \
    --bench-scene <S1|S2|S3|S4|S6> --bench-warmup 300 --bench-frames 300 \
    --bench-config D1 --bench-allow-editor \
    --bench-out /tmp/opencode/kz-run/<cell>.json
```

- Warm-up 300 and sampling 300 frames: the shortest legal window, chosen to keep
  the smoke run short. The protocol's 900/3600 was not used.
- `--bench-allow-editor` because no export template exists in this environment.
  The harness marks every report `valid = false` with that reason.
- S6 was run at `--bench-count 0` and `--bench-count 4`, with `--bench-preset`
  from `P0` to `P5`.

## 2. Environment

| Item | Value |
|---|---|
| CPU threads | 24 |
| GPU | AMD Radeon RX 550 / 550 Series (RADV POLARIS12) |
| Vulkan | 1.4.354, Forward+ |
| Mesa | 26.2.5 |
| Display server | X11 on a Wayland session (XWayland) |
| Requested window | 1920×1080 (project setting) |
| Actual window | **1600×834** — the compositor clamped it, and `--resolution 1920x1080` did not win |
| VSync | 0 (project setting, reported by the harness) |

The clamped window is why these runs cannot be published: the frame was rendered
at another resolution than the one the baseline pins. `benchmarks/tools/check_report.py`
fails a window-size mismatch for exactly this reason.

## 3. Observed values

300 sampled frames, one run each, RADV/Forward+, 1600×834. `gpu` and `cpu` are
`summary.gpu_ms_median` and `summary.render_cpu_ms_median`; the FPS column is the
median wall-clock frame rate with presentation blocking included.

| Scene | Parameters | Draw calls | Primitives | FPS | GPU ms | CPU ms | `first_120` compiles |
|---|---|---|---|---|---|---|---|
| S1 | 400 boxes | 7 (constant) | 4802 | 707 | 1.23 | 0.12 | 5 |
| S2 | 4096 instances | 1 (constant) | 4,456,448 | 166 | 5.80 | 0.07 | 5 |
| S3a | 64 materials | 64 (constant) | 69,632 | 1292 | 0.59 | 0.10 | 5 |
| S3b | 8 feature materials | 16 (constant) | 69,632 | 1241 | 0.62 | 0.09 | 19 |
| S4 | 8 spots + 8 omnis | **11–15** | 2350 | 364 | 2.54 | 0.15 | 7 |
| S6 P0 | 4 quads | 3 (constant) | 394 | 623 | 1.42 | 0.08 | 7 |
| S6 P2 | 4 quads | 3 (constant) | 394 | 372 | 2.49 | 0.11 | 8 |

`first_120` is the per-source pipeline-compilation delta for the first 120 frames
after the scene starts loading. `timing.pso_in_sample` was 0 in every run, and
`last_compile_frame` was -1 in every run: the compiles happened during scene
load, before frame 0.

S6 with `--bench-count 0`, GPU median only:

| Preset | Effects | GPU ms |
|---|---|---|
| P0 | none | 1.42 |
| P1 | glow | 3.14 |
| P2 | SSAO | 2.49 |
| P3 | SSAO, SSIL | 5.54 |
| P4 | glow, SSAO, SSIL | 7.25 |
| P5 | glow, SSAO, SSIL, volumetric fog | 8.06 |

Image difference against the P0 screenshot of the same run, over every RGBA byte
(0–255):

| Preset | Mean abs diff | Max | Pixels changed |
|---|---|---|---|
| P1 | 3.055 | 37 | 100% |
| P2 | 0.0011 | 1 | 0.35% |
| P3 | 0.0049 | 5 | 1.34% |
| P4 | 3.051 | 37 | 99.99% |
| P5 | 17.24 | 87 | 94.36% |

## 4. What the runs established

- **S1 is deterministic.** Two independent runs of S1/400/D1/Q0 produced the
  same `frame_hash` (`c150718e…`) and the same draw-call median; the GPU median
  spread between them was 0.04%. A third run with the full protocol window
  (900 warm-up, 3600 sampled) reproduced the same frame hash, the same 7 draw
  calls and the same 4802 primitives. The camera-by-frame-index contract holds
  when the engine actually renders.
- **A live desktop is not a measurement environment.** The full-protocol S1 run
  had a 1.4 ms median frame but 737 of 3600 frames above twice the median, a
  11.8 ms p99 and an 18.6 ms worst frame. Nothing in the scene changed; the
  presentation path on a composited desktop does. This is why the gate requires
  a controlled presentation setup and why these runs are not published.
- **Every scene builds its content from code and runs.** No binary asset is
  needed, and no scene failed to expose `apply_frame` or `scene_parameters`.
- **The S6 support map matches the renderer.** Forward+ reports support for all
  four effects; the Mobile refusal path was not exercised (no Mobile run here).
- **`--bench-min-diff` works as a gate.** S6 P2 against P0 at 0.5 marked the run
  invalid with the measured value in the reason, which is the intended behaviour.

## 5. Findings that changed the code or the documents

1. **The project was named `project.godot`.** This engine loads `project.kaizen`
   (`core/config/project_settings.cpp:709`), so the project was invisible and the
   engine opened the project manager instead. Renamed; every reference updated.
   Nothing before this run had ever loaded the project.
2. **`Performance.TIME_PROCESS` and `TIME_PHYSICS_PROCESS` are unusable in a
   benchmark.** `main/main.cpp:5186` publishes them as the maximum over a
   1,000,000-frame window, so in a 600-frame run they hold the startup frame:
   every sample read 104.281 ms. They were removed; the renderer's own
   `viewport_cpu_ms` is published instead.
3. **The window can be smaller than the pinned viewport.** A compositor clamp
   silently changes the rendered resolution, and `--resolution` does not override
   it. The report records the real size, and the checker now fails a mismatch
   instead of warning.
4. **Forward+ auto-batching is not "one draw for all identical surfaces".**
   S1 at 1/64/65/128/256/400 boxes produced 2/5/4/5/6/7 draw calls. The count is
   non-monotonic because batching only merges *adjacent* render-list elements and
   the sort order changes with the camera. No fixed expected count is valid; only
   stability across runs is.
5. **S4 draw calls change within one run (11–15).** The cell was 8 spots + 8
   omnis with 64 boxes and a moving camera, and the count varies because the
   batching of shadowed and lit surfaces changes as the view moves. The
   specification's across-cell comparison (same camera path, different light
   counts) was **not** run, so this does not say whether light count changes the
   count; it says only that a within-run constant is not a property of S4.
6. **SSAO barely changes this image.** It modulates the ambient term, and the S6
   base scene lights everything from a sun with no shadows, so P2's mean
   difference is 0.0011 even though SSAO costs about 1 ms. "Exactly zero means the
   effect did not run" is the correct gate; a fixed 0.5 threshold would be
   satisfied only by glow and fog, and per-preset thresholds belong to the
   collector, not to the specification.

## 6. Still missing before any number is published

- An export template. The tools build is not representative: the editor forces
  `separate_thread_render = 0` and carries editor-only work.
- The pinned 1920×1080 surface, or a documented decision to pin another size.
- The full 900/3600 window and five repetitions per cell.
- Driver-forced VSync, thermal state, and content-on-screen verification, which
  remain manual.
