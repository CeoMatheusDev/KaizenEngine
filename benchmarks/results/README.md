# Published benchmark results

Evidence for the rendering benchmark cells that passed the validity gate
defined in [Benchmark scene specification §7](../../docs/architecture/rendering-benchmark-scenes.md)
and enforced by [`benchmarks/tools/check_report.py`](../tools/check_report.py).

A directory here means something specific: the reports came from a
`target=template_release` build, at the pinned viewport size, with VSync off, no
pipeline compilation in or near the sampling window, and five warm runs plus one
cold run that all pass the checker with zero failures. Anything less stays in the
[run log](../../docs/architecture/rendering-benchmark-run-log.md) as a smoke run,
which publishes nothing.

A published number is still only a reference for the machine and commit recorded
inside the report. It is not a portable performance claim.

## Layout

```text
results/
  2026-11-19_s1_d1_q0/        one session, one cell: reports at the top level
    README.md                 environment, commands, per-run table, manual checks
    S1_D1_Q0_{cold,r1..r5}.json.gz
    frame_299.png
    checker.txt
  2026-11-19_d1/              one session, several cells: one directory per cell
    README.md                 shared environment, commands, cell table
    s2_4096_q0/{cold,r1..r5}.json.gz
    ...
  2026-11-19_s6_d1_q{1,4,16}/ S6 at one overdraw level, post presets P0-P5
    README.md
    p0..p5/{cold,r1..r5}.json.gz
    p0/frame_299.png          P0 reference used by every other cell at that level
    checker.txt
```

Each session README records the hardware, the exact command line, which cells it
covers, and which checks stayed manual. It never invents a value: a field without
a run is written as not measured.

## Sessions

| Session | Cells | Reports | Configuration |
|---|---|---|---|
| [`2026-11-19_s1_d1_q0`](2026-11-19_s1_d1_q0/README.md) | S1/Q0, 400 boxes | 6 | D1 |
| [`2026-11-19_d1`](2026-11-19_d1/README.md) | S2/4096, S3a/64, S3b/64, six S4 light cells, all Q0 | 54 | D1 |
| [`2026-11-19_s6_d1_q1`](2026-11-19_s6_d1_q1/README.md) | S6 at one quad, P0–P5 | 36 | D1 |
| [`2026-11-19_s6_d1_q4`](2026-11-19_s6_d1_q4/README.md) | S6 at four quads, P0–P5 | 36 | D1 |
| [`2026-11-19_s6_d1_q16`](2026-11-19_s6_d1_q16/README.md) | S6 at sixteen quads, P0–P5 | 36 | D1 |

## Verifying a session

```text
python3 benchmarks/tools/check_report.py benchmarks/results/<session>/*.json.gz
python3 benchmarks/tools/check_report.py benchmarks/results/<session>/*/*.json.gz
```

The checker reads `.json` and `.json.gz`. Reports are committed gzipped because a
full report is about 540 KB uncompressed (3600 frames of per-frame arrays) and
about 23 KB compressed; the frame hash inside the report is what proves identity,
not the file size.

Exit code 0 means every report and every repeated cell passed. A nonzero exit
prints the failing rule with the measured value.

## What the checker cannot check

Driver-forced VSync, GPU temperature and thermal throttling, and whether the
rendered content is the scene the label claims. The session README states which
of these were inspected manually and how; see
[the method](../../docs/architecture/rendering-benchmarks.md) for why they stay
manual.
