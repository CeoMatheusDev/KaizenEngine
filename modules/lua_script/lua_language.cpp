/**************************************************************************/
/*  lua_language.cpp                                                      */
/**************************************************************************/
/*  Lua script module for KaizenEngine.                                   */
/**************************************************************************/

#include "lua_language.h"

#include "lua_script.h"

#include "core/config/engine.h"
#include "core/io/file_access.h"
#include "core/math/math_funcs.h"
#include "core/object/class_db.h"
#include "core/object/method_bind.h"
#include "core/object/script_language.h"
#include "core/string/print_string.h"
#include "core/variant/variant.h"

#include <cstring>

namespace {

// Lua code run once when the state is created. It defines `preload` for project resources.
// Written as a string so that it is compiled with the same Lua as the scripts.
const char *const LUA_PRELUDE = R"LUA(
function preload(path)
	local file = FileAccess.open(path, FileAccess.READ)
	if file == nil then
		error("preload: cannot open '" .. tostring(path) .. "'", 2)
	end
	local source = file:get_as_text()
	file:close()
	local chunk, message = load(source, "@" .. path, "t")
	if chunk == nil then
		error(message, 2)
	end
	return chunk()
end
)LUA";

// Lua keywords; Lua 5.5 keeps the same set as earlier releases.
const char *const LUA_KEYWORDS[] = {
	"and", "break", "do", "else", "elseif", "end", "false", "for", "function", "goto", "if", "in",
	"local", "nil", "not", "or", "repeat", "return", "then", "true", "until", "while"
};

// `print` writes to the Godot output instead of the process stdout. It uses luaL_tolstring,
// so values print the way Lua prints them (nil, tables, ...).
int lua_print(lua_State *L) {
	String text;
	const int argc = lua_gettop(L);
	for (int i = 1; i <= argc; i++) {
		if (i > 1) {
			text += "\t";
		}
		size_t length = 0;
		const char *piece = luaL_tolstring(L, i, &length);
		text += String::utf8(piece, int(length));
		lua_pop(L, 1);
	}
	print_line(text);
	return 0;
}

// Converts the Lua values from `p_first` to the top of the stack into Variants.
void collect_arguments(lua_State *L, int p_first, Vector<Variant> &r_args, Vector<const Variant *> &r_pointers) {
	int argc = lua_gettop(L) - p_first + 1;
	if (argc < 0) {
		argc = 0;
	}
	r_args.resize(argc);
	r_pointers.resize(argc);
	Variant *values = r_args.ptrw();
	const Variant **pointers = r_pointers.ptrw();
	for (int i = 0; i < argc; i++) {
		values[i] = LuaBridge::to_variant(L, p_first + i);
		pointers[i] = values + i;
	}
}

// Builds a Variant of `p_type` from the Lua arguments starting at `p_first`.
bool construct_variant(lua_State *L, Variant::Type p_type, int p_first, Variant &r_result) {
	Vector<Variant> args;
	Vector<const Variant *> pointers;
	collect_arguments(L, p_first, args, pointers);
	Callable::CallError error;
	Variant::construct(p_type, r_result, pointers.ptrw(), pointers.size(), error);
	return error.error == Callable::CallError::CALL_OK;
}

// Pushes the constructed value, or returns -1 with a message pushed. Keeping the C++ locals in
// here means they are destroyed before the caller raises the error.
int build_variant(lua_State *L, Variant::Type p_type, int p_first) {
	Variant result;
	if (!construct_variant(L, p_type, p_first, result)) {
		lua_pushstring(L, "Invalid arguments for a Variant constructor.");
		return -1;
	}
	LuaBridge::push_variant(L, result);
	return 1;
}

// `Vector2(1, 2)` calls the type table's __call, whose first argument is the table itself.
int lua_variant_call(lua_State *L) {
	const int n = build_variant(L, Variant::Type(lua_tointeger(L, lua_upvalueindex(1))), 2);
	return n < 0 ? lua_error(L) : n;
}

// `Color.from_hsv(...)`: a built-in static method. The Variant type and the name are upvalues.
int call_variant_static(lua_State *L, Variant::Type p_type, const char *p_name) {
	Vector<Variant> args;
	Vector<const Variant *> pointers;
	collect_arguments(L, 1, args, pointers);
	Variant result;
	Callable::CallError error;
	Variant::call_static(p_type, StringName(p_name), pointers.ptrw(), pointers.size(), result, error);
	if (error.error != Callable::CallError::CALL_OK) {
		lua_pushfstring(L, "Invalid arguments for '%s'.", p_name);
		return -1;
	}
	LuaBridge::push_variant(L, result);
	return 1;
}

int lua_variant_static(lua_State *L) {
	const int n = call_variant_static(L, Variant::Type(lua_tointeger(L, lua_upvalueindex(1))), lua_tostring(L, lua_upvalueindex(2)));
	return n < 0 ? lua_error(L) : n;
}

// `Vector2.ZERO` or `Color.from_hsv`: a constant or a static method of a Variant type.
int variant_member(lua_State *L, Variant::Type p_type, const char *p_name) {
	const StringName member(p_name);
	if (Variant::has_constant(p_type, member)) {
		LuaBridge::push_variant(L, Variant::get_constant_value(p_type, member));
		return 1;
	}
	if (Variant::has_builtin_method(p_type, member)) {
		lua_pushinteger(L, lua_Integer(p_type));
		lua_pushstring(L, p_name);
		lua_pushcclosure(L, lua_variant_static, 2);
		return 1;
	}
	lua_pushfstring(L, "'%s' has no constant or static method named '%s'.", Variant::get_type_name(p_type).utf8().get_data(), p_name);
	return -1;
}

int lua_variant_index(lua_State *L) {
	const char *name = lua_tostring(L, 2);
	if (!name) {
		lua_pushnil(L);
		return 1;
	}
	const int n = variant_member(L, Variant::Type(lua_tointeger(L, lua_upvalueindex(1))), name);
	return n < 0 ? lua_error(L) : n;
}

// Registers a Variant type as a global table: calling it constructs a value, indexing it reaches
// constants and static methods.
void register_variant_type(lua_State *L, const char *p_name, Variant::Type p_type) {
	lua_createtable(L, 0, 0); // T
	lua_createtable(L, 0, 2); // T M
	lua_pushinteger(L, lua_Integer(p_type));
	lua_pushcclosure(L, lua_variant_call, 1);
	lua_setfield(L, -2, "__call");
	lua_pushinteger(L, lua_Integer(p_type));
	lua_pushcclosure(L, lua_variant_index, 1);
	lua_setfield(L, -2, "__index");
	lua_setmetatable(L, -2); // T
	lua_setglobal(L, p_name);
}

// Creates an engine object of `p_class`, pushes it, or returns -1 with a message pushed.
int instantiate_class(lua_State *L, const char *p_class) {
	Object *object = nullptr;
	{
		const StringName class_name(p_class);
		if (ClassDB::class_exists(class_name) && ClassDB::can_instantiate(class_name)) {
			object = ClassDB::instantiate(class_name);
		}
	}
	if (!object) {
		lua_pushfstring(L, "Cannot create an object of class '%s'.", p_class);
		return -1;
	}
	LuaBridge::push_variant(L, Variant(object), true);
	return 1;
}

// `new("RigidBody3D")`: the Lua counterpart of `RigidBody3D.new()`. Freeing is the caller's job,
// as it is for any engine object that is not a RefCounted.
int lua_new_object(lua_State *L) {
	const char *name = luaL_checkstring(L, 1);
	const int n = instantiate_class(L, name);
	return n < 0 ? lua_error(L) : n;
}

// `get_singleton("PhysicsServer3D")`: an engine singleton, or nil when there is none.
bool find_singleton(lua_State *L, Variant &r_value) {
	size_t length = 0;
	const char *text = luaL_checklstring(L, 1, &length);
	Object *object = Engine::get_singleton()->get_singleton_object(StringName(String::utf8(text, int(length))));
	if (!object) {
		return false;
	}
	r_value = Variant(object);
	return true;
}

int lua_get_singleton(lua_State *L) {
	Variant value;
	if (!find_singleton(L, value)) {
		lua_pushnil(L);
		return 1;
	}
	LuaBridge::push_variant(L, value, true);
	return 1;
}

// Reads the first `:<digits>:` group after `p_prefix`, which is the line in a luaL error.
int error_line(const std::string &p_message, const std::string &p_prefix) {
	size_t pos = p_message.find(p_prefix);
	if (pos == std::string::npos) {
		return -1;
	}
	pos += p_prefix.size();
	int line = 0;
	bool any = false;
	while (pos < p_message.size() && p_message[pos] >= '0' && p_message[pos] <= '9') {
		line = line * 10 + (p_message[pos] - '0');
		any = true;
		pos++;
	}
	return any ? line : -1;
}

// ---- Engine classes as globals ------------------------------------------------------------
//
// An unknown global that names an engine class (`Image`, `RenderingServer`, ...) becomes a class
// table, cached in the globals. Indexing it gives integer constants (`BaseMaterial3D.TRANSPARENCY_ALPHA`),
// `Class.new()`, static methods, and instance methods of the class's singleton. Other unknown
// globals stay nil.

// `Class.new()`: the class name is the closure's upvalue.
int lua_class_new(lua_State *L) {
	const int n = instantiate_class(L, lua_tostring(L, lua_upvalueindex(1)));
	return n < 0 ? lua_error(L) : n;
}

// Calls `Class.member(...)`: a static method directly, or an instance method of the singleton.
int invoke_class_member(lua_State *L, const char *p_class, const char *p_member) {
	Vector<Variant> args;
	Vector<const Variant *> pointers;
	collect_arguments(L, 1, args, pointers);
	Variant result;
	Callable::CallError error;
	{
		const StringName class_name(p_class);
		const StringName member(p_member);
		const MethodBind *bind = ClassDB::get_method(class_name, member);
		if (bind && bind->is_static()) {
			result = bind->call(nullptr, pointers.ptrw(), pointers.size(), error);
		} else {
			Object *singleton = Engine::get_singleton()->get_singleton_object(class_name);
			if (!singleton) {
				lua_pushfstring(L, "'%s.%s' is an instance method; no singleton '%s' exists.", p_class, p_member, p_class);
				return -1;
			}
			result = singleton->callp(member, pointers.ptrw(), pointers.size(), error);
		}
	}
	if (error.error != Callable::CallError::CALL_OK) {
		lua_pushfstring(L, "Invalid arguments for '%s.%s'.", p_class, p_member);
		return -1;
	}
	LuaBridge::push_variant(L, result);
	return 1;
}

int lua_class_method(lua_State *L) {
	const int n = invoke_class_member(L, lua_tostring(L, lua_upvalueindex(1)), lua_tostring(L, lua_upvalueindex(2)));
	return n < 0 ? lua_error(L) : n;
}

// Resolves `Class.member`: constant, `new`, or a method.
int class_member(lua_State *L, const char *p_class, const char *p_member) {
	{
		const StringName class_name(p_class);
		const StringName member(p_member);
		bool found = false;
		const int64_t constant = ClassDB::get_integer_constant(class_name, member, &found);
		if (found) {
			lua_pushinteger(L, lua_Integer(constant));
			return 1;
		}
		if (ClassDB::has_method(class_name, member)) {
			lua_pushstring(L, p_class);
			lua_pushstring(L, p_member);
			lua_pushcclosure(L, lua_class_method, 2);
			return 1;
		}
	}
	if (strcmp(p_member, "new") == 0) {
		lua_pushstring(L, p_class);
		lua_pushcclosure(L, lua_class_new, 1);
		return 1;
	}
	lua_pushfstring(L, "'%s' has no constant or method named '%s'.", p_class, p_member);
	return -1;
}

int lua_class_index(lua_State *L) {
	const char *member = lua_tostring(L, 2);
	if (!member) {
		lua_pushnil(L);
		return 1;
	}
	const int n = class_member(L, lua_tostring(L, lua_upvalueindex(1)), member);
	return n < 0 ? lua_error(L) : n;
}

// __index of the globals table. Pushes the class table for an engine class name, or nil.
int lua_globals_index(lua_State *L) {
	const char *name = lua_tostring(L, 2);
	bool is_class = false;
	if (name) {
		const StringName class_name(name);
		is_class = ClassDB::class_exists(class_name);
	}
	if (!is_class) {
		lua_pushnil(L);
		return 1;
	}
	lua_createtable(L, 0, 0); // [globals, key, class table]
	lua_createtable(L, 0, 1); // ... class table, metatable
	lua_pushstring(L, name);
	lua_pushcclosure(L, lua_class_index, 1);
	lua_setfield(L, -2, "__index");
	lua_setmetatable(L, -2);
	lua_pushvalue(L, 2);
	lua_pushvalue(L, -2);
	lua_rawset(L, 1); // globals[name] = class table
	return 1;
}

// `randf_range(from, to)`: a uniform float between the two bounds, as in GDScript.
int lua_randf_range(lua_State *L) {
	const double from = luaL_checknumber(L, 1);
	const double to = luaL_checknumber(L, 2);
	lua_pushnumber(L, from + (to - from) * double(Math::randf()));
	return 1;
}

} // namespace

LuaLanguage *LuaLanguage::singleton = nullptr;

LuaLanguage *LuaLanguage::get_singleton() {
	return singleton;
}

LuaLanguage::LuaLanguage() {
	singleton = this;
}

LuaLanguage::~LuaLanguage() {
	finish();
	if (singleton == this) {
		singleton = nullptr;
	}
}

void LuaLanguage::_open_libraries() {
	luaL_openlibs(state);
}

void LuaLanguage::_install_globals() {
	LuaBridge::register_metatable(state);

	lua_pushcfunction(state, lua_print);
	lua_setglobal(state, "print");

	lua_pushcfunction(state, lua_new_object);
	lua_setglobal(state, "new");
	lua_pushcfunction(state, lua_get_singleton);
	lua_setglobal(state, "get_singleton");
	lua_pushcfunction(state, lua_randf_range);
	lua_setglobal(state, "randf_range");

	// Every built-in Variant type with a GDScript-style name (Vector2, Transform3D, PackedFloat32Array, ...)
	// becomes a global constructor. Lowercase names (bool, int, float) are Lua's own types.
	for (int i = Variant::BOOL; i < Variant::VARIANT_MAX; i++) {
		const Variant::Type type = Variant::Type(i);
		if (type == Variant::OBJECT) {
			continue;
		}
		const String name = Variant::get_type_name(type);
		if (name.is_empty() || name[0] < 'A' || name[0] > 'Z') {
			continue;
		}
		register_variant_type(state, name.utf8().get_data(), type);
	}

	// `preload("res://...")` runs the file as a chunk and returns what it returns, like GDScript's
	// preload for a script. It is written in Lua so that the chunk sees the same globals.
	if (luaL_dostring(state, LUA_PRELUDE) != LUA_OK) {
		report_error("Lua prelude failed: " + LuaBridge::top_string(state));
		lua_pop(state, 1);
	}

	// Unknown globals that name an engine class resolve to a class table (see lua_globals_index).
	lua_pushglobaltable(state); // G
	lua_createtable(state, 0, 1); // G M
	lua_pushcfunction(state, lua_globals_index);
	lua_setfield(state, -2, "__index");
	lua_setmetatable(state, -2); // G
	lua_pop(state, 1);
}

void LuaLanguage::report_error(const String &p_message) {
	last_error = p_message;
	ERR_PRINT(p_message);
}

void LuaLanguage::register_script(LuaScript *p_script) {
	scripts.insert(p_script);
}

void LuaLanguage::unregister_script(LuaScript *p_script) {
	scripts.erase(p_script);
}

String LuaLanguage::get_global_class_name_from_file(const String &p_path, String *r_base_type, bool *r_is_tool) {
	const String source = FileAccess::get_file_as_string(p_path);
	const LuaScriptMetadata::Metadata metadata = LuaScriptMetadata::parse(LuaBridge::to_std_utf8(source));
	if (r_base_type) {
		*r_base_type = metadata.extends.empty() ? String("Object") : LuaBridge::from_std_utf8(metadata.extends);
	}
	if (r_is_tool) {
		*r_is_tool = metadata.tool;
	}
	return LuaBridge::from_std_utf8(metadata.class_name);
}

String LuaLanguage::get_name() const {
	return "Lua";
}

void LuaLanguage::init() {
	if (state) {
		return;
	}
	closing = false;
	state = luaL_newstate();
	ERR_FAIL_NULL_MSG(state, "Could not create the Lua state.");
	_open_libraries();
	_install_globals();
}

String LuaLanguage::get_type() const {
	return "LuaScript";
}

String LuaLanguage::get_extension() const {
	return "lua";
}

void LuaLanguage::finish() {
	if (!state) {
		return;
	}
	// Set first, so that destructors that run during lua_close skip their luaL_unref calls.
	closing = true;
	lua_State *old_state = state;
	state = nullptr;
	lua_close(old_state);
	scripts.clear();
}

#ifdef TOOLS_ENABLED
EditorLanguage *LuaLanguage::get_editor_language() {
	return &editor_language;
}
#endif // TOOLS_ENABLED

Vector<String> LuaLanguage::get_reserved_words() const {
	Vector<String> words;
	for (const char *keyword : LUA_KEYWORDS) {
		words.push_back(keyword);
	}
	return words;
}

bool LuaLanguage::is_control_flow_keyword(const String &p_string) const {
	static const char *const control_flow[] = { "if", "elseif", "else", "then", "for", "while", "do", "repeat", "until", "break", "return", "goto" };
	for (const char *keyword : control_flow) {
		if (p_string == keyword) {
			return true;
		}
	}
	return false;
}

Vector<String> LuaLanguage::get_comment_delimiters() const {
	Vector<String> delimiters;
	delimiters.push_back("--");
	return delimiters;
}

Vector<String> LuaLanguage::get_doc_comment_delimiters() const {
	Vector<String> delimiters;
	delimiters.push_back("---");
	return delimiters;
}

Vector<String> LuaLanguage::get_string_delimiters() const {
	Vector<String> delimiters;
	delimiters.push_back("\" \"");
	delimiters.push_back("' '");
	delimiters.push_back("[[ ]]");
	return delimiters;
}

Ref<Script> LuaLanguage::make_template(const String &p_template, const String &p_class_name, const String &p_base_class_name) const {
	Ref<LuaScript> script;
	script.instantiate();
	String source;
	if (!p_base_class_name.is_empty()) {
		source += "--@extends " + p_base_class_name + "\n";
	}
	if (!p_class_name.is_empty()) {
		source += "--@class_name " + p_class_name + "\n";
	}
	source += "\n" + p_template;
	script->set_source_code(source);
	return script;
}

Vector<ScriptLanguage::ScriptTemplate> LuaLanguage::get_built_in_templates(const StringName &p_object) {
	return Vector<ScriptTemplate>();
}

String LuaLanguage::make_function(const String &p_class, const String &p_name, const PackedStringArray &p_args) const {
	String args;
	for (int i = 0; i < p_args.size(); i++) {
		if (i > 0) {
			args += ", ";
		}
		args += p_args[i];
	}
	return "function " + p_name + "(" + args + ")\n\nend\n";
}

void LuaLanguage::add_global_constant(const StringName &p_variable, const Variant &p_value) {
	if (!state) {
		return;
	}
	const CharString name = String(p_variable).utf8();
	LuaBridge::push_variant(state, p_value);
	lua_setglobal(state, name.get_data());
}

String LuaLanguage::debug_get_error() const {
	return last_error;
}

void LuaLanguage::reload_all_scripts() {
	for (LuaScript *script : scripts) {
		script->reload(true);
	}
}

void LuaLanguage::reload_scripts(const Array &p_scripts) {
	for (int i = 0; i < p_scripts.size(); i++) {
		Object *object = p_scripts[i];
		if (LuaScript *script = Object::cast_to<LuaScript>(object)) {
			script->reload(true);
		}
	}
}

void LuaLanguage::reload_tool_script(const Ref<Script> &p_script) {
	if (LuaScript *script = Object::cast_to<LuaScript>(p_script.ptr())) {
		script->reload(true);
	}
}

bool LuaLanguage::handles_global_class_type(const String &p_type) const {
	return p_type == "LuaScript";
}

String LuaLanguage::get_global_class_name(const String &p_path, String *r_base_type, String *r_icon_path, bool *r_is_abstract, bool *r_is_tool) const {
	if (r_icon_path) {
		*r_icon_path = String();
	}
	if (r_is_abstract) {
		*r_is_abstract = false;
	}
	return get_global_class_name_from_file(p_path, r_base_type, r_is_tool);
}

// ---------------------------------------------------------------------------------------
// Editor support
// ---------------------------------------------------------------------------------------

#ifdef TOOLS_ENABLED
bool LuaEditorLanguage::validate(const String &p_code, const String &p_path, List<ScriptError> *r_errors, List<Warning> *r_warnings, List<String> *r_functions, HashSet<int> *r_safe_lines) const {
	String error;
	const bool ok = LuaScript::check_syntax(p_code, p_path, error);
	if (!ok && r_errors) {
		ScriptError script_error;
		script_error.path = p_path;
		// luaL messages look like "path:LINE: text"; fall back to line 1 when that is absent.
		const std::string message = LuaBridge::to_std_utf8(error);
		const std::string prefix = LuaBridge::to_std_utf8(p_path.is_empty() ? String("script.lua") : p_path) + ":";
		const int line = error_line(message, prefix);
		script_error.start_line = line > 0 ? line : 1;
		script_error.end_line = script_error.start_line;
		script_error.message = error;
		r_errors->push_back(script_error);
	}
	if (r_functions) {
		const LuaScriptMetadata::Metadata metadata = LuaScriptMetadata::parse(LuaBridge::to_std_utf8(p_code));
		for (const LuaScriptMetadata::Function &function : metadata.functions) {
			r_functions->push_back(LuaBridge::from_std_utf8(function.name));
		}
	}
	return ok;
}

int32_t LuaEditorLanguage::find_function(const String &p_function, const String &p_code) const {
	const int line = LuaScriptMetadata::find_function_line(LuaBridge::to_std_utf8(p_code), LuaBridge::to_std_utf8(p_function));
	return line > 0 ? line - 1 : -1;
}
#endif // TOOLS_ENABLED
