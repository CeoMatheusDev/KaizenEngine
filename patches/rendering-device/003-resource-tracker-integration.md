# Patch 003 — Resource Tracker Integration

# Status: IMPLEMENTED (2026-11-19)
# Priority: R1.1 (diagnostics, not an upstream bug fix)
# Target: servers/rendering/rendering_device.cpp
#
# This is not a fix for inherited behaviour. It is the minimal set of hooks
# needed to feed Kaizen's own diagnostics from the real RenderingDevice
# lifecycle. Every entry point returns immediately when diagnostics are
# disabled, which is the default.
#
# Touch points (function, hook):
#   storage_buffer_create   KAIZEN_TRACK_GPU_RESOURCE(...)
#   texture_buffer_create   KAIZEN_TRACK_GPU_RESOURCE(...)
#   uniform_buffer_create   KAIZEN_TRACK_GPU_RESOURCE(...)
#   vertex_buffer_create    KAIZEN_TRACK_GPU_RESOURCE(...)
#   index_buffer_create     KAIZEN_TRACK_GPU_RESOURCE(...)
#   texture_create          KAIZEN_TRACK_GPU_RESOURCE(...) with driver allocation size
#   _free_internal          notify_logical_free(rid, frames_drawn + frames.size())
#   _begin_frame            notify_frame_completed(frames_drawn)
#   finalize                notify_device_teardown()
#
# Why these points:
#   - Creation is the only place that knows both the RID and the byte count.
#   - _free_internal is the single funnel for every RID release, so buffers
#     and textures cannot be freed through a path that skips tracking.
#   - The ring frame that owns the deferred destruction is processed in
#     _begin_frame, which is the correct moment to confirm a release.
#   - finalize() runs after the GPU is idle and is the only place where
#     forcing the remaining releases is safe.
#
# Compatibility:
#   - Default mode is Disabled: one predictable branch per hook, no atomics.
#   - No behaviour, ordering, memory ownership or API changes.
#   - Shared textures and slices are not registered (they would double count
#     the owner's memory).
#
# Verification:
#   - template_release build succeeds.
#   - S1 (400 boxes, Forward+, 1920x1080): 112 allocations, 112 releases,
#     0 live, peak 85.54 MiB; FPS 380.30 disabled vs 378.79 basic (noise).
#   - churn_project: 600 textures created/drawn/freed over 300 frames;
#     604 releases confirmed at runtime, 81 at teardown, 0 leaks.
#
# See docs/architecture/resource-diagnostics.md for the full design.
