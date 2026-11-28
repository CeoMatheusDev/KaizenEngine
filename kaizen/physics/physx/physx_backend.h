//************************************************************************//
//  physx_backend.h                                                       //
//************************************************************************//
//                         This file is part of:                          //
//                            KaizenEngine                                //
//              https://github.com/CeoMatheusDev/Kaizen                   //
//************************************************************************//
// Copyright (c) 2007-2014 Juan Linietsky, Ariel Manzur.                  //
// Copyright (c) 2014-present Godot Engine contributors.                  //
// Copyright (c) 2026 Matheus A. Santos, Hardion Labs.                    //
//                                                                        //
// This file contains code derived from the Godot Engine                  //
// (https://godotengine.org), which is licensed under the MIT License.    //
// The original MIT notice is preserved below.                            //
//                                                                        //
// Modified by Matheus A. Santos, Hardion Labs on 2026-11-23.             //
// Modifications and additional code are licensed under the GNU Affero    //
// General Public License v3.0 (AGPL-3.0).                                //
// See the files LICENSE.txt and LICENSE-ADDITIONAL-TERMS.md for details. //
//************************************************************************//

#pragma once

#include "core/object/ref_counted.h"
#include "kaizen/physics/backend/physics_backend.h"

#include <functional>

#include <PxPhysicsAPI.h>

/// @brief PhysX backend for the physics system.
/// Real PhysX 5 implementation: foundation, physics, cooking, dispatcher and
/// scene lifecycle with fixed-step simulation. Owned by the "PhysX Physics"
/// PhysicsServer3D (modules/physx_physics_3d), which drives simulate()/fetch_results().
class PhysXBackend : public PhysicsBackend {
	GDCLASS(PhysXBackend, PhysicsBackend);

public:
	/// @brief Configuration for the PhysX backend.
	struct Config {
		bool gpu_acceleration = false; // GPU simulation is not compiled into this CPU-only SDK build.
		bool cooking = true; // Enable mesh cooking for convex/triangle meshes.
		bool character_controller = false; // Create the low-level manager for native integration clients.
		real_t simulation_step = 1.0 / 60.0; // Fixed simulation step.
		int max_sub_steps = 4; // Maximum sub-steps per simulate() call.
		bool broadphase_mbp = true; // Use MBP broadphase instead of SAP.

		Config() = default;
	};

private:
	Config config;
	void *foundation = nullptr; // PxFoundation.
	void *physics = nullptr; // PxPhysics.
	void *dispatcher = nullptr; // PxDefaultCpuDispatcher.
	void *controller_manager = nullptr; // PxControllerManager of the first scene.
	Vector<void *> controller_managers; // Per-scene PxControllerManager instances.
	Vector<void *> scenes; // Created PxScene instances.
	Vector<uint8_t> scene_active; // Whether each scene should participate in simulation.
	Vector<uint8_t> scene_pending_results; // True after simulate() until fetchResults().
	std::function<void()> before_simulation_step; // Applies engine-side forces once per fixed substep.
	real_t step_accumulator = 0; // Fixed-step accumulator.

public:
	PhysXBackend() = default;
	~PhysXBackend() override;

	// ---- PhysicsBackend interface ----

	Type get_type() const override { return TYPE_PHYSX; }
	bool is_supported() const override;
	bool init() override;
	void shutdown() override;
	bool simulate(float p_step) override;
	bool fetch_results(bool p_wait = true) override;
	void *create_scene(const Vector3 &p_gravity) override;
	void destroy_scene(void *p_scene) override;
	void set_scene_active(void *p_scene, bool p_active);
	void cleanup() override;

	// ---- SDK accessors for the PhysX physics server ----

	physx::PxPhysics *get_physics() const { return (physx::PxPhysics *)physics; }
	physx::PxFoundation *get_foundation() const { return (physx::PxFoundation *)foundation; }
	physx::PxControllerManager *get_controller_manager() const { return (physx::PxControllerManager *)controller_manager; }
	const Vector<void *> &get_scenes() const { return scenes; }

	Config &get_config() { return config; }
	const Config &get_config() const { return config; }
	void set_before_simulation_step(const std::function<void()> &p_callback) { before_simulation_step = p_callback; }

	// ---- Conversion helpers ----

	static physx::PxVec3 to_px(const Vector3 &p_v) { return physx::PxVec3(p_v.x, p_v.y, p_v.z); }
	static Vector3 from_px(const physx::PxVec3 &p_v) { return Vector3(p_v.x, p_v.y, p_v.z); }
	static physx::PxTransform to_px(const Transform3D &p_t);
	static Transform3D from_px(const physx::PxTransform &p_t);

	// ---- Simulation filter (layer/mask + collision exceptions) ----

	/// @brief Custom PxSimulationFilterShader used by every created scene.
	/// Filter data layout: word0 = collision layer, word1 = collision mask,
	/// word2/word3 = owner pointer halves (for exception lookup).
	static physx::PxFilterFlags filter_shader(physx::PxFilterObjectAttributes p_attr_0, physx::PxFilterData p_data_0,
			physx::PxFilterObjectAttributes p_attr_1, physx::PxFilterData p_data_1,
			physx::PxPairFlags &r_pair_flags, const void *p_constant_block, physx::PxU32 p_constant_block_size);

	/// @brief Register/unregister a collision exception between two owners.
	static void add_collision_exception(const void *p_owner_a, const void *p_owner_b);
	static void remove_collision_exception(const void *p_owner_a, const void *p_owner_b);
};
