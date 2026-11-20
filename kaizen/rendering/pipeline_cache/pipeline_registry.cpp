/**************************************************************************/
/*  pipeline_registry.cpp                                                 */
/**************************************************************************/

#include "pipeline_registry.h"

namespace Kaizen {

PipelineRegistry::PipelineRegistry(uint32_t p_capacity, uint64_t p_budget_bytes, uint32_t p_initial_generation) :
		capacity(p_capacity == 0 ? 1 : p_capacity),
		budget_bytes(p_budget_bytes),
		slots(capacity),
		lru(capacity),
		retirement(capacity) {
	free_indices.reserve(capacity);
	// Push in reverse so index 0 is handed out first.
	for (uint32_t i = capacity; i > 0; i--) {
		free_indices.push_back(i - 1);
	}
	ready_buffer.resize(capacity);

	// A fresh allocation must yield `p_initial_generation`, so the slot starts
	// one below it. Generation 0 is reserved for null handles.
	const uint32_t starting_generation = p_initial_generation == 0 ? 0 : p_initial_generation - 1;
	for (Slot &slot : slots) {
		slot.generation = starting_generation;
	}
}

PipelineHandle PipelineRegistry::_make_handle_locked(uint32_t p_index) const {
	PipelineHandle handle;
	handle.index = p_index;
	handle.generation = slots[p_index].generation;
	return handle;
}

static std::string numeric_pipeline_key_identity(uint64_t p_key) {
	std::string identity(1, '\0');
	for (size_t i = 0; i < sizeof(p_key); i++) {
		identity.push_back(static_cast<char>(p_key >> (i * 8)));
	}
	return identity;
}

static std::string canonical_pipeline_key_identity(const PipelineKey &p_key) {
	std::string identity(1, '\1');
	if (!p_key.data.empty()) {
		identity.append(reinterpret_cast<const char *>(p_key.data.data()), p_key.data.size());
	}
	return identity;
}

bool PipelineRegistry::_resolve_locked(PipelineHandle p_handle, Slot *&r_slot) const {
	if (p_handle.is_null() || p_handle.index >= capacity) {
		return false;
	}

	const Slot &slot = slots[p_handle.index];
	if (slot.state == PipelineState::Free || slot.generation != p_handle.generation) {
		return false;
	}

	r_slot = const_cast<Slot *>(&slot);
	return true;
}

bool PipelineRegistry::_is_ready_locked(const Slot &p_slot) const {
	if (p_slot.host_refs != 0 || p_slot.recording_refs != 0) {
		return false;
	}
	return p_slot.usage.is_complete(completed_serials);
}

PipelineHandle PipelineRegistry::acquire(uint64_t p_key, uint64_t p_bytes) {
	return _acquire(numeric_pipeline_key_identity(p_key), p_key, p_bytes);
}

PipelineHandle PipelineRegistry::acquire(const PipelineKey &p_key, uint64_t p_bytes) {
	return _acquire(canonical_pipeline_key_identity(p_key), pipeline_key_hash(p_key), p_bytes);
}

PipelineHandle PipelineRegistry::_acquire(const std::string &p_key_identity, uint64_t p_report_key, uint64_t p_bytes) {
	lock.lock();

	PipelineHandle result;
	auto existing = key_to_index.find(p_key_identity);

	if (existing != key_to_index.end()) {
		const uint32_t index = existing->second;
		Slot &slot = slots[index];

		if (slot.state != PipelineState::Resident) {
			// The object exists but is on its way out. Creating a second object
			// for the same key would break the "one live object per key" rule,
			// so the caller must retry after it is destroyed.
			blocked_by_retirement++;
			lock.unlock();
			return result;
		}

		slot.host_refs++;
		lru.touch(index);
		hits++;
		acquisitions++;
		result = _make_handle_locked(index);
		lock.unlock();
		return result;
	}

	if (free_indices.empty()) {
		rejected_full++;
		lock.unlock();
		return result;
	}

	const uint32_t index = free_indices[free_indices.size() - 1];
	free_indices.pop_back();

	Slot &slot = slots[index];
	// 0 is reserved for null handles; skip it on wrap.
	if (slot.generation == UINT32_MAX) {
		slot.generation = 1;
	} else {
		slot.generation++;
	}

	slot.key = p_report_key;
	slot.key_identity = p_key_identity;
	slot.bytes = p_bytes;
	slot.usage = PipelineQueueUsage();
	slot.host_refs = 1;
	slot.recording_refs = 0;
	slot.state = PipelineState::Resident;

	key_to_index[p_key_identity] = index;
	lru.touch(index);

	resident_bytes += p_bytes;
	if (resident_bytes > peak_resident_bytes) {
		peak_resident_bytes = resident_bytes;
	}

	misses++;
	acquisitions++;
	result = _make_handle_locked(index);
	lock.unlock();
	return result;
}

bool PipelineRegistry::release(PipelineHandle p_handle) {
	lock.lock();

	Slot *slot = nullptr;
	if (!_resolve_locked(p_handle, slot)) {
		lock.unlock();
		return false;
	}

	if (slot->state == PipelineState::Retiring || slot->state == PipelineState::Destroyable) {
		lock.unlock();
		return false;
	}

	if (slot->host_refs == 0) {
		lock.unlock();
		return false;
	}

	slot->host_refs--;
	releases++;

	if (slot->host_refs == 0 && slot->recording_refs == 0 &&
			slot->state == PipelineState::EvictionCandidate) {
		_retire_locked(p_handle.index);
	}

	lock.unlock();
	return true;
}

bool PipelineRegistry::begin_use(PipelineHandle p_handle) {
	lock.lock();

	Slot *slot = nullptr;
	if (!_resolve_locked(p_handle, slot)) {
		lock.unlock();
		return false;
	}

	// Candidates are no longer acquirable and must not receive new recording
	// work; an in-flight recording keeps its reference and is left alone.
	if (slot->state != PipelineState::Resident) {
		lock.unlock();
		return false;
	}

	slot->recording_refs++;
	recording_starts++;
	lock.unlock();
	return true;
}

bool PipelineRegistry::end_use(PipelineHandle p_handle, PipelineQueue p_queue, uint64_t p_submission_serial) {
	const uint32_t queue = pipeline_queue_index(p_queue);
	if (queue >= PIPELINE_QUEUE_COUNT || p_submission_serial == 0) {
		lock.lock();
		invalid_serials++;
		lock.unlock();
		return false;
	}

	lock.lock();

	Slot *slot = nullptr;
	if (!_resolve_locked(p_handle, slot)) {
		lock.unlock();
		return false;
	}

	if (slot->state != PipelineState::Resident && slot->state != PipelineState::EvictionCandidate) {
		lock.unlock();
		return false;
	}

	if (slot->recording_refs == 0) {
		lock.unlock();
		return false;
	}

	slot->recording_refs--;
	slot->usage.record(p_queue, p_submission_serial);

	if (slot->host_refs == 0 && slot->recording_refs == 0 &&
			slot->state == PipelineState::EvictionCandidate) {
		_retire_locked(p_handle.index);
	}

	lock.unlock();
	return true;
}

bool PipelineRegistry::abort_use(PipelineHandle p_handle) {
	lock.lock();

	Slot *slot = nullptr;
	if (!_resolve_locked(p_handle, slot)) {
		lock.unlock();
		return false;
	}

	if (slot->state != PipelineState::Resident && slot->state != PipelineState::EvictionCandidate) {
		lock.unlock();
		return false;
	}

	if (slot->recording_refs == 0) {
		lock.unlock();
		return false;
	}

	// The submission never executed, so no queue usage is recorded. Dropping
	// the reference is enough: the object either stays resident or retires
	// with its previously recorded uses.
	slot->recording_refs--;
	aborted_recordings++;

	if (slot->host_refs == 0 && slot->recording_refs == 0 &&
			slot->state == PipelineState::EvictionCandidate) {
		_retire_locked(p_handle.index);
	}

	lock.unlock();
	return true;
}

bool PipelineRegistry::mark_use(PipelineHandle p_handle, PipelineQueue p_queue, uint64_t p_submission_serial) {
	const uint32_t queue = pipeline_queue_index(p_queue);
	if (queue >= PIPELINE_QUEUE_COUNT || p_submission_serial == 0) {
		lock.lock();
		invalid_serials++;
		lock.unlock();
		return false;
	}

	lock.lock();

	Slot *slot = nullptr;
	if (!_resolve_locked(p_handle, slot)) {
		lock.unlock();
		return false;
	}

	if (slot->state != PipelineState::Resident && slot->state != PipelineState::EvictionCandidate) {
		// The object is already waiting for completion; a submission referring
		// to it now would race with destruction.
		late_use++;
		lock.unlock();
		return false;
	}

	slot->usage.record(p_queue, p_submission_serial);
	lock.unlock();
	return true;
}

bool PipelineRegistry::describe(PipelineHandle p_handle, PipelineInfo &r_info) const {
	lock.lock();

	Slot *slot = nullptr;
	if (!_resolve_locked(p_handle, slot)) {
		lock.unlock();
		return false;
	}

	r_info.key = slot->key;
	r_info.bytes = slot->bytes;
	r_info.last_use_serial = slot->usage.max_serial();
	r_info.usage = slot->usage;
	r_info.generation = slot->generation;
	r_info.host_refs = slot->host_refs;
	r_info.recording_refs = slot->recording_refs;
	r_info.state = slot->state;

	lock.unlock();
	return true;
}

bool PipelineRegistry::is_valid(PipelineHandle p_handle) const {
	lock.lock();
	Slot *slot = nullptr;
	const bool valid = _resolve_locked(p_handle, slot);
	lock.unlock();
	return valid;
}

uint32_t PipelineRegistry::set_completed_serial(PipelineQueue p_queue, uint64_t p_completed_serial) {
	const uint32_t queue = pipeline_queue_index(p_queue);
	if (queue >= PIPELINE_QUEUE_COUNT) {
		return 0;
	}

	lock.lock();

	if (p_completed_serial > completed_serials[queue]) {
		completed_serials[queue] = p_completed_serial;
	}

	const uint32_t ready = retirement.collect_ready(completed_serials, ready_buffer.data(), (uint32_t)ready_buffer.size());

	uint32_t transitioned = 0;
	for (uint32_t i = 0; i < ready; i++) {
		const PipelineRetirementQueue::Entry &entry = ready_buffer[i];
		if (entry.index >= capacity) {
			continue;
		}

		Slot &slot = slots[entry.index];
		if (slot.state != PipelineState::Retiring || slot.generation != entry.generation) {
			continue;
		}

		slot.state = PipelineState::Destroyable;
		destroyable++;
		transitioned++;
	}

	lock.unlock();
	return transitioned;
}

uint32_t PipelineRegistry::collect_destroyable(PipelineHandle *r_out, uint32_t p_max) const {
	lock.lock();

	uint32_t written = 0;
	for (uint32_t i = 0; i < capacity && written < p_max; i++) {
		if (slots[i].state == PipelineState::Destroyable) {
			r_out[written++] = _make_handle_locked(i);
		}
	}

	lock.unlock();
	return written;
}

bool PipelineRegistry::destroy(PipelineHandle p_handle) {
	lock.lock();

	Slot *slot = nullptr;
	if (!_resolve_locked(p_handle, slot)) {
		lock.unlock();
		return false;
	}

	if (slot->state != PipelineState::Destroyable) {
		lock.unlock();
		return false;
	}

	if (slot->host_refs != 0 || slot->recording_refs != 0) {
		lock.unlock();
		return false;
	}

	const uint32_t index = p_handle.index;

	// Keep the identity reserved until this physical slot is destroyed.
	auto existing = key_to_index.find(slot->key_identity);
	if (existing != key_to_index.end() && existing->second == index) {
		key_to_index.erase(existing);
	}

	if (pending_bytes >= slot->bytes) {
		pending_bytes -= slot->bytes;
	} else {
		pending_bytes = 0;
	}

	slot->state = PipelineState::Free;
	slot->key = 0;
	slot->key_identity.clear();
	slot->bytes = 0;
	slot->usage = PipelineQueueUsage();
	slot->host_refs = 0;
	slot->recording_refs = 0;

	free_indices.push_back(index);
	destroyed++;

	lock.unlock();
	return true;
}

void PipelineRegistry::_move_to_pending_locked(Slot &r_slot) {
	if (resident_bytes >= r_slot.bytes) {
		resident_bytes -= r_slot.bytes;
	} else {
		resident_bytes = 0;
	}
	pending_bytes += r_slot.bytes;
}

void PipelineRegistry::_mark_candidate_locked(uint32_t p_index) {
	Slot &slot = slots[p_index];
	if (slot.state != PipelineState::Resident) {
		return;
	}

	slot.state = PipelineState::EvictionCandidate;
	lru.remove(p_index);
	_move_to_pending_locked(slot);
	eviction_candidates++;

	if (slot.host_refs == 0 && slot.recording_refs == 0) {
		_retire_locked(p_index);
	}
}

void PipelineRegistry::_retire_locked(uint32_t p_index) {
	Slot &slot = slots[p_index];
	if (slot.state == PipelineState::Retiring || slot.state == PipelineState::Destroyable || slot.state == PipelineState::Free) {
		return;
	}

	slot.state = PipelineState::Retiring;
	retirement.push(p_index, slot.generation, slot.usage);
	retired++;

	// Normally the slot left the LRU when it became a candidate; this keeps the
	// invariant if retirement is ever reached from another path.
	lru.remove(p_index);

	// The key mapping is intentionally kept: a second object for the same key
	// must not be created while this one is still waiting for completion.
	// destroy() erases the mapping, and only then can the key be acquired again.
}

void PipelineRegistry::_enforce_budget_locked() {
	while (resident_bytes > budget_bytes && !lru.is_empty()) {
		const uint32_t index = lru.peek_lru();
		_mark_candidate_locked(index);
	}
}

bool PipelineRegistry::retire_now(PipelineHandle p_handle) {
	lock.lock();

	Slot *slot = nullptr;
	if (!_resolve_locked(p_handle, slot)) {
		lock.unlock();
		return false;
	}

	if (slot->state != PipelineState::Resident && slot->state != PipelineState::EvictionCandidate) {
		lock.unlock();
		return false;
	}

	// The caller must have dropped every reference first; otherwise the
	// object could still be recorded into a command buffer.
	if (slot->host_refs != 0 || slot->recording_refs != 0) {
		lock.unlock();
		return false;
	}

	const uint32_t index = p_handle.index;
	if (slot->state == PipelineState::Resident) {
		_move_to_pending_locked(*slot);
		lru.remove(index);
	}

	requested_retirements++;
	_retire_locked(index);

	lock.unlock();
	return true;
}

bool PipelineRegistry::retire_after_owner_shutdown(PipelineHandle p_handle) {
	lock.lock();

	Slot *slot = nullptr;
	if (!_resolve_locked(p_handle, slot)) {
		lock.unlock();
		return false;
	}

	if (slot->state == PipelineState::Resident) {
		_move_to_pending_locked(*slot);
		lru.remove(p_handle.index);
	} else if (slot->state != PipelineState::EvictionCandidate) {
		const bool already_retiring = slot->state == PipelineState::Retiring || slot->state == PipelineState::Destroyable;
		lock.unlock();
		return already_retiring;
	}

	slot->host_refs = 0;
	slot->recording_refs = 0;
	_retire_locked(p_handle.index);

	lock.unlock();
	return true;
}

uint32_t PipelineRegistry::enforce_budget() {
	lock.lock();
	const uint64_t before = eviction_candidates;
	_enforce_budget_locked();
	const uint32_t added = (uint32_t)(eviction_candidates - before);
	lock.unlock();
	return added;
}

void PipelineRegistry::set_budget(uint64_t p_budget_bytes) {
	lock.lock();
	budget_bytes = p_budget_bytes;
	lock.unlock();
}

uint64_t PipelineRegistry::get_budget() const {
	lock.lock();
	const uint64_t budget = budget_bytes;
	lock.unlock();
	return budget;
}

PipelineStats PipelineRegistry::get_stats() const {
	lock.lock();

	PipelineStats stats;
	stats.acquisitions = acquisitions;
	stats.hits = hits;
	stats.misses = misses;
	stats.releases = releases;
	stats.recording_starts = recording_starts;
	stats.aborted_recordings = aborted_recordings;
	stats.eviction_candidates = eviction_candidates;
	stats.requested_retirements = requested_retirements;
	stats.retired = retired;
	stats.destroyable = destroyable;
	stats.destroyed = destroyed;
	stats.blocked_by_retirement = blocked_by_retirement;
	stats.rejected_full = rejected_full;
	stats.late_use = late_use;
	stats.invalid_serials = invalid_serials;
	stats.resident_bytes = resident_bytes;
	stats.pending_bytes = pending_bytes;
	stats.peak_resident_bytes = peak_resident_bytes;
	for (uint32_t i = 0; i < PIPELINE_QUEUE_COUNT; i++) {
		stats.completed_serials[i] = completed_serials[i];
	}

	for (const Slot &slot : slots) {
		switch (slot.state) {
			case PipelineState::Resident:
				stats.resident_count++;
				break;
			case PipelineState::EvictionCandidate:
				stats.candidate_count++;
				break;
			case PipelineState::Retiring:
				stats.retiring_count++;
				break;
			case PipelineState::Destroyable:
				stats.destroyable_count++;
				break;
			case PipelineState::Free:
				break;
		}
	}

	lock.unlock();
	return stats;
}

uint32_t PipelineRegistry::get_slot_size() {
	return (uint32_t)sizeof(Slot);
}

} // namespace Kaizen
