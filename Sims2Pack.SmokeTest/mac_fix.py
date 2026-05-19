#!/usr/bin/env python3
"""
Mac-fix a .Sims2Pack so it stops claiming to need an EP the Mac game lacks.

The Sims 2 lot DBPF stores its game/EP requirement in a VERS resource:
  - Property "0": display string  ("The Sims® 2 Mansion and Garden Stuff")
  - Property "1": build version   ("1.17.0.66")  ← parsed numerically

Format on disk (EXMP property block, repeated for each property):
    18 EA 8B 0B  marker (4 bytes) — string-property kind tag
    01 00 00 00  property-name length = 1
    NN           property name (ASCII '0' or '1')
    LL LL LL LL  string value length (uint32 LE)
    <value bytes, UTF-8>

The game reads property "1" as "1.<EP_index>.<build>.<patch>". If the EP
index references an expansion the running game doesn't have installed
(e.g. 17 = Mansion & Garden Stuff on the Aspyr Mac Super Collection),
loading the lot crashes.

This patcher finds every property "1" whose value matches "1.NN..." and
rewrites the EP index digits to all zeros, preserving the exact byte
length so no DBPF offsets, sizes, or .Sims2Pack envelope CRCs need
recalculating.

Usage:
    python3 mac_fix.py <input>...

Each <input> may be:
  - <name>.Sims2Pack       -> writes <name>-Mac.Sims2Pack alongside
  - <name>.Sims2Pack.zip   -> writes <name>-Mac.Sims2Pack.zip alongside
  - <name>.rar             -> extracts the inner .Sims2Pack and writes
                              <name>-Mac.Sims2Pack alongside the .rar
                              (.rar is not re-archived; use the plain
                              .Sims2Pack directly with the installer)

Files whose version is already base-game are skipped (no -Mac copy written).
"""

import sys
import os
import re
import io
import zipfile
import subprocess

PROPERTY_1_MARKER = bytes.fromhex("18 EA 8B 0B 01 00 00 00 31".replace(" ", ""))


def patch(data: bytearray) -> int:
    """Patch all property-'1' build-version values in-place. Returns count."""
    patches = 0
    pos = 0
    while True:
        i = data.find(PROPERTY_1_MARKER, pos)
        if i < 0:
            break
        len_off = i + len(PROPERTY_1_MARKER)
        val_off = len_off + 4
        length = int.from_bytes(data[len_off:len_off + 4], "little")

        # Sanity: length must be plausible (a build version isn't 1MB).
        if length == 0 or length > 64:
            pos = i + 1
            continue

        try:
            value = data[val_off:val_off + length].decode("utf-8")
        except UnicodeDecodeError:
            pos = i + 1
            continue

        # Value must look like "1.<digits>.<rest>".
        m = re.match(r"^1\.(\d+)\.(.*)$", value)
        if not m:
            pos = val_off + length
            continue

        ep = m.group(1)
        if ep == "0" * len(ep):
            pos = val_off + length
            continue  # already base game

        new_ep = "0" * len(ep)
        new_value = f"1.{new_ep}.{m.group(2)}"
        assert len(new_value) == length, (
            f"length mismatch: '{value}' ({length}) -> '{new_value}' ({len(new_value)})"
        )
        data[val_off:val_off + length] = new_value.encode("utf-8")
        print(f"  @ 0x{val_off:08x}: '{value}' -> '{new_value}'")
        patches += 1

        pos = val_off + length
    return patches


def patch_plain(src: str) -> int:
    """Patch a .Sims2Pack; write *-Mac.Sims2Pack alongside. Returns patch count."""
    stem, ext = os.path.splitext(src)
    dst = f"{stem}-Mac{ext}"
    with open(src, "rb") as f:
        data = bytearray(f.read())
    n = patch(data)
    if n == 0:
        print(f"  {os.path.basename(src)}: already base-game, skipped")
        return 0
    with open(dst, "wb") as f:
        f.write(data)
    print(f"  {os.path.basename(src)} -> {os.path.basename(dst)} ({n} patch{'es' if n != 1 else ''})")
    return n


def patch_zip(src: str) -> int:
    """Patch a .Sims2Pack inside a .zip. Writes *-Mac.Sims2Pack.zip alongside."""
    # Build the -Mac output name by inserting -Mac before .Sims2Pack.zip
    base = src
    # Strip ".zip" then ".Sims2Pack" then re-attach
    for suffix in (".zip", ".Sims2Pack", ".sims2pack"):
        if base.lower().endswith(suffix.lower()):
            base = base[: -len(suffix)]
    dst = f"{base}-Mac.Sims2Pack.zip"

    total_patches = 0
    with zipfile.ZipFile(src, "r") as zin:
        members = zin.infolist()
        # Patch in-memory and re-zip
        patched_data = {}
        for m in members:
            content = zin.read(m.filename)
            if m.filename.lower().endswith(".sims2pack"):
                buf = bytearray(content)
                n = patch(buf)
                total_patches += n
                patched_data[m.filename] = bytes(buf)
            else:
                patched_data[m.filename] = content

    if total_patches == 0:
        print(f"  {os.path.basename(src)}: already base-game, skipped")
        return 0

    with zipfile.ZipFile(dst, "w", compression=zipfile.ZIP_DEFLATED) as zout:
        for m in members:
            zout.writestr(m, patched_data[m.filename])
    print(f"  {os.path.basename(src)} -> {os.path.basename(dst)} ({total_patches} patch{'es' if total_patches != 1 else ''})")
    return total_patches


def patch_rar(src: str) -> int:
    """Extract single .Sims2Pack from a .rar via bsdtar, patch it, write
    *-Mac.Sims2Pack alongside the .rar. No re-archiving."""
    # List members
    proc = subprocess.run(
        ["bsdtar", "-tf", src], capture_output=True, text=True, check=True)
    members = [m for m in proc.stdout.splitlines() if m.lower().endswith(".sims2pack")]
    if len(members) != 1:
        print(f"  {os.path.basename(src)}: expected one .Sims2Pack inside, found {len(members)} — skipped",
              file=sys.stderr)
        return 0
    member = members[0]
    # Extract to stdout
    proc = subprocess.run(
        ["bsdtar", "-xOf", src, member], capture_output=True, check=True)
    content = bytearray(proc.stdout)
    n = patch(content)
    if n == 0:
        print(f"  {os.path.basename(src)}: already base-game, skipped")
        return 0
    base, _ = os.path.splitext(src)  # strip .rar
    # If base still ends in .Sims2Pack (e.g. "X.Sims2Pack.rar"), strip that too
    if base.lower().endswith(".sims2pack"):
        base = base[: -len(".Sims2Pack")]
    dst = f"{base}-Mac.Sims2Pack"
    with open(dst, "wb") as f:
        f.write(content)
    print(f"  {os.path.basename(src)} -> {os.path.basename(dst)} ({n} patch{'es' if n != 1 else ''})")
    return n


def process(path: str) -> int:
    """Dispatch by file kind. Returns patch count for this file."""
    lower = path.lower()
    if lower.endswith(".sims2pack.zip"):
        return patch_zip(path)
    if lower.endswith(".rar"):
        return patch_rar(path)
    if lower.endswith(".sims2pack"):
        return patch_plain(path)
    print(f"  {path}: unknown extension, skipped", file=sys.stderr)
    return 0


def main(argv):
    if len(argv) < 2:
        print(__doc__, file=sys.stderr)
        return 2
    total_files_patched = 0
    total_patches = 0
    for src in argv[1:]:
        if not os.path.exists(src):
            print(f"  {src}: not found, skipped", file=sys.stderr)
            continue
        n = process(src)
        if n > 0:
            total_files_patched += 1
            total_patches += n
    print(f"\nSummary: {total_files_patched} file(s) patched, {total_patches} property-'1' value(s) updated.")
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv))
