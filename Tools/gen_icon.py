#!/usr/bin/env python3
"""App icon for RushOps (issue #191, decision R7).

The player with his pistol in front, a walker one shade darker behind him with glowing red eyes, under a pale full
moon that lifts the player's dark outfit off the night sky. Drawn on a 64x64 canvas from the game's own sprites and palette, then scaled up to
1024x1024 without smoothing: opaque, square corners (iOS rounds them).

    python3 Tools/gen_icon.py                      # writes Assets/_Project/Art/Icon/AppIcon.png
    python3 Tools/gen_icon.py --preview docs/art   # also writes icon_preview.png with 60, 120 and 180 px

Fails if a pixel lies outside the palette (ADR 0006). Afterwards run „RushOps → Build → App-Icon setzen“ in Unity.
"""

import argparse
from pathlib import Path

from PIL import Image

from gen_pixelart import PALETTE

ROOT = Path(__file__).resolve().parent.parent
SPRITES = ROOT / "Assets/_Project/Art/Sprites"
OUT = ROOT / "Assets/_Project/Art/Icon/AppIcon.png"

CANVAS = 64
SIZE = 1024
SCALE = SIZE // CANVAS

PLAYER = SPRITES / "Player/Player_Idle_SE.png"
ZOMBIE = SPRITES / "Enemies/Walker_Walk_SW_0.png"

MOON = (44, 22)
MOON_RADIUS = 15
CRATERS = ((39, 17, 2), (49, 27, 3), (50, 15, 1))
GROUND = 57

# One shade darker for the lighter colours of the walker; its dark trousers and outline stay, or they would melt
# into the night sky. The eyes (red_1) keep glowing.
DARKER = {
    PALETTE[light]: PALETTE[dark]
    for light, dark in (
        ("green_2", "green_1"), ("green_1", "green_0"), ("brown_1", "brown_0"), ("mauve", "dusk"),
    )
}

PREVIEW_SIZES = (60, 120, 180)


def background() -> Image.Image:
    """Night sky with a pale full moon behind the player, so his dark outfit stands out, and dark ground."""
    image = Image.new("RGB", (CANVAS, CANVAS), PALETTE["night"])
    pixels = image.load()
    for y in range(CANVAS):
        for x in range(CANVAS):
            d2 = (x - MOON[0]) ** 2 + (y - MOON[1]) ** 2
            if d2 <= MOON_RADIUS ** 2:
                color = PALETTE["slate_5"] if d2 <= (MOON_RADIUS - 2) ** 2 else PALETTE["slate_4"]
            elif d2 <= (MOON_RADIUS + 5) ** 2:
                color = PALETTE["navy_1"]
            elif d2 <= (MOON_RADIUS + 12) ** 2:
                color = PALETTE["navy_0"]
            else:
                color = PALETTE["night"]
            pixels[x, y] = color

    # Craters, so the disc reads as a moon.
    for cx, cy, r in CRATERS:
        for y in range(cy - r, cy + r + 1):
            for x in range(cx - r, cx + r + 1):
                if (x - cx) ** 2 + (y - cy) ** 2 <= r * r:
                    pixels[x, y] = PALETTE["slate_4"]

    for y in range(GROUND, CANVAS):
        for x in range(CANVAS):
            pixels[x, y] = PALETTE["slate_0"] if y == GROUND else PALETTE["night"]
    return image


def darken(sprite: Image.Image) -> Image.Image:
    """The sprite one shade darker, so it stays behind the player; the red eyes keep glowing."""
    src = sprite.load()
    out = sprite.copy()
    dst = out.load()
    w, h = sprite.size
    for y in range(h):
        for x in range(w):
            r, g, b, a = src[x, y]
            if a == 0 or (r, g, b) == PALETTE["red_1"]:
                continue
            dst[x, y] = DARKER.get((r, g, b), (r, g, b)) + (255,)
    return out


def paste_at(canvas: Image.Image, sprite: Image.Image, left: int, bottom: int) -> None:
    """Pastes the opaque part of a sprite with its box's left edge at <left> and its lowest row at <bottom>."""
    box = sprite.getbbox()
    part = sprite.crop(box)
    canvas.paste(part, (left, bottom - part.height + 1), part)


def draw() -> Image.Image:
    canvas = background().convert("RGBA")
    zombie = darken(Image.open(ZOMBIE).convert("RGBA"))
    paste_at(canvas, zombie, 3, GROUND + 1)
    player = Image.open(PLAYER).convert("RGBA")
    paste_at(canvas, player, 24, 62)
    return canvas.convert("RGB")


def check_palette(image: Image.Image) -> None:
    allowed = set(PALETTE.values())
    wrong = {image.getpixel((x, y)) for x in range(image.width) for y in range(image.height)} - allowed
    if wrong:
        raise SystemExit(f"Icon uses colours outside the palette: {sorted(wrong)[:5]}")


def preview(icon: Image.Image, folder: Path) -> Path:
    """The icon in the sizes of the home screen (60, 120, 180 px) side by side, scaled like iOS does."""
    gap = 16
    width = sum(PREVIEW_SIZES) + gap * (len(PREVIEW_SIZES) + 1)
    height = max(PREVIEW_SIZES) + 2 * gap
    sheet = Image.new("RGB", (width, height), PALETTE["slate_4"])
    x = gap
    for size in PREVIEW_SIZES:
        sheet.paste(icon.resize((size, size), Image.Resampling.LANCZOS), (x, height - gap - size))
        x += size + gap
    folder.mkdir(parents=True, exist_ok=True)
    path = folder / "icon_preview.png"
    sheet.save(path)
    return path


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--preview", type=Path, help="folder for icon_preview.png")
    args = parser.parse_args()

    small = draw()
    check_palette(small)
    icon = small.resize((SIZE, SIZE), Image.Resampling.NEAREST)
    OUT.parent.mkdir(parents=True, exist_ok=True)
    icon.save(OUT)
    print(f"{OUT.relative_to(ROOT)}: {SIZE}x{SIZE}, {CANVAS}x{CANVAS} pixel art scaled {SCALE}x")
    if args.preview:
        print(preview(icon, args.preview))


if __name__ == "__main__":
    main()
