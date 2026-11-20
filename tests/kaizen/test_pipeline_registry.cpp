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
#include "kaizen/rendering/pipeline_cache/pipeline_registry.h"
#include "kaizen/rendering/pipeline_cache/pipeline_retirement.h"

#include <algorithm>
#include <atomic>
#include <chrono>
#include <cstdint>
#include <cstdio>
#include <cstring>
#include <thread>
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
using Kaizen::PipelineLRU;
using Kaizen::PipelineRegistry;
using Kaizen::PipelineRetirementQueue;
using Kaizen::PipelineState;
using Kaizen::PipelineStats;

constexpr uint64_t KIB = 1024;

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
	CHECK(!registry.end_use(handle, 1)); // no recording reference yet
	CHECK(registry.begin_use(handle));
	CHECK(registry.end_use(handle, 1));
	CHECK(!registry.end_use(handle, 2)); // already ended
	CHECK(!registry.begin_use(PipelineHandle()));
TEST_END()

TEST_BEGIN(stale_handle_rejected)
	PipelineRegistry registry(4, 0);

	PipelineHandle handle = registry.acquire(1, KIB);
	registry.release(handle);
	registry.enforce_budget();
	registry.set_completed_serial(0);
	CHECK_EQ(drain_destroyable(registry), (uint32_t)1);
	CHECK(!registry.is_valid(handle));

	PipelineInfo info;
	CHECK(!registry.describe(handle, info));
	CHECK(!registry.release(handle));
	CHECK(!registry.begin_use(handle));
	CHECK(!registry.end_use(handle, 1));
	CHECK(!registry.mark_use(handle, 1));
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
	CHECK_EQ(registry.set_completed_serial(0), (uint32_t)1);
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
	registry.set_completed_serial(100);
	CHECK_EQ(registry.get_stats().destroyable_count, (uint64_t)0);
	CHECK_EQ(drain_destroyable(registry), (uint32_t)0);

	CHECK(registry.release(handle));
	CHECK_EQ(registry.get_stats().retiring_count, (uint64_t)1);
	CHECK_EQ(registry.set_completed_serial(100), (uint32_t)1);
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

	CHECK(registry.end_use(handle, 7));
	CHECK_EQ(registry.get_stats().retiring_count, (uint64_t)1);

	CHECK_EQ(registry.set_completed_serial(6), (uint32_t)0);
	CHECK_EQ(registry.get_stats().destroyable_count, (uint64_t)0);

	CHECK_EQ(registry.set_completed_serial(7), (uint32_t)1);
	CHECK_EQ(registry.get_stats().destroyable_count, (uint64_t)1);
	CHECK_EQ(drain_destroyable(registry), (uint32_t)1);
TEST_END()

TEST_BEGIN(pending_submission_delays_destruction)
	PipelineRegistry registry(4, 0);
	PipelineHandle handle = registry.acquire(1, KIB);

	CHECK(registry.mark_use(handle, 10));
	CHECK(registry.release(handle));
	CHECK_EQ(registry.enforce_budget(), (uint32_t)1);
	CHECK_EQ(registry.get_stats().retiring_count, (uint64_t)1);

	CHECK_EQ(registry.set_completed_serial(9), (uint32_t)0);
	CHECK_EQ(registry.get_stats().destroyable_count, (uint64_t)0);

	CHECK_EQ(registry.set_completed_serial(10), (uint32_t)1);
	CHECK_EQ(drain_destroyable(registry), (uint32_t)1);
TEST_END()

TEST_BEGIN(late_use_is_rejected)
	PipelineRegistry registry(4, 0);
	PipelineHandle handle = registry.acquire(1, KIB);
	CHECK(registry.mark_use(handle, 4));
	CHECK(registry.release(handle));
	registry.enforce_budget();

	PipelineInfo info;
	CHECK(registry.describe(handle, info));
	CHECK_EQ(info.state, PipelineState::Retiring);

	CHECK(!registry.mark_use(handle, 5));
	CHECK(!registry.begin_use(handle));
	CHECK_EQ(registry.get_stats().late_use, (uint64_t)1);
TEST_END()

// ---------------------------------------------------------------------------
// Identity, reuse and capacity

TEST_BEGIN(key_is_unique_while_retiring)
	PipelineRegistry registry(4, 0);
	PipelineHandle first = registry.acquire(1, KIB);
	const uint32_t first_generation = first.generation;
	CHECK(registry.mark_use(first, 5));
	CHECK(registry.release(first));
	registry.enforce_budget();

	// The old object still owns the key until it is destroyed.
	CHECK(registry.acquire(1, KIB).is_null());
	CHECK_EQ(registry.get_stats().blocked_by_retirement, (uint64_t)1);

	registry.set_completed_serial(5);
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
	registry.set_completed_serial(0);
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
	registry.set_completed_serial(0);
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
		CHECK(registry.mark_use(handle, 9));
		handles.push_back(handle);
	}

	for (PipelineHandle &handle : handles) {
		CHECK(registry.release(handle));
	}

	// Budget 0 evicts everything; recording references keep half alive until
	// the command buffers finish.
	CHECK_EQ(registry.enforce_budget(), count);
	for (uint32_t i = 0; i < count; i += 2) {
		CHECK(registry.end_use(handles[i], 9));
	}

	CHECK_EQ(registry.set_completed_serial(9), count);
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
	CHECK(registry.mark_use(b, 3));

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
	registry.set_completed_serial(3);
	drain_destroyable(registry);

	stats = registry.get_stats();
	CHECK_EQ(stats.destroyed, (uint64_t)2);
	CHECK_EQ(stats.resident_bytes, (uint64_t)0);
	CHECK_EQ(stats.pending_bytes, (uint64_t)0);
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
	queue.push(0, 10, 5);
	queue.push(1, 11, 2);
	queue.push(2, 12, 9);

	CHECK_EQ(queue.get_oldest_serial(), (uint64_t)2);

	PipelineRetirementQueue::Entry out[4];
	CHECK_EQ(queue.collect_ready(4, out, 4), (uint32_t)1);
	CHECK_EQ(out[0].index, (uint32_t)1);
	CHECK_EQ(queue.size(), (uint32_t)2);

	// A limited collection must not drop the entries it could not return.
	CHECK_EQ(queue.collect_ready(9, out, 1), (uint32_t)1);
	CHECK_EQ(out[0].index, (uint32_t)0);
	CHECK_EQ(queue.size(), (uint32_t)1);

	CHECK_EQ(queue.collect_ready(9, out, 4), (uint32_t)1);
	CHECK_EQ(out[0].index, (uint32_t)2);
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

				const uint64_t use_serial = serial.fetch_add(1, std::memory_order_acq_rel) + 1;
				if (registry.begin_use(handle)) {
					// Simulate a command buffer that is submitted later.
					registry.mark_use(handle, use_serial);
					registry.end_use(handle, use_serial);
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
			registry.set_completed_serial(serial.load(std::memory_order_acquire));
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

	registry.set_budget(0);
	registry.enforce_budget();
	registry.set_completed_serial(UINT64_MAX);
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
			registry.mark_use(h, ++serial);
			registry.release(h);
			registry.enforce_budget();
			registry.set_completed_serial(serial);
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
			registry.end_use(h, ++serial);
			registry.release(h);
			registry.enforce_budget();
			registry.set_completed_serial(serial);
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
	lru_basics();
	retirement_queue_basics();
	concurrent_eviction_and_destruction();

	if (bench) {
		run_benchmark();
	}

	std::printf("%d checks, %d failure(s)\n", checks, failures);
	return failures == 0 ? 0 : 1;
}
