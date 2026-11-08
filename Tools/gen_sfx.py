#!/usr/bin/env python3
"""Generates the RushOps sound effects in an 8-bit style (issue #108, design decisions G9 and G10).

Every sound is built from the voices of an old console sound chip: pulse waves with a duty cycle, a triangle and
LFSR noise, with pitch moving in steps (arpeggios, slides) and the output quantised to 16 levels like a 4-bit DAC.
A fixed seed keeps the files reproducible; only the standard library is used.

Output (under Assets/_Project/Audio/SFX/), 16-bit mono WAV at 22050 Hz:
  Shot.wav            short pulse "pew" falling in pitch, with a noise click
  ShotRifle.wav       assault rifle: shorter, sharper, with a metallic crack (issue #174)
  ShotShotgun.wav     shotgun: deep noise boom with a falling thump (issue #174)
  ShotDual.wav        dual pistols: light high tick (issue #174)
  ShotDrone.wav       combat drone of the companion perk: a thin, short high blip, no noise (issue #245)
  FlameLoop.wav       flamethrower: a seamless roaring hiss with a slow flutter, played as a loop (issue #254)
  Hit.wav             dull noise thump on an enemy
  Death.wav           falling pulse slide with crunchy noise
  PlayerHurt.wav      harsh two-step down buzz when the player loses a heart
  Dash.wav            quick rising noise whoosh
  UiClick.wav         tiny high blip for buttons
  GateOpen.wav        rattling noise ratchet ending in a low clunk
  MissionDone.wav     rising three-note arpeggio chime
  WalkerSpawn.wav     low wobbling groan
  WalkerApproach.wav  slow two-note moan
  RunnerSpawn.wav     raspy rising shriek
  RunnerApproach.wav  fast panting noise bursts
  BruteSpawn.wav      deep growl with a heavy noise rumble
  BruteApproach.wav   two heavy stomps
  BoomerSpawn.wav     low bubbling gurgle
  BoomerApproach.wav  warning: fast high two-tone alarm over a fizzing fuse, unlike any other enemy (F-AUD-03)
  Explosion.wav       noise boom with a falling rumble
  PerkPickup.wav      bright upward arpeggio
  GoldPickup.wav      two short bright notes (coin)
  Nuke.wav            rising charge, a crack and a long low boom
  PuzzleSolved.wav    unlock click and a warm two-note chord
  SwitchOn.wav        plate click with a confirm blip
  SwitchReset.wav     three falling buzzer pulses

Usage:
  python3 Tools/gen_sfx.py
"""
import math
import random
import struct
import wave
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SFX_DIR = ROOT / "Assets/_Project/Audio/SFX"
RATE = 22050
# Output levels of the 4-bit DAC.
LEVELS = 16


class Noise:
    """15-bit LFSR noise like the NES noise channel; short mode gives a metallic buzz."""

    def __init__(self, short: bool = False):
        self.reg = 1
        self.tap = 6 if short else 1
        self.phase = 0.0
        self.value = 1.0

    def sample(self, freq: float) -> float:
        self.phase += freq / RATE
        while self.phase >= 1.0:
            self.phase -= 1.0
            bit = (self.reg ^ (self.reg >> self.tap)) & 1
            self.reg = (self.reg >> 1) | (bit << 14)
            self.value = 1.0 if self.reg & 1 else -1.0
        return self.value


class Pulse:
    """Pulse wave with a duty cycle (0.125, 0.25, 0.5)."""

    def __init__(self, duty: float = 0.5):
        self.duty = duty
        self.phase = 0.0

    def sample(self, freq: float) -> float:
        self.phase = (self.phase + freq / RATE) % 1.0
        return 1.0 if self.phase < self.duty else -1.0


class Triangle:
    """Stepped triangle (16 steps per half cycle)."""

    def __init__(self):
        self.phase = 0.0

    def sample(self, freq: float) -> float:
        self.phase = (self.phase + freq / RATE) % 1.0
        tri = 4.0 * abs(self.phase - 0.5) - 1.0
        return round(tri * 7.5) / 7.5


def frames(length: float) -> range:
    return range(int(RATE * length))


def decay(t: float, length: float, attack: float = 0.003) -> float:
    """Fast attack, then a stepped linear fade like a volume envelope of 15 steps."""
    if t < attack:
        return t / attack
    level = max(0.0, 1.0 - (t - attack) / max(1e-6, length - attack))
    return math.ceil(level * 15) / 15


def slide(f0: float, f1: float, t: float, length: float) -> float:
    """Frequency sliding exponentially from f0 to f1."""
    return f0 * (f1 / f0) ** min(1.0, t / length)


def notes(t: float, steps: list[float], step_length: float) -> float:
    """Frequency of an arpeggio: each note lasts step_length seconds; holds the last one."""
    return steps[min(len(steps) - 1, int(t / step_length))]


def mix(*tracks: list[float]) -> list[float]:
    out = [0.0] * max(len(t) for t in tracks)
    for track in tracks:
        for i, s in enumerate(track):
            out[i] += s
    return out


def silence(length: float) -> list[float]:
    return [0.0] * int(RATE * length)


# --- Sounds -----------------------------------------------------------------------

def shot() -> list[float]:
    length = 0.08
    p, n = Pulse(0.25), Noise()
    return [(0.6 * p.sample(slide(1400, 300, t, length)) + 0.4 * n.sample(9000) * (t < 0.015)) * decay(t, length)
            for t in (i / RATE for i in frames(length))]


def shot_rifle() -> list[float]:
    """Assault rifle (issue #174): shorter and sharper than the pistol, a hard noise crack on top."""
    length = 0.06
    p, n = Pulse(0.125), Noise(short=True)
    return [(0.5 * p.sample(slide(1900, 500, t, length)) + 0.5 * n.sample(12000) * (t < 0.02)) * decay(t, length)
            for t in (i / RATE for i in frames(length))]


def shot_shotgun() -> list[float]:
    """Shotgun (issue #174): a deep boom, mostly noise, with a low thump falling under it."""
    length = 0.24
    p, n = Pulse(0.5), Noise()
    return [(0.45 * p.sample(slide(220, 55, t, length)) + 0.7 * n.sample(slide(6000, 900, t, length)))
            * decay(t, length, 0.002) for t in (i / RATE for i in frames(length))]


def shot_dual() -> list[float]:
    """Dual pistols (issue #174): a light, high tick, so the fast stream does not tire the ear."""
    length = 0.045
    p = Pulse(0.25)
    return [0.8 * p.sample(slide(2400, 900, t, length)) * decay(t, length)
            for t in (i / RATE for i in frames(length))]


def shot_drone() -> list[float]:
    """Combat drone (issue #245): a thin electronic blip, higher and shorter than the pistol and without the noise
    click, so it stays under the player's weapon even at the same volume."""
    length = 0.05
    p = Pulse(0.125)
    return [0.7 * p.sample(slide(2200, 1500, t, length)) * decay(t, length)
            for t in (i / RATE for i in frames(length))]


def hit() -> list[float]:
    length = 0.07
    n, p = Noise(), Pulse(0.5)
    return [(0.7 * n.sample(slide(3000, 600, t, length)) + 0.3 * p.sample(slide(220, 90, t, length))) * decay(t, length)
            for t in (i / RATE for i in frames(length))]


def death() -> list[float]:
    length = 0.28
    p, n = Pulse(0.125), Noise()
    return [(0.55 * p.sample(slide(500, 60, t, length)) + 0.45 * n.sample(slide(4000, 500, t, length))) * decay(t, length)
            for t in (i / RATE for i in frames(length))]


def player_hurt() -> list[float]:
    length = 0.25
    p, q = Pulse(0.5), Pulse(0.25)
    return [(0.5 * p.sample(notes(t, [330, 220], 0.09)) + 0.5 * q.sample(notes(t, [335, 222], 0.09))) * decay(t, length)
            for t in (i / RATE for i in frames(length))]


def dash() -> list[float]:
    length = 0.16
    n = Noise()
    out = []
    for i in frames(length):
        t = i / RATE
        swell = min(1.0, t / 0.04) * (1.0 - t / length)
        out.append(n.sample(slide(1500, 9000, t, length)) * swell)
    return out


def ui_click() -> list[float]:
    length = 0.04
    p = Pulse(0.5)
    return [p.sample(notes(t, [1760, 2640], 0.015)) * decay(t, length) for t in (i / RATE for i in frames(length))]


def gate_open() -> list[float]:
    length = 0.8
    n, tri = Noise(short=True), Triangle()
    out = []
    for i in frames(length):
        t = i / RATE
        ratchet = 1.0 if (t * 22) % 1.0 < 0.35 else 0.15
        body = 0.4 * n.sample(slide(1200, 3000, t, length)) * ratchet * (t < 0.65)
        clunk = 0.8 * tri.sample(70) * decay(t - 0.65, 0.15) if t >= 0.65 else 0.0
        out.append(body + clunk)
    return out


def mission_done() -> list[float]:
    length = 0.45
    p, q = Pulse(0.25), Pulse(0.5)
    return [(0.6 * p.sample(notes(t, [523, 659, 784, 1047], 0.08)) + 0.25 * q.sample(notes(t, [262, 330, 392, 523], 0.08)))
            * decay(t, length, 0.002) for t in (i / RATE for i in frames(length))]


def walker_spawn() -> list[float]:
    length = 0.5
    tri, n = Triangle(), Noise()
    return [(0.7 * tri.sample(110 + 15 * math.sin(t * 30)) + 0.3 * n.sample(800)) * decay(t, length, 0.05)
            for t in (i / RATE for i in frames(length))]


def walker_approach() -> list[float]:
    length = 0.6
    p = Pulse(0.125)
    return [p.sample(notes(t, [147, 131], 0.3) * (1 + 0.02 * math.sin(t * 40))) * decay(t, length, 0.06) * 0.8
            for t in (i / RATE for i in frames(length))]


def runner_spawn() -> list[float]:
    length = 0.3
    p, n = Pulse(0.125), Noise()
    return [(0.5 * p.sample(slide(400, 1400, t, length)) + 0.5 * n.sample(slide(3000, 8000, t, length))) * decay(t, length, 0.02)
            for t in (i / RATE for i in frames(length))]


def runner_approach() -> list[float]:
    length = 0.5
    n = Noise()
    out = []
    for i in frames(length):
        t = i / RATE
        pant = 1.0 if (t * 8) % 1.0 < 0.45 else 0.0
        out.append(n.sample(5000) * pant * (0.4 + 0.6 * t / length))
    return out


def brute_spawn() -> list[float]:
    length = 0.7
    tri, n = Triangle(), Noise()
    return [(0.6 * tri.sample(slide(80, 55, t, length) * (1 + 0.05 * math.sin(t * 60))) + 0.4 * n.sample(300))
            * decay(t, length, 0.04) for t in (i / RATE for i in frames(length))]


def brute_approach() -> list[float]:
    stomp_length = 0.18
    tri, n = Triangle(), Noise()
    stomp = [(0.7 * tri.sample(slide(90, 40, t, stomp_length)) + 0.3 * n.sample(400)) * decay(t, stomp_length)
             for t in (i / RATE for i in frames(stomp_length))]
    return stomp + silence(0.17) + stomp


def boomer_spawn() -> list[float]:
    length = 0.6
    p, n = Pulse(0.5), Noise()
    out = []
    for i in frames(length):
        t = i / RATE
        bubble = 120 + 80 * ((t * 12) % 1.0)
        out.append((0.6 * p.sample(bubble) + 0.2 * n.sample(600)) * decay(t, length, 0.03))
    return out


def boomer_approach() -> list[float]:
    """The warning: a fast, high two-tone alarm no other enemy uses, getting quicker, over a fizzing fuse."""
    length = 1.0
    p, n = Pulse(0.25), Noise(short=True)
    out = []
    for i in frames(length):
        t = i / RATE
        rate = 6 + 10 * t / length
        tone = 1320 if (t * rate) % 1.0 < 0.5 else 990
        beep = 1.0 if (t * rate * 2) % 1.0 < 0.7 else 0.0
        out.append(0.6 * p.sample(tone) * beep + 0.25 * n.sample(7000))
    return out


def flame_loop() -> list[float]:
    """Flamethrower (issue #254): a low noise roar under a thin hiss, fluttering four times a second. No envelope and
    only modulation periods that divide the length, so the end joins the start and the file loops without a click."""
    length = 1.0
    roar, hiss = Noise(), Noise()
    out = []
    for t in (i / RATE for i in frames(length)):
        flutter = 0.8 + 0.2 * math.sin(2 * math.pi * 4 * t / length)
        swell = 0.9 + 0.1 * math.sin(2 * math.pi * t / length)
        out.append((0.7 * roar.sample(700 * swell) + 0.3 * hiss.sample(9000)) * flutter)
    return out


def explosion() -> list[float]:
    length = 0.9
    n, tri = Noise(), Triangle()
    return [(0.75 * n.sample(slide(2500, 150, t, length)) + 0.35 * tri.sample(slide(90, 35, t, length))) * decay(t, length)
            for t in (i / RATE for i in frames(length))]


def perk_pickup() -> list[float]:
    length = 0.32
    p = Pulse(0.125)
    return [p.sample(notes(t, [784, 988, 1175, 1568], 0.05)) * decay(t, length, 0.002)
            for t in (i / RATE for i in frames(length))]


def gold_pickup() -> list[float]:
    length = 0.14
    p = Pulse(0.25)
    return [0.8 * p.sample(notes(t, [1319, 1760], 0.05)) * decay(t, length, 0.002)
            for t in (i / RATE for i in frames(length))]


def nuke() -> list[float]:
    charge_length, boom_length = 0.5, 1.4
    p, n, tri = Pulse(0.5), Noise(), Triangle()
    charge = [0.5 * p.sample(slide(200, 1600, t, charge_length)) * (t / charge_length)
              for t in (i / RATE for i in frames(charge_length))]
    boom = [(0.7 * n.sample(slide(6000, 100, t, boom_length)) + 0.5 * tri.sample(slide(70, 30, t, boom_length)))
            * decay(t, boom_length) for t in (i / RATE for i in frames(boom_length))]
    return charge + boom


def puzzle_solved() -> list[float]:
    click_length, chord_length = 0.04, 0.5
    n, a, b = Noise(short=True), Pulse(0.5), Triangle()
    click = [n.sample(6000) * decay(t, click_length) for t in (i / RATE for i in frames(click_length))]
    chord = [(0.45 * a.sample(notes(t, [392, 523], 0.12)) + 0.55 * b.sample(notes(t, [196, 262], 0.12)))
             * decay(t, chord_length, 0.01) for t in (i / RATE for i in frames(chord_length))]
    return click + chord


def switch_on() -> list[float]:
    click_length, blip_length = 0.05, 0.1
    n, p = Noise(), Pulse(0.25)
    click = [n.sample(2000) * decay(t, click_length) for t in (i / RATE for i in frames(click_length))]
    blip = [p.sample(1046) * decay(t, blip_length) * 0.7 for t in (i / RATE for i in frames(blip_length))]
    return click + blip


def switch_reset() -> list[float]:
    out = []
    p = Pulse(0.5)
    for k, freq in enumerate((330, 262, 196)):
        length = 0.14
        out += [p.sample(freq) * decay(t, length, 0.002) * 0.8 for t in (i / RATE for i in frames(length))]
        out += silence(0.05)
    return out


SOUNDS = {
    "Shot": shot, "ShotRifle": shot_rifle, "ShotShotgun": shot_shotgun, "ShotDual": shot_dual, "ShotDrone": shot_drone, "FlameLoop": flame_loop, "Hit": hit, "Death": death, "PlayerHurt": player_hurt, "Dash": dash, "UiClick": ui_click,
    "GateOpen": gate_open, "MissionDone": mission_done,
    "WalkerSpawn": walker_spawn, "WalkerApproach": walker_approach,
    "RunnerSpawn": runner_spawn, "RunnerApproach": runner_approach,
    "BruteSpawn": brute_spawn, "BruteApproach": brute_approach,
    "BoomerSpawn": boomer_spawn, "BoomerApproach": boomer_approach,
    "Explosion": explosion, "PerkPickup": perk_pickup, "GoldPickup": gold_pickup, "Nuke": nuke,
    "PuzzleSolved": puzzle_solved, "SwitchOn": switch_on, "SwitchReset": switch_reset,
}


def quantise(samples: list[float]) -> list[float]:
    """Normalises to 90 % and snaps to the levels of a 4-bit DAC."""
    peak = max(abs(s) for s in samples) or 1.0
    half = (LEVELS - 1) / 2
    return [round(0.9 * s / peak * half) / half for s in samples]


def write(path: Path, samples: list[float]) -> None:
    with wave.open(str(path), "wb") as f:
        f.setnchannels(1)
        f.setsampwidth(2)
        f.setframerate(RATE)
        f.writeframes(b"".join(struct.pack("<h", int(32767 * s)) for s in quantise(samples)))


def main() -> None:
    SFX_DIR.mkdir(parents=True, exist_ok=True)
    random.seed(12)
    for name, make in SOUNDS.items():
        path = SFX_DIR / f"{name}.wav"
        write(path, make())
        print(f"Wrote {path.relative_to(ROOT)}")


if __name__ == "__main__":
    main()
