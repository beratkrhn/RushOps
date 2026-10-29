# Lastenheft RushOps

| | |
|---|---|
| **Version** | 0.3 |
| **Stand** | 29.10.2026 |
| **Status** | Entwurf, nicht final |
| **Autor** | Berat Karahan |

Änderungen gegenüber Version 0.1 stehen in [Abschnitt 9](#9-änderungshistorie). Werte, die mit *(Startwert)* markiert sind, sind Balancing-Vorgaben. Sie werden im Projekt als ScriptableObjects gepflegt und können ohne Codeänderung angepasst werden.

Anforderungen tragen IDs (z. B. `F-STE-01`), damit User Stories und Tests darauf verweisen können. Die Priorität folgt MoSCoW: **Muss**, **Soll**, **Kann**.

---

## 1. Produkt und Einsatzszenarien

### 1.1 Spielkonzept und Plattformen

RushOps ist ein Offline-Singleplayer-Spiel: ein 2D-Top-down-Shooter mit Twin-Stick-Steuerung im Retro-Pixelart-Stil.

| ID | Anforderung | Prio |
|---|---|---|
| F-PLT-01 | Zielplattformen sind iOS und Android. | Muss |
| F-PLT-02 | Das Spiel läuft ausschließlich im **Hochformat**. Querformat wird nicht unterstützt. | Muss |
| F-PLT-03 | Das Spiel ist vollständig offline spielbar. Es gibt keine Server, Konten oder Cloud-Anbindung. | Muss |
| F-PLT-04 | Die Spielsprache ist **Deutsch**. | Muss |
| F-PLT-05 | UI-Elemente berücksichtigen die Safe Areas (Notch, Kamera-Aussparung, Home-Indikator). | Muss |

Entwickelt wird auf macOS mit Unity 6 (Universal Render Pipeline, 2D).

### 1.2 Zielgruppe und Spielgefühl

Das Spiel richtet sich an Fans actionreicher Arcade-Shooter, inspiriert vom Spielprinzip von *Dead Ops Arcade*. Kernaspekte sind das schnelle Ausweichen vor Gegnerhorden, das taktische Einsetzen von Perks und das Lösen von Umgebungsrätseln unter Zeit- und Gegnerdruck.

RushOps übernimmt nur Spielmechaniken. Namen, Figuren, Texte, Grafiken und Sounds sind eigenständig.

### 1.3 Grafikstil

| ID | Anforderung | Prio |
|---|---|---|
| F-GFX-01 | Pixelart mit einer festen, begrenzten Farbpalette für einen einheitlichen Look. | Muss |
| F-GFX-02 | **Schrägansicht von vorn oben** mit **8 Blickrichtungen**: Figuren zeigen je Richtung ein eigenes Sprite, das passend zur Ziel- bzw. Laufrichtung gewählt wird. Figuren werden nicht rotiert; Projektile und Effekte fliegen stufenlos in jede Richtung. | Muss |
| F-GFX-03 | Pixelgenaue Darstellung ohne Verschwimmen oder Zittern (Pixel Perfect Camera). | Muss |
| F-GFX-04 | Leuchteffekte über das 2D-Lichtsystem von URP (Perks, Mündungsfeuer, Explosionen, Zonen). | Soll |

---

## 2. Spielregeln und Mechaniken

### 2.1 Steuerung

| ID | Anforderung | Prio |
|---|---|---|
| F-STE-01 | **Bewegungs-Stick (links)** bestimmt die Laufrichtung. | Muss |
| F-STE-02 | **Schuss-Stick (rechts)** bestimmt unabhängig von der Laufrichtung die Zielrichtung. Solange er geneigt ist, feuert die Waffe automatisch und kontinuierlich. | Muss |
| F-STE-03 | **Ausweich-Button** oberhalb des Schuss-Sticks löst einen Dash in die aktuelle Laufrichtung aus (ohne Laufrichtung: in Blickrichtung). Währenddessen ist die Figur unverwundbar (i-Frames). | Muss |
| F-STE-04 | Dash-Werte: i-Frames 0,3 s, Abklingzeit 1,0 s *(Startwert)*. Die Abklingzeit ist am Button sichtbar. | Muss |
| F-STE-05 | **Floating Joysticks**: Die Sticks erscheinen dort, wo der Daumen den Bildschirm berührt. Feste Sticks sind in den Einstellungen wählbar. | Soll |
| F-STE-06 | **Zielhilfe**: Liegt ein Gegner innerhalb von ±10° *(Startwert)* der Zielrichtung, wird leicht auf ihn korrigiert. In den Einstellungen abschaltbar. | Soll |
| F-STE-07 | Für Tests im Editor ist das Spiel zusätzlich mit Tastatur und Maus oder Gamepad steuerbar. | Soll |

### 2.2 Spielfigur und Waffe

| ID | Anforderung | Prio |
|---|---|---|
| F-SPF-01 | Die Spielfigur hat **5 Herzen** *(Startwert)*. | Muss |
| F-SPF-02 | Nach einem Treffer ist die Figur **1,0 s** *(Startwert)* unverwundbar und blinkt. | Muss |
| F-SPF-03 | Fallen die Herzen auf 0, ist das Level gescheitert und startet von vorn. Es gibt kein Lebenssystem. | Muss |
| F-SPF-04 | Die Munition ist **unbegrenzt**. Es gibt kein Nachladen. | Muss |
| F-SPF-05 | Zum Start gibt es eine Waffe. Weitere Waffentypen (z. B. Sturmgewehr, MP, Dual-Pistolen) werden später über den Level-Fortschritt freigeschaltet, siehe [Abschnitt 8](#8-spätere-erweiterungen). | Muss / Später |

### 2.3 Gegner

Alle Gegner verursachen ausschließlich **Kontaktschaden**. Es gibt keine Fernkampfgegner.

| ID | Anforderung | Prio |
|---|---|---|
| F-GEG-01 | Gegner verfolgen den Spieler per Pathfinding um Hindernisse herum. Technisch wird ein Flow-Field verwendet (siehe ADR 0003). | Muss |
| F-GEG-02 | Gegner spawnen **wellenweise pro Zone** an **festen Spawnpunkten**. Spawnpunkte im direkten Sichtbereich nahe am Spieler werden übersprungen. | Muss |
| F-GEG-03 | Höchstens **10 Gegner** *(Startwert)* gleichzeitig. Bosse zählen nicht dazu. | Muss |
| F-GEG-04 | Gegner sind über ScriptableObjects konfigurierbar (Gesundheit, Tempo, Schaden, Größe, Punkte). | Muss |

**Gegnertypen**

| Typ | Gesundheit | Tempo | Schaden | Besonderheit |
|---|---|---|---|---|
| Walker | mittel | mittel | 1 Herz | Basisgegner |
| Runner | gering | sehr hoch | 1 Herz | zwingt zum Priorisieren und Ausweichen |
| Brute | sehr hoch | langsam | 2 Herzen | größere Hitbox, blockiert Engpässe, schirmt andere ab |
| Boomer | mittel | mittel | 2 Herzen (Explosion) | pulsierend, explodiert bei Tod oder Kontakt; Flächenschaden trifft Spieler **und** Zombies |

### 2.4 Bosse

Jede Insel endet mit einem Boss im dritten Level. Bosse folgen ebenfalls dem Kontaktschaden-Prinzip.

| Insel | Boss | Verhalten |
|---|---|---|
| 1 | **Koloss** | Riesiger Brute. Lädt sich sichtbar auf und stürmt dann geradeaus durch den Raum. Lehrt das Timing des Dash. |
| 2 | **Brutmutter** | Langsam, spawnt laufend Runner. Der Spieler muss Spawns kontrollieren und gleichzeitig Schaden machen. |
| 3 | **Mega-Boomer** | Hinterlässt explosive Pfützen. Teilt sich bei halber Gesundheit in zwei kleinere Boomer. |

| ID | Anforderung | Prio |
|---|---|---|
| F-BOS-01 | Bosse haben eine eigene Lebensleiste im HUD. | Muss |
| F-BOS-02 | Bosse sind immun gegen Nuke und Insta-Kill, nehmen aber Doppelschaden. | Muss |

### 2.5 Level, Zonen und Fortschritt

| ID | Anforderung | Prio |
|---|---|---|
| F-LVL-01 | Level sind größer als der Bildschirm. Die Kamera folgt dem Spieler. | Muss |
| F-LVL-02 | Ein Level ist in Zonen unterteilt, die durch Tore getrennt sind. | Muss |
| F-LVL-03 | **Kill-Gates** öffnen sich nach einer festen Anzahl Kills. Gezählt wird **global im Level**. Kills durch Nuke und Boomer-Explosionen zählen mit. | Muss |
| F-LVL-04 | Weitere Tore öffnen sich durch das Lösen von Rätseln (2.6). | Muss |
| F-LVL-05 | Ein Level gilt als geschafft, wenn das Levelziel erreicht ist (letztes Tor bzw. Boss besiegt). | Muss |
| F-LVL-06 | Umfang Version 1.0: **3 Inseln mit je 3 Leveln**, das dritte Level jeder Insel ist ein Boss-Level. | Muss |

### 2.6 Rätsel

Rätsel sind Action-Rätsel: Während sie gelöst werden, spawnen weiterhin Gegner.

**Zonen-Kontrolle (Hold the Line)**

| ID | Anforderung | Prio |
|---|---|---|
| F-RAE-01 | Eine leuchtende Zone muss für eine definierte Dauer (z. B. 10 s) besetzt werden, um einen Fortschrittsbalken zu füllen. | Muss |
| F-RAE-02 | Verlässt der Spieler die Zone, **pausiert** der Fortschritt. | Muss |

**Zeitkritische Schalter (Timed Switches)**

| ID | Anforderung | Prio |
|---|---|---|
| F-RAE-03 | Mehrere Schalter werden durch Darüberlaufen aktiviert. Der erste aktivierte Schalter startet einen Timer. | Muss |
| F-RAE-04 | Der Timer wird als **Countdown im HUD angezeigt**. | Muss |
| F-RAE-05 | Läuft der Timer ab, bevor alle Schalter aktiv sind, setzen sich alle zurück. | Muss |

**Objekt-Transport**

| ID | Anforderung | Prio |
|---|---|---|
| F-RAE-06 | Ein Objekt (z. B. Energiezelle) wird aufgesammelt und zu einer Zielstation gebracht. | Muss |
| F-RAE-07 | Während des Tragens: **Schießen möglich**, **Dash deaktiviert**, Laufgeschwindigkeit −20 % *(Startwert)*. | Muss |
| F-RAE-08 | Bei Schaden bleibt das Objekt in der Hand. | Muss |

### 2.7 Perks

| ID | Anforderung | Prio |
|---|---|---|
| F-PRK-01 | Perks erscheinen als leuchtende, animierte Icons und werden durch Berührung aufgesammelt. | Muss |
| F-PRK-02 | Jeder besiegte Gegner hat eine globale Drop-Chance von **8 %** *(Startwert)*. Welcher Perk erscheint, bestimmt ein gewichteter Zufallspool. | Muss |
| F-PRK-03 | Liegengebliebene Perks verschwinden nach **10 s** *(Startwert)*. In den letzten 3 s blinken sie. | Muss |
| F-PRK-04 | Aktive zeitlich begrenzte Perks werden im HUD mit ablaufendem Timer angezeigt. Erneutes Aufsammeln setzt die Dauer zurück. | Muss |

| Perk | Typ | Wirkung | Dauer | Gewicht |
|---|---|---|---|---|
| Speed-Boost | temporär | deutlich höhere Laufgeschwindigkeit | 15 s | 25 |
| Medkit | sofort | +2 Herzen (max. bis zum Maximum) | – | 20 |
| Doppelschaden | temporär | doppelter Waffenschaden | 10 s | 20 |
| Schnellfeuer | temporär | doppelte Feuerrate | 10 s | 15 |
| Schutzschild | temporär | kein Kontaktschaden, sichtbare Barriere | 8 s | 10 |
| Insta-Kill | temporär | jeder Treffer tötet (außer Bosse) | 10 s | 7 |
| Nuke | sofort | tötet alle **sichtbaren** Gegner (außer Bosse) | – | 3 |

Alle Werte sind *Startwerte*.

### 2.8 Score und Bewertung

| ID | Anforderung | Prio |
|---|---|---|
| F-SCO-01 | Jeder Kill gibt Punkte je nach Gegnertyp. | Soll |
| F-SCO-02 | **Kombo-Multiplikator**: Kills kurz hintereinander erhöhen den Multiplikator (bis ×5). Ein erlittener Treffer setzt ihn zurück. | Soll |
| F-SCO-03 | Pro Level werden 1 bis 3 Sterne nach Score vergeben. Bester Score und Sterne werden gespeichert. | Soll |

### 2.9 Meta-Progression und Skins

| ID | Anforderung | Prio |
|---|---|---|
| F-SKN-01 | Es gibt keine In-App-Käufe und keine Premium-Währung. | Muss |
| F-SKN-02 | Jedes abgeschlossene Level schaltet einen fest definierten Skin frei. | Muss |
| F-SKN-03 | Zusätzliche Skins werden über spielübergreifende Meilensteine freigeschaltet (z. B. 100 Kills insgesamt). | Muss |
| F-SKN-04 | Skins sind in Version 1.0 **rein kosmetisch**. | Muss |

---

## 3. Benutzeroberfläche

### 3.1 Start und Hauptmenü

| ID | Anforderung | Prio |
|---|---|---|
| F-UI-01 | Beim Start läuft eine kurze **Logo-Animation**, die in das Hauptmenü übergeht. | Muss |
| F-UI-02 | Das Hauptmenü hat drei Bereiche: **Story Mode**, **Spielerauswahl**, **Einstellungen**. | Muss |
| F-UI-03 | **Spielerauswahl**: horizontal scrollbares Karussell mit allen Skins. Gesperrte Skins zeigen ihre Freischaltbedingung. | Muss |
| F-UI-04 | **Einstellungen**: Lautstärke (Gesamt, Musik, Effekte), Stick-Modus (fest/floating), Größe und Position der Sticks, Zielhilfe an/aus, Credits. | Muss |

### 3.2 Story Mode (Levelauswahl)

| ID | Anforderung | Prio |
|---|---|---|
| F-UI-05 | Eine Karte zeigt drei durch Brücken verbundene Inseln, die unterste ist zu Beginn freigeschaltet. | Muss |
| F-UI-06 | Eine Insel wird freigeschaltet, wenn alle Level der vorherigen Insel geschafft sind. Innerhalb einer Insel werden Level nacheinander freigeschaltet. | Muss |
| F-UI-07 | Pro Level werden die erreichten Sterne angezeigt. | Soll |

### 3.3 HUD

| ID | Element | Position | Verhalten | Prio |
|---|---|---|---|---|
| F-HUD-01 | Herzen | oben links | blinken rot bei Schaden | Muss |
| F-HUD-02 | Aktive Perks | unter den Herzen | Icon mit kreisförmigem Timer | Muss |
| F-HUD-03 | Missions-Tracker | oben rechts | z. B. „Tor gesperrt: 4/10 Kills“; bei Rätseln Fortschrittsbalken oder Countdown | Muss |
| F-HUD-04 | Score und Multiplikator | oben mittig | | Soll |
| F-HUD-05 | Boss-Lebensleiste | oben, über die Breite | nur in Bosskämpfen | Muss |
| F-HUD-06 | Pause-Button | obere Ecke | öffnet Overlay: Fortsetzen, Neustart, Hauptmenü | Muss |
| F-HUD-07 | Sticks und Dash-Button | unten | siehe 2.1 | Muss |

| ID | Anforderung | Prio |
|---|---|---|
| F-HUD-08 | Wechselt die App in den Hintergrund, pausiert das Spiel automatisch. | Muss |

---

## 4. Datenverwaltung

| ID | Anforderung | Prio |
|---|---|---|
| F-DAT-01 | Gespeichert wird lokal als JSON-Datei im App-Speicher (`Application.persistentDataPath`). | Muss |
| F-DAT-02 | Es gibt kein manuelles Speichern und keinen Speicherstand innerhalb eines Levels. | Muss |
| F-DAT-03 | **Autosave** bei: Level geschafft, Tod der Spielfigur (Statistiken), Änderung der Einstellungen. | Muss |
| F-DAT-04 | Gespeichert werden: freigeschaltete Inseln und Level, Sterne und beste Scores, Kills insgesamt und weitere Statistiken, freigeschaltete Skins und aktueller Skin, alle Einstellungen. | Muss |
| F-DAT-05 | Das Speicherformat hat eine Versionsnummer, damit spätere Versionen alte Spielstände migrieren können. | Muss |
| F-DAT-06 | Ein beschädigter Spielstand führt nicht zum Absturz. Es wird ein Backup geladen oder ein neuer Spielstand angelegt. | Muss |

---

## 5. Audio

| ID | Anforderung | Prio |
|---|---|---|
| F-AUD-01 | Hintergrundmusik im Chiptune- oder Synthwave-Stil: ein Track für das Hauptmenü, ein loopbarer Action-Track pro Insel. | Soll |
| F-AUD-02 | Soundeffekte für Schüsse, Treffer, Gegner (Spawn, Annäherung, Schaden), Perks, sich öffnende Tore, Dash und UI-Klicks. | Muss |
| F-AUD-03 | Der Boomer hat ein deutlich anderes Warnsignal als die übrigen Gegner. | Muss |
| F-AUD-04 | Lautstärke getrennt einstellbar: Gesamt, Musik, Effekte. | Muss |

---

## 6. Nichtfunktionale Anforderungen

| ID | Anforderung | Prio |
|---|---|---|
| NF-01 | Ziel: stabile **60 FPS** auf Mittelklasse-Geräten, auch bei maximaler Gegnerzahl. | Muss |
| NF-02 | Häufig erzeugte Objekte (Projektile, Gegner, Partikel) werden über Object Pooling wiederverwendet. | Muss |
| NF-03 | Balancing-Werte liegen in ScriptableObjects, nicht im Code. | Muss |
| NF-04 | Kernlogik (Schaden, Perks, Spawns, Kill-Gates, Savegame) ist durch automatisierte Tests abgedeckt. | Muss |
| NF-05 | Ladezeit vom App-Start bis zum Hauptmenü unter 5 Sekunden. | Soll |

---

## 7. Abgrenzung (nicht Teil von Version 1.0)

- Querformat
- Online-Funktionen, Bestenlisten, Cloud-Speicherstände
- In-App-Käufe und Werbung
- Mehrspielermodus
- Skins mit Spielwert-Modifikatoren

---

## 8. Spätere Erweiterungen

- **Waffensystem**: weitere Waffentypen (z. B. Sturmgewehr, MP, Dual-Pistolen), freigeschaltet über den Level-Fortschritt, jeweils mit eigenem Schussgeräusch.
- **Skins mit Modifikatoren** (z. B. Tempo, Feuerrate).
- Weitere Inseln und Gegnertypen.
- Vibration (Haptik) bei Treffern und Explosionen.

### Offene Punkte

- Bundle-ID für iOS und Android
- Mindestversionen von iOS und Android
- Herkunft der Musik (selbst erzeugt oder lizenzfreie Packs)

---

## 9. Änderungshistorie

| Version | Datum | Änderung |
|---|---|---|
| 0.1 | 10/2026 | Erster Entwurf |
| 0.2 | 29.10.2026 | Nur Hochformat; Sprache Deutsch; reine Top-down-Perspektive; unbegrenzte Munition, Perk „Maximale Munition“ durch „Schnellfeuer“ ersetzt; Insta-Kill und Doppelschaden als getrennte Perks; Nuke trifft nur sichtbare Gegner; Kill-Gates zählen global inkl. Nuke- und Explosions-Kills; Timer bei Schaltern sichtbar; Zonen-Kontrolle pausiert; Schießen beim Tragen erlaubt; Herzen statt Lebensbalken, Unverwundbarkeit nach Treffer; Wellen an festen Spawnpunkten, max. 10 Gegner; 3 Inseln × 3 Level; Bosse, Score mit Kombo und Sterne ergänzt; Zielhilfe ergänzt; Skins in 1.0 rein kosmetisch; Logo-Animation als Startsequenz; Anforderungs-IDs und Prioritäten eingeführt; Kapitelnummerierung bereinigt. |
| 0.3 | 29.10.2026 | F-GFX-02: Schrägansicht mit 8 Richtungs-Sprites statt reiner Top-down-Ansicht mit rotierten Sprites (ADR 0006, Issue #58). |
