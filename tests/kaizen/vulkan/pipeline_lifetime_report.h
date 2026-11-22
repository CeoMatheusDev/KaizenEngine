/**************************************************************************/
/*  pipeline_lifetime_report.h                                            */
/**************************************************************************/
/*  Reporting helpers for the device-level pipeline lifetime test.        */
/*                                                                        */
/*  Deliberately free of Vulkan types and of any engine header, so the    */
/*  rules below can be unit-tested without a device or a loader: see      */
/*  test_pipeline_lifetime_report.cpp, which runs in ./run_tests.sh.       */
/*                                                                        */
/*  This file exists because the report used to be written inline in       */
/*  pipeline_lifetime_vulkan.cpp with four lines that hardcoded "PASS",    */
/*  three that hardcoded "CLEAN" for sanitizers the build never enabled,   */
/*  six that printed "(validated in phases)" instead of a number, and an   */
/*  exit code that ignored the validation-error count the summary above it */
/*  had just reported as a failure. A report that manufactures its own     */
/*  evidence is worse than no report, and none of it was checkable.        */
/*                                                                        */
/*  Copyright (c) 2014-present Godot Engine contributors.                  */
/*  Copyright (c) 2026 Matheus A. Santos, Hardion Labs.                    */
/*  See the files LICENSE.txt and LICENSE-ADDITIONAL-TERMS.md.             */
/**************************************************************************/

#pragma once

#include <cstddef>
#include <cstdio>
#include <cstring>

namespace kaizen_report {

/// Outcome of one phase of the test. `p_ran == false` must never be able to
/// produce the word PASS - that is the invariant the whole file exists for.
struct Phase {
	const char *label = "";
	bool ran = false;
	bool passed = false;
};

/// A claim about behaviour this harness does not exercise, with the reason.
/// Rendered as `NOT COVERED (<reason>)`; it has no pass/fail state at all.
struct Uncovered {
	const char *label = "";
	const char *reason = "";
};

enum MaxLine {
	MAX_LINE = 160, ///< One report line, label and status included.
};

inline const char *status_text(const Phase &p_phase) {
	if (!p_phase.ran) {
		return "NOT RUN";
	}
	return p_phase.passed ? "PASS" : "FAIL";
}

/// "Label:  STATUS" padded so the statuses line up. Returns the formatted line
/// in p_buffer and the number of characters written (like snprintf).
inline int format_phase_line(char *p_buffer, size_t p_size, const Phase &p_phase) {
	const int written = std::snprintf(p_buffer, p_size, "%-27s %s", p_phase.label, status_text(p_phase));
	return written;
}

inline int format_uncovered_line(char *p_buffer, size_t p_size, const Uncovered &p_uncovered) {
	const int written = std::snprintf(p_buffer, p_size, "%-27s NOT COVERED (%s)", p_uncovered.label, p_uncovered.reason);
	return written;
}

struct Summary {
	int checks = 0;
	int failures = 0;
	int validation_errors = 0;
	int phases_passed = 0;
	int phases_failed = 0;
	int phases_not_run = 0;
};

/// Adds one phase to the tallies. The three counters a caller reports separately
/// (checks, failures, validation errors) are deliberately left alone: they belong to
/// the CHECK macros, and folding phase counts into them would make "42 checks" mean
/// something different depending on who added a phase last.
inline void count_phase(Summary &p_summary, const Phase &p_phase) {
	if (!p_phase.ran) {
		p_summary.phases_not_run++;
	} else if (p_phase.passed) {
		p_summary.phases_passed++;
	} else {
		p_summary.phases_failed++;
	}
}

/// The exit status. Validation errors count even when every CHECK passed: the
/// layer reported something wrong about the calls this test made, which is a
/// result, not noise. That is the half of the old bug this reproduces: the
/// printed verdict used validation_errors and the return value did not.
inline bool run_succeeded(const Summary &p_summary) {
	return p_summary.failures == 0 &&
			p_summary.validation_errors == 0 &&
			p_summary.phases_failed == 0;
}

/// Which sanitizers this binary was built with. Only the build system knows for
/// sure - GCC defines no macro for `-fsanitize=undefined` - so the runner passes
/// the list in via -DKAIZEN_TEST_SANITIZERS and the compiler feature macros are
/// only a fallback for hand-built binaries. An empty string means "none", and the
/// report must then say so instead of claiming a sanitizer ran clean.
inline const char *sanitizers() {
#ifdef KAIZEN_TEST_SANITIZERS
	return KAIZEN_TEST_SANITIZERS;
#else
#if defined(__has_feature)
#if __has_feature(address_sanitizer)
	return "address";
#endif
#elif defined(__SANITIZE_ADDRESS__)
	return "address";
#endif
	return "";
#endif
}

inline bool has_sanitizer(const char *p_list, const char *p_name) {
	const size_t name_len = std::strlen(p_name);
	for (const char *p = p_list; p != nullptr && *p != '\0';) {
		const char *end = std::strchr(p, ',');
		const size_t field_len = (end == nullptr) ? std::strlen(p) : size_t(end - p);
		if (field_len == name_len && std::strncmp(p, p_name, name_len) == 0) {
			return true;
		}
		p = (end == nullptr) ? p + field_len : end + 1;
	}
	return false;
}

/// One sanitizer's verdict. It is never the word CLEAN unless the binary was
/// actually built with it, and even then it says "no report", because a sanitizer
/// that found something prints its own report and aborts - the absence of one is
/// the evidence, not a status this program can observe about itself.
inline int format_sanitizer_line(char *p_buffer, size_t p_size, const char *p_list, const char *p_name) {
	int written = 0;
	if (has_sanitizer(p_list, p_name)) {
		written = std::snprintf(p_buffer, p_size, "%-27s built with %s; no sanitizer report", p_name, p_name);
	} else {
		written = std::snprintf(p_buffer, p_size, "%-27s NOT BUILT (re-run with KAIZEN_SANITIZERS=%s)", p_name, p_name);
	}
	return written;
}

} // namespace kaizen_report
