# Patch 001 - Pipeline cache telemetry through RenderingServer

# Status: applied upstream of this patch file since commit 087edfa7 (2026-11-22);
#         this note and the diff were written afterwards.
# Priority: P2 (observability)
# Targets: servers/rendering/rendering_server_enums.h
#          servers/rendering/rendering_server.cpp
#          servers/rendering/rendering_server_default.cpp
#          doc/classes/RenderingServer.xml
#          modules/mono/glue/KaizenSharp/KaizenSharp/Generated/KaizenObjects/RenderingServer.cs
# Diff: patches/rendering-server/001-pipeline-cache-telemetry.patch

# What this file is
#   Not a new change. 087edfa7 modified three upstream-derived files to expose four
#   counters of the shared pipeline registry through RenderingServer::get_rendering_info
#   and never got a patch entry, which is the one thing this repository requires of a
#   change to an upstream file. This patch documents that delta as it stands, so it can
#   be reverted by dropping a file, and completes the two halves 087edfa7 left out:
#   the four class-reference entries and the doc comments on the generated C# entries.

# The change, in one paragraph
#   RENDERING_INFO_PIPELINE_CACHE_{LOGICAL_ACQUIRES,SHARED_ACQUIRES,PHYSICAL_CREATES,
#   RELEASES} are appended before RENDERING_INFO_MAX, bound with BIND_ENUM_CONSTANT, and
#   answered in RenderingServerDefault::get_rendering_info from
#   RenderingDevice::pipeline_cache_get_stats(), returning 0 when there is no
#   RenderingDevice singleton. Cumulative counters, not per-frame values - the four
#   neighbours they sit next to in the enum are per-frame, which is why the doc text says
#   so explicitly. Read-only, no behaviour, no ordering, no ownership change.

# Why the missing documentation was worth treating as part of the patch
#   This fork tracks the generated C# glue (1031 files under Generated/), so the build
#   does not run bindings_generator.cpp; when the enum gained four values, the generated
#   enum was edited by hand to match. Hand edits are fine exactly as long as every other
#   source agrees with them, and four of the entries had no doc block at all: the only
#   description of what these counters mean was a C++ comment. The XML entry is the
#   source (the class reference, the editor tooltip and the C# <para> all come from it),
#   so the pair added here is the doc entry plus the mirror the generator would have
#   produced from it - including the [constant X] to <see cref=...> translation.

# Verification
#   - tests/kaizen/api_glue/check_api_glue.py compares all four sources (C++ declaration
#     order, ClassDB binds, doc XML, generated C#) for this enum and fails on name, value
#     or doc-text drift; 18 unittests in test_check_api_glue.py drive it over a synthetic
#     tree with one source broken at a time, including the renumber-without-doc-update
#     case. ./tests/kaizen/run_tests.sh runs both.
#   - Running the checker against the state just before this note reproduces the problem:
#     8 failures (4 missing doc entries, 4 glue entries with nothing behind them).
#   - The diff applies to all five files at 087edfa7^ and reproduces the current tree byte
#     for byte (`patch -p1` plus `cmp` per file). `git apply` from a subdirectory resolves
#     paths against the repository root and is not a valid way to run this check.

# Not verified, deliberately
#   - No engine build exists in this sandbox (no scons, no generated *.gen.h), so the C++
#     half has not been compiled by anything. It is unchanged by this note; the risk this
#     patch adds to the build is zero and the risk it inherits from 087edfa7 is the
#     uncompiled engine code the whole PR shares.
#   - modules/mono/glue/KaizenSharp/KaizenSharp/Generated/NativeCalls.cs carries
#     `kaizen_api_hash = 2389096496`, also hand-edited by 087edfa7 (from 34626250). That
#     value has to equal ClassDB::get_api_hash(ClassDB::API_CORE) - gd_mono.cpp:749 -
#     which only a built engine can compute. It is NOT checked here and NOT claimed
#     correct; a stale value is a C# startup failure, not a silent one. Whoever builds the
#     engine next should regenerate the glue and confirm this line survives it.
#   - PipelineStats also has physical_destroys (kaizen/rendering/pipeline_cache/
#     pipeline_registry.h:86), and it is not exposed. Creates are visible, destroys are
#     not, so "is the cache leaking pipelines?" cannot be asked through
#     get_rendering_info today. Not added: a fifth enumerator means the same hand-edit
#     dance through three more sources, which is a feature decision, not a quality fix.
#     Recorded here so the asymmetry is a known state instead of an oversight.

# Rollback: drop patches/rendering-server/001-pipeline-cache-telemetry.patch - removes the
# four counters and their docs, and takes tests/kaizen/api_glue's spec down to zero entries
# for this enum (the checker would then be checking nothing but its own fixtures, which is
# worth saying out loud before anyone reverts it).
