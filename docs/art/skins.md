# Skins

Erzeugt von `Tools/gen_skins.py` (Issue #129). Jeder Skin ist eine Umfärbung der Spielerfigur aus dem Character-Tool: Die Idle-Posen (mit der Pistole in der Hand, siehe `Tools/gen_pixelart.py`) werden umgefärbt, Laufen, Purzelbaum (Dash), Treffer und Tod daraus genauso abgeleitet wie beim Standard-Skin. Alle Skins haben also dieselben Animationen. Nur Farben aus der Palette.

Vorschau zur Abnahme: [`skins/skins.png`](skins/skins.png) (Spalten: Standard und die 6 Skins in Katalog-Reihenfolge; Zeilen: S, SE, E, N und das Ende der Todesanimation) und [`skins/skins.gif`](skins/skins.gif) (alle laufen zum Betrachter).

## Farbrollen

| Farbe im Standard | Bereich | Rolle |
|---|---|---|
| `navy_0` | dunkler Stoff, Turban, Binnenlinien | `dark` (Außenkontur wird `outline`) |
| `navy_1` | Stoff-Lichter, Turbanfalten | `mid` |
| `dusk` | Umhang und Gewand | `cloak` |
| `mauve` | Umhang-Licht, Brustriemen | `light` |
| `clay` | Rüstung und Haut, Schatten | `accent_0` |
| `apricot` | Rüstung und Haut, Licht | `accent_1` |

`cream` und `peach` (Glanz in den Augen) bleiben.

## Skins

Ein Skin pro Insel, freigeschaltet mit ihrem letzten Level (F-SKN-02), dazu Meilenstein-, Endlos-, Gold- und Abzeichen-Skins.

| Skin | Freischaltung | Idee |
|---|---|---|
| Sandsturm | Insel Strand (Level 1-3) | Sand und Braun |
| Rost | Insel Düstere Stadt (Level 2-3) | Rostbraun mit Orange |
| Obsidian | Insel Vulkan (Level 3-3) | Schwarz mit roten Adern |
| Veteran | 100 Kills | Oliv und Braun |
| Zombiejäger | 1000 Kills | Dunkelgrün mit Blutrot |
| Goldstern | 27 Sterne | Gold |
| Gladiator | Endlos-Runde 10 | Rot und Braun |
| Zenturio | Endlos-Runde 25 | Grau mit Rot |
| Imperator | Endlos-Runde 50 | Weiß und Gold |
| Goldrausch | 2500 Gold | ganz in Gold |
| Späher | 9 Abzeichen (Issue #288) | brauner Umhang, grüne Rüstung |
| Elite | 18 Abzeichen | Schwarz mit goldener Rüstung |
| Legende | 27 Abzeichen | heller, leuchtender Umhang, blaue Rüstung |

Neue Farben eines Skins ändert man in `SKINS` in `Tools/gen_skins.py`, danach das Skript ausführen und in Unity „RushOps → Skins → Skin-Katalog aktualisieren“.
