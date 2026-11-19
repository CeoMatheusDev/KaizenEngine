extends Node3D
## S4 "Lighting and shadows": light-count scaling and directional shadow cost.
##
## Ground plus 64 boxes using the S1 material. One directional key light always
## casts shadows; the cell adds --bench-spots spot lights and --bench-omnis omni
## lights on deterministic rings. --bench-variant shadows enables shadows on at
## most four spot lights.
##
## The camera orbits the field on a 300-frame loop so every run renders the same
## sequence.
##
## See docs/architecture/rendering-benchmark-scenes.md, section "S4 - Lighting and shadows".

const Common = preload("res://scripts/bench_scene_common.gd")

const LOOP_FRAMES := 300
const GRID_SIDE := 8
const BOX_COUNT := GRID_SIDE * GRID_SIDE
const SPACING := 6.0
const BOX_HEIGHT := 4.0
const GROUND_SIZE := 400.0
const CAMERA_RADIUS := 60.0
const CAMERA_HEIGHT := 25.0
const CAMERA_LOOK_AT := Vector3(0.0, 2.0, 0.0)
const CAMERA_FOV := 75.0
const SUN_SHADOW_DISTANCE := 200.0

## Spot lights: ring radius 20 m at 12 m height, aimed at the field.
const SPOT_RADIUS := 20.0
const SPOT_HEIGHT := 12.0
const SPOT_RANGE := 30.0
const SPOT_ENERGY := 4.0
const SPOT_ANGLE := 50.0
## Omni lights: ring radius 30 m, alternating 6 m and 9 m heights.
const OMNI_RADIUS := 30.0
const OMNI_RANGE := 20.0
const OMNI_ENERGY := 2.0
## The specification allows shadows on at most four spots.
const MAX_SHADOWED_SPOTS := 4

## Overridden by the harness before this node is ready.
@export var spot_count: int = 8
@export var omni_count: int = 0
@export var spot_shadows: bool = false

var _camera: Camera3D
var _shadowed_spots: int = 0

func _ready() -> void:
	_apply_harness_overrides()
	_build()

func _apply_harness_overrides() -> void:
	var harness := get_node_or_null("/root/BenchHarness")
	if harness == null:
		return
	var spots: Variant = harness.call("param", "spots", spot_count)
	if int(spots) >= 0:
		spot_count = int(spots)
	var omnis: Variant = harness.call("param", "omnis", omni_count)
	if int(omnis) >= 0:
		omni_count = int(omnis)
	var variant := String(harness.call("param", "variant", ""))
	if not variant.is_empty():
		spot_shadows = variant.to_lower() == "shadows"

func _build() -> void:
	spot_count = maxi(spot_count, 0)
	omni_count = maxi(omni_count, 0)

	var material := Common.make_checker_material()
	_add_ground(material)
	_add_boxes(material)
	_add_sun()
	_add_spot_lights()
	_add_omni_lights()
	Common.add_world_environment(self, 0.1)
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
	mesh.size = Vector3(2.0, BOX_HEIGHT, 2.0)
	mesh.material = p_material

	var offset := (float(GRID_SIDE) - 1.0) * 0.5
	for index in BOX_COUNT:
		var column := index % GRID_SIDE
		var row := int(index / GRID_SIDE)
		var node := MeshInstance3D.new()
		node.name = "Box%03d" % index
		node.mesh = mesh
		node.position = Vector3(
				(float(column) - offset) * SPACING,
				BOX_HEIGHT * 0.5,
				(float(row) - offset) * SPACING)
		add_child(node)

func _add_sun() -> void:
	var light := Common.make_sun(true)
	light.directional_shadow_max_distance = SUN_SHADOW_DISTANCE
	add_child(light)

func _add_spot_lights() -> void:
	for index in spot_count:
		var angle := TAU * float(index) / float(maxi(spot_count, 1))
		var light := SpotLight3D.new()
		light.name = "Spot%03d" % index
		light.light_energy = SPOT_ENERGY
		light.spot_range = SPOT_RANGE
		light.spot_angle = SPOT_ANGLE
		light.position = Vector3(cos(angle) * SPOT_RADIUS, SPOT_HEIGHT, sin(angle) * SPOT_RADIUS)
		light.look_at_from_position(light.position, CAMERA_LOOK_AT, Vector3.UP)
		if spot_shadows and index < MAX_SHADOWED_SPOTS:
			light.shadow_enabled = true
			_shadowed_spots += 1
		add_child(light)

func _add_omni_lights() -> void:
	for index in omni_count:
		var angle := TAU * float(index) / float(maxi(omni_count, 1))
		var height := 6.0 + 3.0 * float(index % 2)
		var light := OmniLight3D.new()
		light.name = "Omni%03d" % index
		light.light_energy = OMNI_ENERGY
		light.omni_range = OMNI_RANGE
		light.position = Vector3(cos(angle) * OMNI_RADIUS, height, sin(angle) * OMNI_RADIUS)
		add_child(light)

func _add_camera() -> void:
	_camera = Camera3D.new()
	_camera.name = "BenchCamera"
	_camera.fov = CAMERA_FOV
	_camera.near = 0.05
	_camera.far = 400.0
	add_child(_camera)
	_camera.current = true

func apply_frame(p_frame: int) -> void:
	var t := float(p_frame % LOOP_FRAMES) / float(LOOP_FRAMES)
	var angle := TAU * t
	_camera.global_position = Vector3(
			cos(angle) * CAMERA_RADIUS,
			CAMERA_HEIGHT,
			sin(angle) * CAMERA_RADIUS)
	_camera.look_at(CAMERA_LOOK_AT, Vector3.UP)

func scene_parameters() -> Dictionary:
	return {
		"spot_count": spot_count,
		"omni_count": omni_count,
		"shadowed_spots": _shadowed_spots,
		"box_count": BOX_COUNT,
		"grid_side": GRID_SIDE,
		"spacing": SPACING,
		"box_height": BOX_HEIGHT,
		"ground_size": GROUND_SIZE,
		"sun_shadow_distance": SUN_SHADOW_DISTANCE,
		"spot_radius": SPOT_RADIUS,
		"spot_height": SPOT_HEIGHT,
		"spot_range": SPOT_RANGE,
		"spot_energy": SPOT_ENERGY,
		"spot_angle": SPOT_ANGLE,
		"omni_radius": OMNI_RADIUS,
		"omni_range": OMNI_RANGE,
		"omni_energy": OMNI_ENERGY,
		"camera_radius": CAMERA_RADIUS,
		"camera_height": CAMERA_HEIGHT,
		"camera_look_at": [CAMERA_LOOK_AT.x, CAMERA_LOOK_AT.y, CAMERA_LOOK_AT.z],
		"camera_fov": CAMERA_FOV,
		"loop_frames": LOOP_FRAMES,
	}
