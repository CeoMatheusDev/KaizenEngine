/**************************************************************************/
/*  pipeline_retirement.h                                                 */
/**************************************************************************/
/*  Retirement queue for evicted pipeline objects.                        */
/*                                                                        */
/*  An object enters this queue only after its host and recording         */
/*  references reached zero. It becomes destroyable when every queue that */
/*  used it has completed the corresponding submission serial.            */
/*                                                                        */
/*  This is deliberately independent from VkPipelineCache: that cache     */
/*  stores driver creation data, not objects with references.             */
/**************************************************************************/

#pragma once

#include "pipeline_types.h"

#include <cstdint>
#include <vector>

namespace Kaizen {

class PipelineRetirementQueue {
public:
	struct Entry {
		uint32_t index = UINT32_MAX;
		uint32_t generation = 0;
		PipelineQueueUsage usage;
	};

	explicit PipelineRetirementQueue(uint32_t p_capacity);

	void push(uint32_t p_index, uint32_t p_generation, const PipelineQueueUsage &p_usage);

	// Removes and returns every entry whose usage is complete at
	// `p_completed` (one value per queue). Entries that are not ready, and
	// ready entries that do not fit in `p_max`, are preserved.
	uint32_t collect_ready(const uint64_t p_completed[PIPELINE_QUEUE_COUNT], Entry *r_out, uint32_t p_max);

	void clear();
	uint32_t size() const { return (uint32_t)entries.size(); }
	bool is_empty() const { return entries.empty(); }

private:
	std::vector<Entry> entries;
};

} // namespace Kaizen
