extends RefCounted
## Shared deterministic content constructors for the 3D benchmark scenes.
##
## Nothing here may use randomness or depend on frame time: the same call
## sequence must produce the same material and environment on every backend.
## Scene-specific geometry, camera and parameters stay in each scene script.
##
## See docs/architecture/rendering-benchmark-scenes.md.

## Checker albedo generated at 32x32 and upscaled to 1024x1024 with nearest
## filtering, then mipmapped. No imported assets, no external state.
static func make_checker_material(p_luminance_a: float = 0.62, p_luminance_b: float = 0.34) -> StandardMaterial3D:
	var image := Image.create_empty(32, 32, false, Image.FORMAT_RGBA8)
	for y in 32:
		for x in 32:
			var on_square := (int(x / 4) + int(y / 4)) % 2 == 0
			var luminance := p_luminance_a if on_square else p_luminance_b
			image.set_pixel(x, y, Color(luminance, luminance, luminance))
	image.resize(1024, 1024, Image.INTERPOLATE_NEAREST)
	image.generate_mipmaps()

	var material := StandardMaterial3D.new()
	material.albedo_texture = ImageTexture.create_from_image(image)
	material.roughness = 0.6
	material.metallic = 0.0
	return material

## Solid background plus flat ambient, so the sky pass cannot dominate a cell.
static func make_environment() -> Environment:
	var environment := Environment.new()
	environment.background_mode = Environment.BG_COLOR
	environment.background_color = Color(0.05, 0.05, 0.07)
	environment.ambient_light_source = Environment.AMBIENT_SOURCE_COLOR
	environment.ambient_light_color = Color(1.0, 1.0, 1.0)
	environment.ambient_light_energy = 0.3
	return environment

## One shadowless directional key light, as required by S1-S3.
static func make_sun() -> DirectionalLight3D:
	var light := DirectionalLight3D.new()
	light.name = "KeyLight"
	light.light_energy = 1.0
	light.shadow_enabled = false
	light.rotation_degrees = Vector3(-50.0, -30.0, 0.0)
	return light

static func add_world_environment(p_root: Node3D) -> void:
	var node := WorldEnvironment.new()
	node.name = "Environment"
	node.environment = make_environment()
	p_root.add_child(node)
