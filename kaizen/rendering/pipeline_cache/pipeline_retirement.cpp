/**************************************************************************/
/*  pipeline_retirement.cpp                                               */
/**************************************************************************/

#include "pipeline_retirement.h"

namespace Kaizen {

PipelineRetirementQueue::PipelineRetirementQueue(uint32_t p_capacity) {
	entries.reserve(p_capacity);
}

void PipelineRetirementQueue::push(uint32_t p_index, uint32_t p_generation, const PipelineQueueUsage &p_usage) {
	Entry entry;
	entry.index = p_index;
	entry.generation = p_generation;
	entry.usage = p_usage;
	entries.push_back(entry);
}

uint32_t PipelineRetirementQueue::collect_ready(const uint64_t p_completed[PIPELINE_QUEUE_COUNT], Entry *r_out, uint32_t p_max) {
	uint32_t written = 0;
	uint32_t write = 0;

	for (uint32_t read = 0; read < entries.size(); read++) {
		const Entry &entry = entries[read];
		if (entry.usage.is_complete(p_completed) && written < p_max) {
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

} // namespace Kaizen
