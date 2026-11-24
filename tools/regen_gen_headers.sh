#!/bin/bash
# Regenerate the gitignored *.gen.h build artifacts that the sandbox cleaner
# periodically removes. Syntax-only compile checks need these to exist.
# Usage: bash tools/regen_gen_headers.sh
set -e
set -o pipefail
cd /home/user/KaizenEngine
export PATH=/tmp/stubbin:$PATH

TARGETS="core/disabled_classes.gen.h core/version_generated.gen.h core/authors.gen.h core/donors.gen.h core/license.gen.h core/io/certs_compressed.gen.h core/extension/ext_wrappers.gen.h core/extension/gdextension_interface_dump.gen.h core/extension/gdextension_interface.gen.h core/object/gdvirtual.gen.h core/profiling/profiling.gen.h modules/modules_enabled.gen.h servers/rendering/storage/ltc_lut.gen.h"
# Shader gen headers. Root-level *_inc.glsl files get .gen.h targets via
# GLSL_HEADER (shaders/SCsub); *_inc.glsl files in subdirectories are consumed
# raw by the shader builds and have NO .gen.h target.
while IFS= read -r f; do
  TARGETS="$TARGETS ${f}.gen.h"
done < <(find servers/rendering/renderer_rd/shaders -name '*.glsl' | while IFS= read -r g; do
  case "$g" in
    servers/rendering/renderer_rd/shaders/*/*) case "$g" in *_inc.glsl) ;; *) echo "$g" ;; esac ;;
    *) echo "$g" ;;
  esac
done)

/tmp/sconsenv/bin/scons accesskit=no -j2 $TARGETS
