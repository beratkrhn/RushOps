# ADR 0013: Endlos-Runden über die bestehenden Wellen

- **Status:** vorgeschlagen
- **Datum:** 2026-11-07

## Kontext

Der Endlos-Modus (v1.1.0, F-END-03 bis F-END-07) spielt Runde um Runde, jede schwerer als die letzte. Eine Runde ist eine Welle, die endet, wenn alle ihre Gegner besiegt sind (N5). Die Kampagne hat dafür schon `WaveDefinition`, `WaveSequence` und `WaveDirector` mit Spawnpunkten, Ankündigung, Höchstzahl und Pools. Die Regeln einer Runde (Gegnerzahl, Mischung, Spawn-Intervall, Höchstzahl, Boss, Waffen) hängen dagegen nur an der Rundennummer und lassen sich nicht als feste Liste von Wellen ablegen.

## Entscheidung

- `EndlessConfig` (`Data/Endless/EndlessConfig.asset`) hält alle Startwerte. Die Gegnermischung ist eine Tabelle: pro schwerem Typ die erste Runde, der Anteil in dieser Runde, der Zuwachs je Runde und der höchste Anteil; den Rest füllt der Grundtyp (Walker) auf. Jeder Typ, der dazugekommen ist, bringt mindestens einen Gegner.
- `EndlessRules` ist eine reine C#-Klasse und liefert für eine Rundennummer alles, was die Runde braucht (`EndlessRound`). Mit `CreateWave` baut sie daraus zur Laufzeit eine `WaveDefinition` mit genau einer Welle und `AfterLastWave.End`. Die Arena spielt diese Welle mit dem bestehenden `WaveDirector` ab; ein eigener Spawner entsteht nicht.
- `EndlessRun` ist die Zustandsmaschine des Laufs: Pause („Runde X“) → Runde läuft → Runde geschafft (+Herz bis zum Maximum) → Pause …, Ende bei 0 Herzen. Sie kennt keine Gegner; der Adapter meldet geschaffte Runden und die Herzen des Spielers.
- Bossrunden bringen keine normalen Gegner; die Runde endet mit dem Boss (#210). Die Runner der Brutmutter zählen wie in der Kampagne zur Höchstzahl.
- Der Lebensfaktor der Bosse zählt die Bossauftritte im ganzen Lauf, gleich welcher Boss: Runde 10 ×1, Runde 20 ×1,25, Runde 30 ×1,5 und so weiter (F-END-05 „mit jedem Auftritt mehr Leben“). So wird jede Bossrunde schwerer als die vorige.
- Die Höchstzahl wächst bei Runde 5, 10, 15 … um 1 (also 11 ab Runde 5, 20 ab Runde 50), passend zu den Waffenrunden.

## Begründung und Alternativen

- **Eigener Endlos-Spawner:** hätte Spawnpunkte, Ankündigung, Warteschlange und Pools doppelt gebraucht. Verworfen.
- **Viele vorab erzeugte Wellen-Assets:** begrenzt die Rundenzahl und lässt sich schlecht abstimmen. Verworfen.
- **Mischung als Tabelle pro Runde:** wäre für hohe Runden endlos lang. Die Tabelle pro Gegnertyp mit Zuwachs und Obergrenze reicht mit drei Zeilen für jede Runde.
- **Lebensfaktor pro Boss** (Koloss erst in Runde 40 stärker): hätte Runde 20 und 30 nicht schwerer gemacht als die Kampagne. Lässt sich später über die Konfiguration nachrüsten, falls der Spieltest es verlangt.

## Konsequenzen

- Pro Runde entsteht eine `WaveDefinition` zur Laufzeit; der Adapter muss sie nach der Runde zerstören. Allokationen gibt es nur einmal pro Runde, nicht pro Frame.
- `WaveDirector` braucht in #207 eine Höchstzahl, die von außen kommt (Runde statt `SpawnerConfig`); in der Kampagne bleibt sie bei 10.
- Alle Zahlen sind Startwerte in `EndlessConfig` und werden im Spieltest abgestimmt; „RushOps → Daten → Endlos-Konfiguration erzeugen“ setzt Zahlen nur bei einem neuen Asset, Bossreihenfolge und Waffenrunden immer.
