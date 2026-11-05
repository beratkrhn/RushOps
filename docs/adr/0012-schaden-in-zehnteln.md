# ADR 0012: Schaden und Leben in zehnfacher Einheit

- **Status:** vorgeschlagen (vom Menschen abzunehmen)
- **Datum:** 2026-11-05

## Kontext

Schaden und Leben sind ganze Zahlen (`Health`, `IDamageable.TakeHit(int)`). Die Pistole machte 1 Schaden bei 6 Schuss/s, ein Runner hatte 1 Leben. Für die Waffen aus #174 (W2, W7) reicht das nicht: Alle Waffen sollen etwa 6 Schaden pro Sekunde machen (±15 %). Die Dual-Pistolen sollen aber „sehr schnell, wenig Schaden“ haben und das Sturmgewehr schneller feuern als die Pistole. Mit mindestens 1 Schaden pro Kugel geht das nicht: Mehr als etwa 6,9 Schuss/s sprengen die Toleranz, und weniger als 1 Schaden gibt es nicht.

## Entscheidung

Alle Schadens- und Lebenswerte werden mit 10 multipliziert. Die Spielwerte bleiben dabei im Verhältnis gleich:

- Pistole: 10 Schaden (vorher 1), also 60 Schaden pro Sekunde
- Gegner: Runner 10, Walker 30, Boomer 40, Brute 200; Explosion gegen Gegner 50
- Bosse: Koloss 2500, Brutmutter 3200, Mega-Boomer 3600, Testboss 600
- Startwerte in den Buildern und Standardwerte der ScriptableObjects entsprechend

Herzen des Spielers, Kontaktschaden und Explosionsschaden gegen den Spieler bleiben unverändert, die zählen in Herzen.

## Begründung und Alternativen

- **Schaden als `float`**: hätte `Health`, `IDamageable`, Bosse, Gegner und Explosionen geändert, dazu Rundungsfragen bei Lebensleisten und Kill-Zählern. Mehr Code, gleiches Ergebnis.
- **Werte in den Ganzzahlen lassen**: Dann wären Sturmgewehr und Dual-Pistolen kaum schneller als die Pistole, und W2 wäre nicht erfüllt.
- Das Mal-10-Modell ist reine Datenänderung. Das Spielgefühl bleibt gleich: gleich viele Pistolentreffer pro Gegner, gleiche Kampfdauer bei Bossen.

## Konsequenzen

- Neue Waffen können Schaden in Schritten von einem Zehntel der alten Einheit bekommen, z. B. Dual-Pistolen 6, Sturmgewehr 7.
- Waffen, die weniger als 10 Schaden machen, brauchen zwei Treffer für einen Runner. Das ist gewollt, denn sie feuern schneller.
- Tests mit festen Lebens- oder Schadenswerten nutzen die neuen Werte.
