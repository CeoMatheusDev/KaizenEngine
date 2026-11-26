--@extends SceneTree

-- Engine-level regression check for the C++ script language.
--
-- Run through tests/kaizen/run_cpp_engine_test.sh. The runner writes the C++ fixtures into a
-- temporary project and starts this file as the main loop. The probe loads the fixtures, drives
-- a node through the lifecycle, reloads the script, and reports PASS or FAIL. The runner checks
-- the "[C++] ..." lines the scripts print and the errors the engine logs.

local failures = 0
local frames = 0
local node = nil
local ok_script = nil

-- Source the probe reloads into the running script. Written inline: it is never saved to disk.
local V2_SOURCE = [==[
#include "kaizen_cpp.h"

class OkScript : public kaizen::Script {
public:
	void _enter_tree() override { log("enter_tree v2"); }
	void _ready() override { log("ready v2"); }
	void _exit_tree() override { log("exit_tree v2"); }
};

KAIZEN_CPP_SCRIPT(OkScript)
]==]

local BROKEN_SOURCE = [==[
#include "kaizen_cpp.h"

class Broken : public kaizen::Script {
	void _ready() override { this is not C++ }
};

KAIZEN_CPP_SCRIPT(Broken)
]==]

local function check(ok, message)
	print(string.format("  [%s] %s", ok and "ok" or "FAIL", message))
	if not ok then
		failures = failures + 1
	end
end

local function check_class_registered()
	check(ClassDB.class_exists("CppScript"), "the module registers the CppScript class")
end

local function check_loading()
	ok_script = ResourceLoader.load("res://cpp_ok.cpp")
	check(ok_script ~= nil and ok_script:get_class() == "CppScript", "res://cpp_ok.cpp loads as a CppScript")
	check(ok_script ~= nil and ok_script:can_instantiate(), "res://cpp_ok.cpp compiles and can be instantiated")

	local broken = ResourceLoader.load("res://cpp_broken.cpp")
	check(broken ~= nil, "a .cpp that fails to compile still loads, so the editor can show it")
	check(broken ~= nil and not broken:can_instantiate(), "a .cpp that fails to compile cannot be instantiated")

	local noentry = ResourceLoader.load("res://cpp_noentry.cpp")
	check(noentry ~= nil and not noentry:can_instantiate(), "a .cpp without KAIZEN_CPP_SCRIPT cannot be instantiated")
end

local function check_instance()
	node = new("Node")
	node:set_name("cpp_probe")
	node:set_script(ok_script)
	check(node:get_script() ~= nil, "a node accepts the C++ script")
	self.root:add_child(node)
end

-- A check that raises must count as a failure, so the probe still reaches its quit below.
local function run_check(name, fn)
	local ok, err = pcall(fn)
	if not ok then
		check(false, name .. " raised: " .. tostring(err))
	end
end

-- Runs once the main loop is set up, the same way a GDScript SceneTree does.
function _initialize()
	run_check("class", check_class_registered)
	run_check("loading", check_loading)
	if ok_script ~= nil then
		run_check("instance", check_instance)
	end
end

function _process(delta)
	frames = frames + 1
	if ok_script == nil then
		-- Nothing to run: report the failure at the frame the normal run would have quit.
		if frames >= 2 then
			print("CPP_ENGINE_PROBE: FAIL (no script to run)")
			self:quit(1)
		end
		return false
	end
	if frames == 3 then
		-- A broken reload fails and leaves the running library in place.
		ok_script:set_source_code(BROKEN_SOURCE)
		local broken_err = ok_script:reload(true)
		check(broken_err ~= 0, "reload of a broken .cpp reports an error (code " .. tostring(broken_err) .. ")")
		-- A fixed source reloads, and the node in the tree gets the new object straight away.
		ok_script:set_source_code(V2_SOURCE)
		local fixed_err = ok_script:reload(true)
		check(fixed_err == 0, "reload of a fixed .cpp succeeds")
	elseif frames == 6 then
		self.root:remove_child(node)
		node:free()
	elseif frames == 8 then
		print(string.format("CPP_ENGINE_PROBE: %s (%d failure(s))", failures == 0 and "PASS" or "FAIL", failures))
		self:quit(failures == 0 and 0 or 1)
	end
	return false
end
