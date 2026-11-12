#!/usr/bin/env python3
"""Intro cutscene of RushOps (issue #332, F-STO-01, F-STO-02, docs/story.md).

Draws the seven intro panels of the storyboard (docs/art/cutscene-intro/) at 180x320 without text, from the game's
own sprites (player, walker) and the project palette, and writes the German texts and the box layout to
Intro_Cutscene.json next to them. Unity draws the boxes (CutscenePlayer), so they can fade in one after another;
"RushOps -> Story -> Intro-Cutscene aktualisieren" turns the JSON into Data/Story/IntroCutscene.asset.

Rules from the storyboard: the Toke always glows gold; the man gets wings and horns only in the last panel; in the
showdown the hero stands on top and speaks first.

    python3 Tools/gen_cutscenes.py                    # writes Assets/_Project/Art/Cutscenes/Intro/
    python3 Tools/gen_cutscenes.py --preview docs/art/cutscene-intro   # plus intro_preview.png with the boxes
"""
import argparse
import json
import random
import sys
from pathlib import Path

from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / "Tools"))
import pixel_font as pf  # noqa: E402

SPRITES = ROOT / "Assets/_Project/Art/Sprites"
OUT_DIR = ROOT / "Assets/_Project/Art/Cutscenes/Intro"
PALETTE_FILE = ROOT / "Assets/_Project/Art/Palettes/RushOps_Palette.png"
W, H = 180, 320

# Box metrics, the same as CutsceneConfig and the pixel font in Unity.
BOX_PADDING = 4
SPEAKER_LINE = 10
LINE_HEIGHT = pf.LINE_HEIGHT
BOX_GAP = 4
BOX_X = 4
BOX_W = W - 2 * BOX_X
# Keep the bottom strip free: „Überspringen“ sits there.
BOTTOM_FREE = 18


def c(hex_color):
    return tuple(int(hex_color[i:i + 2], 16) for i in (0, 2, 4)) + (255,)


K, K2, K3 = c("0d0b14"), c("1f1d2b"), c("2e2c3d")
G1, G2, G3, G4 = c("3b3a4f"), c("4d4c63"), c("6b6a80"), c("8f8ea3")
RED, DRED = c("c8323c"), c("7a1f2b")
GRN, LGRN, DGRN = c("4f7a3a"), c("8db254"), c("2c4a2a")
YEL, LYEL, ORG = c("f6d04d"), c("fff6c2"), c("e8862f")
CRM, WHT = c("ffecd6"), c("f4f1e8")
BLU, LBLU, DBLU, MBLU = c("2f6ea8"), c("5fb0e0"), c("0d2b45"), c("1e3f66")
BRN, DBRN = c("7d5e46"), c("4a342c")
SKIN, SKIN2, SKIN3 = c("d39a6a"), c("d08159"), c("8a5a3c")
SLATE = c("544e68")

HERO_COLOR = "2f6ea8"
VILLAIN_COLOR = "c8323c"


def sprite(path):
    return Image.open(SPRITES / path).convert("RGBA")


HERO_E = sprite("Player/Player_Idle_E.png")
HERO_S = sprite("Player/Player_Idle_S.png")
HERO_N = sprite("Player/Player_Idle_N.png")
WALKERS = [sprite(f"Enemies/Walker_Walk_{d}_0.png") for d in ("S", "E", "W")]


def recolor(image, mapping):
    image = image.copy()
    px = image.load()
    for y in range(image.height):
        for x in range(image.width):
            if px[x, y] in mapping:
                px[x, y] = mapping[px[x, y]]
    return image


HUMAN = {GRN: SKIN2, DGRN: SKIN3, LGRN: SKIN}   # walker before the Toke: skin instead of rot
SICK = {GRN: SKIN2, DGRN: SKIN3}                 # skin, but the green veins stay
CLOTHES = [(DBRN, BRN), (DRED, RED), (G1, G2), (MBLU, BLU)]


def person(index, state, seed):
    """A walker as healthy human (0), sick (1) or zombie (2); humans get varied clothes."""
    base = WALKERS[index % 3]
    if state == 2:
        return base
    shirt, light = CLOTHES[seed % len(CLOTHES)]
    mapping = dict(HUMAN if state == 0 else SICK)
    mapping[DBRN] = shirt
    mapping[BRN] = light
    return recolor(base, mapping)


def scaled(image, factor):
    return image.resize((image.width * factor, image.height * factor), Image.NEAREST)


def put(canvas, image, cx, bottom, factor=1):
    if factor != 1:
        image = scaled(image, factor)
    canvas.alpha_composite(image, (int(cx - image.width / 2), int(bottom - image.height)))


def crowd(canvas, spots, states, seed=0):
    order = sorted(range(len(spots)), key=lambda i: spots[i][1])
    for i in order:
        x, y = spots[i]
        put(canvas, person(i, states[i], i + seed), x, y)


def stars(draw, count, ymax, seed=1):
    r = random.Random(seed)
    for _ in range(count):
        draw.point((r.randint(0, W - 1), r.randint(0, ymax)), fill=r.choice([LYEL, G4, LBLU]))


def skyline(draw, ground, seed=3, color=K3):
    r = random.Random(seed)
    for bx in range(-4, W, 20):
        height = r.randint(26, 80)
        draw.rectangle([bx, ground - height, bx + 16, ground], fill=color)
        for wy in range(ground - height + 6, ground - 4, 10):
            for wx in range(bx + 3, bx + 14, 6):
                if r.random() < 0.25:
                    draw.rectangle([wx, wy, wx + 2, wy + 3], fill=YEL)


def toke(canvas, cx, cy, radius):
    """The Toke: always gold, with an orange glow and a dark halo."""
    d = ImageDraw.Draw(canvas)
    d.ellipse([cx - radius - 6, cy - radius - 6, cx + radius + 6, cy + radius + 6], fill=DBRN)
    d.ellipse([cx - radius - 3, cy - radius - 3, cx + radius + 3, cy + radius + 3], fill=ORG)
    d.ellipse([cx - radius, cy - radius, cx + radius, cy + radius], fill=YEL)
    d.ellipse([cx - radius // 3, cy - radius // 3, cx + radius // 3, cy + radius // 3], fill=K)


def man(wings=False):
    """The man, 128x96: a dark robe and a faceless head; wings, horns and red eyes only in the showdown."""
    image = Image.new("RGBA", (128, 96), (0, 0, 0, 0))
    d = ImageDraw.Draw(image)
    if wings:
        for side in (1, -1):
            def mx(x):
                return 64 + side * (x - 64)
            wing = [(60, 34), (14, 4), (20, 24), (4, 40), (24, 46), (14, 66), (46, 58), (58, 62)]
            d.polygon([(mx(a), b) for a, b in wing], fill=DRED, outline=K)
            for a, b in [(14, 4), (4, 40), (14, 66)]:
                d.line([(mx(60), 36), (mx(a), b)], fill=K, width=1)
    d.polygon([(48, 34), (80, 34), (90, 95), (38, 95)], fill=K2, outline=K)
    d.rectangle([60, 34, 68, 95], fill=K)
    d.rectangle([52, 12, 76, 34], fill=K3, outline=K)
    d.rectangle([56, 30, 72, 34], fill=K)
    if wings:
        d.polygon([(52, 14), (44, 0), (58, 12)], fill=G4, outline=K)
        d.polygon([(76, 14), (84, 0), (70, 12)], fill=G4, outline=K)
        d.rectangle([58, 22, 61, 24], fill=RED)
        d.rectangle([67, 22, 70, 24], fill=RED)
    else:
        d.rectangle([58, 22, 61, 23], fill=G3)
        d.rectangle([67, 22, 70, 23], fill=G3)
    d.polygon([(78, 40), (88, 52), (94, 50), (86, 36)], fill=K2, outline=K)
    toke(image, 96, 48, 5)
    return image


def new_panel(color):
    canvas = Image.new("RGBA", (W, H), color)
    return canvas, ImageDraw.Draw(canvas)


def bands(draw, colors, top=0, step=60):
    for i, color in enumerate(colors):
        draw.rectangle([0, top + i * step, W, top + (i + 1) * step], fill=color)


# --- text boxes ------------------------------------------------------------------------------------------------------

def text_width(text):
    width = 0
    for ch in text:
        if ch == " ":
            width += pf.SPACE_ADVANCE
            continue
        _, rows = pf.trimmed(pf.GLYPHS.get(ch) or pf.GLYPHS["?"])
        width += len(rows[0]) + 1
    return width


def wrap(text, max_width):
    lines, current = [], ""
    for word in text.split():
        candidate = (current + " " + word).strip()
        if text_width(candidate) <= max_width:
            current = candidate
        else:
            lines.append(current)
            current = word
    lines.append(current)
    return lines


def box_height(box):
    inner = box["width"] - 2 * BOX_PADDING - 1
    lines = len(wrap(box["text"], inner))
    return 2 * BOX_PADDING + (SPEAKER_LINE if box["speaker"] else 0) + lines * LINE_HEIGHT


def narration(text, y=None, x=BOX_X, width=BOX_W):
    return {"kind": "Narration", "speaker": "", "speakerColor": VILLAIN_COLOR, "text": text, "x": x, "y": y, "width": width}


def speech(speaker, color, text, y=None, x=BOX_X, width=BOX_W):
    return {"kind": "Speech", "speaker": speaker, "speakerColor": color, "text": text, "x": x, "y": y, "width": width}


def stack(boxes, top=4):
    """Places boxes without a y under each other, starting at top; boxes with a y start a new stack there."""
    y = top
    for box in boxes:
        if box["y"] is None:
            box["y"] = y
        y = box["y"] + box_height(box) + BOX_GAP
    return boxes


# --- panels ----------------------------------------------------------------------------------------------------------

def panel_rumor():
    canvas, d = new_panel(K2)
    bands(d, [K, K2, K3])
    stars(d, 30, 90)
    skyline(d, 186)
    d.rectangle([0, 186, W, H], fill=G1)
    d.rectangle([24, 92, 156, 182], fill=G2, outline=K)
    d.rectangle([28, 96, 152, 178], fill=K)
    for y in range(98, 178, 4):
        d.line([28, y, 152, y], fill=K2)
    put(canvas, man(), 92, 178)
    crowd(canvas, [(18, 280), (52, 292), (88, 284), (124, 296), (162, 282), (36, 316), (110, 318)], [0] * 7, 1)
    boxes = stack([
        narration("Es begann mit einem Satz."),
        speech("DER MANN", VILLAIN_COLOR,
               '"Der Toke. Wer ihn besitzt, hat alles, was er sich wünscht: Gold, Kleidung, Essen, Besitz."'),
        narration("Den Toke trug er längst selbst in der Hand.", y=188),
    ])
    return canvas, boxes


def panel_search():
    canvas, d = new_panel(DBLU)
    d.rectangle([0, 0, W, 50], fill=MBLU)
    d.polygon([(0, 200), (44, 140), (90, 200)], fill=K3)
    d.polygon([(50, 200), (100, 156), (152, 200)], fill=G1)
    d.rectangle([118, 96, 144, 200], fill=LBLU)
    for y in range(100, 200, 6):
        d.line([120, y, 142, y + 3], fill=CRM)
    d.rectangle([0, 200, W, H], fill=DBRN)
    d.rectangle([10, 214, 50, 240], fill=BRN, outline=K)
    d.rectangle([10, 214, 50, 222], fill=YEL, outline=K)
    d.polygon([(108, 226), (170, 226), (160, 252), (118, 252)], fill=BRN, outline=K)
    d.line([139, 196, 139, 226], fill=K)
    crowd(canvas, [(32, 284), (76, 266), (118, 292), (160, 276), (56, 318), (136, 318)], [0] * 6, 2)
    boxes = stack([
        narration("Alle machten sich auf die Suche: in Truhen, hinter Wasserfällen, auf Inseln, auf Schiffen."),
        narration("Tag 1. Alle gesund."),
    ])
    return canvas, boxes


def panel_first_signs():
    canvas, d = new_panel(K3)
    bands(d, [K2, K3], step=100)
    skyline(d, 196, seed=11, color=K2)
    d.rectangle([0, 196, W, H], fill=DBRN)
    d.rectangle([10, 210, 50, 236], fill=BRN, outline=K)
    d.polygon([(130, 212), (170, 212), (164, 230), (136, 230)], fill=BRN, outline=K)
    crowd(canvas, [(32, 284), (76, 266), (118, 292), (160, 276), (56, 318), (136, 318), (98, 250)],
          [0, 1, 0, 0, 1, 0, 0], 3)
    boxes = stack([
        narration("Wochen später. Die Ersten wurden krank. Grüne Adern, Husten, Fieber."),
        narration("Sie suchten trotzdem weiter."),
    ])
    return canvas, boxes


def panel_warning():
    canvas, d = new_panel(K3)
    bands(d, [K2, K3], step=110)
    d.rectangle([0, 200, W, H], fill=G1)
    put(canvas, HERO_E, 46, 318, 2)
    burst = [(34, 150), (40, 162), (28, 168), (42, 172), (38, 184), (50, 176), (60, 184), (58, 172),
             (72, 166), (58, 162), (62, 150), (48, 158)]
    d.polygon(burst, fill=YEL, outline=K)
    crowd(canvas, [(130, 262), (164, 278), (118, 300), (152, 316)], [0, 1, 0, 1], 4)
    boxes = stack([
        narration("Nur einer hörte auf zu suchen."),
        speech("HELD", HERO_COLOR, "Retired die Suche nach dem Toke!"),
        speech("MENGE", VILLAIN_COLOR, "Geh weg. Du bist nur neidisch.", x=64, width=112),
    ])
    return canvas, boxes


def panel_zombies():
    canvas, d = new_panel(K)
    bands(d, [K, K2], step=100)
    stars(d, 15, 80, seed=5)
    d.rectangle([0, 190, W, H], fill=DGRN)
    for y in range(196, H, 6):
        d.line([0, y, W, y], fill=K2)
    spots = [(22, 240), (66, 256), (110, 240), (154, 260), (40, 288), (92, 304), (140, 296), (18, 318), (170, 318)]
    crowd(canvas, spots, [2, 2, 2, 2, 2, 1, 1, 2, 2], 5)
    boxes = stack([
        narration("Einer nach dem anderen. Tag für Tag."),
        narration("Wer krank war, suchte weiter, bis nichts Menschliches mehr in ihm war."),
        narration("Wo sie zusammenbrachen, standen sie als Zombies wieder auf."),
    ])
    return canvas, boxes


def panel_truth():
    canvas, d = new_panel(K)
    bands(d, [K, K2], step=110)
    stars(d, 20, 120, seed=6)
    skyline(d, 214, seed=7, color=K2)
    d.rectangle([0, 214, W, H], fill=DGRN)
    d.rectangle([26, 122, 154, 210], fill=SLATE, outline=K)
    d.rectangle([30, 126, 150, 206], fill=ORG)
    put(canvas, man(), 88, 206)
    crowd(canvas, [(20, 262), (60, 280), (102, 264), (144, 284), (40, 312), (122, 316)], [2] * 6, 6)
    put(canvas, HERO_N, 166, 318)
    boxes = stack([
        narration("Alle Menschen waren Zombies geworden. Nur der Mann kannte die Wahrheit:"),
        speech("DER MANN", VILLAIN_COLOR,
               "Der Toke vergiftet jeden, der ihn sucht. Und nur er kann sie heilen. Ich habe ihn."),
    ])
    return canvas, boxes


def panel_showdown():
    canvas, d = new_panel(K)
    bands(d, [K, K2, K3, DRED], step=80)
    stars(d, 20, 70, seed=8)
    for y in range(170, H, 8):
        d.line([0, y, W, y], fill=K)
    put(canvas, HERO_S, 90, 146, 2)
    put(canvas, man(wings=True), 90, 286)
    boxes = stack([
        narration("Der Held stand dem Mann gegenüber. Nur noch einer."),
        speech("HELD", HERO_COLOR, "assalamu aleykum wa rahmetullahi wa barakatuhu", y=140),
        speech("DER MANN", VILLAIN_COLOR, "wa aleykum ya ars", y=262),
    ])
    return canvas, boxes


PANELS = [panel_rumor, panel_search, panel_first_signs, panel_warning, panel_zombies, panel_truth, panel_showdown]


# --- checks and output -----------------------------------------------------------------------------------------------

def palette():
    image = Image.open(PALETTE_FILE).convert("RGBA")
    return {px[:3] for px in image.getdata()}


def check(index, canvas, boxes, colors):
    for px in canvas.getdata():
        if px[3] != 255:
            raise SystemExit(f"Intro_{index}: transparent pixel")
        if px[:3] not in colors:
            raise SystemExit(f"Intro_{index}: color {px[:3]} is not in the palette")
    for box in boxes:
        bottom = box["y"] + box_height(box)
        if box["x"] < 0 or box["x"] + box["width"] > W or box["y"] < 0 or bottom > H - BOTTOM_FREE:
            raise SystemExit(f"Intro_{index}: box '{box['text'][:20]}' leaves the panel or its free bottom strip")


def draw_text(canvas, x, y, text, color):
    px = canvas.load()
    for ch in text:
        if ch == " ":
            x += pf.SPACE_ADVANCE
            continue
        _, rows = pf.trimmed(pf.GLYPHS.get(ch) or pf.GLYPHS["?"])
        for gy, row in enumerate(rows):
            for gx, cell in enumerate(row):
                if cell == "#" and 0 <= x + gx < W and 0 <= y + gy < H:
                    px[x + gx, y + gy] = color
        x += len(rows[0]) + 1


def preview_panel(canvas, boxes):
    """The panel with its boxes the way CutscenePlayer draws them, for review only."""
    image = canvas.copy()
    d = ImageDraw.Draw(image)
    for box in boxes:
        x, y, w = box["x"], box["y"], box["width"]
        h = box_height(box)
        d.rectangle([x, y, x + w - 1, y + h - 1], fill=K)
        d.rectangle([x + 1, y + 1, x + w - 2, y + h - 2], fill=CRM if box["kind"] == "Narration" else WHT)
        ty = y + BOX_PADDING
        if box["speaker"]:
            draw_text(image, x + BOX_PADDING, ty, box["speaker"], c(box["speakerColor"]))
            ty += SPEAKER_LINE
        for line in wrap(box["text"], w - 2 * BOX_PADDING - 1):
            draw_text(image, x + BOX_PADDING, ty, line, K2)
            ty += LINE_HEIGHT
    return image


def main():
    parser = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    parser.add_argument("--preview", help="folder for intro_preview.png (panels with their boxes, 3x)")
    args = parser.parse_args()

    colors = palette()
    OUT_DIR.mkdir(parents=True, exist_ok=True)
    data = {"panelWidth": W, "panelHeight": H, "panels": []}
    previews = []
    for index, build in enumerate(PANELS, 1):
        canvas, boxes = build()
        canvas = canvas.convert("RGBA")
        check(index, canvas, boxes, colors)
        name = f"Intro_{index}.png"
        canvas.save(OUT_DIR / name)
        data["panels"].append({"image": name, "boxes": boxes})
        previews.append(preview_panel(canvas, boxes))

    (OUT_DIR / "Intro_Cutscene.json").write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(f"{len(PANELS)} panels and Intro_Cutscene.json in {OUT_DIR.relative_to(ROOT)}")

    if args.preview:
        scale, gap = 3, 8
        sheet = Image.new("RGB", (4 * (W * scale + gap) + gap, 2 * (H * scale + gap) + gap), (8, 8, 10))
        for i, image in enumerate(previews):
            sheet.paste(scaled(image, scale).convert("RGB"),
                        (gap + (i % 4) * (W * scale + gap), gap + (i // 4) * (H * scale + gap)))
        out = Path(args.preview)
        out.mkdir(parents=True, exist_ok=True)
        sheet.save(out / "intro_preview.png")
        print(f"Preview: {out / 'intro_preview.png'}")


if __name__ == "__main__":
    main()
