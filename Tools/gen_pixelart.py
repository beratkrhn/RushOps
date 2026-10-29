#!/usr/bin/env python3
"""Generates the RushOps palette and the v0.1.0 pixel art (issue #3).

Everything is drawn from code so the art is reproducible. All colors come from
PALETTE; the script refuses to write an image that uses any other color.

Output (under Assets/_Project/Art/):
  Palettes/RushOps_Palette.png        one pixel per palette color, in order
  Sprites/Player/Player.png           16x16, facing right (+X)
  Sprites/Enemies/Walker_Walk_0..3.png 16x16, 4-frame walk cycle, facing right
  Sprites/Projectiles/Bullet.png      8x8
  Sprites/Effects/MuzzleFlash.png     8x8
  Tiles/Floor_0..2.png                16x16 floor variants
  Tiles/Wall.png                      16x16

Usage:
  python3 Tools/gen_pixelart.py [--preview PATH]

--preview writes an upscaled contact sheet (player and walker on the floor)
for a visual check before committing. It is not part of the project.
"""
import argparse
import random
from pathlib import Path

from PIL import Image

ROOT = Path(__file__).resolve().parent.parent
ART_DIR = ROOT / "Assets/_Project/Art"

# Fixed palette (ADR 0002). Order matters: it is the order in the palette PNG.
PALETTE = {
    "outline": (13, 11, 20),
    "night": (31, 29, 43),
    "slate_0": (46, 44, 61),
    "slate_1": (59, 58, 79),
    "slate_2": (77, 76, 99),
    "slate_3": (107, 106, 128),
    "slate_4": (143, 142, 163),
    "slate_5": (199, 198, 214),
    "white": (244, 241, 232),
    "blue_0": (30, 63, 102),
    "blue_1": (47, 110, 168),
    "blue_2": (95, 176, 224),
    "skin_0": (138, 90, 60),
    "skin_1": (211, 154, 106),
    "green_0": (44, 74, 42),
    "green_1": (79, 122, 58),
    "green_2": (141, 178, 84),
    "brown_0": (74, 52, 44),
    "brown_1": (125, 94, 70),
    "red_0": (122, 31, 43),
    "red_1": (200, 50, 60),
    "orange": (232, 134, 47),
    "yellow": (246, 208, 77),
    "glow": (255, 246, 194),
}

CLEAR = None
TILE = 16


class Canvas:
    """Indexed drawing surface: each pixel holds a palette key or None."""

    def __init__(self, width: int, height: int, fill: str | None = CLEAR):
        self.width = width
        self.height = height
        self.px = [[fill] * width for _ in range(height)]

    def inside(self, x: int, y: int) -> bool:
        return 0 <= x < self.width and 0 <= y < self.height

    def set(self, x: int, y: int, color: str | None) -> None:
        if self.inside(x, y):
            self.px[y][x] = color

    def get(self, x: int, y: int) -> str | None:
        return self.px[y][x] if self.inside(x, y) else None

    def rect(self, x0: int, y0: int, x1: int, y1: int, color: str) -> None:
        """Fills the inclusive rectangle x0..x1, y0..y1."""
        for y in range(y0, y1 + 1):
            for x in range(x0, x1 + 1):
                self.set(x, y, color)

    def ellipse(self, cx: float, cy: float, rx: float, ry: float, color: str, shade: str | None = None) -> None:
        """Fills an ellipse; pixels near the rim get `shade` if given."""
        for y in range(self.height):
            for x in range(self.width):
                d = ((x + 0.5 - cx) / rx) ** 2 + ((y + 0.5 - cy) / ry) ** 2
                if d <= 1.0:
                    self.set(x, y, shade if shade and d > 0.55 else color)

    def outline(self, color: str = "outline") -> None:
        """Adds a 1 px outline around all filled pixels (4-neighbourhood)."""
        edge = []
        for y in range(self.height):
            for x in range(self.width):
                if self.px[y][x] is not None:
                    continue
                if any(self.get(x + dx, y + dy) not in (None, color)
                       for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1))):
                    edge.append((x, y))
        for x, y in edge:
            self.px[y][x] = color

    def to_image(self) -> Image.Image:
        img = Image.new("RGBA", (self.width, self.height), (0, 0, 0, 0))
        for y in range(self.height):
            for x in range(self.width):
                key = self.px[y][x]
                if key is not None:
                    img.putpixel((x, y), PALETTE[key] + (255,))
        return img


# --- Characters --------------------------------------------------------------
# Top-down view, facing right (+X). Image y grows downwards, so the character's
# right hand side is the lower half of the image.

def player() -> Canvas:
    c = Canvas(TILE, TILE)
    # Rifle held on the right hand side, barrel pointing right.
    c.rect(8, 9, 11, 10, "night")
    c.rect(12, 9, 14, 9, "slate_3")
    c.set(9, 9, "slate_2")
    # Shoulders/torso (vest), wide across the facing direction.
    c.ellipse(6.0, 8.0, 2.6, 5.0, "blue_1", shade="blue_0")
    # Sleeves reaching to the rifle: front hand on the barrel, rear hand at the grip.
    for x, y in [(8, 4), (9, 5), (10, 6), (11, 7)]:
        c.set(x, y, "blue_1")
    c.set(12, 8, "skin_1")
    c.set(8, 11, "blue_1")
    c.set(9, 11, "skin_1")
    # Helmet.
    c.ellipse(7.0, 8.0, 2.0, 2.0, "slate_3", shade="slate_2")
    c.set(6, 7, "slate_4")
    c.outline()
    return c


def walker_frame(frame: int) -> Canvas:
    """One frame of the walker's 4-frame shuffle: arms reach in turn, body sways."""
    c = Canvas(TILE, TILE)
    # Per frame: (upper arm reach, lower arm reach, stepping foot row, body sway in y).
    reach_up, reach_down, foot_row, sway = [
        (13, 11, 6, 0),
        (12, 12, None, 1),
        (11, 13, 10, 0),
        (12, 12, None, -1),
    ][frame]
    cy = 8.0 + sway
    if foot_row is not None:
        c.set(10, foot_row, "brown_0")
    # Arms stretched forward, the classic shamble.
    c.rect(8, 5, reach_up - 1, 5, "green_1")
    c.set(reach_up, 5, "green_2")
    c.rect(8, 11, reach_down - 1, 11, "green_1")
    c.set(reach_down, 11, "green_2")
    # Torn shirt.
    c.ellipse(6.0, cy, 2.5, 4.6, "brown_1", shade="brown_0")
    c.set(5, round(cy) - 3, "red_0")
    c.set(4, round(cy) + 2, "red_0")
    # Bald green head, pushed forward, with glowing eyes on the facing side.
    c.ellipse(7.5, cy, 2.1, 2.1, "green_1", shade="green_0")
    c.set(6, round(cy) - 1, "green_2")
    c.set(8, round(cy) - 2, "red_1")
    c.set(8, round(cy) + 1, "red_1")
    c.outline()
    return c


# --- Projectile and effects --------------------------------------------------

def bullet() -> Canvas:
    c = Canvas(8, 8)
    c.rect(2, 3, 2, 4, "orange")
    c.rect(3, 3, 4, 4, "yellow")
    c.rect(5, 3, 5, 4, "glow")
    return c


def muzzle_flash() -> Canvas:
    c = Canvas(8, 8)
    for x, y in [(1, 3), (1, 4), (3, 1), (3, 6), (7, 3), (7, 4), (5, 2), (5, 5)]:
        c.set(x, y, "orange")
    c.rect(2, 2, 6, 5, "yellow")
    c.set(2, 2, "orange")
    c.set(2, 5, "orange")
    c.rect(2, 3, 5, 4, "glow")
    return c


# --- Tiles ------------------------------------------------------------------

def floor(variant: int) -> Canvas:
    """Worn concrete. Variant 0 is plain, 1 has a crack, 2 has a stain."""
    c = Canvas(TILE, TILE, fill="slate_1")
    rng = random.Random(1000 + variant)
    for _ in range(10):
        c.set(rng.randrange(TILE), rng.randrange(TILE), "slate_0")
    for _ in range(4):
        c.set(rng.randrange(TILE), rng.randrange(TILE), "slate_2")
    if variant == 1:
        for x, y in [(3, 4), (4, 5), (5, 5), (6, 6), (7, 7), (7, 8), (8, 9), (9, 9), (10, 10), (11, 12)]:
            c.set(x, y, "night")
        c.set(8, 8, "slate_0")
    elif variant == 2:
        c.ellipse(9.0, 7.0, 3.5, 2.5, "slate_0")
        c.set(6, 10, "slate_0")
        c.set(12, 4, "slate_0")
    return c


def wall() -> Canvas:
    """Top of a stone wall: staggered blocks with light from above-left."""
    c = Canvas(TILE, TILE, fill="slate_3")
    for row in range(4):
        y0 = row * 4
        offset = 0 if row % 2 == 0 else 4
        c.rect(0, y0 + 3, TILE - 1, y0 + 3, "slate_2")
        for seam in (offset, offset + 8):
            c.rect(seam % TILE, y0, seam % TILE, y0 + 2, "slate_2")
        for x in range(TILE):
            if c.get(x, y0) == "slate_3":
                c.set(x, y0, "slate_4")
    for x, y in [(5, 1), (13, 9), (2, 13)]:
        c.set(x, y, "slate_2")
    return c


# --- Output ------------------------------------------------------------------

def palette_image() -> Image.Image:
    img = Image.new("RGBA", (len(PALETTE), 1))
    for i, rgb in enumerate(PALETTE.values()):
        img.putpixel((i, 0), rgb + (255,))
    return img


def assert_in_palette(img: Image.Image, name: str) -> None:
    allowed = set(PALETTE.values())
    for x in range(img.width):
        for y in range(img.height):
            r, g, b, a = img.getpixel((x, y))
            if a == 0:
                continue
            if a != 255 or (r, g, b) not in allowed:
                raise ValueError(f"{name}: color {(r, g, b, a)} at {(x, y)} is not in the palette")



def outputs() -> dict[str, Image.Image]:
    files = {
        "Palettes/RushOps_Palette.png": palette_image(),
        "Sprites/Player/Player.png": player().to_image(),
        "Sprites/Projectiles/Bullet.png": bullet().to_image(),
        "Sprites/Effects/MuzzleFlash.png": muzzle_flash().to_image(),
        "Tiles/Wall.png": wall().to_image(),
    }
    for frame in range(4):
        files[f"Sprites/Enemies/Walker_Walk_{frame}.png"] = walker_frame(frame).to_image()
    for variant in range(3):
        files[f"Tiles/Floor_{variant}.png"] = floor(variant).to_image()
    return files


def preview(files: dict[str, Image.Image], path: Path, scale: int = 8) -> None:
    """Floor patch with player, walker frames, bullet and flash, plus the palette."""
    floors = [files[f"Tiles/Floor_{v}.png"] for v in range(3)]
    sheet = Image.new("RGBA", (8 * TILE, 4 * TILE), PALETTE["night"] + (255,))
    for ty in range(3):
        for tx in range(8):
            tile = files["Tiles/Wall.png"] if ty == 0 and tx >= 6 else floors[(tx * 7 + ty * 3) % 3]
            sheet.paste(tile, (tx * TILE, ty * TILE))
    sheet.alpha_composite(files["Sprites/Player/Player.png"], (TILE, TILE))
    sheet.alpha_composite(files["Sprites/Effects/MuzzleFlash.png"], (2 * TILE, TILE + 5))
    sheet.alpha_composite(files["Sprites/Projectiles/Bullet.png"], (2 * TILE + 8, TILE + 5))
    for frame in range(4):
        sheet.alpha_composite(files[f"Sprites/Enemies/Walker_Walk_{frame}.png"], ((frame + 4) * TILE, 2 * TILE))
    sheet.alpha_composite(files[f"Sprites/Enemies/Walker_Walk_0.png"], (4 * TILE, TILE))
    pal = files["Palettes/RushOps_Palette.png"].resize((len(PALETTE) * 4, 4), Image.NEAREST)
    sheet.paste(pal, (4, 3 * TILE + 6))
    sheet.resize((sheet.width * scale, sheet.height * scale), Image.NEAREST).save(path)


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--preview", type=Path, help="write an upscaled contact sheet to this path")
    args = parser.parse_args()

    files = outputs()
    for rel, img in files.items():
        assert_in_palette(img, rel)
        out = ART_DIR / rel
        out.parent.mkdir(parents=True, exist_ok=True)
        img.save(out)
        print(f"Wrote {out.relative_to(ROOT)}")

    if args.preview:
        preview(files, args.preview)
        print(f"Wrote preview {args.preview}")


if __name__ == "__main__":
    main()
