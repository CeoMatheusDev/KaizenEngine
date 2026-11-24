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

#include "core/os/memory.h"
#include "core/os/mutex.h"
#include "core/os/os.h"

#include <characterkinematic/PxControllerManager.h>
#include <extensions/PxDefaultCpuDispatcher.h>

using namespace physx;

// ---------------------------------------------------------------------------
// Simulation filter: layer/mask in words 0/1, owner pointer in words 2/3.
// ---------------------------------------------------------------------------

namespace {

Mutex exception_mutex;
HashMap<uint64_t, HashSet<uint64_t>> exception_pairs;

_FORCE_INLINE_ uint64_t _owner_key_from_filter(const PxFilterData &d) {
	return ((uint64_t)d.word3 << 32) | (uint64_t)d.word2;
}

} // namespace

PxFilterFlags PhysXBackend::filter_shader(PxFilterObjectAttributes p_attr_0, PxFilterData p_data_0, PxFilterObjectAttributes p_attr_1, PxFilterData p_data_1, PxPairFlags &r_pair_flags, const void *, PxU32) {
	if (PxFilterObjectIsTrigger(p_attr_0) || PxFilterObjectIsTrigger(p_attr_1)) {
		r_pair_flags = PxPairFlag::eTRIGGER_DEFAULT;
		return PxFilterFlags();
	}
	if (!((p_data_0.word0 & p_data_1.word1) || (p_data_1.word0 & p_data_0.word1))) {
		return PxFilterFlag::eSUPPRESS;
	}
	const uint64_t k0 = _owner_key_from_filter(p_data_0);
	const uint64_t k1 = _owner_key_from_filter(p_data_1);
	if (k0 && k1 && k0 != k1) {
		MutexLock lock(exception_mutex);
		const HashMap<uint64_t, HashSet<uint64_t>>::ConstIterator it = exception_pairs.find(k0);
		if (it && it->value.has(k1)) {
			return PxFilterFlag::eSUPPRESS;
		}
	}
	r_pair_flags = PxPairFlag::eSOLVE_CONTACT | PxPairFlag::eDETECT_DISCRETE_CONTACT | PxPairFlag::eNOTIFY_TOUCH_FOUND;
	return PxFilterFlags();
}

void PhysXBackend::add_collision_exception(const void *p_owner_a, const void *p_owner_b) {
	MutexLock lock(exception_mutex);
	exception_pairs[(uint64_t)(uintptr_t)p_owner_a].insert((uint64_t)(uintptr_t)p_owner_b);
	exception_pairs[(uint64_t)(uintptr_t)p_owner_b].insert((uint64_t)(uintptr_t)p_owner_a);
}

void PhysXBackend::remove_collision_exception(const void *p_owner_a, const void *p_owner_b) {
	MutexLock lock(exception_mutex);
	exception_pairs[(uint64_t)(uintptr_t)p_owner_a].erase((uint64_t)(uintptr_t)p_owner_b);
	exception_pairs[(uint64_t)(uintptr_t)p_owner_b].erase((uint64_t)(uintptr_t)p_owner_a);
}

// ---------------------------------------------------------------------------
// Internal helpers: allocator and error callbacks.
// ---------------------------------------------------------------------------

namespace {

class KaizenPxAllocator : public PxAllocatorCallback {
public:
	virtual void *allocate(size_t size, const char *, const char *, int) override {
		return Memory::alloc_static(size);
	}
	virtual void deallocate(void *ptr) override {
		Memory::free_static(ptr);
	}
};

class KaizenPxErrorCallback : public PxErrorCallback {
public:
	virtual void reportError(PxErrorCode::Enum code, const char *message, const char *file, int line) override {
		if (code & (PxErrorCode::eABORT | PxErrorCode::eINTERNAL_ERROR | PxErrorCode::eOUT_OF_MEMORY | PxErrorCode::eINVALID_OPERATION | PxErrorCode::eINVALID_PARAMETER)) {
			ERR_PRINT(vformat("PhysX error 0x%x at %s:%d: %s", (uint32_t)code, file, line, message));
		} else if (code & (PxErrorCode::eDEBUG_WARNING | PxErrorCode::ePERF_WARNING)) {
			WARN_PRINT(vformat("PhysX warning 0x%x at %s:%d: %s", (uint32_t)code, file, line, message));
		} else {
			print_verbose(vformat("PhysX info 0x%x at %s:%d: %s", (uint32_t)code, file, line, message));
		}
	}
};

KaizenPxAllocator px_allocator;
KaizenPxErrorCallback px_error_callback;

} // namespace

// ---------------------------------------------------------------------------
// Transform conversion.
// ---------------------------------------------------------------------------

PxTransform PhysXBackend::to_px(const Transform3D &p_t) {
	const Quaternion q = p_t.basis.get_quaternion();
	return PxTransform(to_px(p_t.origin), PxQuat(q.x, q.y, q.z, q.w));
}

Transform3D PhysXBackend::from_px(const PxTransform &p_t) {
	Transform3D t;
	t.basis = Basis(Quaternion(p_t.q.x, p_t.q.y, p_t.q.z, p_t.q.w));
	t.origin = from_px(p_t.p);
	return t;
}

// ---------------------------------------------------------------------------
// Lifecycle.
// ---------------------------------------------------------------------------

PhysXBackend::~PhysXBackend() {
	shutdown();
}

bool PhysXBackend::is_supported() const {
	return true;
}

bool PhysXBackend::init() {
	if (foundation) {
		return true;
	}

	foundation = PxCreateFoundation(PX_PHYSICS_VERSION, px_allocator, px_error_callback);
	ERR_FAIL_NULL_V_MSG(foundation, false, "PhysX: PxCreateFoundation failed.");

	PxTolerancesScale scale;
	physics = PxCreatePhysics(PX_PHYSICS_VERSION, *static_cast<PxFoundation *>(foundation), scale, false, nullptr);
	ERR_FAIL_NULL_V_MSG(physics, false, "PhysX: PxCreatePhysics failed.");

	if (!PxInitExtensions(*static_cast<PxPhysics *>(physics), nullptr)) {
		ERR_PRINT("PhysX: PxInitExtensions failed.");
		shutdown();
		return false;
	}

	const uint32_t worker_count = MAX(1u, (uint32_t)OS::get_singleton()->get_processor_count() - 1u);
	dispatcher = PxDefaultCpuDispatcherCreate(worker_count);
	ERR_FAIL_NULL_V_MSG(dispatcher, false, "PhysX: PxDefaultCpuDispatcherCreate failed.");

	print_line(vformat("PhysX %d initialized (workers: %d, cooking: %s)", (int)PX_PHYSICS_VERSION, (int)worker_count, config.cooking ? "on" : "off"));
	return true;
}

void PhysXBackend::shutdown() {
	cleanup();

	if (physics) {
		PxCloseExtensions();
		static_cast<PxPhysics *>(physics)->release();
		physics = nullptr;
	}
	if (foundation) {
		static_cast<PxFoundation *>(foundation)->release();
		foundation = nullptr;
	}
}

void PhysXBackend::cleanup() {
	// Release order: controllers -> scenes -> dispatcher.
	for (int i = 0; i < controller_managers.size(); i++) {
		PxControllerManager *mgr = static_cast<PxControllerManager *>(controller_managers[i]);
		if (mgr) {
			mgr->release();
		}
	}
	controller_managers.clear();
	controller_manager = nullptr;

	for (int i = 0; i < scenes.size(); i++) {
		PxScene *scene = static_cast<PxScene *>(scenes[i]);
		if (scene) {
			scene->release();
		}
	}
	scenes.clear();

	if (dispatcher) {
		static_cast<PxDefaultCpuDispatcher *>(dispatcher)->release();
		dispatcher = nullptr;
	}
	step_accumulator = 0;
}

// ---------------------------------------------------------------------------
// Scenes and stepping.
// ---------------------------------------------------------------------------

void *PhysXBackend::create_scene(const Vector3 &p_gravity) {
	PxPhysics *px = get_physics();
	ERR_FAIL_NULL_V_MSG(px, nullptr, "PhysX: backend not initialized.");

	PxSceneDesc scene_desc(px->getTolerancesScale());
	scene_desc.gravity = to_px(p_gravity);
	scene_desc.cpuDispatcher = static_cast<PxDefaultCpuDispatcher *>(dispatcher);
	scene_desc.filterShader = PhysXBackend::filter_shader;
	if (!scene_desc.isValid()) {
		ERR_PRINT("PhysX: invalid scene description.");
		return nullptr;
	}

	PxScene *scene = px->createScene(scene_desc);
	ERR_FAIL_NULL_V_MSG(scene, nullptr, "PhysX: createScene failed.");
	scenes.push_back(scene);

	if (config.character_controller) {
		PxControllerManager *mgr = PxCreateControllerManager(*scene);
		if (mgr) {
			controller_managers.push_back(mgr);
			if (!controller_manager) {
				controller_manager = mgr;
			}
		} else {
			WARN_PRINT("PhysX: character controller manager creation failed.");
			controller_managers.push_back(nullptr);
		}
	} else {
		controller_managers.push_back(nullptr);
	}
	return scene;
}

void PhysXBackend::destroy_scene(void *p_scene) {
	if (!p_scene) {
		return;
	}
	PxScene *scene = static_cast<PxScene *>(p_scene);

	// Release the controller manager tied to this scene first.
	const int scene_idx = scenes.find(scene);
	if (scene_idx >= 0 && scene_idx < controller_managers.size()) {
		PxControllerManager *mgr = static_cast<PxControllerManager *>(controller_managers[scene_idx]);
		if (mgr) {
			mgr->release();
		}
		controller_managers.remove_at(scene_idx);
		controller_manager = nullptr;
		for (int i = 0; i < controller_managers.size(); i++) {
			if (controller_managers[i]) {
				controller_manager = controller_managers[i];
				break;
			}
		}
	}
	if (scene_idx >= 0) {
		scenes.remove_at(scene_idx);
	}
	scene->release();
}

bool PhysXBackend::simulate(float p_step) {
	if (scenes.is_empty()) {
		return true;
	}

	// Fixed-step accumulation with a sub-step cap (avoids the spiral of death).
	const real_t fixed_step = config.simulation_step;
	step_accumulator += p_step;
	int sub_steps = 0;
	while (step_accumulator >= fixed_step && sub_steps < config.max_sub_steps) {
		for (int i = 0; i < scenes.size(); i++) {
			PxScene *scene = static_cast<PxScene *>(scenes[i]);
			scene->simulate(fixed_step);
		}
		step_accumulator -= fixed_step;
		sub_steps++;
	}
	if (sub_steps == config.max_sub_steps) {
		// Drop excess accumulated time to stay real-time.
		step_accumulator = MIN(step_accumulator, fixed_step);
	}
	return true;
}

bool PhysXBackend::fetch_results(bool p_wait) {
	bool all_ok = true;
	for (int i = 0; i < scenes.size(); i++) {
		PxScene *scene = static_cast<PxScene *>(scenes[i]);
		if (p_wait || scene->checkResults()) {
			if (!scene->fetchResults(p_wait)) {
				all_ok = false;
			}
		}
	}
	return all_ok;
}
