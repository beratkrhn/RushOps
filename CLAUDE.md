# RushOps – Projektkontext für Claude

Diese Datei wird zu Beginn jeder Claude-Sitzung gelesen. Sie beschreibt, wie in diesem Repo gearbeitet wird. Bei Widersprüchen gilt: Lastenheft für *was*, diese Datei für *wie*.

## Projekt

RushOps ist ein mobiler 2D-Top-down-Twin-Stick-Shooter (Zombiehorden, Perks, Action-Rätsel) im Pixelart-Stil, offline, nur Hochformat, Sprache Deutsch.

- Anforderungen: [`docs/lastenheft.md`](docs/lastenheft.md) (Anforderungs-IDs wie `F-STE-03`)
- Architekturentscheidungen: [`docs/adr/`](docs/adr/)
- Backlog pro Version: [`docs/backlog/`](docs/backlog/)
- Änderungen pro Version: [`CHANGELOG.md`](CHANGELOG.md)

**Stand:** v1.0.0 (Kampagne fertig). Es gibt noch **kein Release**; TestFlight, Gesamtspieltest und Einreichung (#194 bis #196) kommen in einen Milestone „Release“ ganz am Ende. Als Nächstes kommt v1.1.0 Endlos-Modus. Am Gerät wird getestet, wann Berat es sagt.

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
- Bezeichner und Code-Kommentare auf **Englisch**.
- **Commits, Branches, Pull Requests, Issues und Kommentare darin auf Englisch.**
- Doku im Repo (Lastenheft, ADRs, Backlog, `CHANGELOG.md`) auf **Deutsch**. Spieltexte auf Deutsch.
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

### Gerätebuild iOS (Issue #13)

Getestet wird auf einem iPhone (kein Android-Gerät vorhanden).

```bash
Tools/ios-install.sh              # Xcode-Projekt exportieren (Batchmode), bauen, signieren, installieren, starten
Tools/ios-install.sh --no-export  # nach „RushOps → Build → iOS-Xcode-Projekt (Development) bauen“ im offenen Editor
```

- Nur die Installation `6000.3.10f1-arm64` hat das iOS-Modul (die Unity-CLI meldet es fälschlich auch für `6000.3.10f1`). Das Skript sucht sie automatisch. Soll aus dem offenen Editor gebaut werden, muss das Projekt mit dieser Installation geöffnet sein.
- Signiert wird automatisch mit dem Team `WAM6U438Y8`. Logs liegen unter `Logs/build-ios.log`, `Logs/xcodebuild-ios.log` und `Logs/devicectl-ios.log`.

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
- Die Spielerfigur liegt roh unter `Tools/characters/Player/`; `gen_pixelart.py` zeichnet ihr die Pistole in die Hand und leitet daraus alle Spieler-Frames, die Skins und die Mündungspunkte (`Player_Muzzles.json`) ab.

## Git-Workflow

- `main` ist immer lauffähig. Gearbeitet wird auf Branches, gemergt per Pull Request.
- Branch-Namen auf Englisch: `feature/<issue>-<short>`, `fix/<issue>-<short>`, `chore/<short>`, `docs/<short>`.
- **Conventional Commits auf Englisch**: `feat(player): add dash with i-frames`, `fix(enemies): runner no longer gets stuck on corners`, `docs: add ADR for flow field`, `chore: …`, `refactor: …`, `test: …`, `art: …`, `audio: …`.
- **Modell im Commit**: Jede Commit-Nachricht endet mit einer Trailer-Zeile, die das verwendete Modell nennt:

  ```
  feat(endless): add round counter

  Model: Claude Sonnet 5.5
  ```

- **Sonst wird Claude nirgends erwähnt**: keine `Co-Authored-By`-Zeilen, keine Session-Links, kein „Generated with …“, keine Prompts, Chatverläufe oder Hinweise auf KI in Commits, PRs, Issues oder Kommentaren. Die einzige Ausnahme ist die `Model:`-Zeile im Commit. Das gilt auch, wenn das Werkzeug solche Zeilen standardmäßig anhängen will.
- PRs verweisen auf das Issue (`Closes #12`) und folgen der PR-Vorlage.
- **In-Game-Screenshot**: Zu jeder sichtbaren Änderung kommt, wenn möglich, mindestens ein Screenshot aus dem Spiel im Hochformat in den PR (z. B. per PlayMode-Testklasse wie `StoreScreenshots` oder Editor-Skript im Device Simulator aufgenommen). Ist kein Screenshot möglich, steht im PR, warum.
- Versionen nach SemVer, ein Milestone pro Version. Tags `vX.Y.Z` und `bundleVersion` erst ab dem Release.

## Modellwahl vor jedem Issue

Bevor die Arbeit an einem Issue beginnt, schickt Claude Berat **eine kurze Nachricht auf Deutsch** und wartet auf seine Antwort:

```
Issue #<nr> – <Titel>
Aktuelles Modell: <Modell>
Empfehlung: <Haiku | Sonnet | Opus> – <ein Satz, warum>
Modell wechseln? (/model)
```

Faustregel für die Empfehlung:

| Modell | Wofür |
|---|---|
| Haiku | Doku, Backlog, Changelog, Werte in ScriptableObjects ändern, kleine Fixes in einer Datei |
| Sonnet | Normale Stories nach bestehendem Muster: neue Perks, UI-Elemente, Level mit dem Baukasten, Tests, Pixelart- und Sound-Skripte |
| Opus | Neue Systeme und Architektur (z. B. Endlos-Modus-Grundlage, Flow-Field-Ziele, Boss-Verhalten, Savegame-Migration), ADRs, schwierige Bugs, Performance |

Reicht ein kleineres Modell, sagt Claude das ausdrücklich. Erst nach Berats Antwort geht es weiter.

## Ablauf bei „Mach das nächste Issue“

Das GitHub-Repo ist `beratkrhn/RushOps`. Für GitHub-Zugriffe die REST-API über `gh api` verwenden.

1. **Ausgangslage prüfen:** `git checkout main && git pull`. Ist das Arbeitsverzeichnis nicht sauber oder gibt es noch einen offenen PR von mir, der nicht gemergt ist: nachfragen, bevor es weitergeht.
2. **Issue bestimmen:** `Tools/next-issue.sh` ausführen. Regel: ältester offener Milestone, Label `story`, Issues mit Label `priority` zuerst, dann niedrigste Nummer, ohne `blocked`, `needs-design` oder `in-progress`. Hat der Mensch eine Nummer genannt, diese nehmen.
3. **Issue lesen:** `gh api repos/beratkrhn/RushOps/issues/<N>` und den passenden Abschnitt in `docs/backlog/`. Die referenzierten Anforderungen im Lastenheft nachlesen.
4. **Modell abfragen:** Nachricht nach „Modellwahl vor jedem Issue“ schicken und auf die Antwort warten. Wechselt Berat das Modell, liest das neue Modell die Schritte 1 bis 3 noch einmal.
5. **Unklarheiten zuerst klären:** Widersprechen sich Issue und Lastenheft, oder fehlt eine Designentscheidung, nachfragen statt raten. Kann das Issue nicht umgesetzt werden (z. B. Abhängigkeit fehlt), Label `blocked` setzen, Grund als Kommentar ins Issue schreiben und stoppen. Ist eine Story größer als etwa zwei Tage, eine Aufteilung vorschlagen.
6. **Starten:** Label `in-progress` setzen (`gh api repos/beratkrhn/RushOps/issues/<N>/labels -f "labels[]=in-progress"`), Branch `feature/<N>-<short>` (bzw. `fix/…`) von `main` anlegen.
7. **Umsetzen** nach den Regeln dieser Datei, in kleinen Conventional Commits auf Englisch mit `Model:`-Zeile.
8. **Prüfen:** `Tools/unity-check.sh` (kompilieren ohne Warnungen, EditMode- und PlayMode-Tests). Unity darf dabei offen bleiben. Nur wenn das Skript im Batchmode-Rückfall landet und das Projekt trotzdem geöffnet ist, den Menschen bitten, Unity zu schließen. Alle Fehler beheben, bevor ein PR entsteht.
9. **Abschließen:** `CHANGELOG.md` ergänzen, pushen, PR nach Vorlage auf Englisch öffnen (`Closes #<N>`), mit In-Game-Screenshot bei sichtbaren Änderungen. Im Abschnitt „How to test“ konkrete Schritte für den manuellen Test im Editor oder auf dem Gerät aufschreiben.
10. **Stoppen.** Kein weiteres Issue beginnen, bevor der PR gemergt ist, außer der Mensch sagt es ausdrücklich. Dem Menschen kurz sagen, was fertig ist und was er testen soll.

Bei „Mach die nächsten N Issues“ werden die Issues nacheinander abgearbeitet, jedes auf eigenem Branch mit eigenem PR. Baut ein Issue auf einem ungemergten vorherigen auf, wird der neue Branch vom vorherigen abgezweigt und das im PR vermerkt.

## Nachtmodus (unbeaufsichtigt)

Berat startet `Tools/nachtlauf.sh [max_issues] [model]` im Terminal (Laptop offen, am Strom, Unity-Editor mit dem Projekt am besten offen). Das Skript startet pro Issue eine **eigene Claude-Code-Sitzung mit frischem Kontext**; der Prompt beginnt mit „Nachtmodus“. Dann gilt der Ablauf bei „Mach das nächste Issue“ mit diesen Abweichungen:

- **Niemand antwortet.** Keine Rückfragen und keine Modell-Nachricht (Schritt 4). Stattdessen am Ende einen Eintrag an `Logs/nachtlauf.md` anhängen: Datum und Uhrzeit, Issue, verwendetes Modell, empfohlenes Modell mit einem Satz Begründung, Ergebnis, Branch, PR-Link. `Logs/` ist nicht im Repo.
- **Schritt 1:** Offene PRs aus früheren Nachtläufen sind erlaubt. Der neue Branch zweigt vom Branch des zuletzt geöffneten, noch ungemergten Nachtlauf-PRs ab (steht in `Logs/nachtlauf.md`), sonst von `main`. Im PR steht dann „Builds on #<nr>“. Ist das Arbeitsverzeichnis nicht sauber: `Logs/nachtlauf.stop` mit Grund anlegen und beenden.
- **Schritt 5:** Bei Unklarheiten zuerst die Entscheidungen in `docs/backlog/design-fragen.md` und die Startwerte im Backlog anwenden. Fehlt wirklich eine Designentscheidung: Label `blocked`, Kommentar ins Issue, `Logs/nachtlauf.stop` mit Grund anlegen und beenden.
- **Schritt 8:** Lassen sich Compile-Fehler oder rote Tests nicht beheben: Zwischenstand auf dem Branch pushen, Draft-PR öffnen (`gh api … -F draft=true`), Label `blocked` setzen, `Logs/nachtlauf.stop` anlegen und beenden.
- **Nach dem PR** bleibt das Label `in-progress` gesetzt, damit `Tools/next-issue.sh` das Issue überspringt.
- **Nie:** nach `main` pushen, PRs mergen, Issues schließen, Szenen, Prefabs oder Assets löschen, Pakete hinzufügen, Dateien außerhalb des Repos ändern, Designentscheidungen ändern.
- Ist der Unity-Editor offen, im offenen Editor arbeiten (Pipeline, `unity command`). Screenshots per PlayMode-Test aufnehmen.
- **Genau ein Issue pro Sitzung.** Zum Schluss eine kurze Zusammenfassung ausgeben.

Am Morgen liest Berat `Logs/nachtlauf.md`, prüft die PRs in Reihenfolge und mergt sie.

## Definition of Done (pro Story)

1. Akzeptanzkriterien des Issues erfüllt.
2. Projekt kompiliert ohne Fehler und ohne neue Warnungen.
3. EditMode-Tests für neue Logik vorhanden und grün.
4. Balancing-Werte in ScriptableObjects, nicht im Code.
5. Im Device Simulator (Hochformat) angespielt; am Gerät nur, wenn Berat es verlangt. Ergebnis im PR vermerkt.
6. In-Game-Screenshot im PR (bei sichtbaren Änderungen, sofern möglich).
7. `CHANGELOG.md` unter „Unreleased“ ergänzt.
8. Bei Architekturentscheidungen: ADR angelegt.

## Was Claude nicht allein entscheidet

- Änderungen am Spieldesign, die vom Lastenheft abweichen: erst nachfragen.
- Neue Pakete oder Asset-Store-Abhängigkeiten: erst nachfragen.
- Löschen von Szenen, Prefabs oder Assets: erst nachfragen.
- Spielgefühl (Tempo, Sprungweiten, Schwierigkeit) wird vorgeschlagen, aber vom Menschen am Gerät abgenommen.
- Mit einem Issue anfangen, bevor Berat auf die Modell-Nachricht geantwortet hat.
