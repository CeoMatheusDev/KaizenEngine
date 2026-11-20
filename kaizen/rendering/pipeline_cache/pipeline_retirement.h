/**************************************************************************/
/*  pipeline_retirement.h                                                 */
/**************************************************************************/
/*  Retirement queue for evicted pipeline objects.                        */
/*                                                                        */
/*  An object enters this queue only after its host and recording         */
/*  references reached zero. It becomes destroyable when the backend's    */
/*  completion serial catches up with the last submission that used it.   */
/*                                                                        */
/*  This is deliberately independent from VkPipelineCache: that cache     */
/*  stores driver creation data, not objects with references.             */
/**************************************************************************/

#pragma once

#include <cstdint>
#include <vector>

namespace Kaizen {

class PipelineRetirementQueue {
public:
	struct Entry {
		uint32_t index = UINT32_MAX;
		uint32_t generation = 0;
		uint64_t last_use_serial = 0;
	};

	explicit PipelineRetirementQueue(uint32_t p_capacity);

	void push(uint32_t p_index, uint32_t p_generation, uint64_t p_last_use_serial);

	// Removes and returns every entry with last_use_serial <= p_completed_serial.
	// Entries for other serials are preserved. Returns the number written.
	uint32_t collect_ready(uint64_t p_completed_serial, Entry *r_out, uint32_t p_max);

	void clear();
	uint32_t size() const { return (uint32_t)entries.size(); }
	bool is_empty() const { return entries.empty(); }
	uint64_t get_oldest_serial() const;

private:
	std::vector<Entry> entries;
};

} // namespace Kaizen
