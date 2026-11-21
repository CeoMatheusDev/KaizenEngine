# Patch 004 — Pipeline Lifetime Integration

# Status: IMPLEMENTED, OPT-IN (2026-11-21)
# Priority: P2.1-P2.4 (pipeline object lifetime, command buffer recording tracking, and device-level safety/concurrency validation)
# Target: drivers/vulkan/rendering_device_driver_vulkan.cpp/.h

# This is not a fix for inherited behaviour. It is the complete set of hooks
# needed to manage VkPipeline object lifecycles from creation to physical
# destruction, including recording reference tracking (`begin_use`,
# `end_use_with_serial`, `abort_use`), queue synchronization, and completion
# waiting. The Kaizen core lives in kaizen/rendering/pipeline_cache/.

# Enabled only with KAIZEN_PIPELINE_LIFETIME=on. When the variable is absent,
# every hook is bypassed and pipeline_free() is the upstream one-line function.

# Touch points (function, hook):
#   constructor                      allocate the registry + submission tracker
#   command_queue_create             assign a per-queue completion slot
#   command_queue_execute_and_present
#                                    after a successful submit: record the
#                                    submission serial, close submission group,
#                                    and transition recording references to
#                                    GPU in-flight via `end_use_with_serial()`.
#   command_buffer_execute_secondary propagate pipeline references from secondary
#                                    to primary command buffers (`begin_use`).
#   command_pool_reset / free        abort active recording references (`abort_use`).
#   fence_wait                       confirm the group serial, feed
#                                    set_completed_serial(), drain destroyables
#   pipeline_free                    retire instead of vkDestroyPipeline
#   destructor                       vkDeviceWaitIdle, confirm all, drain, then
#                                    destroy the VkDevice

# Why these points:
#   - pipeline_free is the single destruction funnel for graphics and compute
#     pipeline objects.
#   - Command buffer binding hooks (`command_bind_*_pipeline`) track active
#     recording references, ensuring pipelines cannot be evicted or destroyed
#     while being recorded into a command buffer.
#   - The Vulkan driver passes one fence for the last command buffer of a batch;
#     earlier command buffers are chained with semaphores. Recording a group
#     serial per fence models exactly that shape without adding a fence or a
#     timeline semaphore to every submission.
#   - fence_wait is the only place where completion is actually known, and it
#     runs on the render thread.
#   - The destructor is the only point where the device is guaranteed idle
#     before vkDestroyDevice.

# Compatibility:
#   - Default is off: destruction timing, ordering and API are unchanged.
#   - When on, destruction timing and recording safety are managed by the registry.
#     RID lifetimes, frame-ring ownership and the pipeline cache blob are untouched.
#   - Raytracing pipelines are not deferred (raytracing_pipeline_free still
#     destroys immediately).
#   - The registry is used as a completion-safe destruction and recording queue.

# Verification:
#   - template_release build succeeds.
#   - tests/kaizen/vulkan/run_vulkan_tests.sh: real VkPipeline objects on a
#     graphics and a compute queue, timeline semaphores, Khronos validation
#     layer active; 109 checks, 0 failures, 0 validation errors.
#   - tests/kaizen/run_tests.sh: 182 tracker checks + 398 pipeline checks.
#   - ASan, UBSan, TSan builds: 0 failures / data races.
#   - In-engine functional run with KAIZEN_PIPELINE_LIFETIME=on under the
#     validation layer: see docs/architecture/pipeline-lifetime.md.

# See docs/architecture/pipeline-lifetime.md for the full design and the
# remaining work (natural multi-consumer reuse and clean shader teardown).