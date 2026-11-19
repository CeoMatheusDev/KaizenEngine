/**************************************************************************/
/*  resource_registry.cpp                                                 */
/**************************************************************************/

#include "resource_registry.h"

namespace Kaizen {

ResourceRegistry::ResourceRegistry(ResourceKind p_kind, uint32_t p_capacity) :
		kind(p_kind), capacity(p_capacity) {
	records.resize(capacity);
	free_indices.reserve(capacity);
	pending.reserve(capacity);
}

void ResourceRegistry::set_event_log_enabled(bool p_enabled) {
	lock.lock();
	event_log_enabled = p_enabled;
	if (p_enabled && events.empty()) {
		events.resize(EVENT_CAPACITY);
	}
	event_head = 0;
	event_count = 0;
	lock.unlock();
}

bool ResourceRegistry::_resolve_locked(ResourceHandle p_handle, ResourceState *r_state) const {
	if (p_handle.is_null() || p_handle.kind != kind || p_handle.index >= capacity) {
		return false;
	}

	const ResourceRecord &record = records[p_handle.index];
	if (record.generation != p_handle.generation) {
		return false;
	}

	if (r_state != nullptr) {
		*r_state = record.state;
	}
	return true;
}

void ResourceRegistry::_push_event_locked(ResourceEvent::Type p_type, uint32_t p_index, uint64_t p_frame) {
	if (!event_log_enabled || events.empty()) {
		return;
	}

	const ResourceRecord &record = records[p_index];

	ResourceEvent &event = events[event_head];
	event.type = p_type;
	event.index = p_index;
	event.generation = record.generation;
	event.kind = record.kind;
	event.frame = p_frame;
	event.bytes = record.requested_bytes;
	event.type_name = record.type_name;

	event_head = (event_head + 1) % EVENT_CAPACITY;
	if (event_count < EVENT_CAPACITY) {
		event_count++;
	}
}

ResourceHandle ResourceRegistry::allocate(const char *p_type, uint64_t p_requested_bytes, uint64_t p_frame) {
	lock.lock();

	uint32_t index;
	if (!free_indices.empty()) {
		index = free_indices.back();
		free_indices.pop_back();
	} else if (next_index < capacity) {
		index = next_index++;
	} else {
		stats.registry_full++;
		lock.unlock();
		return ResourceHandle();
	}

	ResourceRecord &record = records[index];
	record.generation++;
	if (record.generation == 0) {
		// 0 is reserved for null handles; skip it on wrap (practically unreachable).
		record.generation = 1;
	}
	record.kind = kind;
	record.state = ResourceState::Alive;
	record.type_name = p_type;
	record.requested_bytes = p_requested_bytes;
	record.backend_bytes = p_requested_bytes;
	record.allocation_frame = p_frame;
	record.release_epoch = 0;
	record.release_frame = 0;

	live_count++;
	stats.allocations++;
	stats.live_resources = live_count;
	stats.alive_requested_bytes += p_requested_bytes;
	stats.alive_backend_bytes += p_requested_bytes;

	const uint64_t live_requested = stats.get_live_requested_bytes();
	if (live_requested > stats.peak_live_requested_bytes) {
		stats.peak_live_requested_bytes = live_requested;
	}
	const uint64_t live_backend = stats.get_live_backend_bytes();
	if (live_backend > stats.peak_live_backend_bytes) {
		stats.peak_live_backend_bytes = live_backend;
	}

	ResourceHandle handle;
	handle.index = index;
	handle.generation = record.generation;
	handle.kind = kind;

	_push_event_locked(ResourceEvent::Type::Allocated, index, p_frame);
	lock.unlock();

	return handle;
}

bool ResourceRegistry::set_backend_bytes(ResourceHandle p_handle, uint64_t p_backend_bytes) {
	lock.lock();

	ResourceState state;
	if (!_resolve_locked(p_handle, &state) || state == ResourceState::Released) {
		stats.invalid_handles++;
		lock.unlock();
		return false;
	}

	ResourceRecord &record = records[p_handle.index];
	const uint64_t old_bytes = record.backend_bytes;
	record.backend_bytes = p_backend_bytes;

	if (state == ResourceState::Alive) {
		stats.alive_backend_bytes -= old_bytes;
		stats.alive_backend_bytes += p_backend_bytes;
	} else {
		stats.pending_backend_bytes -= old_bytes;
		stats.pending_backend_bytes += p_backend_bytes;
	}

	const uint64_t live_backend = stats.get_live_backend_bytes();
	if (live_backend > stats.peak_live_backend_bytes) {
		stats.peak_live_backend_bytes = live_backend;
	}
	stats.backend_bytes_overrides++;

	lock.unlock();
	return true;
}

bool ResourceRegistry::begin_release(ResourceHandle p_handle, uint64_t p_release_epoch) {
	lock.lock();

	ResourceState state;
	if (!_resolve_locked(p_handle, &state)) {
		if (p_handle.kind == kind && p_handle.index < capacity) {
			stats.generation_mismatches++;
		} else {
			stats.invalid_handles++;
		}
		lock.unlock();
		return false;
	}

	ResourceRecord &record = records[p_handle.index];

	if (state == ResourceState::Alive) {
		record.state = ResourceState::PendingRelease;
		record.release_epoch = p_release_epoch;

		stats.alive_requested_bytes -= record.requested_bytes;
		stats.pending_requested_bytes += record.requested_bytes;
		stats.alive_backend_bytes -= record.backend_bytes;
		stats.pending_backend_bytes += record.backend_bytes;
		stats.pending_releases = ++pending_count;

		PendingEntry entry;
		entry.index = p_handle.index;
		entry.generation = p_handle.generation;
		entry.epoch = p_release_epoch;
		pending.push_back(entry);

		_push_event_locked(ResourceEvent::Type::ReleaseRequested, p_handle.index, p_release_epoch);
		lock.unlock();
		return true;
	}

	// PendingRelease, Released and Free all mean "already logically freed".
	stats.double_frees++;
	_push_event_locked(ResourceEvent::Type::DoubleFree, p_handle.index, p_release_epoch);
	lock.unlock();
	return false;
}

void ResourceRegistry::_release_locked(uint32_t p_index) {
	ResourceRecord &record = records[p_index];
	if (record.state != ResourceState::PendingRelease) {
		return;
	}

	record.state = ResourceState::Released;

	stats.pending_requested_bytes -= record.requested_bytes;
	stats.pending_backend_bytes -= record.backend_bytes;
	stats.released_requested_bytes += record.requested_bytes;
	stats.released_backend_bytes += record.backend_bytes;
	stats.pending_releases = --pending_count;
	stats.live_resources = --live_count;
	stats.releases++;

	_push_event_locked(ResourceEvent::Type::Released, p_index, 0);

	free_indices.push_back(p_index);
}

bool ResourceRegistry::confirm_release(ResourceHandle p_handle) {
	lock.lock();

	ResourceState state;
	if (!_resolve_locked(p_handle, &state)) {
		stats.invalid_handles++;
		lock.unlock();
		return false;
	}

	// A handle in the free list may be re-allocated and then have a newer
	// generation, so reaching here with a non-pending state is misuse.
	if (state != ResourceState::PendingRelease) {
		stats.double_frees++;
		lock.unlock();
		return false;
	}

	_release_locked(p_handle.index);
	lock.unlock();
	return true;
}

uint32_t ResourceRegistry::confirm_releases_until(uint64_t p_epoch) {
	lock.lock();

	uint32_t released = 0;
	uint32_t write = 0;
	for (uint32_t read = 0; read < pending.size(); read++) {
		const PendingEntry &entry = pending[read];
		const ResourceRecord &record = records[entry.index];
		const bool same_generation = record.generation == entry.generation;

		if (entry.epoch <= p_epoch && same_generation && record.state == ResourceState::PendingRelease) {
			_release_locked(entry.index);
			released++;
			continue;
		}

		// Keep entries that are still pending or not due yet.
		if (same_generation && record.state == ResourceState::PendingRelease) {
			if (write != read) {
				pending[write] = entry;
			}
			write++;
		}
	}
	pending.resize(write);

	lock.unlock();
	return released;
}

bool ResourceRegistry::is_valid(ResourceHandle p_handle) const {
	lock.lock();
	ResourceState state;
	const bool valid = _resolve_locked(p_handle, &state) &&
			(state == ResourceState::Alive || state == ResourceState::PendingRelease);
	lock.unlock();
	return valid;
}

bool ResourceRegistry::describe(ResourceHandle p_handle, ResourceRecord &r_record) const {
	lock.lock();
	ResourceState state;
	const bool found = _resolve_locked(p_handle, &state);
	if (found) {
		r_record = records[p_handle.index];
	}
	lock.unlock();
	return found;
}

ResourceStats ResourceRegistry::get_stats() const {
	lock.lock();
	ResourceStats copy = stats;
	lock.unlock();
	return copy;
}

uint32_t ResourceRegistry::get_events(ResourceEvent *r_events, uint32_t p_max) const {
	lock.lock();

	const uint32_t copy_count = event_count < p_max ? event_count : p_max;
	const uint32_t skip = event_count - copy_count;
	for (uint32_t i = 0; i < copy_count; i++) {
		const uint32_t slot = (event_head + EVENT_CAPACITY - event_count + skip + i) % EVENT_CAPACITY;
		r_events[i] = events[slot];
	}

	lock.unlock();
	return copy_count;
}

} // namespace Kaizen
