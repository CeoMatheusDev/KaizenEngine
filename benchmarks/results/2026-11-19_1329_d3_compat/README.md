# D3 session — Compatibility (OpenGL) renderer (2026-11-19)

> **Status:** 9 of 13 cells pass `benchmarks/tools/check_report.py` with zero
> failures; the remaining 3 cells (S6 P5 at 1, 4 and 16 quads) are
> **not applicable** on Compatibility and are published as invalid with the
> unsupported effect named (SSIL and volumetric fog).

- **Configuration:** D3 — Linux x86_64, OpenGL, **gl_compatibility**, radeonsi.
- **Engine:** `kaizen.linuxbsd.template_release.x86_64`, commit `43c45bf`.
- **Preset:** Q0 for every cell (the scenes do not implement Q1).
- **Window:** 1920×1080 fullscreen, VSync off, MSAA off, 3D scale 1.0.
- **Sampling:** 900 warm-up frames, 3600 sampled frames per run.
- **Cold run:** no Vulkan pipeline cache exists for OpenGL; the cold run is a
  normal run and `load_to_sample_ms` is still recorded.

## Machine

| Item | Value |
|---|---|
| CPU | Intel Xeon E5-2680 v3 @ 2.50 GHz, 24 threads |
| GPU | AMD Radeon RX 550 / 550 Series (radeonsi, polaris12, ACO) |
| OpenGL | 4.6 (Core Profile) Mesa 26.2.5 |
| Kernel | 7.2.15-200.fc44.x86_64 |
| Display server | X11 (on a Wayland session), fullscreen 1920×1080 |
| Rendering method | gl_compatibility (`--rendering-method gl_compatibility`) |

## Cells (valid)

| Cell | FPS median | GPU ms | CPU ms | Draw calls | Primitives | Load ms | `first_120` | Hitches |
|---|---|---|---|---|---|---|---|---|
| `S2_D3_Q0` | 220.7–223.0 | 4.232–4.233 | 0.104–0.109 | 1 | 4 456 448 | 4275–4393 | 0 | 0 |
| `S3a_D3_Q0` | 2298.9–2427.2 | 0.137 | 0.197–0.215 | 64 | 69 632 | 485–611 | 0 | 29 |
| `S3b_D3_Q0` | 2232.1–2288.3 | 0.145 | 0.210–0.226 | 16 | 69 632 | 677–2851 | 0 | 17 |
| `S4_D3_Q0` | 105.9–106.3 | 8.872–8.876 | 0.829–0.845 | 377–444 | 1 870 | 8584–8952 | 0 | 0 |
| `S6_1q_P0` | 1310.6–1314.1 | 0.580 | 0.151–0.164 | 34 | 388 | 829–932 | 0 | 8 |
| `S6_1q_P2` | 473.7–475.3 | 1.936 | 0.188–0.193 | 34 | 388 | 2744–2861 | 0 | 0 |
| `S6_4q_P0` | 1039.5–1042.8 | 0.780 | 0.158–0.168 | 37 | 394 | 1012–1114 | 0 | 0 |
| `S6_4q_P2` | 430.5–433.1 | 2.138–2.139 | 0.181–0.193 | 37 | 394 | 2934–3063 | 0 | 0 |
| `S6_16q_P0` | 426.6–429.2 | 2.149 | 0.173–0.178 | 49 | 418 | 2281–2381 | 0 | 0 |
| `S6_16q_P2` | 267.2–270.4 | 3.535 | 0.186–0.195 | 49 | 418 | 4233–4333 | 0 | 0 |

## Cells (not applicable on Compatibility)

`S6_${q}q_P5` (SSIL + volumetric fog) is marked invalid by the harness
because Compatibility provides SSAO but not SSIL or volumetric fog. The
cell is published as not applicable, never as 0 ms.

SSAO (`P2`) **does** run on Compatibility: it changes 49% of the pixels
and costs ~0.87 ms more than P0 (measured in the D1 session).

## Notes

- Compatibility reports 377–444 draw calls for S4 (vs 13 on Forward+ and 66–130
  on Mobile): each instance is its own draw, and batching does not merge them.
- `first_120` is 0 on Compatibility for every cell: OpenGL compiles its shaders
  during the first frame differently, and no pipeline counter increments in the
  first 120 frames the harness measures.
- Hitches: S3a (29), S3b (17) and S6_1q_P0 (8) reported frames above twice
  the median. The machine was idle; these are renderer/driver pacing, not
  contention. They are recorded, not hidden.

## Verification

```text
python3 benchmarks/tools/check_report.py benchmarks/results/2026-11-19_1329_d3_compat/*/*.json
```

Exit code 0 for the valid cells; the 3 not-applicable cells exit 3 with
the unsupported effect named.