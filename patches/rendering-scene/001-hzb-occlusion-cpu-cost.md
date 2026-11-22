# Patch 001 — HZB Occlusion Culling CPU Cost

# Status: APPLIED IN TREE, MEASURED OFF-TARGET (2026-11-22)
# Priority: R5 item 2 / large-scene scaling
# Target: servers/rendering/renderer_scene_occlusion_cull.cpp
#         servers/rendering/renderer_scene_occlusion_cull.h

# Two changes to the CPU half of occlusion culling. This is the path a project
# enters as soon as it turns on rendering/occlusion_culling/use_occlusion_culling,
# which is exactly what large open-world and indoor/corridor maps do: the cost is
# paid on the cull thread per viewport per frame, next to the per-instance loop in
# RasterizerSceneCull::_scene_cull().

# 1. HZBuffer::update_mips(): hoist the loop-invariant work out of the texel loop.
#    The upstream body re-read sizes[mip - 1].width/.height, recomputed the two
#    parity tests and re-indexed mips[mip - 1] for every single texel, and ran two
#    MIN() per sampled offset to clamp against the previous level's edge. Only the
#    last row and column can read past that edge, so the clamps move to a per-row
#    and per-column branch and the inner loop walks three row pointers.
#
#    Sizing decides what this is worth, and the buffer is NOT screen sized:
#    RendererViewport::_draw_3d() picks the base level as
#    rendering/occlusion_culling/occlusion_rays_per_thread (512) times the worker
#    thread count, then clamps it to one depth texel per 32x32 screen region at the
#    low end and per 2x2 region at the high end. Three cases at 1920x1080 on the
#    24-thread reference machine, cost scaled linearly in texels from the table
#    below (the pyramid is 4/3 of its base level):
#
#      setting            base level   pyramid     upstream   this change   saved
#      512 (default)      147x83       ~16k        ~7.5 us    ~4.2 us       ~3 us
#      2048 (slider max)  295x166      ~65k        ~30 us     ~17 us        ~13 us
#      unbounded          960x540      ~691k       ~320 us    ~180 us       ~140 us
#
#    The unbounded row is the ceiling the engine will not cross, and only reachable
#    through RenderingServer.viewport_set_occlusion_rays_per_thread(): the project
#    setting hint stops at 2048. So the honest in-engine range is single-digit
#    microseconds per viewport per frame at defaults, low tens at the slider
#    maximum, and a fraction of a millisecond if a project deliberately pays for a
#    near-screen pyramid. Quoting the 1920x1080 row as an engine cost would overstate
#    the default case by about two orders of magnitude; benchmarks/rd_3d S8 exists to
#    replace these estimates with measurements.
#    Reachability, because it bounds what this is worth: update_mips() has exactly
#    one caller in the tree, RaycastOcclusionCull::buffer_update()
#    (modules/raycast/raycast_occlusion_cull.cpp:619). The base
#    RendererSceneOcclusionCull::buffer_update() is an empty virtual
#    (renderer_scene_occlusion_cull.h:245), so without modules/raycast built there is
#    no pyramid at all: buffer_get_ptr() returns null, the OCCLUSION_CULLED macro
#    short-circuits on that first test, and enabling occlusion culling costs nothing
#    and culls nothing. Same shape upstream (checked against godotengine/godot 4.6):
#    RendererSceneCull's constructor creates the dummy unconditionally and the module
#    instance overwrites the singleton afterwards, because
#    initialize_modules(MODULE_INITIALIZATION_LEVEL_SCENE) runs after the rendering
#    server is constructed (main/main.cpp:794 vs :774).
#    Within that reachable path, buffer_update() is Embree ray casting over the
#    scenario (scenario.raycast + sort_rays) plus this mip build, so update_mips() is
#    one term of it, not the whole. The percentages below are of that term. The term
#    beside it - the level-0 scatter that fills the buffer these mips are built from -
#    is patched separately in patches/raycast/001-hzb-level0-scatter.md, and it
#    touches about three times as many texels per frame as update_mips() does.
#
# 2. HZBuffer::_is_occluded(): test the near plane before projecting the corner.
#    The corner was run through p_cam_projection and the result thrown away when
#    -view.z < 0.0 breaks out of the 8-corner loop. Moving the projection after
#    the test saves a matrix multiply per corner for any instance whose AABB
#    straddles the camera plane, which is the normal case for geometry a player
#    walks into, i.e. the indoor case. No semantic change: projected is not read
#    on the break path and xform() has no side effects.

# Measured cost (the only measurement available so far):
#    Both bodies were extracted verbatim into a standalone harness on plain
#    std::vector mip chains and timed on this sandbox's 2-core Xeon @ 2.60GHz
#    (no GPU, no engine build), 1920x1080 pyramid, best of 5 blocks x 30 frames:
#      -O2: 1.274 ms -> 0.720 ms per frame  (-43.5 %, 1.77x)
#      -O3: 1.284 ms -> 0.882 ms per frame  (-31.3 %, 1.45x)
#    The 1920x1080 size is a worst case for the function, not a configuration the
#    engine can reach (see the ceiling row in item 1); it is reported because it
#    isolates the per-texel win. Scale it down by the texel ratio for in-engine use.
#    Output compared bit-for-bit against the upstream body over 15 fixed sizes
#    (1x1, 2x2, 3x3, 5x7, 1x1080, 1920x1, 1023x1023, 127x255, odd and even
#    chains) plus 40 random sizes up to 2048x2048 with random depth values
#    including FLT_MAX: identical in all 55 cases.
#    These are harness numbers, not in-engine numbers: the engine adds the
#    per-instance test, the raycast module's traversal and the readback around
#    it. Treat the percentage as the win on update_mips() alone and read the sizing
#    note in item 1 before quoting any per-frame number.

# Deliberately NOT done:
#    ceil(log2(size)) for the starting LOD in _is_occluded() is a libm call per
#    instance and looks trivially replaceable by reading the float exponent and
#    adding one when the mantissa is non-zero. It is not equivalent: for the
#    smallest float above a power of two, log2f() rounds to exactly that integer
#    and the upstream expression returns one level lower. The result is still a
#    valid LOD (coarser, so more conservative for culling) but it is a behaviour
#    change, and on this path a changed LOD choice changes which instances get
#    culled. Left alone until there is an in-engine A/B to justify it.
#    The dummy/base RendererSceneOcclusionCull keeps update_mips() with no caller
#    outside modules/raycast, so the gain lands in Embree-backed occlusion
#    culling builds (default on x86_64/arm64 desktop), not in the stub.

# Verification:
#   - Extracted function compiles clean under -Wall -Wextra -Wshadow -Wconversion
#     at -O2 and -O3 (0 diagnostics from the function body itself).
#   - Bit-identity: 55/55 mip chains, see above.
#   - Outstanding: a full engine build. The sandbox has no scons installed and no
#     generated *.gen.h headers, so the two edited files were never fed to the real
#     build; the extracted-body compile is a weaker check. Needs a large-scene frame
#     capture with occlusion culling on to confirm the win survives the code around it.

# Rollback: drop patches/rendering-scene/001-hzb-occlusion-cpu-cost.patch.
