# RushOps – Projektkontext für Claude

Diese Datei wird zu Beginn jeder Claude-Sitzung gelesen. Sie beschreibt, wie in diesem Repo gearbeitet wird. Bei Widersprüchen gilt: Lastenheft für *was*, diese Datei für *wie*.

## Projekt

RushOps ist ein mobiler 2D-Top-down-Twin-Stick-Shooter (Zombiehorden, Perks, Action-Rätsel) im Pixelart-Stil, offline, nur Hochformat, Sprache Deutsch.

- Anforderungen: [`docs/lastenheft.md`](docs/lastenheft.md) (Anforderungs-IDs wie `F-STE-03`)
- Architekturentscheidungen: [`docs/adr/`](docs/adr/)
- Backlog pro Version: [`docs/backlog/`](docs/backlog/)
- Änderungen pro Version: [`CHANGELOG.md`](CHANGELOG.md)

## Technik

| | |
|---|---|
| Engine | Unity **6000.3.10f1** (Unity 6) |
| Rendering | URP 2D (Renderer2D, 2D-Licht) |
| Eingabe | Input System (neues System), kein altes `Input.GetAxis` |
| Tests | Unity Test Framework (EditMode + PlayMode) |
| Plattformen | iOS, Android; Entwicklung auf macOS (Apple Silicon) |
| Serialisierung | Force Text (Szenen, Prefabs und Assets sind YAML) |
| Große Dateien | Git LFS (siehe `.gitattributes`) |

## Ordnerstruktur

Alles Eigene liegt unter `Assets/_Project/`. Unity-Standardordner (`Settings`, Paket-Assets) bleiben, wo Unity sie anlegt.

```
Assets/_Project/
  Art/        Sprites, Tiles, Paletten, Animationen
  Audio/      Music, SFX
  Data/       ScriptableObject-Instanzen (Balancing, Gegner, Perks, Level)
  Prefabs/
  Scenes/
  Scripts/
    Runtime/  Spielcode (Assembly RushOps.Runtime)
    Editor/   Editor-Tools (Assembly RushOps.Editor)
  Tests/
    EditMode/ (Assembly RushOps.Tests.EditMode)
    PlayMode/ (Assembly RushOps.Tests.PlayMode)
Tools/        Python-Skripte für Asset-Erzeugung (Pixelart, SFX), nicht Teil des Builds
```

## Architekturregeln

- **Datengetrieben**: Alle Balancing-Werte (Tempo, Schaden, Dauer, Drop-Chancen, Wellen) liegen in ScriptableObjects unter `Data/`, nie als Konstanten im Code. Jeder Wert mit *(Startwert)* im Lastenheft gehört dorthin.
- **Logik testbar halten**: Reine Regeln (Schadensberechnung, Perk-Pool, Kill-Zähler, Kombo, Savegame-Migration) in einfachen C#-Klassen ohne `MonoBehaviour`, damit sie in EditMode-Tests laufen. `MonoBehaviour`s sind dünne Adapter.
- **Entkopplung über Events**: Systeme kommunizieren über C#-Events oder ScriptableObject-Event-Kanäle, nicht über `FindObjectOfType` oder Singletons-Ketten.
- **Pooling**: Projektile, Gegner, Partikel und Pickups kommen aus Object Pools (`UnityEngine.Pool`).
- **Keine Allokationen in `Update`** (keine LINQ-Ausdrücke, keine `new List` pro Frame, kein `GetComponent` pro Frame).
- Größere Entscheidungen als ADR in `docs/adr/` festhalten (Vorlage: `0000-vorlage.md`).

## Code-Konventionen

- Namespaces: `RushOps.<Bereich>` (z. B. `RushOps.Player`, `RushOps.Enemies`, `RushOps.Perks`).
- Bezeichner und Code-Kommentare auf **Englisch**; Doku, Issues und PR-Texte auf **Deutsch**. Spieltexte auf Deutsch.
- C#-Standard: `PascalCase` für Typen, Methoden, Properties; `_camelCase` für private Felder; `[SerializeField] private` statt öffentlicher Felder.
- Eine Klasse pro Datei, Dateiname = Klassenname.
- XML-Doku-Kommentare für öffentliche APIs von Systemen.

## Arbeiten mit Unity-Dateien

- **Jede neue Datei unter `Assets/` braucht ihre `.meta`-Datei.** Beim Anlegen, Verschieben oder Löschen immer die `.meta` mitnehmen. GUIDs in bestehenden `.meta`-Dateien nie ändern.
- Szenen und Prefabs bevorzugt **per Editor-Skript** erzeugen oder ändern (`Scripts/Editor/`), statt YAML von Hand zu bearbeiten. Kleine Werteänderungen im YAML sind in Ordnung; neue Objekte mit `fileID`-Verweisen nicht von Hand.
- Ist der Unity-Editor offen, können Dateiänderungen von außen kollidieren. Vor größeren Änderungen an Szenen nachfragen, ob der Editor geschlossen ist.

### Unity im Batchmode (macOS)

```bash
UNITY="/Applications/Unity/Hub/Editor/6000.3.10f1/Unity.app/Contents/MacOS/Unity"

# Kompilieren prüfen
"$UNITY" -batchmode -nographics -quit -projectPath . -logFile -

# EditMode-Tests (ohne -quit, das beendet der Test-Runner selbst)
"$UNITY" -batchmode -nographics -projectPath . -runTests -testPlatform EditMode \
  -testResults Logs/editmode-results.xml -logFile -
```

Ein Batchmode-Lauf schlägt fehl, solange das Projekt im Editor geöffnet ist.

## Pixelart

Die Details stehen in ADR 0002. Kurzfassung:

- Kachelgröße **16×16 px**, **Pixels Per Unit = 16** (1 Kachel = 1 Unity-Einheit).
- Import: Filter Mode **Point**, Compression **None**, Mip Maps aus.
- Referenzauflösung der Pixel Perfect Camera **180×320** (Hochformat 9:16).
- Feste Palette unter `Assets/_Project/Art/Palettes/`. Keine Farben außerhalb der Palette.
- Reine Top-down-Ansicht: Figuren schauen im Sprite nach **rechts (+X)** und werden zur Zielrichtung rotiert.
- Assets werden per Skript in `Tools/` erzeugt, damit sie reproduzierbar sind. Erzeugte PNGs vor dem Commit hochskaliert ansehen.

## Git-Workflow

- `main` ist immer lauffähig. Gearbeitet wird auf Branches, gemergt per Pull Request.
- Branch-Namen: `feature/<issue>-<kurz>`, `fix/<issue>-<kurz>`, `chore/<kurz>`, `docs/<kurz>`.
- **Conventional Commits**, Beschreibung auf Deutsch: `feat(player): Dash mit i-Frames`, `fix(enemies): Runner bleibt an Ecken hängen`, `docs: ADR zu Flow-Field`, `chore: …`, `refactor: …`, `test: …`, `art: …`, `audio: …`.
- PRs verweisen auf das Issue (`Closes #12`) und folgen der PR-Vorlage.
- Versionen nach SemVer, ein Milestone pro Version, Tag `vX.Y.Z` bei Release. `bundleVersion` in den ProjectSettings passt zur Version.

## Definition of Done (pro Story)

1. Akzeptanzkriterien des Issues erfüllt.
2. Projekt kompiliert ohne Fehler und ohne neue Warnungen.
3. EditMode-Tests für neue Logik vorhanden und grün.
4. Balancing-Werte in ScriptableObjects, nicht im Code.
5. Auf dem Handy oder im Device Simulator (Hochformat) angespielt; Ergebnis im PR vermerkt.
6. `CHANGELOG.md` unter „Unreleased“ ergänzt.
7. Bei Architekturentscheidungen: ADR angelegt.

## Was Claude nicht allein entscheidet

- Änderungen am Spieldesign, die vom Lastenheft abweichen: erst nachfragen.
- Neue Pakete oder Asset-Store-Abhängigkeiten: erst nachfragen.
- Löschen von Szenen, Prefabs oder Assets: erst nachfragen.
- Spielgefühl (Tempo, Sprungweiten, Schwierigkeit) wird vorgeschlagen, aber vom Menschen am Gerät abgenommen.
