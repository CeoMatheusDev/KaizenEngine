extends Node3D
## S3 "Material variety": uniform variety (S3a) and shader/PSO variety (S3b).
##
## 64 separate MeshInstance3D nodes share one sphere geometry. Each node gets a
## material override, so the material set controls the draw and PSO behaviour
## without duplicating meshes. S3a varies only albedo/roughness/metallic across
## N materials; S3b cycles eight shader-feature combinations.
##
## The camera is frame-indexed and follows the keyframe contract: a +/-10 m
## lateral triangle sweep over the 300-frame loop.
##
## See docs/architecture/rendering-benchmark-scenes.md, section "S3 - Material variety".

const Common = preload("res://scripts/bench_scene_common.gd")

const LOOP_FRAMES := 300
const GRID_SIDE := 8
const NODE_COUNT := GRID_SIDE * GRID_SIDE
const SPACING := 3.0
const CAMERA_POSITION := Vector3(0.0, 8.0, 25.0)
const CAMERA_LOOK_AT := Vector3(0.0, 3.0, 0.0)
const CAMERA_FOV := 75.0
const SWEEP_KEYS := [0.0, 10.0, 0.0, -10.0]

## S3a: distinct materials to create. Ignored when feature_variety is true,
## which always uses the eight feature combinations.
@export var material_count: int = 64
## S3b: cycle normal/ORM/emission/rim/clearcoat/anisotropy/alpha features
## instead of varying only uniforms. `--bench-variant b` selects it.
@export var feature_variety: bool = false

var _camera: Camera3D
var _materials_used: int = 0

func _ready() -> void:
	_apply_harness_overrides()
	_build()

func _apply_harness_overrides() -> void:
	var harness := get_node_or_null("/root/BenchHarness")
	if harness == null:
		return
	var count: Variant = harness.call("param", "count", material_count)
	if int(count) > 0:
		material_count = int(count)
	var variant := String(harness.call("param", "variant", ""))
	if not variant.is_empty():
		feature_variety = variant.to_lower() == "b"

func _build() -> void:
	var materials := _make_materials()
	var sphere := Common.make_bench_sphere(null)

	var offset := (float(GRID_SIDE) - 1.0) * 0.5
	for index in NODE_COUNT:
		var column := index % GRID_SIDE
		var row := int(index / GRID_SIDE)
		var node := MeshInstance3D.new()
		node.mesh = sphere
		node.material_override = materials[index % materials.size()]
		node.position = Vector3(
				(float(column) - offset) * SPACING,
				Common.SPHERE_RADIUS,
				(float(row) - offset) * SPACING)
		add_child(node)

	add_child(Common.make_sun())
	Common.add_world_environment(self)
	_add_camera()

func _make_materials() -> Array[BaseMaterial3D]:
	var materials: Array[BaseMaterial3D] = []

	if feature_variety:
		var normal := Common.make_normal_texture()
		var orm := Common.make_orm_texture()
		for feature in 8:
			materials.append(Common.make_feature_material(feature, normal, orm))
		_materials_used = materials.size()
		return materials

	var count := clampi(material_count, 1, NODE_COUNT)
	for index in count:
		materials.append(Common.make_variation_material(index))
	_materials_used = materials.size()
	return materials

func _add_camera() -> void:
	_camera = Camera3D.new()
	_camera.name = "BenchCamera"
	_camera.fov = CAMERA_FOV
	_camera.near = 0.05
	_camera.far = 500.0
	add_child(_camera)
	_camera.current = true

func apply_frame(p_frame: int) -> void:
	var t := float(p_frame % LOOP_FRAMES) / float(LOOP_FRAMES) * float(SWEEP_KEYS.size())
	var index := int(floor(t))
	var fraction := t - float(index)
	var x0: float = SWEEP_KEYS[index]
	var x1: float = SWEEP_KEYS[(index + 1) % SWEEP_KEYS.size()]
	_camera.global_position = Vector3(lerpf(x0, x1, fraction), CAMERA_POSITION.y, CAMERA_POSITION.z)
	_camera.look_at(CAMERA_LOOK_AT, Vector3.UP)

func scene_parameters() -> Dictionary:
	return {
		"material_count": _materials_used,
		"feature_variety": feature_variety,
		"node_count": NODE_COUNT,
		"grid_side": GRID_SIDE,
		"spacing": SPACING,
		"sphere_radius": Common.SPHERE_RADIUS,
		"sphere_height": Common.SPHERE_HEIGHT,
		"sphere_radial_segments": Common.SPHERE_RADIAL_SEGMENTS,
		"sphere_rings": Common.SPHERE_RINGS,
		"camera_position": [CAMERA_POSITION.x, CAMERA_POSITION.y, CAMERA_POSITION.z],
		"camera_look_at": [CAMERA_LOOK_AT.x, CAMERA_LOOK_AT.y, CAMERA_LOOK_AT.z],
		"camera_sweep_x": SWEEP_KEYS,
		"camera_fov": CAMERA_FOV,
		"loop_frames": LOOP_FRAMES,
	}
