#!/usr/bin/env python3
"""Generates the placeholder sound effects (issue #12) as 16-bit mono WAV files.

The sounds are synthesised from code with a fixed random seed, so they are reproducible.
They are placeholders until real sound design arrives; only the standard library is used.

Output (under Assets/_Project/Audio/SFX/):
  Shot.wav    short bright blip with a falling pitch
  Hit.wav     dull thump with a noise burst
  Death.wav   lower, longer squelch with a falling saw tone
  GateOpen.wav  rattling chain with a heavy rising clunk at the end (issue #17)
  MissionDone.wav  bright rising three-note chime when a kill gate opens (issue #19)
  RunnerSpawn.wav     short raspy shriek rising in pitch when a runner appears (issue #20)
  RunnerApproach.wav  fast panting hiss that swells, when a runner closes in (issue #20)
  BoomerSpawn.wav     low wet gurgle of bubbles when a boomer appears (issue #22)
  BoomerApproach.wav  warning: two-tone alarm beeps that speed up over a fizzing fuse (issue #22, F-AUD-03)
  Explosion.wav       deep boom with a noise blast and a rumbling tail (issue #22)
  PerkPickup.wav      quick sparkly upward sweep with two bell tones when a perk is collected (issue #23)

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


def envelope(t: float, length: float, attack: float = 0.004) -> float:
    """Fast attack, exponential decay towards the end of the sound."""
    if t < attack:
        return t / attack
    return math.exp(-5.0 * (t - attack) / length)


def sweep(f0: float, f1: float, t: float, length: float) -> float:
    """Phase of a sine sweeping exponentially from f0 to f1 over length seconds."""
    k = math.log(f1 / f0) / length
    return 2 * math.pi * f0 * (math.exp(k * t) - 1) / k


def shot(rng: random.Random) -> list[float]:
    length = 0.09
    out = []
    for i in range(int(RATE * length)):
        t = i / RATE
        square = 1.0 if math.sin(sweep(1200, 260, t, length)) >= 0 else -1.0
        noise = rng.uniform(-1, 1) * max(0.0, 1 - t / 0.02)
        out.append((0.55 * square + 0.45 * noise) * envelope(t, length))
    return out


def hit(rng: random.Random) -> list[float]:
    length = 0.08
    out = []
    smoothed = 0.0
    for i in range(int(RATE * length)):
        t = i / RATE
        smoothed += 0.25 * (rng.uniform(-1, 1) - smoothed)
        thump = math.sin(sweep(220, 90, t, length))
        out.append((0.7 * thump + 0.6 * smoothed) * envelope(t, length, 0.002))
    return out


def death(rng: random.Random) -> list[float]:
    length = 0.28
    out = []
    smoothed = 0.0
    for i in range(int(RATE * length)):
        t = i / RATE
        phase = sweep(320, 55, t, length) / (2 * math.pi)
        saw = 2 * (phase - math.floor(phase + 0.5))
        smoothed += 0.12 * (rng.uniform(-1, 1) - smoothed)
        out.append((0.5 * saw + 0.8 * smoothed) * envelope(t, length, 0.006))
    return out


def gate_open(rng: random.Random) -> list[float]:
    length = 0.7
    out = []
    smoothed = 0.0
    for i in range(int(RATE * length)):
        t = i / RATE
        # Chain rattle: noise bursts at a fast clicking rate that slows down.
        click = 1.0 if math.sin(2 * math.pi * (38 - 20 * t / length) * t) > 0.7 else 0.0
        smoothed += 0.35 * (rng.uniform(-1, 1) - smoothed)
        rattle = smoothed * click * (1 - 0.5 * t / length)
        # Low clunk when the bars hit the bottom.
        tc = t - 0.55
        clunk = math.sin(sweep(140, 60, tc, 0.15)) * envelope(tc, 0.15, 0.003) if tc >= 0 else 0.0
        hum = 0.25 * math.sin(2 * math.pi * 90 * t) * min(1.0, t / 0.05) * (1 - t / length)
        out.append(0.6 * rattle + 0.9 * clunk + hum)
    return out


def mission_done(rng: random.Random) -> list[float]:
    # Rising major arpeggio in square waves, the last note held: reads as "goal reached" over the battle noise.
    notes = [(523.25, 0.0), (659.25, 0.09), (783.99, 0.18), (1046.5, 0.27)]
    length = 0.75
    out = []
    for i in range(int(RATE * length)):
        t = i / RATE
        sample = 0.0
        for freq, start in notes:
            if t < start:
                continue
            tn = t - start
            note_len = length - start if freq == notes[-1][0] else 0.16
            if tn > note_len:
                continue
            square = 1.0 if math.sin(2 * math.pi * freq * tn) >= 0 else -1.0
            sample += 0.35 * square * envelope(tn, note_len, 0.003)
        out.append(sample + 0.02 * rng.uniform(-1, 1) * max(0.0, 1 - t / 0.05))
    return out


def runner_spawn(rng: random.Random) -> list[float]:
    # Rough square shriek with vibrato and a breathy noise layer: higher and thinner than the walker's sounds.
    length = 0.22
    out = []
    smoothed = 0.0
    for i in range(int(RATE * length)):
        t = i / RATE
        vibrato = 1 + 0.04 * math.sin(2 * math.pi * 30 * t)
        square = 1.0 if math.sin(sweep(520, 1100, t, length) * vibrato) >= 0 else -1.0
        smoothed += 0.5 * (rng.uniform(-1, 1) - smoothed)
        out.append((0.45 * square + 0.5 * smoothed) * envelope(t, length, 0.01))
    return out


def runner_approach(rng: random.Random) -> list[float]:
    # Three quick breaths of filtered noise, each louder and higher, under a rising growl.
    length = 0.42
    out = []
    smoothed = 0.0
    for i in range(int(RATE * length)):
        t = i / RATE
        breath = (t * 7.5) % 1.0
        gate = math.sin(math.pi * breath) ** 2
        cutoff = 0.15 + 0.35 * t / length
        smoothed += cutoff * (rng.uniform(-1, 1) - smoothed)
        growl = math.sin(sweep(110, 190, t, length))
        swell = 0.4 + 0.6 * t / length
        fade = min(1.0, (length - t) / 0.03)
        out.append((0.8 * smoothed * gate + 0.3 * growl) * swell * fade)
    return out


def boomer_spawn(rng: random.Random) -> list[float]:
    # Bubbles: short low sine blips with a rising pitch each, over a slow wobbling drone.
    length = 0.45
    bubbles = [(0.0, 90), (0.08, 120), (0.15, 80), (0.24, 140), (0.31, 100)]
    out = []
    for i in range(int(RATE * length)):
        t = i / RATE
        sample = 0.25 * math.sin(2 * math.pi * (55 + 8 * math.sin(2 * math.pi * 6 * t)) * t) * (1 - t / length)
        for start, freq in bubbles:
            tb = t - start
            if 0 <= tb < 0.07:
                sample += 0.6 * math.sin(sweep(freq, freq * 2.2, tb, 0.07)) * envelope(tb, 0.07, 0.003)
        out.append(sample + 0.03 * rng.uniform(-1, 1))
    return out


def boomer_approach(rng: random.Random) -> list[float]:
    # Alarm unlike any other enemy: square beeps alternating high and low, getting faster, over a fuse hiss.
    length = 0.9
    beeps = []
    start, gap, high = 0.0, 0.2, True
    while start < length - 0.05:
        beeps.append((start, 1320.0 if high else 990.0))
        start += gap
        gap = max(0.07, gap * 0.78)
        high = not high
    out = []
    smoothed = 0.0
    for i in range(int(RATE * length)):
        t = i / RATE
        smoothed += 0.6 * (rng.uniform(-1, 1) - smoothed)
        sample = 0.18 * smoothed * (0.5 + 0.5 * t / length)
        for b, freq in beeps:
            tb = t - b
            if 0 <= tb < 0.06:
                square = 1.0 if math.sin(2 * math.pi * freq * tb) >= 0 else -1.0
                sample += 0.45 * square * min(1.0, (0.06 - tb) / 0.01)
        out.append(sample * min(1.0, (length - t) / 0.02))
    return out


def explosion(rng: random.Random) -> list[float]:
    # Low sine drop for the punch, a loud noise burst that darkens, and a long rumble.
    length = 1.1
    out = []
    bright = dark = 0.0
    for i in range(int(RATE * length)):
        t = i / RATE
        noise = rng.uniform(-1, 1)
        bright += 0.5 * (noise - bright)
        dark += 0.04 * (noise - dark)
        cutoff_mix = max(0.0, 1 - t / 0.25)
        boom = math.sin(sweep(120, 35, t, 0.5)) * envelope(t, 0.6, 0.002)
        blast = (bright * cutoff_mix + 3.0 * dark * (1 - cutoff_mix)) * envelope(t, length, 0.002)
        out.append(1.0 * boom + 0.7 * blast)
    return out


def perk_pickup(rng: random.Random) -> list[float]:
    # Fast rising triangle sweep, then two bright bell tones a fifth apart with a short sparkle: clearly a reward,
    # unlike the square-wave chime of a finished mission.
    length = 0.4
    out = []
    phase = 0.0
    for i in range(int(RATE * length)):
        t = i / RATE
        sample = 0.0
        if t < 0.09:
            freq = 600 + 1400 * t / 0.09
            phase += 2 * math.pi * freq / RATE
            tri = 2 / math.pi * math.asin(math.sin(phase))
            sample += 0.4 * tri * min(1.0, (0.09 - t) / 0.02 + 0.3)
        for start, freq in ((0.07, 1567.98), (0.13, 2349.32)):
            tb = t - start
            if tb >= 0:
                bell = math.sin(2 * math.pi * freq * tb) + 0.3 * math.sin(2 * math.pi * freq * 2.76 * tb)
                sample += 0.35 * bell * envelope(tb, length - start, 0.002)
        sparkle = 0.05 * rng.uniform(-1, 1) * max(0.0, 1 - t / 0.15)
        out.append(sample + sparkle)
    return out


def write(path: Path, samples: list[float]) -> None:
    peak = max(abs(s) for s in samples) or 1.0
    with wave.open(str(path), "wb") as f:
        f.setnchannels(1)
        f.setsampwidth(2)
        f.setframerate(RATE)
        f.writeframes(b"".join(struct.pack("<h", int(32767 * 0.9 * s / peak)) for s in samples))


def main() -> None:
    SFX_DIR.mkdir(parents=True, exist_ok=True)
    rng = random.Random(12)
    for name, make in (("Shot", shot), ("Hit", hit), ("Death", death), ("GateOpen", gate_open),
                       ("MissionDone", mission_done), ("RunnerSpawn", runner_spawn),
                       ("RunnerApproach", runner_approach), ("BoomerSpawn", boomer_spawn),
                       ("BoomerApproach", boomer_approach), ("Explosion", explosion),
                       ("PerkPickup", perk_pickup)):
        path = SFX_DIR / f"{name}.wav"
        write(path, make(rng))
        print(f"Wrote {path.relative_to(ROOT)}")


if __name__ == "__main__":
    main()
