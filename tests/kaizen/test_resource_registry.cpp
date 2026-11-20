/**************************************************************************/
/*  test_resource_registry.cpp                                            */
/**************************************************************************/
/*  Standalone tests for Kaizen::ResourceRegistry.                        */
/*                                                                        */
/*  Build and run (no engine build required):                             */
/*    ./tests/kaizen/run_tests.sh                                         */
/*    ./tests/kaizen/run_tests.sh --bench                                 */
/*                                                                        */
/*  Acceptance criteria covered:                                          */
/*    - 100k allocations and releases with exact counters                 */
/*    - index reuse increments the generation                             */
/*    - stale handles are rejected                                        */
/*    - double free is detected without corrupting counters               */
/*    - full registry fails in a controlled way                           */
/*    - byte accounting never underflows, peak is correct                 */
/*    - CPU and GPU registries stay independent                           */
/*    - pending (deferred) release moves bytes only after confirmation    */
/*    - concurrent alloc/free from 8 threads                              */
/**************************************************************************/

#include "kaizen/diagnostics/rendering/resource_registry.h"

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

using Kaizen::ResourceHandle;
using Kaizen::ResourceKind;
using Kaizen::ResourceRegistry;
using Kaizen::ResourceState;

// ---------------------------------------------------------------------------

TEST_BEGIN(lifecycle_alive_pending_released)
	ResourceRegistry registry(ResourceKind::GPU, 16);

	ResourceHandle handle = registry.allocate("Buffer", 4096, 7);
	CHECK(!handle.is_null());
	CHECK_EQ(handle.kind, ResourceKind::GPU);
	CHECK(registry.is_valid(handle));

	Kaizen::ResourceRecord record;
	CHECK(registry.describe(handle, record));
	CHECK_EQ(record.state, ResourceState::Alive);
	CHECK_EQ(record.requested_bytes, (uint64_t)4096);
	CHECK_EQ(record.allocation_frame, (uint64_t)7);
	CHECK(std::strcmp(record.type_name, "Buffer") == 0);

	CHECK(registry.begin_release(handle, 10));
	CHECK(registry.is_valid(handle));
	CHECK(registry.describe(handle, record));
	CHECK_EQ(record.state, ResourceState::PendingRelease);

	Kaizen::ResourceStats stats = registry.get_stats();
	CHECK_EQ(stats.pending_releases, (uint64_t)1);
	CHECK_EQ(stats.live_resources, (uint64_t)1);
	CHECK_EQ(stats.alive_requested_bytes, (uint64_t)0);
	CHECK_EQ(stats.pending_requested_bytes, (uint64_t)4096);

	// Not due yet.
	CHECK_EQ(registry.confirm_releases_until(9), (uint32_t)0);
	CHECK_EQ(registry.confirm_releases_until(10), (uint32_t)1);
	CHECK(!registry.is_valid(handle));
	CHECK(registry.describe(handle, record));
	CHECK_EQ(record.state, ResourceState::Released);

	stats = registry.get_stats();
	CHECK_EQ(stats.releases, (uint64_t)1);
	CHECK_EQ(stats.live_resources, (uint64_t)0);
	CHECK_EQ(stats.pending_releases, (uint64_t)0);
	CHECK_EQ(stats.released_requested_bytes, (uint64_t)4096);
	CHECK_EQ(stats.get_live_requested_bytes(), (uint64_t)0);
TEST_END()

TEST_BEGIN(hundred_thousand_allocations_and_releases)
	const uint32_t total = 100000;
	ResourceRegistry registry(ResourceKind::GPU, total);

	std::vector<ResourceHandle> handles;
	handles.reserve(total);
	for (uint32_t i = 0; i < total; i++) {
		handles.push_back(registry.allocate("Buffer", 64, i));
		if (handles.back().is_null()) {
			CHECK(!"allocation returned a null handle");
			break;
		}
	}

	Kaizen::ResourceStats stats = registry.get_stats();
	CHECK_EQ(stats.allocations, (uint64_t)total);
	CHECK_EQ(stats.live_resources, (uint64_t)total);
	CHECK_EQ(stats.alive_requested_bytes, (uint64_t)total * 64);
	CHECK_EQ(stats.peak_live_requested_bytes, (uint64_t)total * 64);

	uint32_t release_failures = 0;
	for (uint32_t i = 0; i < total; i++) {
		if (!registry.begin_release(handles[i], i)) {
			release_failures++;
		}
	}
	CHECK_EQ(release_failures, (uint32_t)0);
	CHECK_EQ(registry.confirm_releases_until(total), total);

	stats = registry.get_stats();
	CHECK_EQ(stats.allocations, (uint64_t)total);
	CHECK_EQ(stats.releases, (uint64_t)total);
	CHECK_EQ(stats.live_resources, (uint64_t)0);
	CHECK_EQ(stats.pending_releases, (uint64_t)0);
	CHECK_EQ(stats.get_live_requested_bytes(), (uint64_t)0);
	CHECK_EQ(stats.released_requested_bytes, (uint64_t)total * 64);
	CHECK_EQ(stats.double_frees, (uint64_t)0);
	CHECK_EQ(stats.invalid_handles, (uint64_t)0);
	CHECK_EQ(stats.generation_mismatches, (uint64_t)0);
TEST_END()

TEST_BEGIN(index_reuse_increments_generation)
	ResourceRegistry registry(ResourceKind::GPU, 2);

	ResourceHandle first = registry.allocate("A", 16, 0);
	CHECK(registry.begin_release(first, 0));
	CHECK_EQ(registry.confirm_releases_until(0), (uint32_t)1);

	ResourceHandle second = registry.allocate("B", 32, 1);
	CHECK_EQ(second.index, first.index); // Free list reuses the slot.
	CHECK(second.generation != first.generation);
	CHECK(registry.is_valid(second));
	CHECK(!registry.is_valid(first));
TEST_END()

TEST_BEGIN(stale_handle_rejected)
	ResourceRegistry registry(ResourceKind::GPU, 2);

	ResourceHandle first = registry.allocate("A", 16, 0);
	CHECK(registry.begin_release(first, 0));
	registry.confirm_releases_until(0);

	ResourceHandle second = registry.allocate("B", 32, 1);
	CHECK(registry.begin_release(first, 0) == false);

	const Kaizen::ResourceStats stats = registry.get_stats();
	CHECK_EQ(stats.generation_mismatches, (uint64_t)1);
	CHECK_EQ(stats.double_frees, (uint64_t)0);
	CHECK_EQ(stats.releases, (uint64_t)1);
	CHECK_EQ(stats.live_resources, (uint64_t)1);
	CHECK(registry.is_valid(second));
TEST_END()

TEST_BEGIN(double_free_detected)
	ResourceRegistry registry(ResourceKind::GPU, 4);

	ResourceHandle handle = registry.allocate("A", 16, 0);
	CHECK(registry.begin_release(handle, 0));
	CHECK(registry.begin_release(handle, 0) == false); // Pending, not yet released.
	CHECK_EQ(registry.get_stats().double_frees, (uint64_t)1);

	CHECK_EQ(registry.confirm_releases_until(0), (uint32_t)1);
	CHECK(registry.begin_release(handle, 0) == false); // Already released.
	CHECK(registry.confirm_release(handle) == false);

	const Kaizen::ResourceStats stats = registry.get_stats();
	CHECK_EQ(stats.double_frees, (uint64_t)3);
	CHECK_EQ(stats.releases, (uint64_t)1);
	CHECK_EQ(stats.live_resources, (uint64_t)0);
	CHECK_EQ(stats.pending_releases, (uint64_t)0);
TEST_END()

TEST_BEGIN(full_registry_fails_controlled)
	ResourceRegistry registry(ResourceKind::GPU, 8);

	ResourceHandle handles[8];
	for (int i = 0; i < 8; i++) {
		handles[i] = registry.allocate("A", 1, 0);
		CHECK(!handles[i].is_null());
	}

	ResourceHandle overflow = registry.allocate("A", 1, 0);
	CHECK(overflow.is_null());

	Kaizen::ResourceStats stats = registry.get_stats();
	CHECK_EQ(stats.registry_full, (uint64_t)1);
	CHECK_EQ(stats.allocations, (uint64_t)8);
	CHECK_EQ(stats.live_resources, (uint64_t)8);

	// Releasing one slot must make room again.
	CHECK(registry.begin_release(handles[3], 0));
	CHECK_EQ(registry.confirm_releases_until(0), (uint32_t)1);
	ResourceHandle reused = registry.allocate("A", 1, 0);
	CHECK(!reused.is_null());
	CHECK_EQ(registry.get_stats().registry_full, (uint64_t)1);
TEST_END()

TEST_BEGIN(byte_accounting_and_peak)
	ResourceRegistry registry(ResourceKind::GPU, 8);

	ResourceHandle a = registry.allocate("A", 100, 0);
	ResourceHandle b = registry.allocate("B", 200, 0);
	ResourceHandle c = registry.allocate("C", 300, 0);

	Kaizen::ResourceStats stats = registry.get_stats();
	CHECK_EQ(stats.alive_requested_bytes, (uint64_t)600);
	CHECK_EQ(stats.peak_live_requested_bytes, (uint64_t)600);

	CHECK(registry.begin_release(b, 5));
	stats = registry.get_stats();
	CHECK_EQ(stats.alive_requested_bytes, (uint64_t)400);
	CHECK_EQ(stats.pending_requested_bytes, (uint64_t)200);
	CHECK_EQ(stats.get_live_requested_bytes(), (uint64_t)600);

	// Backend allocation differs from the requested size.
	CHECK(registry.set_backend_bytes(a, 4096));
	stats = registry.get_stats();
	CHECK_EQ(stats.alive_backend_bytes, (uint64_t)4096 + 300);
	CHECK_EQ(stats.peak_live_backend_bytes, (uint64_t)4096 + 300 + 200);

	CHECK_EQ(registry.confirm_releases_until(5), (uint32_t)1);
	stats = registry.get_stats();
	CHECK_EQ(stats.pending_requested_bytes, (uint64_t)0);
	CHECK_EQ(stats.released_requested_bytes, (uint64_t)200);
	CHECK_EQ(stats.pending_backend_bytes, (uint64_t)0);

	CHECK(registry.begin_release(a, 6));
	CHECK(registry.begin_release(c, 6));
	CHECK_EQ(registry.confirm_releases_until(6), (uint32_t)2);

	stats = registry.get_stats();
	CHECK_EQ(stats.get_live_requested_bytes(), (uint64_t)0);
	CHECK_EQ(stats.get_live_backend_bytes(), (uint64_t)0);
	CHECK_EQ(stats.released_requested_bytes, (uint64_t)600);
	CHECK_EQ(stats.released_backend_bytes, (uint64_t)4096 + 300 + 200);
TEST_END()

TEST_BEGIN(cpu_and_gpu_registries_are_independent)
	ResourceRegistry cpu(ResourceKind::CPU, 4);
	ResourceRegistry gpu(ResourceKind::GPU, 4);

	ResourceHandle cpu_handle = cpu.allocate("Scene", 1000, 0);
	ResourceHandle gpu_handle = gpu.allocate("Texture", 2000, 0);

	// Both registries start at index 0; the kind disambiguates them.
	CHECK_EQ(cpu_handle.index, gpu_handle.index);
	CHECK(cpu_handle.generation == gpu_handle.generation);
	CHECK_EQ(cpu_handle.kind, ResourceKind::CPU);
	CHECK_EQ(gpu_handle.kind, ResourceKind::GPU);

	CHECK(cpu.is_valid(cpu_handle));
	CHECK(!cpu.is_valid(gpu_handle)); // Wrong registry.
	CHECK(gpu.is_valid(gpu_handle));

	CHECK(cpu.begin_release(cpu_handle, 0));
	CHECK_EQ(cpu.confirm_releases_until(0), (uint32_t)1);

	CHECK_EQ(cpu.get_stats().live_resources, (uint64_t)0);
	CHECK_EQ(gpu.get_stats().live_resources, (uint64_t)1);
	CHECK_EQ(gpu.get_stats().alive_requested_bytes, (uint64_t)2000);
	CHECK(gpu.is_valid(gpu_handle));
TEST_END()

TEST_BEGIN(multi_frame_epochs)
	ResourceRegistry registry(ResourceKind::GPU, 8);

	ResourceHandle a = registry.allocate("A", 10, 0);
	ResourceHandle b = registry.allocate("B", 20, 0);
	ResourceHandle c = registry.allocate("C", 30, 0);

	CHECK(registry.begin_release(a, 10));
	CHECK(registry.begin_release(b, 20));
	CHECK(registry.begin_release(c, 30));

	CHECK_EQ(registry.confirm_releases_until(10), (uint32_t)1);
	CHECK_EQ(registry.get_stats().pending_releases, (uint64_t)2);
	CHECK_EQ(registry.confirm_releases_until(19), (uint32_t)0);
	CHECK_EQ(registry.confirm_releases_until(30), (uint32_t)2);
	CHECK_EQ(registry.get_stats().pending_releases, (uint64_t)0);

	// Confirming an already processed epoch is a no-op.
	CHECK_EQ(registry.confirm_releases_until(100), (uint32_t)0);
TEST_END()

TEST_BEGIN(event_log_records_lifetime)
	ResourceRegistry registry(ResourceKind::GPU, 4);
	registry.set_event_log_enabled(true);

	ResourceHandle handle = registry.allocate("A", 16, 3);
	CHECK(registry.begin_release(handle, 4));
	CHECK_EQ(registry.confirm_releases_until(4), (uint32_t)1);

	Kaizen::ResourceEvent events[8];
	const uint32_t count = registry.get_events(events, 8);
	CHECK_EQ(count, (uint32_t)3);
	CHECK_EQ(events[0].type, Kaizen::ResourceEvent::Type::Allocated);
	CHECK_EQ(events[0].frame, (uint64_t)3);
	CHECK_EQ(events[1].type, Kaizen::ResourceEvent::Type::ReleaseRequested);
	CHECK_EQ(events[1].frame, (uint64_t)4);
	CHECK_EQ(events[2].type, Kaizen::ResourceEvent::Type::Released);
	CHECK_EQ(events[2].bytes, (uint64_t)16);
TEST_END()

TEST_BEGIN(concurrent_alloc_free)
	const uint32_t thread_count = 8;
	const uint32_t per_thread = 25000;
	ResourceRegistry registry(ResourceKind::GPU, thread_count * 64);

	std::atomic<uint32_t> ready{ 0 };
	std::atomic<bool> go{ false };

	std::vector<std::thread> threads;
	threads.reserve(thread_count);
	for (uint32_t t = 0; t < thread_count; t++) {
		threads.emplace_back([&registry, &ready, &go, t]() {
			ready.fetch_add(1);
			while (!go.load(std::memory_order_acquire)) {
			}

			std::vector<ResourceHandle> local;
			local.reserve(64);
			for (uint32_t i = 0; i < per_thread; i++) {
				ResourceHandle handle = registry.allocate("A", 16, i);
				if (handle.is_null()) {
					continue;
				}
				local.push_back(handle);

				// Keep a small working set per thread, then drain it.
				if (local.size() >= 64) {
					for (ResourceHandle &h : local) {
						registry.begin_release(h, i);
					}
					registry.confirm_releases_until(i);
					local.clear();
				}
			}

			for (ResourceHandle &h : local) {
				registry.begin_release(h, per_thread);
			}
			registry.confirm_releases_until(per_thread);
			(void)t;
		});
	}

	while (ready.load() < thread_count) {
	}
	go.store(true, std::memory_order_release);

	for (std::thread &thread : threads) {
		thread.join();
	}

	const Kaizen::ResourceStats stats = registry.get_stats();
	CHECK_EQ(stats.allocations, (uint64_t)thread_count * per_thread);
	CHECK_EQ(stats.releases, stats.allocations);
	CHECK_EQ(stats.live_resources, (uint64_t)0);
	CHECK_EQ(stats.pending_releases, (uint64_t)0);
	CHECK_EQ(stats.get_live_requested_bytes(), (uint64_t)0);
	CHECK_EQ(stats.registry_full, (uint64_t)0);
	CHECK_EQ(stats.invalid_handles, (uint64_t)0);
	CHECK_EQ(stats.generation_mismatches, (uint64_t)0);
	CHECK_EQ(stats.double_frees, (uint64_t)0);
TEST_END()

TEST_BEGIN(generation_wrap_skips_zero)
	// Start at the last 32-bit generation so the next reuse wraps.
	ResourceRegistry registry(ResourceKind::GPU, 2, UINT32_MAX);

	ResourceHandle first = registry.allocate("A", 1, 0);
	CHECK_EQ(first.generation, UINT32_MAX);

	CHECK(registry.begin_release(first, 0));
	CHECK_EQ(registry.confirm_releases_until(0), (uint32_t)1);

	ResourceHandle second = registry.allocate("B", 1, 0);
	CHECK_EQ(second.index, first.index);
	CHECK_EQ(second.generation, (uint32_t)1); // Zero is reserved for null handles.
	CHECK(!registry.is_valid(first));
	CHECK(registry.begin_release(first, 0) == false);
	CHECK_EQ(registry.get_stats().generation_mismatches, (uint64_t)1);
TEST_END()

TEST_BEGIN(large_byte_values_are_exact)
	const uint64_t one_tib = 1ull << 40;
	ResourceRegistry registry(ResourceKind::GPU, 4);

	ResourceHandle a = registry.allocate("A", one_tib, 0);
	ResourceHandle b = registry.allocate("B", one_tib, 0);

	Kaizen::ResourceStats stats = registry.get_stats();
	CHECK_EQ(stats.alive_requested_bytes, 2 * one_tib);
	CHECK_EQ(stats.peak_live_requested_bytes, 2 * one_tib);

	CHECK(registry.begin_release(a, 1));
	CHECK_EQ(registry.confirm_releases_until(1), (uint32_t)1);

	stats = registry.get_stats();
	CHECK_EQ(stats.released_requested_bytes, one_tib);
	CHECK_EQ(stats.get_live_requested_bytes(), one_tib);
	CHECK_EQ(stats.pending_requested_bytes, (uint64_t)0);

	CHECK(registry.begin_release(b, 2));
	CHECK_EQ(registry.confirm_releases_until(2), (uint32_t)1);
	CHECK_EQ(registry.get_stats().released_requested_bytes, 2 * one_tib);
	CHECK_EQ(registry.get_stats().get_live_requested_bytes(), (uint64_t)0);
TEST_END()

TEST_BEGIN(event_ring_overflow_keeps_newest)
	ResourceRegistry registry(ResourceKind::GPU, 4);
	registry.set_event_log_enabled(true);

	const uint32_t cycles = 2000; // 3 events per cycle, far beyond the 1024-entry ring.
	uint32_t last_generation = 0;
	uint64_t last_frame = 0;
	for (uint32_t i = 0; i < cycles; i++) {
		ResourceHandle handle = registry.allocate("A", 16, i);
		last_generation = handle.generation;
		last_frame = i;
		registry.begin_release(handle, i);
		registry.confirm_releases_until(i);
	}

	std::vector<Kaizen::ResourceEvent> events(ResourceRegistry::EVENT_CAPACITY);
	const uint32_t count = registry.get_events(events.data(), events.size());
	CHECK_EQ(count, ResourceRegistry::EVENT_CAPACITY);

	// The newest cycle must still be present, in order.
	CHECK_EQ(events[count - 3].type, Kaizen::ResourceEvent::Type::Allocated);
	CHECK_EQ(events[count - 3].frame, last_frame);
	CHECK_EQ(events[count - 3].generation, last_generation);
	CHECK_EQ(events[count - 2].type, Kaizen::ResourceEvent::Type::ReleaseRequested);
	CHECK_EQ(events[count - 1].type, Kaizen::ResourceEvent::Type::Released);
	CHECK_EQ(events[count - 1].generation, last_generation);

	// Requesting fewer events must return the same newest tail.
	Kaizen::ResourceEvent tail[10];
	CHECK_EQ(registry.get_events(tail, 10), (uint32_t)10);
	CHECK_EQ(tail[9].frame, events[count - 1].frame);
	CHECK_EQ(tail[9].type, events[count - 1].type);
	CHECK_EQ(tail[9].generation, events[count - 1].generation);
TEST_END()

TEST_BEGIN(snapshot_consistency_under_concurrency)
	const uint32_t thread_count = 4;
	const uint32_t per_thread = 20000;
	ResourceRegistry registry(ResourceKind::GPU, 4096);

	std::atomic<uint64_t> allocated_bytes{ 0 };
	std::atomic<uint32_t> workers_done{ 0 };
	std::atomic<uint32_t> snapshots{ 0 };
	std::atomic<uint32_t> invariant_failures{ 0 };
	std::atomic<uint32_t> ready{ 0 };
	std::atomic<bool> go{ false };

	auto verify_snapshot = [&]() {
		const Kaizen::ResourceStats stats = registry.get_stats();
		const uint64_t total = allocated_bytes.load();
		const uint64_t live_bytes = stats.get_live_requested_bytes();

		bool ok = true;
		ok = ok && (stats.allocations >= stats.releases);
		ok = ok && (stats.live_resources == stats.allocations - stats.releases);
		ok = ok && (stats.live_resources >= stats.pending_releases);
		ok = ok && (live_bytes == stats.alive_requested_bytes + stats.pending_requested_bytes);
		ok = ok && (stats.peak_live_requested_bytes >= live_bytes);
		ok = ok && (stats.released_requested_bytes + live_bytes <= total);
		ok = ok && (stats.alive_requested_bytes <= stats.peak_live_requested_bytes);
		ok = ok && (stats.alive_backend_bytes + stats.pending_backend_bytes == live_bytes);
		if (!ok) {
			invariant_failures.fetch_add(1);
		}
		snapshots.fetch_add(1);
	};

	std::thread sampler([&]() {
		ready.fetch_add(1);
		while (!go.load(std::memory_order_acquire)) {
		}
		while (workers_done.load(std::memory_order_acquire) < thread_count) {
			verify_snapshot();
		}
		verify_snapshot();
	});

	std::vector<std::thread> workers;
	workers.reserve(thread_count);
	for (uint32_t t = 0; t < thread_count; t++) {
		workers.emplace_back([&]() {
			ready.fetch_add(1);
			while (!go.load(std::memory_order_acquire)) {
			}

			std::vector<ResourceHandle> local;
			local.reserve(32);
			for (uint32_t i = 0; i < per_thread; i++) {
				const uint64_t bytes = 16 * ((i % 7) + 1);
				// Count the bytes before publishing the resource so a snapshot can
				// never observe more live bytes than were ever requested.
				allocated_bytes.fetch_add(bytes, std::memory_order_relaxed);

				ResourceHandle handle = registry.allocate("A", bytes, i);
				if (handle.is_null()) {
					continue;
				}
				local.push_back(handle);

				if (local.size() >= 32) {
					for (ResourceHandle &h : local) {
						registry.begin_release(h, i);
					}
					registry.confirm_releases_until(i);
					local.clear();
				}
			}

			for (ResourceHandle &h : local) {
				registry.begin_release(h, per_thread);
			}
			registry.confirm_releases_until(per_thread);
			workers_done.fetch_add(1);
		});
	}

	while (ready.load() < thread_count + 1) {
	}
	go.store(true, std::memory_order_release);

	for (std::thread &worker : workers) {
		worker.join();
	}
	sampler.join();

	CHECK_EQ(invariant_failures.load(), (uint32_t)0);
	CHECK(snapshots.load() > 0);

	const Kaizen::ResourceStats final_stats = registry.get_stats();
	CHECK_EQ(final_stats.allocations, (uint64_t)thread_count * per_thread);
	CHECK_EQ(final_stats.releases, final_stats.allocations);
	CHECK_EQ(final_stats.live_resources, (uint64_t)0);
	CHECK_EQ(final_stats.pending_releases, (uint64_t)0);
	CHECK_EQ(final_stats.get_live_requested_bytes(), (uint64_t)0);
	CHECK_EQ(final_stats.released_requested_bytes, allocated_bytes.load());
	CHECK_EQ(final_stats.registry_full, (uint64_t)0);
	CHECK_EQ(final_stats.invalid_handles, (uint64_t)0);
	CHECK_EQ(final_stats.generation_mismatches, (uint64_t)0);
	CHECK_EQ(final_stats.double_frees, (uint64_t)0);
TEST_END()

// ---------------------------------------------------------------------------
// Benchmarks

namespace bench {

double percentile_sorted(const std::vector<double> &p_values, double p) {
	if (p_values.empty()) {
		return 0.0;
	}
	const size_t index = (size_t)((p / 100.0) * (p_values.size() - 1));
	return p_values[index];
}

struct Latency {
	double mean_ns = 0;
	double p50_ns = 0;
	double p95_ns = 0;
	double p99_ns = 0;
	double cycles_per_second = 0;
};

// Measures one op = allocate + begin_release + confirm. Operations are timed
// in batches so the steady_clock overhead does not dominate the result; the
// reported percentiles are per-operation values within each batch.
Latency measure(ResourceRegistry &registry, uint32_t cycles, uint32_t batch) {
	std::vector<double> per_cycle;
	per_cycle.reserve(cycles / batch + 2);

	uint32_t done = 0;
	double total_ns = 0;
	while (done < cycles) {
		const uint32_t n = (cycles - done) < batch ? (cycles - done) : batch;
		const auto start = std::chrono::steady_clock::now();
		for (uint32_t i = 0; i < n; i++) {
			ResourceHandle handle = registry.allocate("A", 4096, done + i);
			registry.begin_release(handle, done + i);
			registry.confirm_releases_until(done + i);
		}
		const auto end = std::chrono::steady_clock::now();
		const double ns = std::chrono::duration<double, std::nano>(end - start).count();
		total_ns += ns;
		per_cycle.push_back(ns / n);
		done += n;
	}

	std::sort(per_cycle.begin(), per_cycle.end());

	Latency out;
	out.mean_ns = total_ns / cycles;
	out.p50_ns = percentile_sorted(per_cycle, 50);
	out.p95_ns = percentile_sorted(per_cycle, 95);
	out.p99_ns = percentile_sorted(per_cycle, 99);
	out.cycles_per_second = 1e9 / out.mean_ns;
	return out;
}

void print(const char *p_label, const Latency &p_latency) {
	std::printf("bench %-18s mean=%6.1f ns  p50=%6.1f  p95=%6.1f  p99=%6.1f ns  %6.2f Mops/s\n",
			p_label, p_latency.mean_ns, p_latency.p50_ns, p_latency.p95_ns, p_latency.p99_ns,
			p_latency.cycles_per_second / 1e6);
}

double throughput(uint32_t threads, uint32_t per_thread) {
	ResourceRegistry registry(ResourceKind::GPU, 65536);
	std::atomic<uint32_t> ready{ 0 };
	std::atomic<bool> go{ false };

	std::vector<std::thread> workers;
	workers.reserve(threads);
	for (uint32_t t = 0; t < threads; t++) {
		workers.emplace_back([&registry, &ready, &go, per_thread]() {
			ready.fetch_add(1);
			while (!go.load(std::memory_order_acquire)) {
			}
			for (uint32_t i = 0; i < per_thread; i++) {
				ResourceHandle handle = registry.allocate("A", 4096, i);
				registry.begin_release(handle, i);
				registry.confirm_releases_until(i);
			}
		});
	}

	while (ready.load() < threads) {
	}
	const auto start = std::chrono::steady_clock::now();
	go.store(true, std::memory_order_release);
	for (std::thread &worker : workers) {
		worker.join();
	}
	const auto end = std::chrono::steady_clock::now();

	const double seconds = std::chrono::duration<double>(end - start).count();
	return ((double)threads * per_thread) / seconds / 1e6;
}

} // namespace bench

void run_benchmark() {
	const uint32_t cycles = 320000;

	{
		ResourceRegistry registry(ResourceKind::GPU, 65536);
		bench::print("basic", bench::measure(registry, cycles, 64));
	}
	{
		ResourceRegistry registry(ResourceKind::GPU, 65536);
		registry.set_event_log_enabled(true);
		bench::print("detailed events", bench::measure(registry, cycles, 64));
	}
	{
		ResourceRegistry registry(ResourceKind::GPU, 65536);
		ResourceHandle anchor = registry.allocate("A", 4096, 0);
		bench::print("low occupancy", bench::measure(registry, cycles, 64));
		registry.begin_release(anchor, cycles);
		registry.confirm_releases_until(cycles);
	}
	{
		ResourceRegistry registry(ResourceKind::GPU, 4096);
		for (uint32_t i = 0; i < 4095; i++) {
			registry.allocate("A", 16, 0);
		}
		bench::print("near capacity", bench::measure(registry, cycles, 64));
	}

	std::printf("bench throughput (alloc+release+confirm, shared registry):\n");
	for (uint32_t threads : { 1u, 2u, 4u, 8u }) {
		std::printf("bench threads=%u %6.2f Mops/s\n", threads, bench::throughput(threads, 50000));
	}

	std::printf("bench registry footprint: %.1f KiB per %u slots\n",
			(65536 * (double)sizeof(Kaizen::ResourceRecord)) / 1024.0, 65536);
}

} // namespace

int main(int argc, char **argv) {
	const bool bench = argc > 1 && std::strcmp(argv[1], "--bench") == 0;

	lifecycle_alive_pending_released();
	hundred_thousand_allocations_and_releases();
	index_reuse_increments_generation();
	stale_handle_rejected();
	double_free_detected();
	full_registry_fails_controlled();
	byte_accounting_and_peak();
	cpu_and_gpu_registries_are_independent();
	multi_frame_epochs();
	event_log_records_lifetime();
	concurrent_alloc_free();
	generation_wrap_skips_zero();
	large_byte_values_are_exact();
	event_ring_overflow_keeps_newest();
	snapshot_consistency_under_concurrency();

	if (bench) {
		run_benchmark();
	}

	std::printf("%d checks, %d failure(s)\n", checks, failures);
	return failures == 0 ? 0 : 1;
}
