# PhysX Physics 3D

A `PhysicsServer3D` implementation backed by the NVIDIA PhysX 5 SDK
(`thirdparty/physx/physx`, fetched with `tools/sdk/sdk_manager.py --download physx/physx`).

Registration name: **"PhysX Physics"** (`PhysicsServer3DManager::PHYSX_PHYSICS_NAME`).
Select it with `physics/3d/physics_engine = "PhysX Physics"` in the project's
`project.kaizen` (or Project Settings). The project must be a `project.kaizen`
file: Kaizen ignores `project.godot`.

## Layout

| File | Contents |
| --- | --- |
| `physx_physics_server_3d.h/cpp` | `PhysXPhysicsServer3D` — the server implementation (subclass of `PhysicsServer3DDummy`, overrides the covered surface with real PhysX behavior). |
| `register_types.cpp` | Module registration + factory (`memnew(PhysXPhysicsServer3D)`). |

The shared SDK lifecycle (foundation, physics, cooking, dispatcher, controller
manager, scene creation, fixed-step simulate/fetch, filter shader, collision
exceptions) lives in `kaizen/physics/physx/physx_backend.{h,cpp}` (`PhysXBackend`).

## Coverage

| Family | Status |
| --- | --- |
| Spaces (`PxScene`, gravity, active flag) | Real; World3D's default gravity is routed to the scene (`PxScene::setGravity`) |
| Shapes: world boundary / sphere / box / capsule | Real (native geometry) |
| Shapes: cylinder | Real (cooked convex hull) |
| Shapes: convex polygon / concave polygon / heightmap | Real (cooked via `PxCook*`) |
| Shapes: separation ray | Thin-box approximation |
| Bodies: static / kinematic / rigid / rigid-linear | Real (`PxRigidStatic` / `PxRigidDynamic`) |
| Bodies: mass, inertia, velocities, forces, impulses, damping | Real |
| Bodies: axis locks, CCD, layer/mask/exceptions | Real (lock flags, `eENABLE_CCD`, custom filter shader) |
| Areas (triggers + monitor callbacks) | Real (per-step overlap diffing) |
| Queries: ray / point / shape / collide / rest / motion | Real (`PxScene` queries) |
| Joints: pin / hinge / slider | Real (spherical / revolute / prismatic) |
| Joints: cone-twist / generic 6DOF | Real (D6 with limits, motors, drives) |
| Soft bodies | **Not supported** (warns once; RID invalid) |
| Custom shapes, heightmap sample editing | Not supported (inherits dummy no-op) |
| Area wind, gravity/damp override modes | Cached only (documented) |

## Notes

- Simulation uses a fixed step (`PhysXBackend::Config::simulation_step`, default
  1/60 s) with a sub-step cap (`max_sub_steps`, default 4).
- Collision filtering mirrors Godot semantics: two-way layer/mask check plus
  per-pair exceptions, implemented as a custom `PxSimulationFilterShader`.
- Mesh cooking (`PxCookConvexMesh`, `PxCookTriangleMesh`) is compiled from
  `physxcooking`. Mesh shapes still fail gracefully (the shape stays invalid) if
  cooking returns nothing.
- GPU simulation is not built. `builtin_physx_gpu=yes` stops the SCsub with an error.
- The SDK is built with `PX_PUBLIC_RELEASE=1`, which leaves out the internal
  `PhysXIndicator.h` telemetry header that is not in the public repository.

## Tests

```bash
bash tests/kaizen/run_physx_smoke.sh        # SDK alone: a box lands on a plane
bash tests/kaizen/run_physx_engine_test.sh  # engine: gravity routing, node sync, rest position
```

The engine test needs the editor binary (`./build.sh physx`). It fails on the two
regressions fixed in this module: a null-area error from World3D's space gravity,
and RigidBody3D nodes that never followed the simulation.
