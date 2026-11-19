extends Node2D
## S5 — 2D / Canvas scene.
##
## Content: sprite_count Sprite2D nodes on a grid, all sharing one generated
## 256x256 texture and the default canvas material; label_count Label nodes
## with a fixed ASCII string; layer_count CanvasLayer nodes (first holds the
## sprites, the rest hold a subset of labels).
##
## Animation: the root CanvasLayer (sprite layer) offset pans horizontally
## over a 300-frame loop; sprites are not rotated.

const LOOP_FRAMES := 300

var sprite_count := 2000
var label_count := 200
var layer_count := 1

var _texture: ImageTexture
var _sprite_layer: CanvasLayer
var _label_layers: Array = []
var _pan_amplitude := 200.0

func _ready() -> void:
	_apply_harness_overrides()
	_generate_texture()
	_build_layers()
	_build_sprites()
	_build_labels()

func _apply_harness_overrides() -> void:
	var harness := get_node_or_null("/root/BenchHarness")
	if harness == null:
		return
	var c: Variant = harness.call("param", "count", sprite_count)
	if int(c) > 0:
		sprite_count = int(c)
	var l: Variant = harness.call("param", "labels", label_count)
	if int(l) >= 0:
		label_count = int(l)
	var ly: Variant = harness.call("param", "layers", layer_count)
	if int(ly) > 0:
		layer_count = int(ly)

func apply_parameters(params: Dictionary) -> void:
	sprite_count = int(params.get("sprite_count", sprite_count))
	label_count = int(params.get("label_count", label_count))
	layer_count = int(params.get("layer_count", layer_count))

func scene_parameters() -> Dictionary:
	return {
		"sprite_count": sprite_count,
		"label_count": label_count,
		"layer_count": layer_count,
		"texture_size": 256,
		"loop_frames": LOOP_FRAMES,
	}

func unmet_requirements() -> Array:
	return []

func apply_frame(frame: int) -> void:
	var t := float(frame % LOOP_FRAMES) / float(LOOP_FRAMES)
	var offset_x := sin(t * TAU) * _pan_amplitude
	if _sprite_layer != null:
		_sprite_layer.offset = Vector2(offset_x, 0.0)
	for layer in _label_layers:
		if is_instance_valid(layer):
			layer.offset = Vector2(offset_x * 0.5, 0.0)

func _generate_texture() -> void:
	var img := Image.create(256, 256, false, Image.FORMAT_RGBA8)
	for y in 256:
		for x in 256:
			var r := float(x) / 255.0
			var g := float(y) / 255.0
			img.set_pixel(x, y, Color(r, g, 0.5, 1.0))
	_texture = ImageTexture.create_from_image(img)

func _build_layers() -> void:
	_sprite_layer = CanvasLayer.new()
	_sprite_layer.name = "SpriteLayer"
	add_child(_sprite_layer)

	_label_layers.clear()
	for i in layer_count:
		var layer := CanvasLayer.new()
		layer.name = "LabelLayer%d" % i
		add_child(layer)
		_label_layers.append(layer)

func _build_sprites() -> void:
	var grid_side := int(ceil(sqrt(float(sprite_count))))
	var spacing := 40.0
	var origin := Vector2(-grid_side * spacing / 2.0, -grid_side * spacing / 2.0)
	var placed := 0
	for gy in grid_side:
		for gx in grid_side:
			if placed >= sprite_count:
				break
			var sprite := Sprite2D.new()
			sprite.texture = _texture
			sprite.position = origin + Vector2(gx * spacing, gy * spacing)
			sprite.scale = Vector2(0.1, 0.1)
			_sprite_layer.add_child(sprite)
			placed += 1

func _build_labels() -> void:
	if label_count <= 0:
		return
	var per_layer := int(ceil(float(label_count) / float(layer_count)))
	var placed := 0
	for li in layer_count:
		var layer: CanvasLayer = _label_layers[li]
		for j in per_layer:
			if placed >= label_count:
				break
			var label := Label.new()
			label.text = "Kaizen benchmark 0123456789"
			label.position = Vector2(100 + (placed % 10) * 180, 100 + (placed / 10) * 24)
			label.add_theme_font_size_override("font_size", 16)
			layer.add_child(label)
			placed += 1