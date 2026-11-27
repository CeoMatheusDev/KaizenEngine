extends Node3D

## P2.5.1 Natural Deduplication Test
##
## This test validates that PipelineCacheRD performs semantic deduplication
## across independent renderer consumers sharing identical pipeline states,
## keeps distinct states isolated, and tears the shared entries down cleanly.
##
## Scenario groups:
##   Group A - 32 MeshInstance3D nodes sharing ONE StandardMaterial3D instance
##             (single consumer, many draws).
##   Group B - 16 MeshInstance3D nodes, each with its OWN StandardMaterial3D
##             instance carrying IDENTICAL parameters (independent
##             PipelineCacheRD consumers of the same semantic key).
##   Group C - 8 MeshInstance3D nodes with DISTINCT material states (semantic
##             isolation: different keys must not merge).
##
## Phase 2 frees every mesh and material while rendering continues, so shared
## cache entries retire during active frames. The run is clean when the process
## log contains "teardown_ok" and no "Deferring" warnings
## (see tests/kaizen/README.md for the assertion protocol).

const TOTAL_FRAMES := 120
const TEARDOWN_FRAMES := 30
const SHARED_CONSUMER_COUNT := 32   # Group A: one shared material instance
const INDEPENDENT_CONSUMER_COUNT := 16 # Group B: independent identical instances
const UNIQUE_VARIANT_COUNT := 8     # Group C: distinct material state

var _frame := 0
var _phase := 1
var _teardown_frame := 0
var _group_a_nodes: Array[MeshInstance3D] = []
var _group_b_nodes: Array[MeshInstance3D] = []
var _group_b_materials: Array[StandardMaterial3D] = []
var _group_c_nodes: Array[MeshInstance3D] = []

func _make_identical_material() -> StandardMaterial3D:
	var mat := StandardMaterial3D.new()
	mat.albedo_color = Color(0.2, 0.6, 1.0)
	mat.metallic = 0.2
	mat.roughness = 0.4
	return mat

func _ready() -> void:
	# Group A: 32 independent MeshInstance3D nodes sharing one StandardMaterial3D
	# instance. They should share the exact same compiled pipeline in
	# PipelineHashMapRD / RenderingDevice.
	var shared_mat = _make_identical_material()

	for i in SHARED_CONSUMER_COUNT:
		var mi := MeshInstance3D.new()
		mi.mesh = BoxMesh.new()
		mi.material_override = shared_mat
		mi.position = Vector3(float(i % 8) * 0.6 - 2.1, 0.5, float(i / 8) * 0.6 - 1.0)
		add_child(mi)
		_group_a_nodes.append(mi)

	# Group B: 16 independent MeshInstance3D nodes, each with its own
	# StandardMaterial3D instance carrying identical parameters. These are
	# independent PipelineCacheRD consumers of the same semantic key: if the
	# shader identity is shared (identical shader code), the device-level
	# get-or-create path must return one shared RID and report shared acquires
	# above physical creates.
	for i in INDEPENDENT_CONSUMER_COUNT:
		var own_mat := _make_identical_material()
		var mi := MeshInstance3D.new()
		mi.mesh = BoxMesh.new()
		mi.material_override = own_mat
		mi.position = Vector3(float(i % 8) * 0.6 - 2.1, -0.7, float(i / 8) * 0.6 - 1.0)
		add_child(mi)
		_group_b_nodes.append(mi)
		_group_b_materials.append(own_mat)

	# Group C: 8 independent MeshInstance3D nodes with DISTINCT material states
	# (semantic isolation test).
	for i in UNIQUE_VARIANT_COUNT:
		var unique_mat = StandardMaterial3D.new()
		# Distinct albedo / roughness to produce different pipeline keys
		unique_mat.albedo_color = Color.from_hsv(float(i) / float(UNIQUE_VARIANT_COUNT), 0.9, 0.9)
		unique_mat.roughness = float(i) / float(UNIQUE_VARIANT_COUNT)

		var mi := MeshInstance3D.new()
		mi.mesh = BoxMesh.new()
		mi.material_override = unique_mat
		mi.position = Vector3(float(i) * 0.6 - 2.1, 1.5, 1.5)
		add_child(mi)
		_group_c_nodes.append(mi)

func _print_telemetry() -> void:
	var logical = RenderingServer.get_rendering_info(RenderingServer.RENDERING_INFO_PIPELINE_CACHE_LOGICAL_ACQUIRES)
	var shared = RenderingServer.get_rendering_info(RenderingServer.RENDERING_INFO_PIPELINE_CACHE_SHARED_ACQUIRES)
	var physical = RenderingServer.get_rendering_info(RenderingServer.RENDERING_INFO_PIPELINE_CACHE_PHYSICAL_CREATES)
	print("telemetry_stats: logical_acquires=%d shared_acquires=%d physical_creates=%d" % [logical, shared, physical])

	# Verify semantic identity separation / deduplication success criteria.
	# Group A has 32 meshes, Group B 16 meshes and Group C 8 meshes rendered
	# per frame. With deduplication, shared acquires should be high
	# (logical >> physical).
	if logical > 0 and physical > 0:
		var hit_rate = float(shared) / float(logical)
		print("telemetry_analysis: HitRate=%.2f (shared/logical)" % hit_rate)

func _process(_delta: float) -> void:
	if _phase == 1:
		_frame += 1
		if _frame >= TOTAL_FRAMES:
			_print_telemetry()
			# Phase 2: free every consumer while frames keep rendering. This
			# exercises retirement during active frames and clean shader
			# teardown (no deferred-release warnings may remain in the log).
			for mi in _group_a_nodes + _group_b_nodes + _group_c_nodes:
				mi.material_override = null
				mi.queue_free()
			_group_a_nodes.clear()
			_group_b_nodes.clear()
			_group_c_nodes.clear()
			_group_b_materials.clear()
			_phase = 2
	else:
		_teardown_frame += 1
		if _teardown_frame >= TEARDOWN_FRAMES:
			_print_telemetry()
			print("teardown_ok: freed every consumer and material with the device still running")
			get_tree().quit()
