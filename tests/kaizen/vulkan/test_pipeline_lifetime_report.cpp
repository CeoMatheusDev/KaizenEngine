/**************************************************************************/
/*  test_pipeline_lifetime_report.cpp                                     */
/**************************************************************************/
/*  Unit tests for the pipeline lifetime report helpers.                  */
/*                                                                        */
/*  These run with no Vulkan loader, no device and no validation layer -  */
/*  that is the point. The test they guard (pipeline_lifetime_vulkan.cpp) */
/*  skips on a machine without a device, so before this file the report an */
/*  operator reads was checked by nobody: it was only ever seen by hand,  */
/*  on a machine that had a GPU attached, after a run whose exit code     */
/*  already disagreed with it.                                            */
/*                                                                        */
/*  Build twice to cover both sides of the sanitizer question:            */
/*    g++ -std=c++17 test_pipeline_lifetime_report.cpp -o t && ./t        */
/*    g++ -std=c++17 -DKAIZEN_TEST_SANITIZERS=\"address,undefined\" ...   */
/*                                                                        */
/*  Copyright (c) 2014-present Godot Engine contributors.                 */
/*  Copyright (c) 2026 Matheus A. Santos, Hardion Labs.                   */
/*  See the files LICENSE.txt and LICENSE-ADDITIONAL-TERMS.md.            */
/**************************************************************************/

#include "pipeline_lifetime_report.h"

#include <cstdio>
#include <cstring>

using namespace kaizen_report;

static int checks = 0;
static int failures = 0;

static void check(bool p_condition, const char *p_expression, int p_line) {
	checks++;
	if (!p_condition) {
		failures++;
		std::printf("FAIL line %d: %s\n", p_line, p_expression);
	}
}

#define CHECK(cond) check((cond), #cond, __LINE__)

static bool line_is(const char *p_buffer, const char *p_expected) {
	return std::strcmp(p_buffer, p_expected) == 0;
}

static bool line_has(const char *p_buffer, const char *p_needle) {
	return std::strstr(p_buffer, p_needle) != nullptr;
}

// The word PASS may only ever come from a phase that ran and passed.
static void test_no_pass_without_a_run() {
	char line[MAX_LINE];

	Phase ran_pass;
	ran_pass.label = "Multi-queue submissions";
	ran_pass.ran = true;
	ran_pass.passed = true;
	format_phase_line(line, sizeof(line), ran_pass);
	CHECK(line_has(line, "PASS"));
	CHECK(!line_has(line, "NOT RUN"));

	Phase ran_fail = ran_pass;
	ran_fail.passed = false;
	format_phase_line(line, sizeof(line), ran_fail);
	CHECK(line_has(line, "FAIL"));
	CHECK(!line_has(line, "PASS"));

	// The case the old report got wrong four times over: nothing ran.
	Phase not_run;
	not_run.label = "Command buffer reuse";
	not_run.ran = false;
	not_run.passed = true; // even a stale `true` must not leak through
	format_phase_line(line, sizeof(line), not_run);
	CHECK(line_has(line, "NOT RUN"));
	CHECK(!line_has(line, "PASS"));
	CHECK(line_has(line, "Command buffer reuse"));
}

// A claim that is not tested is not a pass, and must say why instead.
static void test_uncovered_lines_carry_a_reason() {
	char line[MAX_LINE];
	Uncovered u;
	u.label = "Simultaneous submissions";
	u.reason = "harness submits once per queue";
	format_uncovered_line(line, sizeof(line), u);
	CHECK(line_has(line, "NOT COVERED"));
	CHECK(line_has(line, "harness submits once per queue"));
	CHECK(!line_has(line, "PASS"));
}

static void test_summary_tally() {
	Summary s;
	Phase p;
	p.ran = true;
	p.passed = true;
	count_phase(s, p);
	p.passed = false;
	count_phase(s, p);
	p.ran = false;
	count_phase(s, p);

	// The CHECK-macro counters are the caller's, not the tally's.
	CHECK(s.checks == 0);
	CHECK(s.phases_passed == 1);
	CHECK(s.phases_failed == 1);
	CHECK(s.phases_not_run == 1);
}

// The exit code and the printed verdict have to agree. They did not: the
// summary folded validation_errors into the verdict while `main` returned on
// `ok && failures == 0`, so a run that printed "Result: FAIL" with 3 layer
// errors exited 0 and a scripted caller read it as a pass.
static void test_validation_errors_fail_the_run() {
	Summary clean;
	clean.checks = 4;
	CHECK(run_succeeded(clean));

	Summary layer_noise = clean;
	layer_noise.validation_errors = 3;
	CHECK(!run_succeeded(layer_noise));

	Summary assertion = clean;
	assertion.failures = 1;
	CHECK(!run_succeeded(assertion));

	Summary phase_failed = clean;
	phase_failed.phases_failed = 1;
	CHECK(!run_succeeded(phase_failed));

	// A phase that could not run (device without the feature) is reported, but
	// does not by itself fail the run - skipping must stay distinguishable from
	// breaking.
	Summary skipped = clean;
	skipped.phases_not_run = 1;
	CHECK(run_succeeded(skipped));
}

static void test_sanitizer_list_parsing() {
	CHECK(has_sanitizer("address,undefined", "address"));
	CHECK(has_sanitizer("address,undefined", "undefined"));
	CHECK(!has_sanitizer("address,undefined", "thread"));
	CHECK(!has_sanitizer("", "address"));
	CHECK(!has_sanitizer(nullptr, "address"));
	// Whole fields only: a prefix must not count as the sanitizer being present.
	CHECK(!has_sanitizer("address-extra", "address"));
	CHECK(has_sanitizer("memory,address", "address"));
}

static void test_sanitizer_verdict_is_honest() {
	char line[MAX_LINE];

	// Not built: must not say clean, and must name the way to get the coverage.
	format_sanitizer_line(line, sizeof(line), "", "thread");
	CHECK(line_has(line, "NOT BUILT"));
	CHECK(line_has(line, "KAIZEN_SANITIZERS=thread"));
	CHECK(!line_has(line, "clean"));

	format_sanitizer_line(line, sizeof(line), "address", "address");
	CHECK(line_has(line, "built with address"));
	CHECK(!line_has(line, "NOT BUILT"));
	// Even when built, the claim stays observational: the harness cannot see its
	// own sanitizer beyond "it did not report anything".
	CHECK(!line_is(line, "address                      CLEAN"));
}

static void test_the_build_macros_agree() {
#ifdef KAIZEN_TEST_SANITIZERS
	// Compiled by the runner with a sanitizer list: sanitizers() must echo it,
	// otherwise the report would claim "NOT BUILT" for flags the build did pass.
	CHECK(!std::strcmp(sanitizers(), KAIZEN_TEST_SANITIZERS));
	CHECK(has_sanitizer(sanitizers(), "address") || !has_sanitizer(KAIZEN_TEST_SANITIZERS, "address"));
#else
	// A plain build has no sanitizer, so the report must not imply one. A binary
	// built by hand with -fsanitize=address is the one case the compiler macro
	// can see, so allow exactly that and nothing else.
	const char *s = sanitizers();
	CHECK(std::strcmp(s, "") == 0 || std::strcmp(s, "address") == 0);
#endif
}

int main() {
	test_no_pass_without_a_run();
	test_uncovered_lines_carry_a_reason();
	test_summary_tally();
	test_validation_errors_fail_the_run();
	test_sanitizer_list_parsing();
	test_sanitizer_verdict_is_honest();
	test_the_build_macros_agree();

	if (failures > 0) {
		std::printf("pipeline_lifetime_report: %d failures, %d checks\n", failures, checks);
		return 1;
	}
	std::printf("pipeline_lifetime_report: %d checks, 0 failure(s)\n", checks);
	return 0;
}
