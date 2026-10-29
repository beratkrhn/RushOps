#!/usr/bin/env python3
"""Generates temporary placeholder sprites for the test arena (issue #2).

Produces a floor tile, a wall tile and a debug target marker, each 16x16 px.
These are stand-ins until the real palette and art arrive (issue #3).

Usage: python3 Tools/gen_placeholder_arena.py
"""
from pathlib import Path

from PIL import Image

ROOT = Path(__file__).resolve().parent.parent
TILES_DIR = ROOT / "Assets/_Project/Art/Tiles/Placeholder"
SPRITES_DIR = ROOT / "Assets/_Project/Art/Sprites/Placeholder"

SIZE = 16

FLOOR = (52, 56, 64, 255)
FLOOR_DOT = (64, 69, 78, 255)
WALL = (120, 112, 104, 255)
WALL_EDGE = (84, 78, 72, 255)
WALL_LIGHT = (146, 138, 128, 255)
TARGET = (232, 196, 72, 255)
TARGET_EDGE = (150, 110, 30, 255)
CLEAR = (0, 0, 0, 0)


def floor() -> Image.Image:
    img = Image.new("RGBA", (SIZE, SIZE), FLOOR)
    # Sparse dots make camera movement and pixel jitter visible.
    for x, y in [(3, 4), (11, 2), (7, 9), (13, 12), (2, 13)]:
        img.putpixel((x, y), FLOOR_DOT)
    return img


def wall() -> Image.Image:
    img = Image.new("RGBA", (SIZE, SIZE), WALL)
    for i in range(SIZE):
        img.putpixel((i, 0), WALL_LIGHT)
        img.putpixel((0, i), WALL_LIGHT)
        img.putpixel((i, SIZE - 1), WALL_EDGE)
        img.putpixel((SIZE - 1, i), WALL_EDGE)
    return img


def target() -> Image.Image:
    """Round marker with a nose pointing right (+X), as required by ADR 0002."""
    img = Image.new("RGBA", (SIZE, SIZE), CLEAR)
    cx, cy, r = 7.5, 7.5, 6.0
    for y in range(SIZE):
        for x in range(SIZE):
            d = ((x - cx) ** 2 + (y - cy) ** 2) ** 0.5
            if d <= r - 1:
                img.putpixel((x, y), TARGET)
            elif d <= r:
                img.putpixel((x, y), TARGET_EDGE)
    for x in range(12, 16):
        img.putpixel((x, 7), TARGET_EDGE)
        img.putpixel((x, 8), TARGET_EDGE)
    return img


def main() -> None:
    TILES_DIR.mkdir(parents=True, exist_ok=True)
    SPRITES_DIR.mkdir(parents=True, exist_ok=True)
    floor().save(TILES_DIR / "Placeholder_Floor.png")
    wall().save(TILES_DIR / "Placeholder_Wall.png")
    target().save(SPRITES_DIR / "Placeholder_Target.png")
    print(f"Wrote placeholders to {TILES_DIR} and {SPRITES_DIR}")


if __name__ == "__main__":
    main()
