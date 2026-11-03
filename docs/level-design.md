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

## Prüfen

- Die PlayMode-Tests `BruteTests` und `RunnerTests` lassen Gegner in `Arena_Test` um Hindernisse herum zum Spieler laufen und schlagen fehl, wenn einer stecken bleibt oder in eine Wand gerät.
- Neue Testlevel: im Editor eine Welle mit Brutes spawnen lassen und zusehen, ob alle Gegner den Spieler überall erreichen.
