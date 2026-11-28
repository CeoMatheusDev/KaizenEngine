/**************************************************************************/
/*  test_lua_metadata.cpp                                                 */
/**************************************************************************/
/*  Lua script module for KaizenEngine.                                   */
/**************************************************************************/

#include "modules/lua_script/lua_script_metadata.h"

#include <cstdio>
#include <string>

namespace {

int failures = 0;
int checks = 0;

void check_impl(bool p_condition, const char *p_expression, const char *p_test, int p_line) {
	checks++;
	if (!p_condition) {
		failures++;
		std::fprintf(stderr, "FAIL [%s] %s:%d: %s\n", p_test, __FILE__, p_line, p_expression);
	}
}

#define CHECK(cond) check_impl((cond), #cond, test_name, __LINE__)
#define CHECK_EQ(a, b) check_impl((a) == (b), #a " == " #b, test_name, __LINE__)

const char *SCRIPT =
		"--@tool\n"
		"--@extends Node2D\n"
		"--@class_name Player\n"
		"\n"
		"--@export speed: float = 120.5\n"
		"--@export health: int = 3\n"
		"--@export label\n"
		"--@signal died(reason, amount)\n"
		"--@signal jumped\n"
		"\n"
		"-- A plain comment that mentions function and --@export is ignored.\n"
		"local helper = 1\n"
		"function _ready()\n"
		"\tself.position = Vector2(1, 2)\n"
		"end\n"
		"\n"
		"function take_damage(amount, source)\n"
		"\thealth = health - amount\n"
		"end\n"
		"\n"
		"local function private_helper(x)\n"
		"end\n"
		"\n"
		"on_hit = function(body, normal)\n"
		"end\n"
		"\n"
		"  function nested_indented()\n"
		"  end\n";

} // namespace

static void test_annotations() {
	const char *test_name = "test_annotations";
	const LuaScriptMetadata::Metadata meta = LuaScriptMetadata::parse(SCRIPT);

	CHECK(meta.tool);
	CHECK_EQ(meta.extends, std::string("Node2D"));
	CHECK_EQ(meta.class_name, std::string("Player"));
}

static void test_exports() {
	const char *test_name = "test_exports";
	const LuaScriptMetadata::Metadata meta = LuaScriptMetadata::parse(SCRIPT);

	CHECK_EQ(meta.exports.size(), size_t(3));
	if (meta.exports.size() != 3) {
		return;
	}
	CHECK_EQ(meta.exports[0].name, std::string("speed"));
	CHECK_EQ(meta.exports[0].type, std::string("float"));
	CHECK_EQ(meta.exports[0].default_expr, std::string("120.5"));
	CHECK_EQ(meta.exports[0].line, 5);

	CHECK_EQ(meta.exports[1].name, std::string("health"));
	CHECK_EQ(meta.exports[1].type, std::string("int"));
	CHECK_EQ(meta.exports[1].default_expr, std::string("3"));

	// Untyped, default-less export: the engine infers the type from the value.
	CHECK_EQ(meta.exports[2].name, std::string("label"));
	CHECK(meta.exports[2].type.empty());
	CHECK(meta.exports[2].default_expr.empty());
}

static void test_signals() {
	const char *test_name = "test_signals";
	const LuaScriptMetadata::Metadata meta = LuaScriptMetadata::parse(SCRIPT);

	CHECK_EQ(meta.signals.size(), size_t(2));
	if (meta.signals.size() != 2) {
		return;
	}
	CHECK_EQ(meta.signals[0].name, std::string("died"));
	CHECK_EQ(meta.signals[0].params.size(), size_t(2));
	CHECK_EQ(meta.signals[1].name, std::string("jumped"));
	CHECK(meta.signals[1].params.empty());
}

static void test_functions() {
	const char *test_name = "test_functions";
	const LuaScriptMetadata::Metadata meta = LuaScriptMetadata::parse(SCRIPT);

	// Top-level definitions only: `local function` and indented functions are not members.
	CHECK_EQ(meta.functions.size(), size_t(3));
	if (meta.functions.size() != 3) {
		return;
	}
	CHECK_EQ(meta.functions[0].name, std::string("_ready"));
	CHECK(meta.functions[0].params.empty());
	CHECK_EQ(meta.functions[1].name, std::string("take_damage"));
	CHECK_EQ(meta.functions[1].params.size(), size_t(2));
	CHECK_EQ(meta.functions[1].params[1], std::string("source"));
	CHECK_EQ(meta.functions[2].name, std::string("on_hit"));
	CHECK_EQ(meta.functions[2].params.size(), size_t(2));
	CHECK_EQ(meta.functions[2].line, 24);
}

static void test_long_comments_and_strings_are_ignored() {
	const char *test_name = "test_long_comments_and_strings_are_ignored";
	const char *source = R"lua(local example = [==[
function fake_string() end
--@class_name FakeString
]==]
--[=[
function fake_comment() end
--@export hidden: int = 9
]=]
function actual()
end
)lua";
	const LuaScriptMetadata::Metadata meta = LuaScriptMetadata::parse(source);
	CHECK(meta.class_name.empty());
	CHECK(meta.exports.empty());
	CHECK_EQ(meta.functions.size(), size_t(1));
	if (meta.functions.size() == 1) {
		CHECK_EQ(meta.functions[0].name, std::string("actual"));
		CHECK_EQ(meta.functions[0].line, 9);
	}

	const char *continued_string = R"lua(local text = "continued\
function false_positive() end"
function real_method()
end
)lua";
	const LuaScriptMetadata::Metadata continued = LuaScriptMetadata::parse(continued_string);
	CHECK_EQ(continued.functions.size(), size_t(1));
	if (continued.functions.size() == 1) {
		CHECK_EQ(continued.functions[0].name, std::string("real_method"));
		CHECK_EQ(continued.functions[0].line, 3);
	}
}

static void test_lookup_and_empty() {
	const char *test_name = "test_lookup_and_empty";
	CHECK_EQ(LuaScriptMetadata::find_function_line(SCRIPT, "take_damage"), 17);
	CHECK_EQ(LuaScriptMetadata::find_function_line(SCRIPT, "missing"), -1);
	CHECK_EQ(LuaScriptMetadata::find_function_line(SCRIPT, "private_helper"), -1);

	const LuaScriptMetadata::Metadata empty = LuaScriptMetadata::parse("");
	CHECK(!empty.tool);
	CHECK(empty.extends.empty());
	CHECK(empty.exports.empty());
	CHECK(empty.functions.empty());

	// CRLF line endings must not leak carriage returns into names or expressions.
	const LuaScriptMetadata::Metadata crlf = LuaScriptMetadata::parse("--@export a: int = 1\r\nfunction f(x)\r\nend\r\n");
	CHECK_EQ(crlf.exports.size(), size_t(1));
	if (crlf.exports.size() == 1) {
		CHECK_EQ(crlf.exports[0].default_expr, std::string("1"));
	}
	CHECK_EQ(crlf.functions.size(), size_t(1));
	if (crlf.functions.size() == 1) {
		CHECK_EQ(crlf.functions[0].params.size(), size_t(1));
	}
}

int main() {
	test_annotations();
	test_exports();
	test_signals();
	test_functions();
	test_long_comments_and_strings_are_ignored();
	test_lookup_and_empty();

	if (failures == 0) {
		std::printf("lua_metadata: %d checks, 0 failures\n", checks);
		return 0;
	}
	std::printf("lua_metadata: %d checks, %d failure(s)\n", checks, failures);
	return 1;
}
