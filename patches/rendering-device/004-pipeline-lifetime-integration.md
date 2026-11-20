# Patch 004 — Pipeline Lifetime Integration

# Status: IMPLEMENTED, OPT-IN (2026-11-20)
# Priority: P2.1 (pipeline object lifetime; not an upstream bug fix)
# Target: drivers/vulkan/rendering_device_driver_vulkan.cpp/.h
#
# This is not a fix for inherited behaviour. It is the minimal set of hooks
# needed to defer VkPipeline destruction until every queue that submitted work
# using the object has completed. The Kaizen core lives in
# kaizen/rendering/pipeline_cache/ and is not edited by this patch.
#
# Enabled only with KAIZEN_PIPELINE_LIFETIME=on. When the variable is absent,
# every hook is bypassed and pipeline_free() is the upstream one-line function.
#
# Touch points (function, hook):
#   constructor                      allocate the registry + submission tracker
#   command_queue_create             assign a per-queue completion slot
#   command_queue_execute_and_present
#                                    after a successful submit: record the
#                                    submission serial; a fenced submission
#                                    closes the group and stores its serial
#   fence_wait                       confirm the group serial, feed
#                                    set_completed_serial(), drain destroyables
#   pipeline_free                    retire instead of vkDestroyPipeline
#   destructor                       vkDeviceWaitIdle, confirm all, drain, then
#                                    destroy the VkDevice
#
# Why these points:
#   - pipeline_free is the single destruction funnel for graphics and compute
#     pipeline objects.
#   - The Vulkan driver passes one fence for the last command buffer of a batch;
#     earlier command buffers are chained with semaphores. Recording a group
#     serial per fence models exactly that shape without adding a fence or a
#     timeline semaphore to every submission.
#   - fence_wait is the only place where completion is actually known, and it
#     runs on the render thread.
#   - The destructor is the only point where the device is guaranteed idle
#     before vkDestroyDevice.
#
# Compatibility:
#   - Default is off: destruction timing, ordering and API are unchanged.
#   - When on, only destruction timing changes. RID lifetimes, frame-ring
#     ownership and the pipeline cache blob are untouched.
#   - Raytracing pipelines are not deferred (raytracing_pipeline_free still
#     destroys immediately).
#   - The registry is used as a completion-safe destruction queue, not as a
#     deduplicating object cache: creation is not gated on it.
#
# Verification:
#   - template_release build succeeds.
#   - tests/kaizen/vulkan/run_vulkan_tests.sh: real VkPipeline objects on a
#     graphics and a compute queue, timeline semaphores, Khronos validation
#     layer active; 70 checks, 0 failures, 0 validation errors.
#   - tests/kaizen/run_tests.sh: 182 tracker checks + 352 pipeline checks.
#   - In-engine functional run with KAIZEN_PIPELINE_LIFETIME=on under the
#     validation layer: see docs/architecture/pipeline-lifetime.md.
#   - No frame-time measurement of the enabled path is claimed.
#
# See docs/architecture/pipeline-lifetime.md for the full design and the
# remaining work (creation-side eviction, raytracing pipelines, in-engine
# hit/miss data).
