/**************************************************************************/
/*  cpp_resource_format.h                                                 */
/**************************************************************************/
/*  C++ script module for KaizenEngine.                                   */
/**************************************************************************/

#pragma once

#include "core/io/resource_loader.h"
#include "core/io/resource_saver.h"

// Loads `.cpp` files as CppScript resources. Loading compiles the file, so the script can be
// instantiated as soon as the resource is available.
class ResourceFormatLoaderCpp : public ResourceFormatLoader {
public:
	virtual Ref<Resource> load(const String &p_path, const String &p_original_path = "", Error *r_error = nullptr, bool p_use_sub_threads = false, float *r_progress = nullptr, CacheMode p_cache_mode = CACHE_MODE_REUSE) override;
	virtual void get_recognized_extensions(List<String> *p_extensions) const override;
	virtual bool handles_type(const String &p_type) const override;
	virtual String get_resource_type(const String &p_path) const override;
};

// Writes CppScript resources back to `.cpp` files.
class ResourceFormatSaverCpp : public ResourceFormatSaver {
public:
	virtual Error save(const Ref<Resource> &p_resource, const String &p_path, uint32_t p_flags = 0) override;
	virtual void get_recognized_extensions(const Ref<Resource> &p_resource, List<String> *p_extensions) const override;
	virtual bool recognize(const Ref<Resource> &p_resource) const override;
};
