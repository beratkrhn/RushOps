#!/usr/bin/env python3
"""Generates the RushOps palette and the scripted pixel art (ADR 0006).

Everything is drawn from code so the art is reproducible. All colors come from
PALETTE; the script refuses to write an image that uses any other color.
The player character is not drawn here: it comes from the product owner's
character tool and is imported with Tools/import_character.py.

Output (under Assets/_Project/Art/):
  Palettes/RushOps_Palette.png             one pixel per palette color, in order
  Sprites/Enemies/Walker_Walk_<DIR>_<F>.png 64x64, 8 directions x 4 frames
  Sprites/Enemies/Walker_Hit_<DIR>_<F>.png  64x64, 8 directions x 2 frames, recoil (issue #100)
  Sprites/Enemies/Walker_Death_<DIR>_<F>.png 64x64, 8 directions x 5 frames, topples backward (issue #100)
  Sprites/Enemies/Runner_Run_<DIR>_<F>.png  64x64, 8 directions x 4 frames (issue #20)
  Sprites/Enemies/Runner_Hit_<DIR>_<F>.png  64x64, 8 directions x 2 frames, recoil (issue #100)
  Sprites/Enemies/Runner_Death_<DIR>_<F>.png 64x64, 8 directions x 5 frames, pitches forward (issue #100)
  Sprites/Enemies/Brute_Walk_<DIR>_<F>.png  96x96, 8 directions x 4 frames (issue #21)
  Sprites/Enemies/Brute_Hit_<DIR>_<F>.png   96x96, 8 directions x 2 frames (issue #101)
  Sprites/Enemies/Brute_Death_<DIR>_<F>.png 96x96, 8 directions x 5 frames, slams face down (issue #101)
  Sprites/Enemies/Boomer_Walk_<DIR>_<F>.png 64x64, 8 directions x 4 frames, pulsing belly (issue #22)
  Sprites/Enemies/Boomer_Hit_<DIR>_<F>.png  64x64, 8 directions x 2 frames (issue #101)
  Sprites/Enemies/Boomer_Swell_<DIR>_<F>.png 64x64, 8 directions x 4 frames, inflated and glowing near the player (issue #101)
  Sprites/Enemies/Boomer_Death_<DIR>_<F>.png 64x64, 8 directions x 4 frames, bursts (issue #101)
  Sprites/Effects/Explosion_<F>.png         160x160, 6 frames of the boomer blast, radius 2.5 tiles (issue #22)
  Sprites/Projectiles/Bullet.png           16x16, flying right (+X)
  Sprites/Projectiles/Bullet_DoubleDamage.png 16x16, red and thicker, during double damage (issue #25)
  Sprites/Projectiles/Bullet_InstaKill.png 16x16, pale and longer, during insta-kill (issue #25)
  Sprites/Effects/MuzzleFlash.png          16x16, pointing right (+X) from the centre (issue #107)
  Sprites/Effects/HitSpark_<F>.png         16x16, 3 frames of sparks flying towards +X (issue #107)
  Sprites/Effects/DeathSplat_<F>.png       32x32, 4 frames of blood flying apart when an enemy dies (issue #107)
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
  Sprites/Level/Exit_Pad.png               128x48 glowing exit field on the floor with arrows pointing on (issue #35, #103)
  Sprites/Level/Exit_Pad_Locked.png        128x48 the exit while locked: dark plate, red rim and padlock (issue #103)
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
  Tiles/<Island>/Floor_0..2.png            island floor variants (issue #104: Strand, #105: Stadt, #106: Vulkan)
  Tiles/<Island>/Wall_Top.png, Wall_Front.png        outer wall of the island
  Tiles/<Island>/Obstacle_Top.png, Obstacle_Front.png inner walls (obstacles)
  Tiles/<Island>/Decor_<N>.png             32x32 flat decorations without collision
  Tiles/<Island>/Gate_<F>.png              32x32 gate cell, frame 0 closed .. last frame open

Usage:
  python3 Tools/gen_pixelart.py [--preview PATH] [--animations DIR]

--preview writes an upscaled contact sheet (characters on the floor next to
walls) for a visual check before committing. It is not part of the project.
--animations writes one upscaled GIF per enemy (all 8 directions playing walk,
hit and death) plus a sprite sheet, for the product owner to sign off.
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

# Palette groups (issue #99, design decisions G1 and G3): bright, saturated colors for characters, perks and
# puzzle objects in front of dark, desaturated surroundings. Every island takes 6 to 8 accent colors from the one
# palette; islands may share colors. check_palette_groups() enforces these rules.
PALETTE_GROUPS = {
    # On every island: outlines, dark ground and UI, plus the signal colors of perks, damage, solved puzzles and light.
    "Basis": ["outline", "night", "slate_0", "slate_1", "slate_2", "slate_3", "slate_4", "slate_5", "white",
              "glow", "yellow", "red_1", "green_2", "blue_2"],
    # Characters: the player from the character tool and the four enemy types.
    "Figuren": ["navy_0", "navy_1", "dusk", "mauve", "clay", "apricot", "peach", "cream", "green_0", "green_1",
                "green_2", "skin_0", "skin_1", "red_0", "red_1", "orange", "brown_0", "brown_1", "slate_4",
                "slate_5", "white", "yellow"],
    # Island 1: sand, sea and palms.
    "Strand": ["peach", "cream", "apricot", "clay", "blue_0", "blue_1", "blue_2", "green_1"],
    # Island 2: asphalt, concrete and cold neon.
    "Düstere Stadt": ["navy_0", "navy_1", "dusk", "slate_1", "slate_2", "slate_3", "mauve", "blue_2"],
    # Island 3: basalt, embers and lava.
    "Vulkan": ["outline", "night", "brown_0", "brown_1", "red_0", "red_1", "orange", "yellow"],
}
ISLAND_GROUPS = ["Strand", "Düstere Stadt", "Vulkan"]
MAX_PALETTE_COLORS = 32


def check_palette_groups() -> None:
    """Raises if the palette or its groups break the rules of issue #99."""
    if len(PALETTE) > MAX_PALETTE_COLORS:
        raise ValueError(f"palette has {len(PALETTE)} colors, at most {MAX_PALETTE_COLORS} are allowed")
    used = set()
    for group, keys in PALETTE_GROUPS.items():
        unknown = [k for k in keys if k not in PALETTE]
        if unknown:
            raise ValueError(f"palette group {group}: unknown colors {unknown}")
        used.update(keys)
    for island in ISLAND_GROUPS:
        if not 6 <= len(PALETTE_GROUPS[island]) <= 8:
            raise ValueError(f"island {island} needs 6 to 8 accent colors, has {len(PALETTE_GROUPS[island])}")
    unused = [k for k in PALETTE if k not in used]
    if unused:
        raise ValueError(f"palette colors in no group: {unused}")


def palette_sheet(scale: int = 16) -> Image.Image:
    """One row of swatches per palette group, for docs/art/palette.png."""
    width = max(len(keys) for keys in PALETTE_GROUPS.values())
    rows = len(PALETTE_GROUPS)
    gap = scale // 4
    img = Image.new("RGBA", (width * (scale + gap) + gap, rows * (scale + gap) + gap), PALETTE["night"] + (255,))
    for row, keys in enumerate(PALETTE_GROUPS.values()):
        for col, key in enumerate(keys):
            x0 = gap + col * (scale + gap)
            y0 = gap + row * (scale + gap)
            for x in range(x0, x0 + scale):
                for y in range(y0, y0 + scale):
                    img.putpixel((x, y), PALETTE[key] + (255,))
    return img


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


class Pose:
    """Whole-body tilt for hit and death frames (issue #100).

    tilt: degrees the body tips over around the feet; positive falls backward, negative falls forward.
    shift: ground offset along the facing (pixels) at a full 90 degree fall, keeps a lying body on the canvas.
    squash: share of the height lost as the knees buckle (0 = upright, 0.3 = sagging).
    """

    def __init__(self, tilt: float = 0.0, shift: float = 0.0, squash: float = 0.0):
        self.tilt = tilt
        self.shift = shift
        self.squash = squash


UPRIGHT = Pose()


class Model:
    """Collects sphere samples and rasterises them for one facing angle."""

    def __init__(self, angle_deg: float, size: int = CHARACTER, pose: Pose = UPRIGHT):
        a = math.radians(angle_deg)
        self.size = size
        self.forward = (math.cos(a), math.sin(a))
        self.right = (math.sin(a), -math.cos(a))
        self.pose = pose
        self.samples = []

    def ground(self, f: float, s: float) -> tuple[float, float]:
        """Local (forward, right) offset to world ground (east, north)."""
        return (f * self.forward[0] + s * self.right[0], f * self.forward[1] + s * self.right[1])

    def posed(self, f: float, s: float, z: float) -> tuple[float, float, float]:
        """Applies the pose: squashes the height, then tips the body around the feet."""
        z *= 1.0 - self.pose.squash
        t = math.radians(self.pose.tilt)
        # Positive tilt rotates "up" towards "back" (-f).
        f, z = f * math.cos(t) - z * math.sin(t), f * math.sin(t) + z * math.cos(t)
        f += self.pose.shift * math.sin(t)
        return f, s, z

    def project(self, f: float, s: float, z: float) -> tuple[float, float, float]:
        """Local point to canvas (x, y) plus its depth (north, larger = farther)."""
        f, s, z = self.posed(f, s, z)
        return self._project_raw(f, s, z)

    def _project_raw(self, f: float, s: float, z: float) -> tuple[float, float, float]:
        east, north = self.ground(f, s)
        return self.size / 2 + east, self.size - FOOT_MARGIN - 0.5 - z - north * 0.5, north

    def sphere(self, f: float, s: float, z: float, r: float, material: str) -> None:
        f, s, z = self.posed(f, s, z)
        # A tipped-over body part rests on the floor instead of sinking into it.
        if self.pose.tilt:
            z = max(z, r)
        self.samples.append((*self._project_raw(f, s, z), z, r, material))

    def ground_stain(self, canvas: "Canvas", f: float, s: float, rx: float, color: str, rim: str) -> None:
        """Paints a flat pool on the floor at local (f, s) (not posed), before the body, e.g. blood under a corpse."""
        x, y, _ = self._project_raw(f, s, 0.0)
        canvas.ellipse(x, y, rx, rx * 0.5, rim)
        canvas.ellipse(x, y - 0.5, rx - 1.5, rx * 0.5 - 1.0, color)

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


def walker_frame(direction: int, frame: int, pose: Pose = UPRIGHT, eyes: bool = True,
                 stain: float = 0.0) -> Canvas:
    """Walker in one of 8 directions, frame 0..3 of a slow shamble. Hit and death frames pass a pose; dead
    walkers lose the glow in their eyes, and a stain radius > 0 puts a blood pool under the body."""
    m = Model(direction * 45.0, pose=pose)
    c = Canvas(CHARACTER, CHARACTER)
    if stain > 0:
        m.ground_stain(c, m.posed(0.0, 0.0, 44.0)[0], 0.0, stain, "red_0", "outline")
    limp = pose.tilt > 30.0
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
        if limp:
            # Dead arms fall to the sides.
            elbow = (2.5, side * 1.1, 26.0)
            hand = (4.0, side * 1.2, 19.0)
        m.capsule(shoulder, elbow, 2.9, "shirt")
        m.capsule(elbow, hand, 2.5, "skin")
    # Head, hanging forward.
    head = (3.0, sway, 42 - bob)
    m.sphere(*head, 6.8, "skin")

    m.paint(c)

    # Glowing eyes, only where they face the viewer.
    _, _, head_north = m.project(*head)
    for s in (-2.4, 2.4):
        x, y, north = m.project(head[0] + 5.4, head[1] + s, head[2] + 0.5)
        if eyes and north <= head_north + 0.5:
            c.set(int(x), int(y), "red_1")
    # Blood stain on the chest when the chest is visible.
    x, y, north = m.project(7.0, sway + 1.5, 27 - bob)
    if north <= 0.5:
        c.set(int(x), int(y), "red_0")
        c.set(int(x) + 1, int(y) + 1, "red_0")
    c.outline("navy_0")
    return c


def runner_frame(direction: int, frame: int, pose: Pose = UPRIGHT, eyes: bool = True,
                 stain: float = 0.0) -> Canvas:
    """Runner in one of 8 directions, frame 0..3 of a sprint: smaller and thinner than the walker,
    leaning far forward with long strides and pumping arms. Hit and death frames as with the walker."""
    m = Model(direction * 45.0, pose=pose)
    c = Canvas(CHARACTER, CHARACTER)
    if stain > 0:
        m.ground_stain(c, m.posed(0.0, 0.0, 32.0)[0], 0.0, stain, "red_0", "outline")
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

    m.paint(c)

    # Yellow eyes, only where they face the viewer.
    _, _, head_north = m.project(*head)
    for s in (-1.8, 1.8):
        x, y, north = m.project(head[0] + 4.0, head[1] + s, head[2] + 0.5)
        if eyes and north <= head_north + 0.5:
            c.set(int(x), int(y), "yellow")
    c.outline("outline")
    return c


def brute_frame(direction: int, frame: int, pose: Pose = UPRIGHT, eyes: bool = True,
                stain: float = 0.0) -> Canvas:
    """Brute in one of 8 directions, frame 0..3 of a heavy stomp: broad and hunched, with a small head
    between huge shoulders and long bare arms hanging almost to the ground. Hit and death frames as with the walker."""
    m = Model(direction * 45.0, BRUTE, pose)
    c = Canvas(BRUTE, BRUTE)
    if stain > 0:
        m.ground_stain(c, m.posed(0.0, 0.0, 56.0)[0], 0.0, stain, "red_0", "outline")
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

    m.paint(c)

    # Deep-set red eyes under a dark brow, only where they face the viewer.
    _, _, head_north = m.project(*head)
    for s in (-2.6, 2.6):
        x, y, north = m.project(head[0] + 5.0, head[1] + s, head[2] + 2.0)
        if north <= head_north + 0.5:
            c.set(int(x), int(y) - 1, "skin_0")
            c.set(int(x) + 1, int(y) - 1, "skin_0")
            c.set(int(x), int(y), "red_1" if eyes else "skin_0")
    # Torn vest: a bloody gash across the chest when the chest is visible.
    for k in range(-2, 3):
        x, y, north = m.project(13.0, sway + k * 2.5, 40 - bob - k)
        if north <= 0.5:
            c.set(int(x), int(y), "red_0")
    c.outline("outline")
    return c


def boomer_frame(direction: int, frame: int, pose: Pose = UPRIGHT, swell: float = 0.0, eyes: bool = True) -> Canvas:
    """Boomer in one of 8 directions, frame 0..3 of a waddle: short legs and arms on a huge swollen belly that
    pulses (inflates and sags over the 4 frames) with glowing boils that flare at the fullest frame.
    swell > 0 inflates the belly further (pixels of extra radius) and keeps the boils glowing: the warning
    before it blows up (issue #101)."""
    m = Model(direction * 45.0, pose=pose)
    phase = frame / 4 * 2 * math.pi
    stride = 3.0 * math.sin(phase)
    sway = 1.2 * math.sin(phase)
    # Pulse: fullest in frame 1, slackest in frame 3.
    pulse = math.sin(phase)
    belly_r = 12.0 + 2.0 * pulse + swell
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
    hot = "glow" if pulse > 0.5 or swell > 0 else "yellow"
    rim = "yellow" if swell > 0 and pulse > -0.5 else "orange"
    for f, s, z in ((belly_r * 0.8, -4.0, 24.0), (belly_r * 0.75, 4.5, 19.0), (belly_r * 0.6, 0.5, 14.5),
                    (belly_r * 0.55, -7.0, 17.0), (belly_r * 0.5, 8.0, 25.0)):
        x, y, north = m.project(belly[0] + f, belly[1] + s, z)
        if north <= belly_r * 0.25:
            for dx, dy in ((-1, 0), (2, 0), (0, -1), (1, -1), (0, 2), (1, 2)):
                c.set(int(x) + dx, int(y) + dy, rim)
            for dx, dy in ((0, 0), (1, 0), (0, 1), (1, 1)):
                c.set(int(x) + dx, int(y) + dy, hot)
    # Green eyes, only where they face the viewer.
    _, _, head_north = m.project(*head)
    for s in (-2.0, 2.0):
        x, y, north = m.project(head[0] + 4.4, head[1] + s, head[2] + 0.5)
        if eyes and north <= head_north + 0.5:
            c.set(int(x), int(y), "glow" if swell > 0 else "green_2")
    c.outline("outline")
    return c


# Hit and death frames (issue #100). Hit: the body jerks back from the impact and settles. Death: the walker
# topples backward from the shot, the runner pitches forward with its momentum; the last frame lies in a pool.
HIT_POSES = [Pose(tilt=14.0), Pose(tilt=6.0)]
WALKER_DEATH_POSES = [Pose(tilt=24.0, shift=26.0, squash=0.08), Pose(tilt=48.0, shift=26.0, squash=0.15),
                      Pose(tilt=70.0, shift=26.0, squash=0.2), Pose(tilt=84.0, shift=26.0, squash=0.22),
                      Pose(tilt=90.0, shift=26.0, squash=0.22)]
RUNNER_DEATH_POSES = [Pose(tilt=-28.0, shift=20.0, squash=0.1), Pose(tilt=-55.0, shift=20.0, squash=0.18),
                      Pose(tilt=-76.0, shift=20.0, squash=0.22), Pose(tilt=-88.0, shift=20.0, squash=0.24),
                      Pose(tilt=-90.0, shift=20.0, squash=0.24)]
DEATH_STAIN = [0.0, 0.0, 0.0, 8.0, 12.0]
# Brute (issue #101): heavy, it sinks to its knees first and then slams face down.
BRUTE_DEATH_POSES = [Pose(tilt=-8.0, shift=34.0, squash=0.15), Pose(tilt=-30.0, shift=34.0, squash=0.25),
                     Pose(tilt=-62.0, shift=34.0, squash=0.3), Pose(tilt=-86.0, shift=34.0, squash=0.32),
                     Pose(tilt=-90.0, shift=34.0, squash=0.32)]
BRUTE_DEATH_STAIN = [0.0, 0.0, 0.0, 12.0, 18.0]
# Boomer (issue #101): the swollen warning loop near the player, and a burst on death.
BOOMER_SWELL = 4.0
BOOMER_DEATH_FRAMES = 4


def brute_hit(direction: int, frame: int) -> Canvas:
    return brute_frame(direction, 0, Pose(tilt=HIT_POSES[frame].tilt * 0.6))


def brute_death(direction: int, frame: int) -> Canvas:
    return brute_frame(direction, 0, BRUTE_DEATH_POSES[frame], eyes=frame == 0, stain=BRUTE_DEATH_STAIN[frame])


def boomer_hit(direction: int, frame: int) -> Canvas:
    return boomer_frame(direction, 1, HIT_POSES[frame])


def boomer_swell(direction: int, frame: int) -> Canvas:
    return boomer_frame(direction, frame, swell=BOOMER_SWELL)


def boomer_death(direction: int, frame: int) -> Canvas:
    """Frame 0: blown up to bursting, glowing all over. Frame 1: the burst, chunks flying. Frames 2..3: the
    remains, a sticky pool with scraps. The explosion effect plays on top."""
    if frame == 0:
        c = boomer_frame(direction, 1, swell=BOOMER_SWELL + 3.0, eyes=False)
        # Cracks of light all over the skin.
        rng = random.Random(direction * 31 + 7)
        for _ in range(14):
            x, y = rng.randint(18, 45), rng.randint(22, 52)
            if c.get(x, y) in ("dusk", "mauve", "clay"):
                c.set(x, y, "glow")
                c.set(x + 1, y, "yellow")
        return c
    c = Canvas(CHARACTER, CHARACTER)
    cx, cy = CHARACTER / 2, CHARACTER - FOOT_MARGIN - 8
    pool = 8.0 + 2.5 * frame
    c.ellipse(cx, cy, pool + 1, pool * 0.5 + 1, "outline")
    c.ellipse(cx, cy - 0.5, pool, pool * 0.5, "green_0")
    c.ellipse(cx - 1, cy - 1, pool * 0.55, pool * 0.3, "dusk")
    rng = random.Random(direction * 13 + frame)
    # Chunks: high and far in the burst frame, settled on the floor afterwards.
    reach, lift = (16.0, 14.0) if frame == 1 else (pool + 1.0, 0.0)
    for i in range(7):
        a = rng.uniform(0, 2 * math.pi)
        d = rng.uniform(0.5, 1.0) * reach
        x = cx + math.cos(a) * d
        y = cy - math.sin(a) * d * 0.5 - lift * rng.uniform(0.4, 1.0)
        material = ("mauve", "clay", "rags", "dusk")[i % 4]
        color = RAMP[material][1] if material in RAMP else material
        c.ellipse(x, y, 1.8, 1.4, color)
    if frame == 1:
        c.ellipse(cx, cy - 10, 5, 4, "glow")
        c.ellipse(cx, cy - 10, 3, 2.5, "white")
    c.outline("outline")
    return c


def lowest_row(c: Canvas) -> int:
    """Index of the lowest row with a filled pixel, -1 if empty."""
    for y in range(c.height - 1, -1, -1):
        if any(px is not None for px in c.px[y]):
            return y
    return -1


def shifted_up(c: Canvas, dy: int) -> Canvas:
    out = Canvas(c.width, c.height)
    out.px = c.px[dy:] + [[None] * c.width for _ in range(dy)]
    return out


def keep_on_canvas(frames: list[Canvas], margin: int = 1) -> list[Canvas]:
    """A body falling towards the viewer lies below the feet and would leave the canvas at the bottom. Lifts each
    frame just enough, never less than the frame before, so the corpse slides up smoothly instead of jumping."""
    lift, out = 0, []
    for c in frames:
        lift = max(lift, lowest_row(c) - (c.height - 1 - margin))
        out.append(shifted_up(c, lift) if lift > 0 else c)
    return out


def death_frames(draw, direction: int, count: int) -> list[Canvas]:
    return keep_on_canvas([draw(direction, f) for f in range(count)])


def walker_hit(direction: int, frame: int) -> Canvas:
    return walker_frame(direction, 0, HIT_POSES[frame])


def walker_death(direction: int, frame: int) -> Canvas:
    return walker_frame(direction, 0, WALKER_DEATH_POSES[frame], eyes=frame == 0, stain=DEATH_STAIN[frame])


def runner_hit(direction: int, frame: int) -> Canvas:
    return runner_frame(direction, 1, HIT_POSES[frame])


def runner_death(direction: int, frame: int) -> Canvas:
    return runner_frame(direction, 1, RUNNER_DEATH_POSES[frame], eyes=frame == 0, stain=DEATH_STAIN[frame])


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
    """Muzzle flash pointing right (+X) from the canvas centre, where the muzzle sits (issue #107): a hot core with
    a long forward prong and two short side prongs."""
    c = Canvas(16, 16)
    c.ellipse(9.5, 8.0, 3.5, 3.5, "orange")
    for i in range(6):
        c.set(10 + i, 7, "orange")
        c.set(10 + i, 8, "yellow" if i < 4 else "orange")
    for i in range(3):
        c.set(10 + i, 5 - i, "orange")
        c.set(10 + i, 10 + i, "orange")
    c.ellipse(9.5, 8.0, 2.4, 2.0, "yellow")
    c.ellipse(9.0, 7.5, 1.3, 1.0, "glow")
    c.set(9, 7, "white")
    return c


HIT_SPARK = 16
HIT_SPARK_FRAMES = 3


def hit_spark(frame: int) -> Canvas:
    """Sparks where a shot hits, flying towards +X (the game turns them back towards the shooter): a bright burst,
    then streaks flying apart, then a few dying embers (issue #107)."""
    c = Canvas(HIT_SPARK, HIT_SPARK)
    cx, cy = 5, 8
    if frame == 0:
        c.ellipse(cx + 0.5, cy + 0.5, 3.0, 3.0, "yellow")
        c.ellipse(cx + 0.5, cy + 0.5, 1.6, 1.6, "glow")
        for a in (-50, -20, 0, 25, 55):
            r = math.radians(a)
            for d in range(3, 6):
                c.set(cx + round(math.cos(r) * d), cy + round(math.sin(r) * d), "orange")
    else:
        reach = 6 + frame * 3
        for a in (-55, -25, 5, 30, 60):
            r = math.radians(a)
            for d in range(reach - 3, reach):
                color = "yellow" if frame == 1 and d >= reach - 2 else "orange"
                c.set(cx + round(math.cos(r) * d), cy + round(math.sin(r) * d), color if frame == 1 else "red_1")
    return c


DEATH_SPLAT = 32
DEATH_SPLAT_FRAMES = 4


def death_splat(frame: int) -> Canvas:
    """Splatter where an enemy dies (issue #107): a burst of blood drops flying outward and falling, ending in
    small stains; the corpse animation lies underneath."""
    c = Canvas(DEATH_SPLAT, DEATH_SPLAT)
    rng = random.Random(7000)
    drops = [(rng.uniform(0, 2 * math.pi), rng.uniform(0.6, 1.0), rng.choice((1.0, 1.5))) for _ in range(12)]
    t = (frame + 1) / DEATH_SPLAT_FRAMES
    if frame == 0:
        c.ellipse(16.0, 16.0, 5.0, 4.0, "red_1")
        c.ellipse(15.5, 15.5, 2.5, 2.0, "orange")
    for angle, speed, size in drops:
        d = 4.0 + 10.0 * t * speed
        x = 16 + math.cos(angle) * d
        y = 16 - math.sin(angle) * d * 0.7 + 4.0 * t * t
        color = "red_1" if frame < 2 else "red_0"
        c.ellipse(x, y, size, size * 0.8, color)
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
    """Cracked floor glowing red from below where an enemy is about to climb out (issue #103): a dim warning ring,
    squashed for the oblique view, around a star of glowing cracks with a hot centre."""
    c = Canvas(32, 32)
    c.ellipse(16.0, 16.5, 14.0, 8.5, "red_0")
    c.ellipse(16.0, 16.5, 12.5, 7.3, None)
    for x, y in [(16, 8), (16, 25), (2, 16), (29, 16)]:
        c.set(x, y, "orange")
    # Cracks: jagged strokes out from the centre.
    cracks = [[(16, 16), (13, 14), (11, 14), (8, 12)], [(16, 16), (19, 14), (21, 15), (24, 13)],
              [(16, 16), (14, 19), (12, 20), (10, 21)], [(16, 16), (18, 19), (21, 20), (23, 21)],
              [(16, 16), (16, 13), (17, 11)]]
    for crack in cracks:
        for (x0, y0), (x1, y1) in zip(crack, crack[1:]):
            steps = max(abs(x1 - x0), abs(y1 - y0))
            for i in range(steps + 1):
                x = round(x0 + (x1 - x0) * i / steps)
                y = round(y0 + (y1 - y0) * i / steps)
                c.set(x, y + 1, "red_0")
                c.set(x, y, "red_1")
    c.ellipse(16.0, 16.0, 3.0, 2.0, "orange")
    c.ellipse(16.0, 16.0, 1.6, 1.1, "yellow")
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
    the game checks (issue #103): a dashed bright blue rim on a dark band, glowing marks every 45 degrees with
    short ticks pointing inward."""
    c = Canvas(HOLD_ZONE, HOLD_ZONE)
    cx = cy = HOLD_ZONE / 2
    for y in range(HOLD_ZONE):
        for x in range(HOLD_ZONE):
            r = math.hypot(x + 0.5 - cx, y + 0.5 - cy)
            a = math.degrees(math.atan2(cy - y - 0.5, x + 0.5 - cx)) % 360.0
            if 61.0 <= r <= 63.5:
                # 24 dashes, bright with a darker gap.
                c.set(x, y, "blue_2" if (a % 15.0) < 11.0 else "blue_1")
            elif 59.0 <= r < 61.0:
                c.set(x, y, "blue_0")
    for k in range(8):
        a = math.radians(k * 45.0)
        for step in range(6):
            r = 62.5 - step
            c.set(int(cx + math.cos(a) * r), int(cy - math.sin(a) * r), "glow" if step < 3 else "blue_2")
        # Small bright marker on the rim.
        for da in (-1.2, 1.2):
            b = a + math.radians(da)
            c.set(int(cx + math.cos(b) * 62.0), int(cy - math.sin(b) * 62.0), "white")
    return c


def hold_zone_fill() -> Canvas:
    """Disc that grows inside the ring with the hold progress; drawn solid, the game makes it see-through."""
    c = Canvas(HOLD_ZONE, HOLD_ZONE)
    c.ellipse(HOLD_ZONE / 2, HOLD_ZONE / 2, 59.0, 59.0, "blue_1")
    c.ellipse(HOLD_ZONE / 2, HOLD_ZONE / 2, 55.0, 55.0, "blue_2")
    return c


def floor_switch(on: bool) -> Canvas:
    """Square pressure plate on the floor (issue #103), the same on every island. A steel frame with hazard stripes in
    the corners holds a raised button. Off: the button stands proud (front face visible) with a dim red lamp.
    On: the button is pressed flush and glows green with a bright ring and light spilling onto the frame."""
    c = Canvas(TILE, TILE)
    # Frame with a lit top edge and a dark front edge.
    c.rect(2, 3, 29, 29, "slate_1")
    c.rect(2, 3, 29, 3, "slate_3")
    c.rect(2, 29, 29, 29, "slate_0")
    c.rect(4, 5, 27, 27, "slate_0")
    # Hazard stripes in the four corners.
    for x0, y0 in ((2, 3), (25, 3), (2, 25), (25, 25)):
        for i in range(5):
            for j in range(5):
                if (i + j) % 4 < 2:
                    c.set(x0 + i, y0 + j, "yellow")
    if on:
        c.rect(6, 7, 25, 26, "slate_1")
        c.ellipse(16.0, 16.5, 10.0, 10.0, "green_0")
        c.ellipse(16.0, 16.5, 8.5, 8.5, "green_1")
        c.ellipse(16.0, 16.5, 6.5, 6.5, "green_2")
        c.ellipse(16.0, 16.5, 3.5, 3.5, "glow")
        c.set(14, 14, "white")
        # Light spilling into the frame.
        for x, y in ((16, 5), (16, 27), (4, 16), (27, 16)):
            c.set(x, y, "green_2")
    else:
        # Raised button: top face and a visible front face.
        c.rect(6, 6, 25, 22, "slate_2")
        c.rect(6, 6, 25, 6, "slate_4")
        c.rect(6, 23, 25, 25, "slate_1")
        c.rect(6, 26, 25, 26, "night")
        c.ellipse(16.0, 14.5, 6.0, 5.0, "red_0")
        c.ellipse(16.0, 14.0, 4.0, 3.2, "red_1")
        c.set(14, 12, "orange")
        c.set(15, 12, "orange")
    c.outline()
    return c


def energy_cell() -> Canvas:
    """Upright energy cell (issue #103): steel caps with terminals at both ends, a glowing green core in a glass tube
    with a bright highlight and a lightning bolt."""
    c = Canvas(16, 16)
    c.rect(6, 0, 9, 1, "slate_3")
    c.rect(4, 2, 11, 3, "slate_2")
    c.rect(4, 2, 11, 2, "slate_4")
    c.rect(4, 4, 11, 12, "green_0")
    c.rect(5, 4, 10, 12, "green_1")
    c.rect(6, 4, 9, 12, "green_2")
    c.rect(5, 5, 5, 11, "glow")
    # Lightning bolt.
    for x, y in ((9, 5), (8, 6), (7, 7), (8, 7), (9, 7), (8, 8), (7, 9), (6, 10)):
        c.set(x, y, "white")
    c.rect(4, 13, 11, 14, "slate_2")
    c.rect(4, 14, 11, 14, "slate_0")
    c.outline()
    return c


def generator(on: bool) -> Canvas:
    """Squat machine (issue #103), the same on every island: a lit top with two lamps and a vent grille, a hazard
    band, a cell slot in the front panel with a lightning sign above it and cables at the foot. Running, the
    slot, lamps and sign glow green and yellow; waiting, the lamps are red and the slot is dark."""
    c = Canvas(32, 48)
    # Top face (seen from above) and front face.
    c.rect(4, 12, 27, 19, "slate_3")
    c.rect(4, 12, 27, 12, "slate_4")
    for x in range(12, 21, 2):
        c.rect(x, 14, x, 17, "slate_1")
    c.rect(4, 20, 27, 45, "slate_1")
    c.rect(6, 22, 25, 43, "slate_2")
    # Hazard band.
    for x in range(4, 28):
        c.set(x, 20, "yellow" if (x // 2) % 2 == 0 else "night")
        c.set(x, 21, "yellow" if ((x + 1) // 2) % 2 == 0 else "night")
    # Lamps on top.
    for x in (7, 24):
        c.rect(x - 2, 7, x + 2, 12, "slate_0")
        c.rect(x - 1, 8, x + 1, 10, "green_2" if on else "red_0")
        c.set(x - 1, 8, "glow" if on else "red_1")
    # Lightning sign.
    bolt = ((17, 24), (16, 25), (15, 26), (16, 26), (17, 26), (16, 27), (15, 28))
    for x, y in bolt:
        c.set(x, y, "yellow" if on else "slate_3")
    # Cell slot.
    c.rect(11, 30, 20, 41, "night")
    if on:
        c.rect(12, 31, 19, 40, "green_1")
        c.rect(13, 31, 18, 40, "green_2")
        c.rect(14, 32, 14, 39, "glow")
        c.rect(10, 30, 10, 41, "green_2")
        c.rect(21, 30, 21, 41, "green_2")
    else:
        c.rect(12, 31, 19, 32, "slate_0")
        c.rect(12, 40, 19, 40, "slate_0")
    # Foot and cables.
    c.rect(3, 44, 28, 45, "slate_0")
    c.rect(0, 43, 3, 44, "night")
    c.rect(28, 42, 31, 43, "night")
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


def exit_pad(open_: bool = True) -> Canvas:
    """Level exit on the floor (issue #103). Open: a green field with a bright, dashed rim and three glowing
    chevrons pointing on (north). Locked: the same plate dark grey with a red rim, unlit chevrons and a red padlock
    in the middle, so it reads as "not yet" from across the screen."""
    c = Canvas(128, 48)
    rim, edge, field, chevron = (("green_2", "green_0", "green_1", "glow") if open_
                                 else ("red_1", "slate_0", "slate_1", "slate_2"))
    c.rect(2, 2, 125, 45, rim)
    # Dashes along the rim.
    for x in range(2, 126):
        if (x // 4) % 2 == 0:
            c.set(x, 2, edge if not open_ else "glow")
            c.set(x, 45, edge)
    c.rect(4, 4, 123, 43, edge)
    c.rect(6, 6, 121, 41, field)
    for cx in (32, 64, 96):
        if not open_ and cx == 64:
            continue
        # Chevron: two diagonal strokes meeting at the top.
        for i in range(10):
            for t in range(3):
                c.set(cx - 10 + i, 32 - i + t, chevron)
                c.set(cx + 10 - i, 32 - i + t, chevron)
        c.rect(cx - 1, 22, cx + 1, 24, chevron)
        if open_:
            for i in range(10):
                c.set(cx - 10 + i, 35 - i, "green_2")
                c.set(cx + 10 - i, 35 - i, "green_2")
    if not open_:
        # Padlock.
        c.rect(58, 13, 59, 21, "red_1")
        c.rect(68, 13, 69, 21, "red_1")
        c.rect(60, 11, 67, 12, "red_1")
        c.rect(55, 21, 72, 36, "red_1")
        c.rect(57, 23, 70, 34, "red_0")
        c.rect(63, 26, 64, 31, "night")
        c.set(57, 23, "orange")
        c.set(58, 23, "orange")
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


# --- Island 1: beach (issue #104) ------------------------------------------------
# Dark, desaturated sand and rock so the bright characters stand out (G1); the palm-green hedge on the cliffs and
# the shells give the island its colour. Every tile wraps at its edges so it tiles in all directions.
GATE_FRAMES = 5


def wrap_set(c: Canvas, x: int, y: int, color: str) -> None:
    c.set(x % c.width, y % c.height, color)


def sand(variant: int) -> Canvas:
    """Beach sand. Variant 0 plain, 1 with wind ripples, 2 with pebbles and a shell fragment."""
    c = Canvas(TILE, TILE, fill="brown_1")
    rng = random.Random(4100 + variant)
    for _ in range(36):
        wrap_set(c, rng.randrange(TILE), rng.randrange(TILE), "brown_0")
    for _ in range(22):
        wrap_set(c, rng.randrange(TILE), rng.randrange(TILE), "skin_0")
    for _ in range(4):
        wrap_set(c, rng.randrange(TILE), rng.randrange(TILE), "clay")
    if variant == 1:
        for row, y0 in enumerate((7, 17, 27)):
            for x in range(TILE):
                y = y0 + round(1.5 * math.sin((x + row * 9) / TILE * 2 * math.pi * 2))
                wrap_set(c, x, y, "brown_0")
                wrap_set(c, x, y - 1, "skin_0")
    elif variant == 2:
        for x, y in ((8, 9), (22, 21), (25, 7)):
            c.rect(x, y, x + 1, y + 1, "slate_2")
            c.set(x, y, "slate_3")
        for x, y in ((14, 18), (15, 18), (16, 19), (14, 19)):
            c.set(x, y, "peach")
        c.set(15, 19, "cream")
    return c


def cliff_top_face(c: Canvas, y0: int, y1: int) -> None:
    """Dense palm hedge on top of the cliff, seen from above."""
    c.rect(0, y0, TILE - 1, y1, "green_0")
    rng = random.Random(4200)
    for _ in range(40):
        x, y = rng.randrange(TILE), rng.randrange(TILE)
        if y0 <= y <= y1:
            wrap_set(c, x, y, "green_1")
            if y + 1 <= y1:
                wrap_set(c, x + 1, y + 1, "green_1")
    for _ in range(18):
        x, y = rng.randrange(TILE), rng.randrange(TILE)
        if y0 <= y <= y1:
            wrap_set(c, x, y, "night")


def cliff_top() -> Canvas:
    c = Canvas(TILE, TILE)
    cliff_top_face(c, 0, TILE - 1)
    return c


def cliff_front() -> Canvas:
    """Cliff with nothing to the south: hedge on top, a dark rock face with layers below."""
    c = Canvas(TILE, TILE)
    top = 10
    cliff_top_face(c, 0, top - 1)
    # Leaves hanging over the edge.
    for x in range(0, TILE, 3):
        c.set(x, top, "green_0")
        c.set(x + 1, top, "green_1" if x % 2 else "green_0")
    c.rect(0, top + 1, TILE - 1, TILE - 1, "slate_1")
    rng = random.Random(4300)
    for y in range(top + 4, TILE - 2, 5):
        for x in range(TILE):
            if rng.random() < 0.85:
                c.set(x, y, "slate_0")
            if rng.random() < 0.3:
                c.set(x, y - 1, "slate_2")
    for _ in range(10):
        c.set(rng.randrange(TILE), rng.randrange(top + 2, TILE - 2), "brown_0")
    c.rect(0, TILE - 2, TILE - 1, TILE - 1, "night")
    return c


def crate_top_face(c: Canvas, y0: int, y1: int) -> None:
    """Lid of a wooden crate seen from above: planks with dark seams and a dark rim."""
    c.rect(0, y0, TILE - 1, y1, "brown_1")
    for x in range(0, TILE, 8):
        c.rect(x, y0, x, y1, "brown_0")
    for x in range(3, TILE, 8):
        c.rect(x, y0 + 1, x, y1 - 1, "clay")
    c.rect(0, y0, TILE - 1, y0, "brown_0")
    c.rect(0, y0, 0, y1, "brown_0")
    c.rect(TILE - 1, y0, TILE - 1, y1, "brown_0")
    for x, y in ((2, y0 + 2), (TILE - 3, y0 + 2), (2, y1 - 1), (TILE - 3, y1 - 1)):
        c.set(x, y, "slate_3")


def crate_top() -> Canvas:
    c = Canvas(TILE, TILE)
    crate_top_face(c, 0, TILE - 1)
    c.rect(0, TILE - 1, TILE - 1, TILE - 1, "brown_0")
    return c


def crate_front() -> Canvas:
    """Crate with nothing to the south: lid above, the front with a cross brace below."""
    c = Canvas(TILE, TILE)
    top = 12
    crate_top_face(c, 0, top - 1)
    c.rect(0, top, TILE - 1, TILE - 1, "brown_0")
    c.rect(2, top + 1, TILE - 3, TILE - 3, "brown_1")
    for i in range(TILE - 4):
        y = top + 1 + round(i * (TILE - 4 - top) / (TILE - 5))
        c.set(2 + i, y, "brown_0")
        c.set(TILE - 3 - i, y, "brown_0")
    c.rect(2, top + 1, TILE - 3, top + 1, "clay")
    c.rect(0, TILE - 2, TILE - 1, TILE - 1, "night")
    return c


def beach_decoration(kind: int) -> Canvas:
    """Flat things on the sand, without collision: 0 shell, 1 starfish, 2 seaweed, 3 pebbles, 4 driftwood."""
    c = Canvas(TILE, TILE)
    if kind == 0:
        c.ellipse(16.0, 17.0, 4.0, 3.0, "peach")
        for x in (13, 15, 17, 19):
            c.rect(x, 15, x, 19, "cream")
        c.rect(15, 20, 17, 20, "clay")
    elif kind == 1:
        for a in range(5):
            ang = math.radians(90 + a * 72)
            for r in range(5):
                x = 16 + round(math.cos(ang) * r)
                y = 16 - round(math.sin(ang) * r * 0.7)
                c.set(x, y, "apricot")
                if r < 3:
                    c.set(x + 1, y, "apricot")
        c.set(16, 16, "clay")
        c.set(16, 13, "peach")
    elif kind == 2:
        for i, (x, y) in enumerate(((10, 18), (13, 16), (16, 18), (19, 15), (22, 18))):
            for k in range(5):
                c.set(x + (k % 2), y - k + 2, "green_0" if k % 2 else "green_1")
        c.rect(10, 20, 23, 20, "green_0")
    elif kind == 3:
        for x, y, r in ((12, 16, 2.0), (18, 18, 2.5), (21, 14, 1.5)):
            c.ellipse(x, y, r, r * 0.7, "slate_2")
            c.set(int(x) - 1, int(y) - 1, "slate_4")
    elif kind == 4:
        c.rect(9, 17, 23, 18, "brown_1")
        c.rect(9, 18, 23, 18, "brown_0")
        c.rect(19, 15, 21, 16, "brown_1")
        c.set(12, 17, "skin_0")
        c.set(18, 17, "skin_0")
    c.outline("outline")
    return c


BEACH_DECORATIONS = 5
# Folders under Tiles/ with an island tileset.
ISLAND_TILESETS = ["Strand", "Stadt", "Vulkan"]


def palisade(frame: int) -> Canvas:
    """One cell of the beach gate: sharpened logs lashed with rope. Opening (frames 1..GATE_FRAMES-1), the logs sink
    into the sand, throwing up little puffs, until only a low mound is left."""
    c = Canvas(TILE, TILE)
    ground = TILE - 3
    sink = round(frame / (GATE_FRAMES - 1) * 26)
    for i, x0 in enumerate(range(0, TILE, 8)):
        top = 2 + (i % 2) * 2 + sink
        for y in range(top, ground + 1):
            for x in range(x0 + 1, x0 + 7):
                if y < top + 3:
                    # Pointed tip.
                    if abs(x - (x0 + 3.5)) > (y - top + 0.5) * 1.2:
                        continue
                color = "brown_1"
                if x == x0 + 1:
                    color = "clay"
                elif x >= x0 + 5:
                    color = "brown_0"
                c.set(x, y, color)
        # Rope band.
        rope = 12 + sink
        if rope < ground:
            c.rect(x0 + 1, rope, x0 + 6, rope + 1, "peach")
            c.set(x0 + 3, rope + 1, "cream")
    # Sand at the foot; puffs while sinking.
    c.rect(0, ground, TILE - 1, TILE - 1, "skin_0")
    c.rect(0, TILE - 1, TILE - 1, TILE - 1, "brown_0")
    if 0 < frame < GATE_FRAMES - 1:
        rng = random.Random(4400 + frame)
        for _ in range(10):
            x, y = rng.randrange(TILE), rng.randrange(ground - 5, ground)
            c.set(x, y, "skin_0")
    if frame == GATE_FRAMES - 1:
        c.rect(0, ground - 2, TILE - 1, ground - 1, "skin_0")
        for x in range(2, TILE, 8):
            c.rect(x, ground - 3, x + 3, ground - 2, "brown_1")
    c.outline("outline")
    return c


# --- Island 2: dark city (issue #105) ---------------------------------------------
# Cold navy asphalt and brick at night, mauve containers; the cold neon blue is kept for small highlights.

def asphalt(variant: int) -> Canvas:
    """Grey asphalt (light enough that the dark navy player stands out). Variant 0 plain, 1 cracked, 2 patched."""
    c = Canvas(TILE, TILE, fill="slate_2")
    rng = random.Random(5100 + variant)
    for _ in range(40):
        wrap_set(c, rng.randrange(TILE), rng.randrange(TILE), "slate_1")
    for _ in range(14):
        wrap_set(c, rng.randrange(TILE), rng.randrange(TILE), "dusk")
    for _ in range(3):
        wrap_set(c, rng.randrange(TILE), rng.randrange(TILE), "slate_3")
    if variant == 1:
        x, y = 4, 20
        for step in range(26):
            c.set(x, y, "slate_0")
            c.set(x, y + 1, "slate_3")
            x += 1
            y += rng.choice((-1, 0, 0, 1))
            y = min(max(y, 6), 26)
            if step == 12:
                for k in range(5):
                    c.set(x + k // 2, y - k, "slate_0")
    elif variant == 2:
        # A darker patch of newer asphalt with a seam around it.
        c.rect(7, 9, 24, 22, "slate_0")
        c.rect(8, 10, 23, 21, "slate_1")
        for _ in range(16):
            c.set(rng.randrange(8, 24), rng.randrange(10, 22), "slate_0")
    return c


def brick_top_face(c: Canvas, y0: int, y1: int) -> None:
    """Dark gravel roof of a city building, seen from above."""
    c.rect(0, y0, TILE - 1, y1, "navy_1")
    rng = random.Random(5200)
    for _ in range(30):
        x, y = rng.randrange(TILE), rng.randrange(TILE)
        if y0 <= y <= y1:
            wrap_set(c, x, y, "navy_0")
    for _ in range(10):
        x, y = rng.randrange(TILE), rng.randrange(TILE)
        if y0 <= y <= y1:
            wrap_set(c, x, y, "dusk")


def brick_top() -> Canvas:
    c = Canvas(TILE, TILE)
    brick_top_face(c, 0, TILE - 1)
    return c


def brick_front() -> Canvas:
    """City wall with nothing to the south: concrete cap above, dark brick below with a strip of cold neon."""
    c = Canvas(TILE, TILE)
    top = 9
    brick_top_face(c, 0, top - 1)
    c.rect(0, top - 1, TILE - 1, top - 1, "slate_2")
    c.rect(0, top, TILE - 1, TILE - 1, "dusk")
    for row, y in enumerate(range(top, TILE, 4)):
        c.rect(0, y + 3, TILE - 1, y + 3, "navy_1")
        offset = 0 if row % 2 == 0 else 4
        for x in range(offset, TILE, 8):
            c.rect(x, y, x, min(y + 2, TILE - 1), "navy_1")
    rng = random.Random(5300)
    for _ in range(8):
        c.set(rng.randrange(TILE), rng.randrange(top, TILE - 3), "mauve")
    # Neon strip under the cap.
    c.rect(0, top + 1, TILE - 1, top + 1, "blue_2")
    c.rect(0, top + 2, TILE - 1, top + 2, "navy_1")
    c.rect(0, TILE - 2, TILE - 1, TILE - 1, "night")
    return c


def container_top_face(c: Canvas, y0: int, y1: int) -> None:
    """Roof of a shipping container: corrugated ribs across."""
    c.rect(0, y0, TILE - 1, y1, "mauve")
    for x in range(1, TILE, 4):
        c.rect(x, y0, x, y1, "dusk")
    c.rect(0, y0, 0, y1, "navy_1")
    c.rect(TILE - 1, y0, TILE - 1, y1, "navy_1")


def container_top() -> Canvas:
    c = Canvas(TILE, TILE)
    container_top_face(c, 0, TILE - 1)
    return c


def container_front() -> Canvas:
    """Container with nothing to the south: roof above, the corrugated door side with locking bars below."""
    c = Canvas(TILE, TILE)
    top = 10
    container_top_face(c, 0, top - 1)
    c.rect(0, top - 1, TILE - 1, top - 1, "white")
    c.rect(0, top, TILE - 1, TILE - 1, "dusk")
    for x in range(0, TILE, 3):
        c.rect(x, top, x, TILE - 3, "navy_1")
    for x in (9, 22):
        c.rect(x, top + 1, x, TILE - 4, "slate_3")
        c.rect(x - 1, top + 8, x + 1, top + 9, "slate_3")
    c.rect(0, TILE - 2, TILE - 1, TILE - 1, "night")
    return c


def city_decoration(kind: int) -> Canvas:
    """Flat things on the asphalt: 0 puddle, 1 newspaper, 2 broken bricks, 3 glass shards, 4 drain grate."""
    c = Canvas(TILE, TILE)
    if kind == 0:
        c.ellipse(16.0, 17.0, 7.0, 3.5, "navy_0")
        c.rect(12, 16, 15, 16, "blue_2")
        c.set(19, 18, "slate_3")
    elif kind == 1:
        c.rect(11, 14, 21, 20, "slate_4")
        c.rect(11, 14, 21, 14, "white")
        for y in (16, 18):
            c.rect(13, y, 19, y, "slate_2")
        c.set(21, 20, "slate_3")
    elif kind == 2:
        for x, y in ((11, 17), (16, 15), (19, 19)):
            c.rect(x, y, x + 3, y + 1, "dusk")
            c.rect(x, y, x + 3, y, "mauve")
    elif kind == 3:
        for x, y in ((12, 16), (16, 18), (19, 15), (14, 20)):
            c.set(x, y, "blue_2")
            c.set(x + 1, y + 1, "slate_3")
    elif kind == 4:
        c.rect(10, 13, 21, 20, "slate_2")
        c.rect(11, 14, 20, 19, "night")
        for x in range(12, 20, 2):
            c.rect(x, 14, x, 19, "slate_1")
    c.outline("outline")
    return c


CITY_DECORATIONS = 5


def roller_door(frame: int) -> Canvas:
    """One cell of the city gate: a steel roller shutter under its housing. Opening, the slats roll up into the
    housing from the bottom until only the housing is left."""
    c = Canvas(TILE, TILE)
    housing = 7
    bottom = TILE - 2
    lowest = bottom - round(frame / (GATE_FRAMES - 1) * (bottom - housing))
    for y in range(housing, lowest + 1):
        c.rect(1, y, TILE - 2, y, "slate_3" if (y - housing) % 3 else "slate_2")
    if lowest > housing:
        # Bottom rail with hazard stripes.
        for x in range(1, TILE - 1):
            c.set(x, lowest, "yellow" if (x // 3) % 2 == 0 else "night")
    # Guide rails at the sides and the housing on top.
    c.rect(0, housing, 0, bottom, "slate_1")
    c.rect(TILE - 1, housing, TILE - 1, bottom, "slate_1")
    c.rect(0, 0, TILE - 1, housing - 1, "navy_1")
    c.rect(0, 0, TILE - 1, 0, "slate_2")
    c.rect(0, housing - 1, TILE - 1, housing - 1, "navy_0")
    c.set(3, 3, "blue_2" if frame == GATE_FRAMES - 1 else "red_1")
    c.outline("outline")
    return c


# --- Island 3: volcano (issue #106) ----------------------------------------------
# Brown ash ground and black basalt; lava glows only in cracks and pools that are decoration (no damage).

def ash_ground(variant: int) -> Canvas:
    """Ash-covered rock. Variant 0 plain, 1 with a glowing crack, 2 with grey ash drifts."""
    c = Canvas(TILE, TILE, fill="brown_0")
    rng = random.Random(6100 + variant)
    for _ in range(34):
        wrap_set(c, rng.randrange(TILE), rng.randrange(TILE), "brown_1")
    for _ in range(26):
        wrap_set(c, rng.randrange(TILE), rng.randrange(TILE), "night")
    for _ in range(2):
        wrap_set(c, rng.randrange(TILE), rng.randrange(TILE), "red_0")
    if variant == 1:
        x, y = 5, 9
        for step in range(22):
            c.set(x, y, "night")
            c.set(x, y + 1, "red_0" if step % 4 else "orange")
            x += 1
            y += rng.choice((0, 0, 1))
            y = min(y, 25)
    elif variant == 2:
        for cx, cy in ((10, 12), (22, 20)):
            c.ellipse(cx, cy, 5.0, 2.5, "slate_2")
            c.ellipse(cx - 1, cy - 0.5, 3.0, 1.2, "slate_3")
    return c


def basalt_top_face(c: Canvas, y0: int, y1: int) -> None:
    """Tops of basalt columns seen from above: dark hexagons with lit edges."""
    c.rect(0, y0, TILE - 1, y1, "night")
    for cx, cy in ((8, 8), (24, 8), (16, 22), (0, 22), (32, 22)):
        for y in range(cy - 6, cy + 7):
            for x in range(cx - 7, cx + 8):
                if y0 <= y <= y1 and 0 <= x < TILE and abs(x - cx) + abs(y - cy) * 0.6 <= 6.5:
                    edge = abs(x - cx) + abs(y - cy) * 0.6 > 5.3
                    c.set(x, y, "slate_1" if edge and y < cy else "slate_0")


def basalt_top() -> Canvas:
    c = Canvas(TILE, TILE)
    basalt_top_face(c, 0, TILE - 1)
    return c


def basalt_front() -> Canvas:
    """Basalt cliff with nothing to the south: column tops above, vertical columns below, glowing at the foot."""
    c = Canvas(TILE, TILE)
    top = 10
    basalt_top_face(c, 0, top - 1)
    c.rect(0, top, TILE - 1, TILE - 1, "slate_0")
    for x in range(0, TILE, 8):
        c.rect(x, top, x, TILE - 3, "night")
        c.rect(x + 1, top, x + 1, TILE - 3, "slate_1")
    c.rect(0, TILE - 3, TILE - 1, TILE - 3, "red_0")
    c.rect(0, TILE - 2, TILE - 1, TILE - 1, "night")
    return c


def lava_rock_top_face(c: Canvas, y0: int, y1: int) -> None:
    """Cooled lava block from above: crusty brown rock with faint glowing seams."""
    c.rect(0, y0, TILE - 1, y1, "brown_1")
    rng = random.Random(6200)
    for _ in range(40):
        x, y = rng.randrange(TILE), rng.randrange(TILE)
        if y0 <= y <= y1:
            wrap_set(c, x, y, "brown_0")
    for x0, y0_, x1 in ((3, 6, 12), (18, 18, 28)):
        for x in range(x0, x1):
            yy = y0_ + (x % 3 == 0)
            if y0 <= yy <= y1:
                c.set(x, yy, "red_0")
    c.rect(0, y0, TILE - 1, y0, "brown_0")


def lava_rock_top() -> Canvas:
    c = Canvas(TILE, TILE)
    lava_rock_top_face(c, 0, TILE - 1)
    return c


def lava_rock_front() -> Canvas:
    c = Canvas(TILE, TILE)
    top = 11
    lava_rock_top_face(c, 0, top - 1)
    c.rect(0, top, TILE - 1, TILE - 1, "brown_0")
    rng = random.Random(6300)
    for _ in range(30):
        c.set(rng.randrange(TILE), rng.randrange(top, TILE - 2), "night")
    for x in range(4, TILE, 9):
        for y in range(top + 3, TILE - 4):
            if (x + y) % 5 == 0:
                c.set(x, y, "orange")
                c.set(x + 1, y + 1, "red_0")
    c.rect(0, TILE - 2, TILE - 1, TILE - 1, "night")
    return c


def volcano_decoration(kind: int) -> Canvas:
    """Flat things on the ash, without collision and without damage: 0 small lava pool, 1 lava crack, 2 embers,
    3 ash pile, 4 charred bones."""
    c = Canvas(TILE, TILE)
    if kind == 0:
        c.ellipse(16.0, 17.0, 7.0, 4.0, "red_0")
        c.ellipse(16.0, 16.8, 5.5, 3.0, "orange")
        c.ellipse(15.0, 16.5, 3.0, 1.5, "yellow")
        c.set(13, 16, "glow")
    elif kind == 1:
        x, y = 8, 14
        for step in range(16):
            c.set(x, y, "red_0")
            c.set(x, y + 1, "orange" if step % 3 else "yellow")
            x += 1
            y += (0, 1, 0, -1)[step % 4]
    elif kind == 2:
        for x, y in ((12, 16), (16, 18), (19, 15), (14, 20), (21, 19)):
            c.set(x, y, "orange")
            c.set(x + 1, y, "red_0")
        c.set(16, 17, "yellow")
    elif kind == 3:
        c.ellipse(16.0, 18.0, 6.0, 3.0, "slate_2")
        c.ellipse(15.0, 17.0, 3.5, 1.8, "slate_3")
    elif kind == 4:
        c.rect(10, 17, 20, 18, "slate_4")
        c.rect(9, 16, 10, 19, "slate_5")
        c.rect(20, 16, 21, 19, "slate_5")
        c.set(15, 17, "slate_2")
    c.outline("outline")
    return c


VOLCANO_DECORATIONS = 5


def steel_grate(frame: int) -> Canvas:
    """One cell of the volcano gate: a heavy steel lattice glowing red-hot at its foot. Opening, it lowers into a
    glowing slot in the ground until only the slot is left."""
    c = Canvas(TILE, TILE)
    slot = TILE - 4
    drop = round(frame / (GATE_FRAMES - 1) * (slot - 2))
    top = 2 + drop
    for y in range(top, slot):
        for x in range(1, TILE - 1):
            bar = x % 6 in (0, 1)
            cross = (y - top) % 8 in (0, 1)
            if bar or cross:
                lit = x % 6 == 0 or (y - top) % 8 == 0
                hot = y >= slot - 4
                c.set(x, y, ("orange" if lit else "red_1") if hot else ("slate_3" if lit else "slate_1"))
    if top < slot:
        c.rect(1, top, TILE - 2, top, "slate_4")
    c.rect(0, slot, TILE - 1, TILE - 1, "night")
    c.rect(1, slot + 1, TILE - 2, slot + 1, "red_0" if frame < GATE_FRAMES - 1 else "orange")
    c.rect(1, slot + 2, TILE - 2, slot + 2, "red_0")
    c.outline("outline")
    return c


def island_files(files: dict[str, Image.Image]) -> None:
    """Tiles and gate frames of the islands (issues #104 to #106), under Tiles/<Island>/."""
    for variant in range(3):
        files[f"Tiles/Strand/Floor_{variant}.png"] = sand(variant).to_image()
    files["Tiles/Strand/Wall_Top.png"] = cliff_top().to_image()
    files["Tiles/Strand/Wall_Front.png"] = cliff_front().to_image()
    files["Tiles/Strand/Obstacle_Top.png"] = crate_top().to_image()
    files["Tiles/Strand/Obstacle_Front.png"] = crate_front().to_image()
    for kind in range(BEACH_DECORATIONS):
        files[f"Tiles/Strand/Decor_{kind}.png"] = beach_decoration(kind).to_image()
    for frame in range(GATE_FRAMES):
        files[f"Tiles/Strand/Gate_{frame}.png"] = palisade(frame).to_image()
    for variant in range(3):
        files[f"Tiles/Stadt/Floor_{variant}.png"] = asphalt(variant).to_image()
    files["Tiles/Stadt/Wall_Top.png"] = brick_top().to_image()
    files["Tiles/Stadt/Wall_Front.png"] = brick_front().to_image()
    files["Tiles/Stadt/Obstacle_Top.png"] = container_top().to_image()
    files["Tiles/Stadt/Obstacle_Front.png"] = container_front().to_image()
    for kind in range(CITY_DECORATIONS):
        files[f"Tiles/Stadt/Decor_{kind}.png"] = city_decoration(kind).to_image()
    for frame in range(GATE_FRAMES):
        files[f"Tiles/Stadt/Gate_{frame}.png"] = roller_door(frame).to_image()
    for variant in range(3):
        files[f"Tiles/Vulkan/Floor_{variant}.png"] = ash_ground(variant).to_image()
    files["Tiles/Vulkan/Wall_Top.png"] = basalt_top().to_image()
    files["Tiles/Vulkan/Wall_Front.png"] = basalt_front().to_image()
    files["Tiles/Vulkan/Obstacle_Top.png"] = lava_rock_top().to_image()
    files["Tiles/Vulkan/Obstacle_Front.png"] = lava_rock_front().to_image()
    for kind in range(VOLCANO_DECORATIONS):
        files[f"Tiles/Vulkan/Decor_{kind}.png"] = volcano_decoration(kind).to_image()
    for frame in range(GATE_FRAMES):
        files[f"Tiles/Vulkan/Gate_{frame}.png"] = steel_grate(frame).to_image()


def island_preview(files: dict[str, Image.Image], island: str, path: Path, scale: int = 3) -> None:
    """A small arena in the island's tiles with an obstacle, decorations, the gate frames and a few characters."""
    cols, rows = 11, 11
    sheet = Image.new("RGBA", (cols * TILE, rows * TILE), (0, 0, 0, 255))
    def wall(x, y):
        return x in (0, cols - 1) or y in (0, rows - 1)
    def obstacle(x, y):
        return 3 <= x <= 5 and 3 <= y <= 4
    for y in range(rows):
        for x in range(cols):
            # Image y grows downwards; the cell to the south is y + 1.
            if wall(x, y):
                name = "Wall_Top" if wall(x, y + 1) or y == rows - 1 else "Wall_Front"
            elif obstacle(x, y):
                name = "Obstacle_Top" if obstacle(x, y + 1) else "Obstacle_Front"
            else:
                name = f"Floor_{[0, 0, 0, 1, 2][(x * 7 + y * 3) % 5]}"
            sheet.paste(files[f"Tiles/{island}/{name}.png"], (x * TILE, y * TILE))
    decor = [k for k in files if k.startswith(f"Tiles/{island}/Decor_")]
    for i, key in enumerate(sorted(decor)):
        sheet.alpha_composite(files[key], ((2 + i * 2) * TILE, 8 * TILE))
    for frame in range(GATE_FRAMES):
        sheet.alpha_composite(files[f"Tiles/{island}/Gate_{frame}.png"], ((1 + frame * 2) * TILE, 6 * TILE))
    for i, (key, x) in enumerate((("Walker_Walk_SE_0", 7), ("Runner_Run_SW_1", 8), ("Boomer_Walk_S_0", 9))):
        img = files[f"Sprites/Enemies/{key}.png"]
        sheet.alpha_composite(img, (x * TILE + TILE // 2 - img.width // 2, 4 * TILE - img.height + FOOT_MARGIN))
    player = ART_DIR / "Sprites/Player/Player_Idle_S.png"
    if player.exists():
        img = Image.open(player).convert("RGBA")
        sheet.alpha_composite(img, (2 * TILE + TILE // 2 - 32, 3 * TILE - 64 + FOOT_MARGIN))
    sheet.resize((sheet.width * scale, sheet.height * scale), Image.NEAREST).save(path)


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
        **{f"Sprites/Effects/HitSpark_{f}.png": hit_spark(f).to_image() for f in range(HIT_SPARK_FRAMES)},
        **{f"Sprites/Effects/DeathSplat_{f}.png": death_splat(f).to_image() for f in range(DEATH_SPLAT_FRAMES)},
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
        "Sprites/Level/Exit_Pad.png": exit_pad(True).to_image(),
        "Sprites/Level/Exit_Pad_Locked.png": exit_pad(False).to_image(),
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
        for frame in range(len(HIT_POSES)):
            files[f"Sprites/Enemies/Walker_Hit_{name}_{frame}.png"] = walker_hit(d, frame).to_image()
            files[f"Sprites/Enemies/Runner_Hit_{name}_{frame}.png"] = runner_hit(d, frame).to_image()
        for enemy, draw, count in (("Walker", walker_death, len(DEATH_STAIN)), ("Runner", runner_death, len(DEATH_STAIN)),
                                   ("Brute", brute_death, len(BRUTE_DEATH_STAIN))):
            for frame, c in enumerate(death_frames(draw, d, count)):
                files[f"Sprites/Enemies/{enemy}_Death_{name}_{frame}.png"] = c.to_image()
        for frame in range(len(HIT_POSES)):
            files[f"Sprites/Enemies/Brute_Hit_{name}_{frame}.png"] = brute_hit(d, frame).to_image()
            files[f"Sprites/Enemies/Boomer_Hit_{name}_{frame}.png"] = boomer_hit(d, frame).to_image()
        for frame in range(4):
            files[f"Sprites/Enemies/Boomer_Swell_{name}_{frame}.png"] = boomer_swell(d, frame).to_image()
        for frame in range(BOOMER_DEATH_FRAMES):
            files[f"Sprites/Enemies/Boomer_Death_{name}_{frame}.png"] = boomer_death(d, frame).to_image()
        for frame in range(4):
            files[f"Sprites/Enemies/Walker_Walk_{name}_{frame}.png"] = walker_frame(d, frame).to_image()
            files[f"Sprites/Enemies/Runner_Run_{name}_{frame}.png"] = runner_frame(d, frame).to_image()
            files[f"Sprites/Enemies/Brute_Walk_{name}_{frame}.png"] = brute_frame(d, frame).to_image()
            files[f"Sprites/Enemies/Boomer_Walk_{name}_{frame}.png"] = boomer_frame(d, frame).to_image()
    for frame in range(EXPLOSION_FRAMES):
        files[f"Sprites/Effects/Explosion_{frame}.png"] = explosion_frame(frame).to_image()
    for variant in range(3):
        files[f"Tiles/Floor_{variant}.png"] = floor(variant).to_image()
    island_files(files)
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


# Animation previews (issue #100): which states each enemy has, in playback order, with their frame rates.
# The frame rates match the DirectionalAnimation assets the editor builder creates.
ANIMATION_PREVIEWS = {
    "Walker": [("Walk", 8, 2), ("Hit", 12, 1), ("Death", 10, 1)],
    "Runner": [("Run", 8, 3), ("Hit", 12, 1), ("Death", 10, 1)],
    "Brute": [("Walk", 8, 2), ("Hit", 12, 1), ("Death", 10, 1)],
    "Boomer": [("Walk", 8, 2), ("Hit", 12, 1), ("Swell", 12, 4), ("Death", 10, 1)],
}
# Pause on the last death frame before the preview loops, in milliseconds.
PREVIEW_HOLD_MS = 800


def animation_preview(files: dict[str, Image.Image], name: str, out_dir: Path, scale: int = 4) -> None:
    """Writes <name>.gif (8 directions side by side, playing every state) and <name>_Sheet.png (one row per
    direction, all frames of all states) to out_dir."""
    states = ANIMATION_PREVIEWS[name]
    frames_of = {}
    for state, _, _ in states:
        frames_of[state] = []
        for direction in DIRECTIONS:
            row = []
            for i in range(99):
                key = f"Sprites/Enemies/{name}_{state}_{direction}_{i}.png"
                if key not in files:
                    break
                row.append(files[key])
            frames_of[state].append(row)
    size = files[f"Sprites/Enemies/{name}_{states[0][0]}_E_0.png"].width
    background = PALETTE["slate_0"] + (255,)

    gif_frames, durations = [], []
    for state, fps, repeats in states:
        count = len(frames_of[state][0])
        for _ in range(repeats):
            for i in range(count):
                img = Image.new("RGBA", (size * 8, size), background)
                for d in range(8):
                    img.alpha_composite(frames_of[state][d][i], (d * size, 0))
                gif_frames.append(img.resize((img.width * scale, img.height * scale), Image.NEAREST).convert("RGB"))
                durations.append(round(1000 / fps))
    durations[-1] = PREVIEW_HOLD_MS
    out_dir.mkdir(parents=True, exist_ok=True)
    gif_frames[0].save(out_dir / f"{name}.gif", save_all=True, append_images=gif_frames[1:], duration=durations,
                       loop=0, disposal=1)

    columns = sum(len(frames_of[state][0]) for state, _, _ in states)
    sheet = Image.new("RGBA", (columns * size, 8 * size), background)
    for d in range(8):
        x = 0
        for state, _, _ in states:
            for img in frames_of[state][d]:
                sheet.alpha_composite(img, (x, d * size))
                x += size
    sheet.resize((sheet.width * scale, sheet.height * scale), Image.NEAREST).save(out_dir / f"{name}_Sheet.png")


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--preview", type=Path, help="write an upscaled contact sheet to this path")
    parser.add_argument("--islands", type=Path, help="write one upscaled preview per island tileset to this folder")
    parser.add_argument("--animations", type=Path, help="write upscaled animation previews (GIF + sheet) to this folder")
    args = parser.parse_args()

    check_palette_groups()
    files = outputs()
    for rel, img in files.items():
        assert_in_palette(img, rel)
        out = ART_DIR / rel
        out.parent.mkdir(parents=True, exist_ok=True)
        img.save(out)
        print(f"Wrote {out.relative_to(ROOT)}")

    sheet_path = ROOT / "docs/art/palette.png"
    sheet_path.parent.mkdir(parents=True, exist_ok=True)
    palette_sheet().save(sheet_path)
    print(f"Wrote {sheet_path.relative_to(ROOT)}")

    metrics_path = ART_DIR / "Fonts/PixelFont.json"
    metrics_path.write_text(json.dumps(font_atlas()[1], indent=1) + "\n")
    print(f"Wrote {metrics_path.relative_to(ROOT)}")

    if args.preview:
        preview(files, args.preview)
        print(f"Wrote preview {args.preview}")

    if args.islands:
        args.islands.mkdir(parents=True, exist_ok=True)
        for island in ISLAND_TILESETS:
            island_preview(files, island, args.islands / f"{island}.png")
            print(f"Wrote island preview {args.islands / island}.png")

    if args.animations:
        for name in ANIMATION_PREVIEWS:
            animation_preview(files, name, args.animations)
            print(f"Wrote animation preview {args.animations / name}.gif")


if __name__ == "__main__":
    main()
