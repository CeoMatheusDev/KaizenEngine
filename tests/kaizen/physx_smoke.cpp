//************************************************************************//
//  physx_smoke.cpp                                                       //
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

// Runtime smoke test for the NVIDIA PhysX SDK build.
//
// Creates a real PhysX scene, drops a dynamic box onto a static plane and
// steps the simulation; the box must land and settle under gravity. This
// proves the SDK objects the engine links against actually simulate.

#include "PxPhysicsAPI.h"

#include <cstdio>
#include <cstdlib>
#include <cmath>

using namespace physx;

static PxFilterFlags smoke_filter_shader(
        PxFilterObjectAttributes attributes0, PxFilterData filter_data0,
        PxFilterObjectAttributes attributes1, PxFilterData filter_data1,
        PxPairFlags &pair_flags, const void *constant_block, PxU32 constant_block_size) {
	(void)attributes0;
	(void)filter_data0;
	(void)attributes1;
	(void)filter_data1;
	(void)constant_block;
	(void)constant_block_size;
	pair_flags = PxPairFlag::eCONTACT_DEFAULT | PxPairFlag::eDETECT_DISCRETE_CONTACT;
	return PxFilterFlag::eDEFAULT;
}

static int failures = 0;

static void check(bool ok, const char *what) {
	std::printf("  [%s] %s\n", ok ? "ok" : "FAIL", what);
	if (!ok) {
		failures++;
	}
}

int main() {
	PxDefaultAllocator allocator;
	PxDefaultErrorCallback error_callback;

	PxFoundation *foundation = PxCreateFoundation(PX_PHYSICS_VERSION, allocator, error_callback);
	check(foundation != nullptr, "PxCreateFoundation");
	if (!foundation) {
		return 1;
	}

	PxPhysics *physics = PxCreatePhysics(PX_PHYSICS_VERSION, *foundation, PxTolerancesScale(), false, nullptr);
	check(physics != nullptr, "PxCreatePhysics");
	if (!physics) {
		foundation->release();
		return 1;
	}

	PxSceneDesc scene_desc(physics->getTolerancesScale());
	scene_desc.gravity = PxVec3(0.0f, -9.81f, 0.0f);
	PxDefaultCpuDispatcher *dispatcher = PxDefaultCpuDispatcherCreate(2);
	scene_desc.cpuDispatcher = dispatcher;
	check(dispatcher != nullptr, "PxDefaultCpuDispatcherCreate");
	scene_desc.filterShader = smoke_filter_shader;
	scene_desc.broadPhaseType = PxBroadPhaseType::eABP;

	PxScene *scene = physics->createScene(scene_desc);
	check(scene != nullptr, "physics->createScene");
	if (!scene) {
		physics->release();
		foundation->release();
		return 1;
	}

	PxMaterial *material = physics->createMaterial(0.5f, 0.5f, 0.6f);
	check(material != nullptr, "physics->createMaterial");

	PxRigidStatic *ground = PxCreatePlane(*physics, PxPlane(0.0f, 1.0f, 0.0f, 0.0f), *material);
	check(ground != nullptr, "PxCreatePlane (static ground)");
	if (ground) {
		scene->addActor(*ground);
	}

	const PxReal half_extent = 0.5f;
	PxRigidDynamic *box = PxCreateDynamic(*physics, PxTransform(PxVec3(0.0f, 3.0f, 0.0f)),
			PxBoxGeometry(half_extent, half_extent, half_extent), *material, 1.0f);
	check(box != nullptr, "PxCreateDynamic (falling box)");
	if (box) {
		scene->addActor(*box);
	}

	// Simulate 5 seconds at 60 Hz.
	const int steps = 300;
	bool stepped = true;
	for (int i = 0; i < steps; i++) {
		scene->simulate(1.0f / 60.0f);
		if (!scene->fetchResults(true)) {
			stepped = false;
			break;
		}
	}
	check(stepped, "simulate/fetchResults for 300 steps");

	if (box) {
		const PxReal y = box->getGlobalPose().p.y;
		const PxReal speed = box->getLinearVelocity().magnitude();
		std::printf("  box settled at y=%.3f (expect ~%.2f), |v|=%.3f\n", y, double(half_extent), double(speed));
		// The box must rest on the plane (top face at y=0), not tunnel or fly.
		check(y > half_extent - 0.2f && y < half_extent + 0.2f, "box rests on the plane");
		check(speed < 0.5f, "box comes to rest");
	}

	// Cleanup must not crash or leak.
	if (box) {
		box->release();
	}
	if (ground) {
		ground->release();
	}
	if (material) {
		material->release();
	}
	scene->release();
	if (dispatcher) {
		dispatcher->release();
	}
	physics->release();
	foundation->release();
	check(true, "cleanup");

	if (failures == 0) {
		std::printf("physx_smoke: ALL CHECKS PASSED\n");
		return 0;
	}
	std::printf("physx_smoke: %d FAILURE(S)\n", failures);
	return 1;
}
