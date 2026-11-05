# Store-Screenshots

Fünf Bilder für den App Store, 1320×2868 (iPhone 6,9 Zoll, Hochformat). Apple skaliert sie für kleinere iPhones herunter. iPad-Sätze gibt es nicht, weil RushOps nur fürs iPhone erscheint (R10).

| Datei | Motiv |
|---|---|
| `01_kampf.png` | Level 2-1 „Hinterhöfe“, Walker greifen an, Sturmgewehr |
| `02_boss.png` | Level 3-3 „Krater“, Mega-Boomer mit Pfützen |
| `03_perks.png` | Level 1-2 „Strandpromenade“ mit Schutzschild und Schnellfeuer, Haltezone |
| `04_inselkarte.png` | Inselkarte Strand mit Sternen |
| `05_skins.png` | Spielerauswahl mit Skin „Sandsturm“ |

Neu aufnehmen:

```bash
Tools/store-screenshots.sh     # Unity geschlossen, etwa 2 Minuten
```

Im offenen Editor geht das über „RushOps → Store → Screenshots aufnehmen“. Beides startet die explizite PlayMode-Testklasse `StoreScreenshots`. Sie spielt die Szenen ohne Eingabe an, blendet FPS-Anzeige und Testmenü-Knopf aus und rendert Kamera und HUD in eine Textur. Die Pixel-Perfect-Kamera zoomt dabei wie auf dem iPhone um den ganzen Faktor 3.

Überschriften gibt es nicht; die Akzeptanzkriterien nennen sie optional.
