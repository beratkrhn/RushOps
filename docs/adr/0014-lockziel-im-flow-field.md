# ADR 0014: Lockziel im Flow-Field

- **Status:** angenommen
- **Datum:** 2026-11-08

## Kontext

Der Strohmann (Issue #246, F-PRK-07, K5 bis K7) soll 8 s lang alle normalen Gegner zu sich ziehen statt zur Spielerin oder zum Spieler. Bisher gibt es genau ein Flow-Field mit dem Spieler als Ziel (ADR 0003); alle normalen Gegner (Walker, Runner, Brute, Boomer) steuern über den `FlowFieldNavigator`. Bosse bewegen sich mit eigener Logik (`Boss`) und nutzen den Navigator nicht. Neue Sonderfälle pro Gegnertyp sollen vermieden werden.

## Entscheidung

Der `FlowFieldNavigator` bekommt ein **Lockziel** (`SetLure(Position, Halteradius)`, `ClearLure()`). Solange es gesetzt ist:

- wird das eine Flow-Field **einmal** zum Lockziel berechnet; da es sich nicht bewegt, entfallen die Neuberechnungen beim Kachelwechsel des Spielers,
- steuern `PathDirection` und `Steer` zum Lockziel statt zum Ziel,
- bleiben Gegner innerhalb des Halteradius stehen; nur die Separation wirkt noch, so verteilen sie sich um den Strohmann.

`Target` bleibt der Spieler. Was Gegner sonst am Spieler festmachen (Boomer-Zündung bei Berührung, Annäherungswarnung), ändert sich nicht. Ein neues Lockziel ersetzt das alte, `ClearLure` rechnet das Feld wieder zum Spieler.

## Begründung und Alternativen

- **Zweites Flow-Field zum Lockziel**: doppelter Speicher und eine Auswahl pro Gegner; bringt nichts, solange alle normalen Gegner demselben Ziel folgen.
- **`Target` auf den Strohmann umbiegen**: Boomer würden am Strohmann statt am Spieler zünden wollen, die Annäherungswarnung wäre falsch, und das Zurücksetzen hinge an jedem Leser von `Target`.
- **Sonderfall in `EnemyMovement` oder pro Gegnertyp**: genau das, was die Story vermeiden will.
- **Lockziel im Navigator**: eine Stelle, kein Mehraufwand pro Gegner, keine Allokationen; Gegner, die während des Lockens erscheinen, folgen ihm automatisch.

## Konsequenzen

- Bosse ignorieren das Lockziel ohne eigene Prüfung (K7), weil sie den Navigator nicht nutzen. Soll ein künftiger Boss über den Navigator laufen, braucht er eine Ausnahme (z. B. ein eigenes Feld zum Spieler).
- Es gibt höchstens ein Lockziel gleichzeitig. Weitere Lock-Effekte müssten sich ein Ziel teilen oder ADR 0003/0014 erweitern.
- Das Umschalten kostet eine Feldberechnung beim Setzen und eine beim Aufheben, dieselben Kosten wie ein Kachelwechsel des Spielers.
