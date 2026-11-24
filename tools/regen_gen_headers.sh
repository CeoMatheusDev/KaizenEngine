#!/bin/bash
# Regenerate the gitignored *.gen.h build artifacts that the sandbox cleaner
# periodically removes. Syntax-only compile checks need these to exist.
# Usage: bash tools/regen_gen_headers.sh
set -e
cd /home/user/KaizenEngine
export PATH=/tmp/stubbin:$PATH

TARGETS="core/disabled_classes.gen.h core/version_generated.gen.h core/authors.gen.h core/donors.gen.h core/license.gen.h core/io/certs_compressed.gen.h core/extension/ext_wrappers.gen.h core/extension/gdextension_interface_dump.gen.h core/extension/gdextension_interface.gen.h core/object/gdvirtual.gen.h core/profiling/profiling.gen.h modules/modules_enabled.gen.h servers/rendering/storage/ltc_lut.gen.h"
# Shader gen headers (all .glsl except include-only files).
while IFS= read -r f; do
  TARGETS="$TARGETS ${f}.gen.h"
done < <(find servers/rendering/renderer_rd/shaders -name '*.glsl' ! -name '*_inc.glsl')

/tmp/sconsenv/bin/scons accesskit=no -j2 $TARGETS 2>&1 | tail -1
