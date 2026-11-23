//************************************************************************//
//  physx_backend.cpp                                                     //
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

#include "physx_backend.h"

#include "core/os/os.h"

using namespace RendererRD;

PhysXBackend::PhysXBackend() {
	config.enabled = false;
	config.gpu_acceleration = false;
	config.cooking = true;
	config.character_controller = true;
	config.vehicle = true;
	config.simulation_step = 1.0f / 60.0f;
	config.max_sub_steps = 4;
	config.broadphase_mbp = true;
	supported = false;

#ifdef PHYSX_ENABLED
	foundation = nullptr;
	physics = nullptr;
	cooking = nullptr;
	dispatcher = nullptr;
	controller_manager = nullptr;
	vehicle_manager = nullptr;
#endif
}

PhysXBackend::~PhysXBackend() {
	shutdown();
}

bool PhysXBackend::init() {
#ifdef PHYSX_ENABLED
	// Initialize PhysX Foundation.
	// This would call PxCreateFoundation.
	foundation = memnew(char[1]); // Placeholder

	if (!foundation) {
		WARN_PRINT("PhysXBackend: Failed to create PhysX Foundation.");
		return false;
	}

	// Create PhysX Physics SDK.
	physics = memnew(char[1]);
	if (!physics) {
		WARN_PRINT("PhysXBackend: Failed to create PhysX Physics SDK.");
		return false;
	}

	// Create cooking if enabled.
	if (config.cooking) {
		cooking = memnew(char[1]);
	}

	// Create character controller manager if enabled.
	if (config.character_controller) {
		controller_manager = memnew(char[1]);
	}

	// Create vehicle manager if enabled.
	if (config.vehicle) {
		vehicle_manager = memnew(char[1]);
	}
#endif

	supported = true;
	return true;
}

void PhysXBackend::shutdown() {
#ifdef PHYSX_ENABLED
	if (vehicle_manager) {
		memdelete(vehicle_manager);
		vehicle_manager = nullptr;
	}
	if (controller_manager) {
		memdelete(controller_manager);
		controller_manager = nullptr;
	}
	if (cooking) {
		memdelete(cooking);
		cooking = nullptr;
	}
	if (physics) {
		memdelete(physics);
		physics = nullptr;
	}
	if (foundation) {
		memdelete(foundation);
		foundation = nullptr;
	}
#endif
	supported = false;
}

bool PhysXBackend::simulate(float p_step) {
	if (!supported) {
		return false;
	}

#ifdef PHYSX_ENABLED
	// PhysX simulation step would go here:
	// scene->simulate(p_step);
#endif
	return true;
}

bool PhysXBackend::fetch_results(bool p_wait) {
	if (!supported) {
		return false;
	}

#ifdef PHYSX_ENABLED
	// scene->fetchResults(p_wait);
#endif
	return true;
}

void *PhysXBackend::create_scene(const SceneDesc &p_desc) {
	if (!supported) {
		return nullptr;
	}

#ifdef PHYSX_ENABLED
	// Create PhysX scene.
	// PxSceneDesc sceneDesc(*physics->getTolerancesScale());
	// sceneDesc.gravity = physx::PxVec3(p_desc.gravity.x, p_desc.gravity.y, p_desc.gravity.z);
	// return physics->createScene(sceneDesc);
#endif
	return memnew(char[1]);
}

void PhysXBackend::destroy_scene(void *p_scene) {
	if (p_scene) {
#ifdef PHYSX_ENABLED
		// static_cast<PxScene *>(p_scene)->release();
#endif
		memdelete(p_scene);
	}
}
