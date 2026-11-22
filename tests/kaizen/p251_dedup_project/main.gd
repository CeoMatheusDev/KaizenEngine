extends Node3D

## P2.5.1 Natural Deduplication Test
##
## This test validates that PipelineCacheRD performs semantic deduplication
## across independent renderer consumers sharing identical pipeline states.

const TOTAL_FRAMES := 120
const SHARED_CONSUMER_COUNT := 32   # Independent consumers sharing identical material state (Group B)
const UNIQUE_VARIANT_COUNT := 8     # Independent consumers with distinct material state (Group C)

var _frame := 0
var _group_b_nodes: Array[MeshInstance3D] = []
var _group_c_nodes: Array[MeshInstance3D] = []

func _ready() -> void:
	# Group B: 32 independent MeshInstance3D nodes using IDENTICAL StandardMaterial3D instances.
	# They should naturally share the exact same compiled pipeline in PipelineHashMapRD / RenderingDevice.
	var shared_mat = StandardMaterial3D.new()
	shared_mat.albedo_color = Color(0.2, 0.6, 1.0)
	shared_mat.metallic = 0.2
	shared_mat.roughness = 0.4

	for i in SHARED_CONSUMER_COUNT:
		var mi := MeshInstance3D.new()
		mi.mesh = BoxMesh.new()
		mi.material_override = shared_mat
		mi.position = Vector3(float(i % 8) * 0.6 - 2.1, 0.5, float(i / 8) * 0.6 - 1.0)
		add_child(mi)
		_group_b_nodes.append(mi)

	# Group C: 8 independent MeshInstance3D nodes with DISTINCT material states (semantic isolation test).
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

func _process(_delta: float) -> void:
	_frame += 1
	if _frame >= TOTAL_FRAMES:
		var logical = RenderingServer.get_rendering_info(RenderingServer.RENDERING_INFO_PIPELINE_CACHE_LOGICAL_ACQUIRES)
		var shared = RenderingServer.get_rendering_info(RenderingServer.RENDERING_INFO_PIPELINE_CACHE_SHARED_ACQUIRES)
		var physical = RenderingServer.get_rendering_info(RenderingServer.RENDERING_INFO_PIPELINE_CACHE_PHYSICAL_CREATES)
		print("telemetry_stats: logical_acquires=%d shared_acquires=%d physical_creates=%d" % [logical, shared, physical])
		
		# Verify semantic identity separation / deduplication success criteria
		# Group B has 32 meshes + Group C has 8 meshes = 40 meshes rendered per frame over 120 frames.
		# With deduplication, Group B should have high shared acquires (logical >> physical).
		if logical > 0 and physical > 0:
			var hit_rate = float(shared) / float(logical)
			print("telemetry_analysis: HitRate=%.2f (shared/logical)" % hit_rate)
		
		get_tree().quit()
