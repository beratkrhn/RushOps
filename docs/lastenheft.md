# Lastenheft RushOps

| | |
|---|---|
| **Version** | 1.0 |
| **Stand** | 07.11.2026 |
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
| F-PLT-01 | Zielplattform ist iOS auf dem iPhone. Android und iPad werden nicht veröffentlicht (Design-Fragen R2, R10). | Muss |
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
| F-GEG-03 | Höchstens **10 Gegner** *(Startwert)* gleichzeitig. Bosse zählen nicht dazu. Gilt in der Kampagne; im Endlos-Modus siehe F-END-04. | Muss |
| F-GEG-04 | Gegner sind über ScriptableObjects konfigurierbar (Gesundheit, Tempo, Schaden, Größe, Punkte). | Muss |
| F-GEG-05 | **Zombiehunde** kommen im **Rudel von 3** *(Startwert)* und spawnen gemeinsam an einem normalen Spawnpunkt. Ein Rudel zählt mit 3 zur Höchstzahl gleichzeitiger Gegner. | Muss |
| F-GEG-06 | Ein Hund in Reichweite duckt sich *0,4 s (Startwert)* sichtbar und hörbar und springt dann in gerader Linie zur Position, an der der Spieler beim Ducken stand. Seitliches Ausweichen lässt den Sprung ins Leere gehen. | Muss |
| F-GEG-07 | Hunde gibt es im Endlos-Modus (F-END-10) und in der Kampagne nur in den neuen Zonen mit Zombie-Rätseln (F-LVL-07). | Muss |

**Gegnertypen**

| Typ | Gesundheit | Tempo | Schaden | Besonderheit |
|---|---|---|---|---|
| Walker | mittel | mittel | 1 Herz | Basisgegner |
| Runner | gering | sehr hoch | 1 Herz | zwingt zum Priorisieren und Ausweichen |
| Brute | sehr hoch | langsam | 2 Herzen | größere Hitbox, blockiert Engpässe, schirmt andere ab |
| Boomer | mittel | mittel | 2 Herzen (Explosion) | pulsierend, explodiert bei Tod oder Kontakt; Flächenschaden trifft Spieler **und** Zombies |
| Hund | gering | sehr hoch | 1 Herz | Rudel von 3, angekündigter Sprung (F-GEG-05, F-GEG-06) |

### 2.4 Bosse

Jede Insel endet mit einem Boss im dritten Level. Bosse folgen ebenfalls dem Kontaktschaden-Prinzip.

| Insel | Boss | Verhalten |
|---|---|---|
| 1 | **Koloss** | Riesiger Brute. Lädt sich sichtbar auf und stürmt dann geradeaus durch den Raum. Lehrt das Timing des Dash. |
| 2 | **Brutmutter** | Langsam, spawnt laufend Runner. Der Spieler muss Spawns kontrollieren und gleichzeitig Schaden machen. |
| 3 | **Mega-Boomer** | Hinterlässt Pfützen, die 1 s aufglühen und danach bei Kontakt 1 Herz kosten; nach 6 s sind sie weg, sie explodieren nicht (C9b). Teilt sich bei halber Gesundheit in zwei kleine Bosse mit je der halben Restgesundheit; eine gemeinsame Lebensleiste, beide müssen fallen (C9a). |

| ID | Anforderung | Prio |
|---|---|---|
| F-BOS-01 | Bosse haben eine eigene Lebensleiste im HUD. | Muss |
| F-BOS-02 | Bosse sind immun gegen Nuke und Insta-Kill, nehmen aber Doppelschaden. | Muss |

### 2.5 Level, Zonen und Fortschritt

| ID | Anforderung | Prio |
|---|---|---|
| F-LVL-01 | Level sind größer als der Bildschirm. Die Kamera folgt dem Spieler. | Muss |
| F-LVL-02 | Ein Level ist in Zonen unterteilt, die durch Tore getrennt sind. | Muss |
| F-LVL-03 | **Kill-Gates** öffnen sich nach einer festen Anzahl Kills. Gezählt wird standardmäßig **global im Level**; ein Kill-Gate kann stattdessen erst ab Betreten seiner Zone zählen (Entscheidung P1). Kills durch Nuke und Boomer-Explosionen zählen mit. | Muss |
| F-LVL-04 | Weitere Tore öffnen sich durch das Lösen von Rätseln (2.6). | Muss |
| F-LVL-05 | Ein Level gilt als geschafft, wenn das Levelziel erreicht ist (letztes Tor bzw. Boss besiegt). | Muss |
| F-LVL-06 | Umfang Version 1.0: **3 Inseln mit je 3 Leveln**, das dritte Level jeder Insel ist ein Boss-Level. | Muss |
| F-LVL-07 | Je eine neue Zone vor dem Ausgang mit einem Zombie-Rätsel (2.6) und Hunden: Druckplatte in Level 1-2, rissige Wand in Level 2-2, beide nacheinander in Level 3-2. Nur in der Kampagne (Design-Fragen Z2, N28). | Muss |

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

**Zombie-Rätsel** (Design-Fragen Z1, N9, N26, N27, N29; nur in der Kampagne)

| ID | Anforderung | Prio |
|---|---|---|
| F-RAE-09 | Eine **Druckplatte** senkt sich nur unter einem Brute; Spieler und andere Zombies sind zu leicht. Steht ein Brute *1,5 s (Startwert)* darauf, rastet sie dauerhaft ein und löst das Rätsel. Verlässt er sie vorher, beginnt der Fortschritt neu. | Muss |
| F-RAE-10 | Solange die Druckplatte nicht eingerastet ist, schickt ihre Zone regelmäßig Brutes. | Muss |
| F-RAE-11 | Eine **rissige Wand** versperrt einen Durchgang und bricht nur durch die Explosion eines Boomers in Reichweite auf. Schüsse, Nuke, Rakete und Strohmann wirken nicht. Danach ist der Durchgang frei. | Muss |
| F-RAE-12 | Solange die rissige Wand steht, schickt ihre Zone regelmäßig Boomer. | Muss |
| F-RAE-13 | **Hinweise**: Am Rätsel ist ein Symbol des nötigen Zombies zu sehen. Beim ersten Mal pro Rätselart erklärt der Missions-Tracker, was zu tun ist; das wird gespeichert. | Muss |

### 2.7 Perks

| ID | Anforderung | Prio |
|---|---|---|
| F-PRK-01 | Perks erscheinen als leuchtende, animierte Icons und werden durch Berührung aufgesammelt. | Muss |
| F-PRK-02 | Jeder besiegte Gegner hat eine globale Drop-Chance von **12 %** *(Startwert; nach dem Spieltest v0.3.0 von 8 % erhöht)*. Welcher Perk erscheint, bestimmt ein gewichteter Zufallspool. | Muss |
| F-PRK-03 | Liegengebliebene Perks verschwinden nach **10 s** *(Startwert)*. In den letzten 3 s blinken sie. | Muss |
| F-PRK-04 | Aktive zeitlich begrenzte Perks werden im HUD mit ablaufendem Timer angezeigt. Erneutes Aufsammeln setzt die Dauer zurück. | Muss |
| F-PRK-05 | **Begleiter**: Eine kleine Kampfdrohne folgt dem Spieler *15 s (Startwert)*, kann nicht zerstört werden und schießt auf den nächsten Gegner im Umkreis von *5 Kacheln (Startwert)* mit *halbem Schaden pro Sekunde der Startwaffe (Startwert)*. Erneutes Aufsammeln setzt die Dauer zurück. | Muss |
| F-PRK-06 | Kills der Drohne zählen wie eigene Kills (Punkte, Kill-Gates, Gold, Perk-Drops). | Muss |
| F-PRK-07 | **Strohmann**: Er steht am Aufsammelort. *8 s (Startwert)* lang laufen alle normalen Gegner zu ihm statt zum Spieler; Bosse ignorieren ihn. | Muss |
| F-PRK-08 | Am Ende der Dauer explodiert der Strohmann und verletzt Gegner im Radius, nie den Spieler. Kills zählen wie eigene Kills. | Muss |
| F-PRK-09 | **Superwaffen** ersetzen beim Aufsammeln die aktive Waffe für *12 s (Startwert)*; erneutes Aufsammeln setzt die Dauer zurück, danach ist die vorherige Waffe wieder aktiv. Bosse nehmen die volle Wirkung. | Muss |
| F-PRK-10 | **Flammenwerfer**: Feuer im Kegel auf kurze Reichweite; getroffene Gegner brennen *1,5 s (Startwert)* nach. | Muss |
| F-PRK-11 | **Raketenwerfer**: Raketen explodieren am ersten Treffer und verletzen alle Gegner im Radius. | Muss |
| F-PRK-12 | Die Explosion der Rakete verletzt nie den Spieler. Kills von Superwaffen zählen wie eigene Kills. | Muss |

| Perk | Typ | Wirkung | Dauer | Gewicht |
|---|---|---|---|---|
| Speed-Boost | temporär | deutlich höhere Laufgeschwindigkeit | 15 s | 25 |
| Medkit | sofort | +2 Herzen (max. bis zum Maximum) | – | 20 |
| Doppelschaden | temporär | doppelter Waffenschaden | 10 s | 20 |
| Schnellfeuer | temporär | doppelte Feuerrate | 10 s | 15 |
| Schutzschild | temporär | kein Kontaktschaden, sichtbare Barriere | 8 s | 10 |
| Insta-Kill | temporär | jeder Treffer tötet (außer Bosse) | 10 s | 7 |
| Nuke | sofort | tötet alle **sichtbaren** Gegner (außer Bosse) | – | 3 |
| Begleiter | temporär | Kampfdrohne schießt mit (F-PRK-05) | 15 s | 10 |
| Strohmann | temporär | lockt normale Gegner an und explodiert (F-PRK-07) | 8 s | 8 |
| Flammenwerfer | temporär | ersetzt die Waffe, brennt im Kegel (F-PRK-10) | 12 s | 4 |
| Raketenwerfer | temporär | ersetzt die Waffe, explosive Raketen (F-PRK-11) | 12 s | 4 |

Alle Werte sind *Startwerte*.

### 2.8 Score und Bewertung

| ID | Anforderung | Prio |
|---|---|---|
| F-SCO-01 | Jeder Kill gibt Punkte je nach Gegnertyp. | Soll |
| F-SCO-02 | **Kombo-Multiplikator**: Kills kurz hintereinander erhöhen den Multiplikator (bis ×5). Ein erlittener Treffer setzt ihn zurück. | Soll |
| F-SCO-03 | Pro Level werden 1 bis 3 Sterne nach Score vergeben. Bester Score und Sterne werden gespeichert. | Soll |
| F-SCO-04 | Zusätzlich hat jedes Level drei **Abzeichen**: *Unversehrt* (geschafft ohne Herzverlust), *Blitz* (geschafft unter der Zielzeit des Levels, Startwert pro Level) und *Kombo* (Multiplikator ×5 *(Startwert)* erreicht). Jedes zählt für sich, auch aus verschiedenen Versuchen; einmal verdient, bleibt es (Design-Fragen CH1, N23). | Muss |
| F-SCO-05 | „Level geschafft!“ zeigt die Abzeichen des Levels und hebt neu verdiente hervor; das Pause-Overlay zeigt die drei Bedingungen mit ihrem Stand (N25). | Muss |

### 2.9 Meta-Progression und Skins

| ID | Anforderung | Prio |
|---|---|---|
| F-SKN-01 | Es gibt keine In-App-Käufe und keine Premium-Währung. Gold (2.11) wird nur im Spiel verdient. | Muss |
| F-SKN-02 | Jede abgeschlossene Insel (ihr letztes Level) schaltet einen fest definierten Skin frei. | Muss |
| F-SKN-03 | Zusätzliche Skins werden über spielübergreifende Meilensteine freigeschaltet (z. B. 100 Kills insgesamt). | Muss |
| F-SKN-04 | Skins sind in Version 1.0 **rein kosmetisch**. | Muss |
| F-SKN-05 | Abzeichen (F-SCO-04) schalten Skins frei: *„Späher“* bei 9, *„Elite“* bei 18 und *„Legende“* bei 27 Abzeichen (N24). | Muss |

### 2.10 Endlos-Modus

Neben dem Story Mode gibt es einen Endlos-Modus (Design-Fragen E1 bis E10, N2 bis N5, Backlog [`v1.1.0.md`](backlog/v1.1.0.md)).

| ID | Anforderung | Prio |
|---|---|---|
| F-END-01 | Das Hauptmenü bekommt den Bereich **Endlos**. Er ist von Anfang an spielbar. | Muss |
| F-END-02 | Gespielt wird in **einer runden Arena** im Stil eines Kolosseums mit einem **Brunnen in der Mitte**. Zombies spawnen an festen Punkten im **Norden, Osten, Süden und Westen**. | Muss |
| F-END-03 | Ein Lauf besteht aus **Runden**. Eine Runde endet, wenn alle ihre Gegner besiegt sind; danach folgt eine kurze Pause mit Anzeige „Runde X“ *(Startwert 4 s)*. Es gibt keine Rätsel. | Muss |
| F-END-04 | Jede Runde wird schwerer: mehr Gegner, mehr Runner, Boomer und Brutes. Die Höchstzahl gleichzeitiger Gegner darf über 10 steigen *(Startwert: 10, +1 alle 5 Runden, höchstens 20)*. | Muss |
| F-END-05 | Alle **10 Runden** kommt einer der drei Bosse, mit jedem Auftritt mehr Leben *(Startwert +25 %)*. Bosse gleiten am runden Brunnen entlang. | Muss |
| F-END-06 | Nach jeder geschafften Runde gibt es **+1 Herz** bis zum Maximum. | Muss |
| F-END-07 | Der Lauf startet mit der Pistole. **Alle 5 Runden** wird an der Arenawand eine weitere Waffe verfügbar; die Waffen hängen nebeneinander *(Startwert: Runde 5 Schrotflinte, 10 Sturmgewehr, 15 Dual-Pistolen)*. | Muss |
| F-END-08 | Der Lauf endet bei 0 Herzen; es gibt keinen Zwischenstand. Score und erreichte Runde werden als lokale **Top 10** gespeichert. | Muss |
| F-END-09 | Skins werden für das Erreichen von Runde 10, 25 und 50 freigeschaltet. | Soll |
| F-END-10 | Jede **6. Runde** *(Startwert)* ist eine **Hunderunde**, in der nur Hunde kommen; die Pause davor kündigt sie an. Fällt sie auf eine Bossrunde, geht der Boss vor und die Hunderunde folgt eine Runde später. | Muss |
| F-END-11 | Nach jeder **3. geschafften Runde** *(Startwert)* und nach jeder Bossrunde hält das Spiel an und bietet **3 verschiedene Upgrades** *(Startwert)* zur Wahl; eines muss gewählt werden. Ausgereizte Upgrades werden nicht mehr angeboten. | Muss |
| F-END-12 | Upgrades wirken nur im laufenden Endlos-Lauf und werden nicht gespeichert; es gibt 12 Upgrades mit Stufen (Tabelle unten). | Muss |
| F-END-13 | Der **Tageslauf** nutzt das lokale Datum als Seed für alles Zufällige im Lauf; an einem Tag beginnt er immer gleich. Versuche sind beliebig viele, die Uhr des Geräts gilt. | Muss |
| F-END-14 | Der beste Tageslauf jedes Tages wird gespeichert; der Bereich „Endlos“ zeigt die letzten 7 Tage. Tagesläufe kommen nicht in die Top 10 (F-END-08); Gold, Skins und Statistiken zählen wie immer. | Muss |

**Endlos-Upgrades** (Backlog [`v1.7.0.md`](backlog/v1.7.0.md), alle Werte *Startwerte*)

| Upgrade | Wirkung pro Stufe | Höchststufe |
|---|---|---|
| Abpraller | Kugeln prallen 1× mehr von Wänden ab | 3 |
| +1 Herz | Maximum +1 Herz, heilt 1 Herz | 3 |
| Längere Perks | Perk-Dauer +30 % | 3 |
| Gold-Magnet | Magnetradius +1 Kachel | 2 |
| Feuerspur | Dash hinterlässt 1,5 s Feuer, 30 Schaden pro Sekunde | 1 |
| Begleiter dauerhaft | Kampfdrohne bleibt den ganzen Lauf | 1 |
| Schnelle Füße | Lauftempo +10 % | 3 |
| Feuerrate | +15 % | 3 |
| Schaden | +15 % auf jeden Spielerschaden | 3 |
| Perk-Glück | Drop-Chance +3 Prozentpunkte | 3 |
| Schneller Rush | Rush-Leiste voll bei 20 statt 25 | 1 |
| Kurzer Dash | Dash-Abklingzeit −25 % | 2 |

### 2.11 Beute (Gold)

Zombies lassen Gold fallen. Gold gibt Punkte und ist eine Währung, die nur im Spiel verdient wird (Design-Fragen B1 bis B5, N1, N11 bis N14, Backlog [`v1.2.0.md`](backlog/v1.2.0.md)). Gilt in der Kampagne und im Endlos-Modus (A4).

| ID | Anforderung | Prio |
|---|---|---|
| F-GLD-01 | Jeder besiegte Gegner lässt Gold fallen, auch bei Kills durch Nuke und Explosionen. Es gibt nur eine Sorte Münze; die Menge hängt vom Gegnertyp ab *(Startwert: Walker 1, Runner 1, Boomer 2, Brute 3, Boss 25)*. | Muss |
| F-GLD-02 | **Magnet**: Liegt Gold näher als *1 Kachel (Startwert)* am Spieler, fliegt es zu ihm. Berührung sammelt es ein. | Muss |
| F-GLD-03 | Liegengebliebenes Gold verschwindet nach *8 s (Startwert)* und blinkt in den letzten *2 s (Startwert)*. | Muss |
| F-GLD-04 | Jede Münze gibt *25 Punkte (Startwert)* mal Kombo-Multiplikator. Aufsammeln setzt das Kombo-Fenster zurück, erhöht den Multiplikator aber nicht. | Soll |
| F-GLD-05 | Eingesammeltes Gold kommt sofort aufs Konto und bleibt auch bei Tod und Abbruch erhalten. Das HUD zeigt das Gold des laufenden Levels oder Laufs. | Muss |
| F-GLD-06 | Im Skin-Karussell kann der Skin **„Goldrausch“** für *2500 Gold (Startwert)* gekauft werden. Hauptmenü und Karussell zeigen den Kontostand. | Muss |

### 2.12 Rush-Meter

Kills füllen eine Rush-Leiste; voll löst sie einen kurzen Spezialmodus aus (Design-Fragen RM1 bis RM6, N8, N15 bis N18, Backlog [`v1.6.0.md`](backlog/v1.6.0.md)). Gilt in der Kampagne und im Endlos-Modus (RM5).

| ID | Anforderung | Prio |
|---|---|---|
| F-RSH-01 | Jeder Kill füllt die Rush-Leiste um den aktuellen Kombo-Multiplikator (F-SCO-02); bei *25 (Startwert)* ist sie voll. Ein erlittener Treffer leert sie nicht. | Muss |
| F-RSH-02 | Nur eine volle Leiste lässt sich über den Rush-Button (F-HUD-09) auslösen. Das leert sie und startet den Rush für *5 s (Startwert)*; währenddessen füllt sie sich nicht. | Muss |
| F-RSH-03 | Während des Rush laufen normale Gegner mit *50 % (Startwert)*, Bosse mit *75 % (Startwert)* Tempo, einschließlich ihrer Angriffs-Zeitgeber. Die Spielfigur wird nicht verlangsamt. | Muss |
| F-RSH-04 | Während des Rush hat der Dash keine Abklingzeit; die i-Frames bleiben unverändert (F-STE-04). | Muss |
| F-RSH-05 | Während des Rush zählt jeder Spielerschaden *×1,5 (Startwert)*, mit Doppelschaden ×3: Waffen, Superwaffen, Drohne und Strohmann. Insta-Kill und Nuke bleiben unverändert. | Muss |
| F-RSH-06 | Die Leiste ist zu Beginn jedes Levels und jedes Endlos-Laufs leer und bleibt im Endlos-Modus über die Runden erhalten; sie wird nicht gespeichert. Tod und Levelende beenden den Rush. | Muss |

---

## 3. Benutzeroberfläche

### 3.1 Start und Hauptmenü

| ID | Anforderung | Prio |
|---|---|---|
| F-UI-01 | Beim Start läuft eine kurze **Logo-Animation**, die in das Hauptmenü übergeht. | Muss |
| F-UI-02 | Das Hauptmenü hat vier Bereiche: **Story Mode**, **Endlos**, **Spielerauswahl**, **Einstellungen**. | Muss |
| F-UI-03 | **Spielerauswahl**: horizontal scrollbares Karussell mit allen Skins. Gesperrte Skins zeigen ihre Freischaltbedingung. | Muss |
| F-UI-04 | **Einstellungen**: Lautstärke (Gesamt, Musik, Effekte), Zielhilfe an/aus, Credits. Die Sticks werden nicht eingestellt; Modus, Größe und Position sind fest (Design-Frage M3). | Muss |

### 3.2 Story Mode (Levelauswahl)

| ID | Anforderung | Prio |
|---|---|---|
| F-UI-05 | Eine Karte zeigt drei durch Brücken verbundene Inseln, die unterste ist zu Beginn freigeschaltet. | Muss |
| F-UI-06 | Eine Insel wird freigeschaltet, wenn alle Level der vorherigen Insel geschafft sind. Innerhalb einer Insel werden Level nacheinander freigeschaltet. | Muss |
| F-UI-07 | Pro Level werden die erreichten Sterne angezeigt. | Soll |
| F-UI-08 | Unter den Sternen jedes geschafften Levels stehen seine drei Abzeichen, verdiente hell, fehlende dunkel (N25). | Muss |

### 3.3 HUD

| ID | Element | Position | Verhalten | Prio |
|---|---|---|---|---|
| F-HUD-01 | Herzen | oben links | blinken rot bei Schaden | Muss |
| F-HUD-02 | Aktive Perks | unter den Herzen | Icon mit kreisförmigem Timer | Muss |
| F-HUD-03 | Missions-Tracker | oben rechts | z. B. „Tor gesperrt: 4/10 Kills“; bei Rätseln Fortschrittsbalken oder Countdown | Muss |
| F-HUD-04 | Score und Multiplikator | oben mittig | darunter das Gold des Levels oder Laufs (F-GLD-05) | Soll |
| F-HUD-05 | Boss-Lebensleiste | oben, über die Breite | nur in Bosskämpfen | Muss |
| F-HUD-06 | Pause-Button | obere Ecke | öffnet Overlay: Fortsetzen, Neustart, Hauptmenü | Muss |
| F-HUD-07 | Sticks und Dash-Button | unten | siehe 2.1 | Muss |
| F-HUD-09 | Rush-Button | oben links, unter den aktiven Perks | runder Button, der sich als Ring füllt; leuchtet, wenn voll; ein Tipp löst den Rush aus (2.12) | Muss |

| ID | Anforderung | Prio |
|---|---|---|
| F-HUD-08 | Wechselt die App in den Hintergrund, pausiert das Spiel automatisch. | Muss |

---

## 4. Datenverwaltung

| ID | Anforderung | Prio |
|---|---|---|
| F-DAT-01 | Gespeichert wird lokal als JSON-Datei im App-Speicher (`Application.persistentDataPath`). | Muss |
| F-DAT-02 | Es gibt kein manuelles Speichern und keinen Speicherstand innerhalb eines Levels. | Muss |
| F-DAT-03 | **Autosave** bei: Level geschafft, Tod der Spielfigur (Statistiken), Abbruch eines Levels oder Laufs, Kauf eines Skins, Änderung der Einstellungen. | Muss |
| F-DAT-04 | Gespeichert werden: freigeschaltete Inseln und Level, Sterne und beste Scores, Kills insgesamt und weitere Statistiken, freigeschaltete Skins und aktueller Skin, alle Einstellungen, Bestwerte des Endlos-Modus (F-END-08) und der Tagesläufe (F-END-14), Goldkontostand und Gold insgesamt (F-GLD-05), Abzeichen pro Level (F-SCO-04) und gezeigte Rätsel-Hinweise (F-RAE-13). | Muss |
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

Seit 06.11.2026 wird erst nach den Erweiterungen aus Abschnitt 8 veröffentlicht. Diese Abgrenzung gilt für das spätere Release.

- Querformat
- Android und iPad (Design-Fragen R2, R10)
- Musik (Design-Frage R1)
- Online-Funktionen, Bestenlisten, Cloud-Speicherstände
- In-App-Käufe und Werbung
- Mehrspielermodus
- Skins mit Spielwert-Modifikatoren

---

## 8. Spätere Erweiterungen

Geplant ab v1.0.0 (Roadmap in `docs/backlog/roadmap.md`, Entscheidungen in `docs/backlog/design-fragen.md`). Anforderungs-IDs kommen hinzu, sobald die jeweilige Version in Stories zerlegt wird.

- **v1.1.0 Endlos-Modus**: siehe Abschnitt 2.10.
- **v1.2.0 Beute**: siehe Abschnitt 2.11.
- **v1.3.0 Begleiter und Strohmann**: siehe Abschnitt 2.7 (F-PRK-05 bis F-PRK-08).
- **v1.4.0 Superwaffen**: siehe Abschnitt 2.7 (F-PRK-09 bis F-PRK-12).
- **v1.5.0 Hunde**: siehe Abschnitt 2.3 (F-GEG-05 bis F-GEG-07) und 2.10 (F-END-10).
- **v1.6.0 Rush-Meter**: siehe Abschnitt 2.12 (F-RSH-01 bis F-RSH-06) und 3.3 (F-HUD-09).
- **v1.7.0 Endlos-Upgrades und Tageslauf**: siehe Abschnitt 2.10 (F-END-11 bis F-END-14).
- **v1.8.0 Herausforderungen und Rätsel mit Zombies**: siehe Abschnitt 2.5 (F-LVL-07), 2.6 (F-RAE-09 bis F-RAE-13), 2.8 (F-SCO-04, F-SCO-05), 2.9 (F-SKN-05) und 3.2 (F-UI-08).
- **v1.9.0 Feinschliff**: Musik, Vibration (Haptik) bei Treffern und Explosionen, Balancing, Sterne-Schwellen neu.
- Erledigt: **Waffensystem** (W1 bis W10), erschienen mit v1.0.0 ([`v1.0.0-waffen.md`](backlog/v1.0.0-waffen.md)).
- Nach dem Release: Skins mit Modifikatoren, Dieb-Gegner, Mutatoren, weitere Inseln.

### Entschiedene Punkte

- Bundle-ID für iOS und Android: `com.beratkrhn.rushops` (30.10.2026, Issue #13)
- Mindestversionen: Android 8.0 (API 26), iOS 15 (30.10.2026, Issue #13)
- Veröffentlicht wird nur im App Store für das iPhone, kostenlos, Store-Eintrag auf Deutsch (05.11.2026, Design-Fragen R2, R3, R6, R10)

- Musik: vorerst keine; falls später, per Skript erzeugt (04.11.2026, Design-Frage G11); nicht in 1.0 (05.11.2026, R1)
- Inselthemen: Strand, düstere Stadt, Vulkan (04.11.2026, Design-Frage G6)
- Kein Release vor den Erweiterungen v1.1.0 bis v1.9.0; getestet wird am Gerät, wann Berat es sagt (06.11.2026, Design-Fragen A1 bis A3)

### Offene Punkte

- keine

---

## 9. Änderungshistorie

| Version | Datum | Änderung |
|---|---|---|
| 0.1 | 10/2026 | Erster Entwurf |
| 0.2 | 29.10.2026 | Nur Hochformat; Sprache Deutsch; reine Top-down-Perspektive; unbegrenzte Munition, Perk „Maximale Munition“ durch „Schnellfeuer“ ersetzt; Insta-Kill und Doppelschaden als getrennte Perks; Nuke trifft nur sichtbare Gegner; Kill-Gates zählen global inkl. Nuke- und Explosions-Kills; Timer bei Schaltern sichtbar; Zonen-Kontrolle pausiert; Schießen beim Tragen erlaubt; Herzen statt Lebensbalken, Unverwundbarkeit nach Treffer; Wellen an festen Spawnpunkten, max. 10 Gegner; 3 Inseln × 3 Level; Bosse, Score mit Kombo und Sterne ergänzt; Zielhilfe ergänzt; Skins in 1.0 rein kosmetisch; Logo-Animation als Startsequenz; Anforderungs-IDs und Prioritäten eingeführt; Kapitelnummerierung bereinigt. |
| 0.3 | 29.10.2026 | F-GFX-02: Schrägansicht mit 8 Richtungs-Sprites statt reiner Top-down-Ansicht mit rotierten Sprites (ADR 0006, Issue #58). |
| 0.4 | 30.10.2026 | Offene Punkte entschieden: Bundle-ID `com.beratkrhn.rushops`, Mindestversionen Android 8.0 und iOS 15 (Issue #13). |
| 0.5 | 04.11.2026 | F-PRK-02: Drop-Chance 12 % statt 8 % (Spieltest v0.3.0). Offene Punkte entschieden: Musik vorerst nicht, Inselthemen festgelegt. |
| 0.6 | 04.11.2026 | F-UI-04: keine Stick-Einstellungen im Menü (Design-Frage M3). |
| 0.7 | 04.11.2026 | F-LVL-03: Kill-Gates können ab Betreten ihrer Zone zählen (Design-Frage P1, Issue #147). |
| 0.8 | 05.11.2026 | F-PLT-01: nur iOS auf dem iPhone; Android, iPad und Musik nicht in 1.0 (Design-Fragen R1, R2, R10). |
| 0.9 | 07.11.2026 | Release verschoben, Erweiterungen v1.1.0 bis v1.9.0 in Abschnitt 8; neuer Abschnitt 2.10 Endlos-Modus (F-END-01 bis F-END-09); F-GEG-03 gilt nur in der Kampagne; F-UI-02 mit Bereich Endlos; F-DAT-04 mit Endlos-Bestwerten (Design-Fragen A, E, N). |
| 1.0 | 07.11.2026 | Neuer Abschnitt 2.11 Beute (F-GLD-01 bis F-GLD-06); F-SKN-01 mit Gold als reiner Spielwährung; F-HUD-04 mit Goldanzeige; F-DAT-03 mit Autosave bei Abbruch und Kauf; F-DAT-04 mit Goldkonto (Design-Fragen B, N1, N11 bis N14). |
| 1.1 | 08.11.2026 | Abschnitt 2.7 mit Perks Begleiter und Strohmann (F-PRK-05 bis F-PRK-08) und zwei Pool-Zeilen (Design-Fragen K1 bis K8). |
| 1.2 | 08.11.2026 | Abschnitt 2.7 mit Superwaffen Flammenwerfer und Raketenwerfer (F-PRK-09 bis F-PRK-12) und zwei Pool-Zeilen (Design-Fragen W11 bis W15). |
| 1.3 | 10.11.2026 | Abschnitt 2.3 mit Zombiehunden (F-GEG-05 bis F-GEG-07) und Zeile in der Gegnertabelle; Abschnitt 2.10 mit Hunderunde (F-END-10) (Design-Fragen H1 bis H6, N6, N7). |
| 1.4 | 10.11.2026 | Neuer Abschnitt 2.12 Rush-Meter (F-RSH-01 bis F-RSH-06); Abschnitt 3.3 mit Rush-Button (F-HUD-09) (Design-Fragen RM1 bis RM6, N8, N15 bis N18). |
| 1.5 | 11.11.2026 | Abschnitt 2.10 mit Endlos-Upgrades (F-END-11, F-END-12, Upgrade-Tabelle) und Tageslauf (F-END-13, F-END-14); F-DAT-04 mit Tagesbestwerten (Design-Fragen T1 bis T4, N19 bis N22). |
| 1.6 | 11.11.2026 | Abschnitt 2.6 mit Zombie-Rätseln Druckplatte und rissige Wand samt Hinweisen (F-RAE-09 bis F-RAE-13); F-LVL-07 mit neuen Zonen in 1-2, 2-2 und 3-2; Abzeichen F-SCO-04, F-SCO-05, Skins F-SKN-05, Inselkarte F-UI-08; F-GEG-07 mit Hunden in den neuen Zonen; F-DAT-04 mit Abzeichen und Hinweisen (Design-Fragen CH1, Z1, Z2, N9, N23 bis N29). |
