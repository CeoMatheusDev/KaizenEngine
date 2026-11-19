extends Node3D
## S2 "Instancing": draw-call scaling with a single MultiMesh.
##
## The scene is deliberately one MultiMesh: if draw calls grow with the instance
## count, the scene is misconfigured and the cell must be rejected rather than
## reported. Instance positions are deterministic integer-hash jitter inside a
## regular grid; no rotation, no per-instance color in the baseline.
##
## See docs/architecture/rendering-benchmark-scenes.md, section "S2 - Instancing".

const Common = preload("res://scripts/bench_scene_common.gd")

const LOOP_FRAMES := 300
const SPACING := 2.5
const SPHERE_RADIUS := 0.5
const SPHERE_HEIGHT := 1.0
const SPHERE_SEGMENTS := 32
const SPHERE_RINGS := 16
const JITTER_RATIO := 0.1

## Overridden by --bench-count before this node is ready.
@export var instance_count: int = 4096

var _camera: Camera3D
var _side: int = 0
var _extent: float = 0.0
var _radius: float = 0.0
var _height: float = 0.0
var _look_target := Vector3(0.0, SPHERE_RADIUS, 0.0)

func _ready() -> void:
	_apply_harness_overrides()
	_build()

func _apply_harness_overrides() -> void:
	var harness := get_node_or_null("/root/BenchHarness")
	if harness == null:
		return
	var count: Variant = harness.call("param", "count", instance_count)
	if int(count) > 0:
		instance_count = int(count)

func _build() -> void:
	instance_count = maxi(instance_count, 1)
	_side = int(ceil(sqrt(float(instance_count))))
	_extent = _side * SPACING * 0.5
	_radius = 2.2 * _extent
	_height = 0.9 * _extent

	var sphere := SphereMesh.new()
	sphere.radius = SPHERE_RADIUS
	sphere.height = SPHERE_HEIGHT
	sphere.radial_segments = SPHERE_SEGMENTS
	sphere.rings = SPHERE_RINGS
	sphere.material = Common.make_checker_material()

	var multimesh := MultiMesh.new()
	multimesh.transform_format = MultiMesh.TRANSFORM_3D
	multimesh.mesh = sphere
	multimesh.instance_count = instance_count
	multimesh.visible_instance_count = -1

	var offset := (float(_side) - 1.0) * 0.5
	for index in instance_count:
		var column := index % _side
		var row := int(index / _side)
		var jitter_x := (_hash01(index, 101) * 2.0 - 1.0) * SPACING * JITTER_RATIO
		var jitter_z := (_hash01(index, 211) * 2.0 - 1.0) * SPACING * JITTER_RATIO
		var point := Vector3(
				(float(column) - offset) * SPACING + jitter_x,
				SPHERE_RADIUS,
				(float(row) - offset) * SPACING + jitter_z)
		multimesh.set_instance_transform(index, Transform3D(Basis.IDENTITY, point))

	var node := MultiMeshInstance3D.new()
	node.name = "Instances"
	node.multimesh = multimesh
	add_child(node)

	add_child(Common.make_sun())
	Common.add_world_environment(self)
	_add_camera()

## Deterministic integer hash in [0, 1). Integer-only arithmetic so the jitter
## is identical on every platform, unlike sin/cos-based hashing.
func _hash01(p_index: int, p_salt: int) -> float:
	var hash_value := (p_index * 73856093) ^ (p_salt * 19349663)
	hash_value = (hash_value ^ (hash_value >> 13)) * 1274126177
	return float((hash_value & 0x7FFFFFFF) % 10000) / 10000.0

func _add_camera() -> void:
	_camera = Camera3D.new()
	_camera.name = "BenchCamera"
	_camera.fov = 75.0
	_camera.near = 0.05
	_camera.far = 4.0 * _extent + 200.0
	add_child(_camera)
	_camera.current = true

func apply_frame(p_frame: int) -> void:
	var t := float(p_frame % LOOP_FRAMES) / float(LOOP_FRAMES)
	var angle := TAU * t
	_camera.global_position = _look_target + Vector3(cos(angle) * _radius, _height, sin(angle) * _radius)
	_camera.look_at(_look_target, Vector3.UP)

func scene_parameters() -> Dictionary:
	return {
		"instance_count": instance_count,
		"grid_side": _side,
		"spacing": SPACING,
		"jitter_ratio": JITTER_RATIO,
		"sphere_radius": SPHERE_RADIUS,
		"sphere_height": SPHERE_HEIGHT,
		"sphere_radial_segments": SPHERE_SEGMENTS,
		"sphere_rings": SPHERE_RINGS,
		"camera_radius": _radius,
		"camera_height": _height,
		"camera_fov": 75.0,
		"loop_frames": LOOP_FRAMES,
	}
