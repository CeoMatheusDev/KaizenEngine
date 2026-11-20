/**************************************************************************/
/*  pipeline_types.h                                                      */
/**************************************************************************/
/*  Queue identities and per-queue submission serials for P2.             */
/*                                                                        */
/*  A pipeline can be used by more than one queue. Completion is not a    */
/*  single global number: the graphics queue may finish serial 100 while  */
/*  the compute queue is still at 99. Destruction requires every queue    */
/*  that used the object to report a completed serial >= its last use.    */
/**************************************************************************/

#pragma once

#include <cstdint>

namespace Kaizen {

enum class PipelineQueue : uint8_t {
	Graphics = 0,
	Compute = 1,
	Transfer = 2,
};

constexpr uint32_t PIPELINE_QUEUE_COUNT = 3;

inline uint32_t pipeline_queue_index(PipelineQueue p_queue) {
	return static_cast<uint32_t>(p_queue);
}

// Monotonic last-use serial per queue. The mask records which queues have
// ever submitted work that used the object; unused queues never gate
// destruction.
struct PipelineQueueUsage {
	uint8_t mask = 0;
	uint64_t serials[PIPELINE_QUEUE_COUNT] = {};

	bool is_used(PipelineQueue p_queue) const {
		return (mask & (1u << pipeline_queue_index(p_queue))) != 0;
	}

	uint64_t get(PipelineQueue p_queue) const {
		return serials[pipeline_queue_index(p_queue)];
	}

	void record(PipelineQueue p_queue, uint64_t p_serial) {
		const uint32_t index = pipeline_queue_index(p_queue);
		mask |= static_cast<uint8_t>(1u << index);
		if (p_serial > serials[index]) {
			serials[index] = p_serial;
		}
	}

	uint64_t max_serial() const {
		uint64_t maximum = 0;
		for (uint32_t i = 0; i < PIPELINE_QUEUE_COUNT; i++) {
			if (serials[i] > maximum) {
				maximum = serials[i];
			}
		}
		return maximum;
	}

	// True when every queue that used the object has completed at least the
	// serial recorded for that queue.
	bool is_complete(const uint64_t p_completed[PIPELINE_QUEUE_COUNT]) const {
		for (uint32_t i = 0; i < PIPELINE_QUEUE_COUNT; i++) {
			if ((mask & (1u << i)) != 0 && p_completed[i] < serials[i]) {
				return false;
			}
		}
		return true;
	}
};

} // namespace Kaizen
