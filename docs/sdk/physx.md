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

### Manual Installation (from source)
1. Clone PhysX 5.x from [GitHub](https://github.com/NVIDIA-Omniverse/PhysX) into `thirdparty/physx/physx/`
2. The build system will automatically build PhysX from source using CMake

### Build Configuration
```bash
# Enable PhysX during build
scons builtin_physx=yes builtin_physx_extensions=yes builtin_physx_gpu=no
```

## Build Options

| Option | Default | Description |
|--------|---------|-------------|
| `builtin_physx` | no | Enable NVIDIA PhysX SDK integration |
| `builtin_physx_extensions` | no | Enable PhysX Extensions (cooking, character controller, vehicle) |
| `builtin_physx_gpu` | no | Enable PhysX GPU acceleration (requires CUDA-capable GPU) |
| `physx_sdk_path` | thirdparty/physx | Custom path to PhysX SDK root |

## Directory Structure
```
thirdparty/physx/
├── physx/              # Cloned PhysX repository root
│   └── physx/          # PhysX SDK source
│       ├── source/     # Foundation, Common, Core, Extensions, etc.
│       └── include/    # Public headers (PxPhysics.h, etc.)
├── pxshared/           # Shared PhysX headers (PhysX 4.x layouts only)
└── build/              # CMake build output (generated)
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

## CMake Build
PhysX is built from source using CMake with the following options:
- `PX_BUILD_DEFAULT_SAMPLES=OFF`
- `PX_BUILD_TOOLS=OFF`
- `PX_BUILD_TESTSUITE=OFF`
- `PX_BUILD_COOKING=ON`
- `PX_BUILD_EXTENSIONS=ON`
- `PX_BUILD_CHARACTER_KINEMATIC=ON`
- `PX_BUILD_VEHICLE=ON`
- `PX_BUILD_GPU=ON/OFF` (configurable)
- `PX_BUILD_PVD=OFF`

## Runtime Detection
- PhysX is only enabled when `builtin_physx=yes` is set during build
- GPU acceleration requires a CUDA-capable GPU
- The backend gracefully degrades if PhysX is not available
