/// tests/kaizen/shadow_filter - what the penumbra blocker search's stride costs and buys.
///
/// WHAT THIS COVERS
/// The Vogel disk the engine uploads (get_vogel_disk, lifted verbatim out of
/// servers/rendering/renderer_rd/renderer_scene_render_rd.cpp by extract_source_bodies.py) and
/// the subset the shader indexes in the blocker search. Per quality setting it measures:
///   * the footprint actually probed, sqrt(<r^2>) against the full disk's - the penumbra width
///     the search reports scales the PCF radius of the pass below it, so a narrowed search is a
///     narrowed shadow edge, not merely a noisier one;
///   * the error in the reported penumbra width against the continuous filter's, plus the
///     receiver positions where the two disagree about whether there are blockers at all (a
///     missed blocker is a hard edge where a soft one belongs).
///
/// WHAT IT DOES NOT COVER
/// Time. Halving one loop inside a shadow evaluation is a GPU cost question and this sandbox
/// has no GPU and no shader compiler, so tap counts are printed as counts and nothing more; the
/// S4/S5 measurement that would price it is listed as outstanding in docs/upstream-differences.md.
/// It also does not link the engine: the extracted body gets a three-line Math shim, enough for
/// a function that only uses sqrt/cos/sin. If a refactor moves that function,
/// extract_source_bodies.py exits non-zero instead of quietly testing nothing.
///
/// The numbers in the assertions below were measured on 2026-11-22. They are ratchets on a
/// trade-off that was made deliberately, not a specification of an ideal: a shadow filter is
/// allowed to change, but not to get worse at the thing it was chosen for without the table
/// here changing first.

#include "shadow_filter_extracted.inc"

#include <algorithm>
#include <cmath>
#include <cstdio>
#include <cstring>
#include <vector>

// Nothing about the engine below is typed in by hand: the capacity, the sample counts and the
// stride rule come from the generated header, which parses them out of the three sources that
// define them. A stale or empty header is a compile error, not a silently vacuous test.
#ifndef SHADOW_UBO_KERNEL_SLOTS
#error "shadow_filter_extracted.inc is missing its parsed facts; run extract_source_bodies.py"
#endif

#define KERNEL_CAPACITY SHADOW_UBO_KERNEL_SLOTS
#define MAX_SETTING_SAMPLES SHADOW_MAX_SAMPLES

static int failures = 0;
static int checks = 0;

static void check(bool p_condition, const char *p_what) {
	checks++;
	if (!p_condition) {
		printf("  FAIL: %s\n", p_what);
	}
}

// The engine's kernel, in the engine's layout: x,y at i*4 and i*4+1. The engine leaves the
// other two slots whatever memnew_arr held; the shader never reads them and neither do we.
struct Kernel {
	float data[KERNEL_CAPACITY * 4];
	int count = 0;

	void build(int p_count) {
		count = p_count;
		memset(data, 0, sizeof(data));
		get_vogel_disk(data, p_count);
	}

	float second_moment(const std::vector<int> &p_taps) const {
		double sum = 0.0;
		for (int i : p_taps) {
			sum += double(data[i * 4]) * data[i * 4] + double(data[i * 4 + 1]) * data[i * 4 + 1];
		}
		return p_taps.empty() ? 0.0f : float(sum / double(p_taps.size()));
	}

	float max_radius(const std::vector<int> &p_taps) const {
		float m = 0.0f;
		for (int i : p_taps) {
			m = std::max(m, std::sqrt(data[i * 4] * data[i * 4] + data[i * 4 + 1] * data[i * 4 + 1]));
		}
		return m;
	}
};

// The shader's rule, as parsed from scene_forward_lights_inc.glsl: the blocker search strides
// above a sample count the engine chooses, and uses every tap at or below it.
static int engine_stride(int p_count) {
	return p_count > SHADOW_STRIDE_THRESHOLD ? SHADOW_STRIDE : SHADOW_STRIDE_BELOW;
}

static std::vector<int> indices_all(int p_count) {
	std::vector<int> out;
	for (int i = 0; i < p_count; i++) {
		out.push_back(i);
	}
	return out;
}

static std::vector<int> indices_strided(int p_count, int p_phase) {
	std::vector<int> out;
	const int stride = engine_stride(p_count);
	for (int i = p_phase; i < p_count; i += stride) {
		out.push_back(i);
	}
	return out;
}

// What "just go further" would mean, measured so the bound below is known to have teeth: the
// same rule with a stride of 4 instead of 2.
static std::vector<int> indices_quarter(int p_count) {
	std::vector<int> out;
	for (int i = 0; i < p_count; i += 4) {
		out.push_back(i);
	}
	return out;
}

static std::vector<int> indices_first_half(int p_count) {
	std::vector<int> out;
	for (int i = 0; i < p_count / 2; i++) {
		out.push_back(i);
	}
	return out;
}

static std::vector<int> indices_second_half(int p_count) {
	std::vector<int> out;
	for (int i = p_count / 2; i < p_count; i++) {
		out.push_back(i);
	}
	return out;
}

// The rejected candidate, kept so the rejection stays measured: the same number of taps spread
// over 0..count-1 (index i -> round(i * (count-1) / (taps-1))), which includes the outermost
// tap that plain striding drops. No per-setting constant anywhere, which is what makes it
// tempting, and it is still not shipped - see the assertions.
static std::vector<int> indices_spanned(int p_count, int p_stride) {
	const int taps = p_count / p_stride;
	std::vector<int> out;
	if (taps <= 1) {
		out.push_back(0);
		return out;
	}
	for (int i = 0; i < taps; i++) {
		out.push_back(int((float(i) * float(p_count - 1)) / float(taps - 1) + 0.5f));
	}
	return out;
}

// Shadow map as the hardware sees it: a straight occluder edge at texel 0, linearly filtered,
// so the sampled depth is a one-texel ramp between occluder and background. Lit side is x > 0.
static float sampled_depth(float p_x_texel, float p_occluder_depth, float p_background_depth) {
	const float t = 1.0f - std::clamp(p_x_texel + 0.5f, 0.0f, 1.0f);
	return p_background_depth + (p_occluder_depth - p_background_depth) * t;
}

// The shader's expression: (-z + blocker_average) / (1 - blocker_average).
static float penumbra_of(float p_average, float p_receiver_depth) {
	return (p_average - p_receiver_depth) / (1.0f - p_average);
}

struct Sweep {
	float receiver_depth;
	float occluder_depth;
	float radius_texels;
};

struct Reference {
	float average = 0.0f;
	bool has_blockers = false;
};

// The continuous reference for one receiver offset: the area average over the disk. For a
// straight edge this reduces to the 1-D marginal of the disk, whose density is
// 2/pi * sqrt(1 - x^2) on [-1, 1]; the constant drops out of a ratio, so it is used as the
// quadrature weight directly.
static Reference continuous_reference(const Sweep &p_sweep, float p_offset_texel, int p_steps = 1024) {
	Reference ref;
	double weighted = 0.0;
	double weight = 0.0;
	for (int s = 0; s <= p_steps; s++) {
		const float x = -1.0f + 2.0f * float(s) / float(p_steps);
		const float w = std::sqrt(std::max(0.0f, 1.0f - x * x));
		const float d = sampled_depth(p_offset_texel + p_sweep.radius_texels * x, p_sweep.occluder_depth, p_sweep.receiver_depth);
		if (d > p_sweep.receiver_depth) {
			weighted += double(d) * w;
			weight += w;
		}
	}
	if (weight > 1e-6) {
		ref.has_blockers = true;
		ref.average = float(weighted / weight);
	}
	return ref;
}

struct Result {
	const char *strategy = "";
	int taps = 0;
	float width_ratio = 1.0f; // sqrt(<r^2>) over the full disk's: the probed penumbra width
	float penumbra_rms = 0.0f; // RMS error, as a fraction of the widest penumbra in the sweep
	float bias = 0.0f; // signed: negative = the shadow comes out harder than it should
	int missed = 0; // positions where the disk sees blockers and the taps see none
	int invented = 0; // the other way round
};

static Result measure(const char *p_name, const Kernel &p_kernel, const std::vector<int> &p_taps, const Kernel &p_full, float p_radius_scale, const Sweep &p_sweep, float p_normaliser) {
	Result out;
	out.strategy = p_name;
	out.taps = int(p_taps.size());
	// The radius scale is part of what the strategy probes: a variant that keeps fewer taps and
	// widens each one has to be judged on the footprint it actually walks, not on the table.
	out.width_ratio = p_radius_scale * std::sqrt(p_kernel.second_moment(p_taps) / p_full.second_moment(indices_all(p_full.count)));

	const int k_positions = 512;
	const int k_rotations = 32;
	const float radius = p_sweep.radius_texels * p_radius_scale;

	double sum_sq = 0.0;
	double sum_signed = 0.0;
	int samples = 0;

	for (int position = 0; position < k_positions; position++) {
		// Sweep the receiver across the edge, four penumbra widths either side.
		const float u = (float(position) / float(k_positions - 1) - 0.5f) * 8.0f * radius;
		const Reference ref = continuous_reference(p_sweep, u);
		const float ref_penumbra = ref.has_blockers ? penumbra_of(ref.average, p_sweep.receiver_depth) : 0.0f;

		for (int rot = 0; rot < k_rotations; rot++) {
			const float angle = 2.0f * float(M_PI) * float(rot) / float(k_rotations);
			const float cr = std::cos(angle);
			const float sr = std::sin(angle);

			double depth_sum = 0.0;
			double count = 0.0;
			for (int i : p_taps) {
				const float tx = p_kernel.data[i * 4];
				const float ty = p_kernel.data[i * 4 + 1];
				const float d = sampled_depth(u + radius * (cr * tx - sr * ty), p_sweep.occluder_depth, p_sweep.receiver_depth);
				if (d > p_sweep.receiver_depth) {
					depth_sum += d;
					count += 1.0;
				}
			}
			const bool found = count > 0.0;
			const float penumbra = found ? penumbra_of(float(depth_sum / count), p_sweep.receiver_depth) : 0.0f;

			if (ref.has_blockers && !found) {
				out.missed++;
			}
			if (!ref.has_blockers && found) {
				out.invented++;
			}
			const float error = (penumbra - ref_penumbra) / p_normaliser;
			sum_sq += double(error) * error;
			sum_signed += error;
			samples++;
		}
	}
	if (samples > 0) {
		out.penumbra_rms = float(std::sqrt(sum_sq / double(samples)));
		out.bias = float(sum_signed / double(samples));
	}
	return out;
}

// One scale for every strategy: the widest penumbra the reference reaches in the sweep. Dividing
// by the per-position reference instead would explode exactly where the penumbra is legitimately
// about to vanish.
static float sweep_normaliser(const Sweep &p_sweep) {
	float best = 0.0f;
	for (int position = 0; position < 256; position++) {
		const float u = (float(position) / 255.0f - 0.5f) * 8.0f * p_sweep.radius_texels;
		const Reference ref = continuous_reference(p_sweep, u, 512);
		if (ref.has_blockers) {
			best = std::max(best, penumbra_of(ref.average, p_sweep.receiver_depth));
		}
	}
	return std::max(best, 1e-3f);
}

static const Result *find_row(const std::vector<Result> &p_rows, const char *p_strategy) {
	for (const Result &row : p_rows) {
		if (strcmp(row.strategy, p_strategy) == 0) {
			return &row;
		}
	}
	return nullptr;
}

int main(int argc, char **argv) {
	bool report_only = false;
	bool show_table = false;
	for (int i = 1; i < argc; i++) {
		if (strcmp(argv[i], "--report") == 0) {
			report_only = true;
			show_table = true;
		} else if (strcmp(argv[i], "--verbose") == 0) {
			show_table = true;
		}
	}

	printf("shadow_filter: Vogel disk blocker search - probed footprint and penumbra error\n");

	Kernel kernel;
	Kernel full;

	// 1. Shape and bounds of the kernel the engine uploads, for every count it can pick.
	// Over every count the engine could ask for, not only the ones a setting currently picks.
	for (int count = 1; count <= MAX_SETTING_SAMPLES; count++) {
		kernel.build(count);
		check(kernel.max_radius(indices_all(count)) <= 1.0001f, "Vogel disk stays inside the unit disk");
		const int stride = engine_stride(count);
		const int taps = count / stride;
		check(taps > 0, "the stride never empties the loop");
		check((taps - 1) * stride <= count - 1, "strided index stays inside the kernel");
		check(count <= KERNEL_CAPACITY, "no setting asks for more taps than the UBO holds");
	}

	// 2. The trade-off, per setting the engine has (RSE::ShadowQuality maps the penumbra counts to
	//    {4, 4, 8, 12, 24, 32} for both the directional and the positional filters; all of them
	//    are swept, and the count list is checked against the shader's own bound below).
	const Sweep sweeps[] = {
		{ 0.5f, 0.9f, 2.0f }, // occluder well above the receiver, narrow penumbra
		{ 0.5f, 0.9f, 8.0f }, // same pair, wide penumbra
		{ 0.8f, 0.9f, 4.0f }, // occluder close to the receiver: the ambiguous case
	};
	const char *sweep_names[] = { "far/narrow", "far/wide", "near" };

	float worst_full_error = 0.0f;
	float worst_engine_error = 0.0f;
	float worst_comp_error = 0.0f;
	float worst_spanned_error = 0.0f;
	float worst_comp_gap = 0.0f;
	float worst_spanned_gap = 0.0f;
	float worst_spanned_deviation = 0.0f;
	float worst_engine_width = 1.0f;
	float best_first_half_width = 0.0f;
	float worst_second_half_width = 0.0f;
	float worst_error_ratio = 0.0f;
	float ratio_per_count[MAX_SETTING_SAMPLES + 1] = {};
	float worst_quarter_ratio = 0.0f;
	int cases = 0;
	int cases_first_half_misses_more = 0;

	for (size_t s = 0; s < sizeof(sweeps) / sizeof(sweeps[0]); s++) {
		const Sweep &sweep = sweeps[s];
		const float normaliser = sweep_normaliser(sweep);
		if (show_table) {
			printf("  sweep %s (receiver %.2f, occluder %.2f, radius %.1f texels):\n",
					sweep_names[s], sweep.receiver_depth, sweep.occluder_depth, sweep.radius_texels);
		}

		for (int raw : SHADOW_SETTING_SAMPLES) {
			// The parsed list covers both kernels, so it includes the counts where the filter
			// is a single tap or none at all; there is no subset to compare there.
			if (raw < 2) {
				continue;
			}
			const int count = raw;
			kernel.build(count);
			full.build(count);
			const float engine_width = std::sqrt(kernel.second_moment(indices_strided(count, 0)) / full.second_moment(indices_all(count)));
			const float compensation = engine_width > 0.0f ? 1.0f / engine_width : 1.0f;

			std::vector<Result> rows;
			rows.push_back(measure("all taps", kernel, indices_all(count), full, 1.0f, sweep, normaliser));
			rows.push_back(measure("stride", kernel, indices_strided(count, 0), full, 1.0f, sweep, normaliser));
			rows.push_back(measure("stride+comp", kernel, indices_strided(count, 0), full, compensation, sweep, normaliser));
			rows.push_back(measure("first half", kernel, indices_first_half(count), full, 1.0f, sweep, normaliser));
			rows.push_back(measure("second half", kernel, indices_second_half(count), full, 1.0f, sweep, normaliser));
			rows.push_back(measure("spanned", kernel, indices_spanned(count, engine_stride(count)), full, 1.0f, sweep, normaliser));
			rows.push_back(measure("quarter", kernel, indices_quarter(count), full, 1.0f, sweep, normaliser));

			if (show_table) {
				printf("    %2d taps:", count);
				for (const Result &row : rows) {
					printf("  %s[w%.3f e%.3f m%d i%d]", row.strategy, row.width_ratio, row.penumbra_rms, row.missed, row.invented);
				}
				printf("\n");
			}

			const Result *all = find_row(rows, "all taps");
			const Result *stride = find_row(rows, "stride");
			const Result *comp = find_row(rows, "stride+comp");
			const Result *first = find_row(rows, "first half");
			const Result *second = find_row(rows, "second half");
			const Result *spanned = find_row(rows, "spanned");
			check(all && stride && comp && first && second && spanned, "every strategy was measured");

			cases++;
			// The ratio is taken per case, not between two maxima drawn from different cases:
			// the worst absolute error for both strategies happens at 4 taps, where the stride
			// is not applied at all and the two are the same set. Comparing maxima there would
			// have shown a 1.00 factor and proved nothing.
			const float case_ratio = all->penumbra_rms > 0.0f ? stride->penumbra_rms / all->penumbra_rms : 1.0f;
			worst_error_ratio = std::max(worst_error_ratio, case_ratio);
			ratio_per_count[count] = std::max(ratio_per_count[count], case_ratio);
			worst_quarter_ratio = std::max(worst_quarter_ratio, all->penumbra_rms > 0.0f ? find_row(rows, "quarter")->penumbra_rms / all->penumbra_rms : 1.0f);
			worst_full_error = std::max(worst_full_error, all->penumbra_rms);
			worst_engine_error = std::max(worst_engine_error, stride->penumbra_rms);
			worst_comp_error = std::max(worst_comp_error, comp->penumbra_rms);
			worst_spanned_error = std::max(worst_spanned_error, spanned->penumbra_rms);
			worst_comp_gap = std::max(worst_comp_gap, std::fabs(comp->penumbra_rms - stride->penumbra_rms));
			worst_spanned_gap = std::max(worst_spanned_gap, spanned->penumbra_rms - stride->penumbra_rms);
			worst_engine_width = std::min(worst_engine_width, stride->width_ratio);
			worst_spanned_deviation = std::max(worst_spanned_deviation, std::fabs(spanned->width_ratio - 1.0f));
			best_first_half_width = std::max(best_first_half_width, first->width_ratio);
			worst_second_half_width = std::max(worst_second_half_width, second->width_ratio);
			if (first->missed > stride->missed) {
				cases_first_half_misses_more++;
			}

			// The cost figure is the whole point of the change, so it is asserted rather than
			// assumed: the tested set has exactly the taps the shader's loop bound produces.
			check(stride->taps == count / engine_stride(count), "the tested tap set matches the shader's loop bound");
			check(spanned->taps == stride->taps, "the rejected candidate is compared at the same cost");
		}
	}

	if (!report_only) {
		printf("  probed width: shipped stride never below %.4f, first-half option at most %.4f, second-half at least %.4f\n",
				worst_engine_width, best_first_half_width, worst_second_half_width);
		printf("  penumbra error (fraction of the widest): all taps %.3f, stride %.3f, radius-compensated %.3f, spanned %.3f\n",
				worst_full_error, worst_engine_error, worst_comp_error, worst_spanned_error);
		printf("  %d sweep/setting cases; the first-half set misses more blocker phases in %d of them\n", cases, cases_first_half_misses_more);
		printf("  gap against the shipped rule: radius compensation %.4f, spanned %+.4f\n", worst_comp_gap, worst_spanned_gap);

		// What the shipped stride is argued to do, as numbers rather than prose.
		check(worst_engine_width >= 0.93f, "the stride keeps the search's footprint, so it does not narrow the penumbra");
		printf("  worst per-setting error ratio, stride over all taps: %.2fx\n", worst_error_ratio);
		printf("  error ratio by setting:");
		for (int raw : SHADOW_SETTING_SAMPLES) {
			if (raw >= 2 && ratio_per_count[raw] > 0.0f) {
				printf(" %d:%.2fx", raw, ratio_per_count[raw]);
			}
		}
		printf(", quarter taps: %.2fx\n", worst_quarter_ratio);
		check(worst_error_ratio <= 3.0f, "its penumbra error stays under 3x the full disk's at every setting");
		check(worst_quarter_ratio > 3.0f, "and a further halving would break that gate, so the gate has teeth");

		// What it was argued against, quantified: taking the first half of the kernel would probe
		// a footprint 0.707 as wide, overshoot on the other half, and miss the penumbra edge more
		// often in every single case.
		check(best_first_half_width <= 0.71f, "the first-half alternative would have lost the disk");
		check(worst_second_half_width >= 1.1f, "and the second half would have overshot it");
		check(cases_first_half_misses_more == cases && cases > 0, "the strided set sees the penumbra edge at least as often, everywhere");

		// Two rewrites that look obviously better, recorded as measured and not shipped, so the
		// next reader does not re-derive them. Scaling the probe radius by 1/width makes the
		// footprint exact and changes the error by noise only - neutral, so the shader carries no
		// compensation constant. Spreading the same taps over 0..count-1 makes the footprint
		// exact too and costs real penumbra error, which is a bad trade for a search pass.
		check(worst_comp_gap <= 0.02f, "radius compensation is neutral on error, which is why it is not shipped");
		check(worst_spanned_deviation <= 0.02f, "the spanned subset does fix the footprint");
		check(worst_spanned_gap >= 0.0f, "and pays for it in error, which is why it is not shipped");

		// Layout facts that would silently invalidate every row above.
		check(MAX_SETTING_SAMPLES <= KERNEL_CAPACITY, "the largest setting fits the UBO kernel array");
		check(KERNEL_CAPACITY == MAX_SETTING_SAMPLES, "Ultra is exactly at capacity: a bigger setting needs the array grown first");
	}

	printf("shadow_filter: %d checks, %d failure(s)\n", checks, failures);
	return failures == 0 ? 0 : 1;
}
