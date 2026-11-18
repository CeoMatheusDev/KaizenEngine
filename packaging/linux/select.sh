#!/usr/bin/env bash
#
# Kaizen Engine — channel/version selector.
#
# Changes the default channel (used by the "kaizen" command) and the "current"
# version of a channel, without touching installed files or user data.
#
# Usage:
#   kaizen-select --list                 List installed channels/versions
#   kaizen-select --show                 Show the current selection
#   kaizen-select CHANNEL                Make CHANNEL the default
#   kaizen-select CHANNEL/VERSION        Also repoint CHANNEL/current
#
# Options:
#       --opt-root DIR    Installation root (default: $KZ_OPT_ROOT,
#                         ~/.local/opt/kaizen, or /opt/kaizen)
#   -h, --help            Show this help

set -euo pipefail

die() {
	printf '[fail] %s\n' "$*" >&2
	exit 1
}
info() { printf '[kz] %s\n' "$*"; }
warn() { printf '[warn] %s\n' "$*" >&2; }

usage() {
	sed -n '2,18p' "$0" | sed 's/^# \{0,1\}//'
	exit "${1:-0}"
}

OPT_ROOT="${KZ_OPT_ROOT:-}"
ACTION=""
TARGET=""

while [[ $# -gt 0 ]]; do
	case "$1" in
	--opt-root)
		[[ $# -ge 2 ]] || die "--opt-root requires a value."
		OPT_ROOT="$2"
		shift 2
		;;
	--list)
		ACTION="list"
		shift
		;;
	--show)
		ACTION="show"
		shift
		;;
	-h | --help)
		usage 0
		;;
	-*)
		die "Unknown option: $1 (use --help)"
		;;
	*)
		[[ -z "${TARGET}" ]] || die "Only one channel/version argument is accepted."
		TARGET="$1"
		shift
		;;
	esac
done

if [[ -z "${OPT_ROOT}" ]]; then
	if [[ -d "${HOME}/.local/opt/kaizen" ]]; then
		OPT_ROOT="${HOME}/.local/opt/kaizen"
	elif [[ -d "/opt/kaizen" ]]; then
		OPT_ROOT="/opt/kaizen"
	else
		die "No Kaizen installation found. Pass --opt-root."
	fi
fi
OPT_ROOT="$(realpath -m "${OPT_ROOT}")"
[[ -d "${OPT_ROOT}" ]] || die "Installation root not found: ${OPT_ROOT}"

receipt_get() { { grep -m1 "^$2=" "$1" 2>/dev/null || true; } | cut -d= -f2-; }
is_version_dir() { [[ -f "$1/.kaizen-receipt.env" ]] && [[ "$(receipt_get "$1/.kaizen-receipt.env" PRODUCT)" == "Kaizen" ]]; }

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

show_selection() {
	local ch
	for ch in $(list_channels); do
		printf '  %-10s current: %s\n' "${ch}" "$(readlink "${OPT_ROOT}/${ch}/current" 2>/dev/null || echo '(none)')"
	done
}

do_list() {
	local default ch v marker
	default="$(cat "${OPT_ROOT}/default-channel" 2>/dev/null || true)"
	if [[ -z "$(list_channels)" ]]; then
		info "No versions installed under ${OPT_ROOT}."
		return 0
	fi
	printf 'Installation: %s\n' "${OPT_ROOT}"
	printf 'Default channel: %s\n\n' "${default:-(none)}"
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

do_show() {
	local default
	default="$(cat "${OPT_ROOT}/default-channel" 2>/dev/null || true)"
	printf 'Installation : %s\n' "${OPT_ROOT}"
	printf 'Default      : %s\n' "${default:-(none)}"
	show_selection
}

do_select() {
	local spec="${1}" ch ver
	ch="${spec%%/*}"
	ver=""
	[[ "${spec}" == */* ]] && ver="${spec#*/}"

	case "${ch}" in
	stable | beta | rc | dev) ;;
	*) die "Invalid channel '${ch}'. Use stable, beta, rc or dev." ;;
	esac
	[[ -n "$(list_versions "${ch}")" ]] || die "Channel '${ch}' has no installed versions under ${OPT_ROOT}."

	if [[ -n "${ver}" ]]; then
		[[ "${ver}" =~ ^[0-9]+\.[0-9]+\.[0-9]+(-(beta|rc|dev)[1-9][0-9]*)?$ ]] ||
			die "Invalid version '${ver}'."
		is_version_dir "${OPT_ROOT}/${ch}/${ver}" || die "Version not installed: ${ch}/${ver}"
		ln -sfn "${ver}" "${OPT_ROOT}/${ch}/current"
		info "Channel ${ch}/current -> ${ver}"
	else
		local cur
		cur="$(readlink "${OPT_ROOT}/${ch}/current" 2>/dev/null || true)"
		if [[ -z "${cur}" || ! -d "${OPT_ROOT}/${ch}/${cur}" ]]; then
			# Repair a missing/dangling pointer deterministically (newest).
			ver="$(list_versions "${ch}" | tail -n1)"
			[[ -n "${ver}" ]] || die "Cannot determine a version for channel '${ch}'."
			ln -sfn "${ver}" "${OPT_ROOT}/${ch}/current"
			info "Channel ${ch}/current repaired -> ${ver}"
		fi
	fi

	printf '%s\n' "${ch}" >"${OPT_ROOT}/default-channel.tmp.$$"
	mv -f "${OPT_ROOT}/default-channel.tmp.$$" "${OPT_ROOT}/default-channel"
	info "Default channel: ${ch}"
	info "Run 'kaizen' to start it (or 'kaizen-${ch}' directly)."
}

case "${ACTION}" in
list) do_list ;;
show) do_show ;;
"")
	if [[ -n "${TARGET}" ]]; then
		do_select "${TARGET}"
	else
		do_show
	fi
	;;
esac
