#!/usr/bin/env python3
"""Player skins as recolours of the imported player figure (issue #129, F-SKN-02, F-SKN-03).

Each skin maps the colours of the standard figure to other palette colours. The outer outline turns to the
palette's outline colour so every skin reads the same against the floor. The idle poses are recoloured and the
walk, dash (somersault), hit and death frames are derived from them exactly like the standard figure's (gen_pixelart.player_*), so
all skins share the same animations.

Writes Assets/_Project/Art/Sprites/Player/Skins/<Id>_<State>_<DIR>[_<Frame>].png. With --preview DIR it also
writes an upscaled sheet of all skins (skins.png) and a GIF of all skins walking (skins.gif) for review.

Usage:
  python3 Tools/gen_skins.py [--preview docs/art/skins]
"""
import argparse
import sys
from pathlib import Path

from PIL import Image

sys.path.insert(0, str(Path(__file__).resolve().parent))
from gen_pixelart import (  # noqa: E402
    ART_DIR, DIRECTIONS, PALETTE, PLAYER_HIT_FRAMES, PLAYER_WALK_FRAMES, ROOT, Canvas, assert_in_palette,
    armed, player_death_frames, player_hit, player_roll_frames, player_source, player_walk,
)

SKINS_DIR = ART_DIR / "Sprites/Player/Skins"

# Colours of the standard figure and what they paint (see docs/art/skins.md):
#   navy_0  dark cloth, turban, inner lines   -> "dark"
#   navy_1  cloth highlights, turban folds    -> "mid"
#   dusk    cloak and robe                    -> "cloak"
#   mauve   cloak highlight, chest straps     -> "light"
#   clay    armour and skin, shaded           -> "accent_0"
#   apricot armour and skin, lit              -> "accent_1"
# cream and peach (glints in the eyes) stay as they are.
ROLES = {
    "navy_0": "dark",
    "navy_1": "mid",
    "dusk": "cloak",
    "mauve": "light",
    "clay": "accent_0",
    "apricot": "accent_1",
}

# Same ids and order as SkinCatalogBuilder (Unity). One skin per island, unlocked by its last level; it follows the
# island's accent colours.
SKINS = {
    # Strand
    "Sandsturm": dict(dark="brown_0", mid="brown_1", cloak="clay", light="apricot", accent_0="skin_0", accent_1="peach"),
    # Düstere Stadt
    "Rost": dict(dark="slate_0", mid="brown_0", cloak="brown_1", light="clay", accent_0="orange", accent_1="apricot"),
    # Vulkan
    "Obsidian": dict(dark="outline", mid="night", cloak="slate_0", light="slate_2", accent_0="red_0", accent_1="red_1"),
    # Meilensteine
    "Veteran": dict(dark="green_0", mid="brown_0", cloak="green_1", light="green_2", accent_0="clay", accent_1="apricot"),
    "Zombiejaeger": dict(dark="night", mid="slate_1", cloak="green_0", light="green_1", accent_0="red_0", accent_1="red_1"),
    "Goldstern": dict(dark="brown_0", mid="brown_1", cloak="orange", light="apricot", accent_0="yellow", accent_1="glow"),
    # Endlos-Modus (issue #211): Runde 10, 25, 50, colours of the colosseum
    "Gladiator": dict(dark="brown_0", mid="red_0", cloak="red_1", light="orange", accent_0="brown_1", accent_1="skin_1"),
    "Zenturio": dict(dark="slate_0", mid="slate_2", cloak="slate_4", light="slate_5", accent_0="red_0", accent_1="red_1"),
    "Imperator": dict(dark="brown_0", mid="slate_3", cloak="white", light="cream", accent_0="orange", accent_1="yellow"),
    # Gold (issue #237): bought with gold in the skin carousel; golden clothes and cloak, bright gold armour
    "Goldrausch": dict(dark="brown_0", mid="orange", cloak="yellow", light="glow", accent_0="orange", accent_1="glow"),
    # Abzeichen (issue #288, F-SKN-05): 9, 18 and 27 badges
    "Spaeher": dict(dark="green_0", mid="brown_0", cloak="brown_1", light="skin_1", accent_0="green_1", accent_1="green_2"),
    "Elite": dict(dark="outline", mid="night", cloak="slate_1", light="slate_3", accent_0="orange", accent_1="yellow"),
    "Legende": dict(dark="navy_0", mid="blue_0", cloak="glow", light="white", accent_0="blue_1", accent_1="blue_2"),
}

PREVIEW_DIRECTIONS = ["S", "SE", "E", "N"]
PREVIEW_WALK_FPS = 10


def is_edge(c: Canvas, x: int, y: int) -> bool:
    """True if the pixel borders a transparent pixel or the canvas edge (4-neighbourhood)."""
    for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1)):
        nx, ny = x + dx, y + dy
        if not (0 <= nx < c.width and 0 <= ny < c.height) or c.px[ny][nx] is None:
            return True
    return False


def recolour(idle: Canvas, colours: dict[str, str]) -> Canvas:
    """The idle pose in a skin's colours; the outer dark outline becomes the palette outline."""
    out = Canvas(idle.width, idle.height)
    for y in range(idle.height):
        for x in range(idle.width):
            key = idle.px[y][x]
            if key is None:
                continue
            if key == "navy_0" and is_edge(idle, x, y):
                out.px[y][x] = "outline"
            elif key in ROLES:
                out.px[y][x] = colours[ROLES[key]]
            else:
                out.px[y][x] = key
    return out


def skin_frames(skin: str) -> dict[str, Image.Image]:
    """All frames of one skin, keyed by file name without folder."""
    colours = SKINS[skin]
    for role in ROLES.values():
        if colours[role] not in PALETTE:
            raise ValueError(f"{skin}: {role} uses {colours[role]}, which is not in the palette")
    files = {}
    for d, name in enumerate(DIRECTIONS):
        idle = recolour(armed(player_source(name), name)[0], colours)
        files[f"{skin}_Idle_{name}.png"] = idle.to_image()
        for frame in range(PLAYER_WALK_FRAMES):
            files[f"{skin}_Walk_{name}_{frame}.png"] = player_walk(idle, frame).to_image()
        for frame in range(PLAYER_HIT_FRAMES):
            files[f"{skin}_Hit_{name}_{frame}.png"] = player_hit(idle, frame, d).to_image()
        for frame, c in enumerate(player_death_frames(idle, d)):
            files[f"{skin}_Death_{name}_{frame}.png"] = c.to_image()
        for frame, c in enumerate(player_roll_frames(idle, d)):
            files[f"{skin}_Dash_{name}_{frame}.png"] = c.to_image()
    return files


def standard_frame(state: str, direction: str, frame: int | None = None) -> Image.Image:
    suffix = "" if frame is None else f"_{frame}"
    return Image.open(ART_DIR / f"Sprites/Player/Player_{state}_{direction}{suffix}.png").convert("RGBA")


def preview(all_files: dict[str, dict[str, Image.Image]], out_dir: Path, scale: int = 4) -> None:
    """skins.png: one column per skin (standard first), one row per preview direction, plus the death's last
    frame. skins.gif: all skins walking towards the viewer."""
    names = ["Standard"] + list(SKINS)
    size = 64
    background = PALETTE["slate_0"] + (255,)
    rows = len(PREVIEW_DIRECTIONS) + 1
    sheet = Image.new("RGBA", (len(names) * size, rows * size), background)
    for col, skin in enumerate(names):
        for row, direction in enumerate(PREVIEW_DIRECTIONS):
            img = standard_frame("Idle", direction) if skin == "Standard" else all_files[skin][f"{skin}_Idle_{direction}.png"]
            sheet.alpha_composite(img, (col * size, row * size))
        last = len([k for k in (all_files[names[1]]) if k.startswith(f"{names[1]}_Death_SE_")]) - 1
        dead = standard_frame("Death", "SE", last) if skin == "Standard" else all_files[skin][f"{skin}_Death_SE_{last}.png"]
        sheet.alpha_composite(dead, (col * size, (rows - 1) * size))
    out_dir.mkdir(parents=True, exist_ok=True)
    sheet.resize((sheet.width * scale, sheet.height * scale), Image.NEAREST).save(out_dir / "skins.png")

    gif = []
    for frame in range(PLAYER_WALK_FRAMES * 2):
        img = Image.new("RGBA", (len(names) * size, size), background)
        for col, skin in enumerate(names):
            f = frame % PLAYER_WALK_FRAMES
            walk = standard_frame("Walk", "S", f) if skin == "Standard" else all_files[skin][f"{skin}_Walk_S_{f}.png"]
            img.alpha_composite(walk, (col * size, 0))
        gif.append(img.resize((img.width * scale, img.height * scale), Image.NEAREST).convert("RGB"))
    gif[0].save(out_dir / "skins.gif", save_all=True, append_images=gif[1:], duration=round(1000 / PREVIEW_WALK_FPS),
                loop=0, disposal=1)


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--preview", type=Path, help="write skins.png and skins.gif to this folder")
    args = parser.parse_args()

    SKINS_DIR.mkdir(parents=True, exist_ok=True)
    all_files = {}
    for skin in SKINS:
        all_files[skin] = skin_frames(skin)
        for name, img in all_files[skin].items():
            assert_in_palette(img, name)
            img.save(SKINS_DIR / name)
        print(f"Wrote {len(all_files[skin])} frames of {skin} to {SKINS_DIR.relative_to(ROOT)}")

    if args.preview:
        preview(all_files, args.preview)
        print(f"Wrote preview {args.preview / 'skins.png'} and skins.gif")


if __name__ == "__main__":
    main()
