/**************************************************************************/
/*  lua_script.h                                                          */
/**************************************************************************/
/*  Lua script module for KaizenEngine.                                   */
/**************************************************************************/

#pragma once

#include "lua_bridge.h"
#include "lua_script_metadata.h"

#include "core/io/resource.h"
#include "core/object/script_instance.h"
#include "core/object/script_language.h"

class LuaScriptInstance;

// A Lua source file used as a Godot Script. Parses the `--@` annotations once on load and
// hands each object its own LuaScriptInstance.
class LuaScript : public Script {
	GDCLASS(LuaScript, Script);
	friend class LuaScriptInstance;
	friend class LuaLanguage;
	friend class ResourceFormatLoaderLua;

	String source;
	String path;
	LuaScriptMetadata::Metadata metadata;
	bool valid = false;

	void _update_metadata();

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
	const LuaScriptMetadata::Metadata &get_metadata() const { return metadata; }

	// Syntax-checks `p_code`; on failure, stores the message in `r_error` and returns false.
	static bool check_syntax(const String &p_code, const String &p_path, String &r_error);

	LuaScript();
	virtual ~LuaScript() override;
};

// The per-object state: a Lua environment table whose fields are the object's members.
// Lookups fall through to the shared globals of the LuaLanguage state.
class LuaScriptInstance : public ScriptInstance {
	friend class LuaScript;

	Ref<LuaScript> script;
	Object *owner = nullptr;
	int env_ref = LUA_NOREF;
	bool initialized = false;

	bool _run_chunk();
	bool _push_env(lua_State *L) const;
	void _apply_export_defaults(lua_State *L);
	// Calls `p_name` if the environment defines it as a function. Returns false when absent.
	bool _call_member(const StringName &p_name, const Variant **p_args, int p_argcount, Variant &r_ret, Callable::CallError &r_error);

public:
	LuaScriptInstance(const Ref<LuaScript> &p_script, Object *p_owner);
	virtual ~LuaScriptInstance() override;

	bool init();

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
