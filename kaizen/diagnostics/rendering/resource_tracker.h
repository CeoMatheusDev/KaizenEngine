/**************************************************************************/
/*  resource_tracker.h                                                     */
/**************************************************************************/
/*  Kaizen Engine — Resource Lifetime Diagnostics                          */
/*  Low-overhead allocation tracking with atomic counters, generation IDs, */
/*  and memory accounting. Safe for the render thread hot path when        */
/*  diagnostics are disabled.                                             */
/**************************************************************************/

#pragma once

#include <atomic>
#include "core/os/spin_lock.h"
#include "core/templates/hash_map.h"
#include "core/templates/local_vector.h"

// Configurable diagnostics mode.
enum class KaizenDiagnosticsMode : uint8_t {
	Disabled,   // No instrumentation (zero overhead)
	Basic,      // Counters and statistics only
	Detailed,   // Allocation/deallocation events
	Validation, // Lifetime validation + handle checking
};

// Handle with generation ID to detect stale references.
struct KaizenResourceHandle {
	uint32_t index = 0;
	uint32_t generation = 0;

	bool operator==(const KaizenResourceHandle &p_other) const {
		return index == p_other.index && generation == p_other.generation;
	}
	bool is_null() const { return generation == 0; }
};

// Memory and allocation statistics.
struct KaizenResourceStats {
	std::atomic<uint64_t> allocations = 0;
	std::atomic<uint64_t> deallocations = 0;
	std::atomic<uint64_t> live_resources = 0;
	std::atomic<uint64_t> allocated_bytes = 0;
	std::atomic<uint64_t> peak_bytes = 0;
	std::atomic<uint64_t> invalid_handles = 0;
	std::atomic<uint64_t> generation_mismatches = 0;
	std::atomic<uint64_t> double_frees = 0;
};

// Single resource record.
struct KaizenResourceRecord {
	uint32_t generation = 0;
	uint64_t allocation_frame = 0;
	uint64_t deallocation_frame = 0;
	uint64_t size_bytes = 0;
	const char *type_name = nullptr;
	bool is_freed = false;
	bool is_cpu = true; // CPU vs GPU resource
};

// Registry with reserved capacity and free-list (no dynamic allocation on hot path).
class KaizenResourceRegistry {
public:
	KaizenResourceRegistry(uint32_t p_capacity = 4096);
	~KaizenResourceRegistry();

	KaizenResourceHandle allocate(const char *p_type, uint64_t p_size, bool p_is_cpu);
	void deallocate(KaizenResourceHandle p_handle);
	bool is_valid(KaizenResourceHandle p_handle) const;
	uint64_t get_allocation_frame(KaizenResourceHandle p_handle) const;

	uint32_t get_capacity() const { return capacity; }
	uint32_t get_count() const { return count; }

private:
	uint32_t capacity = 0;
	uint32_t count = 0;
	LocalVector<KaizenResourceRecord> records;
	LocalVector<uint32_t> free_list;
	uint32_t next_index = 0;
	SpinLock lock;
};

// Main resource tracker.
class KaizenResourceTracker {
public:
	static KaizenResourceTracker *get_singleton();
	static void set_diagnostics_mode(KaizenDiagnosticsMode p_mode);
	static KaizenDiagnosticsMode get_diagnostics_mode();

	void record_allocation(uint64_t p_rid, const char *p_type, uint64_t p_size, bool p_is_cpu = true);
	void record_deallocation(uint64_t p_rid);
	bool validate_handle(KaizenResourceHandle p_handle);

	// Statistics
	const KaizenResourceStats &get_stats() const { return stats; }
	uint64_t get_leak_count() const;
	void reset_stats();

private:
	KaizenResourceTracker();
	static KaizenResourceTracker *singleton;
	static KaizenDiagnosticsMode diagnostics_mode;

	KaizenResourceStats stats;
	KaizenResourceRegistry cpu_registry;
	KaizenResourceRegistry gpu_registry;

	// RID -> handle mapping
	HashMap<uint64_t, KaizenResourceHandle> rid_to_handle;
	SpinLock rid_lock;
};