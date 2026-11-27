/**************************************************************************/
/*  cpp_resource_format.cpp                                               */
/**************************************************************************/
/*  C++ script module for KaizenEngine.                                   */
/**************************************************************************/

#include "cpp_resource_format.h"

#include "cpp_script.h"

#include "core/io/file_access.h"

Ref<Resource> ResourceFormatLoaderCpp::load(const String &p_path, const String &p_original_path, Error *r_error, bool p_use_sub_threads, float *r_progress, CacheMode p_cache_mode) {
	Error err = OK;
	const String source = FileAccess::get_file_as_string(p_path, &err);
	if (r_error) {
		*r_error = err;
	}
	if (err != OK) {
		return Ref<Resource>();
	}

	Ref<CppScript> script;
	script.instantiate();
	script->path = p_path;
	script->set_source_code(source);
	// A compile error is reported by reload() and leaves the script invalid. The script still loads,
	// so the editor can show the file and the user can fix it.
	script->reload(false);
	return script;
}

void ResourceFormatLoaderCpp::get_recognized_extensions(List<String> *p_extensions) const {
	p_extensions->push_back("cpp");
}

bool ResourceFormatLoaderCpp::handles_type(const String &p_type) const {
	return p_type == "Script" || p_type == "CppScript";
}

String ResourceFormatLoaderCpp::get_resource_type(const String &p_path) const {
	return p_path.get_extension().to_lower() == "cpp" ? "CppScript" : "";
}

Error ResourceFormatSaverCpp::save(const Ref<Resource> &p_resource, const String &p_path, uint32_t p_flags) {
	Ref<CppScript> script = p_resource;
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

void ResourceFormatSaverCpp::get_recognized_extensions(const Ref<Resource> &p_resource, List<String> *p_extensions) const {
	if (Object::cast_to<CppScript>(p_resource.ptr())) {
		p_extensions->push_back("cpp");
	}
}

bool ResourceFormatSaverCpp::recognize(const Ref<Resource> &p_resource) const {
	return Object::cast_to<CppScript>(p_resource.ptr()) != nullptr;
}
