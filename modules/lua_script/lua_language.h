/**************************************************************************/
/*  lua_language.h                                                        */
/**************************************************************************/
/*  Lua script module for KaizenEngine.                                   */
/**************************************************************************/

#pragma once

#include "lua_bridge.h"

#include "core/object/script_language.h"
#include "core/templates/hash_set.h"

#ifdef TOOLS_ENABLED
#include "core/object/editor_language.h"
#endif // TOOLS_ENABLED

class LuaScript;

#ifdef TOOLS_ENABLED
// Editor support for the script editor: syntax validation and function lookup.
class LuaEditorLanguage : public EditorLanguage {
public:
	virtual bool validate(const String &p_code, const String &p_path, List<ScriptError> *r_errors, List<Warning> *r_warnings, List<String> *r_functions, HashSet<int> *r_safe_lines) const override;
	virtual int32_t find_function(const String &p_function, const String &p_code) const override;
};
#endif // TOOLS_ENABLED

// The Lua script language. Owns the single lua_State shared by all Lua scripts; every
// script instance runs in its own environment table inside that state.
class LuaLanguage : public ScriptLanguage {
	GDCLASS(LuaLanguage, ScriptLanguage);

	static LuaLanguage *singleton;

	lua_State *state = nullptr;
	bool closing = false;
	String last_error;
	HashSet<LuaScript *> scripts;

#ifdef TOOLS_ENABLED
	LuaEditorLanguage editor_language;
#endif // TOOLS_ENABLED

	void _open_libraries();
	void _install_globals();

public:
	static LuaLanguage *get_singleton();

	lua_State *get_state() const { return state; }
	bool is_closing() const { return closing; }

	// Stores the message for debug_get_error() and prints it as an engine error.
	void report_error(const String &p_message);

	void register_script(LuaScript *p_script);
	void unregister_script(LuaScript *p_script);

	static String get_global_class_name_from_file(const String &p_path, String *r_base_type, bool *r_is_tool);

	virtual String get_name() const override;

	/* LANGUAGE FUNCTIONS */
	virtual void init() override;
	virtual String get_type() const override;
	virtual String get_extension() const override;
	virtual void finish() override;

	/* EDITOR FUNCTIONS */
#ifdef TOOLS_ENABLED
	virtual EditorLanguage *get_editor_language() override;
#endif // TOOLS_ENABLED

	virtual Vector<String> get_reserved_words() const override;
	virtual bool is_control_flow_keyword(const String &p_string) const override;
	virtual Vector<String> get_comment_delimiters() const override;
	virtual Vector<String> get_doc_comment_delimiters() const override;
	virtual Vector<String> get_string_delimiters() const override;
	virtual Ref<Script> make_template(const String &p_template, const String &p_class_name, const String &p_base_class_name) const override;
	virtual Vector<ScriptTemplate> get_built_in_templates(const StringName &p_object) override;
	virtual bool is_using_templates() override { return false; }
	virtual bool supports_builtin_mode() const override { return false; }
	virtual String make_function(const String &p_class, const String &p_name, const PackedStringArray &p_args) const override;
	virtual void add_global_constant(const StringName &p_variable, const Variant &p_value) override;

	/* DEBUGGER FUNCTIONS */
	virtual String debug_get_error() const override;
	virtual int debug_get_stack_level_count() const override { return 0; }
	virtual int debug_get_stack_level_line(int p_level) const override { return -1; }
	virtual String debug_get_stack_level_function(int p_level) const override { return String(); }
	virtual String debug_get_stack_level_source(int p_level) const override { return String(); }
	virtual void debug_get_stack_level_locals(int p_level, List<String> *r_locals, List<Variant> *r_values, int p_max_subitems = -1, int p_max_depth = -1) override {}
	virtual void debug_get_stack_level_members(int p_level, List<String> *r_members, List<Variant> *r_values, int p_max_subitems = -1, int p_max_depth = -1) override {}
	virtual void debug_get_globals(List<String> *r_globals, List<Variant> *r_values, int p_max_subitems = -1, int p_max_depth = -1) override {}
	virtual String debug_parse_stack_level_expression(int p_level, const String &p_expression, int p_max_subitems = -1, int p_max_depth = -1) override { return String(); }

	virtual void reload_all_scripts() override;
	virtual void reload_scripts(const Array &p_scripts) override;
	virtual void reload_tool_script(const Ref<Script> &p_script) override;

	/* LOADER FUNCTIONS */
	virtual void get_public_functions(List<MethodInfo> *r_functions) const override {}
	virtual void get_public_constants(List<Pair<String, Variant>> *r_constants) const override {}
	virtual void get_public_annotations(List<MethodInfo> *r_annotations) const override {}

	/* PROFILING (not implemented yet) */
	virtual void profiling_start() override {}
	virtual void profiling_stop() override {}
	virtual void profiling_set_save_native_calls(bool p_enable) override {}
	virtual int profiling_get_accumulated_data(ProfilingInfo *r_info_arr, int p_info_max) override { return 0; }
	virtual int profiling_get_frame_data(ProfilingInfo *r_info_arr, int p_info_max) override { return 0; }

	virtual bool handles_global_class_type(const String &p_type) const override;
	virtual String get_global_class_name(const String &p_path, String *r_base_type = nullptr, String *r_icon_path = nullptr, bool *r_is_abstract = nullptr, bool *r_is_tool = nullptr) const override;

	LuaLanguage();
	virtual ~LuaLanguage() override;
};
