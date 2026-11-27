--@extends Node2D

-- Per-frame GPU texture churn used to validate that Kaizen's resource tracker
-- observes deferred releases correctly. Each texture is created, drawn in the
-- same frame and freed while the GPU may still reference it.

local TOTAL_FRAMES = 300
local TEXTURE_SIZE = 32
local SPRITES_PER_FRAME = 2

local frame = 0
local finished = false

function _process(delta)
	if finished then
		return
	end
	if frame >= TOTAL_FRAMES then
		finished = true
		print(string.format("churn: completed %d frames, %d textures", frame, frame * SPRITES_PER_FRAME))
		self:get_tree():quit()
		return
	end

	for i = 1, SPRITES_PER_FRAME do
		local image = Image.create(TEXTURE_SIZE, TEXTURE_SIZE, false, Image.FORMAT_RGBA8)
		image:fill(Color.from_hsv((frame / TOTAL_FRAMES) % 1.0, 0.7, 0.9))

		local sprite = Sprite2D.new()
		sprite.texture = ImageTexture.create_from_image(image)
		sprite.position = Vector2(randf_range(16.0, 304.0), randf_range(16.0, 224.0))
		self:add_child(sprite)
		sprite:queue_free()
	end

	frame = frame + 1
end
