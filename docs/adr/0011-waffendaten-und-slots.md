# ADR 0011: Waffendaten pro Waffe und zwei Slots

- **Status:** angenommen
- **Datum:** 2026-11-05

## Kontext

Bis v1.0.0 gibt es eine Waffe. Ihre Werte (Feuerrate, Schaden, Reichweite, Kugeltempo) lagen zusammen mit der Zielhilfe in `WeaponConfig`, die alle 14 Szenen über `PlayerWeapon` referenzieren. Für v1.1.0 (Epic #171, W1 bis W10) kommen drei Waffen dazu, die sich nur in Feuerrate, Schaden, Streuung, Kugeln pro Schuss, Reichweite und Klang unterscheiden (W3). Der Spieler trägt zwei davon (W9).

## Entscheidung

- Jede Waffe ist ein `WeaponDefinition`-Asset unter `Data/Weapons/`. Die Startwaffe ist `Pistol.asset` mit den bisherigen Werten.
- `WeaponConfig` bleibt das gemeinsame Asset: Startwaffe und Zielhilfe. Die Reichweite der Zielhilfe ist ein Anteil der Reichweite der aktiven Waffe (Startwert 0,8, also wie bisher 8 Einheiten bei der Pistole).
- `WeaponSlots` ist reine Logik: Slot 0 ist die gewählte Waffe, Slot 1 die aufgehobene. Eine neue Waffe ersetzt Slot 1 und wird aktiv; eine schon getragene Waffe aufzuheben bewirkt nichts. `PlayerWeapon` feuert die aktive Waffe.
- Mehrere Kugeln pro Schuss liegen gleichmäßig in einem Fächer (`FanAngle`), dazu kommt eine zufällige Streuung je Kugel (`Spread`). Die Mathematik liegt in `WeaponMath.ProjectileDirection`.
- Perks wirken weiter über `ShotModifiers` auf die aktive Waffe, also auf jede Waffe.
- Ein EditMode-Test hält alle Waffen-Assets innerhalb von ±15 % Schaden pro Sekunde der Startwaffe (W7).

## Begründung und Alternativen

- `WeaponConfig` in `WeaponDefinition` umzubenennen hätte die Szenen-Verweise behalten, aber Waffenwerte und Zielhilfe weiter vermischt. Die Zielhilfe ist eine Spielereinstellung, keine Eigenschaft der Waffe.
- Ein neues Feld für die Startwaffe direkt in `PlayerWeapon` hätte alle Szenen geändert. Über `WeaponConfig.StartWeapon` bleiben die Szenen unverändert; später (#176) setzt die Waffenwahl aus dem Spielstand den ersten Slot.
- Eine feste Reichweite für die Zielhilfe würde bei der Schrotflinte auf Gegner zielen, die ihre Kugeln nicht erreichen. Deshalb ein Anteil der Reichweite.

## Konsequenzen

- Neue Waffen sind reine Daten; nur Sonderverhalten wie abwechselnde Mündungen der Dual-Pistolen (#174) braucht Code.
- `Fired` wird einmal pro Schuss ausgelöst, nicht pro Kugel; Klang und Mündungsfeuer gibt es also auch bei der Schrotflinte einmal.
- Das Schussgeräusch kommt aus der aktiven Waffe; ohne eigenes Geräusch spielt `GameFeel` das Standardgeräusch.
