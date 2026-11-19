# S1 / D1 / Q0 — 400 boxes, 2026-11-19

> **Status:** First published reference cell. Six reports (five warm runs and one
> cold run) pass `benchmarks/tools/check_report.py` with zero failures. The
> reports in this directory are the evidence; the summary below adds nothing
> that is not in them.

- **Scene:** S1, `box_count = 400` (20×20 grid, 2×2×2 m, spacing 8 m), ground
  plane, one checker material.
- **Configuration:** D1 — Linux, Vulkan, Forward+.
- **Preset:** Q0 (project settings, no overrides).
- **Window:** 1920×1080 fullscreen, VSync off, MSAA off, 3D scale 1.0, TAA off,
  depth prepass on.
- **Engine:** `kaizen.linuxbsd.template_release.x86_64`,
  `4.8.beta.custom_build.43c45bf02` (commit `43c45bf`), built with
  `scons platform=linuxbsd target=template_release -j24 wayland=no alsa=no
  pulseaudio=no speechd=no udev=no dbus=no disable_path_overrides=no`.
- **Content hash:** `32288e115d97…` (full value in each report).
- **Frame hash:** `afeb8045b4b4…` — identical in all six runs.

## Machine

| Item | Value |
|---|---|
| CPU | Intel Xeon E5-2680 v3 @ 2.50 GHz, 24 threads (2×12) |
| GPU | AMD Radeon RX 550 / 550 Series (RADV POLARIS12) |
| Vulkan | 1.4.354 |
| Mesa | 26.2.5 |
| Kernel | 7.2.15-200.fc44.x86_64 |
| Display server | X11 (on a Wayland session), fullscreen on the 1920×1080 output |

The machine was idle: no build, no other benchmark, no virtual machine.

## Reproduce

```text
<template> --path benchmarks/rd_3d --screen 1 --fullscreen -- \
    --bench-scene S1 --bench-count 400 --bench-config D1 --bench-preset Q0 \
    --bench-warmup 900 --bench-frames 3600 \
    --bench-out <abs>/S1_D1_Q0_r1.json
```

Fullscreen is not cosmetic. The desktop here is 1600×900 plus a 1920×1080
output; a windowed 1920×1080 run loses client height to the window manager
(measured 1600×834 when clamped to the primary display, 1920×1052 when the
window is moved to the 1080p output). Fullscreen on the 1080p output gives the
pinned size exactly.

## Warm runs

Each run samples 3600 frames after 900 warm-up frames.

| Run | FPS (median) | Frame ms | p95 ms | p99 ms | Worst ms | GPU ms | CPU ms | Load ms |
|---|---|---|---|---|---|---|---|---|
| r1 | 380.2 | 2.630 | 2.770 | 2.955 | 3.457 | 1.894 | 0.116 | 2605.0 |
| r2 | 380.5 | 2.628 | 2.788 | 2.933 | 3.247 | 1.895 | 0.116 | 2511.0 |
| r3 | 379.0 | 2.639 | 2.760 | 2.893 | 3.260 | 1.895 | 0.115 | 2518.2 |
| r4 | 380.1 | 2.631 | 2.796 | 2.982 | 3.343 | 1.895 | 0.115 | 2517.3 |
| r5 | 379.9 | 2.632 | 2.770 | 2.926 | 3.209 | 1.895 | 0.115 | 2513.6 |

- FPS range 379.0–380.5 (0.4%), GPU median range 1.894–1.895 ms (0.05%).
- `hitches` (frames above twice the median) = 0 in every run.
- `draw_calls` = 7, `objects` = 401, `primitives` = 4802 in all 3600 sampled
  frames of all five runs.
- `pso_in_sample` = 0 and `last_compile_frame` = -1 in every run.
- `pipeline_compilations.first_120` = 5 (2 specialization, 3 surface) in every
  run, identical to `timing.pso_at_start`: the compiles happen while the scene
  loads, before frame 0.
- `load_to_sample_ms` is the wall time from the scene-load call to the first
  sampled frame. It includes the 900 warm-up frames, which is why it is ~2.5 s
  and why r1 is 94 ms slower than the rest: that is process/startup noise, not
  a per-frame difference.
- First-300 vs last-300 frame-median drift: -0.11% to +0.21% across the five
  runs, so there is no monotonic slow-down inside a run.

## Cold run

`S1_D1_Q0_cold.json.gz` was produced after deleting
`user://vulkan/pipelines.forward_plus.*.cache` (1,256,676 bytes); the run
recreated it at 716,464 bytes.

| Run | FPS (median) | GPU ms | Load ms | `first_120` compiles |
|---|---|---|---|---|
| cold | 380.8 | 1.894 | 2509.4 | 5 |

Deleting the cache did not change the compile count or the load-to-sample time
for this scene. The report records what it measures: the scene requests five
pipelines and pays for them on every run, and the 900-frame warm-up dominates
the 2.5 s. A pipeline-cache benefit is not visible in these two fields for S1;
that is a property of the scene, not a claim that the cache does nothing.

## Checks that the report cannot make for you

The automated gate (`check_report.py`) covers the frame hash, the pipeline
counters, the window size, the build type, and the GPU/CPU timing consistency.
Three checks stay manual:

- **VSync:** `runtime.vsync_mode` is 0, the median is 380 FPS (not a 60/120/144
  Hz divisor), and the frame-time p99 is 2.98 ms. No driver-level override is
  configured on this machine, but no swapchain counter was read to prove it.
- **Thermal throttling:** the drift above is within ±0.21%, which rules out a
  monotonic rise; no GPU temperature sensor was read.
- **Content on screen:** `frame_299.png` was inspected: checkerboard ground and
  the 20×20 box grid, no UI, no overlay, 1920×1080.

## Files

The six reports are committed gzipped (`S1_D1_Q0_*.json.gz`) so one cell costs
about 130 KB instead of 3.2 MB. `check_report.py` reads gzip transparently:

```text
python3 benchmarks/tools/check_report.py benchmarks/results/2026-11-19_s1_d1_q0/*.json.gz
```

`checker.txt` is that command's output, and `frame_299.png` is the frame the
screenshot check looked at.

## Scope

One cell on one machine. It is a reference for this GPU and this commit, not a
portable performance claim, and it says nothing about S2–S6, Mobile,
Compatibility, Metal, Web, or another CPU.
