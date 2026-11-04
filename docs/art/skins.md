# Skins

Erzeugt von `Tools/gen_skins.py` (Issue #129). Jeder Skin ist eine Umfärbung der Spielerfigur aus dem Character-Tool: Die Idle-Posen werden umgefärbt, Laufen, Purzelbaum (Dash), Treffer und Tod daraus genauso abgeleitet wie beim Standard-Skin. Alle Skins haben also dieselben Animationen. Nur Farben aus der Palette.

Vorschau zur Abnahme: [`skins/skins.png`](skins/skins.png) (Spalten: Standard und die 12 Skins in Katalog-Reihenfolge; Zeilen: S, SE, E, N und das Ende der Todesanimation) und [`skins/skins.gif`](skins/skins.gif) (alle laufen zum Betrachter).

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

| Skin | Freischaltung | Idee |
|---|---|---|
| Rettungsschwimmer | Level 1-1 | Rot mit gelben Akzenten |
| Surfer | Level 1-2 | Meerblau |
| Sandsturm | Level 1-3 | Sand und Braun |
| Nachtwache | Level 2-1 | Schiefergrau mit blauen Lichtern |
| Neon | Level 2-2 | Mauve mit Neonblau |
| Rost | Level 2-3 | Rostbraun mit Orange |
| Glut | Level 3-1 | Dunkelrot mit Glut |
| Asche | Level 3-2 | Aschgrau mit Glutresten |
| Obsidian | Level 3-3 | Schwarz mit roten Adern |
| Veteran | 100 Kills | Oliv und Braun |
| Zombiejäger | 1000 Kills | Dunkelgrün mit Blutrot |
| Goldstern | 27 Sterne | Gold |

Neue Farben eines Skins ändert man in `SKINS` in `Tools/gen_skins.py`, danach das Skript ausführen und in Unity „RushOps → Skins → Skin-Katalog aktualisieren“.
