// Test: Resource Release During Compute
//
// Validates that resources can be safely released during compute execution
// without use-after-free or race conditions.
//
// Test cases:
// 1. Release buffer while compute is reading it (should defer or error)
// 2. Release texture while compute is sampling it
// 3. Replace resource during compute execution
// 4. Stress: rapid alloc/free during compute
//
// Related: P0/P1 patches, Resource Lifetime Diagnostics