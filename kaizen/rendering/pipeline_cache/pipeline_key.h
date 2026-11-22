/**************************************************************************/
/*  pipeline_key.h                                                        */
/**************************************************************************/
/*  Canonical, padding-free identity for pipeline cache entries.          */
/*                                                                        */
/*  Encoding: each field is stored as type byte + size + payload. The     */
/*  size uses one byte for payloads up to 254 bytes and an escape (255    */
/*  followed by a little-endian uint64) for larger payloads, so the       */
/*  concatenation of fields is uniquely decodable and two different       */
/*  field sequences can never produce the same byte string.               */
/*                                                                        */
/*  The hash is FNV-1a over 8-byte words (with a byte tail) finished by   */
/*  a splitmix64 finalizer, so the low bits are well distributed for      */
/*  power-of-two hash tables. It is only used for bucket selection and    */
/*  diagnostics; identity is always the full byte string.                 */
/**************************************************************************/

#pragma once

#include <cstddef>
#include <cstdint>
#include <cstring>
#include <vector>

namespace Kaizen {

// FNV-1a over raw bytes with a splitmix64 finalizer. Deterministic for a
// given byte string; used for hash-table bucket selection only.
inline uint64_t pipeline_identity_hash(const uint8_t *p_data, size_t p_size) {
	uint64_t h = 0xcbf29ce484222325ULL;
	size_t i = 0;
	for (; i + sizeof(uint64_t) <= p_size; i += sizeof(uint64_t)) {
		uint64_t word = 0;
		std::memcpy(&word, p_data + i, sizeof(word));
		h ^= word;
		h *= 0x100000001b3ULL;
	}
	for (; i < p_size; i++) {
		h ^= p_data[i];
		h *= 0x100000001b3ULL;
	}
	// splitmix64 finalizer: spreads entropy into the low bits used for
	// power-of-two bucket selection.
	h ^= h >> 30;
	h *= 0xbf58476d1ce4e5b9ULL;
	h ^= h >> 27;
	h *= 0x94d049bb133111ebULL;
	h ^= h >> 31;
	return h;
}

struct PipelineKey {
	std::vector<uint8_t> data;

	bool operator==(const PipelineKey &p_other) const {
		return data == p_other.data;
	}

	bool operator!=(const PipelineKey &p_other) const {
		return !(*this == p_other);
	}

	// Reserves room for `p_count` encoded bytes up front, avoiding the
	// repeated reallocations of incremental growth when building a key.
	void reserve(size_t p_count) {
		data.reserve(p_count);
	}

	void clear() {
		data.clear();
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
		// Appends the payload directly instead of staging it in a
		// temporary vector first.
		_append_header(6, p_count * sizeof(uint32_t));
		for (size_t i = 0; i < p_count; i++) {
			for (size_t byte = 0; byte < sizeof(uint32_t); byte++) {
				data.push_back(static_cast<uint8_t>(p_values[i] >> (byte * 8)));
			}
		}
	}

	uint64_t hash() const {
		return pipeline_identity_hash(data.data(), data.size());
	}

private:
	void _append_header(uint8_t p_type, size_t p_size) {
		data.push_back(p_type);
		if (p_size < 255) {
			data.push_back(static_cast<uint8_t>(p_size));
		} else {
			data.push_back(255);
			const uint64_t size = static_cast<uint64_t>(p_size);
			for (size_t i = 0; i < sizeof(size); i++) {
				data.push_back(static_cast<uint8_t>(size >> (i * 8)));
			}
		}
	}

	void _append(uint8_t p_type, const uint8_t *p_value, size_t p_size) {
		_append_header(p_type, p_size);
		if (p_size != 0) {
			data.insert(data.end(), p_value, p_value + p_size);
		}
	}
};

inline uint64_t pipeline_key_hash(const PipelineKey &p_key) {
	return p_key.hash();
}

} // namespace Kaizen
