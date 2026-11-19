# S6 / D1 / quad_count 1 — post presets P0–P5, 2026-11-19

> **Status:** 6 cells (P0–P5), each with one cold run and five warm runs,
> all passing `benchmarks/tools/check_report.py` with zero failures.
> Every runtime number below is read from the committed reports.

- **Scene:** S6 with `quad_count = 1`: one 40×40 m transparent quad
  (alpha 0.35) at z = 10 m, a directional light without shadows, 32 opaque
  boxes behind it, a checker ground, and a static camera.
  It is the overdraw source: it covers most of the viewport, so the
  count is the overdraw multiplier.
- **Configuration:** D1 — Linux x86_64, Vulkan, Forward+, RADV.
- **Engine:** `kaizen.linuxbsd.template_release.x86_64`, commit `43c45bf`.
- **Window:** 1920×1080 fullscreen, VSync off, MSAA off, 3D scale 1.0.
- **Sampling:** 900 warm-up frames, 3600 sampled frames per run. The S6
  camera is static, so the image is the same on every frame of the loop.

## Machine

| Item | Value |
|---|---|
| CPU | Intel Xeon E5-2680 v3 @ 2.50 GHz, 24 threads (2×12) |
| GPU | AMD Radeon RX 550 / 550 Series (RADV POLARIS12) |
| Vulkan | 1.4.354 |
| Mesa | 26.2.5 |
| Kernel | 7.2.15-200.fc44.x86_64 |
| Display server | X11 (on a Wayland session), fullscreen on the 1920×1080 output |

The machine was idle during the batch.

## Reproduce

```text
# 1. P0 cold run, which also writes the reference frame
<template> --path benchmarks/rd_3d --screen 1 --fullscreen -- \
    --bench-scene S6 --bench-count 1 --bench-config D1 --bench-preset P0 \
    --bench-warmup 900 --bench-frames 3600 \
    --bench-screenshot <abs>/p0/frame_299.png --bench-out <abs>/p0/cold.json

# 2. every other run diffs its captured frame against that PNG
<template> --path benchmarks/rd_3d --screen 1 --fullscreen -- \
    --bench-scene S6 --bench-count 1 --bench-config D1 --bench-preset P<n> \
    --bench-warmup 900 --bench-frames 3600 \
    --bench-reference <abs>/p0/frame_299.png --bench-out <abs>/p<n>/r<n>.json
```

Cold runs were started after deleting
`user://vulkan/pipelines.forward_plus.*.cache`. No `--bench-min-diff` was
passed: the gate requires a non-zero difference for a preset that enables
an effect, not a per-effect threshold.

## Cells

| Preset | Effects | Image diff | FPS | GPU ms | Δ GPU ms | CPU ms | Draws | Primitives | Load ms |
|---|---|---|---|---|---|---|---|---|---|
| P0 | none | 0.0000 (self) | 383.0–385.1 | 1.865–1.867 | reference | 0.075–0.081 | 3 | 388 | 3192–3204 |
| P1 | glow | 2.9683 | 188.4–190.5 | 4.508–4.510 | +2.643 | 0.083–0.087 | 3 | 388 | 5564–5599 |
| P2 | SSAO | 0.0096 | 235.2–237.8 | 3.477–3.479 | +1.612 | 0.098–0.100 | 3 | 388 | 4631–4662 |
| P3 | SSAO, SSIL | 0.0263 | 107.9–108.1 | 8.474–8.477 | +6.609 | 0.136–0.140 | 3 | 388 | 9150–9167 |
| P4 | glow, SSAO, SSIL | 2.9497 | 83.5–84.0 | 11.117–11.119 | +9.252 | 0.142–0.159 | 3 | 388 | 11553–11565 |
| P5 | glow, SSAO, SSIL, volumetric fog | 18.7847 | 77.8–78.0 | 12.004–12.007 | +10.139 | 0.156–0.160 | 3 | 388 | 12351–12376 |

The image diff is the mean absolute difference over all RGBA bytes of the
captured frame against this session's P0 reference. P0 is compared with its
own reference, so its diff is zero by construction; for P1–P5 a diff of
exactly zero fails the checker, because it would mean the effect did not
run.

`first_120` is 7 for P0–P1, 8 for P2–P5;
`pso_in_sample` is 0 in all 36 reports, so no preset compiles during
sampling. Load to first sample ranges from 3.19–3.20 s (P0) to 12.35–12.38 s (P5).

Frame drift inside a run (first 300 against last 300 sampled frames)
stays between -0.17% and +0.32% across all cells, so no cell shows
a monotonic slow-down.

## What each preset costs

- **P1 (glow):** 4.508–4.510 ms of GPU median, +2.643 ms over P0; image diff 2.9683.
- **P2 (SSAO):** 3.477–3.479 ms of GPU median, +1.612 ms over P0; image diff 0.0096.
- **P3 (SSAO, SSIL):** 8.474–8.477 ms of GPU median, +6.609 ms over P0; image diff 0.0263.
- **P4 (glow, SSAO, SSIL):** 11.117–11.119 ms of GPU median, +9.252 ms over P0; image diff 2.9497.
- **P5 (glow, SSAO, SSIL, volumetric fog):** 12.004–12.007 ms of GPU median, +10.139 ms over P0; image diff 18.7847.

- The presets are cumulative, so each delta is the cost of a stack, not of
  a single effect.
- P3 − P2 (SSIL on top of SSAO) is +4.997 ms; P5 − P4 (volumetric fog) is
  +0.887 ms.
- Draw calls and primitives are identical across presets: post effects run
  after the geometry pass and do not change either counter.
- Image diff measures visibility, not cost; the checker only rejects a
  difference of exactly zero for a preset that enables an effect.

## Non-zero observations

- No run in this session reported a hitch.
- `report.preset` equals `scene.parameters.post.preset` in every report,
  so the label and the applied preset agree.
- Every cell has one frame hash across its six runs, and one `first_120`
  breakdown: P0 `ec0314fbb6da`, P1 `4632b7c2479e`, P2 `14929fd357ee`, P3 `c34bc10cc045`, P4 `5d82ee0cac04`, P5 `5b3d2671df51`.

## Checks that the report cannot make for you

- **VSync:** `runtime.vsync_mode` is 0 and no frame rate is a display
  refresh divisor; no swapchain counter was read.
- **Thermal throttling:** the drift inside each run is small; no GPU
  temperature sensor was read.
- **Content on screen:** `p0/frame_299.png` is the reference image and
  shows one translucent quad over the checker ground with the
  box field behind it. The other presets were diffed against it, not
  inspected by eye.

## Files

`p0..p5/` hold `cold.json.gz` and `r1..r5.json.gz`; `p0/frame_299.png` is the
reference image. Verify with:

```text
python3 benchmarks/tools/check_report.py benchmarks/results/2026-11-19_s6_d1_q1/*/*.json.gz
```

`checker.txt` is that command's output.

## Scope

One overdraw level (1 quad), one configuration (D1), one commit.
The other D1 overdraw levels are separate sessions under
`benchmarks/results/`; the D2, M1 and W2 configurations are not measured,
and nothing here is a claim about them or about a different backend.
