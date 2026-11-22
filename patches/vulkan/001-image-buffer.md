# Patch 001 - Vulkan image buffer descriptor path

# Status: IMPLEMENTED (2026-11-19, commit 0b5ec230)
# Priority: P0 (functional gap)
# Target: drivers/vulkan/rendering_device_driver_vulkan.cpp
# Diff: patches/vulkan/001-image-buffer.patch

# Changes:
#   1. Layout binding: descriptorCount was left at 0 for image buffers, so the
#      binding never described its own descriptors. Now set from uniform.length.
#   2. Uniform set creation: UNIFORM_TYPE_IMAGE_BUFFER was unhandled. Implemented
#      with VK_DESCRIPTOR_TYPE_STORAGE_TEXEL_BUFFER over a VkBufferView, following
#      the existing UNIFORM_TYPE_TEXTURE_BUFFER case in the same switch.

# Verification recorded at the time (by the author, on real hardware):
#   template_release build succeeds; S1 smoke run at 381 FPS with no crash. The
#   figures are the author's run-log numbers, not reproducible in this sandbox.

# What is NOT verified:
#   No test creates or binds an image buffer uniform, so neither hunk is covered
#   by anything in-repo. The Vulkan-side harness that does exist
#   (tests/kaizen/vulkan/) needs a device and spirv-as, and skips otherwise -
#   see its README section. A machine without a Vulkan loader gets exit 77,
#   which is a skip and not a pass.

# Rollback: drop patches/vulkan/001-image-buffer.patch. The diff applies to
# drivers/vulkan/rendering_device_driver_vulkan.cpp at 0b5ec230^ and was verified
# to reproduce that commit's file byte for byte.
