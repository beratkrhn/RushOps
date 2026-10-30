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
- Spielerbewegung über die Input-Action `Player/Move` (Gamepad-Stick, WASD, Pfeiltasten): analoges Tempo je nach Stick-Auslenkung, kurze Beschleunigung und Abbremsung, Wandkollision über `Rigidbody2D` mit kontinuierlicher Kollisionserkennung (Issue #4).
- `PlayerConfig` (`Data/Player/PlayerConfig.asset`) mit Tempo 5, Beschleunigung 40 und Abbremsung 50 *(Startwerte)* (Issue #4).
- Zielen und automatisches Feuern: Input-Actions `Player/Aim` (rechter Gamepad-Stick) sowie `Player/AimPoint`/`Player/AimPress` (Maus im Editor); der Spieler zeigt das Richtungs-Sprite zur Zielrichtung, ohne Zieleingabe zur Laufrichtung; gefeuert wird, solange die Auslenkung über der Totzone liegt; unbegrenzte Munition, kein Nachladen (Issue #5).
- `WeaponConfig` (`Data/Weapons/WeaponConfig.asset`) mit Feuerrate 6/s, Projektiltempo 14 und Reichweite 10 *(Startwerte)*; Totzone 0,3 in `PlayerConfig` (Issue #5).
- Projektile (`Prefabs/Projectiles/Bullet.prefab`) aus einem Object Pool, stoppen an Wänden und verschwinden nach maximaler Reichweite; neue Physik-Layer `Walls`, `Player`, `Projectiles` (Issue #5).
- Unity CLI und Unity-Pipeline-Paket (`com.unity.pipeline` 0.9.1-exp.1, nur Editor): `Tools/unity-check.sh` kompiliert und testet im offenen Editor, ohne Unity zu schließen, und fällt sonst auf den Batchmode zurück (Issue #62).
- EditMode-Test `PipelineBuildGuardTests` stellt sicher, dass das Pipeline-Paket nicht in Spiel-Builds landet (Issue #62).
- ADR 0007: Unity CLI und Pipeline-Paket für Kompilieren und Tests (Issue #62).
- Dash mit i-Frames über die Input-Action `Player/Dash` (Leertaste, rechte Maustaste, Gamepad Süd/rechte Schultertaste): in Laufrichtung, im Stand in Blickrichtung; Wände stoppen den Dash; `PlayerDash` bietet `IsInvulnerable` für Schadensquellen, `Lock`/`Unlock` zum Sperren (für den Objekt-Transport) und `CooldownProgress` für den Dash-Button (Issue #6).
- Dash-Werte in `PlayerConfig`: Distanz 3, Dauer 0,2 s, i-Frames 0,3 s, Abklingzeit 1,0 s ab Dash-Beginn *(Startwerte)* (Issue #6).
- Touch-Steuerung: linker und rechter virtueller Stick als On-Screen-Controls des Input Systems (schreiben in ein virtuelles Gamepad, Multitouch), Modus **floating** (Stick erscheint am Berührungspunkt der jeweiligen Bildschirmhälfte, Standard) und **fest** (Stick in der unteren Ecke); untätige Sticks werden abgedunkelt (Issue #7).
- Dash-Button oberhalb des rechten Sticks mit radialer Anzeige der Abklingzeit (Issue #7).
- Safe Area: `SafeAreaFitter` hält Sticks und Dash-Button aus Notch, Kamera-Aussparung und Home-Indikator heraus (Issue #7).
- `TouchControlsConfig` (`Data/UI/TouchControlsConfig.asset`): Modus floating, Stick-Durchmesser 96, Knopf 50 %, Randabstand 24, Dash-Button 72 mit Abstand 16 (Referenzeinheiten bei 360×640), Abdunkelung 0,5 *(Startwerte)* (Issue #7).
- Pixelart für Stick-Ring, Stick-Knopf, Dash-Button und Abklingzeit-Overlay unter `Art/Sprites/UI/` per `Tools/gen_pixelart.py` (Issue #7).
- ADR 0008: Touch-Steuerung als On-Screen-Controls in uGUI (Issue #7).
- Herzen, Schaden und Neustart: `PlayerHealth` mit 5 Herzen, Kontaktschaden von Gegnern (`IContactDamage`), danach 1,0 s Unverwundbarkeit mit Blinken; Dash-i-Frames schützen ebenfalls; bei 0 Herzen stoppt der Spieler, färbt sich rot und nach 1,0 s startet das Level neu (`LevelRestarter`); Event `HealthChanged` für das HUD (Issue #8).
- Reine Gesundheitslogik `Health` (Schaden, Unverwundbarkeitsfenster, Heilen bis Maximum, Tod) mit EditMode-Tests, auch für Gegner nutzbar (Issue #8).
- `EnemyDefinition` (`Data/Enemies/Walker.asset`) mit Kontaktschaden 1 Herz *(Startwert)*; die Vorschau-Walker in `Arena_Test` verletzen bei Berührung (Issue #8).
- Neue Werte in `PlayerConfig`: 5 Herzen, Unverwundbarkeit 1,0 s, Blinkintervall 0,1 s, Tod-Moment 1,0 s *(Startwerte)* (Issue #8).
- Walker verfolgen den Spieler über ein Flow-Field auf dem Wand-Raster (ADR 0003): `FlowField` (Dijkstra, 8 Richtungen, keine abgeschnittenen Wandecken, ohne Allokationen) mit EditMode-Tests, `FlowFieldNavigator` rechnet bei Kachelwechsel des Spielers neu, höchstens alle 0,1 s; Walker steuern auf die Mitte der nächsten Kachel und bleiben so nicht an Ecken hängen (Issue #9).
- Separation: Walker drücken sich gegenseitig weg (`SteeringMath`) und blockieren sich über ihre Kollider; sie blockieren auch den Spieler, schieben ihn aber kaum (Masse 10 zu 1) (Issue #9).
- Walker-Prefab (`Prefabs/Enemies/Walker.prefab`) aus dem `EnemyPool`: Gesundheit, nimmt Projektilschaden (`IDamageable`), stirbt mit kurzem Schrumpf- und Ausblend-Effekt und kehrt in den Pool zurück; Event `Died` (Issue #9).
- `EnemyDefinition` (Walker): Gesundheit 3, Tempo 2,5, Kontaktschaden 1, Todeseffekt 0,25 s, Punkte 10 *(Startwerte)*; `WeaponConfig`: Schaden 1; `NavigationConfig` (`Data/Navigation/`): Neuberechnung 0,1 s, Separation Radius 0,9 und Gewicht 1,2 *(Startwerte)* (Issue #9).
- Physik-Layer `Enemies`; Projektile stoppen an Wänden und Gegnern (Issue #9).
- `Arena_Test`: 10 Walker starten über die Arena verteilt (temporärer `DebugEnemySpawner` bis zu den Spawnpunkten in #10) (Issue #9).
- Spawnpunkte: `SpawnPoint`-Objekte in der Szene (8 in `Arena_Test`, Ränder und Ecken) und `EnemySpawner`, der bis zu 10 Gegner gleichzeitig aktiv hält (angekündigte mitgezählt); vor jedem Spawn blinkt 0,6 s eine rote Bodenmarkierung (Issue #10).
- Spawnpunkte im Kamerabild (plus 1 Einheit Rand) oder näher als 4 Einheiten am Spieler werden übersprungen; die Regel wird beim Erscheinen erneut geprüft (`SpawnRules`, mit EditMode-Tests) (Issue #10).
- `SpawnerConfig` (`Data/Enemies/SpawnerConfig.asset`): höchstens 10 Gegner, Abstand 0,8 s, Ankündigung 0,6 s, Mindestabstand 4, Sichtrand 1 *(Startwerte)*; Pixelart `Sprites/Effects/SpawnMarker.png` (Issue #10).
- HUD (eigener Canvas über der Touch-Steuerung, in der Safe Area): Herzen oben links, ein verlorenes Herz blinkt 0,6 s rot (`HeartsView`, `HudConfig` in `Data/UI/`) (Issue #11).
- Pause: Button oben rechts, Overlay mit „Fortsetzen“ und „Neustart“; die Spielzeit steht still, das Overlay blockiert die Touch-Steuerung; pausiert automatisch, wenn die App in den Hintergrund geht (`PauseController`) (Issue #11).
- Pixel-Schrift `PixelFont`, selbst gezeichnet (5×7-Raster in `Tools/pixel_font.py`, A–Z, a–z, Umlaute, ß, Ziffern, Satzzeichen), als Bitmap-Font für uGUI `Text`; Atlas und Metriken per `Tools/gen_pixelart.py`, Font-Asset per `RushOps → Art → Pixel-Schrift aktualisieren`; Lizenz in `Art/Fonts/README.md` (Issue #11).
- Pixelart für Herzen (voll, leer) und Pause-Symbol (Issue #11).
- Treffer-Feedback: Gegner blitzen bei Treffer weiß auf (Shader `RushOps/Sprite Flash`) und werden entlang der Schussrichtung zurückgestoßen; leichter Screenshake bei Schaden am Spieler; kurzer Hitstop (0,05 s) bei Gegnertod, abschaltbar und mit der Pause verträglich (Issue #12).
- `FeelConfig` (`Data/Feel/FeelConfig.asset`): Blitz 0,08 s, Rückstoß 4 für 0,1 s, Shake 0,15 für 0,2 s, Hitstop an mit 0,05 s, Lautstärken und Tonhöhen-Streuung *(Startwerte)* (Issue #12).
- Platzhalter-Soundeffekte für Schuss, Treffer und Tod, synthetisiert per `Tools/gen_sfx.py` (`Audio/SFX/`), abgespielt über `SfxPlayer`; `GameFeel` verbindet Spiel-Events mit Shake, Hitstop und Sounds (Issue #12).
- Zielhilfe: Liegt ein lebender Gegner höchstens 10° neben der Zielrichtung und in 8 Einheiten Reichweite, geht der Schuss auf den nächstgelegenen (`AimAssist`, Auswahl in `AimAssistMath` mit EditMode-Tests); Winkel, Reichweite und Schalter in `WeaponConfig` *(Startwerte)*, zur Laufzeit per `AimAssist.Enabled` umschaltbar (Issue #14).
- Build: `RushOps → Build → Android-APK (Development|Release) bauen` (auch per `-executeMethod`) wendet die Player-Einstellungen aus Code an und baut nach `Builds/Android/` (Issue #13).
- FPS-Anzeige oben mittig, nur in Development-Builds und im Editor (Issue #13).
- Vorlage für das Testprotokoll `docs/testprotokolle/v0.1.0.md` (Issue #13).

### Geändert
- Bildschirmausrichtung fest auf Hochformat.
- `bundleVersion` auf 0.1.0, Firmenname gesetzt.
- `SampleScene` von `Assets/Scenes/` nach `Assets/_Project/Scenes/` verschoben (Issue #1).
- ADR 0002: Verhalten bei abweichendem Seitenverhältnis festgelegt (mehr Welt zeigen, keine Balken) (Issue #2).
- `Arena_Test` ist die erste Szene in den Build Settings (Issue #2).
- `Arena_Test` nutzt die neuen Boden- und Wand-Tiles (Bodenvarianten deterministisch verteilt); das Debug-Ziel zeigt das Spieler-Sprite (Issue #3).
- Die Blickrichtung des Spielers setzt jetzt `PlayerAim` statt `PlayerMovement` (Issue #5).
- Darstellung von reiner Top-down-Ansicht mit rotierten Sprites auf Schrägansicht mit 8 Richtungs-Sprites umgestellt; Lastenheft F-GFX-02 (v0.3), ADR 0002 teilweise ersetzt (Issue #58).
- Raster 32×32 px, 32 PPU, Pixel Perfect Camera 360×640; Palette auf 32 Farben erweitert; Figuren mit Pivot an den Füßen, Sortierung nach Y (Issue #58).
- `Arena_Test`: Debug-Ziel zeigt den Spieler-Character und blickt in Laufrichtung; Reihe mit 8 Walkern als Grafikvorschau (Issue #58).
- `Arena_Test`: Das Debug-Ziel ist jetzt der Spieler mit `PlayerMovement` und `PlayerMoveInput` (Issue #4).
- Die Touch-Steuerung erscheint nur mit Touchscreen (Handy, Device Simulator); im Game View bleiben Maus und Tastatur wie bisher (Issue #7).
- Kontaktschaden trifft schon bei der ersten Berührung, nicht erst im nächsten Physikschritt (Issue #9).
- Gegner aus dem Pool erscheinen um wenige Hundertstel versetzt, weil die Physik exakt übereinanderliegende Körper nicht trennen kann (Issue #10).
- Sortierreihenfolge: Bodenmarkierungen liegen zwischen Boden und Figuren (Issue #10).
- Kein Dash, solange die Spielzeit stillsteht (Pause) (Issue #11).
- Gegner werden mit dem eigenen Sprite-Shader unbeleuchtet gezeichnet; bei nur globalem Licht sieht das gleich aus (Issue #12).
- Bundle-ID `com.beratkrhn.rushops` (iOS und Android), Mindestversionen Android 8.0 (API 26) und iOS 15, Android mit IL2CPP und ARM64, keine Autorotation; Lastenheft v0.4 (Issue #13).

### Entfernt
- Platzhalter-Tiles und Debug-Ziel aus Issue #2 samt `Tools/gen_placeholder_arena.py` (Issue #3).
- 16-px-Spieler, Top-down-Walker und einteiliges Wand-Tile aus Issue #3 (Issue #58).
- Temporärer `DebugTargetMover` (Issue #4).
- `CLAUDE.md`: Prüfen per `Tools/unity-check.sh` statt Batchmode; Unity darf dabei offen bleiben (Issue #62).
- Walker-Vorschaureihe in `Arena_Test` und `EnemyContactDamage`; ersetzt durch echte Walker (Issue #9).
- Temporärer `DebugEnemySpawner`; ersetzt durch Spawnpunkte (Issue #10).
