#!/usr/bin/env python3
"""Generates the RushOps palette and the scripted pixel art (ADR 0006).

Everything is drawn from code so the art is reproducible. All colors come from
PALETTE; the script refuses to write an image that uses any other color.
The player character is not drawn here: it comes from the product owner's
character tool and is imported with Tools/import_character.py.

Output (under Assets/_Project/Art/):
  Palettes/RushOps_Palette.png             one pixel per palette color, in order
  Sprites/Enemies/Walker_Walk_<DIR>_<F>.png 64x64, 8 directions x 4 frames
  Sprites/Projectiles/Bullet.png           16x16, flying right (+X)
  Sprites/Effects/MuzzleFlash.png          16x16, pointing right (+X)
  Sprites/Effects/SpawnMarker.png          32x32 ground mark shown before an enemy spawns
  Sprites/UI/Heart_Full.png                12x11 HUD heart
  Sprites/UI/Heart_Empty.png               12x11 HUD heart, lost
  Sprites/UI/Pause_Icon.png                12x12 pause symbol
  Fonts/PixelFont.png + PixelFont.json     glyph atlas and metrics of the pixel font (Tools/pixel_font.py)
  Sprites/UI/Stick_Base.png                32x32 ring of the virtual stick
  Sprites/UI/Stick_Knob.png                16x16 knob of the virtual stick
  Sprites/UI/Dash_Button.png               24x24 dash button with icon
  Sprites/UI/Dash_Cooldown.png             24x24 disc covering the button during the cooldown
  Tiles/Floor_0..2.png                     32x32 floor variants
  Tiles/Wall_Top.png                       32x32 wall seen from above
  Tiles/Wall_Front.png                     32x32 wall top plus front face

Usage:
  python3 Tools/gen_pixelart.py [--preview PATH]

--preview writes an upscaled contact sheet (characters on the floor next to
walls) for a visual check before committing. It is not part of the project.
"""
import argparse
import json
import math
import random
from pathlib import Path

from PIL import Image

import pixel_font

ROOT = Path(__file__).resolve().parent.parent
ART_DIR = ROOT / "Assets/_Project/Art"

# Fixed palette (ADR 0006, at most 32 colors). Order matters: it is the order in the palette PNG.
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
    # Colors of the player character (issue #58).
    "navy_0": (13, 43, 69),
    "navy_1": (32, 60, 86),
    "dusk": (84, 78, 104),
    "mauve": (141, 105, 122),
    "clay": (208, 129, 89),
    "apricot": (255, 170, 94),
    "peach": (255, 212, 163),
    "cream": (255, 236, 214),
}

TILE = 32
CHARACTER = 64
# Character feet rest this many pixels above the bottom edge (ADR 0006).
FOOT_MARGIN = 5

# Directions in counter-clockwise order from +X, matching RushOps.Characters.Direction8.
DIRECTIONS = ["E", "NE", "N", "NW", "W", "SW", "S", "SE"]


class Canvas:
    """Indexed drawing surface: each pixel holds a palette key or None."""

    def __init__(self, width: int, height: int, fill: str | None = None):
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

    def ellipse(self, cx: float, cy: float, rx: float, ry: float, color: str | None) -> None:
        """Fills an ellipse; None clears it, e.g. to cut a ring."""
        for y in range(self.height):
            for x in range(self.width):
                if ((x + 0.5 - cx) / rx) ** 2 + ((y + 0.5 - cy) / ry) ** 2 <= 1.0:
                    self.set(x, y, color)

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


# --- Characters in 3/4 view -----------------------------------------------------
# Body parts are spheres and capsules in a small 3D space: f = forward, s = to the
# character's right, z = up, all in pixels. They are turned to the facing angle,
# projected obliquely (ground depth is squashed to half height) and painted far to
# near, so all 8 directions come out of the same model.

# Material ramps: (shadow, base, light).
RAMP = {
    "skin": ("green_0", "green_1", "green_2"),
    "shirt": ("brown_0", "brown_1", "mauve"),
    "pants": ("navy_0", "navy_1", "dusk"),
    "shoe": ("night", "night", "slate_0"),
}


class Model:
    """Collects sphere samples and rasterises them for one facing angle."""

    def __init__(self, angle_deg: float):
        a = math.radians(angle_deg)
        self.forward = (math.cos(a), math.sin(a))
        self.right = (math.sin(a), -math.cos(a))
        self.samples = []

    def ground(self, f: float, s: float) -> tuple[float, float]:
        """Local (forward, right) offset to world ground (east, north)."""
        return (f * self.forward[0] + s * self.right[0], f * self.forward[1] + s * self.right[1])

    def project(self, f: float, s: float, z: float) -> tuple[float, float, float]:
        """Local point to canvas (x, y) plus its depth (north, larger = farther)."""
        east, north = self.ground(f, s)
        return CHARACTER / 2 + east, CHARACTER - FOOT_MARGIN - 0.5 - z - north * 0.5, north

    def sphere(self, f: float, s: float, z: float, r: float, material: str) -> None:
        self.samples.append((*self.project(f, s, z), z, r, material))

    def capsule(self, a: tuple, b: tuple, r: float, material: str) -> None:
        """Capsule between local points a and b, each (f, s, z)."""
        steps = max(1, int(math.dist(a, b) * 2))
        for i in range(steps + 1):
            t = i / steps
            self.sphere(*(a[k] + (b[k] - a[k]) * t for k in range(3)), r, material)

    def paint(self, canvas: Canvas) -> None:
        # Far (north) first; within the same depth, lower parts first.
        for sx, sy, north, z, r, material in sorted(self.samples, key=lambda p: (-p[2], p[3])):
            shadow, base, light = RAMP[material]
            for y in range(int(sy - r) - 1, int(sy + r) + 2):
                for x in range(int(sx - r) - 1, int(sx + r) + 2):
                    dx, dy = x + 0.5 - sx, y + 0.5 - sy
                    if dx * dx + dy * dy > r * r:
                        continue
                    # Light from above: lower rim in shadow, upper left rim lit.
                    if dy > r * 0.45:
                        color = shadow
                    elif dy < -r * 0.55 and dx < r * 0.2:
                        color = light
                    else:
                        color = base
                    canvas.set(x, y, color)


def walker_frame(direction: int, frame: int) -> Canvas:
    """Walker in one of 8 directions, frame 0..3 of a slow shamble."""
    m = Model(direction * 45.0)
    phase = frame / 4 * 2 * math.pi
    stride = 4.0 * math.sin(phase)
    bob = 1.0 if frame % 2 == 1 else 0.0
    sway = math.sin(phase)

    # Legs and shoes.
    for side, step in ((-3.5, stride), (3.5, -stride)):
        m.capsule((0, side, 18 - bob), (step, side, 4), 3.8, "pants")
        m.sphere(step + 1.0, side, 2.5, 3.0, "shoe")
    # Torso, slightly hunched forward.
    for side in (-3.5, 3.5):
        m.capsule((0, side + sway * 0.5, 19 - bob), (1.5, side + sway, 34 - bob), 6.0, "shirt")
    # Arms reaching forward, swinging against the legs.
    for side, reach in ((-8.0, -stride * 0.5), (8.0, stride * 0.5)):
        shoulder = (1.5, side + sway, 33 - bob)
        elbow = (7.0 + reach * 0.5, side * 0.85 + sway, 31 - bob)
        hand = (14.0 + reach, side * 0.75 + sway, 30 - bob)
        m.capsule(shoulder, elbow, 2.9, "shirt")
        m.capsule(elbow, hand, 2.5, "skin")
    # Head, hanging forward.
    head = (3.0, sway, 42 - bob)
    m.sphere(*head, 6.8, "skin")

    c = Canvas(CHARACTER, CHARACTER)
    m.paint(c)

    # Glowing eyes, only where they face the viewer.
    _, _, head_north = m.project(*head)
    for s in (-2.4, 2.4):
        x, y, north = m.project(head[0] + 5.4, head[1] + s, head[2] + 0.5)
        if north <= head_north + 0.5:
            c.set(int(x), int(y), "red_1")
    # Blood stain on the chest when the chest is visible.
    x, y, north = m.project(7.0, sway + 1.5, 27 - bob)
    if north <= 0.5:
        c.set(int(x), int(y), "red_0")
        c.set(int(x) + 1, int(y) + 1, "red_0")
    c.outline("navy_0")
    return c


# --- Projectile and effects --------------------------------------------------------

def bullet() -> Canvas:
    c = Canvas(16, 16)
    c.rect(3, 7, 5, 8, "orange")
    c.rect(6, 7, 9, 8, "yellow")
    c.rect(10, 7, 11, 8, "glow")
    c.set(12, 7, "glow")
    c.set(12, 8, "yellow")
    return c


def muzzle_flash() -> Canvas:
    c = Canvas(16, 16)
    c.ellipse(7.0, 8.0, 5.5, 4.0, "orange")
    for x, y in [(12, 7), (13, 8), (14, 8), (13, 7), (6, 3), (6, 12), (9, 4), (9, 11)]:
        c.set(x, y, "orange")
    c.ellipse(7.5, 8.0, 4.0, 2.6, "yellow")
    c.ellipse(7.0, 8.0, 2.4, 1.5, "glow")
    return c


def spawn_marker() -> Canvas:
    """Flat warning ring on the floor, squashed for the oblique view, with four inward ticks."""
    c = Canvas(32, 32)
    c.ellipse(16.0, 16.0, 13.0, 8.0, "red_0")
    c.ellipse(16.0, 16.0, 11.0, 6.5, "red_1")
    c.ellipse(16.0, 16.0, 9.0, 5.0, None)
    for x, y in [(16, 9), (16, 22), (3, 16), (28, 16)]:
        c.set(x, y, "orange")
    for x, y in [(15, 10), (17, 10), (15, 21), (17, 21)]:
        c.set(x, y, "red_1")
    c.ellipse(16.0, 16.0, 2.0, 1.2, "red_0")
    return c


# --- Touch controls ----------------------------------------------------------

def stick_base() -> Canvas:
    """Hollow ring: the area the knob moves in."""
    c = Canvas(32, 32)
    c.ellipse(16.0, 16.0, 15.0, 15.0, "slate_3")
    c.ellipse(16.0, 16.0, 13.0, 13.0, "slate_1")
    c.ellipse(16.0, 16.0, 11.0, 11.0, "slate_0")
    c.ellipse(16.0, 16.0, 3.0, 3.0, "slate_2")
    c.outline()
    return c


def stick_knob() -> Canvas:
    c = Canvas(16, 16)
    c.ellipse(8.0, 8.0, 7.0, 7.0, "slate_3")
    c.ellipse(8.0, 7.5, 5.5, 5.5, "slate_4")
    c.ellipse(7.0, 6.0, 2.5, 2.0, "slate_5")
    c.outline()
    return c


def dash_button() -> Canvas:
    """Round button with a double chevron pointing right."""
    c = Canvas(24, 24)
    c.ellipse(12.0, 12.0, 11.0, 11.0, "blue_0")
    c.ellipse(12.0, 11.5, 9.5, 9.5, "blue_1")
    # Two 2 px thick chevrons (">>"), 7 rows tall.
    for x0 in (6, 11):
        for i in range(7):
            x = x0 + 3 - abs(i - 3)
            c.set(x, 8 + i, "white")
            c.set(x + 1, 8 + i, "white")
    c.outline()
    return c


HEART_SHAPE = [
    ".##...##..",
    "####.####.",
    "#########.",
    "#########.",
    ".#######..",
    "..#####...",
    "...###....",
    "....#.....",
]


def heart(full: bool) -> Canvas:
    """HUD heart: red with a highlight when full, a dark hollow when lost."""
    c = Canvas(12, 11)
    for y, row in enumerate(HEART_SHAPE):
        for x, ch in enumerate(row):
            if ch == "#":
                c.set(x + 1, y + 1, "red_1" if full else "night")
    if full:
        for x, y in [(2, 2), (2, 3), (3, 2)]:
            c.set(x, y, "white")
        for x, y in [(6, 6), (7, 5), (8, 4), (9, 3), (9, 4)]:
            c.set(x, y, "red_0")
    c.outline()
    return c


def pause_icon() -> Canvas:
    c = Canvas(12, 12)
    c.rect(2, 1, 4, 10, "white")
    c.rect(7, 1, 9, 10, "white")
    c.outline()
    return c


def font_atlas() -> tuple[Image.Image, dict]:
    """Glyph atlas of the pixel font in white, and the metrics Unity needs to build the font asset."""
    cell_w, cell_h, columns = 8, 10, 16
    chars = list(pixel_font.GLYPHS)
    rows = (len(chars) + columns - 1) // columns
    c = Canvas(cell_w * columns, cell_h * rows)
    glyphs = []
    for i, ch in enumerate(chars):
        _, rows_ = pixel_font.trimmed(pixel_font.GLYPHS[ch])
        width = len(rows_[0])
        x0, y0 = (i % columns) * cell_w, (i // columns) * cell_h
        for y, row in enumerate(rows_):
            for x, px in enumerate(row):
                if px == "#":
                    c.set(x0 + x, y0 + y, "white")
        glyphs.append({
            "char": ord(ch), "x": x0, "y": y0, "width": width, "height": len(rows_),
            # Bottom of the glyph relative to the baseline; descenders hang below it.
            "minY": pixel_font.CAP_HEIGHT - len(rows_), "advance": width + 1,
        })
    metrics = {
        "atlasWidth": c.width, "atlasHeight": c.height, "lineHeight": pixel_font.LINE_HEIGHT,
        "ascent": pixel_font.CAP_HEIGHT, "spaceAdvance": pixel_font.SPACE_ADVANCE, "glyphs": glyphs,
    }
    return c.to_image(), metrics


def dash_cooldown() -> Canvas:
    """Dark disc of the button size, filled radially to show the remaining cooldown."""
    c = Canvas(24, 24)
    c.ellipse(12.0, 12.0, 12.0, 12.0, "night")
    return c


# --- Tiles ---------------------------------------------------------------------

def floor(variant: int) -> Canvas:
    """Worn stone floor. Variant 0 is plain, 1 has a crack, 2 has a stain."""
    c = Canvas(TILE, TILE, fill="slate_3")
    rng = random.Random(2000 + variant)
    # Faint slab seams on the tile border keep the grid readable.
    for i in range(TILE):
        if rng.random() < 0.8:
            c.set(i, TILE - 1, "slate_2")
        if rng.random() < 0.8:
            c.set(TILE - 1, i, "slate_2")
    for _ in range(26):
        c.set(rng.randrange(TILE - 1), rng.randrange(TILE - 1), "slate_2")
    for _ in range(10):
        c.set(rng.randrange(TILE - 1), rng.randrange(TILE - 1), "slate_4")
    if variant == 1:
        x, y = 6, 7
        for step in range(18):
            c.set(x, y, "slate_1")
            x += 1
            y += 1 if step % 3 != 2 else 0
        c.set(15, 10, "slate_2")
        c.set(20, 18, "slate_2")
    elif variant == 2:
        # Irregular stain: a short random walk of darker pixels.
        x, y = 16, 14
        for _ in range(60):
            c.set(x, y, "slate_2")
            x = min(max(x + rng.choice((-1, 0, 1)), 9), 24)
            y = min(max(y + rng.choice((-1, 0, 1)), 9), 21)
    return c


def wall_top_face(c: Canvas, y0: int, y1: int) -> None:
    """Top of the wall between rows y0..y1: pale stone with block seams."""
    c.rect(0, y0, TILE - 1, y1, "slate_4")
    rng = random.Random(3000 + y0)
    for _ in range(8):
        c.set(rng.randrange(TILE), rng.randrange(y0, y1 + 1), "slate_3")
    for x in (0, 16):
        c.rect(x, y0, x, y1, "slate_3")


def wall_top() -> Canvas:
    c = Canvas(TILE, TILE)
    wall_top_face(c, 0, TILE - 1)
    c.rect(0, TILE // 2, TILE - 1, TILE // 2, "slate_3")
    return c


def wall_front() -> Canvas:
    """Wall with nothing to the south: top face above, brick front face below."""
    c = Canvas(TILE, TILE)
    top = 11
    wall_top_face(c, 0, top - 1)
    c.rect(0, top - 1, TILE - 1, top - 1, "slate_5")
    c.rect(0, top, TILE - 1, TILE - 1, "slate_2")
    for row, y in enumerate(range(top, TILE, 5)):
        c.rect(0, y + 4, TILE - 1, y + 4, "slate_1")
        offset = 0 if row % 2 == 0 else 8
        for x in range(offset, TILE, 16):
            c.rect(x, y, x, min(y + 3, TILE - 1), "slate_1")
        c.rect(0, y, TILE - 1, y, "dusk")
    # Darker foot where the wall meets the floor.
    c.rect(0, TILE - 2, TILE - 1, TILE - 1, "slate_0")
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
        "Sprites/Projectiles/Bullet.png": bullet().to_image(),
        "Sprites/Effects/MuzzleFlash.png": muzzle_flash().to_image(),
        "Tiles/Wall_Top.png": wall_top().to_image(),
        "Tiles/Wall_Front.png": wall_front().to_image(),
        "Sprites/Effects/SpawnMarker.png": spawn_marker().to_image(),
        "Sprites/UI/Heart_Full.png": heart(True).to_image(),
        "Sprites/UI/Heart_Empty.png": heart(False).to_image(),
        "Sprites/UI/Pause_Icon.png": pause_icon().to_image(),
        "Fonts/PixelFont.png": font_atlas()[0],
        "Sprites/UI/Stick_Base.png": stick_base().to_image(),
        "Sprites/UI/Stick_Knob.png": stick_knob().to_image(),
        "Sprites/UI/Dash_Button.png": dash_button().to_image(),
        "Sprites/UI/Dash_Cooldown.png": dash_cooldown().to_image(),
    }
    for d, name in enumerate(DIRECTIONS):
        for frame in range(4):
            files[f"Sprites/Enemies/Walker_Walk_{name}_{frame}.png"] = walker_frame(d, frame).to_image()
    for variant in range(3):
        files[f"Tiles/Floor_{variant}.png"] = floor(variant).to_image()
    return files


def preview(files: dict[str, Image.Image], path: Path, scale: int = 3) -> None:
    """Floor patch with a wall block, the player (if imported) and walkers in all directions."""
    cols, rows = 10, 6
    sheet = Image.new("RGBA", (cols * TILE, rows * TILE), PALETTE["night"] + (255,))
    for ty in range(rows):
        for tx in range(cols):
            if ty == 1 and 2 <= tx <= 6:
                tile = files["Tiles/Wall_Top.png"]
            elif ty == 2 and 2 <= tx <= 6:
                tile = files["Tiles/Wall_Front.png"]
            else:
                tile = files[f"Tiles/Floor_{(tx * 7 + ty * 3) % 5 % 3}.png"]
            sheet.paste(tile, (tx * TILE, ty * TILE))

    def put(img: Image.Image, cell_x: int, cell_y: int) -> None:
        # Feet on the lower part of the cell, as with a bottom-center pivot.
        x = cell_x * TILE + TILE // 2 - img.width // 2
        y = (cell_y + 1) * TILE - img.height + FOOT_MARGIN - 4
        sheet.alpha_composite(img, (x, y))

    for i, name in enumerate(DIRECTIONS):
        put(files[f"Sprites/Enemies/Walker_Walk_{name}_{i % 4}.png"], 1 + i, 5)
    player_dir = ART_DIR / "Sprites/Player"
    if (player_dir / "Player_Idle_SE.png").exists():
        put(Image.open(player_dir / "Player_Idle_N.png").convert("RGBA"), 4, 0)
        put(Image.open(player_dir / "Player_Idle_SE.png").convert("RGBA"), 1, 3)
    put(files["Sprites/Enemies/Walker_Walk_SW_0.png"], 3, 3)
    sheet.alpha_composite(files["Sprites/Effects/MuzzleFlash.png"], (6 * TILE, 3 * TILE + 8))
    sheet.alpha_composite(files["Sprites/Projectiles/Bullet.png"], (6 * TILE + 20, 3 * TILE + 8))
    pal = files["Palettes/RushOps_Palette.png"].resize((len(PALETTE) * 6, 6), Image.NEAREST)
    sheet.paste(pal, (4, 4))
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

    metrics_path = ART_DIR / "Fonts/PixelFont.json"
    metrics_path.write_text(json.dumps(font_atlas()[1], indent=1) + "\n")
    print(f"Wrote {metrics_path.relative_to(ROOT)}")

    if args.preview:
        preview(files, args.preview)
        print(f"Wrote preview {args.preview}")


if __name__ == "__main__":
    main()
