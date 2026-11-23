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

#include "kaizen/physics/physx/physx_backend.h"

static PhysicsServer3D *_create_physx_physics_server() {
	// PhysX physics server creation.
	// When PhysX SDK is available, this would create a PhysicsServer3D
	// implementation that uses the PhysX backend internally.
	// For now, returns nullptr as the full implementation requires
	// the PhysX SDK and a complete PhysicsServer3D implementation.
	return nullptr;
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
