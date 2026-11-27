#!/usr/bin/env bash
#
# Kaizen Engine — Linux packaging driver.
#
# Assembles an official distribution tree and (by default) a deterministic
# .tar.xz archive from the artifacts produced by the SCons build.
#
# Output layout (relative to the archive root, "Kaizen/"):
#
#   Engine/Binaries/Linux/           engine launcher, canonical binary, portable data
#   Engine/Source/                   only with --with-source
#   ExportTemplates/Linux/{Release,Debug}/
#   Templates/<version>/             install-ready symlink view (no duplication)
#   Build/Config/                    build-info.json, package-info.env
#   Build/Toolchains/                toolchain notes
#   Config/linux/                    desktop entry, AppStream, MIME, man page, icons
#   LICENSES/                        Kaizen licenses
#   Samples/                         Lua sample project
#   Setup/                           installer, uninstaller, selector, dependency check
#   README.txt
#   MANIFEST.sha256
#
# Usage:
#   ./packaging/linux/package.sh [options]
#
# Options:
#       --version X.Y.Z        Package version base (default: version.py)
#       --channel NAME         stable|beta|rc|dev (default: version.py status)
#       --channel-revision N   Pre-release number (default: 1)
#   -o, --output DIR           Directory distribution (default: dist/Kaizen)
#       --tarball-out DIR      Archive output directory (default: dist/)
#       --no-dir               Do not write the directory distribution
#       --no-tarball           Do not write the .tar.xz archive
#       --tarball-name NAME    Override the archive file name (testing)
#       --with-source          Include the engine source tree
#       --reproducible         Pin timestamps (SOURCE_DATE_EPOCH wins if set)
#       --xz-preset N          xz compression preset 0-9 (default: 6)
#   -j, --jobs N               Compression threads (default: nproc)
#   -f, --force                Replace existing outputs
#       --dry-run              Print what would be done without writing
#   -h, --help                 Show this help

set -euo pipefail

# shellcheck source=../common.sh
source "$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)/../common.sh"

usage() {
	sed -n '2,48p' "$0" | sed 's/^# \{0,1\}//'
	exit "${1:-0}"
}

# --- Defaults ------------------------------------------------------------
OUTPUT="${KZ_DIST_DIR}"
TARBALL_OUT=""
WRITE_DIR="yes"
WRITE_TARBALL="yes"
TARBALL_NAME=""
FORCE="no"
WITH_SOURCE="no"
DRY_RUN="no"
REPRODUCIBLE="no"
XZ_PRESET="6"
JOBS="$(kz_nproc)"
VERSION_BASE=""
CHANNEL=""
CHANNEL_REVISION="1"
ALLOW_MISMATCH="no"

while [[ $# -gt 0 ]]; do
	case "$1" in
	--version)
		[[ $# -ge 2 ]] || kz_die "--version requires a value."
		VERSION_BASE="$2"
		shift 2
		;;
	--version=*)
		VERSION_BASE="${1#*=}"
		shift
		;;
	--channel)
		[[ $# -ge 2 ]] || kz_die "--channel requires a value."
		CHANNEL="$2"
		shift 2
		;;
	--channel=*)
		CHANNEL="${1#*=}"
		shift
		;;
	--channel-revision)
		[[ $# -ge 2 ]] || kz_die "--channel-revision requires a value."
		CHANNEL_REVISION="$2"
		shift 2
		;;
	--channel-revision=*)
		CHANNEL_REVISION="${1#*=}"
		shift
		;;
	-o | --output)
		[[ $# -ge 2 ]] || kz_die "--output requires a value."
		OUTPUT="$2"
		shift 2
		;;
	--output=*)
		OUTPUT="${1#*=}"
		shift
		;;
	--tarball-out)
		[[ $# -ge 2 ]] || kz_die "--tarball-out requires a value."
		TARBALL_OUT="$2"
		shift 2
		;;
	--tarball-out=*)
		TARBALL_OUT="${1#*=}"
		shift
		;;
	--tarball-name)
		[[ $# -ge 2 ]] || kz_die "--tarball-name requires a value."
		TARBALL_NAME="$2"
		shift 2
		;;
	--tarball-name=*)
		TARBALL_NAME="${1#*=}"
		shift
		;;
	--no-dir)
		WRITE_DIR="no"
		shift
		;;
	--no-tarball)
		WRITE_TARBALL="no"
		shift
		;;
	--with-source)
		WITH_SOURCE="yes"
		shift
		;;
	--reproducible)
		REPRODUCIBLE="yes"
		shift
		;;
	--allow-channel-mismatch)
		ALLOW_MISMATCH="yes"
		shift
		;;
	--xz-preset)
		[[ $# -ge 2 ]] || kz_die "--xz-preset requires a value."
		XZ_PRESET="$2"
		shift 2
		;;
	--xz-preset=*)
		XZ_PRESET="${1#*=}"
		shift
		;;
	-j | --jobs)
		[[ $# -ge 2 ]] || kz_die "--jobs requires a value."
		JOBS="$2"
		shift 2
		;;
	--jobs=*)
		JOBS="${1#*=}"
		shift
		;;
	-f | --force)
		FORCE="yes"
		shift
		;;
	--dry-run)
		DRY_RUN="yes"
		shift
		;;
	-h | --help)
		usage 0
		;;
	*)
		kz_die "Unknown option: $1 (use --help)"
		;;
	esac
done

kz_require_repo
kz_need_cmd python3 find sort sha256sum xz
[[ "${WRITE_TARBALL}" == "yes" ]] && kz_need_cmd tar

[[ "${XZ_PRESET}" =~ ^[0-9]$ ]] || kz_die "--xz-preset must be 0-9."
[[ "${JOBS}" =~ ^[1-9][0-9]*$ ]] || kz_die "--jobs must be a positive integer."
[[ "${CHANNEL_REVISION}" =~ ^[1-9][0-9]*$ ]] || kz_die "--channel-revision must be a positive integer."
# --- Explicit version/channel metadata ----------------------------------
read -r PY_MAJOR PY_MINOR PY_PATCH PY_STATUS <<<"$(
	python3 - "${KZ_REPO_ROOT}" <<'PY'
import sys
sys.path.insert(0, sys.argv[1])
import version
print(version.major, version.minor, version.patch, version.status)
PY
)"

PY_BASE="${PY_MAJOR}.${PY_MINOR}.${PY_PATCH}"
[[ -n "${VERSION_BASE}" ]] || VERSION_BASE="${PY_BASE}"
kz_semver_ok "${VERSION_BASE}" || kz_die "Invalid --version '${VERSION_BASE}'. Expected X.Y.Z."

if [[ -z "${CHANNEL}" ]]; then
	CHANNEL="$(kz_status_to_channel "${PY_STATUS}")"
	[[ -n "${CHANNEL}" ]] || kz_die "Cannot map version.py status '${PY_STATUS}' to a channel. Pass --channel explicitly."
fi
kz_channel_ok "${CHANNEL}" || kz_die "Invalid --channel '${CHANNEL}'. Use stable, beta, rc or dev."

if [[ "${CHANNEL}" == "stable" ]]; then
	PACKAGE_VERSION="${VERSION_BASE}"
	CHANNEL_REVISION="0"
else
	PACKAGE_VERSION="$(kz_package_version "${VERSION_BASE}" "${CHANNEL}" "${CHANNEL_REVISION}")"
fi

kz_info "Packaging Kaizen"

EDITOR_BIN="$(kz_resolve_binary editor)" || kz_die "Editor binary not found. Run ./packaging/linux/build.sh first."
RELEASE_BIN="$(kz_resolve_binary template_release)" || kz_die "Release template not found. Run ./packaging/linux/build.sh first."
DEBUG_BIN="$(kz_resolve_binary template_debug)" || kz_die "Debug template not found. Run ./packaging/linux/build.sh first."

FULL_BUILD="$(kz_version_full_build "${EDITOR_BIN}")" || kz_die "Could not determine the engine version from ${EDITOR_BIN}."
FULL_CONFIG="$(kz_version_full_config "${FULL_BUILD}")"

# The engine exposes its own compiled status; refuse to label a package with a
# channel that contradicts the binary unless the caller insists.
engine_channel() {
	case "$1" in
	*".beta."* | *".beta") printf 'beta' ;;
	*".rc."* | *".rc") printf 'rc' ;;
	*".dev."* | *".dev") printf 'dev' ;;
	*) printf 'stable' ;;
	esac
}
ENGINE_CHANNEL="$(engine_channel "${FULL_CONFIG}")"
if [[ "${ENGINE_CHANNEL}" != "${CHANNEL}" && "${ALLOW_MISMATCH}" != "yes" ]]; then
	kz_die "Channel mismatch: engine reports '${ENGINE_CHANNEL}' but --channel is '${CHANNEL}'. Use --allow-channel-mismatch to override."
fi
ENGINE_MM="$(printf '%s' "${FULL_CONFIG}" | sed -n 's/^\([0-9][0-9]*\.[0-9][0-9]*\).*/\1/p')"
PKG_MM="${VERSION_BASE%.*}"
if [[ -n "${ENGINE_MM}" && "${ENGINE_MM}" != "${PKG_MM}" && "${ALLOW_MISMATCH}" != "yes" ]]; then
	kz_die "Version mismatch: engine is ${ENGINE_MM}.x but package version is ${VERSION_BASE}. Use --allow-channel-mismatch to override."
fi

PACKAGE_NAME="$(kz_package_name "${PACKAGE_VERSION}")"
[[ -n "${TARBALL_NAME}" ]] || TARBALL_NAME="${PACKAGE_NAME}.tar.xz"
[[ "${TARBALL_NAME}" == *.tar.xz ]] || kz_die "--tarball-name must end with .tar.xz."
[[ -n "${TARBALL_OUT}" ]] || TARBALL_OUT="$(dirname "${OUTPUT}")"

EPOCH="$(kz_epoch "${REPRODUCIBLE}")"

kz_info "  version    : ${PACKAGE_VERSION}"
kz_info "  channel    : ${CHANNEL}${CHANNEL_REVISION:+ (revision ${CHANNEL_REVISION})}"
kz_info "  engine     : ${FULL_BUILD}"
kz_info "  templates  : ${FULL_CONFIG}"
kz_info "  directory  : $([[ "${WRITE_DIR}" == "yes" ]] && echo "${OUTPUT}" || echo "(skipped)")"
kz_info "  archive    : $([[ "${WRITE_TARBALL}" == "yes" ]] && echo "${TARBALL_OUT%/}/${TARBALL_NAME}" || echo "(skipped)")"

if [[ "${DRY_RUN}" == "yes" ]]; then
	kz_info "Dry run — no files written."
	exit 0
fi

# --- Output safety -------------------------------------------------------
if [[ "${WRITE_DIR}" == "yes" && -e "${OUTPUT}" && -n "$(ls -A "${OUTPUT}" 2>/dev/null)" && "${FORCE}" != "yes" ]]; then
	kz_die "Output directory is not empty: ${OUTPUT}. Use --force to replace it."
fi
if [[ "${WRITE_TARBALL}" == "yes" && -e "${TARBALL_OUT%/}/${TARBALL_NAME}" && "${FORCE}" != "yes" ]]; then
	kz_die "Archive already exists: ${TARBALL_OUT%/}/${TARBALL_NAME}. Use --force to replace it."
fi

mkdir -p "$(dirname "${OUTPUT}")"
# Stage outside the repository so that tools watching the workspace do not pick up
# files from the staged tree before the manifest is written.
STAGE_ROOT="$(mktemp -d "${TMPDIR:-/tmp}/kaizen-package.XXXXXX")"
STAGE="${STAGE_ROOT}/Kaizen"
TARBALL_TMP=""
cleanup() {
	[[ -n "${TARBALL_TMP}" && -e "${TARBALL_TMP}" ]] && rm -f "${TARBALL_TMP}"
	rm -rf "${STAGE_ROOT}"
}
trap cleanup EXIT

mkdir -p "${STAGE}/Engine/Binaries/${KZ_OS_DIR}" \
	"${STAGE}/ExportTemplates/${KZ_OS_DIR}/Release" \
	"${STAGE}/ExportTemplates/${KZ_OS_DIR}/Debug" \
	"${STAGE}/Templates/${FULL_CONFIG}" \
	"${STAGE}/Build/Config" \
	"${STAGE}/Build/Toolchains" \
	"${STAGE}/Config/linux/icons/hicolor/scalable/apps" \
	"${STAGE}/Config/linux/icons/hicolor/256x256/apps" \
	"${STAGE}/LICENSES" \
	"${STAGE}/Setup" \
	"${STAGE}/Samples/hello-kaizen"

# --- Engine binaries -----------------------------------------------------
BIN_DIR="${STAGE}/Engine/Binaries/${KZ_OS_DIR}"
EDITOR_NAME="$(basename "${EDITOR_BIN}")"
install -m 0755 "${EDITOR_BIN}" "${BIN_DIR}/${EDITOR_NAME}"

# Portable/installed launcher. It resolves symlinks and never depends on the
# current working directory.
cat >"${BIN_DIR}/kaizen" <<EOF
#!/bin/sh
# Kaizen Engine launcher (generated by package.sh).
set -e
self="\$0"
while [ -L "\$self" ]; do
	dir=\$(cd -P "\$(dirname "\$self")" >/dev/null 2>&1 && pwd)
	self=\$(readlink "\$self")
	case "\$self" in
	/*) ;;
	*) self="\$dir/\$self" ;;
	esac
done
dir=\$(cd -P "\$(dirname "\$self")" >/dev/null 2>&1 && pwd)
exec "\$dir/${EDITOR_NAME}" "\$@"
EOF
chmod 0755 "${BIN_DIR}/kaizen"

# Portable mode marker: the editor stores its data in editor_data/ next to the
# executable, so the tree can run from removable media/other machines.
: >"${BIN_DIR}/_sc_"

# --- Export templates ----------------------------------------------------
install -m 0755 "${RELEASE_BIN}" "${STAGE}/ExportTemplates/${KZ_OS_DIR}/Release/linux_release.${KZ_ARCH}"
install -m 0755 "${DEBUG_BIN}" "${STAGE}/ExportTemplates/${KZ_OS_DIR}/Debug/linux_debug.${KZ_ARCH}"

# Portable build: expose templates where the editor looks for them in
# self-contained mode (editor_data/export_templates/<version>/).
TEMPLATES_DIR="${BIN_DIR}/editor_data/export_templates/${FULL_CONFIG}"
mkdir -p "${TEMPLATES_DIR}"
ln -sfn "../../../../../../ExportTemplates/${KZ_OS_DIR}/Release/linux_release.${KZ_ARCH}" \
	"${TEMPLATES_DIR}/linux_release.${KZ_ARCH}"
ln -sfn "../../../../../../ExportTemplates/${KZ_OS_DIR}/Debug/linux_debug.${KZ_ARCH}" \
	"${TEMPLATES_DIR}/linux_debug.${KZ_ARCH}"

# Install-ready view: one directory named after the engine config, containing
# relative symlinks only (never duplicate template binaries).
ln -sfn "../../ExportTemplates/${KZ_OS_DIR}/Release/linux_release.${KZ_ARCH}" \
	"${STAGE}/Templates/${FULL_CONFIG}/linux_release.${KZ_ARCH}"
ln -sfn "../../ExportTemplates/${KZ_OS_DIR}/Debug/linux_debug.${KZ_ARCH}" \
	"${STAGE}/Templates/${FULL_CONFIG}/linux_debug.${KZ_ARCH}"

# --- Desktop integration, icons, licenses -------------------------------
DIST_DIR="${KZ_REPO_ROOT}/misc/dist/linux"
CONFIG_LINUX="${STAGE}/Config/linux"
for f in org.kaizenengine.Kaizen.desktop org.kaizenengine.Kaizen.appdata.xml kaizen.6 org.kaizenengine.Kaizen.xml; do
	[[ -f "${DIST_DIR}/${f}" ]] && install -m 0644 "${DIST_DIR}/${f}" "${CONFIG_LINUX}/"
done
[[ -f "${KZ_REPO_ROOT}/misc/logo/icon.svg" ]] && install -m 0644 "${KZ_REPO_ROOT}/misc/logo/icon.svg" "${CONFIG_LINUX}/icons/hicolor/scalable/apps/kaizen.svg"
[[ -f "${KZ_REPO_ROOT}/misc/logo/icon.png" ]] && install -m 0644 "${KZ_REPO_ROOT}/misc/logo/icon.png" "${CONFIG_LINUX}/icons/hicolor/256x256/apps/kaizen.png"

for legal in LICENSE.txt COPYRIGHT.txt AUTHORS.md; do
	[[ -f "${KZ_REPO_ROOT}/${legal}" ]] && install -m 0644 "${KZ_REPO_ROOT}/${legal}" "${STAGE}/LICENSES/"
done

# --- Sample projects -----------------------------------------------------
copy_sample() { # <source-dir> <destination-name>
	local src="$1" name="$2"
	[[ -d "${src}" ]] || return 0
	mkdir -p "${STAGE}/Samples/${name}"
	tar -C "${src}" \
		--exclude='./.kaizen' --exclude='./out' --exclude='./export' \
		--exclude='./bin' --exclude='./obj' \
		--exclude='.git' --exclude='*.tmp' \
		-cf - . | tar -C "${STAGE}/Samples/${name}" -xf -
}

copy_sample "${KZ_REPO_ROOT}/packaging/samples/hello-kaizen" "hello-kaizen"

# --- Installer / uninstaller --------------------------------------------
for helper in install.sh uninstall.sh select.sh check-deps.sh; do
	[[ -f "${KZ_REPO_ROOT}/packaging/linux/${helper}" ]] &&
		install -m 0755 "${KZ_REPO_ROOT}/packaging/linux/${helper}" "${STAGE}/Setup/${helper}"
done
cat >"${STAGE}/Setup/README.md" <<'EOF'
# Kaizen Engine — Setup

## Portable use

Run `Engine/Binaries/Linux/kaizen` directly. The `_sc_` marker keeps editor data
inside `Engine/Binaries/Linux/editor_data/`.

## Installation

```sh
# per-user (default, no privileges required)
./Setup/install.sh

# per-user from the official archive (verifies the external checksum)
./Setup/install.sh --from-tarball ../kaizen-x86_64-linux-<version>.tar.xz

# system-wide (requires root)
sudo ./Setup/install.sh --system
```

Installed layout (per-user mode):

```text
~/.local/opt/kaizen/<channel>/<version>/   engine, licenses
~/.local/opt/kaizen/<channel>/current -> <version>
~/.local/bin/kaizen                        default channel
~/.local/bin/kaizen-<channel>              per-channel launcher
~/.local/share/kaizen/export_templates/    export templates (shared)
```

Select the default channel or version:

```sh
kaizen-select --list
kaizen-select beta
kaizen-select beta/4.8.0-beta1
```

Uninstall:

```sh
./Setup/uninstall.sh --list
./Setup/uninstall.sh --channel beta --version 4.8.0-beta1
./Setup/uninstall.sh --all --yes
```

User projects, editor settings and caches are never removed.
EOF

# --- Root README ---------------------------------------------------------
cat >"${STAGE}/README.txt" <<EOF
Kaizen Engine ${PACKAGE_VERSION}
================================

Channel  : ${CHANNEL}
Engine   : ${FULL_BUILD}
Platform : linux ${KZ_ARCH}
Archive  : ${TARBALL_NAME}

Portable use
------------
Run the launcher directly; no installation is required:

  ./Engine/Binaries/Linux/kaizen

The '_sc_' marker makes the editor keep its data in
Engine/Binaries/Linux/editor_data/. Export templates are resolved through the
relative symlinks in that directory.

Installation
------------
Per-user (recommended, no privileges required):

  ./Setup/install.sh

From the official archive, with checksum verification:

  ./Setup/install.sh --from-tarball ./${TARBALL_NAME}

System-wide (optional, requires root):

  sudo ./Setup/install.sh --system

After installation the 'kaizen' command starts the selected default channel.
See Setup/README.md for coexistence, selection and removal.

Layout
------
  Engine/Binaries/Linux/     engine executables and portable data
  ExportTemplates/           Linux export templates (Release/Debug)
  Templates/${FULL_CONFIG}/  install-ready template view (symlinks)
  Build/Config/              build and package metadata
  Config/linux/              desktop entry, MIME, AppStream, man page, icons
  LICENSES/                  Kaizen and third-party licenses
  Samples/                   Lua sample project
  Setup/                     installer, uninstaller, selector, dependency check
EOF

# --- Optional source tree ------------------------------------------------
if [[ "${WITH_SOURCE}" == "yes" ]]; then
	kz_info "Copying source tree into Engine/Source ..."
	mkdir -p "${STAGE}/Engine/Source"
	tar -C "${KZ_REPO_ROOT}" \
		--exclude='./bin' \
		--exclude='./dist' \
		--exclude='./doc/translations' \
		--exclude='./.sconsign5.dblite' \
		--exclude='./.scons_env.json' \
		--exclude='./.git' \
		--exclude='__pycache__' \
		--exclude='*.pyc' \
		--exclude='obj' \
		--exclude='*.o' \
		--exclude='*.a' \
		--exclude='*.lib' \
		--exclude='*.nupkg' \
		--exclude='*.snupkg' \
		--exclude='*.tar.xz' \
		-cf - . | tar -C "${STAGE}/Engine/Source" -xf -
fi

# --- Toolchain notes -----------------------------------------------------
cat >"${STAGE}/Build/Toolchains/README.md" <<'EOF'
# Toolchains

Linux (validated in this repository):

- GCC >= 11 or Clang >= 9 (`use_llvm=yes`)
- SCons >= 4.x and Python 3.9+

Windows (prepared, not validated here):

- Visual Studio 2022+ (MSVC 14.3x) or MinGW-w64 (`use_mingw=yes`)
- Python 3.9+ and SCons >= 4.x

The Windows package must be produced on Windows (native toolchain); SCons can
cross-compile with MinGW-w64, but that path is not validated by this project.
EOF

# --- Metadata ------------------------------------------------------------
cat >"${STAGE}/Build/Config/package-info.env" <<EOF
PRODUCT=Kaizen
PACKAGE_NAME=${PACKAGE_NAME}
PACKAGE_VERSION=${PACKAGE_VERSION}
PACKAGE_VERSION_BASE=${VERSION_BASE}
CHANNEL=${CHANNEL}
CHANNEL_REVISION=${CHANNEL_REVISION}
ENGINE_VERSION=${FULL_BUILD}
TEMPLATES_CONFIG=${FULL_CONFIG}
SOURCE_INCLUDED=${WITH_SOURCE}
ARCH=${KZ_ARCH}
PLATFORM=linux
FORMAT=tar.xz
ROOT=Kaizen
EOF


export KZ_MANIFEST_OUT="${STAGE}/Build/Config/build-info.json"
export KZ_MANIFEST_ROOT="${STAGE}"
export KZ_MANIFEST_FULL_BUILD="${FULL_BUILD}"
export KZ_MANIFEST_FULL_CONFIG="${FULL_CONFIG}"
export KZ_MANIFEST_PLATFORM="${KZ_PLATFORM}"
export KZ_MANIFEST_OS_DIR="${KZ_OS_DIR}"
export KZ_MANIFEST_ARCH="${KZ_ARCH}"
export KZ_MANIFEST_REPO="${KZ_REPO_ROOT}"
export KZ_MANIFEST_SOURCE_INCLUDED="${WITH_SOURCE}"
export KZ_MANIFEST_EPOCH="${EPOCH}"
export KZ_MANIFEST_REPRODUCIBLE="${REPRODUCIBLE}"
export KZ_MANIFEST_PACKAGE_VERSION="${PACKAGE_VERSION}"
export KZ_MANIFEST_CHANNEL="${CHANNEL}"
export KZ_MANIFEST_PACKAGE_NAME="${PACKAGE_NAME}"

python3 - <<'PY'
import hashlib
import json
import os
import platform
import time
from pathlib import Path

root = Path(os.environ["KZ_MANIFEST_ROOT"])
repo = Path(os.environ["KZ_MANIFEST_REPO"])
reproducible = os.environ["KZ_MANIFEST_REPRODUCIBLE"] == "yes"

cc = cxx = "unknown"
env_json = repo / ".scons_env.json"
if env_json.is_file():
    try:
        data = json.loads(env_json.read_text(encoding="utf-8"))
        cc = data.get("CC", cc)
        cxx = data.get("CXX", cxx)
    except Exception:
        pass

files = []
symlinks = []
for path in sorted(root.rglob("*")):
    rel = path.relative_to(root).as_posix()
    if path.is_symlink():
        symlinks.append({"path": rel, "target": os.readlink(path)})
    elif path.is_file():
        size = path.stat().st_size
        digest = hashlib.sha256()
        with path.open("rb") as fh:
            for chunk in iter(lambda: fh.read(1024 * 1024), b""):
                digest.update(chunk)
        files.append({"path": rel, "size": size, "sha256": digest.hexdigest()})

epoch = int(os.environ["KZ_MANIFEST_EPOCH"])
manifest = {
    "schema": 2,
    "package": {
        "name": os.environ["KZ_MANIFEST_PACKAGE_NAME"],
        "version": os.environ["KZ_MANIFEST_PACKAGE_VERSION"],
        "channel": os.environ["KZ_MANIFEST_CHANNEL"],
        "format": "tar.xz",
        "root": "Kaizen",
    },
    "engine": {
        "name": "Kaizen Engine",
        "short_name": "kaizen",
        "version_full_build": os.environ["KZ_MANIFEST_FULL_BUILD"],
        "version_full_config": os.environ["KZ_MANIFEST_FULL_CONFIG"],
    },
    "build": {
        "platform": os.environ["KZ_MANIFEST_PLATFORM"],
        "os_dir": os.environ["KZ_MANIFEST_OS_DIR"],
        "arch": os.environ["KZ_MANIFEST_ARCH"],
        "host": None if reproducible else platform.platform(),
        "cc": cc,
        "cxx": cxx,
        "source_included": os.environ["KZ_MANIFEST_SOURCE_INCLUDED"] == "yes",
        "generated_at": time.strftime("%Y-%m-%dT%H:%M:%SZ", time.gmtime(epoch)),
        "reproducible": reproducible,
    },
    "files": files,
    "symlinks": symlinks,
}

out = Path(os.environ["KZ_MANIFEST_OUT"])
out.write_text(json.dumps(manifest, indent=2) + "\n", encoding="utf-8")
print(f"manifest: {len(files)} files, {len(symlinks)} symlinks")
PY

# --- MANIFEST.sha256 -----------------------------------------------------
# Covers regular files only; symlinks are validated structurally by the
# installer and the validator (relative targets, contained in the root).
(
	cd "${STAGE}"
	find . -type f ! -path './MANIFEST.sha256' -printf '%P\0' |
		LC_ALL=C sort -z |
		xargs -0 -r sha256sum
) >"${STAGE}/MANIFEST.sha256"
kz_ok "Manifest: $(wc -l <"${STAGE}/MANIFEST.sha256") files"

# --- Archive -------------------------------------------------------------
if [[ "${WRITE_TARBALL}" == "yes" ]]; then
	mkdir -p "${TARBALL_OUT}"
	TARBALL_TMP="${TARBALL_OUT%/}/.${TARBALL_NAME}.tmp.$$"
	kz_info "Creating ${TARBALL_NAME} (xz -${XZ_PRESET}, ${JOBS} threads) ..."
	tar -C "${STAGE_ROOT}" \
		--sort=name \
		--mtime="@${EPOCH}" \
		--owner=0 --group=0 --numeric-owner \
		--format=posix \
		--pax-option=exthdr.name=%d/PaxHeaders/%f,delete=atime,delete=ctime \
		--no-acls --no-selinux --no-xattrs \
		--hard-dereference \
		-cf - Kaizen |
		xz -T"${JOBS}" -"${XZ_PRESET}" -c >"${TARBALL_TMP}"
	mv -f "${TARBALL_TMP}" "${TARBALL_OUT%/}/${TARBALL_NAME}"
	TARBALL_TMP=""
	(
		cd "${TARBALL_OUT}"
		sha256sum "${TARBALL_NAME}" >"${TARBALL_NAME}.sha256"
	)
	kz_ok "Archive: ${TARBALL_OUT%/}/${TARBALL_NAME} ($(du -h "${TARBALL_OUT%/}/${TARBALL_NAME}" | cut -f1))"
	kz_ok "Checksum: ${TARBALL_OUT%/}/${TARBALL_NAME}.sha256"
fi

# --- Swap directory into place ------------------------------------------
if [[ "${WRITE_DIR}" == "yes" ]]; then
	if [[ -e "${OUTPUT}" ]]; then
		BACKUP="${OUTPUT}.old.$$"
		rm -rf "${BACKUP}"
		mv "${OUTPUT}" "${BACKUP}"
		mv "${STAGE}" "${OUTPUT}"
		rm -rf "${BACKUP}"
	else
		mv "${STAGE}" "${OUTPUT}"
	fi
	kz_ok "Distribution ready: ${OUTPUT}"
	kz_info "Tree:"
	find "${OUTPUT}" -maxdepth 2 -mindepth 1 -type d | sort | sed "s|^${OUTPUT}|  .|"
fi

trap - EXIT
cleanup

if [[ "${WRITE_TARBALL}" == "yes" ]]; then
	kz_info "Verify with: sha256sum -c ${TARBALL_OUT%/}/${TARBALL_NAME}.sha256"
fi
if [[ "${WRITE_DIR}" == "yes" ]]; then
	kz_info "Validate with: ./packaging/linux/validate.sh"
fi
