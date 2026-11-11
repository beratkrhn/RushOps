# ADR 0015: Zufall im Tageslauf über einen Seed mit Strömen pro System

- **Status:** angenommen
- **Datum:** 2026-11-11

## Kontext

Der Tageslauf (Issue #280, F-END-13, N21) soll an einem Tag immer gleich beginnen: dieselben ersten Spawns und dasselbe erste Upgrade-Angebot. Zufall steckte bisher an mehreren Stellen, jede mit eigener ungeseedeter Quelle oder `UnityEngine.Random`: Spawnpunkte und Rudel-Versatz (`WaveDirector`), Upgrade-Angebote (`EndlessRunController`), Perk-Drops (`PerkDropper`, `BossFight`), Gold-Streuung (`GoldDropper`), Waffenstreuung (`PlayerWeapon`) und die Tonhöhe der Sounds (`SfxPlayer`). Außerdem zog `WaveDirector.CanPlace` bei jeder Prüfung eine Zufallszahl, also abhängig von der Bildrate.

## Entscheidung

- Ein Lauf hat **einen Seed**: im Tageslauf `DailySeed` des lokalen Datums, im normalen Lauf keinen (frische Quellen).
- Aus dem Seed entsteht **pro System ein eigener Strom** (`RunRandom.Stream(seed, name)`, z. B. `offers`, `spawn-points`, `packs`, `perks`, `gold`, `spread`, `sounds`, `boss-<Runde>`). Der Strom-Seed ist ein fester Hash aus Lauf-Seed und Name, unabhängig von Plattform und App-Start.
- `EndlessRunRandom` verteilt die Ströme beim Laufstart (`EndlessRunController.RunBegan`) und gibt jeder Bossrunde ihren eigenen Strom.
- `WaveDirector.CanPlace` prüft nur noch, ob ein Punkt frei ist; gewürfelt wird erst bei der Ankündigung eines Spawns.

## Begründung und Alternativen

- **Eine einzige geteilte Quelle für alles**: Das Angebot nach Runde 3 hinge davon ab, wie viele Schüsse mit Streuung, Münzen und Sounds vorher fielen. Zwei Versuche am selben Tag begännen nicht gleich, sobald man anders spielt. Das widerspräche dem Ziel von N21.
- **`UnityEngine.Random.InitState`**: global, wird von Unity und Paketen mitbenutzt und hätte dasselbe Problem wie eine geteilte Quelle.
- **Ströme pro System**: Jeder Strom hängt nur von Datum und Name ab. Wie oft ein anderes System würfelt, spielt keine Rolle. Es bleibt bei einem Seed pro Tag, wie N21 verlangt.

## Konsequenzen

- Gleich bleiben am selben Tag die Folgen pro System: die n-te Spawnpunkt-Wahl, das n-te Angebot, der n-te Drop-Wurf. Was der Spieler tut (Position, Kill-Reihenfolge), kann den Verlauf danach weiter verändern; ein vollständig identischer Lauf ist bei variabler Bildrate nicht das Ziel.
- Neue Zufallsstellen im Lauf brauchen einen eigenen Strom in `RunRandom` und einen Eintrag in `EndlessRunRandom`, sonst sind sie im Tageslauf nicht reproduzierbar.
- Strom-Namen und Hash dürfen sich nicht ändern, sonst beginnt ein Tageslauf nach einem Update anders als vorher am selben Tag (Test in `DailyRunRulesTests`).
