# KaizenEngine Documentation

This directory contains Kaizen-specific engineering documentation. It complements the upstream [Godot documentation](https://docs.godotengine.org/en/latest/), which remains the reference for inherited engine systems unless a Kaizen document explicitly describes a divergence.

## Documents

- [Architecture overview](architecture/overview.md) — current architectural baseline, inherited subsystems, extension boundaries, and engineering constraints.
- [Rendering audit (R0)](architecture/rendering-audit.md) — source-verified baseline of renderer selection, threading, resource lifetime entry points, and the shader pipeline.
- [Rendering resource lifetime](architecture/rendering-resource-lifetime.md) — how RIDs reach the graphics API and when objects are actually destroyed (deferred disposal, frame ring, dependency order).
- [Resource lifetime diagnostics](architecture/resource-diagnostics.md) — Kaizen's own tracker: generation handles, pending-release accounting, diagnostics modes, integration hooks, measured overhead and churn results.
- [Pipeline cache and shader variants](architecture/rendering-pipeline-cache.md) — variant table, device PSO cache persistence, backend support and invalidation rules.
- [Rendering benchmark methodology](architecture/rendering-benchmarks.md) — what to measure, with which built-in instruments, on which reference configurations, and how results must be reported (five D1 sessions published, the rest unmeasured).
- [Benchmark scene specification](architecture/rendering-benchmark-scenes.md) — implementable definitions for scenes S1–S7: fixed content and parameters, camera contract, quality presets, cell matrix, sampling protocol, harness contract, and validity gate.
- [Benchmark implementation report](architecture/rendering-benchmark-implementation.md) — delivered benchmark scenes and harness, implementation decisions, verification status, and the validated executions.
- [Benchmark projects](../benchmarks/README.md) — executable S1–S4 and S6 scenes, the S7 manual editor procedure, the sampling harness, and published D1 cells for S1, S2, S3a, S3b, S4 and S6 (S5 not written).
- [Benchmark run log](architecture/rendering-benchmark-run-log.md) — what was actually executed, on which machine, and which defects only a real run exposed.
- [Web export runtime](architecture/rendering-web-runtime.md) — why Web is pinned to the Compatibility renderer, WebGL 2 requirements, threading and deployment headers, and the absence of shader/pipeline caches on Web.
- [Metal rendering driver](architecture/rendering-metal-driver.md) — build gating, SPIR-V→MSL→metallib shader pipeline, disabled PSO cache, heap allocator and barrier model, reported features, and known stubs.
- [Rendering architecture and roadmap](architecture/rendering-roadmap.md) — the technical plan for evolving Kaizen's rendering stack toward a custom high-quality renderer across desktop, mobile, and Web.

## Documentation principles

1. **Describe the source tree, not aspirations, as current functionality.** Planned work must be labelled as planned.
2. **Identify upstream inheritance.** A directory rename, configuration file, or integration point does not by itself establish an original implementation.
3. **Prefer verifiable statements.** Link source paths and identify assumptions that still require code-level validation.
4. **Keep platform capability explicit.** A platform directory or exporter does not prove that every renderer or feature is supported there.
5. **Update documentation with implementation changes.** When a milestone changes the architecture, update the relevant document in the same change.

## Scope

These documents focus on Kaizen-specific engineering decisions and the current source-code evolution. For inherited APIs, editor workflows, scripting, scene systems, and build details, consult the upstream Godot documentation while checking compatibility against the Kaizen source tree.
