# Architecture Overview

## 1. Purpose

KaizenEngine is a source-level derivative of the Godot Engine. Its current architecture therefore contains a substantial inherited foundation. The purpose of this document is to establish a clear baseline for engineering work: what is inherited, where Kaizen-specific evolution can occur, and which constraints must be respected.

This document is not a claim that every subsystem described by the upstream project has already been independently reimplemented by Kaizen.

## 2. Architectural baseline

### 2.1 Engine foundation

The project retains the Godot-style engine organization and core concepts, including:

- scene and node based application structure;
- editor and project management infrastructure;
- rendering server and rendering backends;
- resource and asset pipelines;
- platform abstraction and export infrastructure;
- audio, physics, and scripting integration points.

The exact availability of a feature depends on the build configuration, platform, and implementation present in this repository revision.

### 2.2 Project identity

Kaizen projects use the `project.kaizen` project file. Do not assume that upstream project-file naming or tooling behavior applies unchanged.

### 2.3 Build system

The source tree uses SCons through `SConstruct`. The repository README documents the verified build targets and minimum build-tool requirements. Build options and platform support must be checked against the current `SConstruct` and platform detection scripts before being documented as guaranteed.

## 3. Inherited systems versus Kaizen-owned work

Use the following terminology consistently:

| Classification | Meaning |
|---|---|
| Inherited | Code or behavior originating from the Godot base and retained by Kaizen. |
| Adapted | Inherited code modified, renamed, configured, or integrated by Kaizen. |
| Kaizen-specific | New implementation or behavior authored for Kaizen. |
| Planned | Intended work that is not yet implemented or verified. |

A renamed module, a configuration file, or a directory with a Kaizen-specific name is not sufficient evidence of an independent implementation.

Examples that require this distinction:

- The 2D and 3D physics foundation is inherited. The presence of Kaizen-named physics module directories does not establish a new physics engine.
- Jolt integration is part of the inherited foundation unless Kaizen-specific implementation changes are explicitly identified.
- Rendering currently builds on the inherited Godot rendering architecture. A future custom renderer is a roadmap objective, not a description of the current renderer.

## 4. Rendering subsystem: current boundary

The rendering stack is organized around the engine's rendering server and renderer implementations. Relevant source areas include:

- `servers/rendering/` — rendering server interfaces and orchestration;
- `servers/rendering/renderer_rd/` — RenderingDevice-based renderer infrastructure;
- `servers/rendering/renderer_rd/forward_clustered/` — Forward+ renderer implementation;
- `servers/rendering/renderer_rd/forward_mobile/` — mobile-oriented renderer implementation;
- `servers/rendering/renderer_gl/` — Compatibility renderer infrastructure;
- `drivers/vulkan/` — Vulkan driver integration;
- `platform/web/` — Web platform integration.

These paths are navigation anchors, not a complete dependency map. Before changing renderer selection, resource ownership, synchronization, or rendering-thread behavior, trace the actual call flow in the current source revision.

The planned Kaizen rendering evolution is documented in [Rendering architecture and roadmap](rendering-roadmap.md).

## 5. Platform strategy

The project targets a broad set of execution environments, but platform presence and feature parity are separate concerns.

| Target | Engineering position |
|---|---|
| Desktop | Primary environment for developing and validating high-quality rendering features. |
| Mobile | Requires explicit bandwidth, memory, thermal, and GPU capability constraints. |
| Web | Must use a backend and feature set supported by browser APIs; WebGL 2 is the conservative baseline for the current Web export path. |

Do not infer support for a rendering feature from the existence of a platform directory. Verify the selected backend, build flags, shader language path, and export configuration.

## 6. Design constraints

Kaizen-specific engine work should preserve the following constraints:

- **Compatibility:** integrate with the engine's resource lifecycle, rendering server, threading model, and editor rather than bypassing them without a deliberate migration plan.
- **Explicit ownership:** document who creates, owns, references, and destroys GPU and CPU resources.
- **Capability-driven behavior:** select rendering paths from queried backend capabilities, not platform-name assumptions alone.
- **Measured performance:** establish repeatable CPU, GPU, memory, and frame-pacing baselines before optimizing.
- **Incremental replacement:** replace or extend one well-defined subsystem at a time; avoid maintaining two competing implementations without a migration boundary.
- **Honest status:** distinguish implemented, partially implemented, inherited, experimental, and planned functionality.

## 7. Source-of-truth policy

When documentation and source disagree, source code is authoritative for current behavior. Update the documentation after verifying the implementation. For upstream behavior, use the matching Godot version's documentation and source rather than assuming that the latest upstream behavior is identical to Kaizen's base.
