#!/usr/bin/env python3
"""Generates the RushOps palette and the scripted pixel art (ADR 0006).

Everything is drawn from code so the art is reproducible. All colors come from
PALETTE; the script refuses to write an image that uses any other color.
The player character is not drawn here: it comes from the product owner's
character tool and is imported with Tools/import_character.py.

Output (under Assets/_Project/Art/):
  Palettes/RushOps_Palette.png             one pixel per palette color, in order
  Sprites/Enemies/Walker_Walk_<DIR>_<F>.png 64x64, 8 directions x 4 frames
  Sprites/Enemies/Runner_Run_<DIR>_<F>.png  64x64, 8 directions x 4 frames (issue #20)
  Sprites/Enemies/Brute_Walk_<DIR>_<F>.png  96x96, 8 directions x 4 frames (issue #21)
  Sprites/Enemies/Boomer_Walk_<DIR>_<F>.png 64x64, 8 directions x 4 frames, pulsing belly (issue #22)
  Sprites/Effects/Explosion_<F>.png         160x160, 6 frames of the boomer blast, radius 2.5 tiles (issue #22)
  Sprites/Projectiles/Bullet.png           16x16, flying right (+X)
  Sprites/Projectiles/Bullet_DoubleDamage.png 16x16, red and thicker, during double damage (issue #25)
  Sprites/Projectiles/Bullet_InstaKill.png 16x16, pale and longer, during insta-kill (issue #25)
  Sprites/Effects/MuzzleFlash.png          16x16, pointing right (+X)
  Sprites/Effects/SpawnMarker.png          32x32 ground mark shown before an enemy spawns
  Sprites/Effects/Shield_Barrier.png       64x64 bubble around the player while the shield runs (issue #26)
  Sprites/Level/HoldZone_Ring.png          128x128 ring on the floor of a hold-the-line zone, radius 2 tiles (issue #31)
  Sprites/Level/HoldZone_Fill.png          128x128 disc inside the ring; the game scales it with the progress (issue #31)
  Sprites/Level/Switch_Off.png             32x32 floor switch, not pressed: dark plate with a dim red lamp (issue #32)
  Sprites/Level/Switch_On.png              32x32 floor switch, pressed: plate sunk in, bright green lamp with glow (issue #32)
  Sprites/Level/EnergyCell.png             16x16 glowing energy cell the player carries (issue #33)
  Sprites/Level/Generator_Off.png          32x48 generator waiting for its cell, dark slot (issue #33)
  Sprites/Level/Generator_On.png           32x48 generator running, slot and lights glowing (issue #33)
  Sprites/UI/Dash_Locked.png               24x24 padlock over the dash button while it is locked (issue #33)
  Sprites/UI/Star_Full.png                 16x16 earned star on the result screen (issue #35)
  Sprites/Level/Exit_Pad.png               128x48 glowing exit field on the floor with arrows pointing on (issue #35)
  Sprites/UI/Star_Empty.png                16x16 missing star on the result screen (issue #35)
  Sprites/Pickups/Perk_<ID>.png            24x24 round perk icons, 7 perks (issue #23)
  Sprites/Pickups/Perk_Glow.png            32x16 light pool under a dropped perk (issue #23)
  Sprites/UI/Heart_Full.png                12x11 HUD heart
  Sprites/UI/Heart_Empty.png               12x11 HUD heart, lost
  Sprites/UI/Pause_Icon.png                12x12 pause symbol
  Fonts/PixelFont.png + PixelFont.json     glyph atlas and metrics of the pixel font (Tools/pixel_font.py)
  Sprites/UI/Stick_Base.png                32x32 ring of the virtual stick
  Sprites/UI/Stick_Knob.png                16x16 knob of the virtual stick
  Sprites/UI/Dash_Button.png               24x24 dash button with icon
  Sprites/UI/Dash_Cooldown.png             24x24 disc covering the button during the cooldown
  Sprites/UI/Perk_Slot.png                 28x28 HUD slot of an active perk: dark disc with an empty ring (issue #24)
  Sprites/UI/Perk_Timer.png                28x28 ring over the slot, filled radially with the remaining time (issue #24)
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
# The brute is drawn on a larger canvas (ADR 0006).
BRUTE = 96
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
    # Runner (issue #20): pale grey skin and a red shirt, so it reads apart from the green walker at a glance.
    "pale": ("slate_4", "slate_5", "white"),
    "red": ("red_0", "red_1", "orange"),
    "shorts": ("slate_0", "slate_1", "slate_2"),
    # Brute (issue #21): tan skin, a dark vest and brown trousers, so its bare arms stand out from the body.
    "hide": ("skin_0", "skin_1", "peach"),
    "vest": ("night", "slate_0", "slate_1"),
    "trousers": ("brown_0", "brown_1", "brown_1"),
    # Boomer (issue #22): sickly mauve, swollen skin with glowing yellow boils, so it reads as "explosive".
    "bloat": ("dusk", "mauve", "clay"),
    "rags": ("green_0", "green_1", "green_1"),
}


class Model:
    """Collects sphere samples and rasterises them for one facing angle."""

    def __init__(self, angle_deg: float, size: int = CHARACTER):
        a = math.radians(angle_deg)
        self.size = size
        self.forward = (math.cos(a), math.sin(a))
        self.right = (math.sin(a), -math.cos(a))
        self.samples = []

    def ground(self, f: float, s: float) -> tuple[float, float]:
        """Local (forward, right) offset to world ground (east, north)."""
        return (f * self.forward[0] + s * self.right[0], f * self.forward[1] + s * self.right[1])

    def project(self, f: float, s: float, z: float) -> tuple[float, float, float]:
        """Local point to canvas (x, y) plus its depth (north, larger = farther)."""
        east, north = self.ground(f, s)
        return self.size / 2 + east, self.size - FOOT_MARGIN - 0.5 - z - north * 0.5, north

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


def runner_frame(direction: int, frame: int) -> Canvas:
    """Runner in one of 8 directions, frame 0..3 of a sprint: smaller and thinner than the walker,
    leaning far forward with long strides and pumping arms."""
    m = Model(direction * 45.0)
    phase = frame / 4 * 2 * math.pi
    stride = 7.0 * math.sin(phase)
    # Highest in the passing frames, lowest when the legs are spread.
    bob = 1.5 * abs(math.cos(phase))
    hip_z = 15 - bob

    # Legs: thigh and shin; the back foot lifts behind the body.
    for side, step in ((-2.5, stride), (2.5, -stride)):
        lift = max(0.0, -step) * 0.6
        knee = (step * 0.6 + 2.0, side, 8.5 + lift * 0.5)
        foot = (step, side, 2.0 + lift)
        m.capsule((0, side, hip_z), knee, 2.8, "shorts")
        m.capsule(knee, foot, 2.3, "pale")
        m.sphere(foot[0] + 1.0, side, foot[2], 2.4, "shoe")
    # Thin torso leaning far forward.
    shoulder_f, shoulder_z = 6.0, 26 - bob
    m.capsule((0.5, 0.0, hip_z + 1), (shoulder_f, 0.0, shoulder_z), 5.6, "red")
    # Arms bent at the elbow, pumping against the legs.
    for side, swing in ((-6.0, -stride), (6.0, stride)):
        shoulder = (shoulder_f, side, shoulder_z)
        elbow = (shoulder_f + swing * 0.7, side * 1.05, shoulder_z - 6)
        hand = (shoulder_f + swing * 0.7 + 4.0, side * 0.95, shoulder_z - 3)
        m.capsule(shoulder, elbow, 2.0, "pale")
        m.capsule(elbow, hand, 1.8, "pale")
    # Small head thrust forward, low between the shoulders.
    head = (shoulder_f + 3.0, 0.0, shoulder_z + 5)
    m.sphere(*head, 5.0, "pale")

    c = Canvas(CHARACTER, CHARACTER)
    m.paint(c)

    # Yellow eyes, only where they face the viewer.
    _, _, head_north = m.project(*head)
    for s in (-1.8, 1.8):
        x, y, north = m.project(head[0] + 4.0, head[1] + s, head[2] + 0.5)
        if north <= head_north + 0.5:
            c.set(int(x), int(y), "yellow")
    c.outline("outline")
    return c


def brute_frame(direction: int, frame: int) -> Canvas:
    """Brute in one of 8 directions, frame 0..3 of a heavy stomp: broad and hunched, with a small head
    between huge shoulders and long bare arms hanging almost to the ground."""
    m = Model(direction * 45.0, BRUTE)
    phase = frame / 4 * 2 * math.pi
    stride = 4.5 * math.sin(phase)
    # The whole body sinks onto each step and rolls from side to side.
    bob = 1.5 * abs(math.sin(phase))
    sway = 1.5 * math.sin(phase)
    hip_z = 24 - bob

    # Short, thick legs.
    for side, step in ((-6.5, stride), (6.5, -stride)):
        m.capsule((0, side, hip_z), (step, side * 1.1, 6), 6.0, "trousers")
        m.sphere(step + 1.5, side * 1.1, 4.0, 5.0, "shoe")
    # Barrel torso in a vest, leaning forward.
    for side in (-5.0, 5.0):
        m.capsule((0, side + sway * 0.5, hip_z + 2), (3.0, side + sway, 48 - bob), 10.5, "vest")
    # Shoulders, upper and lower arms swinging slowly against the legs; fists near the knees.
    for side, swing in ((-19.0, -stride), (19.0, stride)):
        shoulder = (3.0, side + sway, 50 - bob)
        elbow = (6.0 + swing * 0.5, side * 1.08 + sway, 37 - bob)
        fist = (9.0 + swing, side * 1.05 + sway, 23 - bob)
        m.sphere(*shoulder, 7.0, "hide")
        m.capsule(shoulder, elbow, 5.0, "hide")
        m.capsule(elbow, fist, 4.5, "hide")
        m.sphere(*fist, 5.5, "hide")
    # Small head thrust forward above the shoulders.
    head = (8.0, sway, 62 - bob)
    m.sphere(*head, 7.0, "hide")

    c = Canvas(BRUTE, BRUTE)
    m.paint(c)

    # Deep-set red eyes under a dark brow, only where they face the viewer.
    _, _, head_north = m.project(*head)
    for s in (-2.6, 2.6):
        x, y, north = m.project(head[0] + 5.0, head[1] + s, head[2] + 2.0)
        if north <= head_north + 0.5:
            c.set(int(x), int(y) - 1, "skin_0")
            c.set(int(x) + 1, int(y) - 1, "skin_0")
            c.set(int(x), int(y), "red_1")
    # Torn vest: a bloody gash across the chest when the chest is visible.
    for k in range(-2, 3):
        x, y, north = m.project(13.0, sway + k * 2.5, 40 - bob - k)
        if north <= 0.5:
            c.set(int(x), int(y), "red_0")
    c.outline("outline")
    return c


def boomer_frame(direction: int, frame: int) -> Canvas:
    """Boomer in one of 8 directions, frame 0..3 of a waddle: short legs and arms on a huge swollen belly that
    pulses (inflates and sags over the 4 frames) with glowing boils that flare at the fullest frame."""
    m = Model(direction * 45.0)
    phase = frame / 4 * 2 * math.pi
    stride = 3.0 * math.sin(phase)
    sway = 1.2 * math.sin(phase)
    # Pulse: fullest in frame 1, slackest in frame 3.
    pulse = math.sin(phase)
    belly_r = 12.0 + 2.0 * pulse
    hip_z = 12.0

    # Stubby legs, spread wide under the weight.
    for side, step in ((-5.0, stride), (5.0, -stride)):
        m.capsule((0, side, hip_z), (step, side * 1.1, 4), 3.4, "rags")
        m.sphere(step + 1.0, side * 1.1, 2.5, 3.0, "shoe")
    # Swollen belly, bulging forward, and a smaller chest on top of it.
    belly = (2.0 + 0.6 * pulse, sway * 0.5, 21.0 + 0.5 * pulse)
    m.sphere(*belly, belly_r, "bloat")
    m.sphere(0.0, sway, 32.0, 7.5, "bloat")
    # Short arms dangling at the sides of the belly.
    for side in (-1, 1):
        shoulder = (0.5, side * 8.0 + sway, 33.0)
        hand = (4.0 - side * stride * 0.4, side * (belly_r + 1.5) + sway, 22.0)
        m.capsule(shoulder, hand, 2.5, "bloat")
    # Small head sunk between the shoulders.
    head = (2.0, sway, 40.0)
    m.sphere(*head, 5.5, "bloat")

    c = Canvas(CHARACTER, CHARACTER)
    m.paint(c)

    # Boils on the visible side of the belly; they glow brightest when the belly is fullest.
    hot = "glow" if pulse > 0.5 else "yellow"
    for f, s, z in ((belly_r * 0.8, -4.0, 24.0), (belly_r * 0.75, 4.5, 19.0), (belly_r * 0.6, 0.5, 14.5),
                    (belly_r * 0.55, -7.0, 17.0), (belly_r * 0.5, 8.0, 25.0)):
        x, y, north = m.project(belly[0] + f, belly[1] + s, z)
        if north <= belly_r * 0.25:
            for dx, dy in ((-1, 0), (2, 0), (0, -1), (1, -1), (0, 2), (1, 2)):
                c.set(int(x) + dx, int(y) + dy, "orange")
            for dx, dy in ((0, 0), (1, 0), (0, 1), (1, 1)):
                c.set(int(x) + dx, int(y) + dy, hot)
    # Green eyes, only where they face the viewer.
    _, _, head_north = m.project(*head)
    for s in (-2.0, 2.0):
        x, y, north = m.project(head[0] + 4.4, head[1] + s, head[2] + 0.5)
        if north <= head_north + 0.5:
            c.set(int(x), int(y), "green_2")
    c.outline("outline")
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


def bullet_double_damage() -> Canvas:
    """Bullet during double damage: red like the perk icon and one pixel thicker."""
    c = Canvas(16, 16)
    c.rect(2, 7, 4, 8, "red_0")
    c.rect(5, 6, 8, 9, "red_1")
    c.rect(9, 6, 11, 9, "orange")
    c.rect(12, 7, 13, 8, "glow")
    c.set(11, 6, "yellow")
    c.set(11, 9, "yellow")
    return c


def bullet_insta_kill() -> Canvas:
    """Bullet during insta-kill: pale like the skull of the perk icon, with a long trail and a dark edge."""
    c = Canvas(16, 16)
    c.rect(1, 7, 3, 8, "slate_3")
    c.rect(4, 7, 7, 8, "slate_5")
    c.rect(8, 7, 12, 8, "white")
    c.set(13, 7, "white")
    c.set(13, 8, "slate_5")
    c.outline()
    return c


def muzzle_flash() -> Canvas:
    c = Canvas(16, 16)
    c.ellipse(7.0, 8.0, 5.5, 4.0, "orange")
    for x, y in [(12, 7), (13, 8), (14, 8), (13, 7), (6, 3), (6, 12), (9, 4), (9, 11)]:
        c.set(x, y, "orange")
    c.ellipse(7.5, 8.0, 4.0, 2.6, "yellow")
    c.ellipse(7.0, 8.0, 2.4, 1.5, "glow")
    return c


EXPLOSION = 160
EXPLOSION_FRAMES = 6


def explosion_frame(frame: int) -> Canvas:
    """Frame 0..5 of the boomer blast on a 160x160 canvas (radius 2.5 tiles = 80 px): a white flash, a fireball
    that swells and breaks up, a shock ring that runs out to the damage radius, then drifting smoke."""
    rng = random.Random(220 + frame)
    c = Canvas(EXPLOSION, EXPLOSION)
    cx = cy = EXPLOSION / 2
    edge = EXPLOSION / 2 - 1.0

    def disc(r: float, color: str, rough: float = 0.0) -> None:
        for y in range(EXPLOSION):
            for x in range(EXPLOSION):
                d = math.hypot(x + 0.5 - cx, y + 0.5 - cy)
                if d <= r - rough * rng.random():
                    c.set(x, y, color)

    def ring(r: float, width: float, color: str, gaps: float = 0.0) -> None:
        for y in range(EXPLOSION):
            for x in range(EXPLOSION):
                d = math.hypot(x + 0.5 - cx, y + 0.5 - cy)
                if r - width <= d <= r and rng.random() >= gaps:
                    c.set(x, y, color)

    def puffs(count: int, dist: tuple, size: tuple, colors: tuple) -> None:
        for _ in range(count):
            a = rng.uniform(0, 2 * math.pi)
            d = rng.uniform(*dist)
            r = rng.uniform(*size)
            px, py = cx + math.cos(a) * d, cy + math.sin(a) * d
            c.ellipse(px, py, r, r, colors[0])
            c.ellipse(px - r * 0.25, py - r * 0.25, r * 0.6, r * 0.6, colors[1])

    if frame == 0:
        disc(22, "orange", 3)
        disc(17, "yellow", 2)
        disc(12, "glow")
        disc(6, "white")
    elif frame == 1:
        ring(48, 3, "glow")
        disc(40, "red_1", 5)
        disc(33, "orange", 4)
        disc(24, "yellow", 3)
        disc(13, "glow", 2)
    elif frame == 2:
        ring(66, 3, "yellow", 0.1)
        ring(63, 2, "orange", 0.2)
        puffs(10, (28, 44), (6, 10), ("red_0", "red_1"))
        disc(42, "red_1", 6)
        disc(34, "orange", 5)
        disc(22, "yellow", 4)
    elif frame == 3:
        ring(edge, 3, "orange", 0.15)
        puffs(12, (30, 56), (6, 11), ("slate_1", "slate_2"))
        puffs(8, (14, 36), (6, 10), ("red_0", "red_1"))
        disc(22, "orange", 6)
        disc(12, "yellow", 4)
    elif frame == 4:
        ring(edge, 2, "red_0", 0.5)
        puffs(14, (20, 60), (6, 12), ("slate_0", "slate_2"))
        puffs(5, (4, 22), (5, 8), ("red_0", "orange"))
    else:
        puffs(10, (24, 66), (4, 9), ("night", "slate_1"))
        puffs(4, (6, 26), (4, 7), ("slate_0", "slate_2"))
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


# --- Perk pickups (issue #23) --------------------------------------------------

PERK_ICON = 24

# Glyphs drawn on the round perk badge: '#' main color, '+' accent, 'o' dark detail; '.' shows the badge.
PERK_GLYPHS = {
    "SpeedBoost": [
        "......####",
        ".....####.",
        "....####..",
        "...####...",
        "..#######.",
        ".#######..",
        "....###...",
        "...###....",
        "..###.....",
        "..##......",
        ".##.......",
        ".#........",
    ],
    "Medkit": [
        "....####....",
        "....####....",
        "....####....",
        "....####....",
        "############",
        "############",
        "############",
        "############",
        "....####....",
        "....####....",
        "....####....",
        "....####....",
    ],
    "DoubleDamage": [
        ".####.......",
        "##..##......",
        "....##.##..##",
        "...##...####.",
        "..##.....##..",
        ".##.....####.",
        "######.##..##",
    ],
    "RapidFire": [
        ".######+.....",
        "#######++....",
        ".######+.....",
        ".............",
        "...######+...",
        "..#######++..",
        "...######+...",
        ".............",
        ".....######+.",
        "....#######++",
        ".....######+.",
    ],
    "Shield": [
        ".+++++#####.",
        "++++++######",
        "++++++######",
        "++++++######",
        "++++++######",
        ".+++++#####.",
        ".+++++#####.",
        "..++++####..",
        "..++++####..",
        "...+++###...",
        "....++##....",
        ".....+#.....",
    ],
    "InstaKill": [
        "..########..",
        ".##########.",
        "############",
        "##oo####oo##",
        "##oo####oo##",
        "############",
        ".#####o#####",
        "..#########.",
        "..#o#o#o#o..",
        "..########..",
    ],
}

# Badge (shadow, base, light) and glyph (main, accent, detail) colors per perk.
PERK_COLORS = {
    "SpeedBoost": (("green_0", "green_1", "green_2"), ("yellow", "glow", "outline")),
    "Medkit": (("slate_4", "white", "white"), ("red_1", "red_0", "outline")),
    "DoubleDamage": (("red_0", "red_1", "orange"), ("white", "white", "outline")),
    "RapidFire": (("brown_1", "orange", "yellow"), ("white", "glow", "outline")),
    "Shield": (("blue_0", "blue_1", "blue_2"), ("slate_5", "white", "outline")),
    "InstaKill": (("night", "slate_1", "slate_3"), ("white", "white", "outline")),
    "Nuke": (("orange", "yellow", "glow"), ("outline", "outline", "outline")),
}

PERK_IDS = ["SpeedBoost", "Medkit", "DoubleDamage", "RapidFire", "Shield", "InstaKill", "Nuke"]


def perk_badge(shadow: str, base: str, light: str) -> Canvas:
    """Round coin: dark rim at the lower right, a light arc at the upper left."""
    c = Canvas(PERK_ICON, PERK_ICON)
    c.ellipse(12.0, 12.0, 10.5, 10.5, shadow)
    c.ellipse(11.5, 11.5, 9.5, 9.5, base)
    for x, y in [(6, 5), (5, 6), (7, 4), (4, 7), (8, 4), (4, 8)]:
        c.set(x, y, light)
    return c


def perk_icon(perk: str) -> Canvas:
    (shadow, base, light), (main, accent, detail) = PERK_COLORS[perk]
    c = perk_badge(shadow, base, light)
    if perk == "Nuke":
        # Radiation trefoil: three 60 degree blades around a dot, one pointing up.
        cx, cy = 11.5, 11.5
        for y in range(PERK_ICON):
            for x in range(PERK_ICON):
                dx, dy = x + 0.5 - cx, y + 0.5 - cy
                r = math.hypot(dx, dy)
                angle = (math.degrees(math.atan2(-dy, dx)) - 60.0) % 120.0
                if r <= 1.7 or (2.8 <= r <= 7.6 and angle < 60.0):
                    c.set(x, y, main)
    else:
        rows = PERK_GLYPHS[perk]
        width = max(len(row) for row in rows)
        x0 = 12 - (width + 1) // 2
        y0 = 12 - (len(rows) + 1) // 2
        colors = {"#": main, "+": accent, "o": detail}
        for y, row in enumerate(rows):
            for x, ch in enumerate(row):
                if ch in colors:
                    c.set(x0 + x, y0 + y, colors[ch])
    c.outline()
    return c


HOLD_ZONE = 128


def hold_zone_ring() -> Canvas:
    """Rim of a hold-the-line zone, a true circle on the floor grid (radius 2 tiles = 64 px) so it matches the area
    the game checks: a bright blue rim with a dark inner edge and short ticks pointing inward every 45 degrees."""
    c = Canvas(HOLD_ZONE, HOLD_ZONE)
    cx = cy = HOLD_ZONE / 2
    for y in range(HOLD_ZONE):
        for x in range(HOLD_ZONE):
            r = math.hypot(x + 0.5 - cx, y + 0.5 - cy)
            if 61.0 <= r <= 63.5:
                c.set(x, y, "blue_2")
            elif 59.5 <= r < 61.0:
                c.set(x, y, "blue_0")
    for k in range(8):
        a = math.radians(k * 45.0)
        for step in range(5):
            r = 58.5 - step
            c.set(int(cx + math.cos(a) * r), int(cy - math.sin(a) * r), "blue_1" if step else "glow")
    return c


def hold_zone_fill() -> Canvas:
    """Disc that grows inside the ring with the hold progress; drawn solid, the game makes it see-through."""
    c = Canvas(HOLD_ZONE, HOLD_ZONE)
    c.ellipse(HOLD_ZONE / 2, HOLD_ZONE / 2, 59.0, 59.0, "blue_1")
    c.ellipse(HOLD_ZONE / 2, HOLD_ZONE / 2, 55.0, 55.0, "blue_2")
    return c


def floor_switch(on: bool) -> Canvas:
    """Square pressure plate on the floor. Off: raised plate with a dim red lamp. On: the plate sits lower and darker
    while the lamp glows green with a bright ring, readable from across the screen."""
    c = Canvas(TILE, TILE)
    c.rect(3, 3, 28, 28, "slate_0")
    top = 5 if not on else 6
    c.rect(5, top, 26, 26, "slate_2" if not on else "slate_1")
    if not on:
        c.rect(5, 5, 26, 5, "slate_3")
        c.rect(5, 26, 26, 26, "slate_1")
    if on:
        c.ellipse(16.0, 16.5, 9.0, 9.0, "green_1")
        c.ellipse(16.0, 16.5, 7.0, 7.0, "green_2")
        c.ellipse(16.0, 16.5, 4.0, 4.0, "glow")
    else:
        c.ellipse(16.0, 16.0, 6.0, 6.0, "red_0")
        c.ellipse(15.0, 15.0, 2.0, 2.0, "red_1")
    c.outline()
    return c


def energy_cell() -> Canvas:
    """Upright energy cell: dark caps at both ends, a glowing green core with a bright stripe."""
    c = Canvas(16, 16)
    c.rect(5, 1, 10, 2, "slate_2")
    c.rect(4, 3, 11, 13, "slate_1")
    c.rect(5, 4, 10, 12, "green_1")
    c.rect(6, 4, 9, 12, "green_2")
    c.rect(7, 5, 7, 11, "glow")
    c.rect(4, 14, 11, 14, "slate_2")
    c.rect(5, 8, 10, 8, "slate_1")
    c.outline()
    return c


def generator(on: bool) -> Canvas:
    """Squat machine with a cell slot in front and two lamps on top; running, the slot and lamps glow green."""
    c = Canvas(32, 48)
    c.rect(4, 14, 27, 45, "slate_1")
    c.rect(4, 14, 27, 17, "slate_3")
    c.rect(6, 18, 25, 43, "slate_2")
    c.rect(4, 44, 27, 45, "slate_0")
    # Lamps on top.
    for x in (9, 22):
        c.rect(x - 2, 9, x + 2, 13, "slate_0")
        c.rect(x - 1, 10, x + 1, 12, "green_2" if on else "red_0")
        if on:
            c.set(x, 11, "glow")
    # Cell slot.
    c.rect(11, 24, 20, 39, "night")
    if on:
        c.rect(12, 25, 19, 38, "green_1")
        c.rect(14, 26, 17, 37, "green_2")
        c.rect(15, 27, 16, 36, "glow")
    else:
        c.rect(12, 25, 19, 26, "slate_0")
    # Vents.
    for y in (20, 41):
        c.rect(8, y, 23, y, "slate_1")
    c.outline()
    return c


def dash_locked() -> Canvas:
    """Padlock drawn over the dash button while carrying something; the button underneath is covered dark."""
    c = Canvas(24, 24)
    c.ellipse(12.0, 12.0, 11.0, 11.0, "night")
    for y in range(4, 12):
        for x in range(7, 17):
            r = math.hypot(x + 0.5 - 12.0, y + 0.5 - 10.0)
            if 3.0 <= r <= 4.6 and y < 11:
                c.set(x, y, "slate_5")
    c.rect(7, 11, 16, 18, "yellow")
    c.rect(7, 11, 16, 11, "glow")
    c.rect(11, 13, 12, 16, "brown_0")
    return c


def star(full: bool) -> Canvas:
    """Five-pointed star for the result screen: gold with a light upper half when earned, a dark hollow when not."""
    c = Canvas(16, 16)
    cx, cy = 8.0, 8.5
    outer, inner = 7.5, 3.2
    points = []
    for k in range(10):
        r = outer if k % 2 == 0 else inner
        a = math.radians(90 + k * 36)
        points.append((cx + r * math.cos(a), cy - r * math.sin(a)))

    def inside(px: float, py: float) -> bool:
        hit = False
        j = len(points) - 1
        for i in range(len(points)):
            xi, yi = points[i]
            xj, yj = points[j]
            if (yi > py) != (yj > py) and px < (xj - xi) * (py - yi) / (yj - yi) + xi:
                hit = not hit
            j = i
        return hit

    for y in range(16):
        for x in range(16):
            if inside(x + 0.5, y + 0.5):
                if full:
                    c.set(x, y, "yellow" if y < 8 else "orange")
                else:
                    c.set(x, y, "slate_1")
    if full:
        c.set(7, 4, "glow")
        c.set(6, 6, "glow")
    c.outline()
    return c


def exit_pad() -> Canvas:
    """Level exit on the floor: a dark green field with a bright rim and three chevrons pointing up (north)."""
    c = Canvas(128, 48)
    c.rect(2, 2, 125, 45, "green_2")
    c.rect(4, 4, 123, 43, "green_0")
    c.rect(6, 6, 121, 41, "green_1")
    for cx in (32, 64, 96):
        # Chevron: two diagonal strokes meeting at the top.
        for i in range(10):
            for t in range(3):
                c.set(cx - 10 + i, 32 - i + t, "glow")
                c.set(cx + 10 - i, 32 - i + t, "glow")
        c.rect(cx - 1, 22, cx + 1, 24, "glow")
    c.outline()
    return c


SHIELD_BARRIER = 64


def shield_barrier() -> Canvas:
    """Bubble around the player during the shield perk: a bright rim in the blue of the perk icon, a darker inner
    edge, a highlight at the upper left and a few glints. The inside stays empty, so the player shows through."""
    c = Canvas(SHIELD_BARRIER, SHIELD_BARRIER)
    cx = cy = SHIELD_BARRIER / 2
    for y in range(SHIELD_BARRIER):
        for x in range(SHIELD_BARRIER):
            r = math.hypot(x + 0.5 - cx, y + 0.5 - cy)
            if 29.0 <= r <= 31.0:
                c.set(x, y, "blue_2")
            elif 27.5 <= r < 29.0:
                c.set(x, y, "blue_1")
    for y in range(SHIELD_BARRIER):
        for x in range(SHIELD_BARRIER):
            dx, dy = x + 0.5 - cx, y + 0.5 - cy
            r = math.hypot(dx, dy)
            angle = math.degrees(math.atan2(-dy, dx))
            if 28.5 <= r <= 30.5 and 115.0 <= angle <= 155.0:
                c.set(x, y, "white")
    for x, y in [(44, 12), (45, 13), (52, 40), (14, 46), (15, 47), (38, 54)]:
        c.set(x, y, "glow")
    return c


def perk_glow() -> Canvas:
    """Light pool on the floor under a dropped perk, squashed for the oblique view; its alpha pulses in game."""
    c = Canvas(32, 16)
    c.ellipse(16.0, 8.0, 14.0, 6.5, "orange")
    c.ellipse(16.0, 8.0, 11.5, 5.0, "yellow")
    c.ellipse(16.0, 8.0, 7.0, 3.0, "glow")
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


PERK_SLOT = 28


def ring_mask(size: int, inner: float, outer: float):
    """Pixels whose center lies between the radii around the canvas center."""
    center = size / 2.0
    for y in range(size):
        for x in range(size):
            r = math.hypot(x + 0.5 - center, y + 0.5 - center)
            if inner <= r <= outer:
                yield x, y


def perk_slot() -> Canvas:
    """HUD slot behind a perk icon: a dark disc whose rim shows the empty timer track."""
    c = Canvas(PERK_SLOT, PERK_SLOT)
    c.ellipse(14.0, 14.0, 14.0, 14.0, "night")
    for x, y in ring_mask(PERK_SLOT, 11.5, 13.5):
        c.set(x, y, "slate_1")
    return c


def perk_timer() -> Canvas:
    """Bright ring on the slot's track; the HUD fills it radially, so it empties as the perk runs out."""
    c = Canvas(PERK_SLOT, PERK_SLOT)
    for x, y in ring_mask(PERK_SLOT, 11.5, 13.5):
        c.set(x, y, "yellow")
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


def gate_bars() -> Canvas:
    """One cell of a gate (issue #17): iron portcullis under a hazard-striped beam, floor visible between the bars."""
    c = Canvas(TILE, TILE)
    beam = 8
    # Beam on top with yellow and dark hazard stripes, so gates stand out from walls.
    c.rect(0, 0, TILE - 1, beam - 1, "yellow")
    for x in range(-beam, TILE, 8):
        for y in range(beam):
            for w in range(4):
                c.set(x + y + w, y, "outline")
    c.rect(0, beam, TILE - 1, beam, "brown_0")
    # Vertical bars with a lit left edge and a crossbar.
    for x in range(2, TILE, 6):
        c.rect(x, beam + 1, x + 2, TILE - 2, "slate_1")
        c.rect(x, beam + 1, x, TILE - 2, "slate_4")
        c.set(x + 1, TILE - 1, "slate_0")
    c.rect(0, 19, TILE - 1, 20, "slate_1")
    c.rect(0, 19, TILE - 1, 19, "slate_4")
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
        "Sprites/Projectiles/Bullet_DoubleDamage.png": bullet_double_damage().to_image(),
        "Sprites/Projectiles/Bullet_InstaKill.png": bullet_insta_kill().to_image(),
        "Sprites/Effects/MuzzleFlash.png": muzzle_flash().to_image(),
        "Tiles/Wall_Top.png": wall_top().to_image(),
        "Tiles/Wall_Front.png": wall_front().to_image(),
        "Sprites/Effects/SpawnMarker.png": spawn_marker().to_image(),
        "Sprites/Effects/Shield_Barrier.png": shield_barrier().to_image(),
        "Sprites/Level/HoldZone_Ring.png": hold_zone_ring().to_image(),
        "Sprites/Level/HoldZone_Fill.png": hold_zone_fill().to_image(),
        "Sprites/Level/Switch_Off.png": floor_switch(False).to_image(),
        "Sprites/Level/Switch_On.png": floor_switch(True).to_image(),
        "Sprites/Level/EnergyCell.png": energy_cell().to_image(),
        "Sprites/Level/Generator_Off.png": generator(False).to_image(),
        "Sprites/Level/Generator_On.png": generator(True).to_image(),
        "Sprites/UI/Dash_Locked.png": dash_locked().to_image(),
        "Sprites/UI/Star_Full.png": star(True).to_image(),
        "Sprites/Level/Exit_Pad.png": exit_pad().to_image(),
        "Sprites/UI/Star_Empty.png": star(False).to_image(),
        "Sprites/Level/Gate_Bars.png": gate_bars().to_image(),
        "Sprites/UI/Heart_Full.png": heart(True).to_image(),
        "Sprites/UI/Heart_Empty.png": heart(False).to_image(),
        "Sprites/UI/Pause_Icon.png": pause_icon().to_image(),
        "Fonts/PixelFont.png": font_atlas()[0],
        "Sprites/UI/Stick_Base.png": stick_base().to_image(),
        "Sprites/UI/Stick_Knob.png": stick_knob().to_image(),
        "Sprites/UI/Dash_Button.png": dash_button().to_image(),
        "Sprites/UI/Dash_Cooldown.png": dash_cooldown().to_image(),
        "Sprites/UI/Perk_Slot.png": perk_slot().to_image(),
        "Sprites/UI/Perk_Timer.png": perk_timer().to_image(),
        "Sprites/Pickups/Perk_Glow.png": perk_glow().to_image(),
    }
    for perk in PERK_IDS:
        files[f"Sprites/Pickups/Perk_{perk}.png"] = perk_icon(perk).to_image()
    for d, name in enumerate(DIRECTIONS):
        for frame in range(4):
            files[f"Sprites/Enemies/Walker_Walk_{name}_{frame}.png"] = walker_frame(d, frame).to_image()
            files[f"Sprites/Enemies/Runner_Run_{name}_{frame}.png"] = runner_frame(d, frame).to_image()
            files[f"Sprites/Enemies/Brute_Walk_{name}_{frame}.png"] = brute_frame(d, frame).to_image()
            files[f"Sprites/Enemies/Boomer_Walk_{name}_{frame}.png"] = boomer_frame(d, frame).to_image()
    for frame in range(EXPLOSION_FRAMES):
        files[f"Sprites/Effects/Explosion_{frame}.png"] = explosion_frame(frame).to_image()
    for variant in range(3):
        files[f"Tiles/Floor_{variant}.png"] = floor(variant).to_image()
    return files


def preview(files: dict[str, Image.Image], path: Path, scale: int = 3) -> None:
    """Floor patch with a wall block, the player (if imported), walkers, runners, brutes and boomers in all directions."""
    cols, rows = 13, 16
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
        put(files[f"Sprites/Enemies/Runner_Run_{name}_{i % 4}.png"], 1 + i, 7)
        put(files[f"Sprites/Enemies/Brute_Walk_{name}_{i % 4}.png"], 1 + i % 4 * 3, 10 + i // 4 * 3)
        put(files[f"Sprites/Enemies/Boomer_Walk_{name}_{i % 4}.png"], 1 + i, 15)
    player_dir = ART_DIR / "Sprites/Player"
    if (player_dir / "Player_Idle_SE.png").exists():
        put(Image.open(player_dir / "Player_Idle_N.png").convert("RGBA"), 4, 0)
        put(Image.open(player_dir / "Player_Idle_SE.png").convert("RGBA"), 1, 3)
    put(files["Sprites/Enemies/Walker_Walk_SW_0.png"], 3, 3)
    sheet.alpha_composite(files["Sprites/Effects/MuzzleFlash.png"], (6 * TILE, 3 * TILE + 8))
    sheet.alpha_composite(files["Sprites/Projectiles/Bullet.png"], (6 * TILE + 20, 3 * TILE + 8))
    sheet.alpha_composite(files["Sprites/Projectiles/Bullet_DoubleDamage.png"], (7 * TILE + 4, 3 * TILE + 8))
    sheet.alpha_composite(files["Sprites/Projectiles/Bullet_InstaKill.png"], (7 * TILE + 20, 3 * TILE + 8))
    for i, perk in enumerate(PERK_IDS):
        x, y = (8 + i % 4) * TILE, (1 + i // 4 * 2) * TILE
        sheet.alpha_composite(files["Sprites/Pickups/Perk_Glow.png"], (x, y + 24))
        sheet.alpha_composite(files[f"Sprites/Pickups/Perk_{perk}.png"], (x + 4, y + 4))
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
