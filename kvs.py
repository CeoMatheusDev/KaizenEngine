"""Kaizen Versioning System (KVS).

KVS is the official versioning scheme of the Kaizen Engine. It replaces the
upstream `MAJOR.MINOR.PATCH.status` convention with a compact, structured
identifier that encodes both the release stage and its revision.

> **Current engine version:** `4.8.0b1` (KVS specification `1.0`).

## 1. Format

```text
MAJOR.MINOR.PATCH + CHANNEL + REVISION
```

Example:

```text
4.8.0b1
```

| Component | Meaning |
|---|---|
| `MAJOR` | Main engine generation. |
| `MINOR` | Development cycle inside the generation. |
| `PATCH` | Corrective update. |
| `CHANNEL` | Development stage (`a`, `b`, `rc`, `f`, `p`). |
| `REVISION` | Build number for that stage (always `>= 1`). |

The three numeric components are always present, including `PATCH = 0`.

## 2. Channels

| Code | Stage | Meaning |
|---|---|---|
| `a` | Alpha | Active development. |
| `b` | Beta | Features being stabilised. |
| `rc` | Release Candidate | Candidate for the final release. |
| `f` | Final | Stable release. |
| `p` | Patch | Corrective update after a stable release. |

Examples:

```text
1.0.0a1   1.0.0a2   1.0.0b1   1.0.0b2
1.0.0rc1  1.0.0f1   1.0.0p1
```

## 3. Increment rules

| Transition | Rule |
|---|---|
| `1.0.0a1` → `1.0.0a2` | New alpha build; increment `REVISION`. |
| `1.0.0a2` → `1.0.0b1` | Enter beta; change channel and reset `REVISION` to `1`. |
| `1.0.0b2` → `1.0.0rc1` | Enter release candidate; reset `REVISION`. |
| `1.0.0rc1` → `1.0.0f1` | Stable release; reset `REVISION`. |
| `1.0.0f1` → `1.0.0p1` | Corrective update; channel `p`, reset `REVISION`. |
| `1.0.0p1` → `1.0.1p1` | Patch number increment for a new corrective line. |
| `1.9.0f1` → `2.0.0a1` | New generation; increment `MAJOR` and return to alpha. |

The channel order used for comparisons is `a < b < rc < f < p`.

## 4. Independent identifiers

Kaizen keeps three identifiers separate:

| Identifier | Purpose | Source |
|---|---|---|
| **Engine Version** | Public KVS version of the engine (`4.8.0b1`). | `version.py` |
| **KVS Version** | Version of the versioning specification itself (`1.0`). | `kvs.py` (`KVS_SPEC_VERSION`) |
| **ABI Version** | Binary compatibility generation for engine modules (`1`). | `version.py` (`abi_version`) |

The ABI version only changes when the module binary interface breaks. It is
independent from the public engine version.

## 5. Implementation

The version is never parsed from a string at runtime. It is stored as numeric
fields and formatted only when needed.

### `version.py`

```python
major = 4
minor = 8
patch = 0
channel = "b"
revision = 1
abi_version = 1
```

### Generated C++ header

The build generates `core/version_generated.gen.h` with:

```cpp
#define KAIZEN_VERSION_MAJOR 4
#define KAIZEN_VERSION_MINOR 8
#define KAIZEN_VERSION_PATCH 0
#define KAIZEN_VERSION_CHANNEL "b"
#define KAIZEN_VERSION_CHANNEL_NAME "beta"
#define KAIZEN_VERSION_CHANNEL_ID 1
#define KAIZEN_VERSION_REVISION 1
#define KAIZEN_VERSION_KVS_SPEC "1.0"
#define KAIZEN_VERSION_ABI 1
```

`core/version.h` exposes the structured and formatted values:

```cpp
enum class VersionChannel : uint8_t {
    Alpha = 0,
    Beta = 1,
    ReleaseCandidate = 2,
    Final = 3,
    Patch = 4,
};

inline constexpr VersionChannel KAIZEN_VERSION_CHANNEL_ENUM = ...;
inline constexpr bool KAIZEN_VERSION_IS_STABLE = ...;

#define KAIZEN_VERSION_KVS     "4.8.0b1"
#define KAIZEN_VERSION_NUMBER  "4.8.0"
#define KAIZEN_VERSION_BRANCH  "4.8"
```

`KAIZEN_VERSION_STATUS` remains available as a legacy human-readable status
(`alpha`, `beta`, `rc`, `final`, `patch`)."""

from __future__ import annotations

import os
import re
from dataclasses import dataclass

#: Version of the KVS specification implemented by this module.
KVS_SPEC_VERSION = "1.0"

#: Canonical channel codes and their human-readable names.
CHANNELS = {
    "a": "alpha",
    "b": "beta",
    "rc": "rc",
    "f": "final",
    "p": "patch",
}

#: Numeric channel order, used by the generated C++ header and comparisons.
CHANNEL_IDS = {
    "a": 0,
    "b": 1,
    "rc": 2,
    "f": 3,
    "p": 4,
}

#: Distribution channel used by the packaging scripts for each KVS channel.
PACKAGE_CHANNELS = {
    "a": "dev",
    "b": "beta",
    "rc": "rc",
    "f": "stable",
    "p": "stable",
}

_ALIASES = {
    "a": "a",
    "alpha": "a",
    "dev": "a",
    "development": "a",
    "nightly": "a",
    "b": "b",
    "beta": "b",
    "rc": "rc",
    "release-candidate": "rc",
    "release_candidate": "rc",
    "releasecandidate": "rc",
    "f": "f",
    "final": "f",
    "stable": "f",
    "release": "f",
    "official": "f",
    "p": "p",
    "patch": "p",
}

_KVS_RE = re.compile(r"^(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)(a|b|rc|f|p)([1-9][0-9]*)$")
_STATUS_REVISION_RE = re.compile(r"^(.*?)([0-9]+)$")


def normalize_channel(value: object) -> str:
    """Return the canonical KVS channel code for ``value``.

    Accepts channel codes (``a``, ``b``, ``rc``, ``f``, ``p``), names
    (``alpha``, ``beta``, ``final``, ...) and legacy status strings with a
    trailing revision (``beta3``, ``rc1``, ``stable``).
    """

    text = str(value).strip().lower()
    if not text:
        raise ValueError("KVS channel cannot be empty.")

    # Legacy status strings such as "beta3" / "rc1" carry the revision inline.
    match = _STATUS_REVISION_RE.match(text)
    if match and match.group(1) in _ALIASES:
        text = match.group(1)

    try:
        return _ALIASES[text]
    except KeyError:
        valid = ", ".join(f"{code} ({name})" for code, name in CHANNELS.items())
        raise ValueError(f"Invalid KVS channel {value!r}. Expected one of: {valid}.") from None


def channel_name(channel: object) -> str:
    """Return the human-readable name for a KVS channel."""

    return CHANNELS[normalize_channel(channel)]


def channel_id(channel: object) -> int:
    """Return the numeric ordering index for a KVS channel."""

    return CHANNEL_IDS[normalize_channel(channel)]


def is_stable(channel: object) -> bool:
    """Whether ``channel`` represents a stable release (final or patch)."""

    return normalize_channel(channel) in ("f", "p")


def package_channel(channel: object) -> str:
    """Return the distribution channel used by packaging for ``channel``."""

    return PACKAGE_CHANNELS[normalize_channel(channel)]


@dataclass(frozen=True)
class Version:
    """A structured KVS version.

    The version is kept numeric and structured at runtime; it is only turned
    into the compact ``MAJOR.MINOR.PATCH<channel><revision>`` string when it
    needs to be displayed or written to a file.
    """

    major: int
    minor: int
    patch: int
    channel: str
    revision: int

    def __post_init__(self) -> None:
        for field in ("major", "minor", "patch", "revision"):
            value = getattr(self, field)
            if not isinstance(value, int) or isinstance(value, bool):
                raise ValueError(f"KVS field '{field}' must be an integer, got {value!r}.")
            if value < 0:
                raise ValueError(f"KVS field '{field}' cannot be negative, got {value!r}.")
        if self.revision < 1:
            raise ValueError(f"KVS revision must be >= 1, got {self.revision!r}.")
        object.__setattr__(self, "channel", normalize_channel(self.channel))

    @property
    def channel_name(self) -> str:
        return CHANNELS[self.channel]

    @property
    def channel_id(self) -> int:
        return CHANNEL_IDS[self.channel]

    @property
    def is_stable(self) -> bool:
        return self.channel in ("f", "p")

    def __str__(self) -> str:
        return f"{self.major}.{self.minor}.{self.patch}{self.channel}{self.revision}"

    @classmethod
    def parse(cls, text: str) -> "Version":
        """Parse a canonical KVS string such as ``1.4.2a3``."""
        match = _KVS_RE.match(text.strip())
        if not match:
            raise ValueError(f"Invalid KVS version {text!r}. Expected MAJOR.MINOR.PATCH<channel><revision>, e.g. 1.4.2a3.")
        major, minor, patch, channel, revision = match.groups()
        return cls(int(major), int(minor), int(patch), channel, int(revision))

    @classmethod
    def from_module(cls, module: object, environ: dict[str, str] | None = None) -> "Version":
        """Build a version from a module exposing the ``version.py`` fields.

        ``KAIZEN_VERSION_CHANNEL`` and ``KAIZEN_VERSION_REVISION`` can override
        the values from the module. The legacy ``KAIZEN_VERSION_STATUS``
        variable is also accepted (``beta3`` sets channel ``b`` and revision 3).
        """

        env = os.environ if environ is None else environ

        channel = env.get("KAIZEN_VERSION_CHANNEL", getattr(module, "channel"))
        revision = env.get("KAIZEN_VERSION_REVISION", getattr(module, "revision"))

        legacy_status = env.get("KAIZEN_VERSION_STATUS")
        if legacy_status and "KAIZEN_VERSION_CHANNEL" not in env:
            channel = legacy_status
            match = _STATUS_REVISION_RE.match(str(legacy_status).strip().lower())
            if match and "KAIZEN_VERSION_REVISION" not in env:
                revision = match.group(2)

        return cls(
            int(getattr(module, "major")),
            int(getattr(module, "minor")),
            int(getattr(module, "patch")),
            channel,
            int(revision),
        )


__all__ = [
    "CHANNELS",
    "CHANNEL_IDS",
    "KVS_SPEC_VERSION",
    "PACKAGE_CHANNELS",
    "Version",
    "channel_id",
    "channel_name",
    "is_stable",
    "normalize_channel",
    "package_channel",
]