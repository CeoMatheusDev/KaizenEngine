#!/usr/bin/env python3
"""Tests for check_api_glue.py.

The checker exists because a fork commit added four enumerators to a bound enum and then
edited the generated C# glue by hand, with no doc XML entries at all. Nothing in the tree
could have noticed that. These tests drive the checker over a synthetic four-source tree,
built from data rather than by string-poking, and break one source at a time: each test
asserts the specific mismatch is named, in the file that has to change.

Run: python3 -m unittest discover -s tests/kaizen/api_glue -p 'test_*.py'
     (or ./tests/kaizen/run_tests.sh, which does exactly that)
"""

import os
import shutil
import sys
import tempfile
import unittest

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

import check_api_glue  # noqa: E402

SPEC = check_api_glue.SPECS[0]

# A model of the tree small enough to read: five enumerators in C++, three of them bound,
# and RENDERING_INFO_UNUSED deliberately placed after C so that it must not move C's value,
# must not appear in the glue, and must not be demanded in the XML. Those three "must nots"
# are what a checker that guesses from one source instead of reading all four gets wrong.
CPP_ENUM = ["RENDERING_INFO_A", "RENDERING_INFO_B", "RENDERING_INFO_C", "RENDERING_INFO_UNUSED", "RENDERING_INFO_MAX"]
BOUND = ["RENDERING_INFO_A", "RENDERING_INFO_B", "RENDERING_INFO_C"]
# name -> (xml value, description)
DOC = {
    "RENDERING_INFO_A": (0, "First counter."),
    "RENDERING_INFO_B": (1, "Second counter. See [constant RENDERING_INFO_A]."),
    "RENDERING_INFO_C": (2, "Third counter, and it references [member rendering/occlusion_culling/occlusion_rays_per_thread] too."),
}
# name -> (value, C# <para> text or None for "no doc block")
GLUE = {
    "A": (0, "First counter."),
    "B": (1, 'Second counter. See <see cref="Kaizen.RenderingServer.RenderingInfo.A"/>.'),
    "C": (2, "Third counter, and it references <c>ProjectSettings.rendering/occlusion_culling/occlusion_rays_per_thread</c> too."),
}


def render_enums(cpp_enum):
    body = "".join(f"\t{name},\n" for name in cpp_enum)
    return f"enum RenderingInfo {{\n{body}}};\n"


def render_binds(bound):
    return "".join(f"\tBIND_ENUM_CONSTANT(RSE::{name});\n" for name in bound)


def render_doc(doc):
    out = ""
    for name, (value, text) in doc.items():
        out += f'\t<constant name="{name}" value="{value}" enum="RenderingInfo">\n'
        out += f"\t\t{text}\n" if text else "\n"
        out += "\t</constant>\n"
    return out


def render_glue(glue, indent="\t"):
    out = f"{indent}public enum RenderingInfo : long\n{indent}{{\n"
    items = list(glue.items())
    for index, (name, (value, text)) in enumerate(items):
        if text is not None:
            out += f"{indent}{indent}/// <summary>\n"
            out += f"{indent}{indent}/// <para>{text}</para>\n"
            out += f"{indent}{indent}/// </summary>\n"
        out += f"{indent}{indent}{name} = {value}{',' if index < len(items) - 1 else ''}\n"
    out += f"{indent}}}\n"
    return out


class TreeCase(unittest.TestCase):
    def build(self, cpp_enum=None, bound=None, doc=None, glue=None, indent="\t"):
        """Writes a tree from the model above, with any of the four sources replaced."""
        files = {
            SPEC["cpp_enums"]: render_enums(CPP_ENUM if cpp_enum is None else cpp_enum),
            SPEC["cpp_binds"]: render_binds(BOUND if bound is None else bound),
            SPEC["doc_xml"]: render_doc(DOC if doc is None else doc),
            SPEC["cs_glue"]: render_glue(GLUE if glue is None else glue, indent=indent),
        }
        root = tempfile.mkdtemp()
        self.addCleanup(self._remove, root)
        for rel, text in files.items():
            path = os.path.join(root, rel)
            os.makedirs(os.path.dirname(path), exist_ok=True)
            with open(path, "w", encoding="utf-8") as handle:
                handle.write(text)
        return root

    @staticmethod
    def _remove(root):
        shutil.rmtree(root, ignore_errors=True)

    def problems(self, **kwargs):
        return check_api_glue.check_spec(self.build(**kwargs), SPEC, False)

    def assert_named(self, problems, needle):
        self.assertTrue(any(needle in p for p in problems), f"{needle!r} not in {problems}")


class TestAgreement(TreeCase):
    def test_consistent_tree_has_no_problems(self):
        self.assertEqual(self.problems(), [])

    def test_reindenting_the_glue_does_not_break_the_parse(self):
        self.assertEqual(self.problems(indent="    "), [])

    def test_unbound_enumerator_is_not_required_anywhere(self):
        self.assertIn("RENDERING_INFO_UNUSED", check_api_glue.parse_cpp_enum(render_enums(CPP_ENUM), "RenderingInfo"))
        self.assertNotIn("RENDERING_INFO_UNUSED", check_api_glue.parse_bound_enums(render_binds(BOUND), "RENDERING_INFO_"))
        self.assertEqual(self.problems(), [])

    def test_values_come_from_declaration_order_not_from_the_xml(self):
        # C is at position 2 in C++ and documented as 2; the entry after it in C++ is the
        # unbound one. Renumbering the XML to match a C++ insert is the only correct fix,
        # and this asserts which of the two sources the checker treats as authoritative.
        self.assertEqual(self.problems(), [])
        shifted = dict(DOC)
        shifted["RENDERING_INFO_C"] = (3, DOC["RENDERING_INFO_C"][1])
        problems = self.problems(doc=shifted)
        self.assert_named(problems, "RENDERING_INFO_C documented as value 3, but its position in enum RenderingInfo is 2")


class TestEachSourceIsChecked(TreeCase):
    def test_missing_doc_entry_is_named_with_its_value(self):
        doc = {k: v for k, v in DOC.items() if k != "RENDERING_INFO_C"}
        problems = self.problems(doc=doc)
        self.assert_named(problems, 'missing for RENDERING_INFO_C (value 2)')

    def test_glue_entry_with_no_bound_enumerator_behind_it(self):
        glue = dict(GLUE)
        glue["Invented"] = (42, "Nothing behind this one.")
        problems = self.problems(glue=glue)
        self.assert_named(problems, "Invented = 42 has no bound enumerator behind it")

    def test_glue_entry_removed(self):
        glue = {k: v for k, v in GLUE.items() if k != "C"}
        problems = self.problems(glue=glue)
        self.assert_named(problems, "C = 2 missing from the generated glue")

    def test_glue_value_disagreeing_with_cpp(self):
        glue = dict(GLUE)
        glue["C"] = (1, GLUE["C"][1])
        problems = self.problems(glue=glue)
        self.assert_named(problems, "C = 1, expected 2 from")

    def test_stale_doc_entry_for_an_unbound_enumerator(self):
        doc = dict(DOC)
        doc["RENDERING_INFO_UNUSED"] = (3, "Documented but never bound.")
        problems = self.problems(doc=doc)
        self.assert_named(problems, "RENDERING_INFO_UNUSED documented but not bound")

    def test_doc_entry_for_a_name_that_is_not_in_the_enum(self):
        doc = dict(DOC)
        doc["RENDERING_INFO_GHOST"] = (9, "Renamed away in C++.")
        problems = self.problems(doc=doc)
        self.assert_named(problems, "RENDERING_INFO_GHOST documented but not in enum RenderingInfo")

    def test_glue_without_a_summary_block(self):
        glue = dict(GLUE)
        glue["A"] = (0, None)
        problems = self.problems(glue=glue)
        self.assert_named(problems, "A has no <summary>")

    def test_empty_doc_description(self):
        doc = dict(DOC)
        doc["RENDERING_INFO_A"] = (0, "")
        problems = self.problems(doc=doc)
        self.assert_named(problems, "RENDERING_INFO_A has an empty description")

    def test_doc_text_drift(self):
        glue = dict(GLUE)
        glue["A"] = (0, "First counter, reworded by hand.")
        problems = self.problems(glue=glue)
        self.assert_named(problems, "A doc text differs from doc/classes/RenderingServer.xml")

    def test_constant_reference_must_be_translated(self):
        glue = dict(GLUE)
        glue["B"] = (1, "Second counter. See Kaizen.RenderingServer.RenderingInfo.A.")
        problems = self.problems(glue=glue)
        self.assert_named(problems, "B doc text differs")

    def test_markup_this_check_does_not_model_is_skipped(self):
        doc = dict(DOC)
        doc["RENDERING_INFO_A"] = (0, "First counter, see [enum PipelineSource].")
        glue = dict(GLUE)
        glue["A"] = (0, "First counter, see <code>PipelineSource</code>.")
        self.assertEqual(self.problems(doc=doc, glue=glue), [])

    def test_a_moved_source_file_is_an_error_not_a_pass(self):
        broken = dict(SPEC)
        broken["doc_xml"] = "doc/classes/Nope.xml"
        with self.assertRaises(LookupError):
            check_api_glue.check_spec(self.build(), broken, False)


class TestRealTree(TreeCase):
    """The check that guards the fork's own change, run against this repository."""

    repo_root = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "..", ".."))

    def test_rendering_info_agrees_across_all_four_sources(self):
        try:
            problems = check_api_glue.check_spec(self.repo_root, SPEC, False)
        except LookupError as error:
            self.skipTest(f"engine sources not available: {error}")
        self.assertEqual(problems, [])

    def test_pascal_rule_matches_the_names_the_glue_actually_uses(self):
        # The naming rule is upstream generator behaviour, so it is verified against the
        # entries that already agree rather than asserted from memory: if the generator ever
        # changes its casing, this test says so instead of the checker going quiet on drift.
        glue_path = os.path.join(self.repo_root, SPEC["cs_glue"])
        doc_path = os.path.join(self.repo_root, SPEC["doc_xml"])
        if not (os.path.exists(glue_path) and os.path.exists(doc_path)):
            self.skipTest("generated glue or class reference not present")
        with open(glue_path, encoding="utf-8") as handle:
            glue = check_api_glue.parse_cs_enum(handle.read(), SPEC["enum"])
        with open(doc_path, encoding="utf-8") as handle:
            doc = check_api_glue.parse_doc_constants(handle.read(), SPEC["enum"])
        self.assertGreater(len(doc), 10, "expected the real enum, not a fixture")
        for enumerator in doc:
            self.assertIn(check_api_glue.pascal(enumerator, SPEC["prefix"]), glue, enumerator)


if __name__ == "__main__":
    unittest.main(verbosity=2)
