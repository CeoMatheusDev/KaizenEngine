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
}

PhysXBackend::~PhysXBackend() {
	shutdown();
}

bool PhysXBackend::is_supported() const {
#ifdef PHYSX_ENABLED
	return supported;
#else
	return false;
#endif
}

bool PhysXBackend::init() {
#ifdef PHYSX_ENABLED
	// Initialize the PhysX stack here once the SDK is wired in:
	//   foundation = PxCreateFoundation(...);
	//   physics = PxCreatePhysics(...);
	//   cooking = PxCreateCooking(...) (when config.cooking);
	//   dispatcher = PxDefaultCpuDispatcherCreate(...);
	//   controller_manager = PxCreateControllerManager(...) (when config.character_controller);
	//   PxInitVehicleSDK(...) (when config.vehicle);
	// Handles are owned as opaque pointers until then.

	supported = true;
	return true;
#else
	return false;
#endif
}

void PhysXBackend::shutdown() {
#ifdef PHYSX_ENABLED
	// Release the PhysX stack in reverse creation order once the SDK is
	// wired in (vehicle manager, controller manager, dispatcher, cooking,
	// physics, foundation). Opaque handles are simply dropped for now.
#endif
	foundation = nullptr;
	physics = nullptr;
	cooking = nullptr;
	dispatcher = nullptr;
	controller_manager = nullptr;
	vehicle_manager = nullptr;
	supported = false;
}

bool PhysXBackend::simulate(float p_step) {
	if (!supported) {
		return false;
	}

#ifdef PHYSX_ENABLED
	// scene->simulate(p_step); for each created scene.
	(void)p_step;
	return true;
#else
	(void)p_step;
	return false;
#endif
}

bool PhysXBackend::fetch_results(bool p_wait) {
	if (!supported) {
		return false;
	}

#ifdef PHYSX_ENABLED
	// scene->fetchResults(p_wait); for each created scene.
	(void)p_wait;
	return true;
#else
	(void)p_wait;
	return false;
#endif
}

void *PhysXBackend::create_scene(const SceneDesc &p_desc) {
	if (!supported) {
		return nullptr;
	}

#ifdef PHYSX_ENABLED
	// Create a PhysX scene:
	//   PxSceneDesc scene_desc(physics->getTolerancesScale());
	//   scene_desc.gravity = PxVec3(p_desc.gravity.x, p_desc.gravity.y, p_desc.gravity.z);
	//   scene_desc.cpuDispatcher = dispatcher;
	//   scene_desc.filterShader = PxDefaultSimulationFilterShader;
	//   return physics->createScene(scene_desc);
	(void)p_desc;
	return nullptr;
#else
	(void)p_desc;
	return nullptr;
#endif
}

void *PhysXBackend::create_scene(const Vector3 &p_gravity) {
	SceneDesc desc;
	desc.gravity = p_gravity;
	return create_scene(desc);
}

void PhysXBackend::destroy_scene(void *p_scene) {
	if (!p_scene) {
		return;
	}
#ifdef PHYSX_ENABLED
	// static_cast<physx::PxScene *>(p_scene)->release();
#endif
}

void PhysXBackend::cleanup() {
	shutdown();
}
