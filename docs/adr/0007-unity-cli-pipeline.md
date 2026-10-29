# ADR 0007: Unity CLI und Pipeline-Paket für Kompilieren und Tests

- **Status:** angenommen
- **Datum:** 2026-10-29

## Kontext

Bisher wurde per Unity-Batchmode kompiliert und getestet (`Unity -batchmode -runTests …`). Das schlägt fehl, solange das Projekt im Editor geöffnet ist. Vor jeder Prüfung musste der Mensch Unity schließen, und ein Lauf dauerte inklusive Editor-Start etwa zwei Minuten.

Unity bietet die **Unity CLI** (`unity`, eigenes Programm, Version `1.0.0-beta.14`) und das Paket **Unity Pipeline** (`com.unity.pipeline`). Das Paket startet im Editor einen lokalen Server, über den die CLI einen laufenden Editor steuert.

Im Test an RushOps (Issue #62) ergab sich:

- `unity recompile --strict` kompiliert im offenen Editor und meldet Fehler und Warnungen mit Datei und Zeile (Exit 6 bei Fehler oder Warnung).
- `unity command test run --mode editor` führt die EditMode-Tests im offenen Editor aus: 84 Tests in etwa 1 s.
- PlayMode-Tests laufen nur asynchron (`--async_tests true`, danach `test status` abfragen), weil der Start des Play Mode die Domain neu lädt. Ein Editor im Hintergrund drosselt seine Frames; erst mit `editor set_auto_tick` laufen die 16 PlayMode-Tests in etwa 12 s statt in mehreren Minuten.
- Editor-Menüs (z. B. der Szenen-Builder) lassen sich per `unity command editor invoke_menu` ausführen.
- `unity test` (ohne Pipeline) startet nur einen Batchmode-Editor und bricht bei geöffnetem Projekt ab.

## Entscheidung

- Das Pipeline-Paket wird mit **fester Version** (`0.9.1-exp.1`) eingebunden, nur als Editor-Werkzeug.
- `Tools/unity-check.sh` ist der eine Befehl für „kompilieren und testen“. Ist ein Editor mit Pipeline erreichbar (mit Oberfläche oder ohne), prüft es darin; sonst fällt es auf `unity run` / `unity test` im Batchmode zurück.
- Der Runtime-Teil des Pakets wird **nie** in Spiel-Builds kompiliert. Die Define-Symbole `ENABLE_RUNTIME_PIPELINE` und `ENABLE_RUNTIME_PIPELINE_DEV` bleiben ungesetzt; der EditMode-Test `PipelineBuildGuardTests` prüft das für iOS, Android und Standalone.
- Die bisherigen reinen Unity-Batchmode-Befehle bleiben in `CLAUDE.md` als letzter Rückfallweg dokumentiert.

## Begründung und Alternativen

- **Weiter nur Batchmode:** stabil, aber Unity muss jedes Mal geschlossen werden und jeder Lauf kostet etwa zwei Minuten. Verworfen als Standard, bleibt Rückfallweg.
- **Nur Unity CLI ohne Pipeline (`unity test`):** kürzere Befehle und klare Exit-Codes, löst aber das Hauptproblem (Editor offen) nicht. Wird im Rückfallweg genutzt.
- **Unity CLI mit Pipeline:** Prüfen bei offenem Editor in rund 20 s. Preis: experimentelles Paket und Beta-CLI.

## Konsequenzen

- Der Mensch kann Unity offen lassen, während Claude kompiliert und testet. Vor PlayMode-Tests sollte die offene Szene gespeichert sein, sonst kann ein Speichern-Dialog den Lauf blockieren.
- **Experimentell:** Paket (`-exp`) und CLI (Beta) können sich zwischen Versionen ändern. Updates nur bewusst über ein eigenes Issue (`unity pipeline upgrade`), danach `Tools/unity-check.sh` in beiden Modi prüfen. Funktioniert die Pipeline nicht, greift der Batchmode-Rückfallweg.
- **Sicherheit:** Der Editor-Server lauscht nur lokal (127.0.0.1) und erlaubt Code-Ausführung im Editor (`script eval`). Das gilt nur auf dem Entwicklungsrechner; Spiel-Builds sind über den Guard-Test abgesichert.
- `ProjectSettings/Packages/com.unity.pipeline/EditorPipelineConfig.json` wird versioniert. `editor.playmode set_run_in_background` wird nicht benutzt, weil es `runInBackground` in den PlayerSettings umschaltet.
- Hat das Projekt Compile-Fehler, startet ein Editor mit Oberfläche im Safe Mode ohne Pipeline. Dann Fehler im Code beheben (Meldungen über den Batchmode-Rückfallweg) und Unity neu starten.
- Die CLI sendet laut ihrer Doku anonyme Absturzberichte (abschaltbar mit `UNITY_NO_CRASH_REPORT`) und einen anonymen Nutzungs-Ping an Unity.
