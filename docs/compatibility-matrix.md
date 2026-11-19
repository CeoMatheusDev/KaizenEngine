# Compatibility Matrix — KaizenEngine Renderer

## Backend support

| Feature | Forward+ (Vulkan) | Mobile (Vulkan) | Compatibility (OpenGL) |
|---|---|---|---|
| SSAO | Yes | Stubbed (empty) | Yes (GLES3 own impl) |
| SSIL | Yes | Stubbed (empty) | No |
| Volumetric fog | Yes | No | No |
| Glow | Yes | Yes | Yes |
| PSO cache persistence | Yes | Yes | N/A (no RD) |
| GPU timestamps | Yes | Yes | Yes (OpenGL timer) |
| UNIFORM_TYPE_IMAGE_BUFFER | No (crash) | No (crash) | N/A |
| Texture buffer callbacks | No (broken) | No (broken) | N/A |

## Known limitations by backend

### Vulkan (Forward+ and Mobile)
- `UNIFORM_TYPE_IMAGE_BUFFER` causes `CRASH_NOW_MSG("Unimplemented!")`
- `texture_buffer` in callbacks hits `DEV_ASSERT(false && "FIXME: Broken.")`
- Pipeline cache grows unbounded (no eviction policy)

### OpenGL (Compatibility)
- No PSO cache layer (does not use RenderingDevice)
- No SSAO quality settings exposed (GLES3 has own SSAO)
- No SSIL or volumetric fog

### Mobile (Vulkan)
- SSAO/SSIL quality setters are empty (stubbed)
- No volumetric fog support
- `is_volumetric_supported()` returns false

## Test coverage

| Test | Vulkan | OpenGL | Status |
|---|---|---|---|
| D1 (Forward+) | ✅ 28 cells | N/A | Published |
| D2 (Mobile) | ✅ 7 valid + 6 N/A | N/A | Published |
| D3 (Compatibility) | N/A | ✅ 9 valid + 3 N/A | Published |
| S5 (2D/Canvas) | ✅ 12 cells | Not measured | Published (D1 only) |