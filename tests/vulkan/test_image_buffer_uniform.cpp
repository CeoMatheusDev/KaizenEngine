// Test: Vulkan Image Buffer Uniform
//
// Validates that UNIFORM_TYPE_IMAGE_BUFFER creates a valid uniform set
// using VK_DESCRIPTOR_TYPE_STORAGE_TEXEL_BUFFER without crashing.
//
// Test cases:
// 1. Create image buffer with various formats (RGBA8, R32F, etc.)
// 2. Create uniform set with image buffer binding
// 3. Bind and dispatch compute shader using image buffer
// 4. Destroy buffer and uniform set (lifetime validation)
// 5. Test with array of image buffers
// 6. Test alignment and size limits
// 7. Test invalid format rejection
//
// Prerequisites: Vulkan device with storage texel buffer support
// Related: Patch 001 (vulkan/001-image-buffer.patch)