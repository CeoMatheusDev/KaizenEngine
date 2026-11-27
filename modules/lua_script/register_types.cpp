/**************************************************************************/
/*  register_types.cpp                                                    */
/**************************************************************************/
/*  Lua script module for KaizenEngine.                                   */
/**************************************************************************/

#include "register_types.h"

#include "lua_language.h"
#include "lua_resource_format.h"
#include "lua_script.h"

#include "core/config/engine.h"
#include "core/io/resource_loader.h"
#include "core/io/resource_saver.h"
#include "core/object/class_db.h"
#include "core/object/script_language.h"

static LuaLanguage *script_language_lua = nullptr;
static Ref<ResourceFormatLoaderLua> resource_loader_lua;
static Ref<ResourceFormatSaverLua> resource_saver_lua;

void initialize_lua_script_module(ModuleInitializationLevel p_level) {
	if (p_level != MODULE_INITIALIZATION_LEVEL_SERVERS) {
		return;
	}
	GDREGISTER_CLASS(LuaScript);

	script_language_lua = memnew(LuaLanguage);
	ScriptServer::register_language(script_language_lua);

	resource_loader_lua.instantiate();
	ResourceLoader::add_resource_format_loader(resource_loader_lua);

	resource_saver_lua.instantiate();
	ResourceSaver::add_resource_format_saver(resource_saver_lua);
}

void uninitialize_lua_script_module(ModuleInitializationLevel p_level) {
	if (p_level != MODULE_INITIALIZATION_LEVEL_SERVERS) {
		return;
	}
	if (resource_saver_lua.is_valid()) {
		ResourceSaver::remove_resource_format_saver(resource_saver_lua);
		resource_saver_lua.unref();
	}
	if (resource_loader_lua.is_valid()) {
		ResourceLoader::remove_resource_format_loader(resource_loader_lua);
		resource_loader_lua.unref();
	}
	if (script_language_lua) {
		ScriptServer::unregister_language(script_language_lua);
		memdelete(script_language_lua);
		script_language_lua = nullptr;
	}
}
