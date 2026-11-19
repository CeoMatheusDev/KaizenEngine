# S6 / D1 / quad_count 4 — post presets P0–P5, 2026-11-19

> **Status:** 6 cells (P0–P5), each with one cold run and five warm runs,
> all passing `benchmarks/tools/check_report.py` with zero failures.
> Every runtime number below is read from the committed reports.

- **Scene:** S6 with `quad_count = 4`: four 40×40 m transparent quads
  (alpha 0.35) at z = 10, 11, 12 and 13 m, a directional light without
  shadows, 32 opaque boxes behind the quads, a checker ground, and a static
  camera. The quads are the overdraw source; each covers most of the
  viewport, so the count is the overdraw multiplier.
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
    --bench-scene S6 --bench-count 4 --bench-config D1 --bench-preset P0 \
    --bench-warmup 900 --bench-frames 3600 \
    --bench-screenshot <abs>/p0/frame_299.png --bench-out <abs>/p0/cold.json

# 2. every other run diffs its captured frame against that PNG
<template> --path benchmarks/rd_3d --screen 1 --fullscreen -- \
    --bench-scene S6 --bench-count 4 --bench-config D1 --bench-preset P<n> \
    --bench-warmup 900 --bench-frames 3600 \
    --bench-reference <abs>/p0/frame_299.png --bench-out <abs>/p<n>/r<n>.json
```

Cold runs were started after deleting
`user://vulkan/pipelines.forward_plus.*.cache`. No `--bench-min-diff` was
passed: the gate requires a non-zero difference, not a per-effect
threshold.

## Cells

| Preset | Effects | Image diff | FPS | GPU ms | Δ GPU ms | CPU ms | Draws | Primitives | Load ms |
|---|---|---|---|---|---|---|---|---|---|
| P0 | none | 0.0000 | 338.9–340.4 | 2.212–2.214 | reference | 0.074–0.077 | 3 | 394 | 3497–3533 |
| P1 | glow | 3.0142 | 177.6–178.7 | 4.856–4.857 | +2.643 | 0.081–0.096 | 3 | 394 | 5874–5887 |
| P2 | SSAO | 0.0057 | 218.5–219.5 | 3.824–3.825 | +1.612 | 0.098–0.111 | 3 | 394 | 4943–4959 |
| P3 | SSAO, SSIL | 0.0095 | 103.9–104.1 | 8.821–8.823 | +6.609 | 0.135–0.142 | 3 | 394 | 9474–9570 |
| P4 | glow, SSAO, SSIL | 3.0124 | 81.3–81.5 | 11.463–11.466 | +9.251 | 0.138–0.157 | 3 | 394 | 11873–11901 |
| P5 | glow, SSAO, SSIL, volumetric fog | 16.2981 | 75.3–75.4 | 12.433–12.437 | +10.223 | 0.153–0.160 | 3 | 394 | 12742–12773 |

The image diff is the mean absolute difference over all RGBA bytes of the
captured frame against the P0 reference, computed once after sampling. P0
is compared with its own reference, so its diff is zero by construction;
for P1–P5 a diff of exactly zero fails the checker, because it would mean
the effect did not run.

`first_120` is 7 pipelines for P0 and P1, 8 for P2–P5 (3 specialization
plus 4 or 5 surface); `pso_in_sample` is 0 in all 36 reports, so no preset
compiles during sampling. Load to first sample ranges from 3.50–3.53 s (P0) to
12.742–12.773 s (P5): the post passes allocate their screen-space resources
before the first sampled frame.

Frame drift inside a run (first 300 against last 300 sampled frames)
stays between -0.42% and +0.54% across all six cells, so no cell shows
a monotonic slow-down.

## What each preset costs

- **P1 (glow):** 4.856–4.857 ms of GPU median, +2.643 ms over P0; image diff 3.0142.
- **P2 (SSAO):** 3.824–3.825 ms of GPU median, +1.612 ms over P0; image diff 0.0057.
- **P3 (SSAO, SSIL):** 8.821–8.823 ms of GPU median, +6.609 ms over P0; image diff 0.0095.
- **P4 (glow, SSAO, SSIL):** 11.463–11.466 ms of GPU median, +9.251 ms over P0; image diff 3.0124.
- **P5 (glow, SSAO, SSIL, volumetric fog):** 12.433–12.437 ms of GPU median, +10.223 ms over P0; image diff 16.2981.

- The presets are cumulative, so each delta is the cost of a stack, not of
  a single effect. Two measured values still line up: glow costs +2.643 ms
  over P0 and +2.642 ms when added to SSAO and SSIL in P4 (P4 − P3), so on
  this scene its cost does not depend on what the other effects wrote.
- SSIL is the largest step: P3 − P2 is +4.997 ms. Volumetric fog is the
  smallest: P5 − P4 is +0.972 ms.
- Image diff measures visibility, not cost. SSAO changes the frame by
  0.0057 for 1.6 ms, while volumetric fog changes it by 16.2981 for
  0.972 ms; both effects ran, and the checker only rejects a diff of
  exactly zero.
- Draw calls and primitives are identical across presets: post effects run
  after the geometry pass and do not change either counter.

## Non-zero observations

- No run in this session reported a hitch.
- `report.preset` equals `scene.parameters.post.preset` in every report,
  so the label and the applied preset agree.
- Every cell has one frame hash across its six runs, and one `first_120`
  breakdown: P0 `846b0424c65f`, P1 `621511dcbe3e`, P2 `84eaf040aee3`, P3 `ca9e38579c5e`, P4 `9ad92f997bbf`, P5 `db177245b433`.

## Checks that the report cannot make for you

- **VSync:** `runtime.vsync_mode` is 0 and no frame rate is a display
  refresh divisor; no swapchain counter was read.
- **Thermal throttling:** the drift inside each run is small; no GPU
  temperature sensor was read.
- **Content on screen:** `p0/frame_299.png` is the reference image. It was
  inspected: it shows the four nested translucent quads over the checker
  ground with the box field behind them. The other presets were not
  inspected by eye, only diffed against this image.

## Files

`p0..p5/` hold `cold.json.gz` and `r1..r5.json.gz`; `p0/frame_299.png` is the
reference image. Verify with:

```text
python3 benchmarks/tools/check_report.py benchmarks/results/2026-11-19_s6_d1_q4/*/*.json.gz
```

`checker.txt` is that command's output.

## Scope

One overdraw level (4 quads), one configuration (D1), one commit. The
other D1 overdraw levels are separate sessions under `benchmarks/results/`;
the D2, M1 and W2 configurations are not measured, and nothing here is a
claim about them or about a different backend.
