/**************************************************************************/
/*  pipeline_budget.h                                                     */
/*  Budget and accounting infrastructure for P2.4.1–P2.4.2.             */
/*                                                                        */
/*  Responsibilities:                                                     */
/*   - Track resident_bytes, pending_bytes, peak_resident_bytes           */
/*   - Provide high/low watermark thresholds for pressure detection       */
/*   - Expose budget enforcement decisions to PipelineRegistry            */
/*                                                                        */
/*  PipelineBudget does NOT own slot state; Registry retains authority.   */
/*  Budget merely interprets the numbers Registry already produces.       */
/*  No Vulkan integration; purely core accounting.                        */
/**************************************************************************/

#pragma once

#include "pipeline_types.h"

#include <cstdint>

namespace Kaizen {

struct PipelineBudget {
	// --- Configurable thresholds -------------------------------------------

	/** Maximum resident+pending bytes before pressure is signaled. */
	uint64_t budget_bytes = 0;

	/** Percent of budget at which eviction is aggressively triggered. */
	uint32_t high_watermark_percent = 90;

	/** Percent of budget at which eviction tries to reduce consumption. */
	uint32_t low_watermark_percent = 75;

	/** Maximum candidates processed per enforce_budget() cycle. */
	uint32_t max_candidates_per_cycle_val = 32;

	// --- Computed state ----------------------------------------------------

	uint64_t resident_bytes = 0;
	uint64_t pending_bytes = 0;
	uint64_t peak_resident_bytes = 0;

	// --- Construction / reset ----------------------------------------------

	explicit PipelineBudget(uint64_t p_budget_bytes = 0,
			uint32_t p_high_watermark = 90,
			uint32_t p_low_watermark = 75,
			uint32_t p_max_candidates = 32);

	// Reset all accounting to zero; thresholds remain unchanged.
	void reset_accounting();

	// Update the peak if resident_bytes exceeds the current peak.
	void update_peak();

	// --- Pressure detection ------------------------------------------------

	/** Returns true if we are at or above the high watermark. */
	bool at_high_watermark() const;

	/** Returns true if we are at or below the low watermark. */
	bool at_low_watermark() const;

	/** Returns the pressure level: 0 = ok, 1 = high, 2 = low. */
	uint32_t pressure_level() const;

	// --- Convenience -------------------------------------------------------

	uint32_t high_watermark() const { return high_watermark_percent; }
	uint32_t low_watermark() const { return low_watermark_percent; }
	uint32_t max_candidates_per_cycle() const { return max_candidates_per_cycle_val; }
	uint64_t budget() const { return budget_bytes; }

	// --- Accounting adjustments (called by Registry) -----------------------

	/** Add bytes to resident accounting. */
	void add_resident_bytes(uint64_t p_bytes);

	/** Remove bytes from resident accounting. */
	void subtract_resident_bytes(uint64_t p_bytes);

	/** Add bytes to pending accounting. */
	void add_pending_bytes(uint64_t p_bytes);

	/** Remove bytes from pending accounting. */
	void subtract_pending_bytes(uint64_t p_bytes);
};

} // namespace Kaizen