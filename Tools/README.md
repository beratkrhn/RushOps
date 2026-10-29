# Tools

Python-Skripte zur reproduzierbaren Erzeugung von Assets (Pixelart, Soundeffekte). Nicht Teil des Unity-Builds; Unity ignoriert diesen Ordner, weil er außerhalb von `Assets/` liegt.

| Skript | Erzeugt |
|---|---|
| `gen_pixelart.py` | Palette (`Art/Palettes/RushOps_Palette.png`, 24 Farben) und die Pixelart für v0.1.0: Spieler, Walker (4 Lauf-Frames), Projektil, Mündungsfeuer, Boden (3 Varianten), Wand. Bricht ab, wenn eine Farbe außerhalb der Palette liegt. Mit `--preview <pfad>` entsteht ein hochskaliertes Vorschaubild. |

Nach dem Erzeugen setzt Unity die Importeinstellungen automatisch (`PixelArtImportPostprocessor`: Sprite, 16 PPU, Point, keine Kompression, keine Mip Maps). Neue Farben kommen nur über `PALETTE` im Skript hinzu, und die Palette bleibt bei 16–24 Farben (ADR 0002, Test `PixelArtPaletteTests`).
