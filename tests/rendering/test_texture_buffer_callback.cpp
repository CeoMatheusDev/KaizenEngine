// Test: Texture Buffer in Callbacks
//
// Validates that texture buffers can be used in resource callbacks
// (compute dispatch, etc.) without crashing.
//
// Test cases:
// 1. Callback with valid texture buffer RID
// 2. Callback with invalid RID (should return error, not crash)
// 3. Texture buffer freed before callback execution
// 4. Multiple callbacks in same frame
// 5. Concurrent compute + resource release
//
// Related: Patch 002 (rendering-device/002-texture-buffer-callback.patch)