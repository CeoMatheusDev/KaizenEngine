// Test: Texture Buffer Lifetime
//
// Validates correct lifetime management of texture buffers used in callbacks.
//
// Test cases:
// 1. Buffer created and used in callback within same frame
// 2. Buffer freed after callback completes
// 3. Buffer freed during async compute execution
// 4. Multiple frames in flight with buffer reuse
// 5. Buffer replacement (texture_replace_rid) with callbacks
//
// Related: Patch 002 (rendering-device/002-texture-buffer-callback.patch)