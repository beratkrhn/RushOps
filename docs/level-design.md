# Level-Design-Richtlinie

Regeln für den Bau von Levels, damit alle Gegnertypen sie durchqueren können. Maße in Kacheln (1 Kachel = 32 px = 1 Unity-Einheit, ADR 0006).

## Gänge und Durchlässe

| Regel | Wert | Grund |
|---|---|---|
| Mindestbreite von Gängen | **2 Kacheln** | Der Brute (Körperradius 0,45) passt zwar durch 1 Kachel, blockiert sie aber vollständig. Erst ab 2 Kacheln kommen kleinere Zombies und der Spieler an ihm vorbei. |
| Tore (`Gate`) | mindestens 2 Kacheln, Standard 4 | wie oben; Tore sind Engpässe, durch die ganze Wellen müssen |
| Einzelne Lücken von 1 Kachel | nur als bewusstes Gestaltungsmittel | Walker, Runner und Spieler kommen durch, der Brute verstopft sie |
| Diagonale Durchlässe (zwei Wände berühren sich nur an der Ecke) | vermeiden | Das Flow-Field schneidet keine Wandecken; solche Lücken sind für alle Gegner zu. |

## Körpergrößen der Figuren

Die Kollider sitzen an den Füßen (ADR 0006). Die Werte stehen in den Szenen-Buildern (`Scripts/Editor/Scenes/ArenaTestSceneBuilder.cs`).

| Figur | Radius | Durchmesser |
|---|---|---|
| Spieler | 0,30 | 0,60 |
| Walker | 0,28 | 0,56 |
| Runner | 0,24 | 0,48 |
| Brute | 0,45 | 0,90 |

Kein Gegner darf einen Radius von 0,5 oder mehr haben: Das Flow-Field führt Gegner über Kachelmitten, also 0,5 neben Wänden entlang. Ein breiterer Körper würde an Wänden und Ecken hängen bleiben. Größere Gegner (Bosse) brauchen eine eigene Lösung.

## Hindernisse

- Hindernisse liegen im Kachelraster der Wand-Tilemap; alles, was dort eine Kachel hat, blockiert Bewegung und Pfadsuche.
- Zwischen Hindernis und Außenwand bleiben mindestens 2 Kacheln frei, sonst entsteht ein Engpass nach der Tabelle oben.
- Spawnpunkte liegen auf freien Kacheln mit mindestens 1 Kachel Abstand zu Wänden, damit auch der Brute frei erscheint.

## Spawnpunkte und Sicht

- Gegner erscheinen nie im Kamerabild oder nah am Spieler (`SpawnRules`, Abstand und Rand in `SpawnerConfig`). Ein Spawnpunkt, der gerade sichtbar ist, wird übersprungen. Damit eine Zone nicht verstummt, braucht sie Spawnpunkte an **beiden Seiten und am fernen Ende**: Das Kamerabild ist 11,25 × 20 Kacheln groß, eine 24 Kacheln breite Zone hat also immer eine Seite außerhalb der Sicht.
- Keine Spawnpunkte direkt am Eingang einer Zone; der Spieler soll nicht beim Durchschreiten eines Tores angefallen werden.

## Rätsel

- Mindestens ein Teil jedes Rätsels (Kreis, ein Schalter, Generator) liegt in **Sichtlinie vom Zoneneingang** und höchstens eine Bildschirmhöhe (20 Kacheln) entfernt, damit der Spieler sofort sieht, was zu tun ist. Weitere Teile (z. B. Energiezellen) dürfen hinter Wänden liegen.
- Der Kreis der Zonen-Kontrolle liegt auf freiem Boden ohne Hindernis im Radius; rundherum bleibt Platz zum Ausweichen.
- Zeitkritische Schalter so verteilen, dass der kürzeste Weg über alle Schalter im Zeitlimit gut, aber nicht im Schlendern zu schaffen ist (Level 1-1: etwa 33 Kacheln bei 12 s Zeitlimit und Tempo 5).

## Ablauf eines Levels

- Zonen dürfen seitlich abzweigen (senkrechte Trennwand mit Tor, Entscheidung L3). Ein Tor kann an Kills und Rätseln zugleich hängen (L2).

- Jede Zone führt höchstens einen neuen Gegnertyp ein (Level 1-1: Walker → Runner → Boomer → Brute).
- Zonen mit Tor wiederholen ihre letzte Welle, bis das Tor offen ist; die letzte Zone endet nach ihren Wellen.
- Der Ausgang liegt hinter dem letzten Tor.

## Prüfen

- Level werden als `LevelBlueprint` beschrieben und von `LevelBlueprintBuilder` gebaut (ADR 0010); er prüft vor dem Speichern mit `LevelLayoutCheck`: Alle Ziele (Tore, Rätselteile, Spawnpunkte, Ausgang) müssen für einen 2 × 2 Kacheln großen Körper erreichbar sein, Spawnpunkte brauchen 1 Kachel Abstand zu Wänden, und die Sichtlinien-Regel für Rätsel wird geprüft. Ein Verstoß bricht den Bau mit einer Liste der Probleme ab; die EditMode-Tests `LevelLayoutCheckTests` und `LevelBlueprintTests` prüfen dasselbe. Die Sichtlinie wird vom Zoneneingang aus geprüft: eine Kachel hinter dem Tor, das aus einer anderen Zone hereinführt, in der Startzone vom Startpunkt.
- Die PlayMode-Tests `BruteTests` und `RunnerTests` lassen Gegner in `Arena_Test` um Hindernisse herum zum Spieler laufen und schlagen fehl, wenn einer stecken bleibt oder in eine Wand gerät.
- Neue Testlevel: im Editor eine Welle mit Brutes spawnen lassen und zusehen, ob alle Gegner den Spieler überall erreichen.
