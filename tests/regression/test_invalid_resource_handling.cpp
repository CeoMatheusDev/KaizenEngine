// Test: Invalid Resource Handling
//
// Validates that the rendering device handles invalid resources gracefully
// (returns errors, not crashes) across all backends.
//
// Test cases:
// 1. Free invalid RID (should return error, not crash)
// 2. Use freed RID in uniform set (should return error)
// 3. Use freed RID in callback (should return error)
// 4. Free null RID (should be no-op)
// 5. Double-free same RID (should return error)
//
// Related: P0/P1 patches