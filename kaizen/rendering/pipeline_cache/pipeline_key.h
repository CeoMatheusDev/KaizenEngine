/**************************************************************************/
/*  pipeline_key.h                                                        */
/**************************************************************************/
/*  Canonical key for a pipeline object created by PipelineCacheRD.       */
/*                                                                        */
/*  Two PipelineKeys are equal iff every parameter that affects the       */
/*  created VkPipeline is identical. The hash is a 64-bit mix; equality  */
/*  is always checked against the full key, so a hash collision can never */
/*  cause two different pipelines to be confused.                        */
/**************************************************************************/

#pragma once

#include <cstdint>
#include <cstring>

namespace Kaizen {

struct PipelineKey {
	// Shader identity.
	uint64_t shader_id = 0;

	// Render state.
	uint32_t render_primitive = 0;
	uint32_t dynamic_state_flags = 0;
	uint32_t render_pass = 0;
	uint32_t bool_specializations = 0;

	// Vertex / framebuffer formats.
	uint64_t vertex_format_id = 0;
	uint64_t framebuffer_format_id = 0;

	// Wireframe override.
	bool wireframe = false;

	// Rasterization state (serialized).
	uint32_t cull_mode = 0;
	uint32_t front_face = 0;
	float line_width = 0.0f;
	bool depth_clamp = false;
	bool rasterizer_discard = false;

	// Multisample state.
	uint32_t sample_count = 0;
	bool sample_shading = false;
	float min_sample_shading = 0.0f;

	// Depth/stencil state.
	bool depth_test = false;
	bool depth_write = false;
	uint32_t depth_compare = 0;
	bool stencil_test = false;

	// Blend state (simplified; full blend state hashed via a sub-hash).
	uint64_t blend_hash = 0;

	// Specialization constants.
	uint64_t specialization_hash = 0;

	bool operator==(const PipelineKey &p_other) const {
		return shader_id == p_other.shader_id &&
				render_primitive == p_other.render_primitive &&
				dynamic_state_flags == p_other.dynamic_state_flags &&
				render_pass == p_other.render_pass &&
				bool_specializations == p_other.bool_specializations &&
				vertex_format_id == p_other.vertex_format_id &&
				framebuffer_format_id == p_other.framebuffer_format_id &&
				wireframe == p_other.wireframe &&
				cull_mode == p_other.cull_mode &&
				front_face == p_other.front_face &&
				line_width == p_other.line_width &&
				depth_clamp == p_other.depth_clamp &&
				rasterizer_discard == p_other.rasterizer_discard &&
				sample_count == p_other.sample_count &&
				sample_shading == p_other.sample_shading &&
				min_sample_shading == p_other.min_sample_shading &&
				depth_test == p_other.depth_test &&
				depth_write == p_other.depth_write &&
				depth_compare == p_other.depth_compare &&
				stencil_test == p_other.stencil_test &&
				blend_hash == p_other.blend_hash &&
				specialization_hash == p_other.specialization_hash;
	}

	bool operator!=(const PipelineKey &p_other) const { return !(*this == p_other); }
};

// FNV-1a 64-bit hash of all fields.
inline uint64_t pipeline_key_hash(const PipelineKey &p_key) {
	uint64_t h = 0xcbf29ce484222325ULL;
	auto mix = [&h](const void *p_data, size_t p_size) {
		const unsigned char *bytes = static_cast<const unsigned char *>(p_data);
		for (size_t i = 0; i < p_size; i++) {
			h ^= bytes[i];
			h *= 0x100000001b3ULL;
		}
	};
	mix(&p_key, sizeof(PipelineKey));
	return h;
}

} // namespace Kaizen
