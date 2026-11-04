# ADR 0010: Level-Baukasten aus Code-Beschreibungen

- **Status:** angenommen
- **Datum:** 2026-11-04

## Kontext

Für v0.6.0 bis v0.8.0 kommen acht Level dazu. Level 1-1 wurde von einem eigenen Builder (`Level1_1SceneBuilder`, rund 300 Zeilen) erzeugt, der Aufbau, Prüfung und Verdrahtung der Objekte mischte. Ab Insel 2 braucht es seitliche Abzweigungen (Entscheidung L3), Tore an mehreren Bedingungen (L2) und Schwierigkeits-Stellschrauben pro Insel (C3).

## Entscheidung

- Ein Level wird als `LevelBlueprint` beschrieben (Editor-Code unter `Scripts/Editor/Scenes/`): Größe, Start, Wände, Trennwände mit Toren (waagrecht oder senkrecht), Zonen mit Spawnpunkten und Wellen, Rätsel, Ausgang, Kampagnen-Platz und Schwierigkeit.
- `LevelBlueprintBuilder` prüft jede Beschreibung gegen `docs/level-design.md`, baut daraus die Szene über `ArenaTestSceneBuilder.BuildScene` und trägt das Level in die `Campaign` ein.
- Ein Tor mit Kills und Rätseln bekommt eine `GateConditionGroup` (alle Bedingungen müssen erfüllt sein); der Missions-Tracker zeigt beide Zeilen untereinander.
- Schwierigkeit pro Insel liegt im ScriptableObject `LevelDifficulty` (`Data/Levels/Difficulty_Insel1..3`): Wellendichte (teilt Spawn-Abstand und Wellenpause), Faktor auf Schalter-Zeitlimits, Zusatz-Haltezeit. Gegnerwerte bleiben unverändert.

## Begründung und Alternativen

- *Level als ScriptableObject-Daten oder Tiled-Import:* bequemer für Nicht-Programmierer, aber Wellen und Rätsel verweisen auf Assets und Lambdas; der Aufwand für Editor-UI lohnt bei neun Leveln nicht. Die Beschreibung in C# bleibt kurz, versioniert und testbar.
- *Szenen von Hand im Editor:* nicht reproduzierbar und widerspricht der Regel, Szenen per Editor-Skript zu erzeugen.
- *Wellendichte als mehr Gegner pro Welle:* die Höchstzahl von 10 Gegnern begrenzt das ohnehin; kürzere Abstände machen Wellen spürbar dichter, ohne die Wellen-Assets pro Insel zu verdoppeln.

## Konsequenzen

- Ein neues Level ist eine Klasse mit einer Beschreibung und einem Menüeintrag; die Regeln der Level-Design-Richtlinie werden bei jedem Bau und im EditMode-Test geprüft.
- Wellen-Assets werden nur beim ersten Bau angelegt; danach gelten die im Asset getunten Werte.
- Senkrechte Tore stapeln die Gitterstäbe übereinander; ihr Aussehen wird beim ersten Level mit Abzweigung am Gerät abgenommen.
