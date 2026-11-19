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

// ---------------------------------------------------------------------------

void run_benchmark() {
	const uint32_t capacity = 65536;
	const uint32_t iterations = 1000000;

	ResourceRegistry registry(ResourceKind::GPU, capacity);
	registry.set_event_log_enabled(false);

	const auto start = std::chrono::steady_clock::now();
	ResourceHandle handle = registry.allocate("A", 4096, 0);
	for (uint32_t i = 0; i < iterations; i++) {
		registry.begin_release(handle, i);
		registry.confirm_releases_until(i);
		handle = registry.allocate("A", 4096, i);
	}
	const auto end = std::chrono::steady_clock::now();

	const double ns = std::chrono::duration<double, std::nano>(end - start).count();
	const double per_op = ns / (iterations * 3.0); // allocate + begin_release + confirm.
	std::printf("bench: %u cycles (alloc+release+confirm) in %.3f ms -> %.1f ns/op\n",
			iterations, ns / 1e6, per_op);
	std::printf("bench: registry footprint = %.1f KiB per %u slots\n",
			(capacity * (double)sizeof(Kaizen::ResourceRecord)) / 1024.0, capacity);
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

	if (bench) {
		run_benchmark();
	}

	std::printf("%d checks, %d failure(s)\n", checks, failures);
	return failures == 0 ? 0 : 1;
}
