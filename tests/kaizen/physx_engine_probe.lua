--@extends SceneTree

-- Engine-level regression check for the PhysX 3D backend, written in Lua.
--
-- Run through tests/kaizen/run_physx_engine_test.sh, which selects the PhysX server in a
-- temporary project. Covers three regressions that the SDK smoke test cannot see:
--   1. World3D sets gravity by passing the SPACE rid to area_set_param; the backend must
--      route that to the space (it used to fail with "Parameter area is null").
--   2. The state-sync callback must receive a PhysicsDirectBodyState3D, otherwise
--      RigidBody3D nodes never follow the simulation.
--   3. A dynamic box must come to rest on a static floor.

local FLOOR_TOP = 0.5
local BOX_HALF = 0.5
local EXPECTED_REST_Y = FLOOR_TOP + BOX_HALF
local TOLERANCE = 0.05
local STEPS = 240

local box
local floor_body
local area_test_body
local space
local physics
local frames = 0
local failures = 0

local function check(ok, message)
	print(string.format("  [%s] %s", ok and "ok" or "FAIL", message))
	if not ok then
		failures = failures + 1
	end
end

function _initialize()
	local root = self.root

	floor_body = new("StaticBody3D")
	local floor_shape = new("CollisionShape3D")
	local floor_box = new("BoxShape3D")
	floor_box.size = Vector3(50, 1, 50)
	floor_shape.shape = floor_box
	floor_body.position = Vector3(0, FLOOR_TOP - 0.5, 0)
	floor_body:add_child(floor_shape)
	root:add_child(floor_body)

	box = new("RigidBody3D")
	local box_shape = new("CollisionShape3D")
	box_shape.shape = new("BoxShape3D")
	box:add_child(box_shape)
	box.position = Vector3(0, 5, 0)
	root:add_child(box)

	space = root:get_world_3d().space
	physics = get_singleton("PhysicsServer3D")

	check(physics:get_class() == "PhysXPhysicsServer3D", "PhysX server is active (got " .. physics:get_class() .. ")")
	local gravity = physics:area_get_param(space, physics.AREA_PARAM_GRAVITY_VECTOR)
	check(gravity.y < -0.99 and gravity.y > -1.01, "space gravity vector readback is (0,-1,0) (got " .. tostring(gravity) .. ")")
	local magnitude = physics:area_get_param(space, physics.AREA_PARAM_GRAVITY)
	check(magnitude > 9.0, "space gravity magnitude is set (got " .. tostring(magnitude) .. ")")

	-- A non-monitorable area must still participate in the physics override
	-- calculation; monitorability controls notifications, not simulation.
	local gravity_area = new("Area3D")
	local area_shape = new("CollisionShape3D")
	local area_box = new("BoxShape3D")
	area_box.size = Vector3(12, 20, 12)
	area_shape.shape = area_box
	gravity_area.position = Vector3(10, 0, 0)
	gravity_area.monitorable = false
	gravity_area:add_child(area_shape)
	root:add_child(gravity_area)
	local area_rid = gravity_area:get_rid()
	physics:area_set_param(area_rid, physics.AREA_PARAM_GRAVITY_OVERRIDE_MODE, physics.AREA_SPACE_OVERRIDE_REPLACE)
	physics:area_set_param(area_rid, physics.AREA_PARAM_GRAVITY, 20.0)
	physics:area_set_param(area_rid, physics.AREA_PARAM_GRAVITY_VECTOR, Vector3.UP)
	check(physics:area_get_param(area_rid, physics.AREA_PARAM_GRAVITY_OVERRIDE_MODE) == physics.AREA_SPACE_OVERRIDE_REPLACE,
		"area gravity override mode is stored and readable")
	physics:area_set_param(area_rid, physics.AREA_PARAM_LINEAR_DAMP_OVERRIDE_MODE, physics.AREA_SPACE_OVERRIDE_REPLACE)
	physics:area_set_param(area_rid, physics.AREA_PARAM_LINEAR_DAMP, 0.25)
	check(math.abs(physics:area_get_param(area_rid, physics.AREA_PARAM_LINEAR_DAMP) - 0.25) < 0.001,
		"area linear damping override is stored and readable")

	area_test_body = new("RigidBody3D")
	local area_body_shape = new("CollisionShape3D")
	area_body_shape.shape = new("BoxShape3D")
	area_test_body:add_child(area_body_shape)
	area_test_body.position = Vector3(10, 2, 0)
	root:add_child(area_test_body)
end

function _physics_process(delta)
	frames = frames + 1
	if frames < STEPS then
		return
	end
	local node_y = box.global_position.y
	local server_y = physics:body_get_state(box:get_rid(), physics.BODY_STATE_TRANSFORM).origin.y
	check(math.abs(node_y - EXPECTED_REST_Y) <= TOLERANCE, string.format("box rests on floor (node y=%.3f, expected %.3f)", node_y, EXPECTED_REST_Y))
	check(math.abs(node_y - server_y) <= TOLERANCE, string.format("node follows server state (node y=%.3f, server y=%.3f)", node_y, server_y))
	local direct_space = self.root:get_world_3d().direct_space_state
	local ray_params = PhysicsRayQueryParameters3D.create(Vector3(0, 4, 0), Vector3(0, -1, 0))
	local ray_hit = direct_space:intersect_ray(ray_params)
	check(ray_hit.rid == box:get_rid(), "ray query finds the first body using its query collision data")
	ray_params.exclude = { box:get_rid() }
	local floor_hit = direct_space:intersect_ray(ray_params)
	check(floor_hit.rid == floor_body:get_rid(), "ray query excludes the selected body and continues to the floor")
	local area_body_y = area_test_body.global_position.y
	check(area_body_y > 5.0, string.format("non-monitorable area applies replacement gravity (body y=%.3f)", area_body_y))
	print(string.format("PHYSX_ENGINE_PROBE: %s (%d failure(s))", failures == 0 and "PASS" or "FAIL", failures))
	self:call_deferred("quit", failures == 0 and 0 or 1)
end
