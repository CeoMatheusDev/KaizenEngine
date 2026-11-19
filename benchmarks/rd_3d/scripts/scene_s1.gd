extends Node3D
## S1 "Static 3D": baseline geometry and material throughput.
##
## Content is generated at startup from a deterministic generator; nothing is
## imported and nothing depends on `delta`. The camera is driven by frame index
## through `apply_frame()`, which the benchmark harness calls once per frame.
##
## See docs/architecture/rendering-benchmark-scenes.md, section "S1 - Static 3D".

const Common = preload("res://scripts/bench_scene_common.gd")

const LOOP_FRAMES := 300
const SPACING := 8.0
const BOX_SIZE := 2.0
const GROUND_SIZE := 400.0

## Overridden by --bench-count before this node is ready.
@export var box_count: int = 400

var _camera: Camera3D
var _side: int = 0
var _extent: float = 0.0
var _radius: float = 0.0
var _height: float = 0.0
var _look_target := Vector3(0.0, 1.0, 0.0)

func _ready() -> void:
	_apply_harness_overrides()
	_build()

func _apply_harness_overrides() -> void:
	var harness := get_node_or_null("/root/BenchHarness")
	if harness == null:
		return
	var count: Variant = harness.call("param", "count", box_count)
	if int(count) > 0:
		box_count = int(count)

func _build() -> void:
	box_count = maxi(box_count, 1)
	_side = int(ceil(sqrt(float(box_count))))
	_extent = _side * SPACING * 0.5
	_radius = 2.2 * _extent
	_height = 0.9 * _extent

	var material := Common.make_checker_material()
	_add_ground(material)
	_add_boxes(material)
	add_child(Common.make_sun())
	Common.add_world_environment(self)
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

	var offset := (float(_side) - 1.0) * 0.5
	for index in box_count:
		var column := index % _side
		var row := int(index / _side)
		var node := MeshInstance3D.new()
		node.mesh = mesh
		node.position = Vector3(
				(float(column) - offset) * SPACING,
				BOX_SIZE * 0.5,
				(float(row) - offset) * SPACING)
		add_child(node)

func _add_camera() -> void:
	_camera = Camera3D.new()
	_camera.name = "BenchCamera"
	_camera.fov = 75.0
	_camera.near = 0.05
	_camera.far = 4.0 * _extent + 200.0
	add_child(_camera)
	_camera.current = true

## Called by the harness once per rendered frame. Frame-indexed on purpose:
## the same frame index must produce the same camera pose on any frame rate.
func apply_frame(p_frame: int) -> void:
	var t := float(p_frame % LOOP_FRAMES) / float(LOOP_FRAMES)
	var angle := TAU * t
	_camera.global_position = _look_target + Vector3(cos(angle) * _radius, _height, sin(angle) * _radius)
	_camera.look_at(_look_target, Vector3.UP)

func scene_parameters() -> Dictionary:
	return {
		"box_count": box_count,
		"grid_side": _side,
		"spacing": SPACING,
		"box_size": BOX_SIZE,
		"ground_size": GROUND_SIZE,
		"camera_radius": _radius,
		"camera_height": _height,
		"camera_fov": 75.0,
		"loop_frames": LOOP_FRAMES,
	}
