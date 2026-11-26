--@extends Node3D

-- Per-frame 3D pipeline churn. Each frame creates several MeshInstance3D with
-- StandardMaterial3D variants that differ in the properties that affect the
-- shader variant (transparency, culling, shading mode, depth draw, ...), draws
-- them and frees them while the GPU may still reference the resulting pipelines.

local TOTAL_FRAMES = 120
local MESHES_PER_FRAME = 8
local CANONICAL_COUNT = 12
local CANONICAL_PER_FRAME = 4
local CHURN_PER_FRAME = 4

local frame = 0
local finished = false
local created = 0
local freed = 0
local canonical_reuses = 0
local frame_times = {}
local canonical_materials = {}

local function percentile(sorted_values, p)
	if #sorted_values == 0 then
		return 0.0
	end
	local idx = math.ceil(p * #sorted_values) -- 1-based index
	idx = math.max(1, math.min(idx, #sorted_values))
	return sorted_values[idx]
end

-- Applies the property set for `variant` (0..11). Shared by the canonical pool and the churn group.
local function apply_variant(mat, variant)
	local family = variant % 4
	if family == 0 then
		mat.transparency = BaseMaterial3D.TRANSPARENCY_ALPHA
		mat.blend_mode = BaseMaterial3D.BLEND_MODE_MIX
	elseif family == 1 then
		mat.transparency = BaseMaterial3D.TRANSPARENCY_DISABLED
		mat.cull_mode = BaseMaterial3D.CULL_FRONT
	elseif family == 2 then
		mat.shading_mode = BaseMaterial3D.SHADING_MODE_UNSHADED
		mat.emission_enabled = true
		mat.emission = Color(1, 0.3, 0.1)
	else
		mat.shading_mode = BaseMaterial3D.SHADING_MODE_PER_PIXEL
		mat.metallic = 1.0
		mat.roughness = 0.1
	end
	if variant % 3 == 0 then
		mat.depth_draw_mode = BaseMaterial3D.DEPTH_DRAW_ALWAYS
	end
	if variant % 5 == 0 then
		mat.vertex_color_use_as_albedo = true
	end
end

function _ready()
	-- Build canonical material pool with deterministic variants
	for i = 0, CANONICAL_COUNT - 1 do
		local mat = StandardMaterial3D.new()
		local variant = i % 12
		apply_variant(mat, variant)
		mat.albedo_color = Color.from_hsv((variant / 12.0) % 1.0, 0.6, 0.9)
		table.insert(canonical_materials, mat)
	end
end

function _process(delta)
	if finished then
		return
	end
	table.insert(frame_times, delta * 1000.0)
	if frame >= TOTAL_FRAMES then
		finished = true
		table.sort(frame_times)
		local sum = 0.0
		for _, t in ipairs(frame_times) do
			sum = sum + t
		end
		local mean_v = sum / #frame_times
		local p50_v = percentile(frame_times, 0.5)
		local p95_v = percentile(frame_times, 0.95)
		local p99_v = percentile(frame_times, 0.99)
		local worst_v = frame_times[#frame_times]
		print(string.format("pipeline churn: completed %d frames, created=%d freed=%d canonical_reuses=%d", frame, created, freed, canonical_reuses))
		print(string.format("frame_ms: mean=%.3f p50=%.3f p95=%.3f p99=%.3f worst=%.3f", mean_v, p50_v, p95_v, p99_v, worst_v))
		self:get_tree():quit()
		return
	end

	for i = 0, MESHES_PER_FRAME - 1 do
		local mesh_instance = MeshInstance3D.new()
		mesh_instance.mesh = BoxMesh.new()

		local material
		if i < CANONICAL_PER_FRAME then
			-- Canonical group: reuse a fixed pool of materials.
			-- Exercises stability and re-binding of the same logical
			-- pipeline identity across frames.
			local canonical_index = (frame + i) % CANONICAL_COUNT
			material = canonical_materials[canonical_index + 1]
			canonical_reuses = canonical_reuses + 1
		else
			-- Churn group: fresh material every frame.
			-- Preserves creation/destruction pressure.
			material = StandardMaterial3D.new()
			local variant = (frame * MESHES_PER_FRAME + i) % 12
			apply_variant(material, variant)
			material.albedo_color = Color.from_hsv((variant / 12.0) % 1.0, 0.6, 0.9)
		end

		mesh_instance.material_override = material
		mesh_instance.position = Vector3(randf_range(-1.5, 1.5), randf_range(-1.0, 1.0), randf_range(-1.0, 1.0))
		self:add_child(mesh_instance)
		mesh_instance:queue_free()

		created = created + 1
		freed = freed + 1
	end

	frame = frame + 1
end
