# ADR 0006: Schrägansicht mit 8 Richtungs-Sprites und 32-px-Raster

- **Status:** angenommen
- **Datum:** 2026-10-29
- **Ersetzt:** Perspektive, Raster und Figurengrößen aus [ADR 0002](0002-pixelart-topdown.md)

## Kontext

ADR 0002 hat eine reine Top-down-Ansicht mit 16×16-Figuren festgelegt, die per Transform zur Zielrichtung gedreht werden. Der Product Owner hat einen eigenen Spieler-Character in Schrägansicht („low top-down“) erstellt: 64×64 px Leinwand, Figur etwa 25×54 px, je ein Sprite für 8 Richtungen. Er soll unverändert verwendet werden. Er hat entschieden, die Darstellung darauf umzustellen (Issue #58).

## Entscheidung

| Punkt | Wert |
|---|---|
| Perspektive | Schrägansicht von vorn oben. Figuren werden **nicht rotiert**, sondern zeigen je Blickrichtung ein eigenes Sprite. |
| Richtungen | 8: `E`, `NE`, `N`, `NW`, `W`, `SW`, `S`, `SE` (Sektoren zu je 45°, Grenzen bei 22,5° + n · 45°) |
| Kachelgröße | 32×32 px |
| Pixels Per Unit | 32 (1 Kachel = 1 Unity-Einheit) |
| Referenzauflösung | 360×640 px (Pixel Perfect Camera, Hochformat 9:16) |
| Spielfigur, Walker, Runner, Boomer | Leinwand 64×64 px, Figur etwa 24–28 px breit und 50–56 px hoch, Füße 5 px über dem unteren Rand |
| Brute | Leinwand 96×96 px |
| Bosse | Leinwand 128×128 px |
| Pivot von Figuren | unten Mitte (`Sprites/Player/`, `Sprites/Enemies/`), damit nach Y sortiert werden kann |
| Sortierung | Renderer2D mit Custom Axis (0, 1, 0): weiter unten = weiter vorne |
| Projektile, Effekte | werden weiterhin frei in Flugrichtung rotiert |
| Wände | Draufsicht plus sichtbare Vorderseite, wo südlich keine Wand anschließt |
| Palette | eine feste Palette, höchstens 32 Farben, Datei unter `Assets/_Project/Art/Palettes/` |
| Animation | 4–6 Frames pro Zyklus und Richtung |
| Dateinamen | `<Figur>_<Zustand>_<Richtung>[_<Frame>].png`, z. B. `Player_Idle_NE.png`, `Walker_Walk_S_2.png` |

Unverändert aus ADR 0002 gelten: Import mit Filter Mode Point, ohne Kompression, ohne Mip Maps, sowie der Nachtrag zu abweichenden Seitenverhältnissen. Seine Zahlen verdoppeln sich in Pixeln, bleiben in Einheiten aber gleich (360×640 bei 32 PPU = 11,25×20 Einheiten wie zuvor 180×320 bei 16 PPU). Die Kamera rundet auf 1/32 Einheit.

## Begründung und Alternativen

- **Beibehaltung von ADR 0002 (Draufsicht, rotiert):** weniger Grafikaufwand, aber der vorhandene Character wäre nicht nutzbar gewesen. Verworfen durch den Product Owner.
- **16-px-Raster mit neu exportiertem 32×32-Character:** weniger Details, Character hätte neu erzeugt werden müssen. Verworfen.
- **Kontinuierliche Rotation zusätzlich zu den 8 Sprites:** Eine Figur in Schrägansicht kippt beim Drehen. Verworfen. Die Schussrichtung bleibt trotzdem stufenlos, nur das Figuren-Sprite rastet auf 8 Richtungen ein.
- **Gespiegelte Westseite:** Für skriptgenerierte Figuren erlaubt (`W`, `NW`, `SW` aus `E`, `NE`, `SE` gespiegelt), handgezeichnete Figuren dürfen eigene Westansichten haben.

## Konsequenzen

- Etwa vierfacher Grafikaufwand pro Figur (5 eindeutige Richtungen statt einer, 4× so viele Pixel). Gegner werden per Skript in `Tools/` erzeugt.
- Die Zielrichtung wird für das Sprite auf 8 Richtungen gerundet (`Direction8`), Projektile fliegen stufenlos.
- Figuren und Wände müssen nach Y sortiert werden; Kollider sitzen an den Füßen, nicht in der Bildmitte.
- Licht und Schatten kommen von oben vorn; Figuren dürfen eine eingebaute Lichtrichtung haben.
- Bereits erzeugte 16-px-Grafiken aus Issue #3 werden ersetzt.
