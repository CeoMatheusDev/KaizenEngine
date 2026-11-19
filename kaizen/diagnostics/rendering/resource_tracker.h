/**************************************************************************/
/*  resource_tracker.h                                                     */
/**************************************************************************/
/*  Kaizen Engine — Resource Lifetime Diagnostics                          */
/*                                                                        */
/*  Engine-facing wrapper around Kaizen::ResourceRegistry. The tracker is */
/*  disabled by default and is enabled with the KAIZEN_DIAGNOSTICS         */
/*  environment variable (basic, detailed or validation). When disabled,  */
/*  every notify_* call returns after a single mode check.                */
/*                                                                        */
/*  Integration model:                                                    */
/*    RenderingDevice::create_*  -> notify_allocation()                   */
/*    RenderingDevice::free_rid() -> notify_logical_free()                */
/*    RenderingDevice::_begin_frame() -> notify_frame_completed()         */
/*    RenderingDevice::finalize() -> notify_device_teardown()             */
/**************************************************************************/

#pragma once

#include "core/error/error_macros.h"
#include "core/os/spin_lock.h"
#include "core/string/ustring.h"
#include "core/templates/hash_map.h"
#include "core/templates/rid.h"

#include "kaizen/diagnostics/rendering/resource_registry.h"

class KaizenResourceTracker {
public:
	static KaizenResourceTracker *get_singleton();

	void set_mode(Kaizen::DiagnosticsMode p_mode);
	Kaizen::DiagnosticsMode get_mode() const { return mode; }
	bool is_enabled() const { return mode != Kaizen::DiagnosticsMode::Disabled; }

	// Registers a newly created resource. `p_frame` is the renderer frame
	// counter at creation time, kept for lifetime diagnostics.
	void notify_allocation(RID p_rid, const char *p_type, uint64_t p_requested_bytes, Kaizen::ResourceKind p_kind, uint64_t p_frame);

	// Refines the backend byte count once the driver reports the real
	// allocation size (e.g. driver->texture_get_allocation_size()).
	void notify_backend_bytes(RID p_rid, uint64_t p_backend_bytes);

	// Marks the resource as logically freed. It remains accounted for as
	// PendingRelease until the ring frame that owns the deferred destruction
	// is processed. Untracked RIDs (uniform sets, framebuffers, ...) are
	// ignored on purpose: the tracker only follows buffers and textures.
	void notify_logical_free(RID p_rid, uint64_t p_release_epoch);

	// Confirms every pending release scheduled for an epoch <= p_epoch.
	uint32_t notify_frame_completed(uint64_t p_epoch);

	// Forces all pending releases to complete (device teardown, GPU idle) and
	// prints the session summary when diagnostics are enabled.
	void notify_device_teardown();

	bool is_tracked(RID p_rid) const;
	Kaizen::ResourceStats get_stats(Kaizen::ResourceKind p_kind) const;
	String get_summary() const;

private:
	KaizenResourceTracker();
	~KaizenResourceTracker();

	// Registries are allocated only when diagnostics are enabled, so a
	// disabled build keeps no tracking storage at all.
	void _ensure_registries();

	Kaizen::ResourceRegistry *cpu_registry = nullptr;
	Kaizen::ResourceRegistry *gpu_registry = nullptr;

	HashMap<uint64_t, Kaizen::ResourceHandle> rid_handles;
	mutable SpinLock rid_lock;

	Kaizen::DiagnosticsMode mode = Kaizen::DiagnosticsMode::Disabled;

	// Release path accounting: how many releases completed while frames were
	// running versus forced at device teardown. A healthy session confirms the
	// churn at runtime and only flushes the shutdown resources at teardown.
	uint64_t runtime_confirmed_releases = 0;
	uint64_t teardown_confirmed_releases = 0;
};

// Convenience macro for creation sites in upstream code. Expands to nothing
// when diagnostics never run, keeping call sites readable.
#define KAIZEN_TRACK_GPU_RESOURCE(m_rid, m_bytes, m_type, m_frame) \
	KaizenResourceTracker::get_singleton()->notify_allocation((m_rid), (m_type), (m_bytes), Kaizen::ResourceKind::GPU, (m_frame))
