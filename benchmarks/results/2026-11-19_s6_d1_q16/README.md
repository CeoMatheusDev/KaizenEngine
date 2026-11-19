# S6 / D1 / quad_count 16 — post presets P0–P5, 2026-11-19

> **Status:** 6 cells (P0–P5), each with one cold run and five warm runs,
> all passing `benchmarks/tools/check_report.py` with zero failures.
> Every runtime number below is read from the committed reports.

- **Scene:** S6 with `quad_count = 16`: 16 40×40 m transparent quads
  (alpha 0.35) at z = 10 m to 25 m, a directional light without shadows,
  32 opaque boxes behind them, a checker ground, and a static camera.
  The quads are the overdraw source; each covers most of the viewport,
  so the count is the overdraw multiplier.
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
    --bench-scene S6 --bench-count 16 --bench-config D1 --bench-preset P0 \
    --bench-warmup 900 --bench-frames 3600 \
    --bench-screenshot <abs>/p0/frame_299.png --bench-out <abs>/p0/cold.json

# 2. every other run diffs its captured frame against that PNG
<template> --path benchmarks/rd_3d --screen 1 --fullscreen -- \
    --bench-scene S6 --bench-count 16 --bench-config D1 --bench-preset P<n> \
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
| P0 | none | 0.0000 (self) | 185.5–186.6 | 4.609–4.611 | reference | 0.074–0.078 | 3 | 418 | 5642–5684 |
| P1 | glow | 2.9926 | 124.1–124.7 | 7.253 | +2.643 | 0.085–0.090 | 3 | 418 | 8044–8057 |
| P2 | SSAO | 0.0045 | 142.5–143.1 | 6.219–6.220 | +1.610 | 0.099–0.104 | 3 | 418 | 7112–7132 |
| P3 | SSAO, SSIL | 0.0046 | 83.1–83.3 | 11.211–11.213 | +6.602 | 0.137–0.139 | 3 | 418 | 11641–11657 |
| P4 | glow, SSAO, SSIL | 2.9943 | 68.0–68.1 | 13.855–13.857 | +9.246 | 0.144–0.146 | 3 | 418 | 14045–14224 |
| P5 | glow, SSAO, SSIL, volumetric fog | 4.7839 | 61.7 | 15.356–15.358 | +10.748 | 0.158–0.168 | 3 | 418 | 15400–15423 |

The image diff is the mean absolute difference over all RGBA bytes of the
captured frame against this session's P0 reference. P0 is compared with its
own reference, so its diff is zero by construction; for P1–P5 a diff of
exactly zero fails the checker, because it would mean the effect did not
run.

`first_120` is 7 for P0–P1, 8 for P2–P5;
`pso_in_sample` is 0 in all 36 reports, so no preset compiles during
sampling. Load to first sample ranges from 5.64–5.68 s (P0) to 15.40–15.42 s (P5).

Frame drift inside a run (first 300 against last 300 sampled frames)
stays between -0.69% and +0.46% across all cells, so no cell shows
a monotonic slow-down.

## What each preset costs

- **P1 (glow):** 7.253 ms of GPU median, +2.643 ms over P0; image diff 2.9926.
- **P2 (SSAO):** 6.219–6.220 ms of GPU median, +1.610 ms over P0; image diff 0.0045.
- **P3 (SSAO, SSIL):** 11.211–11.213 ms of GPU median, +6.602 ms over P0; image diff 0.0046.
- **P4 (glow, SSAO, SSIL):** 13.855–13.857 ms of GPU median, +9.246 ms over P0; image diff 2.9943.
- **P5 (glow, SSAO, SSIL, volumetric fog):** 15.356–15.358 ms of GPU median, +10.748 ms over P0; image diff 4.7839.

- The presets are cumulative, so each delta is the cost of a stack, not of
  a single effect.
- P3 − P2 (SSIL on top of SSAO) is +4.992 ms; P5 − P4 (volumetric fog) is
  +1.502 ms.
- Draw calls and primitives are identical across presets: post effects run
  after the geometry pass and do not change either counter.
- Image diff measures visibility, not cost; the checker only rejects a
  difference of exactly zero for a preset that enables an effect.

## Non-zero observations

- No run in this session reported a hitch.
- `report.preset` equals `scene.parameters.post.preset` in every report,
  so the label and the applied preset agree.
- Every cell has one frame hash across its six runs, and one `first_120`
  breakdown: P0 `51026d936c65`, P1 `b6e0d179d1fe`, P2 `187338d50b7d`, P3 `8080ada6972e`, P4 `db7346728dd3`, P5 `7208f94a1916`.

## Checks that the report cannot make for you

- **VSync:** `runtime.vsync_mode` is 0 and no frame rate is a display
  refresh divisor; no swapchain counter was read.
- **Thermal throttling:** the drift inside each run is small; no GPU
  temperature sensor was read.
- **Content on screen:** `p0/frame_299.png` is the reference image and
  shows 16 nested translucent quads over the checker ground with the
  box field behind them. The other presets were diffed against it, not
  inspected by eye.

## Files

`p0..p5/` hold `cold.json.gz` and `r1..r5.json.gz`; `p0/frame_299.png` is the
reference image. Verify with:

```text
python3 benchmarks/tools/check_report.py benchmarks/results/2026-11-19_s6_d1_q16/*/*.json.gz
```

`checker.txt` is that command's output.

## Scope

One overdraw level (16 quads), one configuration (D1), one commit.
The other D1 overdraw levels are separate sessions under
`benchmarks/results/`; the D2, M1 and W2 configurations are not measured,
and nothing here is a claim about them or about a different backend.
