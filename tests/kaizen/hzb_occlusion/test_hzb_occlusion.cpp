/// tests/kaizen/hzb_occlusion - CPU invariants of the HZB occlusion pyramid.
///
/// WHAT THIS COVERS
/// The three functions the shipped occlusion patches touch:
///   * HZBuffer::update_mips()                   (patches/rendering-scene/001)
///   * RaycastHZBuffer::sort_rays()              (patches/raycast/001)
///   * RaycastHZBuffer::_generate_camera_rays()  (patches/raycast/002)
/// plus HZBuffer::resize() and clear(), which define the buffer the other three work on.
///
/// Each function body is copied VERBATIM out of the engine source at build time by
/// extract_source_bodies.py and compiled here against the shims below. Every case is
/// then compared, bit for bit, against a reference implementation written in the naive
/// pre-patch form - max over the 2x2 block with an offset clamp per sample, a scatter
/// with a bounds test per texel, one division and one field reload per ray. Those three
/// properties are exactly what the optimisations claim to preserve (hoist loop
/// invariants, tabulate a repeated value, clamp a loop bound instead of testing each
/// element), so this suite is the repeatable version of the proof the patch notes
/// describe. Bit-exactness is the point: the claim is bit-exactness.
///
/// WHAT IT DOES NOT COVER
/// It does not link the engine. The shipping translation units include platform_config.h
/// and the *.gen.h headers that only exist after a SCons build, and this suite
/// deliberately does not require one, so LocalVector, RID, Ref, RenderingServer and
/// Engine are shimmed with the semantics the bodies rely on. A change whose behaviour
/// depends on real engine types rather than on these members is out of reach here; the
/// patch notes say what was measured offline and where. The Embree traversal is not
/// covered at all - it is third-party, and it is also why no share-of-frame claim is made.
///
/// If a refactor moves or renames one of these functions, extract_source_bodies.py exits
/// non-zero instead of quietly testing nothing. Update its table and the matching
/// patches/*/*.md note together.

#include <algorithm>
#include <cfloat>
#include <cmath>
#include <cstdint>
#include <cstdio>
#include <cstdlib>
#include <cstring>
#include <vector>

// ---------------------------------------------------------------------------
// Shims for the engine types the extracted bodies use.
// ---------------------------------------------------------------------------

#define MIN(a, b) ((a) < (b) ? (a) : (b))
#define MAX(a, b) ((a) > (b) ? (a) : (b))
#define CLAMP(a, min, max) (MIN(MAX((a), (min)), (max)))

// Godot's Size2 keeps x/width and y/height in the same storage, and the bodies use
// both spellings: sizes[mip].width in update_mips, sizes[0].x in the raycast code.
struct Size2i {
	union {
		int width = 0;
		int x;
	};
	union {
		int height = 0;
		int y;
	};

	Size2i() {}
	Size2i(int p_w, int p_h) {
		width = p_w;
		height = p_h;
	}
	bool operator==(const Size2i &p_other) const { return width == p_other.width && height == p_other.height; }
};

// Mirrors core/math/vector3.h for what the ray generation uses: float components,
// length() through a double sqrt, normalize() guarded by is_finite() and by a zero
// length_squared(), and the scalar-on-the-left multiply.
struct Vector3 {
	float x = 0.0f, y = 0.0f, z = 0.0f;
	Vector3() {}
	Vector3(float p_x, float p_y, float p_z) :
			x(p_x), y(p_y), z(p_z) {}
	Vector3 operator+(const Vector3 &p_with) const { return Vector3(x + p_with.x, y + p_with.y, z + p_with.z); }
	Vector3 operator-(const Vector3 &p_with) const { return Vector3(x - p_with.x, y - p_with.y, z - p_with.z); }
	float length_squared() const {
		float x2 = x * x, y2 = y * y, z2 = z * z;
		return x2 + y2 + z2;
	}
	float dot(const Vector3 &p_with) const { return x * p_with.x + y * p_with.y + z * p_with.z; }
	bool is_finite() const { return std::isfinite(x) && std::isfinite(y) && std::isfinite(z); }
	void zero() { x = y = z = 0.0f; }
	void normalize() {
		if (!is_finite()) {
			zero();
			return;
		}
		float l = length_squared();
		if (l == 0) {
			zero();
		} else {
			l = (float)std::sqrt((double)l);
			x /= l;
			y /= l;
			z /= l;
		}
	}
	Vector3 normalized() const {
		Vector3 v = *this;
		v.normalize();
		return v;
	}
};

static inline Vector3 operator*(float p_scale, const Vector3 &p_vector) {
	return Vector3(p_scale * p_vector.x, p_scale * p_vector.y, p_scale * p_vector.z);
}

// LocalVector as these bodies use it: unsigned size(), resize(), ptr(), clear(),
// is_empty(), operator[].
template <class T>
struct LocalVector {
	std::vector<T> items;
	uint32_t size() const { return (uint32_t)items.size(); }
	bool is_empty() const { return items.empty(); }
	void resize(uint32_t p_num) { items.resize(p_num); }
	void clear() { items.clear(); }
	T &operator[](uint32_t p_index) { return items[p_index]; }
	const T &operator[](uint32_t p_index) const { return items[p_index]; }
	T *ptr() { return items.empty() ? nullptr : items.data(); }
};

struct RID {
	bool valid = false;
	bool is_valid() const { return valid; }
};

struct RefImage {
	bool valid = false;
	bool is_valid() const { return valid; }
	void unref() { valid = false; }
};

struct ServerStub {
	int frees = 0;
	void free_rid(RID p_rid) {
		if (p_rid.valid) {
			frees++;
		}
	}
	static ServerStub *get_singleton() {
		static ServerStub instance;
		return &instance;
	}
};
using RenderingServer = ServerStub;
using RS = ServerStub;

struct EngineStub {
	uint64_t frames_drawn = 0;
	uint64_t get_frames_drawn() const { return frames_drawn; }
	static EngineStub *get_singleton() {
		static EngineStub instance;
		return &instance;
	}
};
using Engine = EngineStub;

#define ERR_FAIL_NULL(m_cond)                              \
	if ((m_cond) == nullptr) {                             \
		printf("FAIL: " #m_cond " is null\n");             \
		return;                                            \
	}
#define ERR_FAIL_COND(m_cond) \
	if (m_cond) {             \
		return;               \
	}

// ---------------------------------------------------------------------------
// The state the extracted bodies read and write. The bodies name these members
// unqualified, so globals resolve them without any macro renaming.
// ---------------------------------------------------------------------------

static LocalVector<float> data;
static LocalVector<Size2i> sizes;
static LocalVector<float *> mips;
static LocalVector<float> debug_data;
static RID debug_texture;
static RefImage debug_image;
static Size2i occlusion_buffer_size;
static uint64_t occlusion_frame = 0;

// The raycast HZBuffer adds tiling and Embree's SoA ray packets.
static const int TILE_SIZE = 4;
static const int TILE_RAYS = TILE_SIZE * TILE_SIZE;
#define RTC_INVALID_GEOMETRY_ID 0xFFFFFFFFu

struct RaySoA {
	float tnear[TILE_RAYS], tfar[TILE_RAYS], time[TILE_RAYS];
	float org_x[TILE_RAYS], org_y[TILE_RAYS], org_z[TILE_RAYS];
	float dir_x[TILE_RAYS], dir_y[TILE_RAYS], dir_z[TILE_RAYS];
	uint32_t mask[TILE_RAYS], flags[TILE_RAYS];
};
struct HitSoA {
	uint32_t geomID[TILE_RAYS];
};
struct CameraRayTile {
	RaySoA ray;
	HitSoA hit;
};
struct CameraRayThreadData {
	int thread_count = 1;
	float z_near = 0.0f, z_far = 0.0f;
	Vector3 camera_pos, camera_dir, pixel_corner, pixel_u_interp, pixel_v_interp;
	bool camera_orthogonal = false;
};

static Size2i tile_grid_size;
static CameraRayTile *camera_rays = nullptr;
static LocalVector<uint32_t> camera_ray_masks;

// The extracted bodies call HZBuffer's own members unqualified (resize() calls
// clear(), sort_rays() calls is_empty()). As free functions in this scope they resolve
// to the same thing, and the definitions come from the generated header.
static bool is_empty() { return sizes.is_empty(); }
static void hzbuffer_clear();
static void clear() { hzbuffer_clear(); }

// The bodies below come from the engine unmodified, so the warnings their style
// provokes are turned off here rather than patched away in the source: upstream
// compares int dimensions against unsigned literals (w == 1U).
#if defined(__GNUC__)
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wsign-compare"
#pragma GCC diagnostic ignored "-Wparentheses"
#endif

#include "hzb_extracted.inc"

#if defined(__GNUC__)
#pragma GCC diagnostic pop
#endif

// ---------------------------------------------------------------------------
// Infrastructure
// ---------------------------------------------------------------------------

static int failures = 0;
static int checks = 0;

static void fail(const char *p_what, int p_w, int p_h) {
	failures++;
	if (failures <= 12) {
		printf("FAIL: %s at %dx%d\n", p_what, p_w, p_h);
	}
}

static uint32_t rng_state = 12345u;
static float next_float() {
	// xorshift32: the same pattern on every platform, which matters because the
	// comparison is bit-exact
	rng_state ^= rng_state << 13;
	rng_state ^= rng_state >> 17;
	rng_state ^= rng_state << 5;
	// A bounded value rather than the raw bits: the inputs stay finite, and a poison
	// pattern outside this range cannot collide with a real one.
	return (float)(rng_state % 100000u) / 1000.0f - 50.0f;
}

// Level offsets in the flat data buffer, the way the engine lays them out: level 0
// first, then each successively halved (rounded up to 1) level appended.
static size_t level_offset(const std::vector<Size2i> &p_levels, size_t p_level) {
	size_t offset = (size_t)p_levels[0].width * p_levels[0].height;
	for (size_t k = 1; k < p_level; k++) {
		offset += (size_t)p_levels[k].width * p_levels[k].height;
	}
	return offset;
}

static std::vector<Size2i> current_levels() {
	std::vector<Size2i> levels(sizes.size());
	for (uint32_t i = 0; i < sizes.size(); i++) {
		levels[i] = sizes[i];
	}
	return levels;
}

// The engine's own resize() computes this geometry; the test keeps a copy of the
// formula so it can allocate the same layout before calling into the extracted code
// and so the reference can index levels without the engine's pointer table.
static void allocate_levels(const Size2i &p_size) {
	sizes.clear();
	mips.clear();
	data.clear();

	int w = p_size.width;
	int h = p_size.height;
	int mip_count = 0;
	int data_size = 0;
	while (true) {
		data_size += h * w;
		w = MAX(1, w >> 1);
		h = MAX(1, h >> 1);
		mip_count++;
		if (w == 1 && h == 1) {
			data_size += 1;
			mip_count++;
			break;
		}
	}

	data.resize(data_size);
	mips.resize(mip_count);
	sizes.resize(mip_count);

	w = p_size.width;
	h = p_size.height;
	float *ptr = data.ptr();
	for (int i = 0; i < mip_count; i++) {
		sizes[i] = Size2i(w, h);
		mips[i] = ptr;
		ptr = &ptr[w * h];
		w = MAX(1, w >> 1);
		h = MAX(1, h >> 1);
	}
}

// ---------------------------------------------------------------------------
// References, in the pre-patch shape on purpose
// ---------------------------------------------------------------------------

// Upstream update_mips(): recompute the previous level's dimensions, its parity and the
// level pointer per texel, and clamp every sampled offset. In place, like the engine:
// level n+1 reads the level this pass just wrote, so a reference that read the original
// buffer would disagree for any pyramid deeper than two levels and prove nothing.
static void ref_update_mips(std::vector<float> &p_buffer, const std::vector<Size2i> &p_levels) {
	for (size_t mip = 1; mip < p_levels.size(); mip++) {
		const size_t prev_offset = (mip == 1) ? 0 : level_offset(p_levels, mip - 1);
		const size_t dst_offset = level_offset(p_levels, mip);
		for (int y = 0; y < p_levels[mip].y; y++) {
			for (int x = 0; x < p_levels[mip].x; x++) {
				int prev_x = x * 2;
				int prev_y = y * 2;
				int prev_w = p_levels[mip - 1].width;
				int prev_h = p_levels[mip - 1].height;
				bool odd_w = (prev_w % 2) != 0;
				bool odd_h = (prev_h % 2) != 0;
				const float *prev = &p_buffer[prev_offset];
#define REF_CHECK_OFFSET(ox, oy)                                                      \
	max_depth = MAX(max_depth, p_buffer[prev_offset + (size_t)MIN(prev_h - 1, prev_y + (oy)) * prev_w + MIN(prev_w - 1, prev_x + (ox))])
				float max_depth = prev[prev_y * prev_w + prev_x];
				REF_CHECK_OFFSET(0, 1);
				REF_CHECK_OFFSET(1, 0);
				REF_CHECK_OFFSET(1, 1);
				if (odd_w) {
					REF_CHECK_OFFSET(2, 0);
					REF_CHECK_OFFSET(2, 1);
				}
				if (odd_h) {
					REF_CHECK_OFFSET(0, 2);
					REF_CHECK_OFFSET(1, 2);
				}
				if (odd_w && odd_h) {
					REF_CHECK_OFFSET(2, 2);
				}
#undef REF_CHECK_OFFSET
				p_buffer[dst_offset + (size_t)y * p_levels[mip].width + x] = max_depth;
			}
		}
	}
}

// Upstream _generate_camera_rays(): no hoisting, divisions per ray, flat tile index.
static void ref_generate_camera_rays(const CameraRayThreadData &p_data, std::vector<CameraRayTile> &p_tiles, const Size2i &p_buffer) {
	for (size_t i = 0; i < p_tiles.size(); i++) {
		CameraRayTile &tile = p_tiles[i];
		int tile_x = ((int)i % tile_grid_size.width) * TILE_SIZE;
		int tile_y = ((int)i / tile_grid_size.width) * TILE_SIZE;

		for (int j = 0; j < TILE_RAYS; j++) {
			int x = tile_x + j % TILE_SIZE;
			int y = tile_y + j / TILE_SIZE;

			float u = (float(x) + 0.5f) / p_buffer.width;
			float v = (float(y) + 0.5f) / p_buffer.height;
			Vector3 pixel_pos = p_data.pixel_corner + u * p_data.pixel_u_interp + v * p_data.pixel_v_interp;

			tile.ray.tnear[j] = p_data.z_near;

			Vector3 dir;
			if (p_data.camera_orthogonal) {
				dir = p_data.camera_dir;
				tile.ray.org_x[j] = pixel_pos.x - dir.x * p_data.z_near;
				tile.ray.org_y[j] = pixel_pos.y - dir.y * p_data.z_near;
				tile.ray.org_z[j] = pixel_pos.z - dir.z * p_data.z_near;
			} else {
				dir = (pixel_pos - p_data.camera_pos).normalized();
				tile.ray.org_x[j] = p_data.camera_pos.x;
				tile.ray.org_y[j] = p_data.camera_pos.y;
				tile.ray.org_z[j] = p_data.camera_pos.z;
				tile.ray.tnear[j] /= dir.dot(p_data.camera_dir);
			}

			tile.ray.dir_x[j] = dir.x;
			tile.ray.dir_y[j] = dir.y;
			tile.ray.dir_z[j] = dir.z;

			tile.ray.tfar[j] = p_data.z_far;
			tile.ray.time[j] = 0.0f;

			tile.ray.flags[j] = 0;
			tile.ray.mask[j] = ~0U;
			tile.hit.geomID[j] = RTC_INVALID_GEOMETRY_ID;
		}
	}
}

// Upstream sort_rays(): bounds test and full index derivation per texel.
static void ref_sort_rays(const std::vector<CameraRayTile> &p_tiles, std::vector<float> &p_level0, const Size2i &p_buffer) {
	for (int i = 0; i < tile_grid_size.height; i++) {
		for (int j = 0; j < tile_grid_size.width; j++) {
			for (int tile_i = 0; tile_i < TILE_SIZE; tile_i++) {
				for (int tile_j = 0; tile_j < TILE_SIZE; tile_j++) {
					int x = j * TILE_SIZE + tile_j;
					int y = i * TILE_SIZE + tile_i;
					if (x >= p_buffer.width || y >= p_buffer.height) {
						continue;
					}
					int k = tile_i * TILE_SIZE + tile_j;
					int tile_index = i * tile_grid_size.width + j;
					p_level0[(size_t)y * p_buffer.width + x] = p_tiles[(size_t)tile_index].ray.tfar[k];
				}
			}
		}
	}
}

// ---------------------------------------------------------------------------
// Cases
// ---------------------------------------------------------------------------

static const int kSizes[][2] = {
	{ 1, 1 }, { 1, 7 }, { 7, 1 }, { 2, 2 }, { 3, 3 }, { 4, 4 }, { 5, 3 },
	{ 8, 8 }, { 9, 9 }, { 13, 271 }, { 40, 40 }, { 83, 147 }, { 147, 83 },
	{ 166, 295 }, { 540, 960 }, { 960, 540 }, { 31, 32 }, { 33, 34 }, { 1022, 1026 },
};
static const int kSizeCount = (int)(sizeof(kSizes) / sizeof(kSizes[0]));

static void test_resize() {
	for (int index = 0; index < kSizeCount; index++) {
		const int w = kSizes[index][0];
		const int h = kSizes[index][1];
		Size2i size(w, h);
		checks++;

		// Start from a cleared buffer, which is the state a viewport's first frame is in.
		hzbuffer_clear();
		hzbuffer_resize(size);

		if (!(sizes[0] == size)) {
			fail("resize() level 0 geometry does not match the requested size", w, h);
		}
		if (occlusion_buffer_size.width != w || occlusion_buffer_size.height != h) {
			fail("resize() did not record the buffer size", w, h);
		}
		if (data.size() == 0) {
			fail("resize() allocated nothing", w, h);
			continue;
		}
		// Every texel of every level starts at FLT_MAX: the regions the screen does not
		// cover have to read as "infinitely far", or culling would treat them as occluded.
		bool all_max = true;
		for (uint32_t i = 0; i < data.size(); i++) {
			if (data[i] != FLT_MAX) {
				all_max = false;
				break;
			}
		}
		if (!all_max) {
			fail("resize() left a texel that is not FLT_MAX", w, h);
		}
		// The halving chain has to end in 1x1 levels, and the pointer table has to stay
		// inside the allocation: update_mips() walks it without bounds checks.
		uint32_t expected_total = 0;
		for (uint32_t mip = 0; mip < sizes.size(); mip++) {
			expected_total += (uint32_t)sizes[mip].width * sizes[mip].height;
			if (sizes[mip].width < 1 || sizes[mip].height < 1) {
				fail("resize() produced an empty level", w, h);
			}
			if (mips[mip] < data.ptr() || &mips[mip][sizes[mip].width * sizes[mip].height] > &data.ptr()[data.size()]) {
				fail("resize() left a level pointer outside the buffer", w, h);
			}
		}
		if (expected_total != data.size()) {
			fail("resize() data size does not match its level table", w, h);
		}
		if (!(sizes[sizes.size() - 1].width == 1 && sizes[sizes.size() - 1].height == 1)) {
			fail("resize() pyramid does not end at 1x1", w, h);
		}

		// An unchanged size must return early and keep the contents: _draw_3d marks the
		// buffer dirty on resize calls, and a realloc per frame would be its own bug.
		if (data.size() > 1) {
			data[1] = 0.25f;
			const float *kept = data.ptr();
			hzbuffer_resize(size);
			if (data.ptr() != kept || data[1] != 0.25f) {
				fail("resize() rebuilt the buffer for an unchanged size", w, h);
			}
		}
	}

	// A zero size clears, and clear() on an empty buffer is a no-op that must not touch
	// the debug texture.
	hzbuffer_clear();
	if (!sizes.is_empty()) {
		fail("resize(Size2i()) did not clear the level table", 0, 0);
	}
	if (ServerStub::get_singleton()->frees != 0) {
		fail("clear() on an already-empty buffer released the debug texture", 0, 0);
	}
	checks++;
}

static void test_update_mips() {
	for (int index = 0; index < kSizeCount; index++) {
		const int w = kSizes[index][0];
		const int h = kSizes[index][1];
		allocate_levels(Size2i(w, h));
		if (data.size() < 2) {
			continue;
		}

		std::vector<float> before(data.size());
		for (uint32_t i = 0; i < data.size(); i++) {
			float value = next_float();
			if (i % 97u == 0u) {
				value = FLT_MAX; // what an uncovered region legitimately holds
			} else if (i % 53u == 0u) {
				value = -1.0f; // and what is nearer than every receiver
			}
			data[i] = value;
			before[i] = value;
		}

		std::vector<Size2i> levels = current_levels();
		std::vector<float> reference = before;
		ref_update_mips(reference, levels);

		// update_mips() also stamps the frame used by the jitter hysteresis.
		EngineStub::get_singleton()->frames_drawn = 4242 + (uint64_t)index;
		hzbuffer_update_mips();
		if (occlusion_frame != 4242u + (uint64_t)index) {
			fail("update_mips() did not record the frame for the occlusion timers", w, h);
		}
		if (memcmp(data.ptr(), reference.data(), sizeof(float) * before.size()) != 0) {
			fail("update_mips() differs from the naive 2x2 max pyramid", w, h);
		}
		// Level 0 is an input. Writing it would mean the scatter and the build disagree
		// about who owns which level.
		for (uint32_t i = 0; i < (uint32_t)(w * h); i++) {
			if (data[i] != before[i]) {
				fail("update_mips() wrote into level 0", w, h);
				break;
			}
		}
		checks++;
	}

	// A hand-computed pyramid, so a mistake in the reference cannot hide by being
	// shared: 0..15 in a 4x4 grid, so the 2x2 maxima are 5, 7, 13, 15 and the top is 15.
	allocate_levels(Size2i(4, 4));
	for (uint32_t i = 0; i < 16; i++) {
		data[i] = (float)i;
	}
	std::fill(&data.ptr()[16], &data.ptr()[21], 0.0f);
	hzbuffer_update_mips();
	if (!(mips[1][0] == 5.0f && mips[1][1] == 7.0f && mips[1][2] == 13.0f && mips[1][3] == 15.0f)) {
		fail("4x4 pyramid is not the hand-computed max of each 2x2 block", 4, 4);
	}
	if (!(sizes[2].width == 1 && mips[2][0] == 15.0f)) {
		fail("4x4 top level is not 15", 4, 4);
	}
	checks++;
}

static void test_sort_rays() {
	for (int index = 0; index < kSizeCount; index++) {
		const int w = kSizes[index][0];
		const int h = kSizes[index][1];
		Size2i size(w, h);
		allocate_levels(size);

		tile_grid_size = Size2i((w + TILE_SIZE - 1) / TILE_SIZE, (h + TILE_SIZE - 1) / TILE_SIZE);
		const size_t tiles = (size_t)tile_grid_size.width * tile_grid_size.height;
		std::vector<CameraRayTile> input(tiles);
		for (size_t t = 0; t < tiles; t++) {
			for (int k = 0; k < TILE_RAYS; k++) {
				input[t].ray.tfar[k] = next_float();
			}
		}

		std::vector<float> expected((size_t)w * h, FLT_MAX);
		ref_sort_rays(input, expected, size);

		// Poison every texel with a value no input can hold, so a texel the extracted
		// code fails to write is a difference rather than an accident of allocation.
		std::fill(&data.ptr()[0], &data.ptr()[0] + (size_t)w * h, -FLT_MAX);
		camera_rays = input.data();
		raycast_sort_rays(Vector3(1.0f, 2.0f, 3.0f), true);

		if (memcmp(data.ptr(), expected.data(), sizeof(float) * expected.size()) != 0) {
			fail("sort_rays() differs from the naive tiled scatter", w, h);
		}
		checks++;
	}
}

static void test_generate_camera_rays() {
	const int cases[] = { 0, 1, 2, 3, 4, 6, 8, 11, 12 };
	for (int index = 0; index < (int)(sizeof(cases) / sizeof(cases[0])); index++) {
		const int w = kSizes[cases[index]][0];
		const int h = kSizes[cases[index]][1];
		Size2i size(w, h);
		allocate_levels(size);
		tile_grid_size = Size2i((w + TILE_SIZE - 1) / TILE_SIZE, (h + TILE_SIZE - 1) / TILE_SIZE);
		const size_t tiles = (size_t)tile_grid_size.width * tile_grid_size.height;

		for (int variant = 0; variant < 6; variant++) {
			CameraRayThreadData d;
			d.camera_orthogonal = (variant % 2) == 1;
			float scale = variant < 2 ? 1.0f : (variant < 4 ? 500.0f : 1e-3f);
			d.z_near = 0.05f * scale;
			d.z_far = 4000.0f * scale;
			srand(77u + (unsigned)variant * 13u + (unsigned)w * 3u + (unsigned)h);
			d.camera_pos = Vector3((rand() / 32767.0f - 0.5f) * scale, (rand() / 32767.0f - 0.5f) * scale, (rand() / 32767.0f - 0.5f) * scale);
			d.camera_dir = Vector3(-0.13f, 0.42f, -0.9f).normalized();
			d.pixel_corner = Vector3(rand() / 32767.0f, rand() / 32767.0f, -d.z_near);
			d.pixel_u_interp = Vector3(scale, 0.0f, scale * 0.1f);
			d.pixel_v_interp = Vector3(0.0f, scale, scale * 0.07f);
			if (variant == 4) {
				// A flat viewport: every ray shares an origin offset.
				d.pixel_u_interp = Vector3(0, 0, 0);
				d.pixel_v_interp = Vector3(0, 0, 0);
			}
			if (variant == 5) {
				// tnear divides by a zero dot product.
				d.camera_dir = Vector3(0, 0, 0);
			}

			std::vector<CameraRayTile> mine(tiles), reference(tiles);
			for (size_t t = 0; t < tiles; t++) {
				for (int k = 0; k < TILE_RAYS; k++) {
					// Distinct poison in every field: a slot one version writes and the
					// other skips has to show up, whichever field it lives in.
					mine[t].ray.tnear[k] = -1.5f;
					mine[t].ray.tfar[k] = -1.5f;
					mine[t].ray.time[k] = -1.5f;
					mine[t].ray.org_x[k] = -1.5f;
					mine[t].ray.org_y[k] = -1.5f;
					mine[t].ray.org_z[k] = -1.5f;
					mine[t].ray.dir_x[k] = -1.5f;
					mine[t].ray.dir_y[k] = -1.5f;
					mine[t].ray.dir_z[k] = -1.5f;
					mine[t].ray.mask[k] = 123u;
					mine[t].ray.flags[k] = 123u;
					mine[t].hit.geomID[k] = 123u;
				}
			}

			camera_rays = mine.data();
			raycast_generate_camera_rays(&d, 0, (int)tiles);
			ref_generate_camera_rays(d, reference, size);

			if (memcmp(mine.data(), reference.data(), sizeof(CameraRayTile) * tiles) != 0) {
				fail("_generate_camera_rays() differs from per-ray reload and division", w, h);
			}
			checks++;
		}
	}
}

int main() {
	if (sizes.is_empty()) {
		// An empty buffer is the state before the first resize: update_mips() must
		// return without touching the pyramid, and it does stamp the frame first,
		// which is what keeps the jitter hysteresis moving while there is nothing to
		// build. Both halves of that behaviour are the contract here.
		EngineStub::get_singleton()->frames_drawn = 7;
		hzbuffer_update_mips();
		if (occlusion_frame != 7) {
			fail("update_mips() did not stamp the frame on an empty buffer", 0, 0);
		}
		if (!data.is_empty()) {
			fail("update_mips() wrote to a buffer it should have skipped", 0, 0);
		}
		checks++;
	}

	test_resize();
	test_update_mips();
	test_sort_rays();
	test_generate_camera_rays();

	if (failures) {
		printf("hzb_occlusion: %d failure(s) over %d case(s)\n", failures, checks);
		return 1;
	}
	printf("hzb_occlusion: %d case(s), 0 failure(s)\n", checks);
	return 0;
}
