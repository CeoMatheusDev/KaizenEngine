# D2 session — Mobile renderer (2026-11-19)

> **Status:** 7 of 13 cells pass `benchmarks/tools/check_report.py` with zero
> failures; the remaining 6 cells (S6 P2 and P5 at 1, 4 and 16 quads) are
> **not applicable** on Mobile and are published as invalid with the
> unsupported effect named, never as 0 ms.

- **Configuration:** D2 — Linux x86_64, Vulkan, **Mobile**, RADV.
- **Engine:** `kaizen.linuxbsd.template_release.x86_64`, commit `43c45bf`.
- **Preset:** Q0 for every cell (the scenes do not implement Q1).
- **Window:** 1920×1080 fullscreen, VSync off, MSAA off, 3D scale 1.0.
- **Sampling:** 900 warm-up frames, 3600 sampled frames per run.

## Machine

| Item | Value |
|---|---|
| CPU | Intel Xeon E5-2680 v3 @ 2.50 GHz, 24 threads |
| GPU | AMD Radeon RX 550 / 550 Series (RADV POLARIS12) |
| Vulkan | 1.4.354 |
| Mesa | 26.2.5 |
| Kernel | 7.2.15-200.fc44.x86_64 |
| Display server | X11 (on a Wayland session), fullscreen 1920×1080 |
| Rendering method | mobile (`--rendering-method mobile`) |

## Cells (valid)

| Cell | FPS median | GPU ms | CPU ms | Draw calls | Primitives | Load ms | `first_120` | Hitches |
|---|---|---|---|---|---|---|---|---|
| `S2_D2_Q0` | 269.9–272.3 | 2.944–2.945 | 0.048–0.050 | 1 | 4 456 448 | 3444–3547 | 3 | 0 |
| `S3a_D2_Q0` | 878.7–884.2 | 0.411–0.412 | 0.075–0.076 | 64 | 69 632 | 1163–1264 | 3 | 0 |
| `S3b_D2_Q0` | 867.3–876.4 | 0.423–0.424 | 0.070–0.075 | 16 | 69 632 | 1245–2085 | 17 | 2 |
| `S4_D2_Q0` | 210.7–211.0 | 4.017 | 0.142–0.146 | 66–130 | 1 870 | 4391–4508 | 6 | 0 |
| `S6_1q_P0` | 644.3–648.1 | 0.824 | 0.059–0.060 | 34 | 388 | 1547–1626 | 5 | 0 |
| `S6_4q_P0` | 604.2–607.9 | 0.922–0.924 | 0.060–0.062 | 37 | 394 | 1629–1722 | 5 | 0 |
| `S6_16q_P0` | 421.4–422.7 | 1.645 | 0.062–0.064 | 49 | 418 | 2268–2347 | 5 | 0 |

## Cells (not applicable on Mobile)

`S6_${q}q_P2` (SSAO) and `S6_${q}q_P5` (SSAO + SSIL + volumetric fog)
are marked invalid by the harness because Mobile does not provide those
effects: glow is the only post effect supported. The cell is published as
not applicable, never as 0 ms, per the specification.

## Notes

- Mobile reports one draw per instance for S1-class content; S6 reports 34–49
  draw calls because the quads and boxes are not batched into one instanced
  draw the way Forward+ does.
- `first_120` is the pipeline-creation count during scene load; `pso_in_sample`
  is 0 in every run.
- Hitches: only S3b reported 2 frames above twice the median across its six runs.

## Verification

```text
python3 benchmarks/tools/check_report.py benchmarks/results/2026-11-19_1252_d2_mobile/*/*.json
```

Exit code 0 for the valid cells; the 6 not-applicable cells exit 3 with the
unsupported effect named.