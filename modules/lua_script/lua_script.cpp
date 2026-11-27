/**************************************************************************/
/*  lua_script.cpp                                                        */
/**************************************************************************/
/*  Lua script module for KaizenEngine.                                   */
/**************************************************************************/

#include "lua_script.h"

#include "lua_language.h"

#include "core/io/file_access.h"
#include "scene/main/node.h"

namespace {

constexpr const char *SELF_NAME = "self";

// Maps the type named in an `--@export name: type` annotation to a Variant type. Unknown or
// missing names fall back to NIL, which means "any value".
Variant::Type type_from_annotation(const std::string &p_name) {
	static const struct {
		const char *name;
		Variant::Type type;
	} table[] = {
		{ "bool", Variant::BOOL },
		{ "int", Variant::INT },
		{ "float", Variant::FLOAT },
		{ "string", Variant::STRING },
		{ "String", Variant::STRING },
		{ "Vector2", Variant::VECTOR2 },
		{ "Vector3", Variant::VECTOR3 },
		{ "Color", Variant::COLOR },
		{ "Array", Variant::ARRAY },
		{ "Dictionary", Variant::DICTIONARY },
	};
	for (const auto &entry : table) {
		if (p_name == entry.name) {
			return entry.type;
		}
	}
	return Variant::NIL;
}

// Pushes `p_name` as a Lua string key.
void push_key(lua_State *L, const StringName &p_name) {
	const CharString utf8 = String(p_name).utf8();
	lua_pushlstring(L, utf8.get_data(), size_t(utf8.length()));
}

} // namespace

// ---------------------------------------------------------------------------------------
// LuaScript
// ---------------------------------------------------------------------------------------

void LuaScript::_bind_methods() {
}

LuaScript::LuaScript() {
	if (LuaLanguage *language = LuaLanguage::get_singleton()) {
		language->register_script(this);
	}
}

LuaScript::~LuaScript() {
	if (LuaLanguage *language = LuaLanguage::get_singleton()) {
		language->unregister_script(this);
	}
}

void LuaScript::_update_metadata() {
	const std::string utf8_source = LuaBridge::to_std_utf8(source);
	metadata = LuaScriptMetadata::parse(utf8_source);
	String error;
	valid = check_syntax(source, path, error);
	if (!valid) {
		LuaLanguage::get_singleton()->report_error(error);
	}
}

bool LuaScript::check_syntax(const String &p_code, const String &p_path, String &r_error) {
	lua_State *L = LuaLanguage::get_singleton() ? LuaLanguage::get_singleton()->get_state() : nullptr;
	if (!L) {
		r_error = "The Lua runtime is not initialized.";
		return false;
	}
	const std::string utf8_code = LuaBridge::to_std_utf8(p_code);
	const std::string chunk_name = "@" + LuaBridge::to_std_utf8(p_path.is_empty() ? String("script.lua") : p_path);
	const int top = lua_gettop(L);
	const int status = luaL_loadbufferx(L, utf8_code.data(), utf8_code.size(), chunk_name.c_str(), "t");
	if (status != LUA_OK) {
		r_error = LuaBridge::top_string(L);
		lua_settop(L, top);
		return false;
	}
	lua_settop(L, top);
	return true;
}

bool LuaScript::can_instantiate() const {
	return valid;
}

Ref<Script> LuaScript::get_base_script() const {
	return Ref<Script>();
}

StringName LuaScript::get_global_name() const {
	return StringName(LuaBridge::from_std_utf8(metadata.class_name));
}

bool LuaScript::inherits_script(const Ref<Script> &p_script) const {
	return false;
}

StringName LuaScript::get_instance_base_type() const {
	if (metadata.extends.empty()) {
		return StringName("Object");
	}
	return StringName(LuaBridge::from_std_utf8(metadata.extends));
}

ScriptInstance *LuaScript::instance_create(Object *p_this) {
	LuaScriptInstance *instance = memnew(LuaScriptInstance(Ref<LuaScript>(this), p_this));
	instance->init();
	return instance;
}

bool LuaScript::has_source_code() const {
	return true;
}

String LuaScript::get_source_code() const {
	return source;
}

void LuaScript::set_source_code(const String &p_code) {
	if (source == p_code) {
		return;
	}
	source = p_code;
	_update_metadata();
}

Error LuaScript::reload(bool p_keep_state) {
	_update_metadata();
	return valid ? OK : ERR_PARSE_ERROR;
}

StringName LuaScript::get_doc_class_name() const {
	return get_global_name();
}

Vector<DocData::ClassDoc> LuaScript::get_documentation() const {
	return Vector<DocData::ClassDoc>();
}

String LuaScript::get_class_icon_path() const {
	return String();
}

bool LuaScript::has_method(const StringName &p_method) const {
	const std::string wanted = LuaBridge::to_std_utf8(String(p_method));
	for (const LuaScriptMetadata::Function &function : metadata.functions) {
		if (function.name == wanted) {
			return true;
		}
	}
	return false;
}

MethodInfo LuaScript::get_method_info(const StringName &p_method) const {
	const std::string wanted = LuaBridge::to_std_utf8(String(p_method));
	for (const LuaScriptMetadata::Function &function : metadata.functions) {
		if (function.name != wanted) {
			continue;
		}
		MethodInfo info;
		info.name = p_method;
		for (const std::string &param : function.params) {
			info.arguments.push_back(PropertyInfo(Variant::NIL, LuaBridge::from_std_utf8(param)));
		}
		return info;
	}
	return MethodInfo();
}

bool LuaScript::is_tool() const {
	return metadata.tool;
}

bool LuaScript::is_script_valid() const {
	return valid;
}

bool LuaScript::is_abstract() const {
	return false;
}

ScriptLanguage *LuaScript::get_language() const {
	return LuaLanguage::get_singleton();
}

bool LuaScript::has_script_signal(const StringName &p_signal) const {
	const std::string wanted = LuaBridge::to_std_utf8(String(p_signal));
	for (const LuaScriptMetadata::Signal &signal : metadata.signals) {
		if (signal.name == wanted) {
			return true;
		}
	}
	return false;
}

void LuaScript::get_script_signal_list(List<MethodInfo> *r_signals) const {
	for (const LuaScriptMetadata::Signal &signal : metadata.signals) {
		MethodInfo info;
		info.name = LuaBridge::from_std_utf8(signal.name);
		for (const std::string &param : signal.params) {
			info.arguments.push_back(PropertyInfo(Variant::NIL, LuaBridge::from_std_utf8(param)));
		}
		r_signals->push_back(info);
	}
}

bool LuaScript::get_property_default_value(const StringName &p_property, Variant &r_value) const {
	return false;
}

void LuaScript::get_script_method_list(List<MethodInfo> *r_list) const {
	for (const LuaScriptMetadata::Function &function : metadata.functions) {
		MethodInfo info;
		info.name = LuaBridge::from_std_utf8(function.name);
		for (const std::string &param : function.params) {
			info.arguments.push_back(PropertyInfo(Variant::NIL, LuaBridge::from_std_utf8(param)));
		}
		r_list->push_back(info);
	}
}

void LuaScript::get_script_property_list(List<PropertyInfo> *r_list) const {
	for (const LuaScriptMetadata::Export &export_info : metadata.exports) {
		r_list->push_back(PropertyInfo(type_from_annotation(export_info.type), LuaBridge::from_std_utf8(export_info.name), PROPERTY_HINT_NONE, "", PROPERTY_USAGE_DEFAULT | PROPERTY_USAGE_SCRIPT_VARIABLE));
	}
}

const Variant LuaScript::get_rpc_config() const {
	return Dictionary();
}

// ---------------------------------------------------------------------------------------
// LuaScriptInstance
// ---------------------------------------------------------------------------------------

LuaScriptInstance::LuaScriptInstance(const Ref<LuaScript> &p_script, Object *p_owner) :
		script(p_script),
		owner(p_owner) {
}

LuaScriptInstance::~LuaScriptInstance() {
	LuaLanguage *language = LuaLanguage::get_singleton();
	if (language && language->get_state() && !language->is_closing() && env_ref != LUA_NOREF) {
		luaL_unref(language->get_state(), LUA_REGISTRYINDEX, env_ref);
	}
	env_ref = LUA_NOREF;
}

bool LuaScriptInstance::_push_env(lua_State *L) const {
	if (env_ref == LUA_NOREF) {
		return false;
	}
	lua_rawgeti(L, LUA_REGISTRYINDEX, env_ref);
	return true;
}

bool LuaScriptInstance::init() {
	LuaLanguage *language = LuaLanguage::get_singleton();
	lua_State *L = language ? language->get_state() : nullptr;
	if (!L || !script->valid) {
		return false;
	}

	// Environment: an empty table whose misses fall through to the shared globals.
	lua_newtable(L);
	lua_newtable(L);
	lua_rawgeti(L, LUA_REGISTRYINDEX, LUA_RIDX_GLOBALS);
	lua_setfield(L, -2, "__index");
	lua_setmetatable(L, -2);

	// `self` refers to the owner; it is not kept alive, because the owner holds this instance.
	lua_pushstring(L, SELF_NAME);
	LuaBridge::push_variant(L, owner ? Variant(owner) : Variant(), false);
	lua_rawset(L, -3);

	env_ref = luaL_ref(L, LUA_REGISTRYINDEX);
	if (!_run_chunk()) {
		return false;
	}
	initialized = true;

	Variant ret;
	Callable::CallError error;
	_call_member(StringName("_init"), nullptr, 0, ret, error);
	return true;
}

bool LuaScriptInstance::_run_chunk() {
	lua_State *L = LuaLanguage::get_singleton()->get_state();
	const int top = lua_gettop(L);
	const std::string utf8_source = LuaBridge::to_std_utf8(script->source);
	const std::string chunk_name = "@" + LuaBridge::to_std_utf8(script->path.is_empty() ? String("script.lua") : script->path);

	if (luaL_loadbufferx(L, utf8_source.data(), utf8_source.size(), chunk_name.c_str(), "t") != LUA_OK) {
		LuaLanguage::get_singleton()->report_error(LuaBridge::top_string(L));
		lua_settop(L, top);
		return false;
	}
	_push_env(L);
	lua_setupvalue(L, -2, 1); // The chunk's _ENV upvalue becomes this instance's environment.
	if (LuaBridge::pcall(L, 0, 0) != LUA_OK) {
		LuaLanguage::get_singleton()->report_error(LuaBridge::top_string(L));
		lua_settop(L, top);
		return false;
	}

	_apply_export_defaults(L);
	lua_settop(L, top);
	return true;
}

void LuaScriptInstance::_apply_export_defaults(lua_State *L) {
	for (const LuaScriptMetadata::Export &export_info : script->metadata.exports) {
		if (export_info.default_expr.empty()) {
			continue;
		}
		const CharString name = LuaBridge::from_std_utf8(export_info.name).utf8();
		// Keep a value the script already assigned itself.
		_push_env(L);
		lua_pushlstring(L, name.get_data(), size_t(name.length()));
		lua_rawget(L, -2);
		const bool already_set = !lua_isnil(L, -1);
		lua_pop(L, 2);
		if (already_set) {
			continue;
		}

		const std::string code = "return " + export_info.default_expr;
		const std::string chunk_name = "=export " + export_info.name;
		if (luaL_loadbufferx(L, code.data(), code.size(), chunk_name.c_str(), "t") != LUA_OK) {
			LuaLanguage::get_singleton()->report_error(LuaBridge::top_string(L));
			lua_pop(L, 1);
			continue;
		}
		_push_env(L);
		lua_setupvalue(L, -2, 1);
		if (LuaBridge::pcall(L, 0, 1) != LUA_OK) {
			LuaLanguage::get_singleton()->report_error(LuaBridge::top_string(L));
			lua_pop(L, 1);
			continue;
		}
		_push_env(L);
		lua_pushlstring(L, name.get_data(), size_t(name.length()));
		lua_pushvalue(L, -3);
		lua_rawset(L, -3);
		lua_pop(L, 2); // the environment and the evaluated value
	}
}

bool LuaScriptInstance::_call_member(const StringName &p_name, const Variant **p_args, int p_argcount, Variant &r_ret, Callable::CallError &r_error) {
	LuaLanguage *language = LuaLanguage::get_singleton();
	lua_State *L = language ? language->get_state() : nullptr;
	if (!L || !initialized) {
		return false;
	}
	const int top = lua_gettop(L);
	_push_env(L);
	push_key(L, p_name);
	lua_rawget(L, -2);
	if (!lua_isfunction(L, -1)) {
		lua_settop(L, top);
		return false;
	}
	lua_checkstack(L, p_argcount + 2);
	for (int i = 0; i < p_argcount; i++) {
		LuaBridge::push_variant(L, *p_args[i]);
	}
	if (LuaBridge::pcall(L, p_argcount, 1) != LUA_OK) {
		language->report_error(LuaBridge::top_string(L));
		lua_settop(L, top);
		r_error.error = Callable::CallError::CALL_OK;
		return true;
	}
	r_ret = LuaBridge::to_variant(L, -1);
	lua_settop(L, top);
	r_error.error = Callable::CallError::CALL_OK;
	return true;
}

bool LuaScriptInstance::set(const StringName &p_name, const Variant &p_value) {
	LuaLanguage *language = LuaLanguage::get_singleton();
	lua_State *L = language ? language->get_state() : nullptr;
	if (!L || !initialized) {
		return false;
	}
	const std::string name = LuaBridge::to_std_utf8(String(p_name));
	bool declared = false;
	for (const LuaScriptMetadata::Export &export_info : script->metadata.exports) {
		declared = declared || export_info.name == name;
	}
	const int top = lua_gettop(L);
	_push_env(L);
	push_key(L, p_name);
	lua_rawget(L, -2);
	const bool exists = !lua_isnil(L, -1);
	lua_settop(L, top);
	if (!declared && !exists) {
		return false;
	}

	_push_env(L);
	push_key(L, p_name);
	LuaBridge::push_variant(L, p_value);
	lua_rawset(L, -3);
	lua_settop(L, top);
	return true;
}

bool LuaScriptInstance::get(const StringName &p_name, Variant &r_ret) const {
	LuaLanguage *language = LuaLanguage::get_singleton();
	lua_State *L = language ? language->get_state() : nullptr;
	if (!L || !initialized) {
		return false;
	}
	const int top = lua_gettop(L);
	_push_env(L);
	push_key(L, p_name);
	lua_rawget(L, -2);
	bool found = !lua_isnil(L, -1) && !lua_isfunction(L, -1);
	if (found) {
		r_ret = LuaBridge::to_variant(L, -1);
	}
	lua_settop(L, top);
	return found;
}

void LuaScriptInstance::get_property_list(List<PropertyInfo> *r_properties) const {
	script->get_script_property_list(r_properties);
}

Variant::Type LuaScriptInstance::get_property_type(const StringName &p_name, bool *r_is_valid) const {
	const std::string name = LuaBridge::to_std_utf8(String(p_name));
	for (const LuaScriptMetadata::Export &export_info : script->metadata.exports) {
		if (export_info.name == name) {
			if (r_is_valid) {
				*r_is_valid = true;
			}
			return type_from_annotation(export_info.type);
		}
	}
	if (r_is_valid) {
		*r_is_valid = false;
	}
	return Variant::NIL;
}

void LuaScriptInstance::validate_property(PropertyInfo &p_property) const {
}

bool LuaScriptInstance::property_can_revert(const StringName &p_name) const {
	return false;
}

bool LuaScriptInstance::property_get_revert(const StringName &p_name, Variant &r_ret) const {
	return false;
}

void LuaScriptInstance::get_method_list(List<MethodInfo> *r_list) const {
	script->get_script_method_list(r_list);
}

bool LuaScriptInstance::has_method(const StringName &p_method) const {
	return script->has_method(p_method);
}

Variant LuaScriptInstance::callp(const StringName &p_method, const Variant **p_args, int p_argcount, Callable::CallError &r_error) {
	Variant ret;
	if (!_call_member(p_method, p_args, p_argcount, ret, r_error)) {
		r_error.error = Callable::CallError::CALL_ERROR_INVALID_METHOD;
	}
	return ret;
}

void LuaScriptInstance::notification(int p_notification, bool p_reversed) {
	const char *function = nullptr;
	switch (p_notification) {
		case Node::NOTIFICATION_ENTER_TREE:
			function = "_enter_tree";
			break;
		case Node::NOTIFICATION_READY:
			function = "_ready";
			break;
		case Node::NOTIFICATION_EXIT_TREE:
			function = "_exit_tree";
			break;
		case Node::NOTIFICATION_PROCESS:
		case Node::NOTIFICATION_PHYSICS_PROCESS: {
			const Node *node = Object::cast_to<Node>(owner);
			if (!node) {
				return;
			}
			const bool physics = p_notification == Node::NOTIFICATION_PHYSICS_PROCESS;
			const double delta = physics ? node->get_physics_process_delta_time() : node->get_process_delta_time();
			const Variant delta_value(delta);
			const Variant *args[1] = { &delta_value };
			Variant ret;
			Callable::CallError error;
			_call_member(StringName(physics ? "_physics_process" : "_process"), args, 1, ret, error);
			return;
		}
		default:
			return;
	}
	Variant ret;
	Callable::CallError error;
	_call_member(StringName(function), nullptr, 0, ret, error);
}

Script *LuaScriptInstance::get_script() const {
	return script.ptr();
}

ScriptLanguage *LuaScriptInstance::get_language() {
	return LuaLanguage::get_singleton();
}
