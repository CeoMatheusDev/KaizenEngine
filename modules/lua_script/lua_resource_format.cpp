/**************************************************************************/
/*  lua_resource_format.cpp                                               */
/**************************************************************************/
/*  Lua script module for KaizenEngine.                                   */
/**************************************************************************/

#include "lua_resource_format.h"

#include "lua_script.h"

#include "core/io/file_access.h"

Ref<Resource> ResourceFormatLoaderLua::load(const String &p_path, const String &p_original_path, Error *r_error, bool p_use_sub_threads, float *r_progress, CacheMode p_cache_mode) {
	Error err = OK;
	const String source = FileAccess::get_file_as_string(p_path, &err);
	if (r_error) {
		*r_error = err;
	}
	if (err != OK) {
		return Ref<Resource>();
	}

	Ref<LuaScript> script;
	script.instantiate();
	script->path = p_path;
	script->set_source_code(source);
	if (r_error) {
		*r_error = script->is_script_valid() ? OK : ERR_PARSE_ERROR;
	}
	return script;
}

void ResourceFormatLoaderLua::get_recognized_extensions(List<String> *p_extensions) const {
	p_extensions->push_back("lua");
}

bool ResourceFormatLoaderLua::handles_type(const String &p_type) const {
	return p_type == "Script" || p_type == "LuaScript";
}

String ResourceFormatLoaderLua::get_resource_type(const String &p_path) const {
	return p_path.get_extension().to_lower() == "lua" ? "LuaScript" : "";
}

Error ResourceFormatSaverLua::save(const Ref<Resource> &p_resource, const String &p_path, uint32_t p_flags) {
	Ref<LuaScript> script = p_resource;
	if (script.is_null()) {
		return ERR_INVALID_PARAMETER;
	}
	Ref<FileAccess> file = FileAccess::open(p_path, FileAccess::WRITE);
	if (file.is_null()) {
		return FileAccess::get_open_error();
	}
	file->store_string(script->get_source_code());
	return OK;
}

void ResourceFormatSaverLua::get_recognized_extensions(const Ref<Resource> &p_resource, List<String> *p_extensions) const {
	if (Object::cast_to<LuaScript>(p_resource.ptr())) {
		p_extensions->push_back("lua");
	}
}

bool ResourceFormatSaverLua::recognize(const Ref<Resource> &p_resource) const {
	return Object::cast_to<LuaScript>(p_resource.ptr()) != nullptr;
}
