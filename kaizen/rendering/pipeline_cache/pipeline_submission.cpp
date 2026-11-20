/**************************************************************************/
/*  pipeline_submission.cpp                                               */
/**************************************************************************/

#include "pipeline_submission.h"

namespace Kaizen {

PipelineSubmissionTracker::PipelineSubmissionTracker(uint32_t p_queue_count) :
		state(p_queue_count),
		confirmed(p_queue_count, 0) {
}

uint64_t PipelineSubmissionTracker::note_submission(uint32_t p_queue, bool p_fenced) {
	if (p_queue >= state.size()) {
		return 0;
	}

	QueueState &queue = state[p_queue];
	if (queue.open_serial == 0) {
		queue.open_serial = ++queue.next_serial;
	}

	const uint64_t serial = queue.open_serial;
	queue.last_serial = serial;

	if (p_fenced) {
		// The next submission starts a new group; the fence of this one
		// confirms everything that joined the group so far.
		queue.open_serial = 0;
	}

	return serial;
}

uint64_t PipelineSubmissionTracker::last_serial(uint32_t p_queue) const {
	if (p_queue >= state.size()) {
		return 0;
	}
	return state[p_queue].last_serial;
}

void PipelineSubmissionTracker::confirm(uint32_t p_queue, uint64_t p_serial) {
	if (p_queue >= confirmed.size() || p_serial == 0) {
		return;
	}
	if (p_serial > confirmed[p_queue]) {
		confirmed[p_queue] = p_serial;
	}
}

void PipelineSubmissionTracker::confirm_all() {
	for (uint32_t i = 0; i < state.size(); i++) {
		confirm(i, state[i].last_serial);
	}
}

} // namespace Kaizen
