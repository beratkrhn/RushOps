#!/usr/bin/env python3
"""Arcade cabinet of RushOps (issue #320, F-ARC-01, F-ARC-06, design decisions AR1 to AR4).

A classic cabinet seen from the front, laid out in the 360x640 reference grid of the game:

    y   0..55   marquee: the RushOps logo in one line, the player and a walker on the sides
    y  56..415  bezel: painted frame with zombies reaching for the screen; the screen (320x320) is a hole in it
    y 416..639  control panel: two joysticks, the big yellow dash button, decorative buttons, coin door below

Everything is drawn from the project palette with a fixed seed, partly from the game's own sprites. The runtime
places the parts with the rectangles in Arcade_Layout.json (reference pixels, origin top left), which
„RushOps → Arcade → Arcade-Daten aktualisieren“ copies into Data/Arcade/ArcadeConfig.asset.

    python3 Tools/gen_arcade.py                         # writes Assets/_Project/Art/Sprites/Arcade/
    python3 Tools/gen_arcade.py --preview docs/art/arcade   # plus arcade_preview.png, 2x, with a fake game screen

Fails if a pixel lies outside the palette (ADR 0006).
"""
import argparse
import json
import math
import random
import sys
from pathlib import Path

from PIL import Image

ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / "Tools"))
import pixel_font as pf  # noqa: E402
from gen_pixelart import PALETTE, Canvas, assert_in_palette  # noqa: E402

SPRITES = ROOT / "Assets/_Project/Art/Sprites"
OUT_DIR = SPRITES / "Arcade"
SEED = 320

# Reference grid (ADR 0006): 360x640 portrait.
W, H = 360, 640
MARQUEE_H = 56
BEZEL_H = 360
PANEL_H = H - MARQUEE_H - BEZEL_H
SCREEN = 320
SCREEN_X = (W - SCREEN) // 2
SCREEN_Y = 16
assert SCREEN_Y + SCREEN <= BEZEL_H

# Control panel, in panel pixels (origin top left of the panel).
PANEL_SURFACE_H = 160
STICK_CANVAS = (56, 64)
# Ball offset of a fully tilted stick: sideways full, forward and back flattened by the slanted view.
STICK_TILT_X = 9
STICK_TILT_Y = 6
STICK_LEFT = (72, 84)
STICK_RIGHT = (288, 84)
DASH_CANVAS = (52, 44)
DASH = (180, 98)
DECO_CANVAS = (16, 14)
DECO = [("Red", (148, 34)), ("Blue", (180, 30)), ("Green", (212, 34))]

DIRECTIONS = ["E", "NE", "N", "NW", "W", "SW", "S", "SE"]
KEY_OF_RGB = {rgb: key for key, rgb in PALETTE.items()}


# --- Helpers --------------------------------------------------------------------

def sprite(path: str) -> Image.Image:
    return Image.open(SPRITES / path).convert("RGBA")


def paste(c: Canvas, img: Image.Image, x: int, y: int, box=None, recolor=None) -> None:
    """Copies the opaque pixels of a palette image (optionally cropped to box) onto the canvas at x, y."""
    if box is not None:
        img = img.crop(box)
    for iy in range(img.height):
        for ix in range(img.width):
            r, g, b, a = img.getpixel((ix, iy))
            if a == 0:
                continue
            key = KEY_OF_RGB[(r, g, b)]
            if recolor:
                key = recolor.get(key, key)
            c.set(x + ix, y + iy, key)


def text_width(text: str, scale: int = 1) -> int:
    width = 0
    for ch in text:
        width += (pf.SPACE_ADVANCE if ch == " " else len(pf.GLYPHS[ch][0]) + 1) * scale
    return width - scale


def draw_text(c: Canvas, x: int, y: int, text: str, color: str, scale: int = 1, shadow: str | None = None) -> None:
    for ch in text:
        if ch == " ":
            x += pf.SPACE_ADVANCE * scale
            continue
        rows = pf.GLYPHS[ch]
        for gy, row in enumerate(rows):
            for gx, v in enumerate(row):
                if v != "#":
                    continue
                for dy in range(scale):
                    for dx in range(scale):
                        if shadow:
                            c.set(x + gx * scale + dx + scale, y + gy * scale + dy + scale, shadow)
                        c.set(x + gx * scale + dx, y + gy * scale + dy, color)
        x += (len(rows[0]) + 1) * scale


def centered_text(c: Canvas, cx: int, y: int, text: str, color: str, scale: int = 1, shadow: str | None = None):
    draw_text(c, cx - text_width(text, scale) // 2, y, text, color, scale, shadow)


def hline(c: Canvas, x0: int, x1: int, y: int, color: str) -> None:
    c.rect(x0, y, x1, y, color)


def vline(c: Canvas, x: int, y0: int, y1: int, color: str) -> None:
    c.rect(x, y0, x, y1, color)


def line(c: Canvas, x0: float, y0: float, x1: float, y1: float, color: str, width: int = 1) -> None:
    steps = int(max(abs(x1 - x0), abs(y1 - y0))) + 1
    for i in range(steps + 1):
        t = i / steps
        x = round(x0 + (x1 - x0) * t)
        y = round(y0 + (y1 - y0) * t)
        for d in range(width):
            c.set(x - width // 2 + d, y, color)


def rounded_rect(c: Canvas, x0: int, y0: int, x1: int, y1: int, r: int, color: str) -> None:
    for y in range(y0, y1 + 1):
        for x in range(x0, x1 + 1):
            dx = max(x0 + r - x, 0, x - (x1 - r))
            dy = max(y0 + r - y, 0, y - (y1 - r))
            if dx * dx + dy * dy <= r * r:
                c.set(x, y, color)


# --- Marquee --------------------------------------------------------------------
# Logo in one line: "RUSH" in the fiery ramp of the menu logo, "OPS" in pale grey, both 4x the pixel font.

LOGO_SCALE = 4
LOGO_GAP = 3
LOGO_RUSH_RAMP = ["yellow", "yellow", "orange", "orange", "orange", "red_1", "red_1"]
LOGO_OPS_RAMP = ["white", "white", "slate_5", "slate_5", "slate_5", "slate_4", "slate_4"]


def word_width(word: str) -> int:
    return sum(len(pf.GLYPHS[ch][0]) * LOGO_SCALE for ch in word) + LOGO_GAP * (len(word) - 1)


def draw_logo_word(c: Canvas, word: str, x: int, y: int, ramp: list[str], shadow: str) -> None:
    cells = []
    for ch in word:
        for gy, row in enumerate(pf.GLYPHS[ch]):
            for gx, v in enumerate(row):
                if v == "#":
                    for dy in range(LOGO_SCALE):
                        for dx in range(LOGO_SCALE):
                            cells.append((x + gx * LOGO_SCALE + dx, y + gy * LOGO_SCALE + dy, gy))
        x += len(pf.GLYPHS[ch][0]) * LOGO_SCALE + LOGO_GAP
    filled = {(cx, cy) for cx, cy, _ in cells}
    for cx, cy, _ in cells:
        c.set(cx + 2, cy + 2, shadow)
    for cx, cy, row in cells:
        top = (cx, cy - 1) not in filled
        c.set(cx, cy, ("glow" if ramp is LOGO_RUSH_RAMP else "white") if top else ramp[row])


def marquee() -> Canvas:
    c = Canvas(W, MARQUEE_H, "outline")
    # Backlit panel: dark centre, rays from the logo outwards.
    c.rect(4, 4, W - 5, MARQUEE_H - 5, "night")
    cx, cy = W // 2, MARQUEE_H // 2
    for y in range(4, MARQUEE_H - 4):
        for x in range(4, W - 4):
            angle = math.atan2((y - cy) * 3.0, x - cx)
            if int((angle + math.pi) / (math.pi / 14)) % 2 == 0:
                c.set(x, y, "navy_1")
    c.ellipse(cx, cy, 104, 22, "navy_0")
    # Rim: chrome strip with bulbs.
    for x in range(0, W):
        c.set(x, 2, "slate_3")
        c.set(x, MARQUEE_H - 3, "slate_2")
    for x in range(10, W - 6, 16):
        for y in (2, MARQUEE_H - 3):
            c.set(x, y, "glow")
            c.set(x + 1, y, "yellow")

    rush_w, ops_w = word_width("RUSH"), word_width("OPS")
    total = rush_w + 8 + ops_w
    lx = cx - total // 2
    ly = cy - 7 * LOGO_SCALE // 2 - 1
    draw_logo_word(c, "RUSH", lx, ly, LOGO_RUSH_RAMP, "red_0")
    draw_logo_word(c, "OPS", lx + rush_w + 8, ly, LOGO_OPS_RAMP, "navy_0")

    # The hero on the left, a walker on the right, both looking at the logo, cut by the frame.
    hero = sprite("Player/Player_Idle_E.png")
    walker = sprite("Enemies/Walker_Walk_W_0.png")
    paste(c, hero, 22, 4, box=(10, 6, 54, 50))
    paste(c, walker, W - 66, 4, box=(10, 9, 54, 53))
    return c


# --- Bezel ----------------------------------------------------------------------

def zombie_arm(c: Canvas, x: int, bottom: int, height: int, lean: int, rng: random.Random) -> None:
    """A rotten arm reaching up from below, hand with three fingers on top."""
    top = bottom - height
    for y in range(top, bottom + 1):
        t = (bottom - y) / max(height, 1)
        ax = x + round(lean * t)
        c.set(ax - 3, y, "outline")
        c.set(ax + 3, y, "outline")
        c.rect(ax - 2, y, ax + 2, y, "green_1")
        c.set(ax - 2, y, "green_0")
        c.set(ax + 2, y, "green_2" if y % 3 else "green_1")
    hx = x + lean
    c.rect(hx - 3, top - 3, hx + 3, top, "green_1")
    for f, fx in enumerate((-3, 0, 3)):
        length = 4 + (f == 1) * 2 + rng.randint(0, 1)
        vline(c, hx + fx, top - 3 - length, top - 3, "green_2" if f == 1 else "green_1")
    # Torn sleeve.
    c.rect(x - 3, bottom - 6, x + 3, bottom, "brown_1")
    for sx in range(x - 3, x + 4, 2):
        c.set(sx, bottom - 7, "brown_1")


def bezel() -> Canvas:
    rng = random.Random(SEED)
    c = Canvas(W, BEZEL_H, "navy_0")
    # Painted backdrop: diagonal flame bands rising from the bottom corners.
    for y in range(BEZEL_H):
        for x in range(W):
            d = min(x, W - 1 - x) * 0.8 + (BEZEL_H - y) * 0.5
            if d < 18:
                c.set(x, y, "red_0")
            elif d < 30 and (x + y) % 4 < 2:
                c.set(x, y, "red_0")
            elif (x * 7 + y * 3) % 23 == 0:
                c.set(x, y, "navy_1")
    for _ in range(40):
        x, y = rng.randrange(W), rng.randrange(BEZEL_H)
        c.set(x, y, "dusk")

    # Zombie arms climbing up both sides, cut off where they reach the glass.
    for x, h, lean in ((7, 120, 3), (13, 70, -2), (W - 8, 140, -3), (W - 14, 90, 2)):
        zombie_arm(c, x, BEZEL_H - 4, h, lean, rng)

    # Heads peeking over the bottom edge of the screen: three walkers and the hero in the middle.
    walker = sprite("Enemies/Walker_Walk_S_0.png")
    runner = sprite("Enemies/Runner_Run_S_0.png")
    hero = sprite("Player/Player_Idle_S.png")
    strip_top = SCREEN_Y + SCREEN
    paste(c, walker, 44, strip_top - 1, box=(20, 10, 44, 10 + BEZEL_H - strip_top + 1))
    paste(c, runner, 104, strip_top + 1, box=(23, 28, 41, 28 + BEZEL_H - strip_top - 1))
    paste(c, hero, W // 2 - 17, strip_top - 2, box=(15, 6, 49, 6 + BEZEL_H - strip_top + 2))
    paste(c, walker, W - 104, strip_top, box=(20, 10, 44, 10 + BEZEL_H - strip_top))
    paste(c, runner, W - 56, strip_top + 2, box=(23, 28, 41, 28 + BEZEL_H - strip_top - 2))

    # Blood drips from the top edge.
    for x in range(30, W - 30, 37):
        length = rng.randint(3, 10)
        vline(c, x, 0, length, "red_1")
        c.set(x, length + 1, "red_0")

    # Recessed tube: dark rim of three pixels around the hole, lit on the bottom edge.
    x0, y0, x1, y1 = SCREEN_X - 3, SCREEN_Y - 3, SCREEN_X + SCREEN + 2, SCREEN_Y + SCREEN + 2
    rounded_rect(c, x0 - 1, y0 - 1, x1 + 1, y1 + 1, 8, "slate_2")
    rounded_rect(c, x0, y0, x1, y1, 7, "outline")
    hline(c, x0 + 7, x1 - 7, y1 + 1, "slate_4")
    # The hole the game shows through.
    c.rect(SCREEN_X, SCREEN_Y, SCREEN_X + SCREEN - 1, SCREEN_Y + SCREEN - 1, None)
    # Outer edge of the cabinet.
    for y in range(BEZEL_H):
        c.set(0, y, "outline")
        c.set(1, y, "slate_1")
        c.set(W - 1, y, "outline")
        c.set(W - 2, y, "slate_1")
    return c


def screen_frame() -> Canvas:
    """Laid over the game image: rounded black corners of the tube and a soft dark rim."""
    c = Canvas(SCREEN, SCREEN)
    r = 10
    for y in range(SCREEN):
        for x in range(SCREEN):
            dx = max(r - x, 0, x - (SCREEN - 1 - r))
            dy = max(r - y, 0, y - (SCREEN - 1 - r))
            if dx * dx + dy * dy > r * r:
                c.set(x, y, "outline")
    for i in range(SCREEN):
        if r <= i < SCREEN - r:
            c.set(i, 0, "outline")
            c.set(0, i, "outline")
            c.set(SCREEN - 1, i, "outline")
            c.set(i, SCREEN - 1, "outline")
    return c


def screen_gloss() -> Canvas:
    """Reflection on the glass: two diagonal streaks top left, a dot bottom right. The runtime draws it faint."""
    c = Canvas(SCREEN, SCREEN)
    # Two slanted bars across the top left corner, rising to the right.
    for offset, thickness, length in ((18, 7, 120), (34, 3, 90)):
        for t in range(length):
            for k in range(thickness):
                y = offset - t // 2 + k + 40
                if 6 < y < SCREEN:
                    c.set(offset + t, y, "white")
    c.ellipse(SCREEN - 26, SCREEN - 22, 5, 3, "white")
    return c


# --- Control panel --------------------------------------------------------------

def control_panel() -> Canvas:
    rng = random.Random(SEED + 1)
    c = Canvas(W, PANEL_H, "slate_1")
    # Lip between bezel and panel.
    c.rect(0, 0, W - 1, 5, "outline")
    hline(c, 0, W - 1, 6, "slate_4")
    hline(c, 0, W - 1, 7, "slate_3")
    # Panel art: a red band with a tracer stripe through the middle, dark wells around the sticks.
    c.rect(0, 50, W - 1, 118, "slate_2")
    for x in range(W):
        if (x // 6) % 2 == 0:
            c.set(x, 50, "red_1")
            c.set(x, 118, "red_1")
    for (sx, sy) in (STICK_LEFT, STICK_RIGHT):
        c.ellipse(sx, sy + 18, 40, 20, "slate_0")
        c.ellipse(sx, sy + 18, 36, 17, "night")
    c.ellipse(DASH[0], DASH[1] + 10, 34, 20, "slate_0")
    c.ellipse(DASH[0], DASH[1] + 10, 30, 17, "red_0")
    for _ in range(60):
        x, y = rng.randrange(W), rng.randrange(10, PANEL_SURFACE_H - 4)
        if c.get(x, y) == "slate_1":
            c.set(x, y, "slate_0")
    # Labels under the controls.
    centered_text(c, STICK_LEFT[0], 128, "BEWEGEN", "slate_5", shadow="outline")
    centered_text(c, STICK_RIGHT[0], 128, "ZIELEN", "slate_5", shadow="outline")
    centered_text(c, DASH[0], 128, "DASH", "yellow", shadow="outline")
    # Screws at the corners.
    for x, y in ((8, 14), (W - 9, 14), (8, PANEL_SURFACE_H - 10), (W - 9, PANEL_SURFACE_H - 10)):
        c.set(x, y, "slate_4")
        c.set(x + 1, y + 1, "slate_0")

    # Front edge of the panel and the cabinet front with the coin door.
    c.rect(0, PANEL_SURFACE_H, W - 1, PANEL_SURFACE_H + 3, "slate_3")
    hline(c, 0, W - 1, PANEL_SURFACE_H + 4, "outline")
    c.rect(0, PANEL_SURFACE_H + 5, W - 1, PANEL_H - 1, "red_0")
    for y in range(PANEL_SURFACE_H + 5, PANEL_H):
        for x in range(W):
            if (x + y * 2) % 29 == 0:
                c.set(x, y, "brown_0")
    door_x0, door_x1 = W // 2 - 50, W // 2 + 49
    door_y0, door_y1 = PANEL_SURFACE_H + 14, PANEL_H - 6
    c.rect(door_x0, door_y0, door_x1, door_y1, "outline")
    c.rect(door_x0 + 2, door_y0 + 2, door_x1 - 2, door_y1 - 2, "slate_1")
    hline(c, door_x0 + 2, door_x1 - 2, door_y0 + 2, "slate_3")
    for i, slot_x in enumerate((W // 2 - 26, W // 2 + 14)):
        c.rect(slot_x, door_y0 + 10, slot_x + 12, door_y0 + 26, "slate_0")
        c.rect(slot_x + 2, door_y0 + 12, slot_x + 10, door_y0 + 24, "red_1" if i == 0 else "orange")
        vline(c, slot_x + 6, door_y0 + 14, door_y0 + 22, "outline")
        c.set(slot_x + 3, door_y0 + 13, "glow")
    centered_text(c, W // 2, door_y1 - 12, "MÜNZE", "glow", shadow="outline")
    # Side moulding.
    for y in range(PANEL_H):
        c.set(0, y, "outline")
        c.set(1, y, "slate_1" if y < PANEL_SURFACE_H else "red_1")
        c.set(W - 1, y, "outline")
        c.set(W - 2, y, "slate_1" if y < PANEL_SURFACE_H else "red_1")
    return c


def body_tile() -> Canvas:
    """32x32 tile of the cabinet side for screens taller or wider than the 9:16 cabinet."""
    c = Canvas(32, 32, "night")
    for (x, y) in ((3, 5), (17, 11), (26, 3), (9, 22), (22, 27), (29, 18)):
        c.set(x, y, "slate_0")
    return c


# --- Joystick -------------------------------------------------------------------

def stick_base() -> Canvas:
    c = Canvas(40, 20)
    c.ellipse(20, 10, 19, 9, "outline")
    c.ellipse(20, 10, 18, 8, "slate_0")
    c.ellipse(20, 9, 15, 6, "slate_2")
    c.ellipse(20, 10, 6, 3, "outline")
    return c


def stick_shaft() -> Canvas:
    c = Canvas(6, 28)
    c.rect(1, 0, 4, 27, "slate_4")
    vline(c, 1, 0, 27, "slate_3")
    vline(c, 4, 0, 27, "slate_5")
    vline(c, 0, 0, 27, "outline")
    vline(c, 5, 0, 27, "outline")
    return c


def draw_ball(c: Canvas, cx: float, cy: float, r: float = 9.0) -> None:
    c.ellipse(cx, cy, r + 1, r + 1, "outline")
    c.ellipse(cx, cy, r, r, "red_0")
    c.ellipse(cx - 1, cy - 1, r - 1.5, r - 1.5, "red_1")
    c.ellipse(cx - 3, cy - 4, 2.5, 2, "white")


def stick_ball() -> Canvas:
    c = Canvas(20, 20)
    draw_ball(c, 10, 10)
    return c


def stick(direction: str | None) -> Canvas:
    """Stick with base, shaft and ball, tilted to direction (None = centred). The base stays where it is."""
    cw, ch = STICK_CANVAS
    c = Canvas(cw, ch)
    base = stick_base()
    bx, by = cw // 2 - base.width // 2, ch - base.height - 1
    for y in range(base.height):
        for x in range(base.width):
            if base.get(x, y):
                c.set(bx + x, by + y, base.get(x, y))
    pivot = (cw / 2, by + base.height / 2)
    dx = dy = 0.0
    if direction:
        angle = DIRECTIONS.index(direction) * math.pi / 4
        dx, dy = math.cos(angle) * STICK_TILT_X, -math.sin(angle) * STICK_TILT_Y
    ball = (pivot[0] + dx, pivot[1] - 30 + dy)
    line(c, pivot[0], pivot[1], ball[0], ball[1], "outline", 6)
    line(c, pivot[0], pivot[1], ball[0], ball[1], "slate_4", 4)
    line(c, pivot[0] + 1, pivot[1], ball[0] + 1, ball[1], "slate_5", 1)
    draw_ball(c, ball[0], ball[1])
    return c


# --- Buttons --------------------------------------------------------------------

def dash_button(pressed: bool) -> Canvas:
    cw, ch = DASH_CANVAS
    c = Canvas(cw, ch)
    cx = cw / 2
    c.ellipse(cx, ch - 12, 25, 11, "outline")
    c.ellipse(cx, ch - 12, 24, 10, "slate_2")
    top = ch - 26 + (6 if pressed else 0)
    # Side of the cap.
    for y in range(int(top), ch - 14):
        for x in range(cw):
            if abs(x + 0.5 - cx) <= 18:
                c.set(x, y + 1, "orange")
    c.ellipse(cx, ch - 14, 18.5, 7.5, "orange")
    c.ellipse(cx, top, 19, 8, "outline")
    c.ellipse(cx, top, 18, 7, "yellow")
    c.ellipse(cx - 4, top - 2, 7, 2.5, "glow")
    for y in range(int(top), ch - 13):
        c.set(int(cx - 19), y, "outline")
        c.set(int(cx + 18), y, "outline")
    return c


def deco_button(color: str) -> Canvas:
    ramps = {"Red": ("red_0", "red_1"), "Blue": ("blue_0", "blue_2"), "Green": ("green_0", "green_2")}
    dark, light = ramps[color]
    c = Canvas(*DECO_CANVAS)
    c.ellipse(8, 8, 7.5, 5.5, "outline")
    c.ellipse(8, 8, 6.5, 4.5, dark)
    c.ellipse(8, 6, 6, 4, "outline")
    c.ellipse(8, 6, 5, 3, light)
    c.set(6, 5, "white")
    return c


# --- Output ---------------------------------------------------------------------

def outputs() -> dict[str, Canvas]:
    files = {
        "Arcade_Marquee": marquee(),
        "Arcade_Bezel": bezel(),
        "Arcade_ScreenFrame": screen_frame(),
        "Arcade_ScreenGloss": screen_gloss(),
        "Arcade_ControlPanel": control_panel(),
        "Arcade_Body": body_tile(),
        "Arcade_StickBase": stick_base(),
        "Arcade_StickShaft": stick_shaft(),
        "Arcade_StickBall": stick_ball(),
        "Arcade_Stick_C": stick(None),
        "Arcade_DashButton": dash_button(False),
        "Arcade_DashButton_Pressed": dash_button(True),
    }
    for d in DIRECTIONS:
        files[f"Arcade_Stick_{d}"] = stick(d)
    for name, _ in DECO:
        files[f"Arcade_Button_{name}"] = deco_button(name)
    return files


def rect(x: int, y: int, w: int, h: int) -> dict:
    return {"x": x, "y": y, "width": w, "height": h}


def layout() -> dict:
    """Rectangles in reference pixels (origin top left of the 360x640 cabinet)."""
    panel_y = MARQUEE_H + BEZEL_H

    def centred(pos, size):
        return rect(pos[0] - size[0] // 2, panel_y + pos[1] - size[1] // 2, size[0], size[1])

    return {
        "reference": {"width": W, "height": H},
        "marquee": rect(0, 0, W, MARQUEE_H),
        "bezel": rect(0, MARQUEE_H, W, BEZEL_H),
        "screen": rect(SCREEN_X, MARQUEE_H + SCREEN_Y, SCREEN, SCREEN),
        "panel": rect(0, panel_y, W, PANEL_H),
        "panelSurfaceHeight": PANEL_SURFACE_H,
        "stickLeft": centred(STICK_LEFT, STICK_CANVAS),
        "stickRight": centred(STICK_RIGHT, STICK_CANVAS),
        "dashButton": centred(DASH, DASH_CANVAS),
        "decoButtons": [dict(name=name, **centred(pos, DECO_CANVAS)) for name, pos in DECO],
    }


def compose(files: dict[str, Image.Image], game: Image.Image | None = None) -> Image.Image:
    """The whole cabinet at rest, as the runtime lays it out."""
    lay = layout()
    img = Image.new("RGBA", (W, H), PALETTE["night"] + (255,))
    if game is not None:
        img.alpha_composite(game, (lay["screen"]["x"], lay["screen"]["y"]))
    img.alpha_composite(files["Arcade_ScreenFrame"], (lay["screen"]["x"], lay["screen"]["y"]))
    img.alpha_composite(files["Arcade_Marquee"], (0, 0))
    img.alpha_composite(files["Arcade_Bezel"], (0, MARQUEE_H))
    img.alpha_composite(files["Arcade_ControlPanel"], (0, lay["panel"]["y"]))
    for key, name in (("stickLeft", "Arcade_Stick_C"), ("stickRight", "Arcade_Stick_NE"),
                      ("dashButton", "Arcade_DashButton")):
        img.alpha_composite(files[name], (lay[key]["x"], lay[key]["y"]))
    for deco in lay["decoButtons"]:
        img.alpha_composite(files["Arcade_Button_" + deco["name"]], (deco["x"], deco["y"]))
    return img


def fake_game() -> Image.Image:
    """A stand-in for the game image in the preview: floor tiles, the hero and some walkers."""
    img = Image.new("RGBA", (SCREEN, SCREEN), PALETTE["slate_1"] + (255,))
    floor = Image.open(ROOT / "Assets/_Project/Art/Tiles/Floor_0.png").convert("RGBA") \
        if (ROOT / "Assets/_Project/Art/Tiles/Floor_0.png").exists() else None
    if floor is not None:
        for y in range(0, SCREEN, floor.height):
            for x in range(0, SCREEN, floor.width):
                img.alpha_composite(floor, (x, y))
    hero = sprite("Player/Player_Idle_NE.png")
    img.alpha_composite(hero, (SCREEN // 2 - 32, SCREEN // 2 - 40))
    for i, (x, y, d) in enumerate(((40, 40, "SE"), (230, 60, "SW"), (60, 220, "NE"), (240, 230, "NW"))):
        img.alpha_composite(sprite(f"Enemies/Walker_Walk_{d}_{i % 4}.png"), (x, y))
    return img


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--preview", type=Path, help="also write arcade_preview.png (2x) into this folder")
    args = parser.parse_args()

    images = {name: canvas.to_image() for name, canvas in outputs().items()}
    for name, img in images.items():
        assert_in_palette(img, name)
    OUT_DIR.mkdir(parents=True, exist_ok=True)
    for name, img in images.items():
        img.save(OUT_DIR / f"{name}.png")
    (OUT_DIR / "Arcade_Layout.json").write_text(json.dumps(layout(), indent=2) + "\n", encoding="utf-8")
    print(f"{len(images)} sprites and Arcade_Layout.json written to {OUT_DIR.relative_to(ROOT)}")

    if args.preview:
        args.preview.mkdir(parents=True, exist_ok=True)
        still = compose(images, fake_game())
        still.resize((W * 2, H * 2), Image.NEAREST).save(args.preview / "arcade_preview.png")
        sheet = Image.new("RGBA", (STICK_CANVAS[0] * 9 + 4 * DASH_CANVAS[0], STICK_CANVAS[1]), PALETTE["slate_1"] + (255,))
        for i, d in enumerate(["C"] + DIRECTIONS):
            sheet.alpha_composite(images[f"Arcade_Stick_{d}"], (i * STICK_CANVAS[0], 0))
        sheet.alpha_composite(images["Arcade_DashButton"], (9 * STICK_CANVAS[0] + 4, 4))
        sheet.alpha_composite(images["Arcade_DashButton_Pressed"], (9 * STICK_CANVAS[0] + DASH_CANVAS[0] + 12, 4))
        sheet.resize((sheet.width * 4, sheet.height * 4), Image.NEAREST).save(args.preview / "arcade_controls.png")
        print(f"previews written to {args.preview}")


if __name__ == "__main__":
    main()
