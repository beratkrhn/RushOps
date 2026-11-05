# Tools

Python-Skripte zur reproduzierbaren Erzeugung von Assets (Pixelart, Soundeffekte). Nicht Teil des Unity-Builds; Unity ignoriert diesen Ordner, weil er außerhalb von `Assets/` liegt.

| Skript | Erzeugt |
|---|---|
| `gen_pixelart.py` | Palette (`Art/Palettes/RushOps_Palette.png`, 32 Farben) und die per Skript gezeichnete Pixelart nach ADR 0006: Walker und Runner in Schrägansicht (je 8 Richtungen × 4 Lauf-Frames, 64×64), Brute (8 Richtungen × 4 Lauf-Frames, 96×96), Boomer (8 Richtungen × 4 pulsierende Lauf-Frames, 64×64), Explosionseffekt (6 Frames, 160×160), Schutzschild-Blase (64×64), Projektil, Mündungsfeuer, Boden (3 Varianten), Wand oben und Wand mit Vorderseite (32×32). Bricht ab, wenn eine Farbe außerhalb der Palette liegt. Mit `--preview <pfad>` entsteht ein hochskaliertes Vorschaubild. |
| `gen_icon.py` | App-Icon (Issue #191, R7): Spielerfigur mit Pistole vor einem hellen Vollmond, dahinter ein abgedunkelter Walker mit roten Augen, auf 64×64 aus den Sprites und der Palette gezeichnet und ohne Glättung auf 1024×1024 hochskaliert (`Art/Icon/AppIcon.png`, deckend, ohne abgerundete Ecken). Mit `--preview docs/art` entsteht `icon_preview.png` in 60, 120 und 180 px. Danach in Unity „RushOps → Build → App-Icon setzen“ (passiert auch bei „Player-Einstellungen anwenden“). |
| `gen_skins.py` | Die 6 Spieler-Skins (je einer pro Insel, drei Meilensteine) als Umfärbungen der importierten Spielerfigur (Issue #129): Idle umgefärbt, Laufen, Treffer und Tod daraus abgeleitet, nach `Art/Sprites/Player/Skins/`. Mit `--preview docs/art/skins` entstehen `skins.png` und `skins.gif` zur Abnahme. Farben und Rollen: `docs/art/skins.md`. Danach in Unity „RushOps → Skins → Skin-Katalog aktualisieren“. |
| `import_character.py` | Übernimmt einen Character-Export (Ordner mit `metadata.json`, 8 Richtungen) nach `Art/Sprites/…` und benennt ihn nach ADR 0006 um (`Player_Idle_SE.png`). Prüft 64×64, Transparenz und Palettenfarben. Die Spielerfigur landet roh in `Tools/characters/Player/`; `gen_pixelart.py` gibt ihr die Pistole in die Hand und schreibt die Idle-, Lauf-, Treffer-, Tod- und Dash-Frames sowie `Sprites/Player/Player_Muzzles.json` (Mündung pro Richtung). Beispiel: `python3 Tools/import_character.py Muslim_with_Thobe-Idle Player` |

Nach dem Erzeugen setzt Unity die Importeinstellungen automatisch (`PixelArtImportPostprocessor`: Sprite, 32 PPU, Point, keine Kompression, keine Mip Maps, Figuren mit Pivot unten Mitte). Danach `RushOps → Art → Richtungs-Animationen aktualisieren` ausführen, damit neue Frames in `Data/Animations/` landen. Neue Farben kommen nur über `PALETTE` im Skript hinzu, und die Palette bleibt bei höchstens 32 Farben (ADR 0006, Test `PixelArtPaletteTests`).

## Shell-Skripte

| Skript | Zweck |
|---|---|
| `unity-check.sh` | Kompilieren (Warnungen = Fehler), EditMode- und PlayMode-Tests (ADR 0007) |
| `unity-perf.sh` | Leistungsmessung im Editor (Issue #189): Stress-Szene, je ein Level pro Insel, Hauptmenü; gibt die Tabelle für `docs/testprotokolle/performance.md` aus. Unity muss geschlossen sein |
| `store-screenshots.sh` | Store-Screenshots (Issue #192): fünf Szenen in 1320×2868 nach `docs/store/screenshots/`. Unity muss geschlossen sein; im offenen Editor „RushOps → Store → Screenshots aufnehmen“ |
| `ios-install.sh` | Development-Build auf das iPhone (Issue #13) |
| `next-issue.sh` | Nächstes Issue nach der Regel in `CLAUDE.md` |
