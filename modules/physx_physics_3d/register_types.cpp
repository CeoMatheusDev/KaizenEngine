/**************************************************************************/
/*  register_types.cpp                                                    */
/**************************************************************************/
/*  NVIDIA PhysX 3D physics module for KaizenEngine.                      */
/**************************************************************************/

#include "register_types.h"

#include "core/config/project_settings.h"
#include "core/object/callable_mp.h"
#include "servers/physics_3d/physics_server_3d.h"
#include "servers/physics_3d/physics_server_3d_manager.h"

#include "physx_physics_server_3d.h"

static PhysicsServer3D *_create_physx_physics_server() {
	// Real PhysX 5 backed PhysicsServer3D.
	return memnew(PhysXPhysicsServer3D);
}

void initialize_physx_physics_3d_module(ModuleInitializationLevel p_level) {
	if (p_level != MODULE_INITIALIZATION_LEVEL_SERVERS) {
		return;
	}

	// PhysX backend configuration (used by PhysXPhysicsServer3D::init()).
	GLOBAL_DEF(PropertyInfo(Variant::BOOL, "physics/physx_physics_3d/gpu_acceleration"), false);
	GLOBAL_DEF(PropertyInfo(Variant::BOOL, "physics/physx_physics_3d/cooking"), true);
	GLOBAL_DEF(PropertyInfo(Variant::BOOL, "physics/physx_physics_3d/character_controller"), false);
	GLOBAL_DEF(PropertyInfo(Variant::FLOAT, "physics/physx_physics_3d/simulation_step", PROPERTY_HINT_RANGE, "0.001,0.1,0.0001"), 1.0 / 60.0);
	GLOBAL_DEF(PropertyInfo(Variant::INT, "physics/physx_physics_3d/max_sub_steps", PROPERTY_HINT_RANGE, "1,16,1"), 4);
	GLOBAL_DEF(PropertyInfo(Variant::BOOL, "physics/physx_physics_3d/broadphase_mbp"), true);

	if (!PhysicsServer3DManager::get_singleton()) {
		return;
	}

	PhysicsServer3DManager::get_singleton()->register_server(PhysicsServer3DManager::PHYSX_PHYSICS_NAME, callable_mp_static(_create_physx_physics_server));
}

void uninitialize_physx_physics_3d_module(ModuleInitializationLevel p_level) {
	if (p_level != MODULE_INITIALIZATION_LEVEL_SERVERS) {
		return;
	}
}
