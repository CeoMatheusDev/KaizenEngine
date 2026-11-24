# PhysX Physics 3D

A `PhysicsServer3D` implementation backed by the NVIDIA PhysX 5 SDK
(`thirdparty/physx/physx`, fetched with `tools/sdk/sdk_manager.py --download physx/physx`).

Registration name: **"PhysX Physics"** (`PhysicsServer3DManager::PHYSX_PHYSICS_NAME`).
Select it with `physics/3d/physics_engine` in Project Settings, or pass
`--physics-3d-server "PhysX Physics"` to the editor/game.

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
| Spaces (`PxScene`, gravity, active flag) | Real |
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
- Cooking requires `builtin_physx_extensions` style support to be present in the
  SDK clone; mesh shapes fail to build gracefully (shape stays invalid) when
  cooking is unavailable.
