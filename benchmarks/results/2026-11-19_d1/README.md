# D1 session — S2, S3a, S3b, S4 (2026-11-19)

> **Status:** 9 cells, each with one cold run and five warm runs, all
> passing `benchmarks/tools/check_report.py` with zero failures.
> Every runtime number below is read from the committed reports; the
> machine record and the manual checks are the only additions.

- **Configuration:** D1 — Linux x86_64, Vulkan, Forward+, RADV.
- **Engine:** `kaizen.linuxbsd.template_release.x86_64`, commit `43c45bf`.
- **Preset:** Q0 for every cell (the scenes do not implement Q1).
- **Window:** 1920×1080 fullscreen, VSync off, MSAA off, 3D scale 1.0,
  depth prepass on.
- **Sampling:** 900 warm-up frames, 3600 sampled frames per run; the
  camera is a 300-frame loop, so every run covers the same frames.

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
    --bench-scene <S2|S3|S4> --bench-config D1 --bench-preset Q0 \
    --bench-warmup 900 --bench-frames 3600 \
    --bench-out <abs>/<cell>/r<n>.json
```

Scene-specific arguments:

- S2: `--bench-scene S2 --bench-count 4096`.
- S3a: `--bench-scene S3 --bench-count 64 --bench-variant a`.
- S3b: `--bench-scene S3 --bench-count 64 --bench-variant b`.
- S4: `--bench-scene S4 --bench-spots <n> --bench-omnis <n>`, plus
  `--bench-variant shadows` for the shadowed-spot cell.

The cold run is the first run of a cell, started after deleting
`user://vulkan/pipelines.forward_plus.*.cache`.

## Cells

| Cell | FPS median | GPU ms | CPU ms | Draw calls | Primitives | Load ms | `first_120` | Hitches |
|---|---|---|---|---|---|---|---|---|
| `s2_4096_q0` | 138.6–138.7 | 6.437–6.438 | 0.069–0.070 | 1 | 4 456 448 | 6622–6640 | 5 | 0/0/0/0/0/0 |
| `s3a_64_q0` | 638.6–641.4 | 0.835–0.836 | 0.099–0.102 | 64 | 69 632 | 1571–1587 | 5 | 0/0/0/0/0/0 |
| `s3b_64_q0` | 622.7–624.2 | 0.875–0.876 | 0.083–0.085 | 16 | 69 632 | 1667–1673 | 19 | 0/1/0/0/0/0 |
| `s4_00_q0` | 293.6–294.4 | 2.656–2.657 | 0.130–0.132 | 13 | 1 870 | 3210–3228 | 6 | 0/0/0/0/0/0 |
| `s4_80_q0` | 245.8–246.1 | 3.315 | 0.141–0.144 | 13 | 1 870 | 3801–3817 | 6 | 0/0/0/0/0/0 |
| `s4_032_q0` | 192.1–192.8 | 4.442–4.444 | 0.151–0.154 | 13 | 1 870 | 4821–4885 | 7 | 0/0/0/0/0/0 |
| `s4_832_q0` | 178.5–178.9 | 4.832–4.834 | 0.158–0.162 | 13 | 1 870 | 5163–5175 | 7 | 0/0/0/0/0/0 |
| `s4_3264_q0` | 119.6–120.0 | 7.551–7.560 | 0.186–0.197 | 13 | 1 870 | 7643–7656 | 7 | 0/0/0/0/0/0 |
| `s4_shadows_q0` | 241.3–242.8 | 3.377–3.378 | 0.140–0.146 | 13 | 1 870 | 3861–3870 | 6 | 0/0/0/0/0/0 |

`first_120` is the number of pipelines created while the scene loads; it
equals `timing.pso_at_start` in every report, and `pso_in_sample` is 0 in
every run. `Hitches` counts frames above twice the median, across all six
runs of the cell (cold first). Draw calls and primitives in this table are
per-frame medians; where the camera orbit changes culling they vary inside
a run, and the checker compares the medians across repetitions.

## S2 — 4096 instances

- **One draw call for 4096 instances.** `draw_calls` = 1 in every
  sampled frame of all six runs: a single `MultiMesh` is one draw, and
  the instance count never reaches the draw-call counter.
- The cost appears elsewhere: 4 456 448 primitives, 6.437–6.438 ms of GPU median,
  138.6–138.7 FPS. For comparison, S1 with 400 separate boxes reports 7 draw
  calls, 4 802 primitives, and 1.895 ms.
- Load to first sampled frame is 6.62–6.64 s, the longest of this session:
  the scene builds its 4096-instance `MultiMesh` before frame 0, and
  the figure still includes the 900 warm-up frames.
- Frame hash identical in all six runs.

## S3a and S3b — what material variety costs

| | S3a (64 materials) | S3b (8 features × 8 meshes) |
|---|---|---|
| Draw calls | 64 | 16 |
| Primitives | 69 632 | 69 632 |
| `first_120` pipelines | 5 | 19 |
| GPU ms median | 0.835–0.836 | 0.875–0.876 |
| FPS median | 638.6–641.4 | 622.7–624.2 |

- Both scenes draw the same 64 spheres with the same primitive count,
  so the difference is pipeline state, not geometry.
- S3b measures 16 draws for the same 64 nodes where S3a measures 64.
  With 8 shared feature sets instead of 64 distinct materials the
  renderer can batch more, so the draw count follows material count
  here, not node count.
- S3b creates 19 pipelines during load against 5 for S3a. Variety in
  shader features costs pipeline creation; it does not cost draws.
- The `first_120` breakdown is identical in every run of a cell, which
  is the S3b determinism check the checker enforces.

## S4 — lights and shadows

The scene always has one shadowed directional light over 64 boxes and the
ground; the cells add spot and omni lights on fixed rings. Delta is
against the no-extra-lights cell.

| Lights | FPS median | GPU ms | Δ GPU ms | Frame drift |
|---|---|---|---|---|
| 0 spots, 0 omnis, 0 shadowed | 293.6–294.4 | 2.656–2.657 | reference | -0.51–0.47 |
| 8 spots, 0 omnis, 0 shadowed | 245.8–246.1 | 3.315 | +0.658 | -0.21–0.39 |
| 0 spots, 32 omnis, 0 shadowed | 192.1–192.8 | 4.442–4.444 | +1.786 | -0.12–0.20 |
| 8 spots, 32 omnis, 0 shadowed | 178.5–178.9 | 4.832–4.834 | +2.176 | -0.11–-0.02 |
| 32 spots, 64 omnis, 0 shadowed | 119.6–120.0 | 7.551–7.560 | +4.895 | -0.20–0.21 |
| 8 spots, 0 omnis, 4 shadowed | 241.3–242.8 | 3.377–3.378 | +0.721 | -0.35–0.48 |

- Draw calls vary between 11 and 15 inside a run because the camera
  orbits the field and culling changes; the median is the same in all
  runs of a cell, which is what the checker compares.
- Extra lights cost GPU time without adding draw calls. Every cell
  reports the same 11–15 draws and 133–200 rendered objects, so the
  light count changes neither the geometry pass nor the object count.
- These six cells are the complete S4/Q0 sweep from the specification
  — (0,0), (8,0), (0,32), (8,32), (32,64) plus the shadow variant — for
  one configuration and one preset.

## Non-zero observations

- `s3b_64_q0`: hitches per run 0/1/0/0/0/0. A single frame above twice the median is
  recorded, not removed: the gate does not require zero hitches, and
  the frame hash and GPU medians are unaffected.
- Frame drift (first 300 vs last 300 sampled frames, in percent) stays
  between -0.89 and +0.48 for every cell, so no cell shows a monotonic
  slow-down inside a run.

## Checks that the report cannot make for you

- **VSync:** `runtime.vsync_mode` is 0 in every report and the frame rates
  are not 60/120/144 Hz divisors, but no swapchain counter was read.
- **Thermal throttling:** the drift above rules out a monotonic rise; no
  GPU temperature sensor was read.
- **Content on screen:** these cells have no screenshot. The frame hash
  proves the runs rendered identical frames; it does not prove the frame
  shows the intended scene. Only S1's cell was inspected visually.

## Files

Each cell directory holds `cold.json.gz` and `r1..r5.json.gz`. Reports are
gzipped because one full report is about 540 KB uncompressed. Verify with:

```text
python3 benchmarks/tools/check_report.py benchmarks/results/2026-11-19_d1/*/*.json.gz
```

`checker.txt` in this directory is that command's output.

## Scope

The Q1 guard in `bench_harness.gd` was added after these runs. It only
adds a new invalid path for `--bench-preset Q1`, which no cell here uses,
so it cannot change a Q0 report.

One machine, one commit, Q0 only. This session covers S2 at 4096
instances, S3a and S3b at 64 nodes, and the six S4 light cells. It says
nothing about other counts, other presets, S5, S6, S7, Mobile,
Compatibility, Metal, or Web.
