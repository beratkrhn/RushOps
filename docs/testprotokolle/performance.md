# Leistungsprotokoll (NF-01, NF-05)

Messungen zu Issue #189 und Entscheidung R8. Das Testgerät ist ein iPhone 16 Pro und damit keine Mittelklasse-Referenz. Deshalb wird zusätzlich im Editor gemessen und gegen ein festes Budget geprüft.

## So wird gemessen

```bash
Tools/unity-perf.sh     # Unity muss geschlossen sein, etwa 2 Minuten
```

Das Skript startet die explizite PlayMode-Testklasse `PerformanceBenchmark` im Batchmode mit Grafik (Metal) und gibt die Tabelle unten aus. Rohwerte liegen unter `Logs/perf/<Szene>.json`.

- **Szenen:** `Stress_Test` (schwerster Fall: Mega-Boomer mit Pfützen und eigene Arena-Wellen, die das Level an der Höchstzahl von 10 Gegnern halten, mit Runnern, Boomern samt Explosionen und einem Brute; Spieler mit Dual-Pistolen im Dauerfeuer). Dazu das erste reguläre Level jeder Insel (1-1, 2-1, 3-1) und das Hauptmenü. Die Stress-Szene erzeugt „RushOps → Szenen → Stress_Test erzeugen“, im Testmenü steht sie als „Stress-Test“.
- **Ablauf:** 3 s Aufwärmen, dann 20 s Messung. Der Spieler kann nicht sterben und zielt auf den Boss, ohne Boss nach oben (Zielhilfe aktiv).
- **Spiel-CPU:** Zeit auf dem Main Thread vom Beginn von `EarlyUpdate` bis zum Ende von `PostLateUpdate` (`PlayerLoopProbe`). Das Warten auf 60 FPS und die Arbeit des Editors zwischen den Frames sind damit nicht drin; die Profiler-Zähler „Main Thread“ und „GC Allocated In Frame“ enthalten beides und taugen im Editor nicht.
- **GC:** verwaltete Allokationen auf dem Main Thread im selben Fenster.
- **Rendern:** Ohne Game-View rendert der Batchmode nur in Texturen. Die Kameras rendern deshalb während der Messung in eine Textur in iPhone-Auflösung (1179×2556). Die CPU-Seite des Renderns ist so mit drin, die GPU-Zeit nicht.
- **Texturen:** Spitzenwert des Zählers „Texture Memory“, einschließlich dieser Zieltextur und der Zwischentexturen von URP (etwa 80 MB). Ohne Zieltextur lagen die Level bei 83 MB, das Hauptmenü bei 66 MB, die Stress-Szene bei 98 MB.

## Messung 05.11.2026

Apple M3, Unity 6000.3.10f1, Batchmode mit Metal, Commit nach `a341fd6` (Branch `feature/189-leistung-messen`).

| Szene | Gegner | Spiel-CPU Ø / 1 % (ms) | Skripte Ø (ms) | Physik Ø (ms) | GC pro Frame (B) | Frames mit GC | Texturen (MB) |
|---|---|---|---|---|---|---|---|
| Stress_Test | 8 | 0.24 / 1.73 | 0.08 | 0.02 | 0 | 0 von 1201 | 204 |
| Level_1-1 | 4 | 0.20 / 0.96 | 0.07 | 0.01 | 0 | 0 von 1201 | 166 |
| Level_2-1 | 5 | 0.19 / 1.02 | 0.06 | 0.01 | 0 | 0 von 1200 | 166 |
| Level_3-1 | 7 | 0.22 / 0.81 | 0.07 | 0.01 | 0 | 0 von 1201 | 166 |
| MainMenu | 0 | 0.06 / 0.21 | 0.01 | 0.00 | 0 | 0 von 1201 | 169 |

„Gegner“ ist die Zahl am Ende der Messung; in der Stress-Szene schwankt sie zwischen 8 und 10.

**Befund**
- Die CPU hat sehr viel Luft: Auch im schwersten Fall braucht ein Frame im Mittel 0,24 ms von 16,7 ms. Die Ausreißer (1 % langsamste Frames, 1,7 ms) kommen von Explosionsketten und dem Spawnen.
- Im Spiel entsteht kein Müll für den Garbage Collector: 0 Byte in allen gemessenen Frames.
- Offen bleibt die GPU (Füllrate, 2D-Licht, Partikel). Das lässt sich nur am Gerät messen: Die FPS-Anzeige im Development-Build zeigt jetzt unter der FPS-Zahl die Frame-Zeit (Mittel / langsamstes 1 % der letzten 5 s). Notiert wird das im Gesamtspieltest (#195).
- Ladezeit (NF-05): Ein Development-Build schreibt beim ersten Hauptmenü „[RushOps] Hauptmenü nach X s geladen“ ins Log (Xcode-Konsole oder `Logs/devicectl-ios.log`). Der Wert wird im Gesamtspieltest notiert.
- Speicher: Der Gesamtspeicher des Editors (etwa 1,3 GB) sagt über das Gerät nichts aus. Gemessen wird er am Gerät.

## Vorschlag für das Frame-Budget (R8)

Ein Mittelklasse-iPhone (A13 bis A15) ist auf einem Kern etwa zwei- bis dreimal langsamer als der M3. Für die Stress-Szene im Editor gilt deshalb:

| Wert | Budget | Gemessen | Am Gerät etwa |
|---|---|---|---|
| Spiel-CPU im Mittel | ≤ 2 ms | 0,24 ms | ≤ 6 ms |
| Spiel-CPU, langsamstes 1 % | ≤ 5 ms | 1,73 ms | ≤ 15 ms, also unter 16,7 ms |
| GC pro Frame nach dem Aufwärmen | 0 Byte | 0 Byte | 0 Byte |

Das Budget kommt in Issue #190 als ScriptableObject dazu und wird dort von einem PlayMode-Test geprüft.
