#!/usr/bin/env python3
"""Generates the two RushOps music tracks as chiptune loops (issue #298, design decisions G11, G12, N30, N31).

Style: a driving top-down arcade zombie shooter, dark and playful at once; own melodies in D minor. Every voice is a
channel of an old console sound chip, quantised to 16 levels on its own like a 4-bit DAC, then mixed:

  lead   pulse wave, duty 25 %, with a delayed vibrato
  arp    pulse wave, duty 12.5 %, fast broken chords
  bass   stepped triangle, octave-jumping eighths
  drums  LFSR noise (kick with a falling pulse thump, snare, short-mode hi-hat)

The tracks loop seamlessly: they are rendered one bar longer and the tail is folded back onto the start, so release
tails continue across the loop point instead of clicking. A fixed seed keeps the files reproducible; only the
standard library is used.

Output (under Assets/_Project/Audio/Music/), 16-bit mono WAV at 22050 Hz:
  Music_Menu.wav   main menu: 100 BPM, 16 bars (38.4 s), eerie arpeggios, the melody in the second half
  Music_Game.wav   in the game: 150 BPM, 40 bars (64 s): groove, theme, B part, breakdown, theme with a drum fill

Usage:
  python3 Tools/gen_music.py            write both tracks
  python3 Tools/gen_music.py --preview  also write a 12 s excerpt of each to docs/audio/ for listening
"""
import math
import random
import struct
import sys
import wave
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
from gen_sfx import Noise, Pulse, Triangle  # noqa: E402  (the same chip voices as the sound effects)

ROOT = Path(__file__).resolve().parent.parent
MUSIC_DIR = ROOT / "Assets/_Project/Audio/Music"
PREVIEW_DIR = ROOT / "docs/audio"
RATE = 22050
HALF_LEVELS = 7.5

# Note names to MIDI numbers (octave 4 holds middle C = 60).
_NAMES = {"C": 0, "C#": 1, "Db": 1, "D": 2, "D#": 3, "Eb": 3, "E": 4, "F": 5, "F#": 6, "Gb": 6, "G": 7, "G#": 8,
          "Ab": 8, "A": 9, "A#": 10, "Bb": 10, "B": 11}


def midi(name: str) -> int:
    """'D5' -> 74, 'C#6' -> 85."""
    return 12 * (int(name[-1]) + 1) + _NAMES[name[:-1]]


def freq(note: int) -> float:
    return 440.0 * 2 ** ((note - 69) / 12)


# Chords as root and the intervals of their tones.
MINOR, MAJOR = (0, 3, 7), (0, 4, 7)
CHORDS = {"Dm": ("D", MINOR), "Bb": ("Bb", MAJOR), "C": ("C", MAJOR), "A": ("A", MAJOR), "Gm": ("G", MINOR),
          "Eb": ("Eb", MAJOR)}


def chord_root(chord: str, octave: int) -> int:
    return midi(CHORDS[chord][0] + str(octave))


def chord_tones(chord: str, octave: int) -> list[int]:
    root = chord_root(chord, octave)
    return [root + i for i in CHORDS[chord][1]]


class Song:
    """Notes per voice as (start in beats, length in beats, MIDI note or drum name, volume 0..1)."""

    def __init__(self, bpm: float, bars: int):
        self.beat = 60.0 / bpm
        self.bars = bars
        self.voices: dict[str, list[tuple[float, float, object, float]]] = {
            "lead": [], "arp": [], "bass": [], "drums": []}

    @property
    def length(self) -> float:
        return self.bars * 4 * self.beat

    def add(self, voice: str, start: float, length: float, note, volume: float) -> None:
        self.voices[voice].append((start, length, note, volume))

    def melody(self, bar: int, phrase: list[tuple[str | None, int]], volume: float) -> None:
        """Adds a phrase in eighths from bar on; None is a rest."""
        beat = bar * 4.0
        for name, eighths in phrase:
            if name is not None:
                self.add("lead", beat, eighths * 0.5, midi(name), volume)
            beat += eighths * 0.5


# --- Rendering ----------------------------------------------------------------------------------------------------


def envelope(t: float, gate: float, attack: float = 0.004, sustain: float = 0.7, decay_time: float = 0.12,
             release: float = 0.03) -> float:
    """Attack, decay to the sustain level, release after the gate; stepped like a 4-bit volume register."""
    if t < attack:
        level = t / attack
    elif t < gate:
        level = sustain + (1.0 - sustain) * max(0.0, 1.0 - (t - attack) / decay_time)
    else:
        held = sustain + (1.0 - sustain) * max(0.0, 1.0 - (gate - attack) / decay_time)
        level = held * max(0.0, 1.0 - (t - gate) / release)
    return math.ceil(level * 15) / 15


def render_tone(out: list[float], song: Song, voice: str, make_osc, gate_ratio: float, vibrato: bool,
                sustain: float, decay_time: float) -> None:
    osc = make_osc()
    for start, length, note, volume in song.voices[voice]:
        f = freq(note)
        begin = int(start * song.beat * RATE)
        gate = length * song.beat * gate_ratio
        release = 0.03
        for i in range(int((gate + release) * RATE)):
            t = i / RATE
            fi = f
            if vibrato and t > 0.15:
                fi = f * (1.0 + 0.012 * math.sin(2 * math.pi * 5.5 * (t - 0.15)))
            s = osc.sample(fi) * volume * envelope(t, gate, sustain=sustain, decay_time=decay_time)
            if begin + i < len(out):
                out[begin + i] += s


def render_drums(out: list[float], song: Song) -> None:
    noise, metal, thump = Noise(), Noise(short=True), Pulse(0.5)
    for start, _, kind, volume in song.voices["drums"]:
        begin = int(start * song.beat * RATE)
        if kind == "kick":
            length = 0.11
            sound = lambda t: (0.75 * thump.sample(150 * (45 / 150) ** min(1.0, t / 0.08))
                               + 0.25 * noise.sample(1800) * (t < 0.01))
        elif kind == "snare":
            length = 0.13
            sound = lambda t: 0.85 * noise.sample(7000 - 20000 * t) + 0.15 * thump.sample(220 - 600 * t)
        else:  # hat
            length = 0.035
            sound = lambda t: metal.sample(14000)
        for i in range(int(length * RATE)):
            t = i / RATE
            level = math.ceil(max(0.0, 1.0 - t / length) * 15) / 15
            if begin + i < len(out):
                out[begin + i] += sound(t) * volume * level


def quantise_voice(samples: list[float]) -> list[float]:
    """One channel of the chip: 16 output levels between -1 and 1."""
    return [round(max(-1.0, min(1.0, s)) * HALF_LEVELS) / HALF_LEVELS for s in samples]


def render(song: Song, mix: dict[str, float]) -> list[float]:
    total = int(song.length * RATE)
    # One bar longer, so release tails and the last notes can be folded back onto the start.
    extra = int(4 * song.beat * RATE)
    voices = {}
    for name in ("lead", "arp", "bass"):
        buffer = [0.0] * (total + extra)
        if name == "lead":
            render_tone(buffer, song, name, lambda: Pulse(0.25), 0.92, True, 0.75, 0.2)
        elif name == "arp":
            render_tone(buffer, song, name, lambda: Pulse(0.125), 0.7, False, 0.5, 0.05)
        else:
            render_tone(buffer, song, name, Triangle, 0.85, False, 0.9, 0.05)
        voices[name] = quantise_voice(buffer)
    drums = [0.0] * (total + extra)
    render_drums(drums, song)
    voices["drums"] = quantise_voice(drums)

    out = [0.0] * total
    for name, samples in voices.items():
        gain = mix[name]
        for i, s in enumerate(samples):
            out[i % total] += gain * s
    peak = max(abs(s) for s in out) or 1.0
    return [0.9 * s / peak for s in out]


def write(path: Path, samples: list[float]) -> None:
    with wave.open(str(path), "wb") as f:
        f.setnchannels(1)
        f.setsampwidth(2)
        f.setframerate(RATE)
        f.writeframes(b"".join(struct.pack("<h", int(32767 * s)) for s in samples))


# --- Parts --------------------------------------------------------------------------------------------------------


def bass_drive(song: Song, bar: int, chord: str, volume: float, fill: bool = False) -> None:
    """Octave-jumping eighths on the root; the last two eighths walk up to the next bar when fill is set."""
    root = chord_root(chord, 2)
    for e in range(8):
        note = root + (12 if e % 2 else 0)
        if fill and e == 6:
            note = root + 7
        elif fill and e == 7:
            note = root + 10
        song.add("bass", bar * 4 + e * 0.5, 0.5, note, volume)


def bass_long(song: Song, bar: int, chord: str, volume: float) -> None:
    root = chord_root(chord, 2)
    song.add("bass", bar * 4, 2, root, volume)
    song.add("bass", bar * 4 + 2, 1.5, root + 7, volume)
    song.add("bass", bar * 4 + 3.5, 0.5, root + 12, volume)


def arp_sixteenths(song: Song, bar: int, chord: str, volume: float) -> None:
    tones = chord_tones(chord, 5)
    pattern = [tones[0], tones[1], tones[2], tones[0] + 12, tones[2], tones[1]]
    for s in range(16):
        song.add("arp", bar * 4 + s * 0.25, 0.25, pattern[s % len(pattern)], volume)


def arp_eighths(song: Song, bar: int, chord: str, volume: float) -> None:
    tones = chord_tones(chord, 4)
    pattern = [tones[0], tones[1], tones[2], tones[0] + 12, tones[2] + 12, tones[0] + 12, tones[2], tones[1]]
    for e in range(8):
        song.add("arp", bar * 4 + e * 0.5, 0.5, pattern[e], volume)


def drums_drive(song: Song, bar: int, volume: float, fill: bool = False) -> None:
    b = bar * 4
    for beat in (0, 2):
        song.add("drums", b + beat, 0, "kick", volume)
    song.add("drums", b + 2.5, 0, "kick", volume * 0.8)
    if fill:
        for s in range(8):
            song.add("drums", b + 2 + s * 0.25, 0, "snare", volume * (0.5 + s * 0.06))
        song.add("drums", b + 1, 0, "snare", volume)
    else:
        for beat in (1, 3):
            song.add("drums", b + beat, 0, "snare", volume)
    for e in range(8 if not fill else 4):
        song.add("drums", b + e * 0.5, 0, "hat", volume * (0.55 if e % 2 else 0.35))


def drums_sparse(song: Song, bar: int, volume: float) -> None:
    song.add("drums", bar * 4, 0, "kick", volume)
    song.add("drums", bar * 4 + 3, 0, "snare", volume * 0.5)


# --- Tracks -------------------------------------------------------------------------------------------------------

THEME = [
    [("A4", 1), ("D5", 1), ("F5", 1), ("A5", 2), ("G5", 1), ("F5", 1), ("E5", 1)],
    [("F5", 3), ("D5", 1), ("Bb4", 2), ("D5", 2)],
    [("E5", 1), ("G5", 1), ("C6", 2), ("Bb5", 1), ("A5", 1), ("G5", 2)],
    [("A5", 3), ("G5", 1), ("F5", 1), ("E5", 1), ("C#5", 2)],
    [("D5", 1), ("F5", 1), ("A5", 1), ("D6", 2), ("C6", 1), ("A5", 1), ("F5", 1)],
    [("G5", 2), ("F5", 1), ("D5", 1), ("Bb4", 1), ("D5", 1), ("F5", 2)],
    [("E5", 2), ("G5", 1), ("E5", 1), ("C5", 1), ("E5", 1), ("G5", 2)],
    [("A5", 2), ("A5", 1), ("G5", 1), ("F5", 1), ("E5", 1), ("C#5", 1), ("A4", 1)],
]

B_PART = [
    [("G5", 1), (None, 1), ("Bb5", 1), (None, 1), ("D6", 2), ("C6", 1), ("Bb5", 1)],
    [("A5", 3), ("F5", 1), ("D5", 2), ("F5", 2)],
    [("G5", 1), (None, 1), ("Bb5", 1), (None, 1), ("Eb6", 2), ("D6", 1), ("C6", 1)],
    [("C#6", 3), ("A5", 1), ("E5", 2), ("A5", 2)],
    [("Bb5", 2), ("A5", 1), ("G5", 1), ("D5", 2), ("G5", 2)],
    [("F5", 2), ("E5", 1), ("D5", 1), ("A4", 2), ("D5", 2)],
    [("Eb5", 2), ("G5", 1), ("Bb5", 1), ("Eb6", 2), ("D6", 1), ("Bb5", 1)],
    [("A5", 2), ("C#6", 2), ("E6", 4)],
]

BREAKDOWN = [
    [("D5", 4), ("A5", 4)],
    [("F5", 4), ("E5", 4)],
    [("D5", 4), ("F5", 4)],
    [("E5", 8)],
    [("D5", 4), ("A5", 4)],
    [("C6", 4), ("Bb5", 4)],
    [("A5", 4), ("F5", 4)],
    [("E5", 4), ("C#5", 4)],
]


def game_track() -> Song:
    song = Song(150, 40)
    theme_chords = ["Dm", "Bb", "C", "A"] * 2
    b_chords = ["Gm", "Dm", "Eb", "A"] * 2
    breakdown_chords = ["Dm", "Dm", "Bb", "A"] * 2
    sections = [
        ("groove", theme_chords, None),
        ("theme", theme_chords, THEME),
        ("b", b_chords, B_PART),
        ("breakdown", breakdown_chords, BREAKDOWN),
        ("theme", theme_chords, THEME),
    ]
    bar = 0
    for s, (kind, chords, phrases) in enumerate(sections):
        for i, chord in enumerate(chords):
            last_of_section = i == len(chords) - 1
            if kind == "breakdown":
                bass_long(song, bar, chord, 0.9)
                arp_eighths(song, bar, chord, 0.5)
                drums_sparse(song, bar, 0.8)
                if last_of_section:
                    # Build back up: kick on every beat with a snare roll into the theme.
                    for beat in range(4):
                        song.add("drums", bar * 4 + beat, 0, "kick", 0.9)
                    for k in range(8):
                        song.add("drums", bar * 4 + 2 + k * 0.25, 0, "snare", 0.4 + k * 0.07)
            else:
                bass_drive(song, bar, chord, 1.0, fill=last_of_section)
                arp_sixteenths(song, bar, chord, 0.45 if kind == "groove" else 0.32)
                drums_drive(song, bar, 0.9, fill=last_of_section and s == len(sections) - 1)
            if phrases is not None:
                song.melody(bar, phrases[i], 0.75 if kind == "breakdown" else 1.0)
            bar += 1
    return song


MENU_MELODY = [
    [("A5", 4), ("F5", 2), ("E5", 2)],
    [("D5", 6), (None, 2)],
    [("F5", 2), ("Bb5", 2), ("D6", 4)],
    [("C#6", 4), ("A5", 4)],
    [("Bb5", 2), ("A5", 2), ("G5", 4)],
    [("F5", 4), ("D5", 4)],
    [("G5", 2), ("Bb5", 2), ("Eb6", 4)],
    [("D6", 2), ("C#6", 2), ("A5", 4)],
]


def menu_track() -> Song:
    song = Song(100, 16)
    chords = ["Dm", "Dm", "Bb", "A", "Gm", "Dm", "Eb", "A"] * 2
    for bar, chord in enumerate(chords):
        root = chord_root(chord, 2)
        song.add("bass", bar * 4, 1.5, root, 0.9)
        song.add("bass", bar * 4 + 1.5, 0.5, root + 12, 0.6)
        song.add("bass", bar * 4 + 2, 2, root + 7, 0.8)
        arp_eighths(song, bar, chord, 0.55 if bar < 8 else 0.4)
        song.add("drums", bar * 4, 0, "kick", 0.7)
        song.add("drums", bar * 4 + 2, 0, "kick", 0.5)
        song.add("drums", bar * 4 + 3, 0, "snare", 0.35)
        for e in range(8):
            song.add("drums", bar * 4 + e * 0.5, 0, "hat", 0.25 if e % 2 else 0.15)
        if bar >= 8:
            song.melody(bar, MENU_MELODY[bar - 8], 0.85)
    return song


TRACKS = {
    "Music_Menu": (menu_track, {"lead": 0.45, "arp": 0.3, "bass": 0.7, "drums": 0.35}),
    "Music_Game": (game_track, {"lead": 0.45, "arp": 0.28, "bass": 0.7, "drums": 0.5}),
}


def main() -> None:
    random.seed(298)
    MUSIC_DIR.mkdir(parents=True, exist_ok=True)
    preview = "--preview" in sys.argv
    if preview:
        PREVIEW_DIR.mkdir(parents=True, exist_ok=True)
    for name, (make, mix) in TRACKS.items():
        samples = render(make(), mix)
        path = MUSIC_DIR / f"{name}.wav"
        write(path, samples)
        print(f"Wrote {path.relative_to(ROOT)} ({len(samples) / RATE:.1f} s)")
        if preview:
            excerpt = PREVIEW_DIR / f"{name}_preview.wav"
            write(excerpt, samples[: 12 * RATE])
            print(f"Wrote {excerpt.relative_to(ROOT)}")


if __name__ == "__main__":
    main()
