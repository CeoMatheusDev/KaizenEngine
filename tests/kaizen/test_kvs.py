#!/usr/bin/env python3
"""Unit tests for the Kaizen Versioning System (KVS) helpers."""

from __future__ import annotations

import os
import sys
import unittest

sys.path.insert(0, os.path.abspath(os.path.join(os.path.dirname(__file__), "../..")))

import kvs
import version


class KvsFormatTests(unittest.TestCase):
    def test_parse_and_format(self) -> None:
        for text in ("1.4.2a3", "1.0.0b2", "1.0.0rc1", "1.0.0f1", "1.0.0p1"):
            self.assertEqual(str(kvs.Version.parse(text)), text)

    def test_channel_metadata(self) -> None:
        expected = {
            "a": ("alpha", 0, False),
            "b": ("beta", 1, False),
            "rc": ("rc", 2, False),
            "f": ("final", 3, True),
            "p": ("patch", 4, True),
        }
        for code, (name, index, stable) in expected.items():
            self.assertEqual(kvs.channel_name(code), name)
            self.assertEqual(kvs.channel_id(code), index)
            self.assertEqual(kvs.is_stable(code), stable)

    def test_aliases(self) -> None:
        self.assertEqual(kvs.normalize_channel("alpha"), "a")
        self.assertEqual(kvs.normalize_channel("beta"), "b")
        self.assertEqual(kvs.normalize_channel("beta3"), "b")
        self.assertEqual(kvs.normalize_channel("stable"), "f")
        self.assertEqual(kvs.normalize_channel("rc2"), "rc")
        self.assertEqual(kvs.normalize_channel("patch"), "p")

    def test_package_channels(self) -> None:
        self.assertEqual(kvs.package_channel("a"), "dev")
        self.assertEqual(kvs.package_channel("b"), "beta")
        self.assertEqual(kvs.package_channel("rc"), "rc")
        self.assertEqual(kvs.package_channel("f"), "stable")
        self.assertEqual(kvs.package_channel("p"), "stable")

    def test_invalid_values(self) -> None:
        for text in ("1.0.0", "1.0.0x1", "1.0.0a0", "1.0.0b", "v1.0.0a1"):
            with self.assertRaises(ValueError):
                kvs.Version.parse(text)
        with self.assertRaises(ValueError):
            kvs.normalize_channel("gamma")


class KvsModuleTests(unittest.TestCase):
    def test_current_version(self) -> None:
        current = kvs.Version.from_module(version)
        self.assertEqual(current.major, version.major)
        self.assertEqual(current.minor, version.minor)
        self.assertEqual(current.patch, version.patch)
        self.assertEqual(current.channel, kvs.normalize_channel(version.channel))
        self.assertEqual(current.revision, version.revision)
        self.assertRegex(str(current), r"^[0-9]+\.[0-9]+\.[0-9]+(a|b|rc|f|p)[1-9][0-9]*$")

    def test_environment_overrides(self) -> None:
        overridden = kvs.Version.from_module(
            version,
            {"KAIZEN_VERSION_CHANNEL": "rc", "KAIZEN_VERSION_REVISION": "7"},
        )
        self.assertEqual(overridden.channel, "rc")
        self.assertEqual(overridden.revision, 7)

    def test_legacy_status_override(self) -> None:
        overridden = kvs.Version.from_module(version, {"KAIZEN_VERSION_STATUS": "beta3"})
        self.assertEqual(overridden.channel, "b")
        self.assertEqual(overridden.revision, 3)


if __name__ == "__main__":
    unittest.main()