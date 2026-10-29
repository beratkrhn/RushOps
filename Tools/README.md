# Tools

Python-Skripte zur reproduzierbaren Erzeugung von Assets (Pixelart, Soundeffekte). Nicht Teil des Unity-Builds; Unity ignoriert diesen Ordner, weil er außerhalb von `Assets/` liegt.

| Skript | Erzeugt |
|---|---|
| `gen_pixelart.py` | Palette (`Art/Palettes/RushOps_Palette.png`, 32 Farben) und die per Skript gezeichnete Pixelart nach ADR 0006: Walker in Schrägansicht (8 Richtungen × 4 Lauf-Frames, 64×64), Projektil, Mündungsfeuer, Boden (3 Varianten), Wand oben und Wand mit Vorderseite (32×32). Bricht ab, wenn eine Farbe außerhalb der Palette liegt. Mit `--preview <pfad>` entsteht ein hochskaliertes Vorschaubild. |
| `import_character.py` | Übernimmt einen Character-Export (Ordner mit `metadata.json`, 8 Richtungen) nach `Art/Sprites/…` und benennt ihn nach ADR 0006 um (`Player_Idle_SE.png`). Prüft 64×64, Transparenz und Palettenfarben. Beispiel: `python3 Tools/import_character.py Muslim_with_Thobe-Idle Player` |

Nach dem Erzeugen setzt Unity die Importeinstellungen automatisch (`PixelArtImportPostprocessor`: Sprite, 32 PPU, Point, keine Kompression, keine Mip Maps, Figuren mit Pivot unten Mitte). Danach `RushOps → Art → Richtungs-Animationen aktualisieren` ausführen, damit neue Frames in `Data/Animations/` landen. Neue Farben kommen nur über `PALETTE` im Skript hinzu, und die Palette bleibt bei höchstens 32 Farben (ADR 0006, Test `PixelArtPaletteTests`).
