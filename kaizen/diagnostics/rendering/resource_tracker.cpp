/**************************************************************************/
/*  resource_tracker.cpp                                                   */
/**************************************************************************/

#include "resource_tracker.h"

#include "core/os/memory.h"
#include "core/os/os.h"
#include "core/string/print_string.h"

namespace {

Kaizen::DiagnosticsMode parse_mode(const String &p_value) {
	const String value = p_value.strip_edges().to_lower();
	if (value.is_empty() || value == "off" || value == "disabled" || value == "0") {
		return Kaizen::DiagnosticsMode::Disabled;
	}
	if (value == "basic" || value == "1") {
		return Kaizen::DiagnosticsMode::Basic;
	}
	if (value == "detailed" || value == "2") {
		return Kaizen::DiagnosticsMode::Detailed;
	}
	if (value == "validation" || value == "3") {
		return Kaizen::DiagnosticsMode::Validation;
	}
	WARN_PRINT(vformat("Unknown KAIZEN_DIAGNOSTICS value '%s'. Valid values: off, basic, detailed, validation.", p_value));
	return Kaizen::DiagnosticsMode::Disabled;
}

String format_bytes(uint64_t p_bytes) {
	if (p_bytes >= 1024ull * 1024ull * 1024ull) {
		return String::num(p_bytes / (1024.0 * 1024.0 * 1024.0), 2) + " GiB";
	}
	if (p_bytes >= 1024ull * 1024ull) {
		return String::num(p_bytes / (1024.0 * 1024.0), 2) + " MiB";
	}
	if (p_bytes >= 1024ull) {
		return String::num(p_bytes / 1024.0, 2) + " KiB";
	}
	return String::num_uint64(p_bytes) + " B";
}

String format_registry(const char *p_label, const Kaizen::ResourceStats &p_stats) {
	return vformat(
			"%s: allocs=%s releases=%s live=%s pending=%s live_bytes=%s peak_bytes=%s invalid=%s stale=%s double_free=%s full=%s",
			p_label,
			String::num_uint64(p_stats.allocations),
			String::num_uint64(p_stats.releases),
			String::num_uint64(p_stats.live_resources),
			String::num_uint64(p_stats.pending_releases),
			format_bytes(p_stats.get_live_backend_bytes()),
			format_bytes(p_stats.peak_live_backend_bytes),
			String::num_uint64(p_stats.invalid_handles),
			String::num_uint64(p_stats.generation_mismatches),
			String::num_uint64(p_stats.double_frees),
			String::num_uint64(p_stats.registry_full));
}

} // namespace

KaizenResourceTracker *KaizenResourceTracker::get_singleton() {
	static KaizenResourceTracker *singleton = memnew(KaizenResourceTracker);
	return singleton;
}

KaizenResourceTracker::KaizenResourceTracker() {
	// The mode is read once per process. Diagnostics never change at runtime,
	// so the hot path only reads a plain field.
	const String value = OS::get_singleton() ? OS::get_singleton()->get_environment("KAIZEN_DIAGNOSTICS") : String();
	set_mode(parse_mode(value));
}

KaizenResourceTracker::~KaizenResourceTracker() {
	memdelete(gpu_registry);
	memdelete(cpu_registry);
}

void KaizenResourceTracker::_ensure_registries() {
	if (gpu_registry == nullptr) {
		gpu_registry = memnew(Kaizen::ResourceRegistry(Kaizen::ResourceKind::GPU, 16384));
	}
	if (cpu_registry == nullptr) {
		cpu_registry = memnew(Kaizen::ResourceRegistry(Kaizen::ResourceKind::CPU, 4096));
	}
}

void KaizenResourceTracker::set_mode(Kaizen::DiagnosticsMode p_mode) {
	mode = p_mode;
	if (mode == Kaizen::DiagnosticsMode::Disabled) {
		return;
	}

	_ensure_registries();
	const bool log_events = mode == Kaizen::DiagnosticsMode::Detailed || mode == Kaizen::DiagnosticsMode::Validation;
	gpu_registry->set_event_log_enabled(log_events);
	cpu_registry->set_event_log_enabled(log_events);
}

void KaizenResourceTracker::notify_allocation(RID p_rid, const char *p_type, uint64_t p_requested_bytes, Kaizen::ResourceKind p_kind, uint64_t p_frame) {
	if (mode == Kaizen::DiagnosticsMode::Disabled) {
		return;
	}

	Kaizen::ResourceRegistry &registry = p_kind == Kaizen::ResourceKind::GPU ? *gpu_registry : *cpu_registry;
	const Kaizen::ResourceHandle handle = registry.allocate(p_type, p_requested_bytes, p_frame);
	if (handle.is_null()) {
		return; // Registry full: counted by the registry, never fatal.
	}

	rid_lock.lock();
	rid_handles[p_rid.get_id()] = handle;
	rid_lock.unlock();
}

void KaizenResourceTracker::notify_backend_bytes(RID p_rid, uint64_t p_backend_bytes) {
	if (mode == Kaizen::DiagnosticsMode::Disabled) {
		return;
	}

	rid_lock.lock();
	const Kaizen::ResourceHandle *handle = rid_handles.getptr(p_rid.get_id());
	const Kaizen::ResourceHandle copy = handle ? *handle : Kaizen::ResourceHandle();
	rid_lock.unlock();

	if (copy.is_null()) {
		return;
	}

	Kaizen::ResourceRegistry &registry = copy.kind == Kaizen::ResourceKind::GPU ? *gpu_registry : *cpu_registry;
	registry.set_backend_bytes(copy, p_backend_bytes);
}

void KaizenResourceTracker::notify_logical_free(RID p_rid, uint64_t p_release_epoch) {
	if (mode == Kaizen::DiagnosticsMode::Disabled) {
		return;
	}

	rid_lock.lock();
	Kaizen::ResourceHandle handle;
	const Kaizen::ResourceHandle *found = rid_handles.getptr(p_rid.get_id());
	if (found) {
		handle = *found;
		rid_handles.erase(p_rid.get_id());
	}
	rid_lock.unlock();

	if (handle.is_null()) {
		return; // Not a buffer/texture RID.
	}

	Kaizen::ResourceRegistry &registry = handle.kind == Kaizen::ResourceKind::GPU ? *gpu_registry : *cpu_registry;
	registry.begin_release(handle, p_release_epoch);
}

uint32_t KaizenResourceTracker::notify_frame_completed(uint64_t p_epoch) {
	if (mode == Kaizen::DiagnosticsMode::Disabled) {
		return 0;
	}

	const uint32_t released = gpu_registry->confirm_releases_until(p_epoch) + cpu_registry->confirm_releases_until(p_epoch);
	runtime_confirmed_releases += released;
	return released;
}

void KaizenResourceTracker::notify_device_teardown() {
	if (mode == Kaizen::DiagnosticsMode::Disabled) {
		return;
	}

	const Kaizen::ResourceStats gpu_before = gpu_registry->get_stats();
	const Kaizen::ResourceStats cpu_before = cpu_registry->get_stats();
	teardown_confirmed_releases += gpu_before.pending_releases + cpu_before.pending_releases;

	gpu_registry->confirm_releases_until(UINT64_MAX);
	cpu_registry->confirm_releases_until(UINT64_MAX);

	uint32_t untracked_rids;
	rid_lock.lock();
	untracked_rids = rid_handles.size();
	rid_handles.clear();
	rid_lock.unlock();

	print_line(get_summary());
	if (untracked_rids > 0) {
		WARN_PRINT(vformat("[Kaizen][Resources] %d tracked RID(s) survived device teardown.", untracked_rids));
	}
}

bool KaizenResourceTracker::is_tracked(RID p_rid) const {
	if (mode == Kaizen::DiagnosticsMode::Disabled) {
		return false;
	}

	rid_lock.lock();
	const bool tracked = rid_handles.has(p_rid.get_id());
	rid_lock.unlock();
	return tracked;
}

Kaizen::ResourceStats KaizenResourceTracker::get_stats(Kaizen::ResourceKind p_kind) const {
	return p_kind == Kaizen::ResourceKind::GPU ? gpu_registry->get_stats() : cpu_registry->get_stats();
}

String KaizenResourceTracker::get_summary() const {
	const String gpu = format_registry("GPU", gpu_registry->get_stats());
	const String cpu = format_registry("CPU", cpu_registry->get_stats());
	return "[Kaizen][Resources] " + gpu + " | " + cpu +
			vformat(" | runtime_releases=%s teardown_releases=%s",
					String::num_uint64(runtime_confirmed_releases),
					String::num_uint64(teardown_confirmed_releases));
}
