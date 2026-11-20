/**************************************************************************/
/*  pipeline_key.h                                                        */
/**************************************************************************/
/*  Canonical, padding-free identity for pipeline cache entries.          */
/**************************************************************************/

#pragma once

#include <cstddef>
#include <cstdint>
#include <cstring>
#include <vector>

namespace Kaizen {

struct PipelineKey {
	std::vector<uint8_t> data;

	bool operator==(const PipelineKey &p_other) const {
		return data == p_other.data;
	}

	bool operator!=(const PipelineKey &p_other) const {
		return !(*this == p_other);
	}

	void add_bool(bool p_value) {
		const uint8_t value = p_value ? 1 : 0;
		_append(1, &value, sizeof(value));
	}

	void add_u32(uint32_t p_value) {
		uint8_t value[sizeof(p_value)];
		for (size_t i = 0; i < sizeof(value); i++) {
			value[i] = static_cast<uint8_t>(p_value >> (i * 8));
		}
		_append(2, value, sizeof(value));
	}

	void add_i32(int32_t p_value) {
		const uint32_t bits = static_cast<uint32_t>(p_value);
		uint8_t value[sizeof(bits)];
		for (size_t i = 0; i < sizeof(value); i++) {
			value[i] = static_cast<uint8_t>(bits >> (i * 8));
		}
		_append(3, value, sizeof(value));
	}

	void add_u64(uint64_t p_value) {
		uint8_t value[sizeof(p_value)];
		for (size_t i = 0; i < sizeof(value); i++) {
			value[i] = static_cast<uint8_t>(p_value >> (i * 8));
		}
		_append(4, value, sizeof(value));
	}

	void add_float(float p_value) {
		static_assert(sizeof(float) == sizeof(uint32_t), "Pipeline keys require 32-bit floats.");
		uint32_t bits = 0;
		std::memcpy(&bits, &p_value, sizeof(bits));
		uint8_t value[sizeof(bits)];
		for (size_t i = 0; i < sizeof(value); i++) {
			value[i] = static_cast<uint8_t>(bits >> (i * 8));
		}
		_append(5, value, sizeof(value));
	}

	void add_u32_array(const uint32_t *p_values, size_t p_count) {
		std::vector<uint8_t> value;
		value.reserve(p_count * sizeof(uint32_t));
		for (size_t i = 0; i < p_count; i++) {
			for (size_t byte = 0; byte < sizeof(uint32_t); byte++) {
				value.push_back(static_cast<uint8_t>(p_values[i] >> (byte * 8)));
			}
		}
		_append(6, value.data(), value.size());
	}

	uint64_t hash() const {
		uint64_t h = 0xcbf29ce484222325ULL;
		for (uint8_t byte : data) {
			h ^= byte;
			h *= 0x100000001b3ULL;
		}
		return h;
	}

private:
	void _append(uint8_t p_type, const uint8_t *p_value, size_t p_size) {
		data.push_back(p_type);
		for (size_t i = 0; i < sizeof(uint64_t); i++) {
			data.push_back(static_cast<uint8_t>(static_cast<uint64_t>(p_size) >> (i * 8)));
		}
		if (p_size != 0) {
			data.insert(data.end(), p_value, p_value + p_size);
		}
	}
};

inline uint64_t pipeline_key_hash(const PipelineKey &p_key) {
	return p_key.hash();
}

} // namespace Kaizen
