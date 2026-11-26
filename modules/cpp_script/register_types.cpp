/**************************************************************************/
/*  register_types.cpp                                                    */
/**************************************************************************/
/*  C++ script module for KaizenEngine.                                   */
/**************************************************************************/

#include "register_types.h"

#include "cpp_language.h"
#include "cpp_resource_format.h"
#include "cpp_script.h"

#include "core/config/engine.h"
#include "core/io/resource_loader.h"
#include "core/io/resource_saver.h"
#include "core/object/class_db.h"
#include "core/object/script_language.h"

static CppLanguage *script_language_cpp = nullptr;
static Ref<ResourceFormatLoaderCpp> resource_loader_cpp;
static Ref<ResourceFormatSaverCpp> resource_saver_cpp;

void initialize_cpp_script_module(ModuleInitializationLevel p_level) {
	if (p_level != MODULE_INITIALIZATION_LEVEL_SERVERS) {
		return;
	}
	GDREGISTER_CLASS(CppScript);

	script_language_cpp = memnew(CppLanguage);
	ScriptServer::register_language(script_language_cpp);

	resource_loader_cpp.instantiate();
	ResourceLoader::add_resource_format_loader(resource_loader_cpp);

	resource_saver_cpp.instantiate();
	ResourceSaver::add_resource_format_saver(resource_saver_cpp);
}

void uninitialize_cpp_script_module(ModuleInitializationLevel p_level) {
	if (p_level != MODULE_INITIALIZATION_LEVEL_SERVERS) {
		return;
	}
	if (resource_saver_cpp.is_valid()) {
		ResourceSaver::remove_resource_format_saver(resource_saver_cpp);
		resource_saver_cpp.unref();
	}
	if (resource_loader_cpp.is_valid()) {
		ResourceLoader::remove_resource_format_loader(resource_loader_cpp);
		resource_loader_cpp.unref();
	}
	if (script_language_cpp) {
		ScriptServer::unregister_language(script_language_cpp);
		memdelete(script_language_cpp);
		script_language_cpp = nullptr;
	}
}
