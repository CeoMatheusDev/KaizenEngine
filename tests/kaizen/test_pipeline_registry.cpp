/**************************************************************************/
/*  test_pipeline_registry.cpp                                            */
/**************************************************************************/
/*  Standalone tests for the Kaizen pipeline lifetime core (P2).          */
/*                                                                        */
/*  Build and run (no engine build required):                             */
/*    ./tests/kaizen/run_tests.sh                                         */
/*    ./tests/kaizen/run_tests.sh --bench                                 */
/*                                                                        */
/*  Acceptance criteria covered:                                          */
/*    - no eviction while resident bytes are at or below the budget       */
/*    - LRU order selects the least recently used resident pipeline       */
/*    - objects with host or recording references are not destroyed       */
/*    - destruction waits for the backend completion serial               */
/*    - a key cannot be duplicated while its old object is retiring       */
/*    - reclaimed slots produce new generations and reject stale handles  */
/*    - capacity exhaustion fails in a controlled way                     */
/*    - every object can be drained at shutdown                           */
/*    - concurrent eviction/destruction never frees a referenced object   */
/*    - statistics match the observed lifecycle                           */
/**************************************************************************/

#include "kaizen/rendering/pipeline_cache/pipeline_lru.h"
#include "kaizen/rendering/pipeline_cache/pipeline_key.h"
#include "kaizen/rendering/pipeline_cache/pipeline_registry.h"
#include "kaizen/rendering/pipeline_cache/pipeline_submission.h"
#include "kaizen/rendering/pipeline_cache/pipeline_retirement.h"

#include <algorithm>
#include <atomic>
#include <chrono>
#include <cstdint>
#include <cstdio>
#include <cstring>
#include <thread>
#include <unordered_map>
#include <vector>

namespace {

int failures = 0;
int checks = 0;

void check_impl(bool p_condition, const char *p_expression, const char *p_test, int p_line) {
	checks++;
	if (!p_condition) {
		failures++;
		std::fprintf(stderr, "FAIL [%s] %s:%d: %s\n", p_test, __FILE__, p_line, p_expression);
	}
}

#define CHECK(cond) check_impl((cond), #cond, test_name, __LINE__)
#define CHECK_EQ(a, b) check_impl((a) == (b), #a " == " #b, test_name, __LINE__)

#define TEST_BEGIN(name) \
	static void name() { \
		const char *test_name = #name;

#define TEST_END() \
	}

using Kaizen::PipelineHandle;
using Kaizen::PipelineInfo;
using Kaizen::PipelineKey;
using Kaizen::PipelineLRU;
using Kaizen::PipelineQueue;
using Kaizen::PipelineRegistry;
using Kaizen::PipelineRetirementQueue;
using Kaizen::PipelineState;
using Kaizen::PipelineStats;

constexpr uint64_t KIB = 1024;

struct ConstantPipelineKeyHash {
	size_t operator()(const PipelineKey &) const {
		return 0;
	}
};

// Drains every Destroyable entry. Returns how many were destroyed.
uint32_t drain_destroyable(PipelineRegistry &r_registry) {
	std::vector<PipelineHandle> handles(r_registry.get_capacity());
	uint32_t total = 0;
	while (true) {
		const uint32_t count = r_registry.collect_destroyable(handles.data(), (uint32_t)handles.size());
		if (count == 0) {
			break;
		}
		for (uint32_t i = 0; i < count; i++) {
			if (!r_registry.destroy(handles[i])) {
				check_impl(false, "r_registry.destroy(handles[i])", "drain_destroyable", __LINE__);
			}
			total++;
		}
	}
	return total;
}

// ---------------------------------------------------------------------------
// Lifecycle and accounting

TEST_BEGIN(pipeline_key_uses_complete_canonical_identity)
	PipelineKey first;
	first.add_u64(0x123456789abcdef0ULL);
	first.add_bool(true);
	first.add_float(1.0f);

	PipelineKey same;
	same.add_u64(0x123456789abcdef0ULL);
	same.add_bool(true);
	same.add_float(1.0f);

	PipelineKey different;
	different.add_u64(0x123456789abcdef0ULL);
	different.add_bool(false);
	different.add_float(1.0f);

	CHECK(first == same);
	CHECK_EQ(Kaizen::pipeline_key_hash(first), Kaizen::pipeline_key_hash(same));
	CHECK(first != different);

	// Force every key into the same hash bucket; equality must still separate them.
	std::unordered_map<PipelineKey, int, ConstantPipelineKeyHash> colliding_keys;
	colliding_keys[first] = 1;
	colliding_keys[different] = 2;
	CHECK_EQ(colliding_keys.size(), (size_t)2);
	CHECK_EQ(colliding_keys[first], 1);
	CHECK_EQ(colliding_keys[different], 2);
TEST_END()

TEST_BEGIN(typed_registry_shares_exact_keys_and_blocks_retirement_reacquire)
	PipelineRegistry registry(4, 0);

	PipelineKey first_key;
	first_key.add_u32(17);
	first_key.add_u32(29);
	PipelineKey different_key;
	different_key.add_u32(17);
	different_key.add_u32(30);

	PipelineHandle first = registry.acquire(first_key, KIB);
	PipelineHandle shared = registry.acquire(first_key, KIB);
	PipelineHandle different = registry.acquire(different_key, KIB);
	CHECK(!first.is_null());
	CHECK_EQ(shared, first);
	CHECK(!different.is_null());
	CHECK(different != first);
	CHECK_EQ(registry.get_stats().hits, (uint64_t)1);
	CHECK_EQ(registry.get_stats().misses, (uint64_t)2);

	CHECK(registry.release(first));
	CHECK(registry.is_valid(shared));
	PipelineInfo shared_info;
	CHECK(registry.describe(shared, shared_info));
	CHECK_EQ(shared_info.host_refs, (uint32_t)1);
	CHECK(registry.release(shared));
	CHECK(registry.retire_now(first));
	CHECK(registry.acquire(first_key, KIB).is_null());

	CHECK_EQ(registry.set_completed_serial(PipelineQueue::Graphics, 0), (uint32_t)1);
	PipelineHandle destroyable[1];
	CHECK_EQ(registry.collect_destroyable(destroyable, 1), (uint32_t)1);
	CHECK(registry.acquire(first_key, KIB).is_null());
	CHECK(registry.destroy(destroyable[0]));
	PipelineHandle recreated = registry.acquire(first_key, KIB);
	CHECK(!recreated.is_null());
	CHECK(recreated != first);
TEST_END()

TEST_BEGIN(owner_shutdown_retires_entries_with_live_logical_references)
	PipelineRegistry registry(2, 0);
	PipelineKey key;
	key.add_u32(41);

	PipelineHandle owner = registry.acquire(key, KIB);
	PipelineHandle shared = registry.acquire(key, KIB);
	CHECK_EQ(shared, owner);
	CHECK(registry.retire_after_owner_shutdown(owner));
	CHECK(registry.acquire(key, KIB).is_null());

	PipelineInfo info;
	CHECK(registry.describe(owner, info));
	CHECK_EQ(info.state, PipelineState::Retiring);
	CHECK_EQ(info.host_refs, (uint32_t)0);
	CHECK_EQ(info.recording_refs, (uint32_t)0);

	CHECK_EQ(registry.set_completed_serial(PipelineQueue::Graphics, 0), (uint32_t)1);
	PipelineHandle destroyable[1];
	CHECK_EQ(registry.collect_destroyable(destroyable, 1), (uint32_t)1);
	CHECK(registry.destroy(destroyable[0]));
	CHECK_EQ(registry.get_stats().resident_count, (uint64_t)0);
	CHECK_EQ(registry.get_stats().pending_bytes, (uint64_t)0);
TEST_END()

TEST_BEGIN(acquire_hit_and_miss)
	PipelineRegistry registry(8, 4 * KIB);

	PipelineHandle a = registry.acquire(1, KIB);
	CHECK(!a.is_null());
	CHECK_EQ(a.index, (uint32_t)0);

	PipelineHandle again = registry.acquire(1, KIB);
	CHECK_EQ(again, a);

	PipelineHandle b = registry.acquire(2, KIB);
	CHECK(!b.is_null());
	CHECK(b != a);

	PipelineStats stats = registry.get_stats();
	CHECK_EQ(stats.acquisitions, (uint64_t)3);
	CHECK_EQ(stats.hits, (uint64_t)1);
	CHECK_EQ(stats.misses, (uint64_t)2);
	CHECK_EQ(stats.resident_count, (uint64_t)2);
	CHECK_EQ(stats.resident_bytes, 2 * KIB);

	PipelineInfo info;
	CHECK(registry.describe(a, info));
	CHECK_EQ(info.key, (uint64_t)1);
	CHECK_EQ(info.bytes, KIB);
	CHECK_EQ(info.host_refs, (uint32_t)2);
	CHECK_EQ(info.state, PipelineState::Resident);
TEST_END()

TEST_BEGIN(release_rejects_invalid_sequences)
	PipelineRegistry registry(4, 4 * KIB);

	PipelineHandle handle = registry.acquire(1, KIB);
	CHECK(registry.release(handle));
	CHECK_EQ(registry.get_stats().releases, (uint64_t)1);

	// Second release of the same host reference is rejected.
	CHECK(!registry.release(handle));

	// begin_use/end_use are paired.
	CHECK(!registry.end_use(handle, PipelineQueue::Graphics, 1)); // no recording reference yet
	CHECK(registry.begin_use(handle));
	CHECK(registry.end_use(handle, PipelineQueue::Graphics, 1));
	CHECK(!registry.end_use(handle, PipelineQueue::Graphics, 2)); // already ended
	CHECK(!registry.begin_use(PipelineHandle()));
TEST_END()

TEST_BEGIN(stale_handle_rejected)
	PipelineRegistry registry(4, 0);

	PipelineHandle handle = registry.acquire(1, KIB);
	registry.release(handle);
	registry.enforce_budget();
	registry.set_completed_serial(PipelineQueue::Graphics, 0);
	CHECK_EQ(drain_destroyable(registry), (uint32_t)1);
	CHECK(!registry.is_valid(handle));

	PipelineInfo info;
	CHECK(!registry.describe(handle, info));
	CHECK(!registry.release(handle));
	CHECK(!registry.begin_use(handle));
	CHECK(!registry.end_use(handle, PipelineQueue::Graphics, 1));
	CHECK(!registry.mark_use(handle, PipelineQueue::Graphics, 1));
	CHECK(!registry.destroy(handle));
TEST_END()

// ---------------------------------------------------------------------------
// Budget and LRU

TEST_BEGIN(no_eviction_below_or_at_budget)
	PipelineRegistry registry(8, 3 * KIB);
	registry.acquire(1, KIB);
	registry.acquire(2, KIB);
	registry.acquire(3, KIB);

	CHECK_EQ(registry.enforce_budget(), (uint32_t)0);
	CHECK_EQ(registry.get_stats().eviction_candidates, (uint64_t)0);
	CHECK_EQ(registry.get_stats().resident_bytes, 3 * KIB);

	registry.set_budget(4 * KIB);
	CHECK_EQ(registry.enforce_budget(), (uint32_t)0);
	CHECK_EQ(registry.get_stats().resident_count, (uint64_t)3);
TEST_END()

TEST_BEGIN(lru_evicts_least_recently_used)
	PipelineRegistry registry(8, 2 * KIB);
	PipelineHandle a = registry.acquire(1, KIB);
	PipelineHandle b = registry.acquire(2, KIB);

	CHECK_EQ(registry.enforce_budget(), (uint32_t)0);

	// Touch 1 so 2 becomes the least recently used entry.
	registry.acquire(1, KIB);

	registry.acquire(3, KIB);
	CHECK_EQ(registry.enforce_budget(), (uint32_t)1);

	PipelineStats stats = registry.get_stats();
	CHECK_EQ(stats.eviction_candidates, (uint64_t)1);
	CHECK_EQ(stats.candidate_count, (uint64_t)1);
	CHECK_EQ(stats.resident_count, (uint64_t)2);
	CHECK_EQ(stats.resident_bytes, 2 * KIB);

	// Key 2 is no longer acquirable while its object drains.
	CHECK(registry.acquire(2, KIB).is_null());
	CHECK(!registry.acquire(1, KIB).is_null());
	CHECK(!registry.acquire(3, KIB).is_null());

	// Releasing the candidate host reference retires it.
	CHECK(registry.release(b));
	CHECK_EQ(registry.get_stats().retiring_count, (uint64_t)1);
	CHECK_EQ(registry.set_completed_serial(PipelineQueue::Graphics, 0), (uint32_t)1);
	CHECK_EQ(drain_destroyable(registry), (uint32_t)1);
	CHECK(!registry.is_valid(b));
	CHECK(registry.is_valid(a));
TEST_END()

TEST_BEGIN(active_host_reference_blocks_destruction)
	PipelineRegistry registry(4, 0);
	PipelineHandle handle = registry.acquire(1, KIB);
	CHECK_EQ(registry.enforce_budget(), (uint32_t)1);

	PipelineInfo info;
	CHECK(registry.describe(handle, info));
	CHECK_EQ(info.state, PipelineState::EvictionCandidate);
	CHECK_EQ(info.host_refs, (uint32_t)1);

	// No recording references, but the host reference keeps it alive.
	registry.set_completed_serial(PipelineQueue::Graphics, 100);
	CHECK_EQ(registry.get_stats().destroyable_count, (uint64_t)0);
	CHECK_EQ(drain_destroyable(registry), (uint32_t)0);

	CHECK(registry.release(handle));
	CHECK_EQ(registry.get_stats().retiring_count, (uint64_t)1);
	CHECK_EQ(registry.set_completed_serial(PipelineQueue::Graphics, 100), (uint32_t)1);
	CHECK_EQ(drain_destroyable(registry), (uint32_t)1);
	CHECK(!registry.is_valid(handle));
TEST_END()

TEST_BEGIN(recording_reference_blocks_destruction)
	PipelineRegistry registry(4, 0);
	PipelineHandle handle = registry.acquire(1, KIB);
	CHECK(registry.begin_use(handle));

	CHECK_EQ(registry.enforce_budget(), (uint32_t)1);
	CHECK(registry.release(handle));
	CHECK_EQ(registry.get_stats().retiring_count, (uint64_t)0);

	// No new recording work is accepted once the object is a candidate.
	CHECK(!registry.begin_use(handle));
	CHECK(registry.is_valid(handle));

	CHECK(registry.end_use(handle, PipelineQueue::Graphics, 7));
	CHECK_EQ(registry.get_stats().retiring_count, (uint64_t)1);

	CHECK_EQ(registry.set_completed_serial(PipelineQueue::Graphics, 6), (uint32_t)0);
	CHECK_EQ(registry.get_stats().destroyable_count, (uint64_t)0);

	CHECK_EQ(registry.set_completed_serial(PipelineQueue::Graphics, 7), (uint32_t)1);
	CHECK_EQ(registry.get_stats().destroyable_count, (uint64_t)1);
	CHECK_EQ(drain_destroyable(registry), (uint32_t)1);
TEST_END()

TEST_BEGIN(pending_submission_delays_destruction)
	PipelineRegistry registry(4, 0);
	PipelineHandle handle = registry.acquire(1, KIB);

	CHECK(registry.mark_use(handle, PipelineQueue::Graphics, 10));
	CHECK(registry.release(handle));
	CHECK_EQ(registry.enforce_budget(), (uint32_t)1);
	CHECK_EQ(registry.get_stats().retiring_count, (uint64_t)1);

	CHECK_EQ(registry.set_completed_serial(PipelineQueue::Graphics, 9), (uint32_t)0);
	CHECK_EQ(registry.get_stats().destroyable_count, (uint64_t)0);

	CHECK_EQ(registry.set_completed_serial(PipelineQueue::Graphics, 10), (uint32_t)1);
	CHECK_EQ(drain_destroyable(registry), (uint32_t)1);
TEST_END()

TEST_BEGIN(late_use_is_rejected)
	PipelineRegistry registry(4, 0);
	PipelineHandle handle = registry.acquire(1, KIB);
	CHECK(registry.mark_use(handle, PipelineQueue::Graphics, 4));
	CHECK(registry.release(handle));
	registry.enforce_budget();

	PipelineInfo info;
	CHECK(registry.describe(handle, info));
	CHECK_EQ(info.state, PipelineState::Retiring);

	CHECK(!registry.mark_use(handle, PipelineQueue::Graphics, 5));
	CHECK(!registry.begin_use(handle));
	CHECK_EQ(registry.get_stats().late_use, (uint64_t)1);
TEST_END()

// ---------------------------------------------------------------------------
// Identity, reuse and capacity

TEST_BEGIN(key_is_unique_while_retiring)
	PipelineRegistry registry(4, 0);
	PipelineHandle first = registry.acquire(1, KIB);
	const uint32_t first_generation = first.generation;
	CHECK(registry.mark_use(first, PipelineQueue::Graphics, 5));
	CHECK(registry.release(first));
	registry.enforce_budget();

	// The old object still owns the key until it is destroyed.
	CHECK(registry.acquire(1, KIB).is_null());
	CHECK_EQ(registry.get_stats().blocked_by_retirement, (uint64_t)1);

	registry.set_completed_serial(PipelineQueue::Graphics, 5);
	CHECK_EQ(drain_destroyable(registry), (uint32_t)1);

	PipelineHandle second = registry.acquire(1, KIB);
	CHECK(!second.is_null());
	CHECK_EQ(second.index, first.index);
	CHECK_EQ(second.generation, first_generation + 1);
	CHECK(!registry.is_valid(first));
	CHECK(registry.is_valid(second));
TEST_END()

TEST_BEGIN(generation_wrap_skips_zero)
	PipelineRegistry registry(2, 0, UINT32_MAX);
	PipelineHandle first = registry.acquire(1, KIB);
	CHECK_EQ(first.generation, UINT32_MAX);

	registry.release(first);
	registry.enforce_budget();
	registry.set_completed_serial(PipelineQueue::Graphics, 0);
	CHECK_EQ(drain_destroyable(registry), (uint32_t)1);

	PipelineHandle second = registry.acquire(2, KIB);
	CHECK_EQ(second.index, first.index);
	CHECK_EQ(second.generation, (uint32_t)1);
TEST_END()

TEST_BEGIN(capacity_full_fails_controlled)
	PipelineRegistry registry(2, 0);
	PipelineHandle a = registry.acquire(1, KIB);
	PipelineHandle b = registry.acquire(2, KIB);
	CHECK(!a.is_null());
	CHECK(!b.is_null());

	PipelineHandle c = registry.acquire(3, KIB);
	CHECK(c.is_null());
	CHECK_EQ(registry.get_stats().rejected_full, (uint64_t)1);

	registry.release(a);
	registry.enforce_budget();
	registry.set_completed_serial(PipelineQueue::Graphics, 0);
	CHECK_EQ(drain_destroyable(registry), (uint32_t)1);

	PipelineHandle reused = registry.acquire(3, KIB);
	CHECK(!reused.is_null());
	CHECK_EQ(reused.index, a.index);
TEST_END()

TEST_BEGIN(shutdown_drains_everything)
	const uint32_t count = 16;
	PipelineRegistry registry(count, 0);

	std::vector<PipelineHandle> handles;
	for (uint32_t i = 0; i < count; i++) {
		PipelineHandle handle = registry.acquire(i + 1, KIB);
		CHECK(!handle.is_null());
		if (i % 2 == 0) {
			CHECK(registry.begin_use(handle));
		}
		CHECK(registry.mark_use(handle, PipelineQueue::Graphics, 9));
		handles.push_back(handle);
	}

	for (PipelineHandle &handle : handles) {
		CHECK(registry.release(handle));
	}

	// Budget 0 evicts everything; recording references keep half alive until
	// the command buffers finish.
	CHECK_EQ(registry.enforce_budget(), count);
	for (uint32_t i = 0; i < count; i += 2) {
		CHECK(registry.end_use(handles[i], PipelineQueue::Graphics, 9));
	}

	CHECK_EQ(registry.set_completed_serial(PipelineQueue::Graphics, 9), count);
	CHECK_EQ(drain_destroyable(registry), count);

	const PipelineStats stats = registry.get_stats();
	CHECK_EQ(stats.resident_count, (uint64_t)0);
	CHECK_EQ(stats.candidate_count, (uint64_t)0);
	CHECK_EQ(stats.retiring_count, (uint64_t)0);
	CHECK_EQ(stats.destroyable_count, (uint64_t)0);
	CHECK_EQ(stats.resident_bytes, (uint64_t)0);
	CHECK_EQ(stats.pending_bytes, (uint64_t)0);
	CHECK_EQ(stats.destroyed, (uint64_t)count);
	CHECK_EQ(stats.releases, (uint64_t)count);
	CHECK_EQ(stats.retired, (uint64_t)count);
TEST_END()

TEST_BEGIN(stats_reflect_lifecycle)
	PipelineRegistry registry(8, KIB);
	PipelineHandle a = registry.acquire(1, KIB);
	PipelineHandle b = registry.acquire(2, KIB);
	registry.acquire(1, KIB); // hit

	CHECK_EQ(registry.enforce_budget(), (uint32_t)1);
	CHECK(registry.mark_use(b, PipelineQueue::Graphics, 3));

	PipelineStats stats = registry.get_stats();
	CHECK_EQ(stats.acquisitions, (uint64_t)3);
	CHECK_EQ(stats.hits, (uint64_t)1);
	CHECK_EQ(stats.misses, (uint64_t)2);
	CHECK_EQ(stats.peak_resident_bytes, 2 * KIB);
	CHECK_EQ(stats.pending_bytes, KIB);
	CHECK_EQ(stats.candidate_count + stats.retiring_count, (uint64_t)1);

	registry.release(a);
	registry.release(a);
	registry.release(b);
	registry.set_budget(0);
	registry.enforce_budget(); // `a` now has zero host refs and is evictable.
	registry.set_completed_serial(PipelineQueue::Graphics, 3);
	drain_destroyable(registry);

	stats = registry.get_stats();
	CHECK_EQ(stats.destroyed, (uint64_t)2);
	CHECK_EQ(stats.resident_bytes, (uint64_t)0);
	CHECK_EQ(stats.pending_bytes, (uint64_t)0);
TEST_END()

TEST_BEGIN(multi_queue_waits_for_all)
	PipelineRegistry registry(4, 0);
	PipelineHandle handle = registry.acquire(1, KIB);

	CHECK(registry.begin_use(handle));
	CHECK(registry.end_use(handle, PipelineQueue::Graphics, 10));
	CHECK(registry.begin_use(handle));
	CHECK(registry.end_use(handle, PipelineQueue::Compute, 20));

	PipelineInfo info;
	CHECK(registry.describe(handle, info));
	CHECK(info.usage.is_used(PipelineQueue::Graphics));
	CHECK(info.usage.is_used(PipelineQueue::Compute));
	CHECK_EQ(info.usage.get(PipelineQueue::Graphics), (uint64_t)10);
	CHECK_EQ(info.usage.get(PipelineQueue::Compute), (uint64_t)20);
	CHECK_EQ(info.last_use_serial, (uint64_t)20);

	CHECK(registry.release(handle));
	CHECK_EQ(registry.enforce_budget(), (uint32_t)1);
	CHECK_EQ(registry.get_stats().retiring_count, (uint64_t)1);

	// Graphics finishing is not enough.
	CHECK_EQ(registry.set_completed_serial(PipelineQueue::Graphics, 10), (uint32_t)0);
	CHECK_EQ(registry.get_stats().destroyable_count, (uint64_t)0);

	// Compute must reach 20 as well.
	CHECK_EQ(registry.set_completed_serial(PipelineQueue::Compute, 19), (uint32_t)0);
	CHECK_EQ(registry.set_completed_serial(PipelineQueue::Compute, 20), (uint32_t)1);
	CHECK_EQ(drain_destroyable(registry), (uint32_t)1);
TEST_END()

TEST_BEGIN(queues_complete_out_of_order)
	PipelineRegistry registry(8, 0);
	PipelineHandle graphics_only = registry.acquire(1, KIB);
	PipelineHandle compute_only = registry.acquire(2, KIB);

	CHECK(registry.mark_use(graphics_only, PipelineQueue::Graphics, 5));
	CHECK(registry.mark_use(compute_only, PipelineQueue::Compute, 5));
	CHECK(registry.release(graphics_only));
	CHECK(registry.release(compute_only));
	CHECK_EQ(registry.enforce_budget(), (uint32_t)2);

	// Compute finishes first: only the compute-only object may be destroyed.
	CHECK_EQ(registry.set_completed_serial(PipelineQueue::Compute, 5), (uint32_t)1);
	CHECK_EQ(registry.get_stats().destroyable_count, (uint64_t)1);
	CHECK_EQ(drain_destroyable(registry), (uint32_t)1);
	CHECK(registry.is_valid(graphics_only));

	CHECK_EQ(registry.set_completed_serial(PipelineQueue::Graphics, 4), (uint32_t)0);
	CHECK_EQ(registry.set_completed_serial(PipelineQueue::Graphics, 5), (uint32_t)1);
	CHECK_EQ(drain_destroyable(registry), (uint32_t)1);
	CHECK(!registry.is_valid(graphics_only));
TEST_END()

TEST_BEGIN(abort_use_drops_protection)
	PipelineRegistry registry(4, 0);

	// A never-submitted recording does not create a completion dependency.
	PipelineHandle handle = registry.acquire(1, KIB);
	CHECK(registry.begin_use(handle));
	CHECK(registry.abort_use(handle));
	CHECK_EQ(registry.get_stats().aborted_recordings, (uint64_t)1);
	CHECK(!registry.abort_use(handle)); // no recording reference left

	CHECK(registry.release(handle));
	CHECK_EQ(registry.enforce_budget(), (uint32_t)1);
	CHECK_EQ(registry.set_completed_serial(PipelineQueue::Graphics, 0), (uint32_t)1);
	CHECK_EQ(drain_destroyable(registry), (uint32_t)1);

	// An aborted submission must not erase the dependency of an older,
	// successful submission on the same object.
	PipelineHandle used = registry.acquire(2, KIB);
	CHECK(registry.mark_use(used, PipelineQueue::Graphics, 7));
	CHECK(registry.begin_use(used));
	CHECK(registry.abort_use(used));
	CHECK(registry.release(used));
	CHECK_EQ(registry.enforce_budget(), (uint32_t)1);
	CHECK_EQ(registry.set_completed_serial(PipelineQueue::Graphics, 6), (uint32_t)0);
	CHECK_EQ(registry.set_completed_serial(PipelineQueue::Graphics, 7), (uint32_t)1);
	CHECK_EQ(drain_destroyable(registry), (uint32_t)1);
TEST_END()

TEST_BEGIN(zero_and_invalid_serials_rejected)
	PipelineRegistry registry(4, 0);
	PipelineHandle handle = registry.acquire(1, KIB);

	CHECK(!registry.mark_use(handle, PipelineQueue::Graphics, 0));
	CHECK(!registry.end_use(handle, PipelineQueue::Graphics, 0));
	CHECK_EQ(registry.get_stats().invalid_serials, (uint64_t)2);

	// The failed end_use must not consume the recording reference.
	CHECK(registry.begin_use(handle));
	CHECK(!registry.end_use(handle, PipelineQueue::Graphics, 0));
	CHECK(registry.abort_use(handle));

	// The invalid call must not have recorded any queue usage.
	PipelineInfo info;
	CHECK(registry.describe(handle, info));
	CHECK_EQ(info.usage.mask, (uint8_t)0);

	// Completed serials are monotonic per queue.
	CHECK(registry.mark_use(handle, PipelineQueue::Compute, 9));
	CHECK(registry.release(handle));
	CHECK_EQ(registry.enforce_budget(), (uint32_t)1);
	CHECK_EQ(registry.set_completed_serial(PipelineQueue::Compute, 9), (uint32_t)1);
	CHECK_EQ(registry.set_completed_serial(PipelineQueue::Compute, 3), (uint32_t)0);
	CHECK_EQ(registry.get_stats().completed_serials[Kaizen::pipeline_queue_index(PipelineQueue::Compute)], (uint64_t)9);
	CHECK_EQ(drain_destroyable(registry), (uint32_t)1);
TEST_END()

TEST_BEGIN(retire_now_requires_no_references)
	PipelineRegistry registry(4, 0);
	PipelineHandle handle = registry.acquire(1, KIB);

	// A host reference means the caller may still hand it out.
	CHECK(!registry.retire_now(handle));
	// A recording reference means a command buffer may still use it.
	CHECK(registry.begin_use(handle));
	CHECK(!registry.retire_now(handle));
	CHECK(registry.abort_use(handle));
	CHECK(registry.release(handle));

	CHECK(registry.retire_now(handle));
	CHECK_EQ(registry.get_stats().requested_retirements, (uint64_t)1);
	CHECK_EQ(registry.get_stats().retiring_count, (uint64_t)1);
	CHECK_EQ(registry.get_stats().resident_count, (uint64_t)0);
	CHECK_EQ(registry.get_stats().pending_bytes, (uint64_t)KIB);
	CHECK(!registry.retire_now(handle)); // already retiring

	// No queue ever used it, so there is nothing to wait for.
	CHECK_EQ(registry.set_completed_serial(PipelineQueue::Graphics, 0), (uint32_t)1);
	CHECK_EQ(drain_destroyable(registry), (uint32_t)1);
	CHECK_EQ(registry.get_stats().resident_bytes, (uint64_t)0);
	CHECK_EQ(registry.get_stats().pending_bytes, (uint64_t)0);
TEST_END()

TEST_BEGIN(retire_now_waits_for_recorded_use)
	PipelineRegistry registry(4, 0);
	PipelineHandle handle = registry.acquire(1, KIB);
	CHECK(registry.mark_use(handle, PipelineQueue::Graphics, 12));
	CHECK(registry.mark_use(handle, PipelineQueue::Compute, 4));
	CHECK(registry.release(handle));
	CHECK(registry.retire_now(handle));

	CHECK_EQ(registry.set_completed_serial(PipelineQueue::Graphics, 12), (uint32_t)0);
	CHECK_EQ(registry.set_completed_serial(PipelineQueue::Compute, 3), (uint32_t)0);
	CHECK_EQ(registry.set_completed_serial(PipelineQueue::Compute, 4), (uint32_t)1);
	CHECK_EQ(drain_destroyable(registry), (uint32_t)1);
TEST_END()

TEST_BEGIN(retire_now_from_candidate)
	PipelineRegistry registry(4, 0);
	PipelineHandle handle = registry.acquire(1, KIB);
	CHECK(registry.release(handle));
	CHECK_EQ(registry.enforce_budget(), (uint32_t)1); // budget 0 evicts now
	CHECK_EQ(registry.get_stats().retiring_count, (uint64_t)1);
	// A candidate that already retired cannot be retired twice.
	CHECK(!registry.retire_now(handle));
	CHECK_EQ(registry.get_stats().requested_retirements, (uint64_t)0);
	CHECK_EQ(registry.set_completed_serial(PipelineQueue::Graphics, 0), (uint32_t)1);
	CHECK_EQ(drain_destroyable(registry), (uint32_t)1);
TEST_END()

TEST_BEGIN(submission_groups_and_fences)
	Kaizen::PipelineSubmissionTracker tracker(2);

	// First submission on the main queue has no fence: it opens a group.
	CHECK_EQ(tracker.note_submission(0, false), (uint64_t)1);
	CHECK_EQ(tracker.note_submission(0, false), (uint64_t)1);
	CHECK_EQ(tracker.last_serial(0), (uint64_t)1);

	// The last submission of the frame carries the fence and closes the group.
	CHECK_EQ(tracker.note_submission(0, true), (uint64_t)1);
	CHECK_EQ(tracker.last_serial(0), (uint64_t)1);

	// The next submission opens a new group with the next serial.
	CHECK_EQ(tracker.note_submission(0, true), (uint64_t)2);
	CHECK_EQ(tracker.last_serial(0), (uint64_t)2);

	// Queues have independent serial spaces.
	CHECK_EQ(tracker.note_submission(1, true), (uint64_t)1);
	CHECK_EQ(tracker.last_serial(1), (uint64_t)1);
	CHECK_EQ(tracker.last_serial(0), (uint64_t)2);

	// Confirmation is monotonic and per queue.
	tracker.confirm(0, 1);
	CHECK_EQ(tracker.get_confirmed()[0], (uint64_t)1);
	tracker.confirm(0, 0);
	CHECK_EQ(tracker.get_confirmed()[0], (uint64_t)1);
	CHECK_EQ(tracker.get_confirmed()[1], (uint64_t)0);

	tracker.confirm(1, 1);
	CHECK_EQ(tracker.get_confirmed()[1], (uint64_t)1);

	// An open group is confirmed by the next fenced submission, not by the
	// confirmation of an earlier group.
	CHECK_EQ(tracker.note_submission(0, false), (uint64_t)3);
	CHECK_EQ(tracker.last_serial(0), (uint64_t)3);
	CHECK_EQ(tracker.get_confirmed()[0], (uint64_t)1);
	CHECK_EQ(tracker.note_submission(0, true), (uint64_t)3);

	// confirm_all() is the device-idle escape hatch.
	tracker.confirm_all();
	CHECK_EQ(tracker.get_confirmed()[0], (uint64_t)3);
	CHECK_EQ(tracker.get_confirmed()[1], (uint64_t)1);

	// Out-of-range queues are ignored rather than corrupting state.
	CHECK_EQ(tracker.note_submission(5, true), (uint64_t)0);
	CHECK_EQ(tracker.last_serial(5), (uint64_t)0);
	tracker.confirm(5, 1);
	CHECK_EQ(tracker.get_confirmed()[1], (uint64_t)1);
TEST_END()

// ---------------------------------------------------------------------------
// Helper structures

TEST_BEGIN(lru_basics)
	PipelineLRU lru(4);
	CHECK(lru.is_empty());
	CHECK_EQ(lru.peek_lru(), PipelineLRU::NO_INDEX);

	lru.touch(0);
	lru.touch(1);
	lru.touch(2);
	CHECK_EQ(lru.size(), (uint32_t)3);
	CHECK_EQ(lru.peek_lru(), (uint32_t)0);

	lru.touch(0);
	CHECK_EQ(lru.peek_lru(), (uint32_t)1);
	CHECK(lru.contains(0));
	CHECK(lru.contains(2));

	lru.remove(2);
	CHECK(!lru.contains(2));
	CHECK_EQ(lru.size(), (uint32_t)2);

	CHECK_EQ(lru.pop_lru(), (uint32_t)1);
	CHECK_EQ(lru.pop_lru(), (uint32_t)0);
	CHECK_EQ(lru.pop_lru(), PipelineLRU::NO_INDEX);
	CHECK(lru.is_empty());
TEST_END()

TEST_BEGIN(retirement_queue_basics)
	PipelineRetirementQueue queue(8);

	Kaizen::PipelineQueueUsage a;
	a.record(PipelineQueue::Graphics, 5);
	Kaizen::PipelineQueueUsage b;
	b.record(PipelineQueue::Graphics, 2);
	Kaizen::PipelineQueueUsage c;
	c.record(PipelineQueue::Graphics, 9);
	c.record(PipelineQueue::Compute, 3);

	queue.push(0, 10, a);
	queue.push(1, 11, b);
	queue.push(2, 12, c);

	const uint64_t completed_graphics_only[Kaizen::PIPELINE_QUEUE_COUNT] = { 4, 0, 0 };
	PipelineRetirementQueue::Entry out[4];
	CHECK_EQ(queue.collect_ready(completed_graphics_only, out, 4), (uint32_t)1);
	CHECK_EQ(out[0].index, (uint32_t)1);
	CHECK_EQ(queue.size(), (uint32_t)2);

	// c used graphics at 9 and compute at 3. Completing graphics is not enough.
	const uint64_t graphics_done[Kaizen::PIPELINE_QUEUE_COUNT] = { 9, 0, 0 };
	CHECK_EQ(queue.collect_ready(graphics_done, out, 4), (uint32_t)1);
	CHECK_EQ(out[0].index, (uint32_t)0);
	CHECK_EQ(queue.size(), (uint32_t)1);

	const uint64_t both_done[Kaizen::PIPELINE_QUEUE_COUNT] = { 9, 3, 0 };
	CHECK_EQ(queue.collect_ready(both_done, out, 4), (uint32_t)1);
	CHECK_EQ(out[0].index, (uint32_t)2);
	CHECK(queue.is_empty());

	// A limited collection must not drop the entries it could not return.
	Kaizen::PipelineQueueUsage one;
	one.record(PipelineQueue::Graphics, 1);
	queue.push(3, 13, one);
	queue.push(4, 14, one);
	queue.push(5, 15, one);
	const uint64_t serial_one[Kaizen::PIPELINE_QUEUE_COUNT] = { 1, 0, 0 };
	CHECK_EQ(queue.collect_ready(serial_one, out, 2), (uint32_t)2);
	CHECK_EQ(queue.size(), (uint32_t)1);
	CHECK_EQ(queue.collect_ready(serial_one, out, 2), (uint32_t)1);
	CHECK_EQ(out[0].index, (uint32_t)5);
	CHECK(queue.is_empty());
TEST_END()

// ---------------------------------------------------------------------------
// Concurrency

TEST_BEGIN(concurrent_eviction_and_destruction)
	const uint32_t worker_count = 4;
	const uint32_t iterations = 20000;
	const uint32_t key_count = 32;
	const uint64_t budget = 4 * KIB;

	PipelineRegistry registry(64, budget);
	std::atomic<uint64_t> serial{ 0 };
	std::atomic<uint32_t> ready{ 0 };
	std::atomic<uint32_t> go{ 0 };
	std::atomic<uint32_t> acquired{ 0 };
	std::atomic<uint32_t> rejected{ 0 };
	std::atomic<uint32_t> unsafe_states{ 0 };

	std::vector<std::thread> workers;
	workers.reserve(worker_count);
	for (uint32_t t = 0; t < worker_count; t++) {
		workers.emplace_back([&, t]() {
			ready.fetch_add(1);
			while (go.load(std::memory_order_acquire) == 0) {
			}

			for (uint32_t i = 0; i < iterations; i++) {
				const uint64_t key = (t * 7919u + i) % key_count;
				PipelineHandle handle = registry.acquire(key + 1, KIB);
				if (handle.is_null()) {
					rejected.fetch_add(1);
					continue;
				}
				acquired.fetch_add(1);

				// A returned handle must never already belong to an object
				// that is committed to destruction. It may become a candidate
				// immediately (host_refs keeps it alive), but never Retiring
				// or Destroyable.
				PipelineInfo info;
				if (!registry.describe(handle, info) || info.host_refs == 0 ||
						info.state == PipelineState::Free ||
						info.state == PipelineState::Retiring ||
						info.state == PipelineState::Destroyable) {
					unsafe_states.fetch_add(1);
				}

				const uint64_t use_serial = serial.fetch_add(1, std::memory_order_acq_rel) + 1;
				if (registry.begin_use(handle)) {
					// Simulate a command buffer that is submitted later.
					registry.mark_use(handle, PipelineQueue::Graphics, use_serial);
					registry.end_use(handle, PipelineQueue::Graphics, use_serial);
				}
				registry.release(handle);
				registry.enforce_budget();
			}
		});
	}

	// Completion thread: advances the backend serial and reclaims objects.
	std::thread collector([&]() {
		ready.fetch_add(1);
		while (go.load(std::memory_order_acquire) == 0) {
		}
		for (uint32_t i = 0; i < iterations * 2; i++) {
			registry.set_completed_serial(PipelineQueue::Graphics, serial.load(std::memory_order_acquire));
			drain_destroyable(registry);
		}
	});

	while (ready.load() < worker_count + 1) {
	}
	go.store(1, std::memory_order_release);

	for (std::thread &worker : workers) {
		worker.join();
	}
	collector.join();

	CHECK(acquired.load() > 0);
	CHECK(rejected.load() > 0); // The budget is deliberately too small.
	CHECK_EQ(unsafe_states.load(), (uint32_t)0);

	registry.set_budget(0);
	registry.enforce_budget();
	registry.set_completed_serial(PipelineQueue::Graphics, UINT64_MAX);
	for (uint32_t i = 0; i < 8; i++) {
		drain_destroyable(registry);
	}

	const PipelineStats stats = registry.get_stats();
	CHECK_EQ(stats.resident_count, (uint64_t)0);
	CHECK_EQ(stats.candidate_count, (uint64_t)0);
	CHECK_EQ(stats.retiring_count, (uint64_t)0);
	CHECK_EQ(stats.destroyable_count, (uint64_t)0);
	CHECK_EQ(stats.resident_bytes, (uint64_t)0);
	CHECK_EQ(stats.pending_bytes, (uint64_t)0);
	CHECK_EQ(stats.late_use, (uint64_t)0);
	CHECK_EQ(stats.destroyed, stats.retired);
	CHECK(stats.hits > 0);
TEST_END()

// ---------------------------------------------------------------------------
// Benchmark

namespace bench {

struct Latency {
	double mean_ns = 0;
	double p50_ns = 0;
	double p95_ns = 0;
	double p99_ns = 0;
	double ops_per_second = 0;
};

double percentile_sorted(const std::vector<double> &p_values, double p) {
	if (p_values.empty()) {
		return 0.0;
	}
	const size_t index = (size_t)((p / 100.0) * (p_values.size() - 1));
	return p_values[index];
}

void print(const char *p_label, const Latency &p_latency) {
	std::printf("bench %-30s mean=%7.1f ns  p50=%7.1f  p95=%7.1f  p99=%7.1f ns  %7.2f Mops/s\n",
			p_label, p_latency.mean_ns, p_latency.p50_ns, p_latency.p95_ns, p_latency.p99_ns,
			p_latency.ops_per_second / 1e6);
}

// Times `p_ops` operations grouped in batches of `p_batch`; percentiles are
// per-operation values within each batch.
template <typename Fn>
Latency measure(uint32_t p_ops, uint32_t p_batch, Fn &&p_fn) {
	std::vector<double> per_batch;
	per_batch.reserve(p_ops / p_batch + 2);

	uint32_t done = 0;
	double total_ns = 0;
	while (done < p_ops) {
		const uint32_t n = std::min(p_batch, p_ops - done);
		const auto start = std::chrono::steady_clock::now();
		for (uint32_t i = 0; i < n; i++) {
			p_fn(done + i);
		}
		const auto end = std::chrono::steady_clock::now();

		const double ns = std::chrono::duration<double, std::nano>(end - start).count();
		total_ns += ns;
		per_batch.push_back(ns / n);
		done += n;
	}

	std::sort(per_batch.begin(), per_batch.end());

	Latency out;
	out.mean_ns = total_ns / p_ops;
	out.p50_ns = percentile_sorted(per_batch, 50);
	out.p95_ns = percentile_sorted(per_batch, 95);
	out.p99_ns = percentile_sorted(per_batch, 99);
	out.ops_per_second = 1e9 / out.mean_ns;
	return out;
}

double throughput(uint32_t p_threads, uint32_t p_per_thread) {
	PipelineRegistry registry(64, 4096 * KIB);
	registry.acquire(1, KIB);
	std::atomic<uint32_t> ready{ 0 };
	std::atomic<bool> go{ false };

	std::vector<std::thread> workers;
	workers.reserve(p_threads);
	for (uint32_t t = 0; t < p_threads; t++) {
		workers.emplace_back([&]() {
			ready.fetch_add(1);
			while (!go.load(std::memory_order_acquire)) {
			}
			for (uint32_t i = 0; i < p_per_thread; i++) {
				PipelineHandle handle = registry.acquire(1, KIB);
				registry.release(handle);
			}
		});
	}

	while (ready.load() < p_threads) {
	}
	const auto start = std::chrono::steady_clock::now();
	go.store(true, std::memory_order_release);
	for (std::thread &worker : workers) {
		worker.join();
	}
	const auto end = std::chrono::steady_clock::now();

	const double seconds = std::chrono::duration<double>(end - start).count();
	return ((double)p_threads * p_per_thread) / seconds / 1e6;
}

} // namespace bench

void run_benchmark() {
	const uint32_t ops = 1000000;

	{
		// One key, always a cache hit.
		PipelineRegistry registry(64, 64 * KIB);
		PipelineHandle handle = registry.acquire(1, KIB);
		bench::print("hit (acquire+release)", bench::measure(ops, 64, [&](uint32_t) {
			PipelineHandle h = registry.acquire(1, KIB);
			registry.release(h);
		}));
		registry.release(handle);
	}

	{
		// Hit with 4 095 other resident objects, to expose free-list or map
		// degradation at capacity.
		PipelineRegistry registry(4096, (uint64_t)4096 * KIB);
		for (uint32_t i = 0; i < 4095; i++) {
			registry.acquire(i + 2, KIB);
		}
		PipelineHandle handle = registry.acquire(1, KIB);
		bench::print("hit, near capacity", bench::measure(ops, 64, [&](uint32_t) {
			PipelineHandle h = registry.acquire(1, KIB);
			registry.release(h);
		}));
		registry.release(handle);
	}

	{
		// Unique keys: miss, create, release; budget high enough to keep them.
		PipelineRegistry registry(ops + 2, (uint64_t)(ops + 2) * KIB);
		bench::print("miss (acquire+release)", bench::measure(ops, 64, [&](uint32_t i) {
			PipelineHandle h = registry.acquire(i + 1, KIB);
			registry.release(h);
		}));
	}

	{
		// Eviction cycle: acquire, release, evict one, complete, destroy.
		PipelineRegistry registry(4, 0);
		uint64_t serial = 0;
		bench::print("evict+retire+destroy", bench::measure(ops, 64, [&](uint32_t) {
			PipelineHandle h = registry.acquire(1, KIB);
			registry.mark_use(h, PipelineQueue::Graphics, ++serial);
			registry.release(h);
			registry.enforce_budget();
			registry.set_completed_serial(PipelineQueue::Graphics, serial);
			drain_destroyable(registry);
		}));
	}

	{
		// Full path: acquire, record, submit, release, evict, retire, destroy.
		PipelineRegistry registry(4, 0);
		uint64_t serial = 0;
		bench::print("full lifetime", bench::measure(ops, 64, [&](uint32_t) {
			PipelineHandle h = registry.acquire(1, KIB);
			registry.begin_use(h);
			registry.end_use(h, PipelineQueue::Graphics, ++serial);
			registry.release(h);
			registry.enforce_budget();
			registry.set_completed_serial(PipelineQueue::Graphics, serial);
			drain_destroyable(registry);
		}));
	}

	{
		// Same full path, but the object is used by graphics and compute.
		PipelineRegistry registry(4, 0);
		uint64_t serial = 0;
		bench::print("multi-queue lifetime", bench::measure(ops, 64, [&](uint32_t) {
			PipelineHandle h = registry.acquire(1, KIB);
			registry.begin_use(h);
			registry.end_use(h, PipelineQueue::Graphics, ++serial);
			registry.begin_use(h);
			registry.end_use(h, PipelineQueue::Compute, serial);
			registry.release(h);
			registry.enforce_budget();
			registry.set_completed_serial(PipelineQueue::Graphics, serial);
			registry.set_completed_serial(PipelineQueue::Compute, serial);
			drain_destroyable(registry);
		}));
	}

	std::printf("bench throughput (acquire+release, shared registry):\n");
	for (uint32_t threads : { 1u, 2u, 4u, 8u }) {
		std::printf("bench threads=%u %7.2f Mops/s\n", threads, bench::throughput(threads, 50000));
	}

	const uint32_t slot_bytes = PipelineRegistry::get_slot_size();
	std::printf("bench slot footprint: %u B slot + 4 B free-list + 8 B LRU = %u B/slot (hash map excluded)\n",
			slot_bytes, slot_bytes + 12);
}

} // namespace

int main(int argc, char **argv) {
	const bool bench = argc > 1 && std::strcmp(argv[1], "--bench") == 0;

	pipeline_key_uses_complete_canonical_identity();
	typed_registry_shares_exact_keys_and_blocks_retirement_reacquire();
	owner_shutdown_retires_entries_with_live_logical_references();
	acquire_hit_and_miss();
	release_rejects_invalid_sequences();
	stale_handle_rejected();
	no_eviction_below_or_at_budget();
	lru_evicts_least_recently_used();
	active_host_reference_blocks_destruction();
	recording_reference_blocks_destruction();
	pending_submission_delays_destruction();
	late_use_is_rejected();
	key_is_unique_while_retiring();
	generation_wrap_skips_zero();
	capacity_full_fails_controlled();
	shutdown_drains_everything();
	stats_reflect_lifecycle();
	multi_queue_waits_for_all();
	queues_complete_out_of_order();
	abort_use_drops_protection();
	zero_and_invalid_serials_rejected();
	retire_now_requires_no_references();
	retire_now_waits_for_recorded_use();
	retire_now_from_candidate();
	submission_groups_and_fences();
	lru_basics();
	retirement_queue_basics();
	concurrent_eviction_and_destruction();

	if (bench) {
		run_benchmark();
	}

	std::printf("%d checks, %d failure(s)\n", checks, failures);
	return failures == 0 ? 0 : 1;
}
