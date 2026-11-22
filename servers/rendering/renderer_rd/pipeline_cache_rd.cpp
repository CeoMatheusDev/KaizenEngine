//************************************************************************//
//  pipeline_cache_rd.cpp                                                 //
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
// Modified by Matheus A. Santos, Hardion Labs on 2026-11-17.             //
// Modifications and additional code are licensed under the GNU Affero    //
// General Public License v3.0 (AGPL-3.0).                                //
// See the files LICENSE.txt and LICENSE-ADDITIONAL-TERMS.md for details. //
//                                                                        //
// Permission is hereby granted, free of charge, to any person obtaining  //
// a copy of this software and associated documentation files (the        //
// "Software"), to deal in the Software without restriction, including    //
// without limitation the rights to use, copy, modify, merge, publish,    //
// distribute, sublicense, and/or sell copies of the Software, and to     //
// permit persons to whom the Software is furnished to do so, subject to  //
// the following conditions:                                              //
//                                                                        //
// The above copyright notice and this permission notice shall be         //
// included in all copies or substantial portions of the Software.        //
//                                                                        //
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,        //
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF     //
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. //
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY   //
// CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,   //
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE      //
// SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.                 //
//************************************************************************//

#include "pipeline_cache_rd.h"

#include "core/os/memory.h"
#include "core/os/os.h"

static void _append_stencil_key(Kaizen::PipelineKey &r_key, const RD::PipelineDepthStencilState::StencilOperationState &p_state) {
	r_key.add_u32(static_cast<uint32_t>(p_state.fail));
	r_key.add_u32(static_cast<uint32_t>(p_state.pass));
	r_key.add_u32(static_cast<uint32_t>(p_state.depth_fail));
	r_key.add_u32(static_cast<uint32_t>(p_state.compare));
	r_key.add_u32(p_state.compare_mask);
	r_key.add_u32(p_state.write_mask);
	r_key.add_u32(p_state.reference);
}

static void _append_specialization_key(Kaizen::PipelineKey &r_key, const Vector<RD::PipelineSpecializationConstant> &p_constants) {
	r_key.add_u32(p_constants.size());
	for (int i = 0; i < p_constants.size(); i++) {
		const RD::PipelineSpecializationConstant &constant = p_constants[i];
		r_key.add_u32(static_cast<uint32_t>(constant.type));
		r_key.add_u32(constant.constant_id);
		switch (constant.type) {
			case RD::PIPELINE_SPECIALIZATION_CONSTANT_TYPE_BOOL:
				r_key.add_bool(constant.bool_value);
				break;
			case RD::PIPELINE_SPECIALIZATION_CONSTANT_TYPE_INT:
				r_key.add_u32(constant.int_value);
				break;
			case RD::PIPELINE_SPECIALIZATION_CONSTANT_TYPE_FLOAT:
				r_key.add_float(constant.float_value);
				break;
		}
	}
}

static Kaizen::PipelineKey _make_pipeline_key(RID p_shader, RD::VertexFormatID p_vertex_format_id, RD::FramebufferFormatID p_framebuffer_format_id, RD::RenderPrimitive p_render_primitive, const RD::PipelineRasterizationState &p_rasterization_state, const RD::PipelineMultisampleState &p_multisample_state, const RD::PipelineDepthStencilState &p_depth_stencil_state, const RD::PipelineColorBlendState &p_blend_state, int p_dynamic_state_flags, uint32_t p_render_pass, uint32_t p_bool_specializations, const Vector<RD::PipelineSpecializationConstant> &p_specialization_constants) {
	Kaizen::PipelineKey key;
	// A full key is roughly 100 fields; reserving once avoids repeated
	// reallocations while the fields are appended below.
	key.reserve(1024);
	key.add_u32(2); // Key schema version (compact field encoding).
	key.add_u64(p_shader.get_id());
	key.add_u64(static_cast<uint64_t>(p_vertex_format_id));
	key.add_u64(static_cast<uint64_t>(p_framebuffer_format_id));
	key.add_u32(static_cast<uint32_t>(p_render_primitive));
	key.add_u32(static_cast<uint32_t>(p_dynamic_state_flags));
	key.add_u32(p_render_pass);
	key.add_u32(p_bool_specializations);

	key.add_bool(p_rasterization_state.enable_depth_clamp);
	key.add_bool(p_rasterization_state.discard_primitives);
	key.add_bool(p_rasterization_state.wireframe);
	key.add_u32(static_cast<uint32_t>(p_rasterization_state.cull_mode));
	key.add_u32(static_cast<uint32_t>(p_rasterization_state.front_face));
	key.add_bool(p_rasterization_state.depth_bias_enabled);
	key.add_float(p_rasterization_state.depth_bias_constant_factor);
	key.add_float(p_rasterization_state.depth_bias_clamp);
	key.add_float(p_rasterization_state.depth_bias_slope_factor);
	key.add_float(p_rasterization_state.line_width);
	key.add_u32(p_rasterization_state.patch_control_points);

	key.add_u32(static_cast<uint32_t>(p_multisample_state.sample_count));
	key.add_bool(p_multisample_state.enable_sample_shading);
	key.add_float(p_multisample_state.min_sample_shading);
	key.add_u32(p_multisample_state.sample_mask.size());
	for (int i = 0; i < p_multisample_state.sample_mask.size(); i++) {
		key.add_u32(p_multisample_state.sample_mask[i]);
	}
	key.add_bool(p_multisample_state.enable_alpha_to_coverage);
	key.add_bool(p_multisample_state.enable_alpha_to_one);

	key.add_bool(p_depth_stencil_state.enable_depth_test);
	key.add_bool(p_depth_stencil_state.enable_depth_write);
	key.add_u32(static_cast<uint32_t>(p_depth_stencil_state.depth_compare_operator));
	key.add_bool(p_depth_stencil_state.enable_depth_range);
	key.add_float(p_depth_stencil_state.depth_range_min);
	key.add_float(p_depth_stencil_state.depth_range_max);
	key.add_bool(p_depth_stencil_state.enable_stencil);
	_append_stencil_key(key, p_depth_stencil_state.front_op);
	_append_stencil_key(key, p_depth_stencil_state.back_op);

	key.add_bool(p_blend_state.enable_logic_op);
	key.add_u32(static_cast<uint32_t>(p_blend_state.logic_op));
	key.add_u32(p_blend_state.attachments.size());
	for (int i = 0; i < p_blend_state.attachments.size(); i++) {
		const RD::PipelineColorBlendState::Attachment &attachment = p_blend_state.attachments[i];
		key.add_bool(attachment.enable_blend);
		key.add_u32(static_cast<uint32_t>(attachment.src_color_blend_factor));
		key.add_u32(static_cast<uint32_t>(attachment.dst_color_blend_factor));
		key.add_u32(static_cast<uint32_t>(attachment.color_blend_op));
		key.add_u32(static_cast<uint32_t>(attachment.src_alpha_blend_factor));
		key.add_u32(static_cast<uint32_t>(attachment.dst_alpha_blend_factor));
		key.add_u32(static_cast<uint32_t>(attachment.alpha_blend_op));
		key.add_bool(attachment.write_r);
		key.add_bool(attachment.write_g);
		key.add_bool(attachment.write_b);
		key.add_bool(attachment.write_a);
	}
	key.add_float(p_blend_state.blend_constant.r);
	key.add_float(p_blend_state.blend_constant.g);
	key.add_float(p_blend_state.blend_constant.b);
	key.add_float(p_blend_state.blend_constant.a);

	_append_specialization_key(key, p_specialization_constants);
	return key;
}

RID PipelineCacheRD::_generate_version(RD::VertexFormatID p_vertex_format_id, RD::FramebufferFormatID p_framebuffer_format_id, bool p_wireframe, uint32_t p_render_pass, uint32_t p_bool_specializations) {
	RD::PipelineMultisampleState multisample_state_version = multisample_state;
	multisample_state_version.sample_count = RD::get_singleton()->framebuffer_format_get_texture_samples(p_framebuffer_format_id, p_render_pass);

	bool wireframe = p_wireframe;

	RD::PipelineRasterizationState raster_state_version = rasterization_state;
	raster_state_version.wireframe = wireframe;

	Vector<RD::PipelineSpecializationConstant> specialization_constants = base_specialization_constants;

	uint32_t bool_index = 0;
	uint32_t bool_specializations = p_bool_specializations;
	while (bool_specializations) {
		RD::PipelineSpecializationConstant sc;
		sc.bool_value = bool(bool_specializations & (1u << bool_index));
		sc.constant_id = bool_index;
		sc.type = RD::PIPELINE_SPECIALIZATION_CONSTANT_TYPE_BOOL;
		specialization_constants.push_back(sc);
		bool_specializations &= ~(1u << bool_index);
		bool_index++;
	}

	Kaizen::PipelineKey pipeline_key = _make_pipeline_key(shader, p_vertex_format_id, p_framebuffer_format_id, render_primitive, raster_state_version, multisample_state_version, depth_stencil_state, blend_state, dynamic_state_flags, p_render_pass, p_bool_specializations, specialization_constants);
	uint64_t pipeline_key_hash = pipeline_key.hash();
	Kaizen::PipelineHandle cache_handle;
	RID pipeline = RD::get_singleton()->render_pipeline_create_cached(pipeline_key_hash, pipeline_key, shader, p_framebuffer_format_id, p_vertex_format_id, render_primitive, raster_state_version, multisample_state_version, depth_stencil_state, blend_state, dynamic_state_flags, p_render_pass, specialization_constants, cache_handle);
	ERR_FAIL_COND_V(pipeline.is_null(), RID());

	static bool shared_acquire_test_ran = false;
	const String cache_verify_mode = OS::get_singleton()->get_environment("KAIZEN_PIPELINE_CACHE_VERIFY");
	if (!shared_acquire_test_ran && cache_verify_mode == "on") {
		shared_acquire_test_ran = true;
		ERR_FAIL_COND_V_MSG(cache_handle.is_null(), RID(), "Pipeline cache verification requires KAIZEN_PIPELINE_CACHE=on.");
		Kaizen::PipelineHandle duplicate_handle;
		const RID duplicate_pipeline = RD::get_singleton()->render_pipeline_create_cached(pipeline_key_hash, pipeline_key, shader, p_framebuffer_format_id, p_vertex_format_id, render_primitive, raster_state_version, multisample_state_version, depth_stencil_state, blend_state, dynamic_state_flags, p_render_pass, specialization_constants, duplicate_handle);
		const bool shared_identity = duplicate_pipeline == pipeline && duplicate_handle == cache_handle;
		if (!duplicate_handle.is_null()) {
			RD::get_singleton()->pipeline_cache_release(duplicate_handle);
		}
		ERR_FAIL_COND_V_MSG(!shared_identity, RID(), "Pipeline cache verification failed: equivalent requests did not share one RID and handle.");
		ERR_FAIL_COND_V_MSG(!RD::get_singleton()->render_pipeline_is_valid(pipeline), RID(), "Pipeline cache verification failed: releasing one logical reference invalidated the shared RID.");
		print_line("[Kaizen][PipelineCacheTest] duplicate acquire and partial release: PASS");
	}

	versions = static_cast<Version *>(memrealloc(versions, sizeof(Version) * (version_count + 1)));
	versions[version_count].framebuffer_id = p_framebuffer_format_id;
	versions[version_count].vertex_id = p_vertex_format_id;
	versions[version_count].wireframe = wireframe;
	versions[version_count].pipeline = pipeline;
	versions[version_count].render_pass = p_render_pass;
	versions[version_count].bool_specializations = p_bool_specializations;
	versions[version_count].cache_handle = cache_handle;
	version_count++;
	return pipeline;
}

void PipelineCacheRD::_clear() {
	// TODO: Clear should probably recompile all the variants already compiled instead to avoid stalls? Needs discussion.
	if (versions) {
		for (uint32_t i = 0; i < version_count; i++) {
			if (!versions[i].cache_handle.is_null()) {
				RD::get_singleton()->pipeline_cache_release(versions[i].cache_handle);
			} else if (RD::get_singleton()->render_pipeline_is_valid(versions[i].pipeline)) {
				RD::get_singleton()->free_rid(versions[i].pipeline);
			}
		}
		version_count = 0;
		memfree(versions);
		versions = nullptr;
	}
}

void PipelineCacheRD::setup(RID p_shader, RD::RenderPrimitive p_primitive, const RD::PipelineRasterizationState &p_rasterization_state, RD::PipelineMultisampleState p_multisample, const RD::PipelineDepthStencilState &p_depth_stencil_state, const RD::PipelineColorBlendState &p_blend_state, int p_dynamic_state_flags, const Vector<RD::PipelineSpecializationConstant> &p_base_specialization_constants) {
	ERR_FAIL_COND(p_shader.is_null());
	_clear();
	shader = p_shader;
	render_primitive = p_primitive;
	rasterization_state = p_rasterization_state;
	multisample_state = p_multisample;
	depth_stencil_state = p_depth_stencil_state;
	blend_state = p_blend_state;
	dynamic_state_flags = p_dynamic_state_flags;
	base_specialization_constants = p_base_specialization_constants;
}
void PipelineCacheRD::update_specialization_constants(const Vector<RD::PipelineSpecializationConstant> &p_base_specialization_constants) {
	base_specialization_constants = p_base_specialization_constants;
	_clear();
}

void PipelineCacheRD::update_shader(RID p_shader) {
	ERR_FAIL_COND(p_shader.is_null());
	_clear();
	setup(p_shader, render_primitive, rasterization_state, multisample_state, depth_stencil_state, blend_state, dynamic_state_flags);
}

void PipelineCacheRD::clear() {
	_clear();
	shader = RID(); //clear shader
}

PipelineCacheRD::PipelineCacheRD() {
	version_count = 0;
	versions = nullptr;
}

PipelineCacheRD::~PipelineCacheRD() {
	_clear();
}
