--@extends SceneTree
--@export speed: float = 2.5

-- Engine-level regression check for the Lua 5.5 script language.
--
-- Run through tests/kaizen/run_lua_engine_test.sh: the editor binary loads this file as the
-- main loop (`--script`), so every check below runs against real Godot Variants, the real
-- Object and the real ScriptServer. The runner fails on any FAIL line or non-zero status.

local failures = 0

local function check(ok, message)
	print(string.format("  [%s] %s", ok and "ok" or "FAIL", message))
	if not ok then
		failures = failures + 1
	end
end

-- The default of the @export annotation above must reach the main script's environment.
local function check_export_default()
	check(speed == 2.5, "@export default applied to the main script (speed=" .. tostring(speed) .. ")")
end

local function check_variant_arithmetic()
	local sum = Vector2(1, 2) + Vector2(3, 4)
	check(sum.x == 4 and sum.y == 6, "Vector2 operator + (got " .. tostring(sum) .. ")")
end

local function check_table_round_trip()
	local root = self.root
	root:set_meta("lua_dict", { a = 1, b = "x" })
	local dict = root:get_meta("lua_dict")
	check(dict.a == 1 and dict.b == "x", "Lua table -> Dictionary -> Lua values")
	root:set_meta("lua_arr", { 10, 20, 30 })
	local arr = root:get_meta("lua_arr")
	check(#arr == 3 and arr[2] == 20, "Lua sequence -> Array, # and 1-based index")
	arr[#arr + 1] = 40
	check(#arr == 4 and arr[4] == 40, "t[#t + 1] = v appends to an Array")
end

local function check_callable_round_trip()
	local root = self.root
	root:set_meta("lua_fn", function(x) return x * 2 end)
	local fn = root:get_meta("lua_fn")
	check(fn(21) == 42, "Lua function -> Callable -> call from Lua")
end

local function check_errors_are_lua_errors()
	local root = self.root
	local ok = pcall(function() return root.no_such_member_xyz end)
	check(not ok, "missing property raises a Lua error")
	local ok2 = pcall(function() return root:no_such_method_xyz() end)
	check(not ok2, "missing method raises a Lua error")
end

local function check_variant_typo_is_an_error()
	-- A misspelt member on a non-Dictionary Variant must fail, not read as nil.
	local v = Vector2(3, 4)
	local ok = pcall(function() return v.nope_xyz end)
	check(not ok, "unknown member of a Variant raises a Lua error")
	check(v.x == 3, "known member of a Variant still reads")
end

local function check_dictionary_missing_key_is_nil()
	local root = self.root
	root:set_meta("lua_dict_nil", { a = 1 })
	local dict = root:get_meta("lua_dict_nil")
	check(dict.missing_xyz == nil, "missing Dictionary key reads as nil, like a Lua table")
end

local function check_create_and_free_object()
	local node = new("Node")
	check(node:get_class() == "Node", "new(\"Node\") creates a native object")
	node:set_name("lua_made")
	check(node.name == "lua_made", "properties of a created object are readable and writable")
	node:free()
	local ok = pcall(function() return new("NoSuchClassXyz") end)
	check(not ok, "new() of an unknown class raises a Lua error")
end

local function check_singletons()
	local ps = get_singleton("PhysicsServer3D")
	check(ps ~= nil and string.find(ps:get_class(), "PhysicsServer3D") ~= nil, "get_singleton(\"PhysicsServer3D\") returns the physics server")
	check(get_singleton("NoSuchSingletonXyz") == nil, "unknown singleton is nil")
end

local function check_class_globals()
	check(BaseMaterial3D.TRANSPARENCY_ALPHA == 1, "class constant BaseMaterial3D.TRANSPARENCY_ALPHA has its enum value")
	check(Image.FORMAT_RGBA8 ~= nil, "class constant Image.FORMAT_RGBA8 is readable")
	local sprite = Sprite2D.new()
	check(sprite:get_class() == "Sprite2D", "Class.new() creates an object of that class")
	sprite:free()
	local img = Image.create(4, 4, false, Image.FORMAT_RGBA8)
	check(img:get_width() == 4, "static class method Image.create() returns a usable object")
	local tex = ImageTexture.create_from_image(img)
	check(tex:get_width() == 4, "static class method ImageTexture.create_from_image() works")
	local rs_ok = RenderingServer.get_rendering_info(RenderingServer.RENDERING_INFO_PIPELINE_CACHE_LOGICAL_ACQUIRES) ~= nil
	check(rs_ok, "singleton instance method RenderingServer.get_rendering_info() is callable")
	local c = Color.from_hsv(0.0, 1.0, 1.0)
	check(c.r > 0.99 and c.g < 0.01, "built-in static Color.from_hsv() is callable")
	check(Vector2.ZERO.x == 0.0 and Vector2.ZERO.y == 0.0, "built-in constant Vector2.ZERO")
	check(Color(1, 0, 0).r == 1.0, "Color(...) still constructs")
	local r = randf_range(2.0, 3.0)
	check(r >= 2.0 and r <= 3.0, "randf_range() stays within its bounds")
	local ok = pcall(function() return Image.NO_SUCH_THING_XYZ end)
	check(not ok, "unknown class member raises a Lua error")
	local ok2 = pcall(function() return Color.no_such_static_xyz end)
	check(not ok2, "unknown Variant-type member raises a Lua error")
	check(NoSuchClassXyzGlobal == nil, "unknown non-class global stays nil")
end

local function check_callable_error_returns_nil()
	-- A Lua function that raises, called through a Callable, must not raise into the caller:
	-- the engine reports the error and the call returns nil (GDScript does the same).
	local root = self.root
	root:set_meta("lua_failing_fn", function() error("expected failure from a Callable") end)
	local fn = root:get_meta("lua_failing_fn")
	local ok, result = pcall(function() return fn:call() end)
	check(ok and result == nil, "a Lua error inside a Callable returns nil to the caller")
	check(root:get_class() == "Window", "owner is still usable after a Callable error")
end

local function check_concat_rejects_nil()
	-- Lua raises for `"a" .. nil`; a Variant operand must not silently turn nil into "<null>".
	local ok = pcall(function() return Vector2(1, 2) .. nil end)
	check(not ok, "Variant .. nil raises a Lua error (concat with nil is rejected)")
end

local function check_lua_error_keeps_instance_alive()
	local ok, err = pcall(function() error("expected from probe") end)
	check(not ok and string.find(err, "expected from probe") ~= nil, "plain Lua error carries its message")
	check(self ~= nil, "owner still reachable after a caught error")
end

local function check_preload_and_variant_constructors()
	local helper = preload("res://lua_preload_helper.lua")
	check(helper.answer == 42, "preload(res://) returns the chunk's return value")
	check(helper.make_vector().y == 4, "preload()ed functions can build Variants")
	local ok = pcall(preload, "res://no_such_preload_file_xyz.lua")
	check(not ok, "preload() of a missing file raises a Lua error")
	local v3 = Vector3(1, 2, 3)
	check(v3.z == 3.0, "Vector3(...) global constructor")
	check(PackedFloat32Array():size() == 0, "PackedFloat32Array() global constructor")
	check(StringName("kaizen") == "kaizen", "StringName(...) global constructor")
	check(vector2 == nil, "lowercase Variant names are not globals")
	local plain = Object.new()
	check(plain:get_class() == "Object", "Object resolves to the class, not a Variant constructor")
	plain:free()
end

-- A check that raises must count as a failure, not stop the probe before it quits.
local function run_check(name, fn)
	local ok, err = pcall(fn)
	if not ok then
		check(false, name .. " raised: " .. tostring(err))
	end
end

-- Runs once the main loop is set up, the same way a GDScript SceneTree does.
function _initialize()
	run_check("export", check_export_default)
	run_check("arithmetic", check_variant_arithmetic)
	run_check("table round trip", check_table_round_trip)
	run_check("callable round trip", check_callable_round_trip)
	run_check("errors", check_errors_are_lua_errors)
	run_check("variant typo", check_variant_typo_is_an_error)
	run_check("dictionary nil key", check_dictionary_missing_key_is_nil)
	run_check("create and free", check_create_and_free_object)
	run_check("singletons", check_singletons)
	run_check("class globals", check_class_globals)
	run_check("callable error", check_callable_error_returns_nil)
	run_check("concat", check_concat_rejects_nil)
	run_check("error recovery", check_lua_error_keeps_instance_alive)
	run_check("preload and constructors", check_preload_and_variant_constructors)

	print(string.format("LUA_ENGINE_PROBE: %s (%d failure(s))", failures == 0 and "PASS" or "FAIL", failures))
	self:quit(failures == 0 and 0 or 1)
end
