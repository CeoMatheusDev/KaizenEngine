#!/usr/bin/env bash
#
# Kaizen Engine — Linux uninstaller.
#
# Removes only Kaizen-installed files for the selected channel/version.
# Other versions, user projects, editor settings and caches are preserved.
#
# Usage:
#   ./Setup/uninstall.sh --list
#   ./Setup/uninstall.sh --channel beta --version 4.8.0-beta1
#   ./Setup/uninstall.sh --all [--channel beta] --yes
#   /opt/kaizen/beta/4.8.0-beta1/Setup/uninstall.sh
#
# Options:
#       --channel NAME     Channel to remove (required unless --all/--list)
#       --version VER      Version to remove (required unless --all/--list)
#       --all              Remove every installed version (optionally per channel)
#       --list             List installed versions and exit
#       --opt-root DIR     Installation root (default: inferred)
#       --bin-dir DIR      Launcher directory (default: from receipt)
#       --share-dir DIR    XDG data root (default: from receipt)
#       --data-home DIR    XDG data home for templates (default: from receipt)
#       --system           Operate on the system-wide installation
#       --prefix DIR       User-mode prefix (default: $HOME/.local)
#       --dry-run          Print what would be removed without deleting
#   -y, --yes              Do not ask for confirmation
#   -h, --help             Show this help

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

MODE="user"
PREFIX="${HOME}/.local"
OPT_ROOT=""
BIN_DIR=""
SHARE_DIR=""
DATA_HOME=""
CHANNEL=""
VERSION=""
ALL="no"
LIST="no"
DRY_RUN="no"
ASSUME_YES="no"
EXPLICIT_OPT="no"
INSTALLED_VERSION_DIR=""

die() {
	printf '[fail] %s\n' "$*" >&2
	exit 1
}
info() { printf '[kz] %s\n' "$*"; }
ok() { printf '[ ok ] %s\n' "$*"; }
warn() { printf '[warn] %s\n' "$*" >&2; }

usage() {
	sed -n '2,27p' "$0" | sed 's/^# \{0,1\}//'
	exit "${1:-0}"
}

while [[ $# -gt 0 ]]; do
	case "$1" in
	--channel)
		[[ $# -ge 2 ]] || die "--channel requires a value."
		CHANNEL="$2"
		shift 2
		;;
	--version)
		[[ $# -ge 2 ]] || die "--version requires a value."
		VERSION="$2"
		shift 2
		;;
	--all)
		ALL="yes"
		shift
		;;
	--list)
		LIST="yes"
		shift
		;;
	--opt-root)
		[[ $# -ge 2 ]] || die "--opt-root requires a value."
		OPT_ROOT="$2"
		EXPLICIT_OPT="yes"
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
	--system)
		MODE="system"
		shift
		;;
	--prefix)
		[[ $# -ge 2 ]] || die "--prefix requires a value."
		PREFIX="$2"
		EXPLICIT_OPT="yes"
		shift 2
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

receipt_get() { { grep -m1 "^$2=" "$1" 2>/dev/null || true; } | cut -d= -f2-; }
is_version_dir() {
	[[ -f "$1/.kaizen-receipt.env" ]] &&
		[[ "$(receipt_get "$1/.kaizen-receipt.env" PRODUCT)" == "Kaizen" ]]
}

# --- Infer paths from an installed copy ---------------------------------
if [[ -f "${SCRIPT_DIR}/../.kaizen-receipt.env" ]]; then
	INSTALLED_VERSION_DIR="$(cd "${SCRIPT_DIR}/.." && pwd)"
	INSTALLED_CHANNEL_DIR="$(dirname "${INSTALLED_VERSION_DIR}")"
	INSTALLED_OPT_ROOT="$(dirname "${INSTALLED_CHANNEL_DIR}")"
	INSTALLED_CHANNEL="$(basename "${INSTALLED_CHANNEL_DIR}")"
	INSTALLED_VERSION="$(basename "${INSTALLED_VERSION_DIR}")"
	RECEIPT="${INSTALLED_VERSION_DIR}/.kaizen-receipt.env"
	[[ -n "${OPT_ROOT}" ]] || {
		OPT_ROOT="${INSTALLED_OPT_ROOT}"
		EXPLICIT_OPT="no"
	}
	[[ -n "${CHANNEL}" ]] || CHANNEL="${INSTALLED_CHANNEL}"
	[[ -n "${VERSION}" ]] || VERSION="${INSTALLED_VERSION}"
	[[ -n "${BIN_DIR}" ]] || BIN_DIR="$(receipt_get "${RECEIPT}" BIN_DIR)"
	[[ -n "${SHARE_DIR}" ]] || SHARE_DIR="$(receipt_get "${RECEIPT}" SHARE_DIR)"
	[[ -n "${DATA_HOME}" ]] || DATA_HOME="$(receipt_get "${RECEIPT}" DATA_HOME)"
fi

if [[ -z "${OPT_ROOT}" ]]; then
	if [[ "${MODE}" == "system" ]]; then
		OPT_ROOT="/opt/kaizen"
	else
		OPT_ROOT="${PREFIX}/opt/kaizen"
	fi
fi
OPT_ROOT="$(realpath -m "${OPT_ROOT}")"
[[ -z "${BIN_DIR}" ]] || BIN_DIR="$(realpath -m "${BIN_DIR}")"
[[ -z "${SHARE_DIR}" ]] || SHARE_DIR="$(realpath -m "${SHARE_DIR}")"
[[ -z "${DATA_HOME}" ]] || DATA_HOME="$(realpath -m "${DATA_HOME}")"

[[ -d "${OPT_ROOT}" ]] || die "No Kaizen installation found at ${OPT_ROOT}."

if [[ "${MODE}" == "user" && "${EXPLICIT_OPT}" != "yes" && "${OPT_ROOT}" != "${HOME}"/* ]]; then
	die "Refusing to modify ${OPT_ROOT} outside ${HOME}. Pass --opt-root explicitly if that is intended."
fi
if [[ "${MODE}" == "system" && "$(id -u)" -ne 0 && ! -w "${OPT_ROOT}" ]]; then
	die "System uninstall requires root (try: sudo $0 --system)."
fi

# --- Discovery ----------------------------------------------------------
list_versions() { # <channel>
	local ch="$1" d
	for d in "${OPT_ROOT}/${ch}"/*; do
		[[ -d "${d}" && ! -L "${d}" ]] || continue
		[[ "$(basename "${d}")" == .* ]] && continue
		is_version_dir "${d}" || continue
		basename "${d}"
	done | sort -V
}
list_channels() {
	local d ch
	for d in "${OPT_ROOT}"/*; do
		[[ -d "${d}" && ! -L "${d}" ]] || continue
		ch="$(basename "${d}")"
		[[ "${ch}" == .* ]] && continue
		[[ -n "$(list_versions "${ch}")" ]] || continue
		printf '%s\n' "${ch}"
	done
}

all_version_dirs() {
	local ch v
	for ch in $(list_channels); do
		for v in $(list_versions "${ch}"); do
			printf '%s/%s/%s\n' "${OPT_ROOT}" "${ch}" "${v}"
		done
	done
}

do_list() {
	local default ch v marker
	default="$(cat "${OPT_ROOT}/default-channel" 2>/dev/null || true)"
	printf 'Installation   : %s\n' "${OPT_ROOT}"
	printf 'Default channel: %s\n\n' "${default:-(none)}"
	if [[ -z "$(list_channels)" ]]; then
		info "No versions installed."
		return 0
	fi
	for ch in $(list_channels); do
		marker=""
		[[ "${ch}" == "${default}" ]] && marker=" (default)"
		printf '%s%s\n' "${ch}" "${marker}"
		for v in $(list_versions "${ch}"); do
			marker=""
			[[ "$(readlink "${OPT_ROOT}/${ch}/current" 2>/dev/null || true)" == "${v}" ]] && marker=" *"
			printf '  %s%s\n' "${v}" "${marker}"
		done
	done
}

if [[ "${LIST}" == "yes" ]]; then
	do_list
	exit 0
fi

# --- Select targets -----------------------------------------------------
declare -a TARGETS=()
if [[ "${ALL}" == "yes" ]]; then
	while IFS= read -r d; do
		[[ -n "${d}" ]] || continue
		if [[ -n "${CHANNEL}" && "$(basename "$(dirname "${d}")")" != "${CHANNEL}" ]]; then
			continue
		fi
		TARGETS+=("${d}")
	done < <(all_version_dirs)
	[[ ${#TARGETS[@]} -gt 0 ]] || die "No installed versions match the request."
else
	[[ -n "${CHANNEL}" ]] || die "Specify --channel (or use --all/--list)."
	[[ -n "${VERSION}" ]] || die "Specify --version (or use --all)."
	case "${CHANNEL}" in
	stable | beta | rc | dev) ;;
	*) die "Invalid channel: ${CHANNEL}" ;;
	esac
	TARGET="${OPT_ROOT}/${CHANNEL}/${VERSION}"
	[[ -d "${TARGET}" ]] || die "Not installed: ${CHANNEL}/${VERSION}"
	is_version_dir "${TARGET}" || die "Refusing to remove ${TARGET}: not a Kaizen installation."
	TARGETS+=("${TARGET}")
fi

# --- Confirm ------------------------------------------------------------
printf 'The following Kaizen installations will be removed:\n'
for d in "${TARGETS[@]}"; do
	[[ -d "${d}" ]] && printf '  %s\n' "${d}"
done
printf 'User projects, editor settings and caches are preserved.\n'

if [[ "${DRY_RUN}" == "yes" ]]; then
	info "Dry run — nothing removed."
	exit 0
fi
if [[ "${ASSUME_YES}" != "yes" ]]; then
	printf 'Proceed? [y/N] '
	read -r answer
	[[ "${answer}" == "y" || "${answer}" == "Y" ]] || die "Aborted by user."
fi

# --- Capture metadata before removing ----------------------------------
declare -a REMOVED_CFG=()
declare -a REMOVED_SDK=()
declare -a REMOVED_CHANNELS=()
for d in "${TARGETS[@]}"; do
	r="${d}/.kaizen-receipt.env"
	cfg="$(receipt_get "${r}" TEMPLATES_CONFIG)"
	sdk="$(receipt_get "${r}" DOTNET_SDK)"
	ch="$(basename "$(dirname "${d}")")"
	[[ -n "${cfg}" ]] && REMOVED_CFG+=("${cfg}")
	[[ -n "${sdk}" ]] && REMOVED_SDK+=("${sdk}")
	REMOVED_CHANNELS+=("${ch}")
	[[ -n "${BIN_DIR}" ]] || BIN_DIR="$(receipt_get "${r}" BIN_DIR)"
	[[ -n "${SHARE_DIR}" ]] || SHARE_DIR="$(receipt_get "${r}" SHARE_DIR)"
	[[ -n "${DATA_HOME}" ]] || DATA_HOME="$(receipt_get "${r}" DATA_HOME)"
done

# --- Remove -------------------------------------------------------------
for d in "${TARGETS[@]}"; do
	rm -rf "${d}"
	ok "Removed: ${d}"
done

# --- Remaining state ----------------------------------------------------
remaining_receipts() {
	local d
	while IFS= read -r d; do
		[[ -n "${d}" ]] || continue
		printf '%s\n' "${d}/.kaizen-receipt.env"
	done < <(all_version_dirs)
}
remaining_has_cfg() {
	local cfg="$1" r
	while IFS= read -r r; do
		[[ "$(receipt_get "${r}" TEMPLATES_CONFIG)" == "${cfg}" ]] && return 0
	done < <(remaining_receipts)
	return 1
}
remaining_has_sdk() {
	local sdk="$1" r
	[[ -z "${sdk}" ]] && return 1
	while IFS= read -r r; do
		[[ "$(receipt_get "${r}" DOTNET_SDK)" == "${sdk}" ]] && return 0
	done < <(remaining_receipts)
	return 1
}

# Export templates (shared per engine version).
while IFS= read -r cfg; do
	[[ -n "${cfg}" ]] || continue
	if remaining_has_cfg "${cfg}"; then
		info "Keeping export templates still used by another version: ${cfg}"
		continue
	fi
	if [[ -n "${DATA_HOME}" && -d "${DATA_HOME}/kaizen/export_templates/${cfg}" ]]; then
		rm -rf "${DATA_HOME}/kaizen/export_templates/${cfg}"
		ok "Removed export templates: ${cfg}"
		rmdir "${DATA_HOME}/kaizen/export_templates" 2>/dev/null || true
		rmdir "${DATA_HOME}/kaizen" 2>/dev/null || true
	fi
done < <(printf '%s\n' ${REMOVED_CFG[@]+"${REMOVED_CFG[@]}"} | sort -u)

# Shared private .NET.
while IFS= read -r sdk; do
	[[ -n "${sdk}" ]] || continue
	if remaining_has_sdk "${sdk}"; then
		info "Keeping shared .NET still used by another version: ${sdk}"
		continue
	fi
	if [[ -d "${OPT_ROOT}/.dotnet/${sdk}" ]]; then
		rm -rf "${OPT_ROOT}/.dotnet/${sdk}"
		ok "Removed shared .NET: ${sdk}"
	fi
done < <(printf '%s\n' ${REMOVED_SDK[@]+"${REMOVED_SDK[@]}"} | sort -u)
rmdir "${OPT_ROOT}/.dotnet" 2>/dev/null || true

# Per-channel pointers and launchers.
while IFS= read -r ch; do
	if [[ -n "$(list_versions "${ch}")" ]]; then
		newest="$(list_versions "${ch}" | tail -n1)"
		cur="$(readlink "${OPT_ROOT}/${ch}/current" 2>/dev/null || true)"
		if [[ -z "${cur}" || ! -d "${OPT_ROOT}/${ch}/${cur}" ]]; then
			ln -sfn "${newest}" "${OPT_ROOT}/${ch}/current"
			info "Channel ${ch}/current -> ${newest}"
		fi
	else
		rm -f "${OPT_ROOT}/${ch}/current"
		rmdir "${OPT_ROOT}/${ch}" 2>/dev/null || true
		[[ -n "${BIN_DIR}" ]] && rm -f "${BIN_DIR}/kaizen-${ch}"
		ok "Channel removed: ${ch}"
	fi
done < <(printf '%s\n' ${REMOVED_CHANNELS[@]+"${REMOVED_CHANNELS[@]}"} | sort -u)

# Default selection.
DEFAULT_FILE="${OPT_ROOT}/default-channel"
current_default="$(cat "${DEFAULT_FILE}" 2>/dev/null || true)"
if [[ -z "${current_default}" || ! -d "${OPT_ROOT}/${current_default}/current" ]]; then
	chosen=""
	for candidate in stable rc beta dev; do
		if [[ -d "${OPT_ROOT}/${candidate}" && -n "$(list_versions "${candidate}")" ]]; then
			chosen="${candidate}"
			break
		fi
	done
	if [[ -n "${chosen}" ]]; then
		printf '%s\n' "${chosen}" >"${DEFAULT_FILE}.tmp.$$"
		mv -f "${DEFAULT_FILE}.tmp.$$" "${DEFAULT_FILE}"
		ok "Default channel: ${chosen}"
	else
		rm -f "${DEFAULT_FILE}"
	fi
fi

# --- Global integration (only when nothing remains) --------------------
if [[ -z "$(list_channels)" ]]; then
	[[ -n "${BIN_DIR}" ]] && rm -f "${BIN_DIR}/kaizen" "${BIN_DIR}/kaizen-select"
	rm -f "${OPT_ROOT}/select.sh" "${OPT_ROOT}/check-deps.sh"
	if [[ -n "${SHARE_DIR}" ]]; then
		rm -f "${SHARE_DIR}/applications/org.kaizenengine.Kaizen.desktop"
		rm -f "${SHARE_DIR}/metainfo/org.kaizenengine.Kaizen.appdata.xml"
		rm -f "${SHARE_DIR}/mime/packages/org.kaizenengine.Kaizen.xml"
		rm -f "${SHARE_DIR}/icons/hicolor/scalable/apps/kaizen.svg"
		rm -f "${SHARE_DIR}/icons/hicolor/256x256/apps/kaizen.png"
		rm -f "${SHARE_DIR}/man/man6/kaizen.6"
		command -v update-desktop-database >/dev/null 2>&1 &&
			update-desktop-database "${SHARE_DIR}/applications" >/dev/null 2>&1 || true
		command -v update-mime-database >/dev/null 2>&1 &&
			update-mime-database "${SHARE_DIR}/mime" >/dev/null 2>&1 || true
		command -v gtk-update-icon-cache >/dev/null 2>&1 &&
			gtk-update-icon-cache -q -t -f "${SHARE_DIR}/icons/hicolor" >/dev/null 2>&1 || true
	fi
	rmdir "${OPT_ROOT}" 2>/dev/null || true
	ok "Kaizen launchers and desktop integration removed."
fi

ok "Uninstallation finished. User projects, settings and caches were preserved."
