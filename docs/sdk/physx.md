# NVIDIA PhysX SDK Integration Guide

## Overview

KaizenEngine supports the NVIDIA PhysX SDK as an alternative physics backend. PhysX is integrated locally and can be built from source. The engine's physics abstraction allows PhysX to be used alongside or instead of the default Kaizen physics backend.

## Supported Components

### PhysX SDK Core
- **Foundation**: Memory management, error handling, profiling
- **Common**: Shared types, math, cooking base
- **Core**: Physics simulation, rigid bodies, constraints
- **Cooking**: Mesh cooking, collision mesh generation
- **Extensions**: Utility extensions, joints, vehicles
- **Character Controller**: Capsule-based character controller
- **Vehicle SDK**: Vehicle simulation (wheels, suspension, drivetrain)
- **GPU Acceleration**: GPU-accelerated rigid body simulation (optional)

## Installation

### Automatic SDK Status Check
```bash
python tools/sdk/sdk_manager.py --list
```

### Download
```bash
python3 tools/sdk/sdk_manager.py --download physx/physx
```
This shallow-clones [NVIDIA-Omniverse/PhysX](https://github.com/NVIDIA-Omniverse/PhysX)
at the tag pinned in `tools/sdk/manifest/physx_sdks.json`
(`110.3-omni-and-physx-5.9.1`, PhysX SDK 5.9.1) into `thirdparty/physx/physx/`.
The pin keeps the build reproducible; the clone is ~90 MB.

PhysX is compiled from source by the module's SCsub (no CMake step).

### Build Configuration
PhysX is compiled straight into the engine: the module's SCsub builds the
CPU SDK from `thirdparty/physx/physx/physx/source`, so a normal build picks
it up with no extra steps:
```bash
./build.sh physx                              # just the PhysX group (~460 SDK objects)
scons platform=linuxbsd target=editor        # PhysX on by default
```

## Build Options

| Option | Default | Description |
|--------|---------|-------------|
| `builtin_physx` | yes | Enable NVIDIA PhysX SDK integration |
| `builtin_physx_extensions` | yes | Enable PhysX Extensions (cooking, character controller, vehicle) |
| `builtin_physx_gpu` | no | Reserved. `yes` is rejected by the SCsub: the PhysX GPU sources are not built yet |
| `physx_sdk_path` | thirdparty/physx | Custom path to PhysX SDK root |

## Directory Structure
```
thirdparty/physx/
├── physx/              # Cloned PhysX repository root
│   └── physx/          # PhysX SDK source
│       ├── source/     # Foundation, Common, Core, Extensions, etc.
│       └── include/    # Public headers (PxPhysics.h, etc.)
└── build/              # CMake build output (generated)
# PhysX 5.x has no separate pxshared/ directory: the shared headers ship inside
# physx/physx/ (manifest: "bundled_with_physx"). A separate pxshared/ tree is only
# relevant for PhysX 4.x layouts.
```

## Architecture

### Engine Physics Abstraction
PhysX is integrated behind the engine's physics abstraction layer:

```
kaizen/physics/
├── backend/            # PhysicsBackend abstract interface
├── collision/          # CollisionInterface abstract interface
├── simulation/         # SimulationInterface abstract interface
├── diagnostics/        # PhysicsDiagnostics abstract interface
└── physx/              # PhysX-specific backend implementation
    ├── physx_backend.h/cpp
    └── SCsub
```

### Key Interfaces
- **PhysicsBackend**: Abstract interface for all physics backends
- **CollisionInterface**: Abstract collision detection (shapes, raycasts, sweeps)
- **SimulationInterface**: Simulation stepping and synchronization
- **PhysicsDiagnostics**: Performance metrics and profiling

### Replaceability
PhysX can be swapped for any other physics backend by implementing the same interfaces. No PhysX-specific types leak into engine code.

## Licensing
- PhysX 5.x: Apache 2.0 (fully open source)
- PhysX 4.1 (legacy): BSD-3-Clause
- Redistribution is permitted for both source and binary forms
- Third-party components (OpenUSD, etc.) retain their own licenses

## In-engine Build
The module's SCsub compiles the CPU SDK sources directly (no separate CMake
invocation). It defines `PX_PHYSX_STATIC_LIB`, `PX_PUBLIC_RELEASE=1` (the same
as NVIDIA's public release build: no internal device telemetry) and
`DISABLE_CUDA_PHYSX`, so the GPU code paths are compiled out. Cooking
(`physxcooking`, used for convex, triangle and heightfield meshes), extensions,
character kinematic and vehicle are included. The OmniPvd headers under
`physx/pvdruntime` are on the include path, so PVD streams compile in.
Platform-specific sources (`*/windows/*`, `*/unix/*`) are filtered per platform.

`tests/kaizen/run_physx_smoke.sh` links its test with the same defines. The
SDK headers change class layouts with these macros, so a mismatch would be
an ABI error and not a clean failure.

To verify the SDK build end to end:
```bash
./build.sh physx     # compile the PhysX SDK group
./build.sh smoke     # runtime smoke test: a box must settle on a plane
```

Verified on Linux x86_64 with PhysX 5.9.1: the group builds with no errors,
and the smoke test passes (the box settles at y = 0.500 on a plane and comes
to rest).

## Runtime Detection
- PhysX is only enabled when `builtin_physx=yes` is set during build
- GPU acceleration requires a CUDA-capable GPU
- The backend gracefully degrades if PhysX is not available

### Implementation Status (modules/physx_physics_3d)

The "PhysX Physics" PhysicsServer3D runs on the real PhysX 5 SDK
(`PhysXPhysicsServer3D`, driven by `PhysXBackend`):

- **Spaces**: per-space `PxScene` with gravity and fixed-step simulation
  (accumulator with a sub-step cap).
- **Shapes**: world boundary, sphere, box, capsule, cylinder (convex hull),
  convex polygon, concave polygon (cooked triangle mesh), heightmap (cooked),
  separation ray (thin-box approximation).
- **Bodies**: static/kinematic/rigid/rigid-linear modes, mass properties,
  velocities, forces/impulses, axis locking, CCD, collision layer/mask and
  collision exceptions (custom `PxSimulationFilterShader`).
- **Areas**: trigger bodies with body/area monitor callbacks (per-step overlap
  diffing, covering both body and area overlaps).
- **Queries**: raycast, point/shape overlap, shape collide, rest info and
  motion tests through the direct space state.
- **Joints**: pin (spherical), hinge (revolute), slider (prismatic), cone-twist
  and generic 6DOF (D6) with limits, motors and drive velocities.

Not covered (documented, loud no-op where applicable): soft bodies, custom
shapes, per-sample heightmap editing, area wind and override-mode gravity.

### Configuration (`physics/physx_physics_3d/*`)

Project settings applied when the "PhysX Physics" server starts:

| Setting | Default | Effect |
| --- | --- | --- |
| `gpu_acceleration` | on | Request GPU acceleration when available (needs a CUDA build, `builtin_physx_gpu=yes`; falls back to CPU with a warning otherwise). |
| `broadphase_mbp` | on | Use the MBP broadphase with a default region (SAP otherwise). |
| `cooking` | on | Enable mesh cooking (convex / concave / heightmap shapes). |
| `character_controller` | on | Create the per-scene character controller manager. |
| `vehicle` | on | Vehicle API availability. |
| `simulation_step` | 1/60 | Fixed simulation step in seconds. |
| `max_sub_steps` | 4 | Sub-step cap per `step()` call. |
| `broadphase_mbp` | on | MBP broadphase (off = SAP). |

Select the server with `physics/3d/physics_engine = "PhysX Physics"` (or
`--physics-3d-server "PhysX Physics"`). Scripts use the standard
`PhysicsServer3D` API; the settings are readable and writable through
`ProjectSettings` like any other engine setting.
