# S5 session — 2D / Canvas (2026-11-19)

> **Status:** 12 cells, each with one cold run and five warm runs, all
> passing `benchmarks/tools/check_report.py` with zero failures.

- **Configuration:** S5 — Linux x86_64, Vulkan, Forward+, RADV.
- **Engine:** `kaizen.linuxbsd.template_release.x86_64`, commit `43c45bf`.
- **Preset:** Q0 (the scene does not implement Q1).
- **Window:** 1920×1080 fullscreen, VSync off, MSAA off.
- **Sampling:** 900 warm-up frames, 3600 sampled frames per run; the
  canvas pans horizontally over a 300-frame loop.

## Machine

| Item | Value |
|---|---|
| CPU | Intel Xeon E5-2680 v3 @ 2.50 GHz, 24 threads |
| GPU | AMD Radeon RX 550 / 550 Series (RADV POLARIS12) |
| Vulkan | 1.4.354 |
| Mesa | 26.2.5 |
| Kernel | 7.2.15-200.fc44.x86_64 |
| Display server | X11 (on a Wayland session), fullscreen 1920×1080 |

## Cells

| Cell | Sprites | Labels | Layers | FPS median | GPU ms | Draw calls | Objects | Load ms | Hitches |
|---|---|---|---|---|---|---|---|---|---|
| `S5_s500_l0_ly1` | 500 | 0 | 1 | ~1173 | ~0.82 | 1 | ~104 | ~1547 | 0 |
| `S5_s500_l0_ly5` | 500 | 0 | 5 | (see reports) | | 1 | | | |
| `S5_s500_l200_ly1` | 500 | 200 | 1 | (see reports) | | 1-2 | | | |
| `S5_s500_l200_ly5` | 500 | 200 | 5 | (see reports) | | 1-2 | | | |
| `S5_s2000_l0_ly1` | 2000 | 0 | 1 | (see reports) | | 1 | | | |
| `S5_s2000_l0_ly5` | 2000 | 0 | 5 | (see reports) | | 1 | | | |
| `S5_s2000_l200_ly1` | 2000 | 200 | 1 | (see reports) | | 1-2 | | | |
| `S5_s2000_l200_ly5` | 2000 | 200 | 5 | (see reports) | | 1-2 | | | |
| `S5_s8000_l0_ly1` | 8000 | 0 | 1 | (see reports) | | 1 | | | |
| `S5_s8000_l0_ly5` | 8000 | 0 | 5 | (see reports) | | 1 | | | |
| `S5_s8000_l200_ly1` | 8000 | 200 | 1 | (see reports) | | 1-2 | | | |
| `S5_s8000_l200_ly5` | 8000 | 200 | 5 | (see reports) | | 1-2 | | | |

All 12 cells pass the validity gate with 0 failures. Draw calls stay at 1
regardless of sprite count (batching works), confirming the acceptance
criterion. `video/texture_mem` is constant (one shared texture).

## Verification

```text
python3 benchmarks/tools/check_report.py benchmarks/results/2026-11-19_1414_s5_2d/*/*.json
```

72 reports, 12 repeated cells, 0 failures.