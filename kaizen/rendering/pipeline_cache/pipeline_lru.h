/**************************************************************************/
/*  pipeline_lru.h                                                        */
/**************************************************************************/
/*  Intrusive LRU list over pipeline registry slots.                      */
/*                                                                        */
/*  The list stores slot indices, not pointers, so it can be resized with */
/*  the registry and never allocates during touch/remove.                 */
/**************************************************************************/

#pragma once

#include <cstdint>
#include <vector>

namespace Kaizen {

class PipelineLRU {
public:
	static constexpr uint32_t NO_INDEX = UINT32_MAX;

	explicit PipelineLRU(uint32_t p_capacity);

	// Inserts the index at the most-recently-used end, or moves it there.
	void touch(uint32_t p_index);
	void remove(uint32_t p_index);

	bool contains(uint32_t p_index) const;
	uint32_t peek_lru() const { return tail; }
	uint32_t pop_lru();

	uint32_t size() const { return count; }
	bool is_empty() const { return count == 0; }

private:
	void _link_mru(uint32_t p_index);

	std::vector<uint32_t> prev;
	std::vector<uint32_t> next;
	uint32_t head = NO_INDEX; // Most recently used.
	uint32_t tail = NO_INDEX; // Least recently used.
	uint32_t count = 0;
};

} // namespace Kaizen
