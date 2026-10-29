#!/usr/bin/env python3
"""Imports a character export (8 directions) into the project (ADR 0006).

Reads the export's metadata.json, copies every rotation and animation frame to
Assets/_Project/Art/<dest>/ using the project naming scheme
<Name>_<State>_<DIR>[_<Frame>].png and checks the rules: 64x64 canvas,
no semi-transparent pixels, only palette colors (see gen_pixelart.PALETTE).

Usage:
  python3 Tools/import_character.py <export-folder> <Name> [--dest Sprites/Player]

Example:
  python3 Tools/import_character.py Muslim_with_Thobe-Idle Player
"""
import argparse
import json
import shutil
import sys
from pathlib import Path

from PIL import Image

sys.path.insert(0, str(Path(__file__).resolve().parent))
from gen_pixelart import ART_DIR, CHARACTER, PALETTE, ROOT, assert_in_palette  # noqa: E402

DIRECTION_CODES = {
    "east": "E",
    "north-east": "NE",
    "north": "N",
    "north-west": "NW",
    "west": "W",
    "south-west": "SW",
    "south": "S",
    "south-east": "SE",
}


def check(path: Path) -> None:
    img = Image.open(path).convert("RGBA")
    if img.size != (CHARACTER, CHARACTER):
        raise ValueError(f"{path}: expected {CHARACTER}x{CHARACTER}, got {img.size[0]}x{img.size[1]}")
    assert_in_palette(img, str(path))


def frames(state: dict, export: Path) -> dict[str, Path]:
    """Maps target file stems (State_DIR[_Frame]) to source files."""
    name = state["character"]["name"]
    result = {}
    for direction, rel in state["frames"].get("rotations", {}).items():
        result[f"{name}_{DIRECTION_CODES[direction]}"] = export / rel
    for animation, directions in state["frames"].get("animations", {}).items():
        for direction, rels in directions.items():
            for i, rel in enumerate(rels):
                result[f"{animation}_{DIRECTION_CODES[direction]}_{i}"] = export / rel
    return result


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("export", type=Path, help="folder with metadata.json")
    parser.add_argument("name", help="character name used in file names, e.g. Player")
    parser.add_argument("--dest", default="Sprites/Player", help="target folder below Assets/_Project/Art")
    args = parser.parse_args()

    metadata = json.loads((args.export / "metadata.json").read_text())
    dest = ART_DIR / args.dest
    dest.mkdir(parents=True, exist_ok=True)

    for state in metadata["states"]:
        for stem, source in frames(state, args.export).items():
            check(source)
            target = dest / f"{args.name}_{stem}.png"
            shutil.copyfile(source, target)
            print(f"Wrote {target.relative_to(ROOT)}")


if __name__ == "__main__":
    main()
