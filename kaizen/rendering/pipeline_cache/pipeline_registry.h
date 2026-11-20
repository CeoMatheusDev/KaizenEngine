/**************************************************************************/
/*  pipeline_registry.h                                                   */
/**************************************************************************/
/*  Pipeline object lifetime and eviction core (P2).                      */
/*                                                                        */
/*  This subsystem owns live pipeline objects, their reference counts and */
/*  their safe-destruction bookkeeping. It is independent from the        */
/*  resource tracker (which observes buffers/textures) and from            */
/*  VkPipelineCache (which stores driver-side creation data).             */
/*                                                                        */
/*  States:                                                               */
/*    Free               slot never used or already destroyed             */
/*    Resident           usable; may be acquired again                    */
/*    EvictionCandidate  selected by LRU; no new acquisitions             */
/*    Retiring           no host/recording refs; waiting for completion   */
/*    Destroyable        completion reached; backend may destroy          */
/*                                                                        */
/*  Destruction precondition (enforced by this class):                    */
/*    host_refs == 0 && recording_refs == 0                               */
/*    && for every queue that used the object:                            */
/*         completed_serial[queue] >= last_use_serial[queue]              */
/*                                                                        */
/*  Completion values are supplied by the backend, one per queue. With    */
/*  Vulkan they must come from a per-queue timeline semaphore or from a   */
/*  fence wait associated with the exact submission that used the object. */
/*  A single global submission counter is not sufficient: work on the     */
/*  graphics queue completing does not imply that compute has finished.   */
/**************************************************************************/

#pragma once

#include "pipeline_lru.h"
#include "pipeline_retirement.h"
#include "pipeline_types.h"

#include <atomic>
#include <cstdint>
#include <unordered_map>
#include <vector>

namespace Kaizen {

enum class PipelineState : uint8_t {
	Free = 0,
	Resident,
	EvictionCandidate,
	Retiring,
	Destroyable,
};

struct PipelineHandle {
	uint32_t index = UINT32_MAX;
	uint32_t generation = 0;

	bool is_null() const { return index == UINT32_MAX || generation == 0; }
	bool operator==(const PipelineHandle &p_other) const {
		return index == p_other.index && generation == p_other.generation;
	}
	bool operator!=(const PipelineHandle &p_other) const { return !(*this == p_other); }
};

struct PipelineInfo {
	uint64_t key = 0;
	uint64_t bytes = 0;
	uint64_t last_use_serial = 0; // Maximum across all queues, for reporting.
	PipelineQueueUsage usage;
	uint32_t generation = 0;
	uint32_t host_refs = 0;
	uint32_t recording_refs = 0;
	PipelineState state = PipelineState::Free;
};

struct PipelineStats {
	uint64_t acquisitions = 0; // Successful acquire() calls (new or cached).
	uint64_t hits = 0; // acquire() on a Resident entry.
	uint64_t misses = 0; // Object created.
	uint64_t releases = 0;

	uint64_t recording_starts = 0;
	uint64_t aborted_recordings = 0;
	uint64_t eviction_candidates = 0;
	uint64_t requested_retirements = 0; // retire_now() calls that moved an object.
	uint64_t retired = 0;
	uint64_t destroyable = 0;
	uint64_t destroyed = 0;

	uint64_t blocked_by_retirement = 0; // acquire() while key is not Resident.
	uint64_t rejected_full = 0;
	uint64_t late_use = 0; // mark_use() on a retired/destroyable handle.
	uint64_t invalid_serials = 0; // end_use/mark_use with serial 0.

	uint64_t resident_count = 0;
	uint64_t candidate_count = 0;
	uint64_t retiring_count = 0;
	uint64_t destroyable_count = 0;

	uint64_t resident_bytes = 0;
	uint64_t pending_bytes = 0; // Candidate + Retiring + Destroyable.
	uint64_t peak_resident_bytes = 0;
	uint64_t completed_serials[PIPELINE_QUEUE_COUNT] = {};
};

class PipelineRegistry {
public:
	// `p_initial_generation` exists so tests can validate wrap behaviour
	// without billions of operations; production uses the default.
	PipelineRegistry(uint32_t p_capacity, uint64_t p_budget_bytes, uint32_t p_initial_generation = 1);
	~PipelineRegistry() = default;

	PipelineRegistry(const PipelineRegistry &) = delete;
	PipelineRegistry &operator=(const PipelineRegistry &) = delete;

	// Returns an existing Resident object (refcount++) or creates a new one.
	// Returns a null handle while the key is retiring or when capacity is full.
	PipelineHandle acquire(uint64_t p_key, uint64_t p_bytes);

	// Drops a host reference. A Candidate that reaches zero refs is retired.
	bool release(PipelineHandle p_handle);

	// Command-buffer recording references. A pipeline cannot be recorded after
	// it became a candidate.
	bool begin_use(PipelineHandle p_handle);

	// Successful submission: drops the recording reference and records that
	// the object was used by `p_queue` at `p_submission_serial` (which must be
	// non-zero and monotonic for that queue).
	bool end_use(PipelineHandle p_handle, PipelineQueue p_queue, uint64_t p_submission_serial);

	// Failed submission: drops the recording reference without recording a
	// use, because no GPU work was executed. The object remains alive if it is
	// a candidate; a later eviction retires it normally.
	bool abort_use(PipelineHandle p_handle);

	// Records a submission that uses the pipeline without a recording scope.
	bool mark_use(PipelineHandle p_handle, PipelineQueue p_queue, uint64_t p_submission_serial);

	bool describe(PipelineHandle p_handle, PipelineInfo &r_info) const;
	bool is_valid(PipelineHandle p_handle) const;

	// Monotonic backend progress for one queue. Retiring entries whose usage
	// is complete become Destroyable. Returns how many transitioned.
	uint32_t set_completed_serial(PipelineQueue p_queue, uint64_t p_completed_serial);

	// Copies handles in Destroyable state. The backend destroys the Vulkan
	// object and then calls destroy() to release the slot.
	uint32_t collect_destroyable(PipelineHandle *r_out, uint32_t p_max) const;

	bool destroy(PipelineHandle p_handle);

	// Marks least-recently-used Resident pipelines as candidates/retiring until
	// the resident byte budget is met. Objects with active references become
	// candidates and are retired when their references drop.
	uint32_t enforce_budget();

	// The backend owns the object and wants it destroyed as soon as it is safe.
	// Unlike an LRU eviction this is not driven by the byte budget, and the
	// object is never re-acquired. The handle must have no host or recording
	// references (call release()/abort_use() first). Usage recorded through
	// mark_use()/end_use() still gates destruction.
	bool retire_now(PipelineHandle p_handle);

	void set_budget(uint64_t p_budget_bytes);
	uint64_t get_budget() const;

	PipelineStats get_stats() const;
	uint32_t get_capacity() const { return capacity; }

	// sizeof(Slot) in bytes, for memory estimates in diagnostics and reports.
	static uint32_t get_slot_size();

private:
	class SpinLock {
	public:
		void lock() {
			while (flag.test_and_set(std::memory_order_acquire)) {
			}
		}
		void unlock() { flag.clear(std::memory_order_release); }

	private:
		std::atomic_flag flag = ATOMIC_FLAG_INIT;
	};

	struct Slot {
		uint64_t key = 0;
		uint64_t bytes = 0;
		PipelineQueueUsage usage;
		uint32_t generation = 0;
		uint32_t host_refs = 0;
		uint32_t recording_refs = 0;
		PipelineState state = PipelineState::Free;
	};

	PipelineHandle _make_handle_locked(uint32_t p_index) const;
	bool _resolve_locked(PipelineHandle p_handle, Slot *&r_slot) const;
	bool _is_ready_locked(const Slot &p_slot) const;
	void _mark_candidate_locked(uint32_t p_index);
	void _retire_locked(uint32_t p_index);
	void _move_to_pending_locked(Slot &r_slot);
	void _enforce_budget_locked();

	mutable SpinLock lock;
	uint32_t capacity = 0;
	uint64_t budget_bytes = 0;

	uint64_t completed_serials[PIPELINE_QUEUE_COUNT] = {};
	uint64_t resident_bytes = 0;
	uint64_t pending_bytes = 0;
	uint64_t peak_resident_bytes = 0;

	uint64_t acquisitions = 0;
	uint64_t hits = 0;
	uint64_t misses = 0;
	uint64_t releases = 0;
	uint64_t recording_starts = 0;
	uint64_t aborted_recordings = 0;
	uint64_t eviction_candidates = 0;
	uint64_t requested_retirements = 0; // retire_now() calls that moved an object.
	uint64_t retired = 0;
	uint64_t destroyable = 0;
	uint64_t destroyed = 0;
	uint64_t blocked_by_retirement = 0;
	uint64_t rejected_full = 0;
	uint64_t late_use = 0;
	uint64_t invalid_serials = 0;

	std::vector<Slot> slots;
	std::vector<uint32_t> free_indices;
	std::unordered_map<uint64_t, uint32_t> key_to_index;

	PipelineLRU lru;
	PipelineRetirementQueue retirement;
	std::vector<PipelineRetirementQueue::Entry> ready_buffer;
};

} // namespace Kaizen
