extends Node2D

## Per-frame GPU texture churn used to validate that Kaizen's resource tracker
## observes deferred releases correctly. Each texture is created, drawn in the
## same frame and freed while the GPU may still reference it.

const TOTAL_FRAMES := 300
const TEXTURE_SIZE := 32
const SPRITES_PER_FRAME := 2

var _frame := 0

func _process(_delta: float) -> void:
	if _frame >= TOTAL_FRAMES:
		print("churn: completed %d frames, %d textures" % [_frame, _frame * SPRITES_PER_FRAME])
		get_tree().quit()
		return

	for i in SPRITES_PER_FRAME:
		var image := Image.create(TEXTURE_SIZE, TEXTURE_SIZE, false, Image.FORMAT_RGBA8)
		image.fill(Color.from_hsv(fmod(float(_frame) / TOTAL_FRAMES, 1.0), 0.7, 0.9))

		var sprite := Sprite2D.new()
		sprite.texture = ImageTexture.create_from_image(image)
		sprite.position = Vector2(randf_range(16.0, 304.0), randf_range(16.0, 224.0))
		add_child(sprite)
		sprite.queue_free()

	_frame += 1
