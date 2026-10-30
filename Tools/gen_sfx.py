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
                       ("MissionDone", mission_done)):
        path = SFX_DIR / f"{name}.wav"
        write(path, make(rng))
        print(f"Wrote {path.relative_to(ROOT)}")


if __name__ == "__main__":
    main()
