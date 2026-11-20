/**************************************************************************/
/*  pipeline_budget.cpp                                                   */
/*  Budget and accounting infrastructure for P2.4.1–P2.4.2.              */
/*                                                                        */
/*  PipelineBudget does NOT own slot state; Registry retains authority.   */
/*  Budget merely interprets the numbers Registry already produces.       */
/*  No Vulkan integration; purely core accounting.                        */
/**************************************************************************/

#include "pipeline_budget.h"

namespace Kaizen {

PipelineBudget::PipelineBudget(uint64_t p_budget_bytes,
		uint32_t p_high_watermark,
		uint32_t p_low_watermark,
		uint32_t p_max_candidates)
	: budget_bytes(p_budget_bytes),
	  high_watermark_percent(p_high_watermark),
	  low_watermark_percent(p_low_watermark),
	  max_candidates_per_cycle_val(p_max_candidates),
	  resident_bytes(0),
	  pending_bytes(0),
	  peak_resident_bytes(0)
{
}

void PipelineBudget::reset_accounting() {
	resident_bytes = 0;
	pending_bytes = 0;
	peak_resident_bytes = 0;
}

void PipelineBudget::update_peak() {
	if (resident_bytes > peak_resident_bytes) {
		peak_resident_bytes = resident_bytes;
	}
}

bool PipelineBudget::at_high_watermark() const {
	return budget_bytes > 0 && resident_bytes >= (budget_bytes * high_watermark_percent / 100);
}

bool PipelineBudget::at_low_watermark() const {
	return budget_bytes > 0 && resident_bytes <= (budget_bytes * low_watermark_percent / 100);
}

uint32_t PipelineBudget::pressure_level() const {
	if (at_high_watermark()) {
		return 2; // high pressure
	}
	if (at_low_watermark()) {
		return 1; // low pressure
	}
	return 0; // no pressure
}

void PipelineBudget::add_resident_bytes(uint64_t p_bytes) {
	resident_bytes += p_bytes;
	update_peak();
}

void PipelineBudget::subtract_resident_bytes(uint64_t p_bytes) {
	if (p_bytes > resident_bytes) {
		resident_bytes = 0;
	} else {
		resident_bytes -= p_bytes;
	}
}

void PipelineBudget::add_pending_bytes(uint64_t p_bytes) {
	pending_bytes += p_bytes;
}

void PipelineBudget::subtract_pending_bytes(uint64_t p_bytes) {
	if (p_bytes > pending_bytes) {
		pending_bytes = 0;
	} else {
		pending_bytes -= p_bytes;
	}
}

// ---------------------------------------------------------------------------

// Explicit instantiation / definitions if needed inline.
// The struct is small enough to keep inline, but we keep the .cpp for
// future expansion (e.g., VRAM-heap queries from the driver).

} // namespace Kaizen