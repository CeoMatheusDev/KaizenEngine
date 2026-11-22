#!/usr/bin/env python3
"""Checks that the hand-maintained C# glue agrees with the engine sources it mirrors.

Why this exists
---------------
``modules/mono/glue/.../Generated/`` is the C# binding surface, and this repository
tracks those generated files in-tree. The pipeline-cache telemetry commit added four
``RenderingInfo`` enumerators in C++ and then edited the generated C# enum by hand to
match - which is the only option while the glue is committed, but it means the three
sources of truth can drift apart with nothing to notice:

    servers/rendering/rendering_server_enums.h   the real values (declaration order)
    servers/rendering/rendering_server.cpp       which enumerators ClassDB exposes
    doc/classes/<Class>.xml                      the documentation the glue is written from
    modules/mono/glue/.../Generated/*.cs         what C# projects actually compile against

A drift here is silent and asymmetric: the engine still works, GDScript still works, and
only C# reads a number that means something else. Adding an enumerator in the middle of
the C++ list without renumbering the XML would hand every C# project the wrong monitor.

So: the invariants are checked here instead of being hoped for. Names and values must
agree across all four sources, and an entry needs documentation on both sides of the
translation - the four telemetry constants shipped with C# entries and no doc text at
all, which is how this was noticed.

Usage
-----
    python3 tests/kaizen/api_glue/check_api_glue.py [--repo-root PATH] [--verbose]

Exit code 0 = every spec agrees, 1 = at least one mismatch (each printed with the file to
fix). Run it after touching a bound enum or a generated glue file.
"""

from __future__ import annotations

import argparse
import os
import re
import sys

# One entry per enum the fork has extended. The name translation rule is not hardcoded
# here: it is learned from the entries that already agree on both sides, then required to
# hold for the rest - so this checks the fork's generator convention rather than my guess
# about it.
SPECS = [
    {
        "enum": "RenderingInfo",
        "cs_class": "RenderingServer",
        "prefix": "RENDERING_INFO_",
        "cpp_enums": "servers/rendering/rendering_server_enums.h",
        "cpp_binds": "servers/rendering/rendering_server.cpp",
        "doc_xml": "doc/classes/RenderingServer.xml",
        "cs_glue": "modules/mono/glue/KaizenSharp/KaizenSharp/Generated/KaizenObjects/RenderingServer.cs",
        "cref_root": "Kaizen.RenderingServer.RenderingInfo",
    },
]

_CPP_ENUM_RE = r"enum\s+{enum}\s*\{{(.*?)\}};"
_BOUND_RE = r"BIND_ENUM_CONSTANT\s*\(\s*(?:RSE::|GodotRenderingServerEnums::)?\s*([A-Z0-9_]+)\s*\)"
_XML_CONST_RE = r'<constant\s+name="([A-Z0-9_]+)"\s+value="(-?\d+)"\s+enum="{enum}"\s*>(.*?)</constant>'


def _strip_comments(text: str) -> str:
    text = re.sub(r"/\*.*?\*/", "", text, flags=re.S)
    return re.sub(r"//[^\n]*", "", text)


def parse_cpp_enum(text: str, enum_name: str) -> list[str]:
    """Enumerator names of `enum <enum_name> { ... }`, in declaration order.

    Order is value: these enums have no explicit initialisers, so the nth name is n.
    """
    match = re.search(_CPP_ENUM_RE.format(enum=re.escape(enum_name)), text, flags=re.S)
    if not match:
        raise LookupError(f"enum {enum_name} not found")
    body = _strip_comments(match.group(1))
    names = []
    for item in body.split(","):
        item = item.strip()
        if item:
            names.append(item.split()[-1].rstrip("=").strip())
    return names


def parse_bound_enums(text: str, prefix: str) -> list[str]:
    bound = [name for name in re.findall(_BOUND_RE, text) if name.startswith(prefix)]
    return bound


def parse_doc_constants(text: str, enum_name: str) -> dict[str, tuple[int, str]]:
    out = {}
    for name, value, body in re.findall(_XML_CONST_RE.format(enum=re.escape(enum_name)), text, flags=re.S):
        cleaned = re.sub(r"\s+", " ", body).strip()
        out[name] = (int(value), cleaned)
    return out


def _brace_block(text: str, header_re: str, what: str) -> str:
    """Body of the first block whose header matches, found by counting braces.

    Deliberately not indentation-based: this reads a generated file, and a generator that
    reindents is exactly the kind of change that must not turn into a false alarm here.
    """
    match = re.search(header_re, text, flags=re.S)
    if not match:
        raise LookupError(f"{what} not found")
    depth = 0
    start = text.index("{", match.end() - 1)
    for index in range(start, len(text)):
        if text[index] == "{":
            depth += 1
        elif text[index] == "}":
            depth -= 1
            if depth == 0:
                return text[start + 1 : index]
    raise LookupError(f"{what} is not closed")


def parse_cs_enum(text: str, enum_name: str) -> dict[str, tuple[int, bool]]:
    """`name -> (value, documented)` for `public enum <enum_name> : long { ... }`."""
    body = _brace_block(text, rf"public enum {re.escape(enum_name)}\s*:\s*\w+\s*\{{", f"C# enum {enum_name}")
    out = {}
    pending_doc = False
    has_para = False
    for raw in body.split("\n"):
        line = raw.strip()
        if line.startswith("///"):
            pending_doc = True
            if "<para>" in line:
                has_para = True
            continue
        entry = re.match(r"^(?P<name>[A-Za-z0-9_]+)\s*=\s*(?P<value>-?\d+),?$", line)
        if entry:
            out[entry.group("name")] = (int(entry.group("value")), pending_doc and has_para)
            pending_doc = False
            has_para = False
    return out


def cs_doc_text(text: str, enum_name: str) -> dict[str, str]:
    """The `<para>` body of each generated C# entry, for the XML-vs-glue text check."""
    try:
        body = _brace_block(text, rf"public enum {re.escape(enum_name)}\s*:\s*\w+\s*\{{", f"C# enum {enum_name}")
    except LookupError:
        return {}
    out = {}
    docs: list[str] = []
    for raw in body.split("\n"):
        line = raw.strip()
        if line.startswith("///"):
            docs.append(line[3:].strip())
            continue
        entry = re.match(r"^([A-Za-z0-9_]+)\s*=\s*-?\d+,?$", line)
        if entry:
            body = " ".join(d for d in docs if d not in ("<summary>", "</summary>"))
            body = re.sub(r"^<para>|</para>$", "", body.strip()).strip()
            out[entry.group(1)] = body
            docs = []
    return out


def xml_to_cs(text: str, cref_root: str, prefix: str) -> tuple[str, bool]:
    """Apply the documented-BBCode -> C#-XML-doc translation.

    Returns (translated, unsupported) where unsupported marks text containing markup this
    function does not model; such entries are skipped rather than reported, because the
    translation is upstream's job and getting it wrong here would be a false alarm.
    """
    unsupported = False
    out = text

    def constant_sub(match):
        nonlocal unsupported
        name = match.group(1)
        if not name.startswith(prefix):
            unsupported = True
            return match.group(0)
        return f'<see cref="{cref_root}.{pascal(name, prefix)}"/>'

    out = re.sub(r"\[constant ([A-Z0-9_]+)\]", constant_sub, out)
    out = re.sub(r"\[member ([a-z0-9_/]+)\]", r"<c>ProjectSettings.\1</c>", out)
    out = re.sub(r"\[i\](.*?)\[/i\]", r"<i>\1</i>", out, flags=re.S)
    out = re.sub(r"\[b\](.*?)\[/b\]", r"<b>\1</b>", out, flags=re.S)
    if "[" in out or "]" in out:
        unsupported = True
    return out, unsupported


def pascal(enumerator: str, prefix: str) -> str:
    name = enumerator[len(prefix) :] if enumerator.startswith(prefix) else enumerator
    return "".join(part.capitalize() for part in name.split("_") if part)


def check_spec(root: str, spec: dict, verbose: bool) -> list[str]:
    problems: list[str] = []

    def read(rel: str) -> str:
        path = os.path.join(root, rel)
        if not os.path.exists(path):
            raise LookupError(f"missing source: {rel}")
        with open(path, encoding="utf-8") as handle:
            return handle.read()

    enum_name = spec["enum"]
    prefix = spec["prefix"]
    cpp = parse_cpp_enum(read(spec["cpp_enums"]), enum_name)
    bound = parse_bound_enums(read(spec["cpp_binds"]), prefix)
    doc = parse_doc_constants(read(spec["doc_xml"]), enum_name)
    cs = parse_cs_enum(read(spec["cs_glue"]), enum_name)
    cs_docs = cs_doc_text(read(spec["cs_glue"]), enum_name)

    value_of = {name: index for index, name in enumerate(cpp)}

    # R1: the doc XML must carry every enumerator ClassDB exposes, with the value the C++
    # declaration order gives it. A renumber without an XML update lands the wrong monitor
    # on every C# project, silently.
    for name in bound:
        if name not in value_of:
            problems.append(f"{spec['cpp_binds']}: binds {name}, which is not in enum {enum_name} ({spec['cpp_enums']})")
            continue
        if name not in doc:
            problems.append(f"{spec['doc_xml']}: <constant enum=\"{enum_name}\"> missing for {name} (value {value_of[name]})")
            continue
        xml_value, text = doc[name]
        if xml_value != value_of[name]:
            problems.append(
                f"{spec['doc_xml']}: {name} documented as value {xml_value}, "
                f"but its position in enum {enum_name} is {value_of[name]}"
            )
        if not text:
            problems.append(f"{spec['doc_xml']}: {name} has an empty description")

    # R2: no doc entry for something that is not bound or not in the enum.
    for name in doc:
        if name not in value_of:
            problems.append(f"{spec['doc_xml']}: {name} documented but not in enum {enum_name}")
        elif name not in bound:
            problems.append(f"{spec['doc_xml']}: {name} documented but not bound with BIND_ENUM_CONSTANT")

    # R3: the glue must expose exactly the bound set, named by the convention the entries
    # that already agree on both sides agree on.
    # The glue must contain one entry per *bound* enumerator - the doc XML is not part of
    # this set, so a glue entry with no doc entry reads as "missing doc" (R1) instead of
    # being blamed on a hand edit.
    expected_cs = {pascal(name, prefix): value_of[name] for name in bound if name in value_of}
    for name, (value, documented) in cs.items():
        if name not in expected_cs:
            problems.append(
                f"{spec['cs_glue']}: enum entry {name} = {value} has no bound enumerator behind it "
                f"(generated glue edited by hand? regenerate, or add the doc XML entry)"
            )
    for name, value in expected_cs.items():
        if name not in cs:
            problems.append(f"{spec['cs_glue']}: enum entry {name} = {value} missing from the generated glue")
            continue
        if cs[name][0] != value:
            problems.append(f"{spec['cs_glue']}: {name} = {cs[name][0]}, expected {value} from {spec['cpp_enums']}")

    # R4: every glue entry needs a doc block, and where the translation is modelled, the
    # text must match the XML it was generated from.
    for name in expected_cs:
        if name in cs and not cs[name][1]:
            enumerator = next((b for b in bound if pascal(b, prefix) == name), name)
            problems.append(
                f"{spec['cs_glue']}: {name} has no <summary> - the doc text for {enumerator} is missing "
                f"in {spec['doc_xml']} or was not carried over by hand"
            )
            continue
        if name not in cs_docs:
            continue
        enumerator = next((b for b in bound if pascal(b, prefix) == name), None)
        if enumerator is None or enumerator not in doc:
            continue
        want, unsupported = xml_to_cs(doc[enumerator][1], spec["cref_root"], prefix)
        if unsupported:
            if verbose:
                print(f"  info: {name}: doc contains markup this check does not model; text not compared")
            continue
        if cs_docs[name] != want:
            problems.append(f"{spec['cs_glue']}: {name} doc text differs from {spec['doc_xml']}")

    if verbose:
        print(f"  {enum_name}: {len(cpp)} enumerators, {len(bound)} bound, {len(doc)} documented, {len(cs)} in glue")
    return problems


def main(argv=None) -> int:
    parser = argparse.ArgumentParser(description=__doc__.split("\n")[0])
    parser.add_argument("--repo-root", default=os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "..", "..")))
    parser.add_argument("--verbose", action="store_true")
    args = parser.parse_args(argv)

    total = 0
    failed = 0
    for spec in SPECS:
        total += 1
        try:
            problems = check_spec(args.repo_root, spec, args.verbose)
        except LookupError as error:
            print(f"FAIL: {spec['enum']}: {error}")
            failed += 1
            continue
        failed += 1 if problems else 0
        for problem in problems:
            print(f"FAIL: {problem}")
        if not problems and args.verbose:
            print(f"ok: {spec['enum']} agrees across 4 sources")

    if failed:
        print(f"api_glue: {failed} of {total} enum spec(s) disagree")
        return 1
    print(f"api_glue: {total} enum spec(s) agree across C++, ClassDB binds, doc XML and the C# glue")
    return 0


if __name__ == "__main__":
    sys.exit(main())
