# ADR 0002: Pixelart-Spezifikation und reine Top-down-Ansicht

- **Status:** angenommen
- **Datum:** 2026-10-29

## Kontext

Grafiken werden größtenteils per Skript erzeugt. Damit alles zusammenpasst, braucht es feste Regeln für Größe, Auflösung, Palette und Perspektive. Twin-Stick-Shooter brauchen frei drehbare Ziel- und Blickrichtungen.

## Entscheidung

| Punkt | Wert |
|---|---|
| Perspektive | Reines Top-down. Figuren schauen im Sprite nach rechts (+X) und werden per Transform zur Zielrichtung rotiert. |
| Kachelgröße | 16×16 px |
| Pixels Per Unit | 16 |
| Spielfigur, Walker, Runner, Boomer | 16×16 px |
| Brute | 24×24 px |
| Bosse | 32×32 bis 48×48 px |
| Referenzauflösung | 180×320 px (Pixel Perfect Camera, Hochformat 9:16) |
| Import | Filter Mode Point, Compression None, Mip Maps aus |
| Palette | eine feste Palette, Datei unter `Assets/_Project/Art/Palettes/` |
| Animation | 4–6 Frames pro Zyklus |

## Begründung und Alternativen

- **3/4-Ansicht (wie Zelda)** sieht figürlicher aus, braucht aber jede Animation für 4 oder 8 Richtungen. Bei frei drehbarem Zielen wirkt das ruckelig. Etwa vierfacher Grafikaufwand. Verworfen.
- **Rotierte Sprites** haben bei Pixelart das Problem, dass Rotationen um krumme Winkel Pixel verzerren. Das ist im Genre üblich und akzeptiert. Optional später: Rotation auf 16 oder 32 Stufen runden.
- 180×320 ergibt auf 1080×1920-Displays eine ganzzahlige Skalierung (×6) und zeigt etwa 11×20 Kacheln.

## Konsequenzen

- Sprites werden einmal gezeichnet, Rotation übernimmt Unity.
- Licht und Schatten müssen von oben gedacht werden, keine fest eingebaute Lichtrichtung im Sprite.
- Bei Displays mit anderem Seitenverhältnis entstehen Ränder oder Zuschnitt, das Verhalten der Pixel Perfect Camera wird in v0.1.0 festgelegt.
