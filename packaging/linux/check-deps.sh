#!/usr/bin/env bash
#
# Kaizen Engine — dependency report.
#
# Reports the external requirements of a Kaizen distribution: the C++ compiler used
# by C++ scripts, dynamically loaded graphics/audio libraries and the display server.
# It never installs anything.
#
# Usage:
#   ./Setup/check-deps.sh [--app-dir DIR] [--quiet] [--strict]
#
# Options:
#   --app-dir DIR   Distribution root (default: parent of this script)
#   --quiet         Only print problems
#   --strict        Exit non-zero when a required dependency is missing
#   -h, --help      Show this help

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
if [[ -n "${KZ_APP_DIR:-}" ]]; then
	APP_DIR="${KZ_APP_DIR}"
else
	APP_DIR="$(cd "${SCRIPT_DIR}/.." && pwd)"
	# When installed at <opt-root>/check-deps.sh, resolve the selected version
	# from <opt-root>/<channel>/current instead of the parent directory.
	if [[ ! -d "${APP_DIR}/Engine" ]]; then
		for candidate in "$(cat "${SCRIPT_DIR}/default-channel" 2>/dev/null || true)" stable rc beta dev; do
			[[ -n "${candidate}" ]] || continue
			if [[ -d "${SCRIPT_DIR}/${candidate}/current/Engine" ]]; then
				APP_DIR="${SCRIPT_DIR}/${candidate}/current"
				break
			fi
		done
	fi
fi
QUIET="no"
STRICT="no"
FAIL=0

usage() {
	sed -n '2,16p' "$0" | sed 's/^# \{0,1\}//'
	exit "${1:-0}"
}

while [[ $# -gt 0 ]]; do
	case "$1" in
	--app-dir)
		[[ $# -ge 2 ]] || {
			echo "[fail] --app-dir requires a value." >&2
			exit 1
		}
		APP_DIR="$2"
		shift 2
		;;
	--quiet)
		QUIET="yes"
		shift
		;;
	--strict)
		STRICT="yes"
		shift
		;;
	-h | --help)
		usage 0
		;;
	*)
		echo "[fail] Unknown option: $1" >&2
		exit 1
		;;
	esac
done

say() { [[ "${QUIET}" == "yes" ]] || printf '%s\n' "$*"; }
good() { say "  [ ok ] $*"; }
bad() {
	if [[ "${QUIET}" == "yes" ]]; then
		printf '  [fail] %s\n' "$*" >&2
	else
		printf '  [fail] %s\n' "$*"
	fi
	FAIL=$((FAIL + 1))
}
warn() { say "  [warn] $*"; }

BIN_DIR="${APP_DIR}/Engine/Binaries/Linux"
LAUNCHER="${BIN_DIR}/kaizen"

[[ -d "${APP_DIR}" ]] || {
	echo "[fail] Application directory not found: ${APP_DIR}" >&2
	exit 1
}

say "Kaizen dependency report"
say "  application: ${APP_DIR}"

# --- Engine --------------------------------------------------------------
say ""
say "Engine"
if [[ -x "${LAUNCHER}" ]]; then
	good "launcher: ${LAUNCHER}"
else
	bad "launcher missing or not executable: ${LAUNCHER}"
fi

# --- C++ scripts --------------------------------------------------------
say ""
say "C++ scripts"
CXX_FOUND=""
for candidate in "${KAIZEN_CPP_COMPILER:-}" "${CXX:-}" clang++ g++ c++; do
	if [[ -n "${candidate}" ]] && command -v "${candidate}" >/dev/null 2>&1; then
		CXX_FOUND="${candidate}"
		break
	fi
done
if [[ -n "${CXX_FOUND}" ]]; then
	good "C++ compiler: ${CXX_FOUND}"
else
	warn "no C++ compiler found (C++ scripts need clang++ or g++, or set KAIZEN_CPP_COMPILER)"
fi

# --- Dynamically loaded system libraries ---------------------------------
say ""
say "System libraries (loaded at runtime)"
REQUIRED_LIBS=(
	libX11.so.6
	libXcursor.so.1
	libXinerama.so.1
	libXrandr.so.2
	libXrender.so.1
	libXi.so.6
	libGL.so.1
	libfontconfig.so.1
	libfreetype.so.6
	libasound.so.2
	libudev.so.1
	libdbus-1.so.3
)
OPTIONAL_LIBS=(libvulkan.so.1 libEGL.so.1 libspeechd.so.2)

have_lib() {
	local cache
	if command -v ldconfig >/dev/null 2>&1; then
		cache="$(ldconfig -p 2>/dev/null || true)"
		grep -q "[[:space:]]$1[[:space:]]" <<<"${cache}"
	else
		local found
		found="$(find /usr/lib /usr/lib64 /lib /lib64 -maxdepth 3 -name "$1" -print -quit 2>/dev/null || true)"
		[[ -n "${found}" ]]
	fi
}

MISSING=()
for lib in "${REQUIRED_LIBS[@]}"; do
	if have_lib "${lib}"; then
		good "${lib}"
	else
		MISSING+=("${lib}")
		bad "${lib} (missing)"
	fi
done
for lib in "${OPTIONAL_LIBS[@]}"; do
	if have_lib "${lib}"; then
		good "${lib} (optional)"
	else
		warn "${lib} not found (optional; Vulkan rendering unavailable)"
	fi
done

if [[ ${#MISSING[@]} -gt 0 ]]; then
	warn "install the corresponding system packages (for example: mesa-libGL, libX11,"
	warn "libXcursor, libXinerama, libXrandr, fontconfig, freetype, alsa-lib, systemd-libs)."
fi

# --- Display -------------------------------------------------------------
say ""
say "Session"
if [[ -n "${WAYLAND_DISPLAY:-}" ]]; then
	good "Wayland: ${WAYLAND_DISPLAY}"
elif [[ -n "${DISPLAY:-}" ]]; then
	good "X11: ${DISPLAY}"
else
	warn "no DISPLAY/WAYLAND_DISPLAY set (headless use only)"
fi

say ""
if [[ "${FAIL}" -gt 0 ]]; then
	if [[ "${STRICT}" == "yes" ]]; then
		printf '[fail] %d required dependency(ies) missing.\n' "${FAIL}" >&2
		exit 1
	fi
	printf '[warn] %d required dependency(ies) missing.\n' "${FAIL}" >&2
	exit 0
fi
say "All required dependencies were found."
