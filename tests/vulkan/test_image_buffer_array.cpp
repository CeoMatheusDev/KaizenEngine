// Test: Vulkan Image Buffer Array
//
// Validates that arrays of image buffers work correctly in uniform sets.
//
// Test cases:
// 1. Create uniform set with N image buffers
// 2. Verify descriptor count matches array size
// 3. Access all elements in compute shader
// 4. Test with dynamic array sizes
// 5. Test descriptor pool sizing for arrays
//
// Related: Patch 001 (vulkan/001-image-buffer.patch)