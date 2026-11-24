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

#include "core/math/vector3.h"
#include "kaizen/physics/backend/physics_backend.h"

#ifdef PHYSX_ENABLED
	#include "PxPhysicsAPI.h"
	#include "PxCooking.h"
	#include "PxController.h"
	#include "PxVehicleDrive.h"
#endif

// NVIDIA PhysX physics backend.
// Implements the engine's physics backend interface using the NVIDIA
// PhysX SDK, isolated behind the PhysicsBackend abstraction.
class PhysXBackend : public PhysicsBackend {
public:
	struct Config {
		bool enabled = false;
		bool gpu_acceleration = false;
		bool cooking = true;
		bool character_controller = true;
		bool vehicle = true;
		float simulation_step = 1.0f / 60.0f;
		int max_sub_steps = 4;
		bool broadphase_mbp = true;
	};

	struct SceneDesc {
		Vector3 gravity = Vector3(0, -9.81, 0);
		bool enable_pvd = false;
	};

	PhysXBackend();
	~PhysXBackend() override;

	Type get_type() const override { return TYPE_PHYSX; }
	bool is_supported() const override;
	bool init() override;
	void shutdown() override;

	// Simulation.
	bool simulate(float p_step) override;
	bool fetch_results(bool p_wait = true) override;

	// Scene management (PhysicsBackend interface).
	void *create_scene(const Vector3 &p_gravity) override;
	void destroy_scene(void *p_scene) override;

	// Scene management with full descriptor (PhysX-specific).
	void *create_scene(const SceneDesc &p_desc);

	// Clean up all resources.
	void cleanup() override;

	// Configuration.
	const Config &get_config() const { return config; }
	void set_config(const Config &p_config) { config = p_config; }

	// PhysX SDK access.
	void *get_physics() const { return physics; }
	void *get_cooking() const { return cooking; }
	void *get_dispatcher() const { return dispatcher; }

private:
	bool supported = false;
	Config config;

	void *foundation = nullptr;
	void *physics = nullptr;
	void *cooking = nullptr;
	void *dispatcher = nullptr;
	void *controller_manager = nullptr;
	void *vehicle_manager = nullptr;
};
