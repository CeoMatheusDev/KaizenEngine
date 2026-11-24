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
