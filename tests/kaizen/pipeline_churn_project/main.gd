extends Node3D

## Per-frame 3D pipeline churn. Each frame creates several MeshInstance3D with
## StandardMaterial3D variants that differ in the properties that affect the
## shader variant (transparency, culling, shading mode, depth draw, ...), draws
## them and frees them while the GPU may still reference the resulting pipelines.

const TOTAL_FRAMES := 120
const MESHES_PER_FRAME := 8

var _frame := 0
var _created := 0
var _freed := 0
var _frame_times := []

func _percentile(sorted_values: Array, p: float) -> float:
	if sorted_values.is_empty():
		return 0.0
	var idx := int(ceil(p * sorted_values.size())) - 1
	idx = clamp(idx, 0, sorted_values.size() - 1)
	return sorted_values[idx]

func _process(_delta: float) -> void:
	_frame_times.append(_delta * 1000.0)
	if _frame >= TOTAL_FRAMES:
		_frame_times.sort()
		var sum := 0.0
		for t in _frame_times:
			sum += t
		var mean_v := sum / _frame_times.size()
		var p50_v := _percentile(_frame_times, 0.5)
		var p95_v := _percentile(_frame_times, 0.95)
		var p99_v := _percentile(_frame_times, 0.99)
		var worst_v := _frame_times.back()
		print("pipeline churn: completed %d frames, created=%d freed=%d" % [_frame, _created, _freed])
		print("frame_ms: mean=%.3f p50=%.3f p95=%.3f p99=%.3f worst=%.3f" % [mean_v, p50_v, p95_v, p99_v, worst_v])
		get_tree().quit()
		return

	for i in MESHES_PER_FRAME:
		var mesh_instance := MeshInstance3D.new()
		mesh_instance.mesh = BoxMesh.new()

		var material := StandardMaterial3D.new()
		var variant := (_frame * MESHES_PER_FRAME + i) % 12
		match variant % 4:
			0:
				material.transparency = BaseMaterial3D.TRANSPARENCY_ALPHA
				material.blend_mode = BaseMaterial3D.BLEND_MODE_MIX
			1:
				material.transparency = BaseMaterial3D.TRANSPARENCY_DISABLED
				material.cull_mode = BaseMaterial3D.CULL_FRONT
			2:
				material.shading_mode = BaseMaterial3D.SHADING_MODE_UNSHADED
				material.emission_enabled = true
				material.emission = Color(1, 0.3, 0.1)
			3:
				material.shading_mode = BaseMaterial3D.SHADING_MODE_PER_PIXEL
				material.metallic = 1.0
				material.roughness = 0.1

		if variant % 3 == 0:
			material.depth_draw_mode = BaseMaterial3D.DEPTH_DRAW_ALWAYS
		if variant % 5 == 0:
			material.vertex_color_use_as_albedo = true

		material.albedo_color = Color.from_hsv(fmod(float(variant) / 12.0, 1.0), 0.6, 0.9)
		mesh_instance.material_override = material

		mesh_instance.position = Vector3(
			randf_range(-1.5, 1.5),
			randf_range(-1.0, 1.0),
			randf_range(-1.0, 1.0))
		add_child(mesh_instance)
		mesh_instance.queue_free()

		_created += 1
		_freed += 1

	_frame += 1
