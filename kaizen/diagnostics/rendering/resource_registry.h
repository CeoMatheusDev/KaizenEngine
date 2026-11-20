/**************************************************************************/
/*  resource_registry.h                                                   */
/**************************************************************************/
/*  Kaizen Engine — Resource Lifetime Diagnostics                          */
/*                                                                        */
/*  Self-contained resource registry with:                                */
/*  - generation-tagged handles (index + generation + kind),              */
/*  - fixed-capacity storage and a free list (no allocation on use),      */
/*  - four-state lifetime (Free, Alive, PendingRelease, Released),        */
/*  - logical (requested) and backend (allocated) byte accounting,        */
/*  - deferred release scheduled by frame epoch.                          */
/*                                                                        */
/*  This file intentionally depends only on the C++ standard library so   */
/*  the registry can be unit-tested outside the engine. The engine-facing */
/*  wrapper lives in resource_tracker.h.                                  */
/**************************************************************************/

#pragma once

#include <atomic>
#include <cstdint>
#include <vector>

namespace Kaizen {

// Instrumentation level. Disabled is checked once in the engine wrapper,
// so no registry work happens when diagnostics are off.
enum class DiagnosticsMode : uint8_t {
	Disabled,
	Basic, // Counters and byte accounting only.
	Detailed, // Adds a bounded ring of lifetime events.
	Validation, // Adds strict handle checks and reports on misuse.
};

// Resources tracked by separate registries so CPU and GPU accounting never mix.
enum class ResourceKind : uint8_t {
	CPU = 0,
	GPU = 1,
};

// Lifetime states. A slot only returns to the pool after the backend has
// confirmed that the resource is no longer in use by the GPU.
enum class ResourceState : uint8_t {
	Free = 0, // Never used.
	Alive,
	PendingRelease, // Logical free requested, backend release still pending.
	Released, // Backend confirmed; slot is reusable.
};

// Stable handle. `generation` changes every time the slot is reused, which
// makes stale handles detectable without touching freed memory.
struct ResourceHandle {
	uint32_t index = UINT32_MAX;
	uint32_t generation = 0;
	ResourceKind kind = ResourceKind::CPU;

	bool is_null() const { return index == UINT32_MAX || generation == 0; }
	bool operator==(const ResourceHandle &p_other) const {
		return index == p_other.index && generation == p_other.generation && kind == p_other.kind;
	}
	bool operator!=(const ResourceHandle &p_other) const { return !(*this == p_other); }
};

struct ResourceRecord {
	uint32_t generation = 0;
	ResourceKind kind = ResourceKind::CPU;
	ResourceState state = ResourceState::Free;
	const char *type_name = nullptr;
	uint64_t requested_bytes = 0; // Bytes asked for by RenderingDevice.
	uint64_t backend_bytes = 0; // Bytes actually allocated by the driver.
	uint64_t allocation_frame = 0;
	uint64_t release_epoch = 0;
	uint64_t release_frame = 0;
};

// Snapshot of the registry counters. Composed under the registry lock so the
// numbers are mutually consistent (no torn reads across fields).
struct ResourceStats {
	uint64_t allocations = 0;
	uint64_t releases = 0;
	uint64_t live_resources = 0; // Alive + PendingRelease.
	uint64_t pending_releases = 0;

	uint64_t alive_requested_bytes = 0;
	uint64_t pending_requested_bytes = 0;
	uint64_t alive_backend_bytes = 0;
	uint64_t pending_backend_bytes = 0;
	uint64_t released_requested_bytes = 0; // Cumulative, for throughput checks.
	uint64_t released_backend_bytes = 0;
	uint64_t peak_live_requested_bytes = 0;
	uint64_t peak_live_backend_bytes = 0;

	uint64_t invalid_handles = 0;
	uint64_t generation_mismatches = 0;
	uint64_t double_frees = 0;
	uint64_t registry_full = 0;
	uint64_t backend_bytes_overrides = 0;

	uint64_t get_live_requested_bytes() const { return alive_requested_bytes + pending_requested_bytes; }
	uint64_t get_live_backend_bytes() const { return alive_backend_bytes + pending_backend_bytes; }
};

// Bounded event log used by Detailed and Validation modes. The buffer is
// preallocated; recording an event never allocates.
struct ResourceEvent {
	enum class Type : uint8_t {
		Allocated,
		ReleaseRequested,
		Released,
		InvalidHandle,
		StaleHandle,
		DoubleFree,
		RegistryFull,
	};

	Type type = Type::Allocated;
	uint32_t index = 0;
	uint32_t generation = 0;
	ResourceKind kind = ResourceKind::CPU;
	uint64_t frame = 0;
	uint64_t bytes = 0;
	const char *type_name = nullptr;
};

class ResourceRegistry {
public:
	// `p_capacity` is the maximum number of simultaneously live resources.
	// Memory is reserved up front; the registry never grows at runtime.
	// `p_initial_generation` exists so tests can validate generation wrap
	// behaviour without billions of operations; production uses the default.
	explicit ResourceRegistry(ResourceKind p_kind, uint32_t p_capacity = 4096, uint32_t p_initial_generation = 1);
	~ResourceRegistry() = default;

	ResourceRegistry(const ResourceRegistry &) = delete;
	ResourceRegistry &operator=(const ResourceRegistry &) = delete;

	ResourceHandle allocate(const char *p_type, uint64_t p_requested_bytes, uint64_t p_frame);

	// Refines the backend byte count once the driver reports the real
	// allocation size. Returns false for a stale handle.
	bool set_backend_bytes(ResourceHandle p_handle, uint64_t p_backend_bytes);

	// Moves Alive -> PendingRelease. The resource stays accounted for until
	// confirm_release()/confirm_releases_until() runs after the GPU is done.
	// Handle checks always run; the diagnostics mode only controls reporting.
	bool begin_release(ResourceHandle p_handle, uint64_t p_release_epoch);

	// PendingRelease -> Released and returns the slot to the free list.
	bool confirm_release(ResourceHandle p_handle);

	// Confirms every pending release whose epoch is <= `p_epoch`.
	// Returns the number of resources released.
	uint32_t confirm_releases_until(uint64_t p_epoch);

	bool is_valid(ResourceHandle p_handle) const;
	bool describe(ResourceHandle p_handle, ResourceRecord &r_record) const;

	ResourceStats get_stats() const;

	// Copies the last `p_max` events, newest last. Returns how many were copied.
	uint32_t get_events(ResourceEvent *r_events, uint32_t p_max) const;

	static constexpr uint32_t EVENT_CAPACITY = 1024;

	ResourceKind get_kind() const { return kind; }
	uint32_t get_capacity() const { return capacity; }

private:
	bool _resolve_locked(ResourceHandle p_handle, ResourceState *r_state) const;
	void _push_event_locked(ResourceEvent::Type p_type, uint32_t p_index, uint64_t p_frame);
	void _release_locked(uint32_t p_index);

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

	struct PendingEntry {
		uint32_t index = 0;
		uint32_t generation = 0;
		uint64_t epoch = 0;
	};

	mutable SpinLock lock;
	ResourceKind kind;
	uint32_t capacity = 0;
	uint32_t next_index = 0;
	uint32_t live_count = 0;
	uint32_t pending_count = 0;

	ResourceStats stats;

	std::vector<ResourceRecord> records;
	std::vector<uint32_t> free_indices;
	std::vector<PendingEntry> pending;

	// Event ring (Detailed/Validation only). The buffer is allocated only when
	// the log is enabled, so Disabled/Basic modes pay no memory for it.
	std::vector<ResourceEvent> events;
	uint32_t event_head = 0;
	uint32_t event_count = 0;
	bool event_log_enabled = false;

public:
	void set_event_log_enabled(bool p_enabled);
};

} // namespace Kaizen
