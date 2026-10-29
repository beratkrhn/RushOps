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
- Ist der Editor offen, Editor-Skripte (z. B. Szenen-Builder) bevorzugt über `unity command editor invoke_menu` im offenen Editor ausführen statt Szenendateien von außen zu überschreiben.

### Kompilieren und Testen (Unity CLI, ADR 0007)

```bash
Tools/unity-check.sh            # kompilieren (Warnungen = Fehler), EditMode- und PlayMode-Tests
Tools/unity-check.sh compile    # nur kompilieren; auch: editmode, playmode
```

- Ist Unity mit dem Projekt offen, läuft alles **im offenen Editor** über das Pipeline-Paket (etwa 20 s). Unity muss dafür nicht geschlossen werden. Vor PlayMode-Tests sollte die offene Szene gespeichert sein.
- Ist kein Editor offen, fällt das Skript auf `unity run` / `unity test` im Batchmode zurück (etwa 2 min).
- Exit-Codes: `0` ok, `1` Compile-Fehler oder -Warnungen, `8` Tests rot, `6` Lauf nicht abgeschlossen. Details liegen als JSON/XML unter `Logs/check-*`.
- Voraussetzung: Unity CLI (`brew install --cask unity-cli`, Befehl `unity`).

Weitere nützliche Befehle gegen den offenen Editor:

```bash
unity command editor invoke_menu --path "RushOps/Szenen/Arena_Test erzeugen"   # Editor-Skript per Menü
unity command console get --level warn --tail 20                                # Console lesen
unity command                                                                   # alle Befehle auflisten
```

Bei `unity command` immer das Flag `--project-path .` mitgeben, wenn mehrere Editoren offen sein können. `editor.playmode set_run_in_background` nicht benutzen (ändert die PlayerSettings). Die Define-Symbole `ENABLE_RUNTIME_PIPELINE*` nie setzen; das Paket gehört nicht in Spiel-Builds.

### Rückfallweg: Unity direkt im Batchmode (macOS)

Nur nötig, falls CLI oder Pipeline nicht funktionieren. Schlägt fehl, solange das Projekt im Editor geöffnet ist.

```bash
UNITY="/Applications/Unity/Hub/Editor/6000.3.10f1/Unity.app/Contents/MacOS/Unity"

# Kompilieren prüfen
"$UNITY" -batchmode -nographics -quit -projectPath . -logFile -

# EditMode-Tests (ohne -quit, das beendet der Test-Runner selbst)
"$UNITY" -batchmode -nographics -projectPath . -runTests -testPlatform EditMode \
  -testResults Logs/editmode-results.xml -logFile -
```

## Pixelart

Die Details stehen in ADR 0006 (ersetzt Teile von ADR 0002). Kurzfassung:

- Kachelgröße **32×32 px**, **Pixels Per Unit = 32** (1 Kachel = 1 Unity-Einheit).
- Import: Filter Mode **Point**, Compression **None**, Mip Maps aus.
- Referenzauflösung der Pixel Perfect Camera **360×640** (Hochformat 9:16).
- Feste Palette (höchstens 32 Farben) unter `Assets/_Project/Art/Palettes/`. Keine Farben außerhalb der Palette.
- Schrägansicht mit **8 Richtungs-Sprites** (`E`, `NE`, `N`, `NW`, `W`, `SW`, `S`, `SE`); Figuren werden nicht rotiert. Figuren-Leinwand 64×64, Pivot unten Mitte, Sortierung nach Y. Projektile und Effekte rotieren frei.
- Assets werden per Skript in `Tools/` erzeugt, damit sie reproduzierbar sind. Erzeugte PNGs vor dem Commit hochskaliert ansehen.
- Ausnahme: vom Menschen gelieferte Figuren (Export aus seinem Character-Tool) werden mit `Tools/import_character.py` übernommen, nicht neu gezeichnet.

## Git-Workflow

- `main` ist immer lauffähig. Gearbeitet wird auf Branches, gemergt per Pull Request.
- Branch-Namen: `feature/<issue>-<kurz>`, `fix/<issue>-<kurz>`, `chore/<kurz>`, `docs/<kurz>`.
- **Conventional Commits**, Beschreibung auf Deutsch: `feat(player): Dash mit i-Frames`, `fix(enemies): Runner bleibt an Ecken hängen`, `docs: ADR zu Flow-Field`, `chore: …`, `refactor: …`, `test: …`, `art: …`, `audio: …`.
- PRs verweisen auf das Issue (`Closes #12`) und folgen der PR-Vorlage.
- Versionen nach SemVer, ein Milestone pro Version, Tag `vX.Y.Z` bei Release. `bundleVersion` in den ProjectSettings passt zur Version.

## Ablauf bei „Mach das nächste Issue“

Das GitHub-Repo ist `beratkrhn/RushOps`. Für GitHub-Zugriffe die REST-API über `gh api` verwenden.

1. **Ausgangslage prüfen:** `git checkout main && git pull`. Ist das Arbeitsverzeichnis nicht sauber oder gibt es noch einen offenen PR von mir, der nicht gemergt ist: nachfragen, bevor es weitergeht.
2. **Issue bestimmen:** `Tools/next-issue.sh` ausführen. Regel: ältester offener Milestone, Label `story`, Issues mit Label `priority` zuerst, dann niedrigste Nummer, ohne `blocked`, `needs-design` oder `in-progress`. Hat der Mensch eine Nummer genannt, diese nehmen.
3. **Issue lesen:** `gh api repos/beratkrhn/RushOps/issues/<N>` und den passenden Abschnitt in `docs/backlog/`. Die referenzierten Anforderungen im Lastenheft nachlesen.
4. **Unklarheiten zuerst klären:** Widersprechen sich Issue und Lastenheft, oder fehlt eine Designentscheidung, nachfragen statt raten. Kann das Issue nicht umgesetzt werden (z. B. Abhängigkeit fehlt), Label `blocked` setzen, Grund als Kommentar ins Issue schreiben und stoppen. Ist eine Story größer als etwa zwei Tage, eine Aufteilung vorschlagen.
5. **Starten:** Label `in-progress` setzen (`gh api repos/beratkrhn/RushOps/issues/<N>/labels -f "labels[]=in-progress"`), Branch `feature/<N>-<kurz>` (bzw. `fix/…`) von `main` anlegen.
6. **Umsetzen** nach den Regeln dieser Datei, in kleinen Conventional Commits.
7. **Prüfen:** `Tools/unity-check.sh` (kompilieren ohne Warnungen, EditMode- und PlayMode-Tests). Unity darf dabei offen bleiben. Nur wenn das Skript im Batchmode-Rückfall landet und das Projekt trotzdem geöffnet ist, den Menschen bitten, Unity zu schließen. Alle Fehler beheben, bevor ein PR entsteht.
8. **Abschließen:** `CHANGELOG.md` ergänzen, pushen, PR nach Vorlage öffnen (`Closes #<N>`). Im Abschnitt „So testest du das“ konkrete Schritte für den manuellen Test im Editor oder auf dem Gerät aufschreiben.
9. **Stoppen.** Kein weiteres Issue beginnen, bevor der PR gemergt ist, außer der Mensch sagt es ausdrücklich. Dem Menschen kurz sagen, was fertig ist und was er testen soll.

Bei „Mach die nächsten N Issues“ werden die Issues nacheinander abgearbeitet, jedes auf eigenem Branch mit eigenem PR. Baut ein Issue auf einem ungemergten vorherigen auf, wird der neue Branch vom vorherigen abgezweigt und das im PR vermerkt.

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
