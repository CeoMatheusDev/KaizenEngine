/**************************************************************************/
/*  lua_bridge.cpp                                                        */
/**************************************************************************/
/*  Lua script module for KaizenEngine.                                   */
/**************************************************************************/

#include "lua_bridge.h"

#include "lua_language.h"

#include "core/object/class_db.h"
#include "core/object/object.h"
#include "core/os/memory.h"
#include "core/variant/array.h"
#include "core/variant/callable.h"
#include "core/variant/dictionary.h"

#include <new>

// Error discipline: Lua raises errors with longjmp, which would skip C++ destructors. Every
// metamethod below therefore does its C++ work in an `*_impl` helper whose locals are gone
// when it returns, and only then raises. Helpers report failure by pushing a message and
// returning -1.

namespace LuaBridge {
namespace {

constexpr int MAX_CONVERSION_DEPTH = 32;
constexpr const char *NEXT_REGISTRY_KEY = "kaizen.next";

LuaVariantBox *check_box(lua_State *L, int p_index) {
	return static_cast<LuaVariantBox *>(luaL_testudata(L, p_index, VARIANT_METATABLE));
}

// Types indexed by 1-based integers from Lua (converted to the 0-based Godot index).
bool is_sequence_type(Variant::Type p_type) {
	switch (p_type) {
		case Variant::ARRAY:
		case Variant::PACKED_BYTE_ARRAY:
		case Variant::PACKED_INT32_ARRAY:
		case Variant::PACKED_INT64_ARRAY:
		case Variant::PACKED_FLOAT32_ARRAY:
		case Variant::PACKED_FLOAT64_ARRAY:
		case Variant::PACKED_STRING_ARRAY:
		case Variant::PACKED_VECTOR2_ARRAY:
		case Variant::PACKED_VECTOR3_ARRAY:
		case Variant::PACKED_COLOR_ARRAY:
		case Variant::PACKED_VECTOR4_ARRAY:
			return true;
		default:
			return false;
	}
}

void push_string(lua_State *L, const String &p_text) {
	const CharString utf8 = p_text.utf8();
	lua_pushlstring(L, utf8.get_data(), utf8.length());
}

// Pushes `p_message` and returns -1; the caller raises it after its C++ locals are gone.
int push_error(lua_State *L, const String &p_message) {
	push_string(L, p_message);
	return -1;
}

int box_method_thunk(lua_State *L);

StringName key_name(lua_State *L, int p_index) {
	size_t length = 0;
	const char *text = lua_tolstring(L, p_index, &length);
	return StringName(String::utf8(text, int(length)));
}

int push_method(lua_State *L, const StringName &p_name) {
	push_string(L, String(p_name));
	lua_pushcclosure(L, box_method_thunk, 1);
	return 1;
}

int64_t sequence_size(const Variant &p_value) {
	Variant copy = p_value;
	Variant ret;
	Callable::CallError error;
	copy.callp(StringName("size"), nullptr, 0, ret, error);
	return error.error == Callable::CallError::CALL_OK ? int64_t(ret) : 0;
}

// Method calls use the colon convention: `value:method(args)` passes the receiver as the
// first argument, so `p_first_arg` is 2 for methods and for `__call`.
int call_method_impl(lua_State *L, const StringName &p_name, int p_first_arg) {
	LuaVariantBox *box = check_box(L, 1);
	if (!box) {
		return push_error(L, "Method '" + String(p_name) + "' must be called with ':' on a Godot value.");
	}
	Variant target = box->value;

	const int top = lua_gettop(L);
	const int argc = top >= p_first_arg ? top - p_first_arg + 1 : 0;
	Vector<Variant> args;
	args.resize(argc);
	Vector<const Variant *> arg_ptrs;
	arg_ptrs.resize(argc);
	{
		Variant *values = args.ptrw();
		const Variant **pointers = arg_ptrs.ptrw();
		for (int i = 0; i < argc; i++) {
			values[i] = to_variant(L, p_first_arg + i);
			pointers[i] = values + i;
		}
	}

	Variant ret;
	Callable::CallError error;
	String description;
	if (target.get_type() == Variant::OBJECT) {
		Object *object = target.get_validated_object();
		if (!object) {
			return push_error(L, "Attempt to call '" + String(p_name) + "' on a freed object.");
		}
		ret = object->callp(p_name, arg_ptrs.ptrw(), argc, error);
		if (error.error != Callable::CallError::CALL_OK) {
			description = Variant::get_call_error_text(object, p_name, arg_ptrs.ptrw(), argc, error);
		}
	} else {
		target.callp(p_name, arg_ptrs.ptrw(), argc, ret, error);
		if (error.error != Callable::CallError::CALL_OK) {
			description = Variant::get_call_error_text(p_name, arg_ptrs.ptrw(), argc, error);
		}
	}

	if (error.error != Callable::CallError::CALL_OK) {
		return push_error(L, description);
	}
	push_variant(L, ret);
	return 1;
}

int index_impl(lua_State *L) {
	LuaVariantBox *box = check_box(L, 1);
	if (!box) {
		return push_error(L, "Invalid Variant receiver.");
	}
	const Variant target = box->value;
	Object *object = nullptr;
	if (target.get_type() == Variant::OBJECT) {
		object = target.get_validated_object();
		if (!object) {
			return push_error(L, "Attempt to access a freed object.");
		}
	}

	if (lua_type(L, 2) == LUA_TSTRING) {
		const StringName name = key_name(L, 2);
		if (object) {
			// Methods first: Object::get() also reports a method name as a valid Callable value,
			// which would turn `obj:method(...)` into `Callable.call(obj, ...)`.
			if (object->has_method(name)) {
				return push_method(L, name);
			}
			bool valid = false;
			const Variant value = object->get(name, &valid);
			if (valid) {
				push_variant(L, value);
				return 1;
			}
			// Integer class constants (`PhysicsServer3D.BODY_STATE_TRANSFORM`), searched up the hierarchy.
			bool found = false;
			const int64_t constant = ClassDB::get_integer_constant(object->get_class_name(), name, &found);
			if (found) {
				lua_pushinteger(L, lua_Integer(constant));
				return 1;
			}
			return push_error(L, "'" + String(object->get_class()) + "' has no property, method or constant named '" + String(name) + "'.");
		}

		if (Variant::has_member(target.get_type(), name)) {
			bool valid = false;
			const Variant value = target.get_named(name, valid);
			if (valid) {
				push_variant(L, value);
				return 1;
			}
		}
		if (target.has_method(name)) {
			return push_method(L, name);
		}
		// Dictionaries fall back to element lookup; a missing key is nil, as for a Lua table.
		// Any other Variant with no such member is an error, the same as for objects, so a typo
		// like `vec.nope` is not silently nil.
		bool valid = false;
		const Variant value = target.get(Variant(String(name)), &valid);
		if (valid) {
			push_variant(L, value);
			return 1;
		}
		if (target.get_type() == Variant::DICTIONARY) {
			lua_pushnil(L);
			return 1;
		}
		return push_error(L, "'" + Variant::get_type_name(target.get_type()) + "' has no member named '" + String(name) + "'.");
	}

	if (object) {
		return push_error(L, "Objects can only be indexed by property or method name.");
	}
	const Variant key = to_variant(L, 2);
	if (is_sequence_type(target.get_type()) && key.get_type() == Variant::INT) {
		// Lua indices are 1-based; Godot sequences are 0-based.
		const int64_t index = int64_t(key) - 1;
		if (index < 0 || index >= sequence_size(target)) {
			lua_pushnil(L);
			return 1;
		}
		bool valid = false;
		const Variant value = target.get(Variant(index), &valid);
		push_variant(L, valid ? value : Variant());
		return 1;
	}
	bool valid = false;
	const Variant value = target.get(key, &valid);
	push_variant(L, valid ? value : Variant());
	return 1;
}

int newindex_impl(lua_State *L) {
	LuaVariantBox *box = check_box(L, 1);
	if (!box) {
		return push_error(L, "Invalid Variant receiver.");
	}
	Object *object = nullptr;
	if (box->value.get_type() == Variant::OBJECT) {
		object = box->value.get_validated_object();
		if (!object) {
			return push_error(L, "Attempt to assign to a freed object.");
		}
	}
	const Variant value = to_variant(L, 3);

	if (lua_type(L, 2) == LUA_TSTRING) {
		const StringName name = key_name(L, 2);
		if (object) {
			bool valid = false;
			object->set(name, value, &valid);
			if (!valid) {
				return push_error(L, "'" + String(object->get_class()) + "' has no property named '" + String(name) + "'.");
			}
			return 0;
		}
		if (Variant::has_member(box->value.get_type(), name)) {
			bool valid = false;
			box->value.set_named(name, value, valid);
			if (!valid) {
				return push_error(L, "Cannot assign '" + String(name) + "'.");
			}
			return 0;
		}
		bool valid = false;
		box->value.set(Variant(String(name)), value, &valid);
		if (!valid) {
			return push_error(L, "Cannot assign '" + String(name) + "'.");
		}
		return 0;
	}

	if (object) {
		return push_error(L, "Objects can only be assigned by property name.");
	}
	const Variant key = to_variant(L, 2);
	if (is_sequence_type(box->value.get_type()) && key.get_type() == Variant::INT) {
		const int64_t index = int64_t(key) - 1;
		const int64_t size = sequence_size(box->value);
		if (index < 0 || index > size) {
			return push_error(L, "Index " + itos(int64_t(key)) + " is out of range.");
		}
		if (index == size) {
			// `t[#t + 1] = value` appends, as it does for Lua tables.
			Variant ret;
			Callable::CallError error;
			const Variant *argv[1] = { &value };
			box->value.callp(StringName("append"), argv, 1, ret, error);
			if (error.error != Callable::CallError::CALL_OK) {
				return push_error(L, "Cannot append to this value.");
			}
			return 0;
		}
		bool valid = false;
		box->value.set(Variant(index), value, &valid);
		if (!valid) {
			return push_error(L, "Cannot assign element " + itos(int64_t(key)) + ".");
		}
		return 0;
	}
	bool valid = false;
	box->value.set(key, value, &valid);
	if (!valid) {
		return push_error(L, "Cannot assign this key.");
	}
	return 0;
}

int len_impl(lua_State *L) {
	LuaVariantBox *box = check_box(L, 1);
	if (!box) {
		return push_error(L, "Invalid Variant receiver.");
	}
	const Variant::Type type = box->value.get_type();
	if (is_sequence_type(type) || type == Variant::DICTIONARY) {
		lua_pushinteger(L, lua_Integer(sequence_size(box->value)));
		return 1;
	}
	return push_error(L, "Attempt to get the length of a " + Variant::get_type_name(type) + " value.");
}

int tostring_impl(lua_State *L) {
	LuaVariantBox *box = check_box(L, 1);
	if (!box) {
		return push_error(L, "Invalid Variant receiver.");
	}
	push_string(L, box->value.stringify());
	return 1;
}

int call_impl(lua_State *L) {
	LuaVariantBox *box = check_box(L, 1);
	if (!box || box->value.get_type() != Variant::CALLABLE) {
		return push_error(L, "Attempt to call a non-callable value.");
	}
	return call_method_impl(L, StringName("call"), 2);
}

// `pairs(value)` iterates a snapshot copy, which keeps the loop safe against mutation.
int pairs_impl(lua_State *L) {
	LuaVariantBox *box = check_box(L, 1);
	if (!box) {
		return push_error(L, "Invalid Variant receiver.");
	}
	const Variant::Type type = box->value.get_type();
	if (type == Variant::DICTIONARY) {
		const Dictionary dictionary = box->value;
		lua_createtable(L, 0, dictionary.size());
		for (const KeyValue<Variant, Variant> &entry : dictionary) {
			push_variant(L, entry.key);
			push_variant(L, entry.value);
			lua_settable(L, -3);
		}
	} else if (is_sequence_type(type)) {
		const int64_t size = sequence_size(box->value);
		lua_createtable(L, int(size), 0);
		for (int64_t i = 0; i < size; i++) {
			bool valid = false;
			const Variant element = box->value.get(Variant(i), &valid);
			lua_pushinteger(L, lua_Integer(i + 1));
			push_variant(L, valid ? element : Variant());
			lua_settable(L, -3);
		}
	} else {
		return push_error(L, "Attempt to iterate a " + Variant::get_type_name(type) + " value.");
	}
	lua_getfield(L, LUA_REGISTRYINDEX, NEXT_REGISTRY_KEY);
	lua_insert(L, -2);
	lua_pushnil(L);
	return 3;
}

// `..` is defined for Variant boxes (any Godot value) plus strings and numbers, like Lua's own
// rule for strings and numbers. Anything else (nil, booleans, tables, functions) is an error.
int concat_impl(lua_State *L) {
	for (int i = 1; i <= 2; i++) {
		const int type = lua_type(L, i);
		const bool allowed = type == LUA_TSTRING || type == LUA_TNUMBER || check_box(L, i) != nullptr;
		if (!allowed) {
			return push_error(L, String("attempt to concatenate a ") + String(lua_typename(L, type)) + " value");
		}
	}
	const String text = to_variant(L, 1).stringify() + to_variant(L, 2).stringify();
	push_string(L, text);
	return 1;
}

int arith_impl(lua_State *L) {
	const Variant::Operator op = Variant::Operator(lua_tointeger(L, lua_upvalueindex(1)));
	const Variant a = to_variant(L, 1);
	const Variant b = to_variant(L, 2);
	Variant result;
	bool valid = false;
	Variant::evaluate(op, a, b, result, valid);
	if (!valid) {
		return push_error(L, "Unsupported operation between " + Variant::get_type_name(a.get_type()) + " and " + Variant::get_type_name(b.get_type()) + ".");
	}
	push_variant(L, result);
	return 1;
}

int unary_impl(lua_State *L) {
	const Variant a = to_variant(L, 1);
	const Variant minus_one(int64_t(-1));
	Variant result;
	bool valid = false;
	Variant::evaluate(Variant::OP_MULTIPLY, a, minus_one, result, valid);
	if (!valid) {
		return push_error(L, "Unsupported operation on " + Variant::get_type_name(a.get_type()) + ".");
	}
	push_variant(L, result);
	return 1;
}

int method_impl(lua_State *L) {
	size_t length = 0;
	const char *text = lua_tolstring(L, lua_upvalueindex(1), &length);
	const StringName name(String::utf8(text, int(length)));
	return call_method_impl(L, name, 2);
}

// Each metamethod is a thin wrapper: run the helper, then raise its error if it failed.
#define LUA_BRIDGE_THUNK(thunk_name, impl_name)  \
	int thunk_name(lua_State *L) {               \
		const int result = impl_name(L);         \
		return result < 0 ? lua_error(L) : result; \
	}

LUA_BRIDGE_THUNK(box_index_thunk, index_impl)
LUA_BRIDGE_THUNK(box_newindex_thunk, newindex_impl)
LUA_BRIDGE_THUNK(box_len_thunk, len_impl)
LUA_BRIDGE_THUNK(box_tostring_thunk, tostring_impl)
LUA_BRIDGE_THUNK(box_call_thunk, call_impl)
LUA_BRIDGE_THUNK(box_pairs_thunk, pairs_impl)
LUA_BRIDGE_THUNK(box_concat_thunk, concat_impl)
LUA_BRIDGE_THUNK(box_arith_thunk, arith_impl)
LUA_BRIDGE_THUNK(box_unary_thunk, unary_impl)
LUA_BRIDGE_THUNK(box_method_thunk, method_impl)

#undef LUA_BRIDGE_THUNK

int box_gc(lua_State *L) {
	LuaVariantBox *box = check_box(L, 1);
	if (box) {
		box->~LuaVariantBox();
	}
	return 0;
}

void set_arith_metamethod(lua_State *L, const char *p_name, Variant::Operator p_op) {
	lua_pushinteger(L, lua_Integer(p_op));
	lua_pushcclosure(L, box_arith_thunk, 1);
	lua_setfield(L, -2, p_name);
}

// Callable bridging: a Lua function converted to a Callable keeps a registry reference.
class LuaCallable : public CallableCustom {
	int ref = LUA_NOREF;

	static bool compare_equal(const CallableCustom *p_a, const CallableCustom *p_b) {
		return static_cast<const LuaCallable *>(p_a)->ref == static_cast<const LuaCallable *>(p_b)->ref;
	}
	static bool compare_less(const CallableCustom *p_a, const CallableCustom *p_b) {
		return static_cast<const LuaCallable *>(p_a)->ref < static_cast<const LuaCallable *>(p_b)->ref;
	}

public:
	explicit LuaCallable(int p_ref) :
			ref(p_ref) {}

	virtual ~LuaCallable() override {
		lua_State *L = LuaLanguage::get_singleton() ? LuaLanguage::get_singleton()->get_state() : nullptr;
		if (L && ref != LUA_NOREF && !LuaLanguage::get_singleton()->is_closing()) {
			luaL_unref(L, LUA_REGISTRYINDEX, ref);
		}
	}

	virtual uint32_t hash() const override { return uint32_t(ref); }
	virtual String get_as_text() const override { return "LuaFunction"; }
	virtual CompareEqualFunc get_compare_equal_func() const override { return compare_equal; }
	virtual CompareLessFunc get_compare_less_func() const override { return compare_less; }
	virtual ObjectID get_object() const override { return ObjectID(); }
	// The default CallableCustom::is_valid() asks ObjectDB for get_object(), which is null for a
	// Lua function, so every call would fail with "Instance is null". Validity is the registry ref.
	virtual bool is_valid() const override { return ref != LUA_NOREF; }

	virtual void call(const Variant **p_arguments, int p_argcount, Variant &r_return_value, Callable::CallError &r_call_error) const override {
		r_call_error.error = Callable::CallError::CALL_OK;
		lua_State *L = LuaLanguage::get_singleton() ? LuaLanguage::get_singleton()->get_state() : nullptr;
		if (!L) {
			r_call_error.error = Callable::CallError::CALL_ERROR_INVALID_METHOD;
			return;
		}
		const int top = lua_gettop(L);
		lua_checkstack(L, p_argcount + 4);
		lua_rawgeti(L, LUA_REGISTRYINDEX, ref);
		for (int i = 0; i < p_argcount; i++) {
			push_variant(L, *p_arguments[i]);
		}
		if (pcall(L, p_argcount, 1) == LUA_OK) {
			r_return_value = to_variant(L, -1);
		} else {
			LuaLanguage::get_singleton()->report_error(top_string(L));
		}
		lua_settop(L, top);
	}
};

Variant to_variant_depth(lua_State *L, int p_index, int p_depth);

Variant table_to_variant(lua_State *L, int p_index, int p_depth) {
	if (p_depth > MAX_CONVERSION_DEPTH) {
		LuaLanguage::get_singleton()->report_error("Table nesting is too deep to convert to a Variant (cyclic table?).");
		return Variant();
	}
	const lua_Unsigned length = lua_rawlen(L, p_index);
	lua_Unsigned count = 0;
	bool sequence = true;
	lua_pushnil(L);
	while (lua_next(L, p_index) != 0) {
		count++;
		// Only read the key; calling lua_tolstring on it would confuse lua_next.
		if (!lua_isinteger(L, -2)) {
			sequence = false;
		} else {
			const lua_Integer key = lua_tointeger(L, -2);
			if (key < 1 || lua_Unsigned(key) > length) {
				sequence = false;
			}
		}
		lua_pop(L, 1);
	}

	if (sequence && count == length && count > 0) {
		Array array;
		for (lua_Unsigned i = 1; i <= length; i++) {
			lua_rawgeti(L, p_index, lua_Integer(i));
			array.push_back(to_variant_depth(L, -1, p_depth + 1));
			lua_pop(L, 1);
		}
		return array;
	}

	Dictionary dictionary;
	lua_pushnil(L);
	while (lua_next(L, p_index) != 0) {
		const Variant key = to_variant_depth(L, -2, p_depth + 1);
		dictionary[key] = to_variant_depth(L, -1, p_depth + 1);
		lua_pop(L, 1);
	}
	return dictionary;
}

Variant to_variant_depth(lua_State *L, int p_index, int p_depth) {
	p_index = lua_absindex(L, p_index);
	switch (lua_type(L, p_index)) {
		case LUA_TBOOLEAN:
			return lua_toboolean(L, p_index) != 0;
		case LUA_TNUMBER:
			if (lua_isinteger(L, p_index)) {
				return int64_t(lua_tointeger(L, p_index));
			}
			return double(lua_tonumber(L, p_index));
		case LUA_TSTRING: {
			size_t length = 0;
			const char *text = lua_tolstring(L, p_index, &length);
			return String::utf8(text, int(length));
		}
		case LUA_TUSERDATA: {
			LuaVariantBox *box = check_box(L, p_index);
			return box ? box->value : Variant();
		}
		case LUA_TFUNCTION: {
			lua_pushvalue(L, p_index);
			const int ref = luaL_ref(L, LUA_REGISTRYINDEX);
			return Callable(memnew(LuaCallable(ref)));
		}
		case LUA_TTABLE:
			return table_to_variant(L, p_index, p_depth);
		default:
			return Variant();
	}
}

} // namespace

void register_metatable(lua_State *L) {
	luaL_newmetatable(L, VARIANT_METATABLE);
	lua_pushstring(L, "Variant");
	lua_setfield(L, -2, "__name");
	lua_pushcfunction(L, box_index_thunk);
	lua_setfield(L, -2, "__index");
	lua_pushcfunction(L, box_newindex_thunk);
	lua_setfield(L, -2, "__newindex");
	lua_pushcfunction(L, box_gc);
	lua_setfield(L, -2, "__gc");
	lua_pushcfunction(L, box_tostring_thunk);
	lua_setfield(L, -2, "__tostring");
	lua_pushcfunction(L, box_len_thunk);
	lua_setfield(L, -2, "__len");
	lua_pushcfunction(L, box_call_thunk);
	lua_setfield(L, -2, "__call");
	lua_pushcfunction(L, box_pairs_thunk);
	lua_setfield(L, -2, "__pairs");
	lua_pushcfunction(L, box_concat_thunk);
	lua_setfield(L, -2, "__concat");
	lua_pushcfunction(L, box_unary_thunk);
	lua_setfield(L, -2, "__unm");
	set_arith_metamethod(L, "__add", Variant::OP_ADD);
	set_arith_metamethod(L, "__sub", Variant::OP_SUBTRACT);
	set_arith_metamethod(L, "__mul", Variant::OP_MULTIPLY);
	set_arith_metamethod(L, "__div", Variant::OP_DIVIDE);
	set_arith_metamethod(L, "__mod", Variant::OP_MODULE);
	set_arith_metamethod(L, "__pow", Variant::OP_POWER);
	set_arith_metamethod(L, "__eq", Variant::OP_EQUAL);
	set_arith_metamethod(L, "__lt", Variant::OP_LESS);
	set_arith_metamethod(L, "__le", Variant::OP_LESS_EQUAL);
	lua_pop(L, 1);

	lua_getglobal(L, "next");
	lua_setfield(L, LUA_REGISTRYINDEX, NEXT_REGISTRY_KEY);
}

void push_variant(lua_State *L, const Variant &p_value, bool p_keep_alive) {
	switch (p_value.get_type()) {
		case Variant::NIL:
			lua_pushnil(L);
			return;
		case Variant::BOOL:
			lua_pushboolean(L, bool(p_value) ? 1 : 0);
			return;
		case Variant::INT:
			lua_pushinteger(L, lua_Integer(int64_t(p_value)));
			return;
		case Variant::FLOAT:
			lua_pushnumber(L, lua_Number(double(p_value)));
			return;
		case Variant::STRING:
		case Variant::STRING_NAME:
			push_string(L, String(p_value));
			return;
		default:
			break;
	}

	void *memory = lua_newuserdatauv(L, sizeof(LuaVariantBox), 0);
	LuaVariantBox *box = new (memory) LuaVariantBox();
	box->value = p_value;
	if (p_keep_alive && p_value.get_type() == Variant::OBJECT) {
		if (RefCounted *counted = Object::cast_to<RefCounted>(p_value.get_validated_object())) {
			box->keepalive = Ref<RefCounted>(counted);
		}
	}
	luaL_setmetatable(L, VARIANT_METATABLE);
}

Variant to_variant(lua_State *L, int p_index) {
	return to_variant_depth(L, p_index, 0);
}

bool is_variant_box(lua_State *L, int p_index) {
	return check_box(L, p_index) != nullptr;
}

int pcall(lua_State *L, int p_nargs, int p_nresults) {
	const int base = lua_gettop(L) - p_nargs;
	lua_pushcfunction(L, [](lua_State *L_inner) -> int {
		const char *message = lua_tostring(L_inner, 1);
		luaL_traceback(L_inner, L_inner, message, 1);
		return 1;
	});
	lua_insert(L, base);
	const int status = lua_pcall(L, p_nargs, p_nresults, base);
	lua_remove(L, base);
	return status;
}

String top_string(lua_State *L) {
	size_t length = 0;
	const char *text = lua_tolstring(L, -1, &length);
	if (!text) {
		return "(error object is not a string)";
	}
	return String::utf8(text, int(length));
}

std::string to_std_utf8(const String &p_text) {
	const CharString utf8 = p_text.utf8();
	return std::string(utf8.get_data(), size_t(utf8.length()));
}

String from_std_utf8(const std::string &p_text) {
	return String::utf8(p_text.data(), int(p_text.size()));
}

} // namespace LuaBridge
