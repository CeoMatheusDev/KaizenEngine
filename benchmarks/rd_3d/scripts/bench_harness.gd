extends Node
## Benchmark harness for Kaizen rendering scenes.
##
## This autoload implements the measurement contract described in
## docs/architecture/rendering-benchmark-scenes.md. It drives the scene camera by
## frame index, records one row per rendered frame, and writes a single JSON
## report at the end. It creates no resources or nodes during sampling.
##
## It refuses to run in editor/tools builds because those are not representative
## (the editor forces rendering onto the main thread). Pass --bench-allow-editor
## only to validate scene content during development; the report is then marked
## invalid.
##
## CLI (after the engine's `--` separator):
##   --bench-scene <S1-S4|S6>    Scene to load and report label (default S1)
##   --bench-count <n>           Forwarded to the scene as its content count
##   --bench-variant <a|b|...>   Forwarded to the scene as its variant label
##   --bench-spots <n>           S4 spot-light count
##   --bench-omnis <n>           S4 omni-light count
##   --bench-preset <Q0|Q1>      Label written to the report (default Q0)
##   --bench-config <label>      Configuration label written to the report (D1, M1, ...)
##   --bench-frames <n>          Sampled frames, multiple of 300 (default 3600)
##   --bench-warmup <n>          Warm-up frames, multiple of 300 (default 900)
##   --bench-out <abs path>      Report path (default user://bench/<scene>_<preset>.json)
##   --bench-no-hash             Skip the frame-hash capture
##   --bench-screenshot <path>   Save the frame-299 image as PNG
##   --bench-reference <png>     Diff the captured frame against this image
##   --bench-min-diff <f>        Mark invalid when the mean diff is below f (off by default)
##   --bench-allow-editor        Allow a tools build (report marked invalid)

const LOOP_FRAMES := 300
const DEFAULT_SAMPLE_FRAMES := 3600
const DEFAULT_WARMUP_FRAMES := 900
const HASH_FRAME := 299
const STABLE_WINDOW := 120
const FIRST_WINDOW := 120

## Project settings copied into the report and into the content hash. These are
## the settings that change frame content or cost most directly.
const PINNED_SETTINGS := [
	"rendering/renderer/rendering_method",
	"rendering/rendering_device/driver",
	"display/window/size/viewport_width",
	"display/window/size/viewport_height",
	"display/window/vsync/vsync_mode",
	"rendering/anti_aliasing/quality/msaa_3d",
	"rendering/anti_aliasing/quality/msaa_2d",
	"rendering/scaling_3d/mode",
	"rendering/scaling_3d/scale",
	"rendering/textures/default_filters/anisotropic_filtering_level",
	"rendering/lights_and_shadows/directional_shadow/size",
	"rendering/lights_and_shadows/directional_shadow/soft_shadow_filter_quality",
	"rendering/driver/depth_prepass/enable",
	"rendering/environment/ssao/quality",
	"rendering/environment/ssil/quality",
]

## Scene registry: the project is shared by all 3D scenes, and --bench-scene
## selects which one is loaded. Adding a scene means adding its entry here.
const SCENES := {
	"S1": "res://scenes/s1_static.tscn",
	"S2": "res://scenes/s2_instancing.tscn",
	"S3": "res://scenes/s3_materials.tscn",
	"S4": "res://scenes/s4_lighting.tscn",
	"S6": "res://scenes/s6_post.tscn",
}

var config := {
	"scene": "S1",
	"count": -1,
	"variant": "",
	"spots": -1,
	"omnis": -1,
	"preset": "Q0",
	"config": "",
	"frames": DEFAULT_SAMPLE_FRAMES,
	"warmup": DEFAULT_WARMUP_FRAMES,
	"out": "",
	"screenshot": "",
	"reference": "",
	"min_diff": -1.0,
	"capture_hash": true,
	"allow_editor": false,
}

var _running := false
var _scene: Node = null
var _viewport_rid: RID

var _frames := DEFAULT_SAMPLE_FRAMES
var _sample_start := DEFAULT_WARMUP_FRAMES
var _hash_frame := HASH_FRAME

var _total := 0
var _sampled := 0
var _last_us := 0
var _frame_us := PackedInt64Array()
var _process_ms := PackedFloat64Array()
var _physics_ms := PackedFloat64Array()
var _draw_calls := PackedInt64Array()
var _objects := PackedInt64Array()
var _primitives := PackedInt64Array()
var _texture_mem := PackedInt64Array()
var _buffer_mem := PackedInt64Array()
var _video_mem := PackedInt64Array()
var _pso_delta := PackedInt64Array()
var _viewport_cpu_ms := PackedFloat64Array()
var _viewport_gpu_ms := PackedFloat64Array()

var _pso_prev := 0
var _pso_at_start := 0
var _pso_in_sample := 0
var _pso_before_scene := {}
var _pso_sources_start := {}
var _pso_first_window := {}
var _first_window_taken := false
var _last_compile_frame := -1
var _frame_hash := ""
var _screenshot_path := ""
var _image_diff := {}
var _hash_started := false
var _hash_pause := false
var _invalid_reason := ""

func _ready() -> void:
	_parse_args()
	_start.call_deferred()

## Used by the scene to read CLI overrides before it generates content.
func param(p_key: String, p_default: Variant = null) -> Variant:
	return config.get(p_key, p_default)

func _parse_args() -> void:
	var args := OS.get_cmdline_user_args()
	var i := 0
	while i < args.size():
		var arg := args[i]
		match arg:
			"--bench-scene":
				if i + 1 < args.size():
					i += 1
					config.scene = args[i]
			"--bench-count":
				if i + 1 < args.size():
					i += 1
					config.count = int(args[i])
			"--bench-variant":
				if i + 1 < args.size():
					i += 1
					config.variant = args[i]
			"--bench-spots":
				if i + 1 < args.size():
					i += 1
					config.spots = int(args[i])
			"--bench-omnis":
				if i + 1 < args.size():
					i += 1
					config.omnis = int(args[i])
			"--bench-preset":
				if i + 1 < args.size():
					i += 1
					config.preset = args[i]
			"--bench-config":
				if i + 1 < args.size():
					i += 1
					config.config = args[i]
			"--bench-frames":
				if i + 1 < args.size():
					i += 1
					config.frames = int(args[i])
			"--bench-warmup":
				if i + 1 < args.size():
					i += 1
					config.warmup = int(args[i])
			"--bench-out":
				if i + 1 < args.size():
					i += 1
					config.out = args[i]
			"--bench-screenshot":
				if i + 1 < args.size():
					i += 1
					config.screenshot = args[i]
			"--bench-reference":
				if i + 1 < args.size():
					i += 1
					config.reference = args[i]
			"--bench-min-diff":
				if i + 1 < args.size():
					i += 1
					config.min_diff = float(args[i])
			"--bench-no-hash":
				config.capture_hash = false
			"--bench-allow-editor":
				config.allow_editor = true
		i += 1

func _start() -> void:
	if not OS.has_feature("template"):
		if not bool(config.allow_editor):
			_fail("benchmarks must run in an export template; pass --bench-allow-editor to run a tools build for scene development")
			return
		_add_invalid("tools build (development override)")

	if DisplayServer.get_name() == "headless":
		_fail("benchmarks must run with a rendering surface; --headless performs no GPU work")
		return

	await get_tree().process_frame

	# The project starts on an empty bootstrap scene; load the requested scene.
	var path := String(SCENES.get(String(config.scene), ""))
	if path.is_empty():
		_fail("unknown scene '%s' (expected one of: %s)" % [String(config.scene), ", ".join(PackedStringArray(SCENES.keys()))])
		return

	# Baseline for the first-window pipeline count: everything the scene causes,
	# including compiles during its first rendered frames, must be counted.
	_pso_before_scene = _pso_sources()

	var current := get_tree().current_scene
	if current == null or current.scene_file_path != path:
		var change_error := get_tree().change_scene_to_file(path)
		if change_error != OK:
			_fail("cannot load %s (error %d)" % [path, change_error])
			return

	# change_scene_to_file is deferred; wait until the requested scene is current.
	for _attempt in 16:
		_scene = get_tree().current_scene
		if _scene != null and _scene.scene_file_path == path:
			break
		await get_tree().process_frame

	if _scene == null or _scene.scene_file_path != path:
		_fail("scene %s did not become current" % path)
		return
	if not _scene.has_method("apply_frame"):
		_fail("scene %s must expose apply_frame(frame_index)" % path)
		return

	# A scene may declare that the active renderer cannot provide what it was
	# asked for (S6 presets on Mobile, for example). That is not a measurement.
	if _scene.has_method("unmet_requirements"):
		for reason in Array(_scene.call("unmet_requirements")):
			_add_invalid(String(reason))

	_frames = maxi(int(config.frames), LOOP_FRAMES)
	_frames = int(ceil(float(_frames) / LOOP_FRAMES)) * LOOP_FRAMES
	_sample_start = maxi(int(config.warmup), LOOP_FRAMES)
	_sample_start = int(ceil(float(_sample_start) / LOOP_FRAMES)) * LOOP_FRAMES
	_hash_frame = mini(HASH_FRAME, _sample_start - 1)

	_frame_us.resize(_frames)
	_draw_calls.resize(_frames)
	_objects.resize(_frames)
	_primitives.resize(_frames)
	_texture_mem.resize(_frames)
	_buffer_mem.resize(_frames)
	_video_mem.resize(_frames)
	_pso_delta.resize(_frames)
	_process_ms.resize(_frames)
	_physics_ms.resize(_frames)
	_viewport_cpu_ms.resize(_frames)
	_viewport_gpu_ms.resize(_frames)

	_viewport_rid = get_viewport().get_viewport_rid()
	RenderingServer.viewport_set_measure_render_time(_viewport_rid, true)

	_pso_sources_start = _pso_sources()
	_pso_prev = _sum_sources(_pso_sources_start)
	_pso_at_start = _pso_prev
	_last_us = Time.get_ticks_usec()
	_total = 0
	_sampled = 0
	_running = true

func _process(_delta: float) -> void:
	if not _running:
		return

	# The frame-hash capture holds the camera for one extra (uncounted) render so
	# that the image belongs to the requested frame index. Resume the sequence on
	# the next callback without shifting any later frame.
	if _hash_pause:
		_hash_pause = false
		_last_us = Time.get_ticks_usec()
		_scene.call("apply_frame", _total)
		_total += 1
		return

	var now := Time.get_ticks_usec()
	var frame_done := _total - 1

	if _total > 0:
		var us := now - _last_us
		var sources := _pso_sources()
		var pso_total := _sum_sources(sources)
		var delta := pso_total - _pso_prev
		_pso_prev = pso_total

		if frame_done == FIRST_WINDOW - 1 and not _first_window_taken:
			_pso_first_window = _delta_sources(_pso_before_scene, sources)
			_first_window_taken = true

		if frame_done < _sample_start:
			if delta > 0:
				_last_compile_frame = frame_done
		elif _sampled < _frames:
			_store(_sampled, us, delta)
			_sampled += 1
			if delta > 0:
				_pso_in_sample += delta
			if _sampled >= _frames:
				_finish()
				return

	var wants_capture := bool(config.capture_hash) or not String(config.screenshot).is_empty() or not String(config.reference).is_empty()
	if wants_capture and not _hash_started and frame_done == _hash_frame:
		_hash_started = true
		_hash_pause = true
		_capture_frame()
		return

	_last_us = now
	_scene.call("apply_frame", _total)
	_total += 1

func _store(p_index: int, p_us: int, p_pso_delta: int) -> void:
	_frame_us[p_index] = p_us
	_process_ms[p_index] = float(Performance.get_monitor(Performance.TIME_PROCESS)) * 1000.0
	_physics_ms[p_index] = float(Performance.get_monitor(Performance.TIME_PHYSICS_PROCESS)) * 1000.0
	_draw_calls[p_index] = int(Performance.get_monitor(Performance.RENDER_TOTAL_DRAW_CALLS_IN_FRAME))
	_objects[p_index] = int(Performance.get_monitor(Performance.RENDER_TOTAL_OBJECTS_IN_FRAME))
	_primitives[p_index] = int(Performance.get_monitor(Performance.RENDER_TOTAL_PRIMITIVES_IN_FRAME))
	_texture_mem[p_index] = int(Performance.get_monitor(Performance.RENDER_TEXTURE_MEM_USED))
	_buffer_mem[p_index] = int(Performance.get_monitor(Performance.RENDER_BUFFER_MEM_USED))
	_video_mem[p_index] = int(Performance.get_monitor(Performance.RENDER_VIDEO_MEM_USED))
	_pso_delta[p_index] = p_pso_delta
	_viewport_cpu_ms[p_index] = RenderingServer.viewport_get_measured_render_time_cpu(_viewport_rid)
	_viewport_gpu_ms[p_index] = RenderingServer.viewport_get_measured_render_time_gpu(_viewport_rid)

func _pso_sources() -> Dictionary:
	return {
		"canvas": int(Performance.get_monitor(Performance.PIPELINE_COMPILATIONS_CANVAS)),
		"mesh": int(Performance.get_monitor(Performance.PIPELINE_COMPILATIONS_MESH)),
		"surface": int(Performance.get_monitor(Performance.PIPELINE_COMPILATIONS_SURFACE)),
		"draw": int(Performance.get_monitor(Performance.PIPELINE_COMPILATIONS_DRAW)),
		"specialization": int(Performance.get_monitor(Performance.PIPELINE_COMPILATIONS_SPECIALIZATION)),
	}

func _sum_sources(p_sources: Dictionary) -> int:
	var total := 0
	for value in p_sources.values():
		total += int(value)
	return total

func _delta_sources(p_before: Dictionary, p_after: Dictionary) -> Dictionary:
	var delta := {}
	for key in p_after.keys():
		delta[key] = int(p_after[key]) - int(p_before.get(key, 0))
	return delta

func _capture_frame() -> void:
	await RenderingServer.frame_post_draw
	var image := get_viewport().get_texture().get_image()
	if image == null:
		_add_invalid("frame capture returned no image")
		return

	if bool(config.capture_hash):
		_frame_hash = _hash_image(image)
	if not String(config.screenshot).is_empty():
		_save_screenshot(image)
	if not String(config.reference).is_empty():
		_compare_reference(image)

func _hash_image(p_image: Image) -> String:
	var ctx := HashingContext.new()
	ctx.start(HashingContext.HASH_SHA256)
	ctx.update(("%dx%d:%d:" % [p_image.get_width(), p_image.get_height(), p_image.get_format()]).to_utf8_buffer())
	ctx.update(p_image.get_data())
	return ctx.finish().hex_encode()

func _save_screenshot(p_image: Image) -> void:
	var path := _absolute_path(String(config.screenshot))
	DirAccess.make_dir_recursive_absolute(path.get_base_dir())
	var error := p_image.save_png(path)
	if error != OK:
		_add_invalid("screenshot could not be written to %s (error %d)" % [path, error])
		return
	_screenshot_path = path

## Mean absolute difference over all RGBA bytes of the captured frame and a
## reference PNG. This runs once, after sampling, so its cost is never measured.
func _compare_reference(p_image: Image) -> void:
	var path := _absolute_path(String(config.reference))
	if not FileAccess.file_exists(path):
		_add_invalid("reference image %s does not exist" % path)
		return
	var reference := Image.load_from_file(path)
	if reference == null:
		_add_invalid("reference image %s could not be loaded" % path)
		return

	p_image.convert(Image.FORMAT_RGBA8)
	reference.convert(Image.FORMAT_RGBA8)
	if p_image.get_width() != reference.get_width() or p_image.get_height() != reference.get_height():
		_add_invalid("reference image is %dx%d but the frame is %dx%d" % [
				reference.get_width(), reference.get_height(), p_image.get_width(), p_image.get_height()])
		return

	var actual_bytes := p_image.get_data()
	var reference_bytes := reference.get_data()
	var total := 0
	var worst := 0
	for index in actual_bytes.size():
		var difference := absi(int(actual_bytes[index]) - int(reference_bytes[index]))
		total += difference
		if difference > worst:
			worst = difference
	var mean := float(total) / float(actual_bytes.size())

	_image_diff = {
		"reference": path,
		"width": p_image.get_width(),
		"height": p_image.get_height(),
		"mean_abs_diff": mean,
		"max_abs_diff": worst,
	}

	if float(config.min_diff) >= 0.0 and mean < float(config.min_diff):
		_add_invalid("image difference %.4f is below the required %.4f; the effect did not run" % [mean, float(config.min_diff)])

func _finish() -> void:
	_running = false
	RenderingServer.viewport_set_measure_render_time(_viewport_rid, false)

	if _pso_in_sample > 0:
		_add_invalid("pipeline compilations during the sampling window (%d)" % _pso_in_sample)
	if _last_compile_frame >= 0 and _sample_start > 0 and _sample_start - 1 - _last_compile_frame < STABLE_WINDOW:
		_add_invalid("pipeline compilations within %d frames of sampling start" % STABLE_WINDOW)
	if int(DisplayServer.window_get_vsync_mode()) != 0:
		_add_invalid("display vsync mode is %d, expected 0 (disabled)" % int(DisplayServer.window_get_vsync_mode()))

	var pso_total_delta := _delta_sources(_pso_sources_start, _pso_sources())
	if not _write_report(pso_total_delta):
		get_tree().quit(4)
		return
	get_tree().quit(0 if _invalid_reason.is_empty() else 3)

func _absolute_path(p_path: String) -> String:
	if p_path.begins_with("user://") or p_path.begins_with("res://"):
		return ProjectSettings.globalize_path(p_path)
	if p_path.is_absolute_path():
		return p_path
	return ProjectSettings.globalize_path("user://%s" % p_path)

func _write_report(p_pso_total_delta: Dictionary) -> bool:
	var report_path := String(config.out)
	if report_path.is_empty():
		report_path = "user://bench/%s_%s.json" % [String(config.scene).validate_filename(), String(config.preset)]
	report_path = _absolute_path(report_path)
	DirAccess.make_dir_recursive_absolute(report_path.get_base_dir())

	var frame_us := Array(_frame_us.slice(0, _sampled))
	var gpu := Array(_viewport_gpu_ms.slice(0, _sampled))
	var gpu_available := false
	for value in gpu:
		if float(value) > 0.0:
			gpu_available = true
			break

	var settings := {}
	for key in PINNED_SETTINGS:
		settings[key] = ProjectSettings.get_setting(key)

	# Effective values, not project defaults: --rendering-method / --rendering-driver
	# override the project at startup and must be captured for the cell to be valid.
	var window_size := DisplayServer.window_get_size()
	var viewport := get_viewport()
	var runtime := {
		"rendering_method": RenderingServer.get_current_rendering_method(),
		"rendering_driver": RenderingServer.get_current_rendering_driver_name(),
		"display_server": DisplayServer.get_name(),
		"vsync_mode": int(DisplayServer.window_get_vsync_mode()),
		"msaa_3d": int(viewport.msaa_3d),
		"use_taa": viewport.use_taa,
		"scaling_3d_mode": int(viewport.scaling_3d_mode),
		"scaling_3d_scale": viewport.scaling_3d_scale,
		"adapter_name": RenderingServer.get_video_adapter_name(),
		"adapter_vendor": RenderingServer.get_video_adapter_vendor(),
		"api_version": RenderingServer.get_video_adapter_api_version(),
		"window_size": [window_size.x, window_size.y],
	}

	var document := {
		"schema": 1,
		"engine": Engine.get_version_info(),
		"build": {
			"type": "template" if OS.has_feature("template") else "tools",
			"flags": OS.get_cmdline_args(),
			"user_args": OS.get_cmdline_user_args(),
		},
		"scene": {
			"id": String(config.scene),
			"parameters": _scene_parameters(),
			"content_hash": _content_hash(settings),
		},
		"preset": String(config.preset),
		"configuration": String(config.config),
		"valid": _invalid_reason.is_empty(),
		"invalid_reason": _invalid_reason,
		"project_settings": settings,
		"runtime": runtime,
		"timing": {
			"first_frame": _sample_start,
			"last_frame": _sample_start + _sampled - 1,
			"warmup_frames": _sample_start,
			"sampled_frames": _sampled,
			"pso_at_start": _pso_at_start,
			"pso_in_sample": _pso_in_sample,
			"last_compile_frame": _last_compile_frame,
		},
		"pipeline_compilations": {
			"first_120": _pso_first_window,
			"total": p_pso_total_delta,
		},
		"frames": {
			"frame_us": frame_us,
			"process_ms": Array(_process_ms.slice(0, _sampled)),
			"physics_ms": Array(_physics_ms.slice(0, _sampled)),
			"draw_calls": Array(_draw_calls.slice(0, _sampled)),
			"objects": Array(_objects.slice(0, _sampled)),
			"primitives": Array(_primitives.slice(0, _sampled)),
			"texture_mem": Array(_texture_mem.slice(0, _sampled)),
			"buffer_mem": Array(_buffer_mem.slice(0, _sampled)),
			"video_mem": Array(_video_mem.slice(0, _sampled)),
			"pso_compilations": Array(_pso_delta.slice(0, _sampled)),
			"viewport_cpu_ms": Array(_viewport_cpu_ms.slice(0, _sampled)),
			"viewport_gpu_ms": gpu if gpu_available else [],
		},
		"summary": _summarize(frame_us, gpu if gpu_available else []),
		"frame_hash": _frame_hash,
		"screenshot": _screenshot_path,
		"image_diff": _image_diff,
		"notes": [
			"frame_us is the wall-clock interval between consecutive process callbacks; it includes presentation blocking.",
			"pso_compilations is a per-frame delta of the cumulative pipeline compilation counters.",
			"viewport_gpu_ms is empty when the backend never reported a GPU timestamp.",
		],
	}

	var file := FileAccess.open(report_path, FileAccess.WRITE)
	if file == null:
		push_error("bench: cannot write report to %s" % report_path)
		return false
	file.store_string(JSON.stringify(document, "  ", true))
	file.close()
	print("bench: report written to %s" % report_path)
	return true

func _summarize(p_frame_us: Array, p_gpu_ms: Array) -> Dictionary:
	if p_frame_us.is_empty():
		return {}

	var sorted_us: Array = p_frame_us.duplicate()
	sorted_us.sort()
	var n := sorted_us.size()
	var median_us := _median(sorted_us)
	var p95_us := _percentile(sorted_us, 0.95)
	var p99_us := _percentile(sorted_us, 0.99)
	var worst_us := int(sorted_us[n - 1])

	var hitches := 0
	for value in p_frame_us:
		if int(value) > median_us * 2:
			hitches += 1

	var cpu_sorted := _array_sorted(Array(_process_ms.slice(0, n)))

	var result := {
		"frames": n,
		"fps_median": 1000000.0 / float(median_us) if median_us > 0 else 0.0,
		"frame_ms_median": float(median_us) / 1000.0,
		"frame_ms_p95": float(p95_us) / 1000.0,
		"frame_ms_p99": float(p99_us) / 1000.0,
		"frame_ms_worst": float(worst_us) / 1000.0,
		"frame_ms_min": float(sorted_us[0]) / 1000.0,
		"hitches": hitches,
		"gpu_ms_median": null,
		"cpu_ms_median": _median(cpu_sorted),
	}

	if not p_gpu_ms.is_empty():
		result.gpu_ms_median = float(_median(_array_sorted(p_gpu_ms)))

	return result

func _array_sorted(p_values: Array) -> Array:
	var copy: Array = p_values.duplicate()
	copy.sort()
	return copy

func _median(p_sorted: Array) -> float:
	var n := p_sorted.size()
	if n == 0:
		return 0.0
	if n % 2 == 1:
		return float(p_sorted[n / 2])
	return (float(p_sorted[n / 2 - 1]) + float(p_sorted[n / 2])) * 0.5

func _percentile(p_sorted: Array, p_ratio: float) -> int:
	var n := p_sorted.size()
	if n == 0:
		return 0
	var index := clampi(int(ceil(p_ratio * n)) - 1, 0, n - 1)
	return int(p_sorted[index])

func _scene_parameters() -> Dictionary:
	if _scene != null and _scene.has_method("scene_parameters"):
		return _scene.call("scene_parameters")
	return {}

func _content_hash(p_settings: Dictionary) -> String:
	var payload := {
		"engine": Engine.get_version_info(),
		"settings": p_settings,
		"parameters": _scene_parameters(),
		"scene": String(config.scene),
		"preset": String(config.preset),
	}
	var ctx := HashingContext.new()
	ctx.start(HashingContext.HASH_SHA256)
	ctx.update(JSON.stringify(payload, "", true).to_utf8_buffer())
	return ctx.finish().hex_encode()

func _fail(p_message: String) -> void:
	push_error("bench: " + p_message)
	get_tree().quit(2)

func _add_invalid(p_reason: String) -> void:
	if _invalid_reason.is_empty():
		_invalid_reason = p_reason
	else:
		_invalid_reason += "; " + p_reason
