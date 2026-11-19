/**************************************************************************/
/*  resource_tracker.cpp                                                   */
/**************************************************************************/

#include "resource_tracker.h"

#include "core/os/os.h"

KaizenResourceTracker *KaizenResourceTracker::singleton = nullptr;
KaizenDiagnosticsMode KaizenResourceTracker::diagnostics_mode = KaizenDiagnosticsMode::Disabled;

KaizenResourceTracker *KaizenResourceTracker::get_singleton() {
	if (singleton == nullptr) {
		singleton = memnew(KaizenResourceTracker);
	}
	return singleton;
}

void KaizenResourceTracker::set_diagnostics_mode(KaizenDiagnosticsMode p_mode) {
	diagnostics_mode = p_mode;
}

KaizenDiagnosticsMode KaizenResourceTracker::get_diagnostics_mode() {
	return diagnostics_mode;
}

KaizenResourceTracker::KaizenResourceTracker() :
		cpu_registry(4096),
		gpu_registry(4096) {
}

KaizenResourceRegistry::KaizenResourceRegistry(uint32_t p_capacity) {
	capacity = p_capacity;
	records.resize(p_capacity);
	free_list.reserve(p_capacity);
}

KaizenResourceRegistry::~KaizenResourceRegistry() {
}

KaizenResourceHandle KaizenResourceRegistry::allocate(const char *p_type, uint64_t p_size, bool p_is_cpu) {
	KaizenResourceHandle handle;

	lock.lock();

	if (!free_list.is_empty()) {
		handle.index = free_list.back();
		free_list.pop_back();
	} else if (next_index < capacity) {
		handle.index = next_index++;
	} else {
		// Registry full - should not happen if capacity is sufficient.
		lock.unlock();
		return handle; // null handle
	}

	KaizenResourceRecord &record = records[handle.index];
	record.generation++;
	handle.generation = record.generation;
	record.allocation_frame = Engine::get_singleton() ? Engine::get_singleton()->get_frames_drawn() : 0;
	record.deallocation_frame = 0;
	record.size_bytes = p_size;
	record.type_name = p_type;
	record.is_freed = false;
	record.is_cpu = p_is_cpu;

	count++;
	lock.unlock();

	return handle;
}

void KaizenResourceRegistry::deallocate(KaizenResourceHandle p_handle) {
	lock.lock();

	if (p_handle.index >= capacity) {
		lock.unlock();
		return;
	}

	KaizenResourceRecord &record = records[p_handle.index];

	// Generation mismatch: stale handle.
	if (record.generation != p_handle.generation) {
		lock.unlock();
		return;
	}

	// Double free detection.
	if (record.is_freed) {
		lock.unlock();
		return;
	}

	record.is_freed = true;
	record.deallocation_frame = Engine::get_singleton() ? Engine::get_singleton()->get_frames_drawn() : 0;

	free_list.push_back(p_handle.index);
	count--;

	lock.unlock();
}

bool KaizenResourceRegistry::is_valid(KaizenResourceHandle p_handle) const {
	if (p_handle.is_null()) {
		return false;
	}

	// Lock-free read for validation (generation check).
	if (p_handle.index >= capacity) {
		return false;
	}

	const KaizenResourceRecord &record = records[p_handle.index];
	return record.generation == p_handle.generation && !record.is_freed;
}

uint64_t KaizenResourceRegistry::get_allocation_frame(KaizenResourceHandle p_handle) const {
	if (p_handle.index >= capacity) {
		return 0;
	}

	const KaizenResourceRecord &record = records[p_handle.index];
	if (record.generation != p_handle.generation) {
		return 0;
	}

	return record.allocation_frame;
}

void KaizenResourceTracker::record_allocation(uint64_t p_rid, const char *p_type, uint64_t p_size, bool p_is_cpu) {
	if (diagnostics_mode == KaizenDiagnosticsMode::Disabled) {
		return;
	}

	stats.allocations.fetch_add(1, std::memory_order_relaxed);
	stats.live_resources.fetch_add(1, std::memory_order_relaxed);
	stats.allocated_bytes.fetch_add(p_size, std::memory_order_relaxed);

	// Update peak bytes.
	uint64_t current = stats.allocated_bytes.load(std::memory_order_relaxed);
	uint64_t peak = stats.peak_bytes.load(std::memory_order_relaxed);
	if (current > peak) {
		stats.peak_bytes.store(current, std::memory_order_relaxed);
	}

	KaizenResourceRegistry &registry = p_is_cpu ? cpu_registry : gpu_registry;
	KaizenResourceHandle handle = registry.allocate(p_type, p_size, p_is_cpu);

	rid_lock.lock();
	rid_to_handle[p_rid] = handle;
	rid_lock.unlock();
}

void KaizenResourceTracker::record_deallocation(uint64_t p_rid) {
	if (diagnostics_mode == KaizenDiagnosticsMode::Disabled) {
		return;
	}

	stats.deallocations.fetch_add(1, std::memory_order_relaxed);
	stats.live_resources.fetch_sub(1, std::memory_order_relaxed);

	rid_lock.lock();
	HashMap<uint64_t, KaizenResourceHandle>::Iterator E = rid_to_handle.find(p_rid);
	if (!E) {
		// Invalid handle: resource not tracked.
		stats.invalid_handles.fetch_add(1, std::memory_order_relaxed);
		rid_lock.unlock();
		return;
	}

	KaizenResourceHandle handle = E->value;
	rid_to_handle.remove(E);

	// Determine CPU vs GPU from the handle's record.
	// Try GPU first (more common in rendering).
	KaizenResourceRegistry &gpu_reg = gpu_registry;
	KaizenResourceRegistry &cpu_reg = cpu_registry;

	bool found = false;
	if (gpu_reg.is_valid(handle)) {
		
		gpu_reg.deallocate(handle);
		found = true;
	} else if (cpu_reg.is_valid(handle)) {
		cpu_reg.deallocate(handle);
		found = true;
	}

	if (!found) {
		// Generation mismatch: stale handle.
		stats.generation_mismatches.fetch_add(1, std::memory_order_relaxed);
	}

	rid_lock.unlock();
}

bool KaizenResourceTracker::validate_handle(KaizenResourceHandle p_handle) {
	if (diagnostics_mode == KaizenDiagnosticsMode::Validation) {
		if (p_handle.is_null()) {
			stats.invalid_handles.fetch_add(1, std::memory_order_relaxed);
			return false;
		}
		// Check both registries.
		if (gpu_registry.is_valid(p_handle) || cpu_registry.is_valid(p_handle)) {
			return true;
		}
		stats.invalid_handles.fetch_add(1, std::memory_order_relaxed);
		return false;
	}
	return true;
}

uint64_t KaizenResourceTracker::get_leak_count() const {
	return stats.live_resources.load(std::memory_order_relaxed);
}

void KaizenResourceTracker::reset_stats() {
	stats.allocations.store(0, std::memory_order_relaxed);
	stats.deallocations.store(0, std::memory_order_relaxed);
	stats.live_resources.store(0, std::memory_order_relaxed);
	stats.allocated_bytes.store(0, std::memory_order_relaxed);
	stats.peak_bytes.store(0, std::memory_order_relaxed);
	stats.invalid_handles.store(0, std::memory_order_relaxed);
	stats.generation_mismatches.store(0, std::memory_order_relaxed);
	stats.double_frees.store(0, std::memory_order_relaxed);
}