extends RefCounted
## Shared deterministic content constructors for the 3D benchmark scenes.
##
## Nothing here may use randomness or depend on frame time: the same call
## sequence must produce the same material and environment on every backend.
## Scene-specific geometry, camera and parameters stay in each scene script.
##
## See docs/architecture/rendering-benchmark-scenes.md.

## Sphere used by S2 and S3, so both scenes measure the same geometry.
const SPHERE_RADIUS := 0.5
const SPHERE_HEIGHT := 1.0
const SPHERE_RADIAL_SEGMENTS := 32
const SPHERE_RINGS := 16

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

static func make_bench_sphere(p_material: Material) -> SphereMesh:
	var sphere := SphereMesh.new()
	sphere.radius = SPHERE_RADIUS
	sphere.height = SPHERE_HEIGHT
	sphere.radial_segments = SPHERE_RADIAL_SEGMENTS
	sphere.rings = SPHERE_RINGS
	sphere.material = p_material
	return sphere

## Deterministic integer hash in [0, 1). Integer-only arithmetic so the result
## is identical on every platform, unlike sin/cos-based hashing.
static func hash01(p_value: int, p_salt: int) -> float:
	var hash_value := (p_value * 73856093) ^ (p_salt * 19349663)
	hash_value = (hash_value ^ (hash_value >> 13)) * 1274126177
	return float((hash_value & 0x7FFFFFFF) % 10000) / 10000.0

## S3a material: same feature set for every instance, only albedo colour,
## roughness and metallic differ. Values are derived from the index, so the
## Nth material is the same on every run and platform.
static func make_variation_material(p_index: int) -> StandardMaterial3D:
	var material := StandardMaterial3D.new()
	var hue := fmod(float(p_index) * 0.61803398875, 1.0)
	var saturation := 0.45 + 0.35 * hash01(p_index, 31)
	material.albedo_color = Color.from_hsv(hue, saturation, 0.85)
	material.roughness = 0.2 + 0.6 * hash01(p_index, 47)
	material.metallic = 0.8 * hash01(p_index, 59)
	return material

## 512x512 tangent-space normal map with a smooth, deterministic ripple.
static func make_normal_texture() -> ImageTexture:
	var size := 512
	var image := Image.create_empty(size, size, false, Image.FORMAT_RGBA8)
	for y in size:
		for x in size:
			var nx := 0.35 * sin(TAU * float(x) / 32.0)
			var ny := 0.35 * cos(TAU * float(y) / 32.0)
			var nz := sqrt(maxf(0.0, 1.0 - nx * nx - ny * ny))
			image.set_pixel(x, y, Color(nx * 0.5 + 0.5, ny * 0.5 + 0.5, nz * 0.5 + 0.5))
	image.generate_mipmaps()
	return ImageTexture.create_from_image(image)

## 512x512 packed occlusion/roughness/metallic texture. Red is full occlusion,
## green varies roughness, blue is zero metallic. Red is only read when the
## material enables ambient occlusion; the test keeps it off.
static func make_orm_texture() -> ImageTexture:
	var size := 512
	var image := Image.create_empty(size, size, false, Image.FORMAT_RGBA8)
	for y in size:
		for x in size:
			var roughness := 0.25 + 0.5 * (0.5 + 0.5 * sin(TAU * float(x) / 64.0))
			image.set_pixel(x, y, Color(1.0, roughness, 0.0))
	image.generate_mipmaps()
	return ImageTexture.create_from_image(image)

## S3b: one feature per material, cycled across the scene. Index order follows
## the specification: plain, normal, ORM, emission, rim, clearcoat, anisotropy,
## alpha. These are intentionally different shader variants.
static func make_feature_material(p_feature: int, p_normal: Texture2D, p_orm: Texture2D) -> BaseMaterial3D:
	var color := Color.from_hsv(fmod(float(p_feature) * 0.125, 1.0), 0.5, 0.8)
	var material: BaseMaterial3D

	if p_feature == 2:
		# ORM needs ORMMaterial3D: StandardMaterial3D reads separate
		# roughness/metallic textures instead of the packed ORM texture.
		var orm := ORMMaterial3D.new()
		orm.orm_texture = p_orm
		material = orm
	else:
		material = StandardMaterial3D.new()

	material.albedo_color = color
	material.roughness = 0.5
	material.metallic = 0.0

	match p_feature:
		1:
			material.normal_enabled = true
			material.normal_texture = p_normal
			material.normal_scale = 1.0
		3:
			material.emission_enabled = true
			material.emission = Color(1.0, 0.45, 0.15)
			material.emission_energy_multiplier = 1.0
		4:
			material.rim_enabled = true
			material.rim = 0.7
			material.rim_tint = 0.5
		5:
			material.clearcoat_enabled = true
			material.clearcoat = 0.6
			material.clearcoat_roughness = 0.1
		6:
			material.anisotropy_enabled = true
			material.anisotropy = 0.5
		7:
			material.transparency = BaseMaterial3D.TRANSPARENCY_ALPHA
			material.albedo_color = Color(color.r, color.g, color.b, 0.55)

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
