/**************************************************************************/
/*  pipeline_retirement.cpp                                               */
/**************************************************************************/

#include "pipeline_retirement.h"

namespace Kaizen {

PipelineRetirementQueue::PipelineRetirementQueue(uint32_t p_capacity) {
	entries.reserve(p_capacity);
}

void PipelineRetirementQueue::push(uint32_t p_index, uint32_t p_generation, uint64_t p_last_use_serial) {
	Entry entry;
	entry.index = p_index;
	entry.generation = p_generation;
	entry.last_use_serial = p_last_use_serial;
	entries.push_back(entry);
}

uint32_t PipelineRetirementQueue::collect_ready(uint64_t p_completed_serial, Entry *r_out, uint32_t p_max) {
	uint32_t written = 0;
	uint32_t write = 0;

	for (uint32_t read = 0; read < entries.size(); read++) {
		const Entry &entry = entries[read];
		if (entry.last_use_serial <= p_completed_serial && written < p_max) {
			r_out[written++] = entry;
			continue;
		}

		if (write != read) {
			entries[write] = entry;
		}
		write++;
	}

	entries.resize(write);
	return written;
}

void PipelineRetirementQueue::clear() {
	entries.clear();
}

uint64_t PipelineRetirementQueue::get_oldest_serial() const {
	uint64_t oldest = UINT64_MAX;
	for (const Entry &entry : entries) {
		if (entry.last_use_serial < oldest) {
			oldest = entry.last_use_serial;
		}
	}
	return oldest;
}

} // namespace Kaizen
