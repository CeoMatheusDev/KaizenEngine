/**************************************************************************/
/*  cpp_script.h                                                          */
/**************************************************************************/
/*  C++ script module for KaizenEngine.                                   */
/**************************************************************************/

#pragma once

#include "core/io/resource.h"
#include "core/object/script_instance.h"
#include "core/object/script_language.h"
#include "core/templates/hash_set.h"

class CppScriptInstance;

// Function pointers exported by a compiled script library. The layout is the C ABI declared
// by kaizen_cpp.h; the engine never sees the script's C++ types.
typedef void *(*CppCreateFn)();
typedef void (*CppDestroyFn)(void *);
typedef void (*CppLogFn)(const char *);
typedef int (*CppCallOwnerFn)(void *, const char *);
typedef void (*CppEventFn)(void *, int, double, void *, CppLogFn, CppCallOwnerFn);

struct CppLibrary {
	void *handle = nullptr;
	String file;
	CppCreateFn create = nullptr;
	CppDestroyFn destroy = nullptr;
	CppEventFn event = nullptr;

	bool is_loaded() const { return handle != nullptr; }
};

// A `.cpp` file used as a script. The source is compiled into a shared library, which is loaded
// on reload. Each object gets its own instance, which holds the state the script creates.
class CppScript : public Script {
	GDCLASS(CppScript, Script);
	friend class CppScriptInstance;
	friend class CppLanguage;
	friend class ResourceFormatLoaderCpp;

	String source;
	String path;
	bool valid = false;
	CppLibrary library;
	HashSet<CppScriptInstance *> instances;

	String _display_path() const;

protected:
	static void _bind_methods();

public:
	virtual bool can_instantiate() const override;
	virtual Ref<Script> get_base_script() const override;
	virtual StringName get_global_name() const override;
	virtual bool inherits_script(const Ref<Script> &p_script) const override;
	virtual StringName get_instance_base_type() const override;
	virtual ScriptInstance *instance_create(Object *p_this) override;
	virtual bool has_source_code() const override;
	virtual String get_source_code() const override;
	virtual void set_source_code(const String &p_code) override;
	virtual Error reload(bool p_keep_state = false) override;
	virtual StringName get_doc_class_name() const override;
	virtual Vector<DocData::ClassDoc> get_documentation() const override;
	virtual String get_class_icon_path() const override;
	virtual bool has_method(const StringName &p_method) const override;
	virtual MethodInfo get_method_info(const StringName &p_method) const override;
	virtual bool is_tool() const override;
	virtual bool is_script_valid() const override;
	virtual bool is_abstract() const override;
	virtual ScriptLanguage *get_language() const override;
	virtual bool has_script_signal(const StringName &p_signal) const override;
	virtual void get_script_signal_list(List<MethodInfo> *r_signals) const override;
	virtual bool get_property_default_value(const StringName &p_property, Variant &r_value) const override;
	virtual void get_script_method_list(List<MethodInfo> *r_list) const override;
	virtual void get_script_property_list(List<PropertyInfo> *r_list) const override;
	virtual const Variant get_rpc_config() const override;
	virtual void update_exports() override {}

	const String &get_path_string() const { return path; }

	CppScript();
	virtual ~CppScript() override;
};

// The per-object state. The compiled library creates the script object on construction; this
// class forwards the engine's lifecycle notifications to it.
class CppScriptInstance : public ScriptInstance {
	friend class CppScript;

	Ref<CppScript> script;
	Object *owner = nullptr;
	void *state = nullptr;
	bool entered = false;
	bool readied = false;

	void _create_state();
	void _destroy_state();
	void _dispatch(int p_event, double p_delta);

public:
	CppScriptInstance(const Ref<CppScript> &p_script, Object *p_owner);
	virtual ~CppScriptInstance() override;

	virtual bool set(const StringName &p_name, const Variant &p_value) override;
	virtual bool get(const StringName &p_name, Variant &r_ret) const override;
	virtual void get_property_list(List<PropertyInfo> *r_properties) const override;
	virtual Variant::Type get_property_type(const StringName &p_name, bool *r_is_valid = nullptr) const override;
	virtual void validate_property(PropertyInfo &p_property) const override;
	virtual bool property_can_revert(const StringName &p_name) const override;
	virtual bool property_get_revert(const StringName &p_name, Variant &r_ret) const override;
	virtual void get_method_list(List<MethodInfo> *r_list) const override;
	virtual bool has_method(const StringName &p_method) const override;
	virtual Variant callp(const StringName &p_method, const Variant **p_args, int p_argcount, Callable::CallError &r_error) override;
	virtual void notification(int p_notification, bool p_reversed = false) override;
	virtual Script *get_script() const override;
	virtual ScriptLanguage *get_language() override;
	virtual Object *get_owner() override { return owner; }
};
