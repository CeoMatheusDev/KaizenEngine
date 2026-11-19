extends Node3D
## S6 "Transparency and post-processing": overdraw and post-effect cost.
##
## Ground plus 32 opaque boxes using the S1 material, then --bench-count
## transparent quads stacked between z = 10 m and z = 25 m. Each quad covers
## most of the viewport, so the count is a direct overdraw multiplier.
##
## --bench-preset P0..P5 selects the post preset from the specification. The
## environment values are recorded in scene.parameters.post, because the report
## must show what actually ran rather than what the label claimed.
##
## See docs/architecture/rendering-benchmark-scenes.md, section "S6 - Transparency and post-processing".

const Common = preload("res://scripts/bench_scene_common.gd")

const LOOP_FRAMES := 300
const BOX_COLUMNS := 8
const BOX_ROWS := 4
const BOX_COUNT := BOX_COLUMNS * BOX_ROWS
const SPACING := 6.0
const BOX_SIZE := 2.0
const GROUND_SIZE := 400.0
const QUAD_SIZE := 40.0
const QUAD_ALPHA := 0.35
const QUAD_START_Z := 10.0
const QUAD_STEP_Z := 1.0
const CAMERA_POSITION := Vector3(0.0, 8.0, 40.0)
const CAMERA_LOOK_AT := Vector3(0.0, 2.0, 0.0)
const CAMERA_FOV := 75.0

## Fixed post values from the specification.
const GLOW_INTENSITY := 0.8
const GLOW_BLOOM := 0.1
const GLOW_HDR_THRESHOLD := 1.0
const SSAO_RADIUS := 1.0
const SSAO_INTENSITY := 2.0
const SSAO_POWER := 1.5
const SSAO_DETAIL := 0.5
const FOG_DENSITY := 0.01
const FOG_LENGTH := 64.0
const FOG_DETAIL_SPREAD := 1.5

## Overridden by the harness before this node is ready.
@export var quad_count: int = 4
@export var post_preset: String = "P0"

var _camera: Camera3D
var _post: Dictionary = {}

func _ready() -> void:
	_apply_harness_overrides()
	_build()

func _apply_harness_overrides() -> void:
	var harness := get_node_or_null("/root/BenchHarness")
	if harness == null:
		return
	var count: Variant = harness.call("param", "count", quad_count)
	if int(count) > 0:
		quad_count = int(count)
	var preset := String(harness.call("param", "preset", post_preset))
	if preset.begins_with("P"):
		post_preset = preset

func _build() -> void:
	quad_count = maxi(quad_count, 0)

	var material := Common.make_checker_material()
	_add_ground(material)
	_add_boxes(material)
	_add_quads()
	add_child(Common.make_sun())
	_add_environment()
	_add_camera()

func _add_ground(p_material: StandardMaterial3D) -> void:
	var mesh := PlaneMesh.new()
	mesh.size = Vector2(GROUND_SIZE, GROUND_SIZE)
	mesh.material = p_material

	var node := MeshInstance3D.new()
	node.name = "Ground"
	node.mesh = mesh
	add_child(node)

func _add_boxes(p_material: StandardMaterial3D) -> void:
	var mesh := BoxMesh.new()
	mesh.size = Vector3(BOX_SIZE, BOX_SIZE, BOX_SIZE)
	mesh.material = p_material

	var offset_x := (float(BOX_COLUMNS) - 1.0) * 0.5
	var offset_z := (float(BOX_ROWS) - 1.0) * 0.5
	for index in BOX_COUNT:
		var column := index % BOX_COLUMNS
		var row := int(index / BOX_COLUMNS)
		var node := MeshInstance3D.new()
		node.name = "Box%03d" % index
		node.mesh = mesh
		node.position = Vector3(
				(float(column) - offset_x) * SPACING,
				BOX_SIZE * 0.5,
				(float(row) - offset_z) * SPACING)
		add_child(node)

func _add_quads() -> void:
	var material := StandardMaterial3D.new()
	material.transparency = BaseMaterial3D.TRANSPARENCY_ALPHA
	material.shading_mode = BaseMaterial3D.SHADING_MODE_UNSHADED
	material.cull_mode = BaseMaterial3D.CULL_DISABLED
	material.albedo_color = Color(0.7, 0.8, 0.9, QUAD_ALPHA)

	var mesh := QuadMesh.new()
	mesh.size = Vector2(QUAD_SIZE, QUAD_SIZE)
	mesh.material = material

	for index in quad_count:
		var z := QUAD_START_Z + float(index) * QUAD_STEP_Z
		var quad_position := _view_axis_point(z)
		var node := MeshInstance3D.new()
		node.name = "Quad%03d" % index
		node.mesh = mesh
		# Basis.looking_at makes -Z point along the given direction, so passing
		# quad - camera leaves +Z (the QuadMesh face) pointing at the camera.
		node.transform = Transform3D(
				Basis.looking_at(quad_position - CAMERA_POSITION, Vector3.UP),
				quad_position)
		add_child(node)

## Point on the camera's view axis at the given z, so every quad is centred in
## the frame instead of drifting as the stack gets closer to the camera.
func _view_axis_point(p_z: float) -> Vector3:
	var t := (CAMERA_POSITION.z - p_z) / (CAMERA_POSITION.z - CAMERA_LOOK_AT.z)
	return CAMERA_POSITION.lerp(CAMERA_LOOK_AT, t)

func _add_environment() -> void:
	if post_preset not in ["P0", "P1", "P2", "P3", "P4", "P5"]:
		post_preset = "P0"

	var environment := Common.make_environment()
	_post = {
		"preset": post_preset,
		"glow": false,
		"ssao": false,
		"ssil": false,
		"volumetric_fog": false,
		"support": _post_support(),
	}

	match post_preset:
		"P1":
			_enable_glow(environment)
		"P2":
			_enable_ssao(environment)
		"P3":
			_enable_ssao(environment)
			environment.ssil_enabled = true
			_post.ssil = true
		"P4":
			_enable_glow(environment)
			_enable_ssao(environment)
			environment.ssil_enabled = true
			_post.ssil = true
		"P5":
			_enable_glow(environment)
			_enable_ssao(environment)
			environment.ssil_enabled = true
			environment.volumetric_fog_enabled = true
			environment.volumetric_fog_density = FOG_DENSITY
			environment.volumetric_fog_length = FOG_LENGTH
			environment.volumetric_fog_detail_spread = FOG_DETAIL_SPREAD
			_post.ssil = true
			_post.volumetric_fog = true

	var node := WorldEnvironment.new()
	node.name = "Environment"
	node.environment = environment
	add_child(node)

func _enable_glow(p_environment: Environment) -> void:
	p_environment.glow_enabled = true
	p_environment.glow_intensity = GLOW_INTENSITY
	p_environment.glow_bloom = GLOW_BLOOM
	p_environment.glow_hdr_threshold = GLOW_HDR_THRESHOLD
	_post.glow = true

func _enable_ssao(p_environment: Environment) -> void:
	p_environment.ssao_enabled = true
	p_environment.ssao_radius = SSAO_RADIUS
	p_environment.ssao_intensity = SSAO_INTENSITY
	p_environment.ssao_power = SSAO_POWER
	p_environment.ssao_detail = SSAO_DETAIL
	_post.ssao = true

## Which effects the active renderer can actually produce, read from the engine
## source rather than assumed:
## - glow: all three renderers.
## - ssao: forward_plus and gl_compatibility. GLES3 has its own SSAO (see the
##   ssao_enabled path in drivers/gles3/rasterizer_scene_gles3.cpp); the Mobile
##   backend stubs environment_set_ssao_quality and S6/P2 there is byte-identical
##   to P0.
## - ssil and volumetric_fog: forward_plus only. The GLES3
##   environment_set_ssil_quality body is empty and there is no volumetric fog
##   implementation; Mobile stubs both quality setters and returns false from
##   is_volumetric_supported().
## Claiming less than the backend can do is not harmless: it turns a measurable
## cell into a "not applicable" one.
func _post_support() -> Dictionary:
	var method := RenderingServer.get_current_rendering_method()
	return {
		"glow": true,
		"ssao": method == "forward_plus" or method == "gl_compatibility",
		"ssil": method == "forward_plus",
		"volumetric_fog": method == "forward_plus",
	}

## Read by the harness after the scene loads: a preset that asks for an effect
## the active renderer cannot provide is reported as invalid instead of as a
## measurement of nothing.
func unmet_requirements() -> Array:
	var unmet := []
	var support: Dictionary = _post.get("support", {})
	for effect in ["glow", "ssao", "ssil", "volumetric_fog"]:
		if bool(_post.get(effect, false)) and not bool(support.get(effect, true)):
			unmet.append("post effect '%s' is not supported by the %s renderer" % [
					effect, RenderingServer.get_current_rendering_method()])
	return unmet

func _add_camera() -> void:
	_camera = Camera3D.new()
	_camera.name = "BenchCamera"
	_camera.fov = CAMERA_FOV
	_camera.near = 0.05
	_camera.far = 500.0
	add_child(_camera)
	_camera.current = true
	_camera.global_position = CAMERA_POSITION
	_camera.look_at(CAMERA_LOOK_AT, Vector3.UP)

## Static camera: two identical keyframes, as the specification requires.
func apply_frame(_p_frame: int) -> void:
	_camera.global_position = CAMERA_POSITION
	_camera.look_at(CAMERA_LOOK_AT, Vector3.UP)

func scene_parameters() -> Dictionary:
	return {
		"quad_count": quad_count,
		"box_count": BOX_COUNT,
		"spacing": SPACING,
		"box_size": BOX_SIZE,
		"ground_size": GROUND_SIZE,
		"quad_size": QUAD_SIZE,
		"quad_alpha": QUAD_ALPHA,
		"quad_start_z": QUAD_START_Z,
		"quad_step_z": QUAD_STEP_Z,
		"camera_position": [CAMERA_POSITION.x, CAMERA_POSITION.y, CAMERA_POSITION.z],
		"camera_look_at": [CAMERA_LOOK_AT.x, CAMERA_LOOK_AT.y, CAMERA_LOOK_AT.z],
		"camera_fov": CAMERA_FOV,
		"loop_frames": LOOP_FRAMES,
		"post": _post,
	}
