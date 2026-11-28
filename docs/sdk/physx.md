# NVIDIA PhysX 5 integration

Kaizen includes an optional, source-built PhysX 5 backend for 3D. It is a separate `PhysicsServer3D` implementation; the built-in Kaizen server remains the default. Current implementation coverage and explicit gaps are maintained in [`modules/physx_physics_3d/README.md`](../../modules/physx_physics_3d/README.md).

## Fetch and build

The PhysX checkout is local and is not downloaded during normal builds. Fetch the pinned revision, then build the backend group:

```bash
python3 tools/sdk/sdk_manager.py --download physx/physx
python3 tools/sdk/sdk_manager.py --validate physx/physx
./build.sh physx
```

The SDK manager places the repository at `thirdparty/physx/physx/`. The SCons module compiles the required CPU SDK sources directly into the engine; no separate CMake output or prebuilt PhysX libraries are required. The current build excludes PhysX GPU/CUDA simulation. SDK acquisition details and the pinned revision live in `tools/sdk/manifest/physx_sdks.json`.

To build the editor with PhysX and run the engine-level regression probe:

```bash
./build.sh engine
bash tests/kaizen/run_physx_engine_test.sh
```

The standalone simulation smoke test is also available after building the SDK group:

```bash
bash tests/kaizen/run_physx_smoke.sh
```

## Select the server

The server registers as **`PhysX Physics`**. Select it in Project Settings with `physics/3d/physics_engine`, or pass `--physics-3d-server "PhysX Physics"` when starting the editor/game. If the SDK checkout is absent, the build disables the optional module and continues with the built-in physics backend.

## Runtime settings

The PhysX module registers these project settings:

| Setting | Default | Behavior |
|---|---:|---|
| `physics/physx_physics_3d/gpu_acceleration` | `false` | GPU simulation is not compiled into this CPU-only build. Enabling this request logs a warning; simulation still runs on CPU. |
| `physics/physx_physics_3d/cooking` | `true` | Enables cooked mesh-based shapes when the SDK cooking sources are available. |
| `physics/physx_physics_3d/character_controller` | `false` | Creates the low-level per-scene PhysX controller manager for native integration clients. |
| `physics/physx_physics_3d/broadphase_mbp` | `true` | Selects MBP broadphase and creates a large default region; `false` uses SAP. |
| `physics/physx_physics_3d/simulation_step` | `1/60` | Fixed simulation step, constrained to 0.001–0.1 seconds at backend startup. |
| `physics/physx_physics_3d/max_sub_steps` | `4` | Maximum sub-steps per engine step, clamped to 1–16 at startup. |

## Current implementation

The backend implements PhysX scenes, supported primitive and cooked shapes, static/kinematic/dynamic bodies, mass and force operations, collision layers/masks/exceptions, trigger areas, direct-space queries, and pin/hinge/slider/cone-twist/generic 6DOF joints. State synchronization and overlap monitor callbacks are handled by `PhysXPhysicsServer3D`. Default world gravity magnitude/direction and damping are received through the engine's space-RID `area_set_param` calls. Forces are applied once per actual fixed simulation substep; inactive spaces are not advanced, and the backend fetches results only for scenes that were simulated.

Soft bodies and custom shapes are unsupported. Separation rays use a thin-box approximation; heightmap sample editing is unsupported. Direct-state contact manifolds, space debug contacts and island/contact-pair profiling counters are not exposed yet. Area gravity/damping override modes, priority ordering and point gravity are evaluated per substep using overlap data; wind parameters are retained but rigid-body aerodynamic wind is not simulated. Vehicle APIs and GPU simulation are not exposed by this backend. See the module README for the detailed coverage table and remaining integration gaps.

PhysX is distributed under the Apache 2.0 license in the pinned 5.x source tree. Do not commit the downloaded SDK checkout; it is ignored local build input.
