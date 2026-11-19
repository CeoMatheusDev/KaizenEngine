/**************************************************************************/
/*  resource_tracker.cpp                                                   */
/**************************************************************************/

#include "resource_tracker.h"

#include "core/os/os.h"

KaizenResourceTracker *KaizenResourceTracker::singleton = nullptr;

KaizenResourceTracker *KaizenResourceTracker::get_singleton() {
	if (singleton == nullptr) {
		singleton = memnew(KaizenResourceTracker);
	}
	return singleton;
}

KaizenResourceTracker::KaizenResourceTracker() {
}

void KaizenResourceTracker::record_allocation(uint64_t p_rid, const char *p_type, uint64_t p_size) {
	total_allocations.increment();

	KaizenResourceRecord record;
	record.generation_id = generation_counter.increment();
	record.allocation_frame = Engine::get_singleton() ? Engine::get_singleton()->get_frames_drawn() : 0;
	record.size_bytes = p_size;
	record.type_name = p_type;
	record.is_freed = false;

	lock.lock();
	records[p_rid] = record;
	lock.unlock();
}

void KaizenResourceTracker::record_deallocation(uint64_t p_rid) {
	total_deallocations.increment();

	lock.lock();
	HashMap<uint64_t, KaizenResourceRecord>::Iterator E = records.find(p_rid);
	if (E) {
		E->value.is_freed = true;
		E->value.deallocation_frame = Engine::get_singleton() ? Engine::get_singleton()->get_frames_drawn() : 0;
	}
	lock.unlock();
}

bool KaizenResourceTracker::is_valid(uint64_t p_rid) const {
	// Note: this acquires the lock, so it's not safe for the hot path.
	// Use generation IDs for lock-free validation instead.
	return true; // Placeholder
}

uint64_t KaizenResourceTracker::get_allocation_frame(uint64_t p_rid) const {
	lock.lock();
	HashMap<uint64_t, KaizenResourceRecord>::Iterator E = records.find(p_rid);
	uint64_t frame = 0;
	if (E) {
		frame = E->value.allocation_frame;
	}
	lock.unlock();
	return frame;
}

uint64_t KaizenResourceTracker::get_leak_count() const {
	uint64_t leaks = 0;
	lock.lock();
	for (const KeyValue<uint64_t, KaizenResourceRecord> &E : records) {
		if (!E.value.is_freed) {
			leaks++;
		}
	}
	lock.unlock();
	return leaks;
}