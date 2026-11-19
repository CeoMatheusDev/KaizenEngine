/**************************************************************************/
/*  resource_tracker.h                                                     */
/**************************************************************************/
/*  Kaizen Engine — Resource Lifetime Diagnostics                          */
/*  Low-overhead allocation tracking with atomic counters and generation   */
/*  IDs. Designed to be safe for the render thread hot path.               */
/**************************************************************************/

#pragma once

#include "core/os/atomic.h"
#include "core/templates/hash_map.h"
#include "core/templates/hash_set.h"

// Tracks a single resource allocation.
struct KaizenResourceRecord {
	uint64_t generation_id = 0;
	uint64_t allocation_frame = 0;
	uint64_t deallocation_frame = 0;
	uint64_t size_bytes = 0;
	const char *type_name = nullptr;
	bool is_freed = false;
};

// Low-overhead resource tracker for the rendering device.
// Uses atomic counters and generation IDs to avoid allocations on the hot path.
class KaizenResourceTracker {
public:
	static KaizenResourceTracker *get_singleton();

	void record_allocation(uint64_t p_rid, const char *p_type, uint64_t p_size);
	void record_deallocation(uint64_t p_rid);
	bool is_valid(uint64_t p_rid) const;
	uint64_t get_allocation_frame(uint64_t p_rid) const;

	// Diagnostics
	uint64_t get_total_allocations() const { return total_allocations.load(); }
	uint64_t get_total_deallocations() const { return total_deallocations.load(); }
	uint64_t get_current_count() const { return total_allocations.load() - total_deallocations.load(); }
	uint64_t get_leak_count() const;

private:
	KaizenResourceTracker();
	static KaizenResourceTracker *singleton;

	Atomic<uint64_t> total_allocations = 0;
	Atomic<uint64_t> total_deallocations = 0;
	Atomic<uint64_t> generation_counter = 0;

	HashMap<uint64_t, KaizenResourceRecord> records;
	SpinLock lock;
};
