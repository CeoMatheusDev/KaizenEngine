#!/usr/bin/env bash
#
# Kaizen Engine — Linux installer.
#
# Installs a packaged distribution per-user (default) or system-wide, with
# version/channel coexistence, checksum verification and transactional swaps.
#
# Usage:
#   ./Setup/install.sh [options]
#
# Options:
#       --user                 Per-user installation (default)
#       --system               System-wide installation (/opt, /usr/local, /usr/share)
#       --prefix DIR           User-mode prefix (default: $HOME/.local)
#       --opt-root DIR         Engine root (default: <prefix>/opt/kaizen or /opt/kaizen)
#       --bin-dir DIR          Launcher directory (default: <prefix>/bin or /usr/local/bin)
#       --share-dir DIR        XDG data root (default: $XDG_DATA_HOME or /usr/share)
#       --data-home DIR        XDG data home for templates (default: --share-dir value)
#       --from-tarball FILE    Verify and install from the official .tar.xz
#       --channel NAME         Require this channel (must match package metadata)
#       --version VER          Require this version (must match package metadata)
#       --select[=CHANNEL]     Set the default channel after installing
#       --no-select            Do not change the current default selection
#       --templates-only       Install export templates only
#       --force                Replace an already installed version
#       --dry-run              Validate and print the plan without writing
#   -y, --yes                  Assume yes for confirmation prompts
#   -h, --help                 Show this help
#
# Layout (per-user):
#   ~/.local/opt/kaizen/<channel>/<version>/   engine tree
#   ~/.local/opt/kaizen/<channel>/current -> <version>
#   ~/.local/opt/kaizen/default-channel        selected default channel
#   ~/.local/bin/kaizen, kaizen-<channel>      launchers
#   ~/.local/share/kaizen/export_templates/    export templates
#   ~/.local/share/{applications,icons,mime,metainfo,man}/
#
# The engine keeps user projects/settings in $XDG_DATA_HOME/kaizen and
# $XDG_CONFIG_HOME/kaizen; this installer never touches those directories.

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
DIST_ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"

MODE="user"
PREFIX="${HOME}/.local"
OPT_ROOT=""
BIN_DIR=""
SHARE_DIR=""
DATA_HOME=""
FROM_TARBALL=""
REQ_CHANNEL=""
REQ_VERSION=""
SELECT_CHANNEL=""
SELECT_MODE="auto" # auto (first install only) | yes | no
TEMPLATES_ONLY="no"
FORCE="no"
DRY_RUN="no"
ASSUME_YES="no"

die() {
	printf '[fail] %s\n' "$*" >&2
	exit 1
}
info() { printf '[kz] %s\n' "$*"; }
ok() { printf '[ ok ] %s\n' "$*"; }
warn() { printf '[warn] %s\n' "$*" >&2; }

usage() {
	sed -n '2,41p' "$0" | sed 's/^# \{0,1\}//'
	exit "${1:-0}"
}

while [[ $# -gt 0 ]]; do
	case "$1" in
	--user)
		MODE="user"
		shift
		;;
	--system)
		MODE="system"
		shift
		;;
	--prefix)
		[[ $# -ge 2 ]] || die "--prefix requires a value."
		PREFIX="$2"
		shift 2
		;;
	--opt-root)
		[[ $# -ge 2 ]] || die "--opt-root requires a value."
		OPT_ROOT="$2"
		shift 2
		;;
	--bin-dir)
		[[ $# -ge 2 ]] || die "--bin-dir requires a value."
		BIN_DIR="$2"
		shift 2
		;;
	--share-dir)
		[[ $# -ge 2 ]] || die "--share-dir requires a value."
		SHARE_DIR="$2"
		shift 2
		;;
	--data-home)
		[[ $# -ge 2 ]] || die "--data-home requires a value."
		DATA_HOME="$2"
		shift 2
		;;
	--from-tarball)
		[[ $# -ge 2 ]] || die "--from-tarball requires a value."
		FROM_TARBALL="$2"
		shift 2
		;;
	--channel)
		[[ $# -ge 2 ]] || die "--channel requires a value."
		REQ_CHANNEL="$2"
		shift 2
		;;
	--version)
		[[ $# -ge 2 ]] || die "--version requires a value."
		REQ_VERSION="$2"
		shift 2
		;;
	--select)
		SELECT_MODE="yes"
		SELECT_CHANNEL=""
		shift
		;;
	--select=*)
		SELECT_MODE="yes"
		SELECT_CHANNEL="${1#*=}"
		shift
		;;
	--no-select)
		SELECT_MODE="no"
		shift
		;;
	--templates-only)
		TEMPLATES_ONLY="yes"
		shift
		;;
	--force)
		FORCE="yes"
		shift
		;;
	--dry-run)
		DRY_RUN="yes"
		shift
		;;
	-y | --yes)
		ASSUME_YES="yes"
		shift
		;;
	-h | --help)
		usage 0
		;;
	*)
		die "Unknown option: $1 (use --help)"
		;;
	esac
done

# --- Paths --------------------------------------------------------------
case "${MODE}" in
user)
	[[ -n "${OPT_ROOT}" ]] || OPT_ROOT="${PREFIX}/opt/kaizen"
	[[ -n "${BIN_DIR}" ]] || BIN_DIR="${PREFIX}/bin"
	if [[ -n "${SHARE_DIR}" ]]; then
		[[ -n "${DATA_HOME}" ]] || DATA_HOME="${SHARE_DIR}"
	else
		SHARE_DIR="${XDG_DATA_HOME:-${HOME}/.local/share}"
		[[ -n "${DATA_HOME}" ]] || DATA_HOME="${SHARE_DIR}"
	fi
	;;
system)
	[[ -n "${OPT_ROOT}" ]] || OPT_ROOT="/opt/kaizen"
	[[ -n "${BIN_DIR}" ]] || BIN_DIR="/usr/local/bin"
	[[ -n "${SHARE_DIR}" ]] || SHARE_DIR="/usr/share"
	if [[ -z "${DATA_HOME}" ]]; then
		# Export templates live in the user's XDG data dir; the engine has no
		# system-wide template path. Prefer the invoking user (sudo).
		TARGET_USER="${SUDO_USER:-${USER:-$(id -un)}}"
		if [[ "${TARGET_USER}" == "root" ]]; then
			DATA_HOME="${HOME}/.local/share"
			warn "running as root: export templates will be installed for root."
			warn "other users must extract the official archive and run: ./Setup/install.sh --templates-only"
		else
			TARGET_HOME="$(getent passwd "${TARGET_USER}" 2>/dev/null | cut -d: -f6 || true)"
			[[ -n "${TARGET_HOME}" ]] || TARGET_HOME="${HOME}"
			DATA_HOME="${TARGET_HOME}/.local/share"
		fi
	fi
	;;
esac

OPT_ROOT="$(realpath -m "${OPT_ROOT}")"
BIN_DIR="$(realpath -m "${BIN_DIR}")"
SHARE_DIR="$(realpath -m "${SHARE_DIR}")"
DATA_HOME="$(realpath -m "${DATA_HOME}")"

# --- Scratch dirs and rollback ------------------------------------------
EXTRACT_DIR=""
STAGE_DIR=""
FINAL_DIR=""
CHANNEL_DIR=""
BACKUP_DIR=""
NEW_INSTALLED="no"
TMP_HOME=""
TMP_TAR=""
CURRENT_LINK=""

cleanup() {
	if [[ -n "${TMP_TAR}" && -f "${TMP_TAR}" ]]; then rm -f "${TMP_TAR}"; fi
	if [[ -n "${TMP_HOME}" && -d "${TMP_HOME}" ]]; then rm -rf "${TMP_HOME}"; fi
	if [[ -n "${STAGE_DIR}" && -d "${STAGE_DIR}" ]]; then rm -rf "${STAGE_DIR}"; fi
	if [[ -n "${EXTRACT_DIR}" && -d "${EXTRACT_DIR}" ]]; then rm -rf "${EXTRACT_DIR}"; fi
	return 0
}
on_error() {
	local rc=$?
	trap - EXIT
	if [[ $rc -ne 0 ]]; then
		if [[ "${NEW_INSTALLED}" == "yes" && -n "${FINAL_DIR}" ]]; then
			rm -rf "${FINAL_DIR}"
			NEW_INSTALLED="no"
		fi
		if [[ -n "${BACKUP_DIR}" && -d "${BACKUP_DIR}" && -n "${FINAL_DIR}" ]]; then
			mv "${BACKUP_DIR}" "${FINAL_DIR}"
			BACKUP_DIR=""
			warn "previous installation restored: ${FINAL_DIR}"
		fi
		# Do not leave a channel pointer to a version that never landed.
		if [[ "${NEW_INSTALLED}" == "no" && -n "${CURRENT_LINK}" && -L "${CURRENT_LINK}" ]]; then
			if [[ "$(readlink "${CURRENT_LINK}")" == "${PACKAGE_VERSION}" ]]; then
				rm -f "${CURRENT_LINK}"
			fi
		fi
		if [[ -n "${CHANNEL_DIR}" && -d "${CHANNEL_DIR}" ]]; then
			rmdir "${CHANNEL_DIR}" 2>/dev/null || true
		fi
	fi
	cleanup
	exit $rc
}
trap on_error EXIT

# --- Helpers ------------------------------------------------------------
need_cmd() {
	local c
	for c in "$@"; do
		command -v "${c}" >/dev/null 2>&1 || die "Required command not found: ${c}"
	done
}
need_cmd xz tar sha256sum realpath find sort

pkg_get() { # <file> <key>
	local file="$1" key="$2"
	{ grep -m1 "^${key}=" "${file}" 2>/dev/null || true; } | cut -d= -f2-
}

require_meta() {
	local key="$1" value="$2"
	[[ -n "${value}" ]] || die "Invalid package metadata: missing ${key} in ${PKG_INFO}"
}

verify_manifest() { # <root>
	local root="$1" mf="$1/MANIFEST.sha256" line path
	[[ -f "${mf}" ]] || die "Missing MANIFEST.sha256 in ${root}"
	while IFS= read -r line; do
		[[ -z "${line}" ]] && continue
		if [[ ! "${line}" =~ ^[0-9a-f]{64}\ \ .+$ ]]; then
			die "Malformed MANIFEST.sha256 line: ${line:0:80}"
		fi
		path="${line#*  }"
		[[ "${path}" == /* ]] && die "Manifest contains an absolute path: ${path}"
		case "/${path}/" in
		*/../* | */./*) die "Manifest contains an unsafe path: ${path}" ;;
		esac
	done <"${mf}"

	( cd "${root}" && sha256sum -c --quiet --strict MANIFEST.sha256 >/dev/null 2>&1 ) ||
		die "SHA-256 verification failed for ${root}"

	local listed actual
	listed="$(grep -c . "${mf}")"
	actual="$(find "${root}" -type f ! -name MANIFEST.sha256 | wc -l)"
	[[ "${listed}" == "${actual}" ]] ||
		die "Manifest coverage mismatch: ${listed} listed, ${actual} present"
}

verify_symlinks() { # <root>
	local root="$1" link target resolved
	while IFS= read -r -d '' link; do
		target="$(readlink "${link}")"
		if [[ "${target}" == /* ]]; then
			die "Absolute symlink rejected: ${link#${root}/} -> ${target}"
		fi
		resolved="$(realpath -m "$(dirname "${link}")/${target}")"
		case "${resolved}/" in
		"${root}/"*) ;;
		*) die "Symlink escapes the distribution: ${link#${root}/} -> ${target}" ;;
		esac
	done < <(find "${root}" -type l -print0)
}

available_kb() { # <path>
	local p="$1"
	while [[ ! -d "${p}" ]]; do p="$(dirname "${p}")"; done
	df -Pk "${p}" | awk 'NR==2 {print $4}'
}

tar_listing() { # <tar-stream>
	tar -tf "$1"
}

tar_types() { # <tar-stream>
	tar -tvf "$1" | cut -c1
}

extract_tarball() { # <tarball>
	local file="$1" sums="$1.sha256" name count types

	[[ -f "${file}" ]] || die "Archive not found: ${file}"
	[[ -f "${sums}" ]] || die "Missing external checksum: ${sums}"

	info "Verifying archive checksum ..."
	( cd "$(dirname "${file}")" && sha256sum -c --quiet "$(basename "${sums}")" >/dev/null 2>&1 ) ||
		die "Archive checksum mismatch: ${file}"

	# Decompress once; the inspection and the extraction both read this stream.
	TMP_TAR="$(mktemp "${TMPDIR:-/tmp}/kaizen-archive.XXXXXX.tar")"
	xz -dc "${file}" >"${TMP_TAR}" || die "Could not decompress ${file}"
	[[ -s "${TMP_TAR}" ]] || die "Archive is empty: ${file}"

	info "Inspecting archive entries ..."
	while IFS= read -r name; do
		[[ -z "${name}" ]] && continue
		case "${name}" in
		Kaizen | Kaizen/*) ;;
		*) die "Archive entry outside Kaizen/: ${name}" ;;
		esac
		case "/${name}/" in
		*/../* | */./*) die "Unsafe archive entry: ${name}" ;;
		esac
	done < <(tar_listing "${TMP_TAR}")

	count="$(tar_listing "${TMP_TAR}" | wc -l)"
	types="$(tar_types "${TMP_TAR}" | sort -u | tr -d '\n')"
	if [[ "${types}" == *[bcpsh]* ]]; then
		die "Archive contains unsupported entry types (device/fifo/hardlink): ${types}"
	fi
	[[ "${count}" -gt 0 ]] || die "Archive is empty"

	EXTRACT_DIR="$(mktemp -d "${TMPDIR:-/tmp}/kaizen-install.XXXXXX")"
	info "Extracting archive ..."
	tar -xpf "${TMP_TAR}" -C "${EXTRACT_DIR}" --no-same-owner
	rm -f "${TMP_TAR}"
	TMP_TAR=""
	DIST_ROOT="${EXTRACT_DIR}/Kaizen"
	[[ -d "${DIST_ROOT}" ]] || die "Archive does not contain the Kaizen/ root directory."
}

# --- Load metadata ------------------------------------------------------
if [[ -n "${FROM_TARBALL}" ]]; then
	extract_tarball "${FROM_TARBALL}"
fi

PKG_INFO="${DIST_ROOT}/Build/Config/package-info.env"
[[ -f "${PKG_INFO}" ]] || die "Not a Kaizen package: missing ${PKG_INFO}"

PRODUCT="$(pkg_get "${PKG_INFO}" PRODUCT)"
PACKAGE_VERSION="$(pkg_get "${PKG_INFO}" PACKAGE_VERSION)"
VERSION_BASE="$(pkg_get "${PKG_INFO}" PACKAGE_VERSION_BASE)"
CHANNEL="$(pkg_get "${PKG_INFO}" CHANNEL)"
ENGINE_VERSION="$(pkg_get "${PKG_INFO}" ENGINE_VERSION)"
TEMPLATES_CONFIG="$(pkg_get "${PKG_INFO}" TEMPLATES_CONFIG)"
ARCH_META="$(pkg_get "${PKG_INFO}" ARCH)"
PLATFORM_META="$(pkg_get "${PKG_INFO}" PLATFORM)"
ROOT_META="$(pkg_get "${PKG_INFO}" ROOT)"

require_meta PRODUCT "${PRODUCT}"
require_meta PACKAGE_VERSION "${PACKAGE_VERSION}"
require_meta CHANNEL "${CHANNEL}"
require_meta ENGINE_VERSION "${ENGINE_VERSION}"
require_meta TEMPLATES_CONFIG "${TEMPLATES_CONFIG}"
require_meta ARCH_META "${ARCH_META}"
require_meta PLATFORM_META "${PLATFORM_META}"

[[ "${PRODUCT}" == "Kaizen" ]] || die "Unexpected product: ${PRODUCT}"
[[ "${ROOT_META}" == "Kaizen" ]] || die "Unexpected archive root: ${ROOT_META}"
[[ "${PLATFORM_META}" == "linux" ]] || die "Unsupported package platform: ${PLATFORM_META}"
[[ "${ARCH_META}" == "x86_64" ]] || die "Unsupported architecture: ${ARCH_META}. Only x86_64 is supported."
case "${CHANNEL}" in
stable | beta | rc | dev) ;;
*) die "Invalid channel in package metadata: ${CHANNEL}" ;;
esac
[[ "${PACKAGE_VERSION}" =~ ^[0-9]+\.[0-9]+\.[0-9]+(-(beta|rc|dev)[1-9][0-9]*)?$ ]] ||
	die "Invalid package version: ${PACKAGE_VERSION}"

[[ -z "${REQ_CHANNEL}" || "${REQ_CHANNEL}" == "${CHANNEL}" ]] ||
	die "Package channel is '${CHANNEL}', but --channel '${REQ_CHANNEL}' was requested."
[[ -z "${REQ_VERSION}" || "${REQ_VERSION}" == "${PACKAGE_VERSION}" ]] ||
	die "Package version is '${PACKAGE_VERSION}', but --version '${REQ_VERSION}' was requested."

# --- Integrity ----------------------------------------------------------
info "Verifying package manifest (${DIST_ROOT}) ..."
verify_manifest "${DIST_ROOT}"
verify_symlinks "${DIST_ROOT}"
ok "Manifest and symlinks verified"

BIN_SRC="${DIST_ROOT}/Engine/Binaries/Linux"
[[ -x "${BIN_SRC}/kaizen" ]] || die "Missing launcher: ${BIN_SRC}/kaizen"

# --- Selection of paths -------------------------------------------------
CHANNEL_DIR="${OPT_ROOT}/${CHANNEL}"
FINAL_DIR="${CHANNEL_DIR}/${PACKAGE_VERSION}"
STAGE_DIR="${CHANNEL_DIR}/.${PACKAGE_VERSION}.staging.$$"
TEMPLATES_DEST="${DATA_HOME}/kaizen/export_templates/${TEMPLATES_CONFIG}"

info "Kaizen ${PACKAGE_VERSION} (${CHANNEL})"
info "  mode       : ${MODE}"
info "  engine     : ${FINAL_DIR}"
info "  launchers  : ${BIN_DIR}"
info "  data       : ${DATA_HOME}"
info "  templates  : ${TEMPLATES_DEST}"

writable_target() { # <path>
	local p="$1"
	while [[ ! -e "${p}" ]]; do p="$(dirname "${p}")"; done
	[[ -w "${p}" ]]
}

# System-wide mode normally requires root. When the target paths are writable
# (a simulated root, a writable /opt, or a --prefix override), proceed.
if [[ "${DRY_RUN}" != "yes" ]]; then
	for target in "$(dirname "${OPT_ROOT}")" "${BIN_DIR}" "${SHARE_DIR}" "${DATA_HOME}"; do
		if ! writable_target "${target}"; then
			if [[ "${MODE}" == "system" ]]; then
				die "System install requires write access to ${target} (try: sudo $0 --system)."
			fi
			die "No write permission for ${target}. Choose another --prefix/--opt-root."
		fi
	done
fi

# --- Space check --------------------------------------------------------
if [[ "${TEMPLATES_ONLY}" != "yes" ]]; then
	NEED_KB="$(du -sk "${DIST_ROOT}" | cut -f1)"
	AVAIL_KB="$(available_kb "$(dirname "${OPT_ROOT}")")"
	if [[ -n "${AVAIL_KB}" && "${AVAIL_KB}" =~ ^[0-9]+$ && "${AVAIL_KB}" -lt $((NEED_KB + 51200)) ]]; then
		die "Not enough free space: need ~$((NEED_KB / 1024)) MB, available $((AVAIL_KB / 1024)) MB."
	fi
fi

if [[ "${DRY_RUN}" == "yes" ]]; then
	info "Dry run — validation passed, nothing written."
	exit 0
fi

# --- Templates only -----------------------------------------------------
if [[ "${TEMPLATES_ONLY}" == "yes" ]]; then
	TEMPLATES_SRC="${DIST_ROOT}/Templates/${TEMPLATES_CONFIG}"
	[[ -d "${TEMPLATES_SRC}" ]] || die "Templates missing: ${TEMPLATES_SRC}"
	install -d "${TEMPLATES_DEST}"
	install -m 0755 "${TEMPLATES_SRC}/linux_release.x86_64" "${TEMPLATES_DEST}/linux_release.x86_64"
	install -m 0755 "${TEMPLATES_SRC}/linux_debug.x86_64" "${TEMPLATES_DEST}/linux_debug.x86_64"
	ok "Export templates installed: ${TEMPLATES_DEST}"
	exit 0
fi

# --- Existing version ---------------------------------------------------
if [[ -d "${FINAL_DIR}" && "${FORCE}" != "yes" ]]; then
	die "Version already installed: ${FINAL_DIR}. Use --force to replace it, or uninstall it first."
fi

# --- Stage the tree -----------------------------------------------------
install -d "${CHANNEL_DIR}"
rm -rf "${STAGE_DIR}"
mkdir -p "${STAGE_DIR}"
info "Copying engine tree ..."
# ExportTemplates/Templates are installed into XDG data below, not per version.
tar -C "${DIST_ROOT}" \
	--exclude='./ExportTemplates' --exclude='./Templates' \
	-cf - . | tar -C "${STAGE_DIR}" -xpf -

# Installed trees use XDG data/config paths (no portable marker) and share
# export templates from the user data directory.
rm -f "${STAGE_DIR}/Engine/Binaries/Linux/_sc_"
rm -rf "${STAGE_DIR}/Engine/Binaries/Linux/editor_data"

TMP_HOME="$(mktemp -d "${TMPDIR:-/tmp}/kaizen-home.XXXXXX")"
STAGED_VERSION="$(HOME="${TMP_HOME}" PATH="/usr/bin:/bin" \
	"${STAGE_DIR}/Engine/Binaries/Linux/kaizen" --version 2>/dev/null | head -n1 | tr -d '\r' || true)"
[[ "${STAGED_VERSION}" == "${ENGINE_VERSION}" ]] ||
	die "Staged launcher reported '${STAGED_VERSION}', expected '${ENGINE_VERSION}'."
ok "Staged engine runs (${STAGED_VERSION})"

# --- Receipt ------------------------------------------------------------
cat >"${STAGE_DIR}/.kaizen-receipt.env" <<EOF
PRODUCT=Kaizen
CHANNEL=${CHANNEL}
VERSION=${PACKAGE_VERSION}
ENGINE_VERSION=${ENGINE_VERSION}
TEMPLATES_CONFIG=${TEMPLATES_CONFIG}
MODE=${MODE}
OPT_ROOT=${OPT_ROOT}
BIN_DIR=${BIN_DIR}
SHARE_DIR=${SHARE_DIR}
DATA_HOME=${DATA_HOME}
INSTALLED_AT=$(date -u +%Y-%m-%dT%H:%M:%SZ)
EOF

# --- Swap the version into place ---------------------------------------
if [[ -d "${FINAL_DIR}" ]]; then
	BACKUP_DIR="${FINAL_DIR}.old.$$"
	rm -rf "${BACKUP_DIR}"
	mv "${FINAL_DIR}" "${BACKUP_DIR}"
fi
mv "${STAGE_DIR}" "${FINAL_DIR}"
STAGE_DIR=""
NEW_INSTALLED="yes"
ok "Engine installed: ${FINAL_DIR}"

# --- Export templates ---------------------------------------------------
TEMPLATES_SRC="${DIST_ROOT}/Templates/${TEMPLATES_CONFIG}"
[[ -d "${TEMPLATES_SRC}" ]] || die "Templates missing: ${TEMPLATES_SRC}"
install -d "${TEMPLATES_DEST}"
install -m 0755 "${TEMPLATES_SRC}/linux_release.x86_64" "${TEMPLATES_DEST}/linux_release.x86_64"
install -m 0755 "${TEMPLATES_SRC}/linux_debug.x86_64" "${TEMPLATES_DEST}/linux_debug.x86_64"
ok "Export templates: ${TEMPLATES_DEST}"

# --- Desktop integration ------------------------------------------------
CONFIG_SRC="${DIST_ROOT}/Config/linux"
install -d "${SHARE_DIR}/applications" "${SHARE_DIR}/icons/hicolor/scalable/apps" \
	"${SHARE_DIR}/icons/hicolor/256x256/apps" "${SHARE_DIR}/metainfo" \
	"${SHARE_DIR}/mime/packages" "${SHARE_DIR}/man/man6"

if [[ -f "${CONFIG_SRC}/org.kaizenengine.Kaizen.desktop" ]]; then
	# Absolute Exec avoids depending on the user's PATH in desktop launchers.
	sed "s|^Exec=.*|Exec=${BIN_DIR}/kaizen %f|" \
		"${CONFIG_SRC}/org.kaizenengine.Kaizen.desktop" >"${SHARE_DIR}/applications/org.kaizenengine.Kaizen.desktop"
	chmod 0644 "${SHARE_DIR}/applications/org.kaizenengine.Kaizen.desktop"
fi
[[ -f "${CONFIG_SRC}/org.kaizenengine.Kaizen.appdata.xml" ]] &&
	install -m 0644 "${CONFIG_SRC}/org.kaizenengine.Kaizen.appdata.xml" "${SHARE_DIR}/metainfo/"
[[ -f "${CONFIG_SRC}/org.kaizenengine.Kaizen.xml" ]] &&
	install -m 0644 "${CONFIG_SRC}/org.kaizenengine.Kaizen.xml" "${SHARE_DIR}/mime/packages/"
[[ -f "${CONFIG_SRC}/icons/hicolor/scalable/apps/kaizen.svg" ]] &&
	install -m 0644 "${CONFIG_SRC}/icons/hicolor/scalable/apps/kaizen.svg" "${SHARE_DIR}/icons/hicolor/scalable/apps/"
[[ -f "${CONFIG_SRC}/icons/hicolor/256x256/apps/kaizen.png" ]] &&
	install -m 0644 "${CONFIG_SRC}/icons/hicolor/256x256/apps/kaizen.png" "${SHARE_DIR}/icons/hicolor/256x256/apps/"
[[ -f "${CONFIG_SRC}/kaizen.6" ]] &&
	install -m 0644 "${CONFIG_SRC}/kaizen.6" "${SHARE_DIR}/man/man6/"
ok "Desktop integration: ${SHARE_DIR}"

# --- Launchers and selection -------------------------------------------
install -d "${BIN_DIR}"

install -d "${OPT_ROOT}"
if [[ -f "${DIST_ROOT}/Setup/select.sh" ]]; then
	install -m 0755 "${DIST_ROOT}/Setup/select.sh" "${OPT_ROOT}/select.sh"
fi
if [[ -f "${DIST_ROOT}/Setup/check-deps.sh" ]]; then
	install -m 0755 "${DIST_ROOT}/Setup/check-deps.sh" "${OPT_ROOT}/check-deps.sh"
fi

# Per-channel "current" pointer and launcher.
CURRENT_LINK="${CHANNEL_DIR}/current"
ln -sfn "${PACKAGE_VERSION}" "${CURRENT_LINK}"
ln -sfn "${OPT_ROOT}/${CHANNEL}/current/Engine/Binaries/Linux/kaizen" "${BIN_DIR}/kaizen-${CHANNEL}"

# Default launcher: reads <opt-root>/default-channel, with a deterministic
# fallback order if the selected channel is not installed.
cat >"${BIN_DIR}/kaizen" <<EOF
#!/bin/sh
# Kaizen Engine default launcher (generated by install.sh).
opt="${OPT_ROOT}"
channel=""
if [ -r "\$opt/default-channel" ]; then
	channel=\$(cat "\$opt/default-channel" 2>/dev/null || true)
fi
if [ -z "\$channel" ] || [ ! -e "\$opt/\$channel/current/Engine/Binaries/Linux/kaizen" ]; then
	for candidate in stable rc beta dev; do
		if [ -e "\$opt/\$candidate/current/Engine/Binaries/Linux/kaizen" ]; then
			channel="\$candidate"
			break
		fi
	done
fi
if [ -z "\$channel" ]; then
	echo "kaizen: no installed Kaizen version found under \$opt" >&2
	exit 127
fi
exec "\$opt/\$channel/current/Engine/Binaries/Linux/kaizen" "\$@"
EOF
chmod 0755 "${BIN_DIR}/kaizen"

cat >"${BIN_DIR}/kaizen-select" <<EOF
#!/bin/sh
KZ_OPT_ROOT="${OPT_ROOT}"
export KZ_OPT_ROOT
exec "${OPT_ROOT}/select.sh" "\$@"
EOF
chmod 0755 "${BIN_DIR}/kaizen-select"

DEFAULT_FILE="${OPT_ROOT}/default-channel"
if [[ "${SELECT_MODE}" == "yes" || ! -f "${DEFAULT_FILE}" ]]; then
	CHOSEN="${SELECT_CHANNEL:-${CHANNEL}}"
	case "${CHOSEN}" in
	stable | beta | rc | dev) ;;
	*) die "Invalid --select channel: ${CHOSEN}" ;;
	esac
	if [[ "${CHOSEN}" != "${CHANNEL}" && ! -d "${OPT_ROOT}/${CHOSEN}/current" ]]; then
		die "Cannot select '${CHOSEN}': it is not installed under ${OPT_ROOT}."
	fi
	# Never leave the selection pointing at a channel whose files are missing.
	if [[ -d "${OPT_ROOT}/${CHOSEN}/current" || "${CHOSEN}" == "${CHANNEL}" ]]; then
		printf '%s\n' "${CHOSEN}" >"${DEFAULT_FILE}.tmp.$$"
		mv -f "${DEFAULT_FILE}.tmp.$$" "${DEFAULT_FILE}"
		ok "Default channel: ${CHOSEN}"
	fi
fi

# --- Caches -------------------------------------------------------------
if command -v update-desktop-database >/dev/null 2>&1; then
	update-desktop-database "${SHARE_DIR}/applications" >/dev/null 2>&1 || true
fi
if command -v update-mime-database >/dev/null 2>&1; then
	update-mime-database "${SHARE_DIR}/mime" >/dev/null 2>&1 || true
fi
if command -v gtk-update-icon-cache >/dev/null 2>&1; then
	gtk-update-icon-cache -q -t -f "${SHARE_DIR}/icons/hicolor" >/dev/null 2>&1 || true
fi

# When installing system-wide as root, hand the per-user template directory
# back to the invoking user.
if [[ "${MODE}" == "system" && "$(id -u)" -eq 0 && -n "${SUDO_USER:-}" && "${SUDO_USER}" != "root" ]]; then
	chown -R "${SUDO_USER}:" "${DATA_HOME}/kaizen" 2>/dev/null ||
		warn "could not change ownership of ${DATA_HOME}/kaizen to ${SUDO_USER}"
fi

if [[ -n "${BACKUP_DIR}" && -d "${BACKUP_DIR}" ]]; then
	rm -rf "${BACKUP_DIR}"
fi
BACKUP_DIR=""
[[ -n "${STAGE_DIR}" && -d "${STAGE_DIR}" ]] && rm -rf "${STAGE_DIR}"
STAGE_DIR=""
trap - EXIT
cleanup

ok "Kaizen ${PACKAGE_VERSION} installed."
info "Run: ${BIN_DIR}/kaizen"
if [[ ":${PATH}:" != *":${BIN_DIR}:"* ]]; then
	warn "${BIN_DIR} is not in PATH. Add it without editing shell rc files automatically:"
	warn "  export PATH=\"${BIN_DIR}:\$PATH\""
fi
info "Select a default channel: kaizen-select --list"
info "Uninstall: ${FINAL_DIR}/Setup/uninstall.sh --channel ${CHANNEL} --version ${PACKAGE_VERSION}"
