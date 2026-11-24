/**************************************************************************/
/*  register_types.cpp                                                    */
/**************************************************************************/
/*  Lua script module for KaizenEngine.                                   */
/**************************************************************************/

#include "register_types.h"

void initialize_lua_script_module(ModuleInitializationLevel p_level) {
	if (p_level != MODULE_INITIALIZATION_LEVEL_SCENE) {
		return;
	}
	// The Lua script language (ScriptLanguage implementation, on par with
	// GDScript and C#) will be registered here in a later round. The bundled
	// Lua 5.5 library is already built into the engine by this module's SCsub.
}

void uninitialize_lua_script_module(ModuleInitializationLevel p_level) {
	if (p_level != MODULE_INITIALIZATION_LEVEL_SCENE) {
		return;
	}
}
