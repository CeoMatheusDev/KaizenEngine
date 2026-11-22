/**************************************************************************/
/*  pipeline_lru.cpp                                                      */
/**************************************************************************/

#include "pipeline_lru.h"

namespace Kaizen {

PipelineLRU::PipelineLRU(uint32_t p_capacity) {
	prev.assign(p_capacity, NO_INDEX);
	next.assign(p_capacity, NO_INDEX);
}

void PipelineLRU::_link_mru(uint32_t p_index) {
	prev[p_index] = NO_INDEX;
	next[p_index] = head;
	if (head != NO_INDEX) {
		prev[head] = p_index;
	}
	head = p_index;
	if (tail == NO_INDEX) {
		tail = p_index;
	}
	count++;
}

void PipelineLRU::touch(uint32_t p_index) {
	// Fast path: the entry is already most-recently-used (the common case
	// for repeated acquires of the same pipeline), so there is nothing
	// to relink.
	if (head == p_index) {
		return;
	}
	remove(p_index);
	_link_mru(p_index);
}

void PipelineLRU::remove(uint32_t p_index) {
	if (!contains(p_index)) {
		return;
	}

	const uint32_t p = prev[p_index];
	const uint32_t n = next[p_index];

	if (p != NO_INDEX) {
		next[p] = n;
	} else {
		head = n;
	}
	if (n != NO_INDEX) {
		prev[n] = p;
	} else {
		tail = p;
	}

	prev[p_index] = NO_INDEX;
	next[p_index] = NO_INDEX;
	count--;
}

bool PipelineLRU::contains(uint32_t p_index) const {
	return prev[p_index] != NO_INDEX || next[p_index] != NO_INDEX || head == p_index;
}

uint32_t PipelineLRU::pop_lru() {
	const uint32_t index = tail;
	if (index != NO_INDEX) {
		remove(index);
	}
	return index;
}

} // namespace Kaizen
