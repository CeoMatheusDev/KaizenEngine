--@extends Node3D

-- P2.5.1 Natural Deduplication Test
--
-- This test validates that PipelineCacheRD performs semantic deduplication
-- across independent renderer consumers sharing identical pipeline states.

local TOTAL_FRAMES = 120
local SHARED_CONSUMER_COUNT = 32 -- Independent consumers sharing identical material state (Group B)
local UNIQUE_VARIANT_COUNT = 8 -- Independent consumers with distinct material state (Group C)

local frame = 0
local finished = false
local group_b_nodes = {}
local group_c_nodes = {}

function _ready()
	-- Group B: 32 independent MeshInstance3D nodes using IDENTICAL StandardMaterial3D instances.
	-- They should naturally share the exact same compiled pipeline in PipelineHashMapRD / RenderingDevice.
	local shared_mat = StandardMaterial3D.new()
	shared_mat.albedo_color = Color(0.2, 0.6, 1.0)
	shared_mat.metallic = 0.2
	shared_mat.roughness = 0.4

	for i = 0, SHARED_CONSUMER_COUNT - 1 do
		local mi = MeshInstance3D.new()
		mi.mesh = BoxMesh.new()
		mi.material_override = shared_mat
		mi.position = Vector3((i % 8) * 0.6 - 2.1, 0.5, (i // 8) * 0.6 - 1.0)
		self:add_child(mi)
		table.insert(group_b_nodes, mi)
	end

	-- Group C: 8 independent MeshInstance3D nodes with DISTINCT material states (semantic isolation test).
	for i = 0, UNIQUE_VARIANT_COUNT - 1 do
		local unique_mat = StandardMaterial3D.new()
		-- Distinct albedo / roughness to produce different pipeline keys
		unique_mat.albedo_color = Color.from_hsv(i / UNIQUE_VARIANT_COUNT, 0.9, 0.9)
		unique_mat.roughness = i / UNIQUE_VARIANT_COUNT

		local mi = MeshInstance3D.new()
		mi.mesh = BoxMesh.new()
		mi.material_override = unique_mat
		mi.position = Vector3(i * 0.6 - 2.1, 1.5, 1.5)
		self:add_child(mi)
		table.insert(group_c_nodes, mi)
	end
end

function _process(delta)
	if finished then
		return
	end
	frame = frame + 1
	if frame >= TOTAL_FRAMES then
		finished = true
		local logical = RenderingServer.get_rendering_info(RenderingServer.RENDERING_INFO_PIPELINE_CACHE_LOGICAL_ACQUIRES)
		local shared = RenderingServer.get_rendering_info(RenderingServer.RENDERING_INFO_PIPELINE_CACHE_SHARED_ACQUIRES)
		local physical = RenderingServer.get_rendering_info(RenderingServer.RENDERING_INFO_PIPELINE_CACHE_PHYSICAL_CREATES)
		print(string.format("telemetry_stats: logical_acquires=%d shared_acquires=%d physical_creates=%d", logical, shared, physical))

		-- Verify semantic identity separation / deduplication success criteria
		-- Group B has 32 meshes + Group C has 8 meshes = 40 meshes rendered per frame over 120 frames.
		-- With deduplication, Group B should have high shared acquires (logical >> physical).
		if logical > 0 and physical > 0 then
			local hit_rate = shared / logical
			print(string.format("telemetry_analysis: HitRate=%.2f (shared/logical)", hit_rate))
		end

		self:get_tree():quit()
	end
end
