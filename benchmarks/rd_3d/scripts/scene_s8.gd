extends Node3D
## S8 "Culling scale": what the per-instance culler costs, and what occlusion
## culling buys, on a scene big enough for both questions to have an answer.
##
## S2 measures instancing with one MultiMesh, which is one draw call and one
## culler test. That is the right shape for draw-call scaling and the wrong shape
## for culling: a MultiMesh cannot be culled per instance, so no cell in the set
## exercises the per-instance path in RasterizerSceneCull::_scene_cull(), and no
## cell turns on occlusion culling at all. S8 uses one MeshInstance3D per object
## for exactly that reason, and the count sweep is the measurement.
##
## Two variants, same instances, same camera path, same material, same mesh:
##   open   nothing but the field. Almost every instance survives culling, so this
##          is the cost of the culler with no benefit: per-instance frustum test,
##          HZB test, occlusion buffer update.
##   closed the field is inside a sealed box and the camera orbits outside it. A ray
##          from the camera to any instance crosses the box surface first, so a
##          correct occlusion culler drops the whole field. This is the geometry that
##          makes the check in check_report.py meaningful: it is not "the wall covers
##          most of the view", it is "the container is closed and convex".
##
## The A/B that matters is (closed, occlusion off) against (closed, occlusion on).
## (open, occlusion on) against (open, occlusion off) is the same machinery with
## nothing to win, which is the price of leaving it on in a scene that cannot use it.
##
## See docs/architecture/rendering-benchmark-scenes.md, section "S8 - Culling scale".

const Common = preload("res://scripts/bench_scene_common.gd")

const LOOP_FRAMES := 300
const ROOM_HALF := 40.0
const ROOM_FILL := 0.75
const JITTER_RATIO := 0.15
const WALL_THICKNESS := 1.0
const CAMERA_ORBIT_RATIO := 2.2
const CAMERA_HEIGHT_RATIO := 0.3
const CAMERA_FOV := 75.0
const MIN_SPACING := 0.4
const MAX_SPACING := 6.0

## Overridden by --bench-count before this node is ready.
@export var instance_count: int = 2400
## Overridden by --bench-variant (open|closed) and --bench-occlusion (0|1).
@export var variant: String = "open"
@export var occlusion_requested: bool = false
## Overridden by --bench-rays. The engine sizes the HZB from a ray budget, so the
## same scene can be run with a pyramid that costs something measurable. -1 keeps
## the project setting.
@export var rays_per_thread_override: int = -1

var _camera: Camera3D
var _side: int = 1
var _spacing: float = MAX_SPACING
var _occlusion_effective: bool = false

func _ready() -> void:
	_apply_harness_overrides()
	_build()
	_apply_occlusion_setting()

func _apply_harness_overrides() -> void:
	var harness := get_node_or_null("/root/BenchHarness")
	if harness == null:
		return
	var count: Variant = harness.call("param", "count", instance_count)
	if int(count) > 0:
		instance_count = int(count)
	var requested: Variant = harness.call("param", "variant", variant)
	if String(requested) != "":
		variant = String(requested)
	var occlusion: Variant = harness.call("param", "occlusion", -1)
	if int(occlusion) >= 0:
		occlusion_requested = int(occlusion) != 0
	var rays: Variant = harness.call("param", "rays", -1)
	if int(rays) > 0:
		rays_per_thread_override = int(rays)

func _build() -> void:
	instance_count = maxi(instance_count, 1)
	if variant != "open" and variant != "closed":
		push_warning("bench: S8 variant must be \"open\" or \"closed\", got \"%s\"; using open" % variant)
		variant = "open"

	# The grid is packed to fill ROOM_HALF * 2 * ROOM_FILL on a side, so the field
	# keeps a margin inside the container at every count and the container keeps a
	# constant size on screen. Spacing is therefore derived, not a constant.
	_side = int(ceil(sqrt(float(instance_count))))
	_spacing = clampf((ROOM_HALF * 2.0 * ROOM_FILL) / float(maxi(_side, 1)), MIN_SPACING, MAX_SPACING)
	var offset := (float(_side) - 1.0) * 0.5

	var sphere := Common.make_bench_sphere(Common.make_checker_material())
	for index in instance_count:
		var column := index % _side
		var row := int(index / _side)
		var jitter_x := (Common.hash01(index, 101) * 2.0 - 1.0) * _spacing * JITTER_RATIO
		var jitter_z := (Common.hash01(index, 211) * 2.0 - 1.0) * _spacing * JITTER_RATIO
		var node := MeshInstance3D.new()
		node.name = "Instance%d" % index
		node.mesh = sphere
		node.position = Vector3(
				(float(column) - offset) * _spacing + jitter_x,
				Common.SPHERE_RADIUS,
				(float(row) - offset) * _spacing + jitter_z)
		add_child(node)

	if variant == "closed":
		_add_container()

	add_child(Common.make_sun())
	Common.add_world_environment(self)
	_add_camera()

## Six boxes: four walls, a floor and a ceiling, forming a closed shell around the
#  field. The ceiling is not decoration. Without it a camera above the wall line
#  looks straight down into the field and the cell measures nothing while still
#  carrying the occlusion label.
func _add_container() -> void:
	var material := Common.make_checker_material(0.5, 0.45)
	var wall_height := ROOM_HALF * 1.6
	var wall_y0 := -wall_height * 0.5
	var half := ROOM_HALF + WALL_THICKNESS * 0.5

	for side_index in 4:
		var wall := BoxMesh.new()
		var node := MeshInstance3D.new()
		node.name = "Wall%d" % side_index
		node.mesh = wall
		node.material = material
		var along_x := side_index < 2
		wall.size = Vector3(half * 2.0, wall_height, WALL_THICKNESS) if along_x else Vector3(WALL_THICKNESS, wall_height, half * 2.0)
		# `sign` é uma função global do GDScript; sombreá-la aciona o warning
		# SHADOWED_GLOBAL_IDENTIFIER.
		var side_sign := 1.0 if (side_index % 2) == 0 else -1.0
		node.position = Vector3(side_sign * half, 0.0, 0.0) if along_x else Vector3(0.0, 0.0, side_sign * half)
		add_child(node)

	# Floor and ceiling close the shell at the wall band, so the camera at
	# wall_y0 < y < wall_height + wall_y0 cannot see over or under the walls.
	for face_index in 2:
		var slab := BoxMesh.new()
		slab.size = Vector3(half * 2.0, WALL_THICKNESS, half * 2.0)
		var face := MeshInstance3D.new()
		face.name = "Floor" if face_index == 0 else "Ceiling"
		face.mesh = slab
		face.material = material
		var face_y := (wall_y0 - WALL_THICKNESS * 0.5) if face_index == 0 else (wall_y0 + wall_height + WALL_THICKNESS * 0.5)
		face.position = Vector3(0.0, face_y, 0.0)
		add_child(face)

func _add_camera() -> void:
	_camera = Camera3D.new()
	_camera.name = "BenchCamera"
	_camera.fov = CAMERA_FOV
	_camera.near = 0.05
	# Far enough to see the back of the container in the closed variant and the far
	# corner of the field in the open one, so no cell culls against a near plane by
	# accident.
	_camera.far = ROOM_HALF * 8.0
	add_child(_camera)
	_camera.current = true

## Requested occlusion state is applied to the root viewport, which is the viewport
## the harness measures and hashes, then read back: if the engine refuses or
## normalises the request, the cell has to say so instead of labelling a
## non-occlusion run with the occlusion label.
func _apply_occlusion_setting() -> void:
	var root := get_tree().root
	# The runtime setter is the only way to reach this: the project setting is read
	# once at startup, and RendererViewport::viewport_set_occlusion_rays_per_thread()
	# marks every active viewport's buffer dirty, so the new size applies on the
	# first frame that is sampled. There is no getter, so the request itself is what
	# the report records.
	if rays_per_thread_override > 0:
		RenderingServer.viewport_set_occlusion_rays_per_thread(rays_per_thread_override)
	root.use_occlusion_culling = occlusion_requested
	_occlusion_effective = bool(root.is_using_occlusion_culling())

func apply_frame(p_frame: int) -> void:
	var t := float(p_frame % LOOP_FRAMES) / float(LOOP_FRAMES)
	var angle := TAU * t
	var radius := ROOM_HALF * CAMERA_ORBIT_RATIO
	# Identical path in both variants; in the closed one it stays outside the shell.
	_camera.global_position = Vector3(cos(angle) * radius, ROOM_HALF * CAMERA_HEIGHT_RATIO, sin(angle) * radius)
	_camera.look_at(Vector3.ZERO, Vector3.UP)

## Everything that changes frame content, so the report can prove two cells are
## different scenes and so a reader can tell what the culler was actually given.
func scene_parameters() -> Dictionary:
	return {
		"instance_count": instance_count,
		"variant": variant,
		"grid_side": _side,
		"spacing": _spacing,
		"jitter_ratio": JITTER_RATIO,
		"room_half_extent": ROOM_HALF if variant == "closed" else 0.0,
		"room_height": ROOM_HALF * 1.6 if variant == "closed" else 0.0,
		"sphere_radius": Common.SPHERE_RADIUS,
		"sphere_height": Common.SPHERE_HEIGHT,
		"sphere_radial_segments": Common.SPHERE_RADIAL_SEGMENTS,
		"sphere_rings": Common.SPHERE_RINGS,
		"camera_radius": ROOM_HALF * CAMERA_ORBIT_RATIO,
		"camera_fov": CAMERA_FOV,
		"loop_frames": LOOP_FRAMES,
		"occlusion_requested": occlusion_requested,
		"occlusion_effective": _occlusion_effective,
		"rays_per_thread": _effective_rays_per_thread(),
		"rays_per_thread_project": ProjectSettings.get_setting("rendering/occlusion_culling/occlusion_rays_per_thread"),
		"rays_requested": rays_per_thread_override,
		"bvh_build_quality": ProjectSettings.get_setting("rendering/occlusion_culling/bvh_build_quality"),
		"jitter_projection": ProjectSettings.get_setting("rendering/occlusion_culling/jitter_projection"),
		"threaded_cull_minimum_instances": ProjectSettings.get_setting("rendering/limits/spatial_indexer/threaded_cull_minimum_instances"),
		"processor_count": OS.get_processor_count(),
		# The base level of the HZB, from the same formula RendererViewport::_draw_3d
		# uses. Reported because it is the input to the cost of the occlusion path, and
		# because it is a ray budget rather than the screen: reading it off the report
		# beats assuming the pyramid is screen sized. A small buffer is also why the
		# removal will be partial, not total.
		"occlusion_buffer": _estimated_occlusion_buffer(),
	}

func unmet_requirements() -> PackedStringArray:
	var reasons := PackedStringArray()
	if occlusion_requested and not _occlusion_effective:
		reasons.append("occlusion culling was requested but Viewport.use_occlusion_culling is off, so the cell describes a non-occlusion run")
	# Compatibility does not maintain an HZB for the viewport, so a "culling on"
	# cell there is a culling-off cell with a wrong label. Say so and let the
	# collector record the cell as not applicable.
	#
	# The other way this scene can cull nothing is a build without modules/raycast:
	# the base RendererSceneOcclusionCull::buffer_get_ptr() returns null and the
	# per-instance test is skipped. That is deliberately NOT checked here. The
	# module registers no classes and no singletons - it only constructs the
	# subclass that claims the static singleton - so there is no ClassDB or
	# Engine.has_singleton() query that can answer it, and
	# Viewport.use_occlusion_culling reads back true either way. A probe would be
	# a guess with a confident label; the off/on pair in check_report.py is the
	# evidence instead.
	if occlusion_requested and RenderingServer.get_current_rendering_method() == "gl_compatibility":
		reasons.append("the Compatibility renderer does not provide occlusion culling; the cell is not applicable")
	return reasons

## Mirrors the engine's sizing: rays_per_thread * threads, clamped to one depth
## texel per 32x32 screen region at the low end and per 2x2 region at the high end.
func _effective_rays_per_thread() -> int:
	if rays_per_thread_override > 0:
		return rays_per_thread_override
	return int(ProjectSettings.get_setting("rendering/occlusion_culling/occlusion_rays_per_thread"))

func _estimated_occlusion_buffer() -> Array:
	var width := int(ProjectSettings.get_setting("display/window/size/viewport_width"))
	var height := int(ProjectSettings.get_setting("display/window/size/viewport_height"))
	var rays := _effective_rays_per_thread()
	var threads := OS.get_processor_count()
	var screen_area := maxi(width * height, 1)
	var max_size := clampi(rays * threads, int(screen_area / (32 * 32)), int(screen_area / (2 * 2)))
	var aspect := float(width) / float(maxi(height, 1))
	var buffer_height := int(sqrt(float(max_size) / maxf(aspect, 0.0001)))
	return [int(buffer_height * aspect), buffer_height]
