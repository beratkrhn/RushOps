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
- Bei Displays mit anderem Seitenverhältnis als 9:16 wird mehr von der Welt gezeigt, siehe Nachtrag.

## Nachtrag (v0.1.0, Issue #2): Abweichende Seitenverhältnisse

**Entscheidung:** Die Pixel Perfect Camera skaliert ganzzahlig und **zeigt auf Displays, die nicht genau 9:16 sind, mehr von der Welt**. Es gibt keine schwarzen Balken.

| Einstellung | Wert |
|---|---|
| Crop Frame | None |
| Grid Snapping | Pixel Snapping |
| Referenzauflösung | 180×320, 16 PPU |

- Die Skalierung ist die größte ganze Zahl, bei der 180×320 noch auf den Bildschirm passt. Der restliche Platz zeigt zusätzliches Level.
- Beispiel: Ein iPhone 13 (1170×2532) skaliert ×6 und zeigt etwa 195×422 px (rund 12×26 Kacheln) statt 180×320 px.
- Die Kamera folgt dem Spieler weich (`CameraFollow`, Wert in `CameraConfig`) und wird mit dem **tatsächlich sichtbaren** Bereich an den Levelgrenzen geklemmt. Ist das Level auf einer Achse kleiner als die Sicht, wird es auf dieser Achse zentriert.
- Die Kameraposition wird auf ganze Kunstpixel (1/16 Einheit) gerundet, damit beim Folgen nichts zittert.

**Verworfen:**
- *Balken mit festem Sichtfeld (Crop X/Y):* Alle sehen genau gleich viel, aber auf aktuellen hohen Handys (etwa 9:19,5) bleibt ein großer Teil des Bildschirms schwarz.
- *Breite fest, Höhe variabel:* Seitlich wären alle Geräte gleich, aber bei breiteren Geräten (Tablets) entstehen seitliche Balken. Bringt gegenüber der gewählten Lösung wenig.

**Konsequenzen:**
- Auf hohen Geräten sieht man nach oben und unten mehr. Für das Balancing gilt 180×320 als Mindest-Sichtbereich: Spawnpunkte und Rätsel müssen darin funktionieren, dürfen sich aber nicht darauf verlassen, dass außerhalb davon nichts sichtbar ist.
- Die Mindestgröße eines Levels sollte 12×26 Kacheln (bzw. 180×320 px) nicht unterschreiten, sonst zentriert die Kamera und der Rand wird sichtbar.
