# Patch 002 - Texture buffer in compute callbacks

# Status: IMPLEMENTED (2026-11-19, commit 0b5ec230)
# Priority: P1 (functionality)
# Target: servers/rendering/rendering_device.cpp
# Diff: patches/rendering-device/002-texture-buffer-callback.patch

# Problem:
#   A texture buffer reaching a callback uniform lookup hit
#   DEV_ASSERT(false && "FIXME: Broken.") in _get_buffer_from_owner, i.e. the
#   path was a declared bug rather than an unimplemented feature.

# Change:
#   _get_buffer_from_owner: retrieve the buffer from texture_buffer_owner with a
#   null check instead of asserting. 6 lines, one function, no ordering or
#   ownership change.

# Verification recorded at the time (by the author, on real hardware):
#   - template_release linuxbsd x86_64 build succeeds.
#   - S1 smoke run (400 boxes, Forward+, 1920x1080): 381 FPS, no crash on the
#     normal rendering path. Those are the author's run-log figures; nothing in
#     this repository re-measures them.

# What is NOT verified:
#   No dedicated test exists for a texture buffer inside a compute callback.
#   The stub files that would have been it (tests/rendering/test_texture_buffer_
#   callback.cpp, test_texture_buffer_lifetime.cpp) contained no code and were
#   removed in f4b8a1ca rather than filled in, so "no crash on the normal path"
#   is the whole extent of the evidence: it shows the assert is gone for callers
#   that reach the new branch, not that the branch is exercised. This is tracked
#   under "Pending validation" in docs/architecture/resource-diagnostics.md and
#   must not be reported as tested until a device-level harness exists.

# Rollback: drop patches/rendering-device/002-texture-buffer-callback.patch.
# The diff applies to servers/rendering/rendering_device.cpp at 0b5ec230^; 003
# then applies on top of it. Verified with `patch -p1` plus a byte comparison
# against 0b5ec230 (file) and f4b8a1ca (after 003). `git apply` from a
# subdirectory is not a valid way to run this check: it resolves paths against
# the repository root.
