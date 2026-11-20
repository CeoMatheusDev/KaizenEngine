/**************************************************************************/
/*  pipeline_submission.h                                                 */
/**************************************************************************/
/*  Maps backend queue submissions to completion serials for P2.          */
/*                                                                        */
/*  The registry needs one completion value per queue. A backend usually  */
/*  has a fence for a group of submissions, not for every single one:     */
/*  Godot's Vulkan driver passes a fence only for the last command buffer */
/*  of a frame, while earlier command buffers are chained with            */
/*  semaphores.                                                           */
/*                                                                        */
/*  This class models that shape:                                         */
/*    - a submission without a fence joins the current open group;        */
/*    - a submission with a fence closes the group, and that fence is the */
/*      completion signal for every submission in it;                     */
/*    - a fence wait confirms exactly the group serial stored in it.      */
/*                                                                        */
/*  It is deliberately transport-agnostic: the driver owns the fences and */
/*  passes only "this submission is/is not fenced" and "this fence for    */
/*  serial X completed".                                                  */
/**************************************************************************/

#pragma once

#include <cstdint>
#include <vector>

namespace Kaizen {

class PipelineSubmissionTracker {
public:
	explicit PipelineSubmissionTracker(uint32_t p_queue_count);

	// Records that a submission was sent to `p_queue`. Returns the serial that
	// identifies the submission for completion purposes. When `p_fenced` is
	// true the submission closes the current group, so the caller must store
	// the returned serial in that submission's fence.
	uint64_t note_submission(uint32_t p_queue, bool p_fenced);

	// Highest serial ever submitted on `p_queue`, or 0 if nothing was
	// submitted. This is the conservative last-use serial for an object the
	// backend destroys at this moment.
	uint64_t last_serial(uint32_t p_queue) const;

	// Records that every submission up to `p_serial` on `p_queue` completed.
	// Monotonic: an older value never moves the completion point back.
	void confirm(uint32_t p_queue, uint64_t p_serial);

	// Treats everything submitted so far as completed. Only valid after the
	// backend waited for the device to be idle.
	void confirm_all();

	uint32_t get_queue_count() const { return (uint32_t)state.size(); }

	// One entry per queue, in the same order used by note_submission().
	const uint64_t *get_confirmed() const { return confirmed.data(); }

private:
	struct QueueState {
		uint64_t next_serial = 0; // Last handed-out group serial.
		uint64_t open_serial = 0; // Non-zero while a group is open.
		uint64_t last_serial = 0; // Most recent submission on this queue.
	};

	std::vector<QueueState> state;
	std::vector<uint64_t> confirmed;
};

} // namespace Kaizen
