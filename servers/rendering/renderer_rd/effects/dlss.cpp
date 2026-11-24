//************************************************************************//
//  dlss.cpp                                                              //
//************************************************************************//
//                         This file is part of:                          //
//                            KaizenEngine                                //
//              https://github.com/CeoMatheusDev/Kaizen                   //
//************************************************************************//
// Copyright (c) 2007-2014 Juan Linietsky, Ariel Manzur.                  //
// Copyright (c) 2014-present Godot Engine contributors.                  //
// Copyright (c) 2026 Matheus A. Santos, Hardion Labs.                    //
//                                                                        //
// This file contains code derived from the Godot Engine                  //
// (https://godotengine.org), which is licensed under the MIT License.    //
// The original MIT notice is preserved below.                            //
//                                                                        //
// Modified by Matheus A. Santos, Hardion Labs on 2026-11-23.             //
// Modifications and additional code are licensed under the GNU Affero    //
// General Public License v3.0 (AGPL-3.0).                                //
// See the files LICENSE.txt and LICENSE-ADDITIONAL-TERMS.md for details. //
//************************************************************************//

#include "dlss.h"

#include "core/os/os.h"

#if defined(NGX_DLSS_ENABLED) && defined(VULKAN_ENABLED)
#include <drivers/vulkan/kaizen_vulkan.h>
#endif

using namespace RendererRD;

// NVIDIA DLSS through the NGX SDK.
//
// The SDK library (libnvidia-ngx-dlss.so / nvngx.dll) is loaded at runtime so
// the engine starts fine without it; is_supported() reports whether the whole
// chain (library, NGX init on the active Vulkan device) came up. Callers that
// get false must fall back to FSR2 or bilinear.
//
// Synchronisation: NGX records its dispatches into a command buffer of our
// own, outside the RenderingDevice command stream. The frame is flushed and
// synced first so the inputs are complete on the GPU, and the submit is
// followed by a queue wait so the output is visible to the engine's later
// passes. Conservative, not fast: a mid-frame stall per upscaled frame. The
// fast path (recording into the RD stream) needs an engine seam for borrowing
// the driver's in-flight command buffer.

static DLSSEffect *dlss_singleton = nullptr;

DLSSEffect *DLSSEffect::get_singleton() {
	if (dlss_singleton == nullptr) {
		dlss_singleton = memnew(DLSSEffect);
	}
	return dlss_singleton;
}

#if defined(NGX_DLSS_ENABLED) && defined(VULKAN_ENABLED)

// Signature mirrors of the exported entry points (nvsdk_ngx_vk.h), resolved
// with dlsym so no link-time dependency on the SDK exists.
typedef NVSDK_NGX_Result(NVSDK_CONV *PFN_ngx_init)(unsigned long long, const wchar_t *, VkInstance, VkPhysicalDevice, VkDevice, PFN_vkGetInstanceProcAddr, PFN_vkGetDeviceProcAddr, const NVSDK_NGX_FeatureCommonInfo *, NVSDK_NGX_Version);
typedef NVSDK_NGX_Result(NVSDK_CONV *PFN_ngx_init_projectid)(const char *, NVSDK_NGX_EngineType, const char *, const wchar_t *, VkInstance, VkPhysicalDevice, VkDevice, PFN_vkGetInstanceProcAddr, PFN_vkGetDeviceProcAddr, const NVSDK_NGX_FeatureCommonInfo *, NVSDK_NGX_Version);
typedef NVSDK_NGX_Result(NVSDK_CONV *PFN_ngx_shutdown)(void);
typedef NVSDK_NGX_Result(NVSDK_CONV *PFN_ngx_allocate_parameters)(NVSDK_NGX_Parameter **);
typedef NVSDK_NGX_Result(NVSDK_CONV *PFN_ngx_destroy_parameters)(NVSDK_NGX_Parameter *);
typedef NVSDK_NGX_Result(NVSDK_CONV *PFN_ngx_create_feature)(VkCommandBuffer, NVSDK_NGX_Feature, const NVSDK_NGX_Parameter *, NVSDK_NGX_Handle **);
typedef NVSDK_NGX_Result(NVSDK_CONV *PFN_ngx_evaluate_feature)(VkCommandBuffer, const NVSDK_NGX_Handle *, const NVSDK_NGX_Parameter *, PFN_NVSDK_NGX_ProgressCallback_C);
typedef NVSDK_NGX_Result(NVSDK_CONV *PFN_ngx_release_feature)(NVSDK_NGX_Handle *);

struct NGXSymbols {
	void *library = nullptr;
	PFN_ngx_init init = nullptr;
	PFN_ngx_init_projectid init_project_id = nullptr;
	PFN_ngx_shutdown shutdown = nullptr;
	PFN_ngx_allocate_parameters allocate_parameters = nullptr;
	PFN_ngx_destroy_parameters destroy_parameters = nullptr;
	PFN_ngx_create_feature create_feature = nullptr;
	PFN_ngx_evaluate_feature evaluate_feature = nullptr;
	PFN_ngx_release_feature release_feature = nullptr;

	bool load() {
		OS *os = OS::get_singleton();
		if (os->open_dynamic_library("libnvidia-ngx-dlss.so", library) != OK) {
			// Windows name; also try the driver-store name used on Linux.
			if (os->open_dynamic_library("nvngx.dll", library) != OK) {
				return false;
			}
		}
		bool ok = true;
		ok = ok && os->get_dynamic_library_symbol_handle(library, "NVSDK_NGX_VULKAN_Shutdown", *(void **)&shutdown, true) == OK;
		ok = ok && os->get_dynamic_library_symbol_handle(library, "NVSDK_NGX_VULKAN_AllocateParameters", *(void **)&allocate_parameters, true) == OK;
		ok = ok && os->get_dynamic_library_symbol_handle(library, "NVSDK_NGX_VULKAN_DestroyParameters", *(void **)&destroy_parameters, true) == OK;
		ok = ok && os->get_dynamic_library_symbol_handle(library, "NVSDK_NGX_VULKAN_CreateFeature", *(void **)&create_feature, true) == OK;
		ok = ok && os->get_dynamic_library_symbol_handle(library, "NVSDK_NGX_VULKAN_ReleaseFeature", *(void **)&release_feature, true) == OK;
		// Evaluate: the plain and _C variants share the ABI with a null callback.
		if (os->get_dynamic_library_symbol_handle(library, "NVSDK_NGX_VULKAN_EvaluateFeature", *(void **)&evaluate_feature, true) != OK) {
			ok = ok && os->get_dynamic_library_symbol_handle(library, "NVSDK_NGX_VULKAN_EvaluateFeature_C", *(void **)&evaluate_feature, true) == OK;
		}
		// Init: prefer the project-ID entry point, fall back to the app-ID one.
		if (os->get_dynamic_library_symbol_handle(library, "NVSDK_NGX_VULKAN_Init_with_ProjectID", *(void **)&init_project_id, true) != OK) {
			ok = ok && os->get_dynamic_library_symbol_handle(library, "NVSDK_NGX_VULKAN_Init", *(void **)&init, true) == OK;
		}
		return ok;
	}
};

static NGXSymbols ngx;

DLSSContext::~DLSSContext() {
	if (created && handle && ngx.release_feature) {
		ngx.release_feature((NVSDK_NGX_Handle *)handle);
	}
	handle = nullptr;
	created = false;
}

static NVSDK_NGX_Resource_VK _make_image_resource(const RID &p_texture, const Size2i &p_size, bool p_depth, bool p_read_write) {
	RD *rd = RD::get_singleton();
	NVSDK_NGX_Resource_VK resource = { };
	resource.Type = NVSDK_NGX_RESOURCE_VK_TYPE_VK_IMAGEVIEW;
	resource.Resource.ImageViewInfo.ImageView = (VkImageView)rd->get_driver_resource(RDD::DRIVER_RESOURCE_TEXTURE_VIEW, p_texture);
	resource.Resource.ImageViewInfo.Image = (VkImage)rd->get_driver_resource(RDD::DRIVER_RESOURCE_TEXTURE, p_texture);
	resource.Resource.ImageViewInfo.SubresourceRange.aspectMask = p_depth ? VK_IMAGE_ASPECT_DEPTH_BIT : VK_IMAGE_ASPECT_COLOR_BIT;
	resource.Resource.ImageViewInfo.SubresourceRange.baseMipLevel = 0;
	resource.Resource.ImageViewInfo.SubresourceRange.levelCount = 1;
	resource.Resource.ImageViewInfo.SubresourceRange.baseArrayLayer = 0;
	resource.Resource.ImageViewInfo.SubresourceRange.layerCount = 1;
	resource.Resource.ImageViewInfo.Format = (VkFormat)rd->get_driver_resource(RDD::DRIVER_RESOURCE_TEXTURE_DATA_FORMAT, p_texture);
	resource.Resource.ImageViewInfo.Width = p_size.width;
	resource.Resource.ImageViewInfo.Height = p_size.height;
	resource.ReadWrite = p_read_write;
	return resource;
}

bool DLSSEffect::init_sdk() {
	RD *rd = RD::get_singleton();
	if (rd == nullptr) {
		return false;
	}

	vulkan_instance = (void *)rd->get_driver_resource(RDD::DRIVER_RESOURCE_TOPMOST_OBJECT);
	vulkan_device = (void *)rd->get_driver_resource(RDD::DRIVER_RESOURCE_LOGICAL_DEVICE);
	void *physical_device = (void *)rd->get_driver_resource(RDD::DRIVER_RESOURCE_PHYSICAL_DEVICE);
	queue = (void *)rd->get_driver_resource(RDD::DRIVER_RESOURCE_COMMAND_QUEUE);
	queue_family = (uint32_t)rd->get_driver_resource(RDD::DRIVER_RESOURCE_QUEUE_FAMILY);
	if (!vulkan_instance || !vulkan_device || !physical_device || !queue) {
		return false; // Not running on Vulkan.
	}

	if (!ngx.load()) {
		return false;
	}

	const wchar_t *app_path = L".";
	NVSDK_NGX_Result res;
	if (ngx.init_project_id) {
		res = ngx.init_project_id("com.hardionlabs.kaizenengine", NVSDK_NGX_ENGINE_TYPE_CUSTOM, "KaizenEngine", app_path,
				(VkInstance)vulkan_instance, (VkPhysicalDevice)physical_device, (VkDevice)vulkan_device,
				nullptr, nullptr, nullptr, NVSDK_NGX_Version_API);
	} else {
		// Not NVIDIA-assigned; unique per engine (NGX requirement).
		res = ngx.init(0x4B41495A, app_path,
				(VkInstance)vulkan_instance, (VkPhysicalDevice)physical_device, (VkDevice)vulkan_device,
				nullptr, nullptr, nullptr, NVSDK_NGX_Version_API);
	}
	if (res != NVSDK_NGX_Result_Success) {
		return false;
	}

	if (ngx.allocate_parameters((NVSDK_NGX_Parameter **)&parameters) != NVSDK_NGX_Result_Success || parameters == nullptr) {
		ngx.shutdown();
		return false;
	}

	// Dedicated command buffer for NGX dispatches (see the sync note above).
	VkCommandPoolCreateInfo pool_info = { };
	pool_info.sType = VK_STRUCTURE_TYPE_COMMAND_POOL_CREATE_INFO;
	pool_info.flags = VK_COMMAND_POOL_CREATE_RESET_COMMAND_BUFFER_BIT;
	pool_info.queueFamilyIndex = queue_family;
	if (vkCreateCommandPool((VkDevice)vulkan_device, &pool_info, nullptr, (VkCommandPool *)&command_pool) != VK_SUCCESS) {
		ngx.shutdown();
		return false;
	}
	VkCommandBufferAllocateInfo alloc_info = { };
	alloc_info.sType = VK_STRUCTURE_TYPE_COMMAND_BUFFER_ALLOCATE_INFO;
	alloc_info.commandPool = (VkCommandPool)command_pool;
	alloc_info.level = VK_COMMAND_BUFFER_LEVEL_PRIMARY;
	alloc_info.commandBufferCount = 1;
	if (vkAllocateCommandBuffers((VkDevice)vulkan_device, &alloc_info, (VkCommandBuffer *)&command_buffer) != VK_SUCCESS) {
		vkDestroyCommandPool((VkDevice)vulkan_device, (VkCommandPool)command_pool, nullptr);
		command_pool = nullptr;
		ngx.shutdown();
		return false;
	}
	return true;
}

void DLSSEffect::shutdown_sdk() {
	if (vulkan_device && command_pool) {
		vkDestroyCommandPool((VkDevice)vulkan_device, (VkCommandPool)command_pool, nullptr);
	}
	command_pool = nullptr;
	command_buffer = nullptr;
	if (parameters && ngx.destroy_parameters) {
		ngx.destroy_parameters((NVSDK_NGX_Parameter *)parameters);
	}
	parameters = nullptr;
	if (vulkan_device && ngx.shutdown) {
		ngx.shutdown();
	}
	vulkan_device = nullptr;
	vulkan_instance = nullptr;
	queue = nullptr;
	if (ngx.library) {
		OS::get_singleton()->close_dynamic_library(ngx.library);
		ngx = NGXSymbols();
	}
}

DLSSEffect::DLSSEffect() {
	supported = init_sdk();
}

DLSSEffect::~DLSSEffect() {
	if (dlss_singleton == this) {
		dlss_singleton = nullptr;
	}
	if (supported) {
		shutdown_sdk();
	}
	supported = false;
}

bool DLSSEffect::is_supported() const {
	return supported;
}

DLSSContext *DLSSEffect::create_context(Size2i p_internal_size, Size2i p_target_size) {
	DLSSContext *ctx = memnew(DLSSContext);
	ctx->internal_size = p_internal_size;
	ctx->target_size = p_target_size;
	if (!supported || p_internal_size.x <= 0 || p_internal_size.y <= 0 || p_target_size.x <= 0 || p_target_size.y <= 0) {
		return ctx; // created == false; caller falls back.
	}

	NVSDK_NGX_Parameter *params = (NVSDK_NGX_Parameter *)parameters;
	// Mirrors NGX_VULKAN_CREATE_DLSS_EXT (nvsdk_ngx_helpers_vk.h).
	params->Set(NVSDK_NGX_Parameter_CreationNodeMask, 1u);
	params->Set(NVSDK_NGX_Parameter_VisibilityNodeMask, 1u);
	params->Set(NVSDK_NGX_Parameter_Width, (unsigned int)p_internal_size.x);
	params->Set(NVSDK_NGX_Parameter_Height, (unsigned int)p_internal_size.y);
	params->Set(NVSDK_NGX_Parameter_OutWidth, (unsigned int)p_target_size.x);
	params->Set(NVSDK_NGX_Parameter_OutHeight, (unsigned int)p_target_size.y);
	params->Set(NVSDK_NGX_Parameter_PerfQualityValue, (int)NVSDK_NGX_PerfQuality_Value_MaxQuality);

	int create_flags = NVSDK_NGX_DLSS_Feature_Flags_IsHDR | // Internal buffers are HDR.
			NVSDK_NGX_DLSS_Feature_Flags_MVLowRes | // Velocity matches the render resolution.
			NVSDK_NGX_DLSS_Feature_Flags_DepthInverted; // Godot uses reversed-Z.
	params->Set(NVSDK_NGX_Parameter_DLSS_Feature_Create_Flags, create_flags);
	params->Set(NVSDK_NGX_Parameter_DLSS_Enable_Output_Subrects, 0);

	VkCommandBufferBeginInfo begin_info = { };
	begin_info.sType = VK_STRUCTURE_TYPE_COMMAND_BUFFER_BEGIN_INFO;
	begin_info.flags = VK_COMMAND_BUFFER_USAGE_ONE_TIME_SUBMIT_BIT;
	VkCommandBuffer cmd = (VkCommandBuffer)command_buffer;
	vkResetCommandBuffer(cmd, 0);
	vkBeginCommandBuffer(cmd, &begin_info);
	NVSDK_NGX_Result res = ngx.create_feature(cmd, NVSDK_NGX_Feature_SuperSampling, params, (NVSDK_NGX_Handle **)&ctx->handle);
	vkEndCommandBuffer(cmd);

	VkSubmitInfo submit = { };
	submit.sType = VK_STRUCTURE_TYPE_SUBMIT_INFO;
	submit.commandBufferCount = 1;
	submit.pCommandBuffers = &cmd;
	vkQueueSubmit((VkQueue)queue, 1, &submit, VK_NULL_HANDLE);
	vkQueueWaitIdle((VkQueue)queue);

	if (res == NVSDK_NGX_Result_Success && ctx->handle) {
		ctx->created = true;
	} else {
		ctx->handle = nullptr;
	}
	return ctx;
}

void DLSSEffect::upscale(const Parameters &p_params) {
	if (!supported || p_params.context == nullptr || !p_params.context->created) {
		return;
	}
	if (p_params.internal_size.x <= 0 || p_params.target_size.x <= 0) {
		return;
	}

	RD *rd = RD::get_singleton();

	// Push the engine's recorded work onto the queue before our submission so
	// the barrier below can order against it. No sync() here: the ALL_COMMANDS
	// / MEMORY_WRITE barrier scope already covers earlier submissions on the
	// queue, so the GPU pipelines both instead of stalling the CPU.
	rd->submit();

	const Size2i internal = p_params.internal_size;
	const Size2i target = p_params.target_size;

	NVSDK_NGX_Resource_VK color = _make_image_resource(p_params.color, internal, false, false);
	NVSDK_NGX_Resource_VK depth = _make_image_resource(p_params.depth, internal, true, false);
	NVSDK_NGX_Resource_VK velocity = _make_image_resource(p_params.velocity, internal, false, false);
	NVSDK_NGX_Resource_VK output = _make_image_resource(p_params.output, target, false, true);

	NVSDK_NGX_Resource_VK reactive = { };
	bool has_reactive = p_params.reactive.is_valid();
	if (has_reactive) {
		reactive = _make_image_resource(p_params.reactive, internal, false, false);
	}

	NVSDK_NGX_Resource_VK exposure = { };
	bool has_exposure = p_params.exposure.is_valid();
	if (has_exposure) {
		RD *r = RD::get_singleton();
		exposure.Type = NVSDK_NGX_RESOURCE_VK_TYPE_VK_BUFFER;
		exposure.Resource.BufferInfo.Buffer = (VkBuffer)r->get_driver_resource(RDD::DRIVER_RESOURCE_BUFFER, p_params.exposure);
		exposure.Resource.BufferInfo.SizeInBytes = sizeof(float) * 4;
		exposure.ReadWrite = false;
	}

	NVSDK_NGX_Parameter *params = (NVSDK_NGX_Parameter *)parameters;
	// Mirrors NGX_VULKAN_EVALUATE_DLSS_EXT (nvsdk_ngx_helpers_vk.h).
	params->Set(NVSDK_NGX_Parameter_Color, &color);
	params->Set(NVSDK_NGX_Parameter_Output, &output);
	params->Set(NVSDK_NGX_Parameter_Depth, &depth);
	params->Set(NVSDK_NGX_Parameter_MotionVectors, &velocity);
	params->Set(NVSDK_NGX_Parameter_Jitter_Offset_X, p_params.jitter.x);
	params->Set(NVSDK_NGX_Parameter_Jitter_Offset_Y, p_params.jitter.y);
	params->Set(NVSDK_NGX_Parameter_Reset, p_params.reset_accumulation ? 1 : 0);
	// The velocity buffer is in NDC units; scale to render pixels like the
	// FSR2 path does (fsr2.cpp: motionVectorScale = internal_size).
	params->Set(NVSDK_NGX_Parameter_MV_Scale_X, (float)internal.x);
	params->Set(NVSDK_NGX_Parameter_MV_Scale_Y, (float)internal.y);
	params->Set(NVSDK_NGX_Parameter_TransparencyMask, (void *)(has_reactive ? &reactive : nullptr));
	params->Set(NVSDK_NGX_Parameter_ExposureTexture, (void *)(has_exposure ? &exposure : nullptr));
	params->Set(NVSDK_NGX_Parameter_DLSS_Input_Bias_Current_Color_Mask, (void *)nullptr);
	params->Set(NVSDK_NGX_Parameter_FrameTimeDeltaInMsec, p_params.delta_time * 1000.0f);
	params->Set(NVSDK_NGX_Parameter_DLSS_Input_Color_Subrect_Base_X, 0u);
	params->Set(NVSDK_NGX_Parameter_DLSS_Input_Color_Subrect_Base_Y, 0u);
	params->Set(NVSDK_NGX_Parameter_DLSS_Input_Depth_Subrect_Base_X, 0u);
	params->Set(NVSDK_NGX_Parameter_DLSS_Input_Depth_Subrect_Base_Y, 0u);
	params->Set(NVSDK_NGX_Parameter_DLSS_Input_MV_SubrectBase_X, 0u);
	params->Set(NVSDK_NGX_Parameter_DLSS_Input_MV_SubrectBase_Y, 0u);
	params->Set(NVSDK_NGX_Parameter_DLSS_Output_Subrect_Base_X, 0u);
	params->Set(NVSDK_NGX_Parameter_DLSS_Output_Subrect_Base_Y, 0u);
	params->Set(NVSDK_NGX_Parameter_DLSS_Render_Subrect_Dimensions_Width, (unsigned int)internal.x);
	params->Set(NVSDK_NGX_Parameter_DLSS_Render_Subrect_Dimensions_Height, (unsigned int)internal.y);
	params->Set(NVSDK_NGX_Parameter_DLSS_Pre_Exposure, 1.0f);
	params->Set(NVSDK_NGX_Parameter_DLSS_Exposure_Scale, 1.0f);

	VkCommandBuffer cmd = (VkCommandBuffer)command_buffer;
	VkCommandBufferBeginInfo begin_info = { };
	begin_info.sType = VK_STRUCTURE_TYPE_COMMAND_BUFFER_BEGIN_INFO;
	begin_info.flags = VK_COMMAND_BUFFER_USAGE_ONE_TIME_SUBMIT_BIT;
	vkResetCommandBuffer(cmd, 0);
	vkBeginCommandBuffer(cmd, &begin_info);

	// Conservative dependencies: the engine keeps textures at rest in
	// SHADER_READ_ONLY_OPTIMAL (rendering_device.cpp _texture_initialize), so
	// move the inputs to GENERAL for the SDK and put everything back after.
	VkImageMemoryBarrier barriers[4] = { };
	uint32_t barrier_count = 0;
	auto push_barrier = [&](const NVSDK_NGX_Resource_VK &p_res, bool p_write) {
		VkImageMemoryBarrier &b = barriers[barrier_count++];
		b = { };
		b.sType = VK_STRUCTURE_TYPE_IMAGE_MEMORY_BARRIER;
		b.srcAccessMask = VK_ACCESS_MEMORY_WRITE_BIT | VK_ACCESS_MEMORY_READ_BIT;
		b.dstAccessMask = VK_ACCESS_MEMORY_WRITE_BIT | VK_ACCESS_MEMORY_READ_BIT;
		b.oldLayout = p_write ? VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL : VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL;
		b.newLayout = VK_IMAGE_LAYOUT_GENERAL;
		b.srcQueueFamilyIndex = VK_QUEUE_FAMILY_IGNORED;
		b.dstQueueFamilyIndex = VK_QUEUE_FAMILY_IGNORED;
		b.image = p_res.Resource.ImageViewInfo.Image;
		b.subresourceRange = p_res.Resource.ImageViewInfo.SubresourceRange;
	};
	push_barrier(color, false);
	push_barrier(depth, false);
	push_barrier(velocity, false);
	if (has_reactive) {
		push_barrier(reactive, false);
	}
	vkCmdPipelineBarrier(cmd, VK_PIPELINE_STAGE_ALL_COMMANDS_BIT, VK_PIPELINE_STAGE_ALL_COMMANDS_BIT, 0,
			0, nullptr, 0, nullptr, barrier_count, barriers);

	NVSDK_NGX_Result res = ngx.evaluate_feature(cmd, (NVSDK_NGX_Handle *)p_params.context->handle, params, nullptr);

	barrier_count = 0;
	push_barrier(output, true);
	push_barrier(color, false);
	push_barrier(depth, false);
	push_barrier(velocity, false);
	for (uint32_t i = 0; i < barrier_count; i++) {
		barriers[i].oldLayout = VK_IMAGE_LAYOUT_GENERAL;
		barriers[i].newLayout = VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL;
	}
	vkCmdPipelineBarrier(cmd, VK_PIPELINE_STAGE_ALL_COMMANDS_BIT, VK_PIPELINE_STAGE_ALL_COMMANDS_BIT, 0,
			0, nullptr, 0, nullptr, barrier_count, barriers);
	vkEndCommandBuffer(cmd);

	VkSubmitInfo submit = { };
	submit.sType = VK_STRUCTURE_TYPE_SUBMIT_INFO;
	submit.commandBufferCount = 1;
	submit.pCommandBuffers = &cmd;
	vkQueueSubmit((VkQueue)queue, 1, &submit, VK_NULL_HANDLE);
	vkQueueWaitIdle((VkQueue)queue);

	if (res != NVSDK_NGX_Result_Success) {
		WARN_PRINT_ONCE("DLSS evaluate failed; upscaling result will be stale for this frame.");
	}
}

#else // NGX SDK or Vulkan not compiled in.

DLSSContext::~DLSSContext() {
}

bool DLSSEffect::init_sdk() {
	return false;
}

void DLSSEffect::shutdown_sdk() {
}

DLSSEffect::DLSSEffect() {
	supported = false;
}

DLSSEffect::~DLSSEffect() {
	if (dlss_singleton == this) {
		dlss_singleton = nullptr;
	}
}

bool DLSSEffect::is_supported() const {
	return supported;
}

DLSSContext *DLSSEffect::create_context(Size2i p_internal_size, Size2i p_target_size) {
	DLSSContext *ctx = memnew(DLSSContext);
	ctx->internal_size = p_internal_size;
	ctx->target_size = p_target_size;
	return ctx;
}

void DLSSEffect::upscale(const Parameters &p_params) {
	(void)p_params;
}

#endif // NGX_DLSS_ENABLED && VULKAN_ENABLED
