# Changelog

Alle relevanten Änderungen an RushOps. Format nach [Keep a Changelog](https://keepachangelog.com/de/1.1.0/), Versionierung nach [SemVer](https://semver.org/lang/de/).

## [Unreleased]

### Hinzugefügt
- Projekt-Grundgerüst: Lastenheft v0.2, `CLAUDE.md`, ADRs 0001–0005, Issue- und PR-Vorlagen, Backlog für v0.1.0.
- Git LFS für Grafik-, Audio- und Schriftdateien.
- Ordnerstruktur unter `Assets/_Project/`.
- Assembly Definitions `RushOps.Runtime`, `RushOps.Editor`, `RushOps.Tests.EditMode`, `RushOps.Tests.PlayMode` inklusive Beispieltests (Issue #1).
- Testszene `Arena_Test` (40×60 Kacheln, Wände mit `TilemapCollider2D` + `CompositeCollider2D`, Hindernisse), per Editor-Skript `RushOps → Szenen → Arena_Test erzeugen` reproduzierbar (Issue #2).
- Pixel Perfect Camera (180×320, 16 PPU) mit weichem Folgen, Klemmen an die Levelgrenzen und Pixel-Snapping; Folgezeit in `CameraConfig` (Issue #2).
- Platzhalter-Tiles und Debug-Ziel per `Tools/gen_placeholder_arena.py`; temporärer `DebugTargetMover` (WASD/Pfeiltasten/Gamepad) bis zur Spielerbewegung (Issue #2).
- Feste Palette mit 24 Farben (`Art/Palettes/RushOps_Palette.png`) und Pixelart per `Tools/gen_pixelart.py`: Spieler, Walker mit 4-Frame-Laufanimation, Projektil, Mündungsfeuer, drei Bodenvarianten, Wand (Issue #3).
- `PixelArtImportPostprocessor` setzt für alle PNGs unter `Art/` die Importeinstellungen aus ADR 0002; EditMode-Tests prüfen Importeinstellungen, Palettentreue und Sprite-Größen (Issue #3).
- Spieler-Character (Idle, 8 Richtungen) aus dem Character-Tool, übernommen per `Tools/import_character.py` (Issue #58).
- Richtungs-Sprites: `Direction8`/`Direction8Math` (Vektor → 8 Richtungen), `DirectionalAnimation` (ScriptableObject, Frames pro Richtung, Bildrate) und `DirectionalSpriteAnimator`; Animationen unter `Data/Animations/` per `RushOps → Art → Richtungs-Animationen aktualisieren` (Issue #58).
- Walker in Schrägansicht (8 Richtungen × 4 Lauf-Frames) und Wand-Tiles mit Vorderseite (Issue #58).
- ADR 0006: Schrägansicht mit 8 Richtungs-Sprites und 32-px-Raster (Issue #58).

### Geändert
- Bildschirmausrichtung fest auf Hochformat.
- `bundleVersion` auf 0.1.0, Firmenname gesetzt.
- `SampleScene` von `Assets/Scenes/` nach `Assets/_Project/Scenes/` verschoben (Issue #1).
- ADR 0002: Verhalten bei abweichendem Seitenverhältnis festgelegt (mehr Welt zeigen, keine Balken) (Issue #2).
- `Arena_Test` ist die erste Szene in den Build Settings (Issue #2).
- `Arena_Test` nutzt die neuen Boden- und Wand-Tiles (Bodenvarianten deterministisch verteilt); das Debug-Ziel zeigt das Spieler-Sprite (Issue #3).
- Darstellung von reiner Top-down-Ansicht mit rotierten Sprites auf Schrägansicht mit 8 Richtungs-Sprites umgestellt; Lastenheft F-GFX-02 (v0.3), ADR 0002 teilweise ersetzt (Issue #58).
- Raster 32×32 px, 32 PPU, Pixel Perfect Camera 360×640; Palette auf 32 Farben erweitert; Figuren mit Pivot an den Füßen, Sortierung nach Y (Issue #58).
- `Arena_Test`: Debug-Ziel zeigt den Spieler-Character und blickt in Laufrichtung; Reihe mit 8 Walkern als Grafikvorschau (Issue #58).

### Entfernt
- Platzhalter-Tiles und Debug-Ziel aus Issue #2 samt `Tools/gen_placeholder_arena.py` (Issue #3).
- 16-px-Spieler, Top-down-Walker und einteiliges Wand-Tile aus Issue #3 (Issue #58).
