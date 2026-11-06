# Offene Design-Fragen v0.4.0 bis v0.6.0

Sammlung für Issue #37: Diese Fragen müssen beantwortet sein, bevor die Epics in Stories zerlegt werden (Vorgehen siehe [`roadmap.md`](roadmap.md)). Entwurf von Claude anhand von Lastenheft, Roadmap und Stand nach Level 1-1. Antworten bitte direkt in die Spalte „Entscheidung“ eintragen; Fragen ohne Entscheidung halten das jeweilige Epic auf `needs-design`.

## v0.4.0 – Grafik und Sound

| # | Epic | Frage | Vorschlag | Entscheidung |
|---|---|---|---|---|
| G1 | #38 | Stilrichtung und Stimmung: düster, bunt oder comichaft? | Bunte Figuren vor dunkler Umgebung, damit Gegner und Perks auf dem Handy gut lesbar sind |Dein Vorschlag|
| G2 | #38 | Referenzspiele für Look und Animationen? | |Bau selber mal Animationen und ich gucke ob das passt|
| G3 | #38 | Bleibt die Palette bei höchstens 32 Farben, oder bekommt jede Insel eine eigene Teilpalette? | Eine gemeinsame Palette, je Insel 6 bis 8 Akzentfarben daraus |Dein Vorschlag|
| G4 | #38 | Welche Animationen braucht jede Figur in 1.0 (Laufen, Treffer, Tod, Angriff, Idle)? | Laufen, Treffer, Tod; Idle nur für den Spieler |Dein Vorschlag|
| G5 | #38 | Spielerfigur: weiter aus dem Character-Tool oder neu per Skript? | Character-Tool (wie bisher, `Tools/import_character.py`) |Der erste Character aus dem Character-Tool, den Rest dann per Skript so, dass es passt|
| G6 | #39 | Themen der drei Inseln? | z. B. Hafen/Strand, Industrie/Labor, Vulkan/Ruinen |Strand, düstere Stadt, Vulkan|
| G7 | #39 | Sollen Tore, Schalter und Generator pro Insel anders aussehen oder überall gleich (Wiedererkennung)? | Form gleich, Farben und Material pro Insel |Schalter und generator gleich, Tore nicht|
| G8 | #40 | Wie stark wird 2D-Licht eingesetzt: nur Akzente (Rätsel, Perks) oder dunkle Level mit Lichtkegeln? | Nur Akzente; dunkle Level kosten Lesbarkeit und Leistung |Dein Vorschlag|
| G9 | #41 | Klangrichtung der Effekte: Chiptune/8-Bit oder „crunchy“ Retro mit Rauschen? | |8-Bit fürs erste|
| G10 | #41 | Werden Sounds weiter per Skript erzeugt (`Tools/`) oder gekauft/aufgenommen? | |per Skript|
| G11 | #42 | Musik selbst erzeugt oder lizenzfreie Packs (offener Punkt im Lastenheft)? | |erstmal per Skript wenn überhaupt, lass Musik erstmal raus|
| G12 | #42 | Dynamische Musik (Intensität je nach Gegnerzahl) oder ein fester Loop pro Insel? | Fester Loop pro Insel für 1.0 |Dein Vorschlag|

## v0.5.0 – Menüs, Speichern, Meta-Progression

| # | Epic | Frage | Vorschlag | Entscheidung |
|---|---|---|---|---|
| M1 | #43 | Wie lang darf die Logo-Animation sein, und ist sie überspringbar? | Höchstens 3 s, Tippen überspringt |Dein Vorschlag|
| M2 | #43 | Was zeigt „Spielerauswahl“ im Hauptmenü genau: Skins oder verschiedene Figuren mit Werten? | Nur Skins (Lastenheft: rein kosmetisch) |Dein Vorschlag|
| M3 | #44 | Welche Stick-Einstellungen gibt es (Modus fest/floating, Größe, Position, Deckkraft)? | |So wie es jetzt ist ist es gut|
| M4 | #44 | Zielhilfe in den Einstellungen nur an/aus oder in Stufen? | An/aus |Dein Vorschlag|
| M5 | #45 | Wann wird automatisch gespeichert: nach jedem Level, beim Pausieren, beim Wechsel in den Hintergrund? | Nach jedem Level und beim Wechsel in den Hintergrund |Dein Vorschlag|
| M6 | #45 | Welche Statistiken werden gespeichert (Kills gesamt, Spielzeit, Tode, beste Kombo)? | |Dein Vorschlag|
| M7 | #46 | Liste der Skins und Meilensteine für 1.0? | z. B. ein Skin pro geschaffter Insel, einer für alle 27 Sterne, einer für 1000 Kills |Dein Vorschlag|
| M8 | #46 | Werden Skins nur über Spielen freigeschaltet oder gibt es eine Währung? | Nur über Spielen, keine Währung |Erstmals nur über Spielen|
| M9 | #47 | Muss ein Level mit Mindest-Sternen geschafft sein, um das nächste freizuschalten, oder reicht „geschafft“? | „Geschafft“ reicht; Sterne sind für Skins |Dein Vorschlag|
| M10 | #47 | Ist die Inselkarte frei scrollbar oder eine feste Ansicht pro Insel? | Feste Ansicht pro Insel, Wischen wechselt |Dein Vorschlag|

## v0.6.0 – Inhalte und Bosse

| # | Epic | Frage | Vorschlag | Entscheidung |
|---|---|---|---|---|
| L1 | #48 | Schwierigkeitskurve: Was ändert sich von Insel zu Insel (mehr Gegner, schnellere Wellen, neue Kombinationen, kürzere Zeitlimits)? | Ab Insel 2 Rätsel kombiniert (z. B. Schalter während Zonen-Kontrolle), ab Insel 3 kürzere Limits |Dein Vorschlag, mit Option mehrere Rätsel etc. einzubauen|
| L2 | #48 | Neue Rätsel-Kombinationen: Dürfen zwei Rätsel an einem Tor hängen? | Ja, das System kann es schon (`PuzzleSolvedCondition` mit mehreren Rätseln) |Dein Vorschlag|
| L3 | #48 | Bleiben Level vertikal (Zonen von Süden nach Norden) oder auch verwinkelt/seitlich? | Ab Insel 2 auch seitliche Abzweigungen |Dein Vorschlag|
| L4 | #48 | Soll es neue Gegnertypen geben oder bleiben es vier plus Bosse? | Vier plus Bosse für 1.0 |Dein Vorschlag|
| L5 | #49–#51 | Bosse sind größer als 0,5 Kacheln Radius; das Flow-Field führt sie dann an Wänden entlang ins Hängen (siehe `docs/level-design.md`). Eigene Bewegung ohne Flow-Field oder Boss-Arenen ohne Hindernisse? | Boss-Arenen ohne Hindernisse, Boss bewegt sich direkt auf den Spieler zu |Dein Vorschlag|
| L6 | #49–#51 | Hat der Boss einen eigenen Lebensbalken im HUD, und laufen während des Kampfes normale Wellen? | Lebensbalken oben; Wellen nur bei der Brutmutter (spawnt selbst) |Dein Vorschlag|
| L7 | #49 | Koloss: Wie wird der Ansturm angekündigt (Aufladen sichtbar, Linie am Boden)? | Aufladen 1 s mit Linie am Boden |Dein Vorschlag|
| L8 | #51 | Mega-Boomer: Bleiben die Pfützen bis zum Ende liegen oder verschwinden sie? | Verschwinden nach einigen Sekunden |Dein VOrschlag|

## v0.6.0 – Feinplanung (nach v0.5.0)

Beantwortet; „V“ heißt „Vorschlag übernommen“. Umgesetzt in [`v0.6.0.md`](v0.6.0.md), [`v0.7.0.md`](v0.7.0.md) und [`v0.8.0.md`](v0.8.0.md).

| # | Story | Frage | Vorschlag | Entscheidung |
|---|---|---|---|---|
| C1 | alle | 12 Stories sind viel für einen Milestone. Ein Milestone v0.6.0 oder je Insel einer (v0.6.0 Insel 1 und Boss-Grundlage, v0.7.0 Insel 2, v0.8.0 Insel 3), jeweils mit eigenem Spieltest? | Je Insel ein Milestone; so wird jeder Boss am Gerät abgenommen, bevor der nächste kommt. Roadmap und Versionsnummern bis v1.0.0 verschieben sich entsprechend |V|
| C2 | 3, 6, 7, 9, 10 | Wie lang ist ein reguläres Level? | Wie 1-1 (5 Zonen, etwa 4 bis 6 Minuten); Insel 2 und 3 je 6 Zonen |V|
| C3 | 2 | Welche Stellschrauben machen eine Insel schwerer? Gegnerwerte selbst (Tempo, Leben) bleiben gleich, damit die Gegner lernbar bleiben. | Insel 2: Wellen 20 % dichter, Haltezeit der Zone +3 s. Insel 3: zusätzlich Schalter-Zeitlimits −20 %, mehr Runner und Boomer in den Wellen |V, später mit abwechslungsreicheren Challenges|
| C4 | 3 | 1-1 führt die Gegnertypen einzeln ein. Sind ab 1-2 alle Typen von Anfang an gemischt? | Ja, gemischt; jede Zone setzt einen Schwerpunkt (z. B. viele Runner) |V|
| C5 | 4, 5, 8, 11 | Wie lange soll ein Bosskampf dauern? Daraus folgen die Lebenspunkte. | Ohne Perks: Koloss etwa 60 s, Brutmutter etwa 90 s, Mega-Boomer etwa 90 s (beide Hälften zusammen) |V|
| C6 | 4 | Besteht ein Boss-Level nur aus der Arena oder gibt es Zonen davor? | Zwei kurze Zonen mit Kill-Gate bzw. einem Rätsel, dann die Arena; das Tor schließt sich hinter dem Spieler |V|
| C7 | 5 | Koloss: Was passiert beim Aufprall an der Wand, und wie viel Schaden macht der Ansturm? | 1,5 s benommen (Zeitfenster zum Schießen); Ansturm kostet wie jeder Kontakt 1 Herz |V|
| C8 | 8 | Brutmutter: Wie oft spawnt sie, und zählen ihre Runner zur Höchstzahl von 10 Gegnern? | Alle 3 s ein Runner, unter halber Gesundheit alle 2 s; ihre Runner zählen zu den 10 |V|
| C9 | 4, 11 | Perks im Bosskampf: Koloss und Mega-Boomer haben keine Begleiter, also fallen keine Perks. Sollen Bosse an Lebensschwellen Perks abwerfen? | Ja: bei 66 % und 33 % Leben fällt je ein zufälliger Perk (ohne Nuke und Insta-Kill, die wirken auf Bosse nicht) |V|
| C9a | 11 | Mega-Boomer: Sind die zwei Hälften kleine Bosse mit eigenem Leben (zusammen in einer Leiste) oder normale Boomer? | Kleine Bosse mit je der halben Restgesundheit, eine gemeinsame Leiste; beide müssen fallen |V|
| C9b | 11 | Mega-Boomer: Was tun die Pfützen? | Sie glühen 1 s auf, schaden dann bei Kontakt (1 Herz) und verschwinden nach 6 s; sie explodieren nicht, damit der Kampf lesbar bleibt |V|
| C10 | 4, 5, 8, 11 | Größe der Bosse und Punkte beim Sieg? | Leinwand 128×128 (Koloss, Mega-Boomer) bzw. 96×96 (Brutmutter); Sieg gibt 1000 Punkte, Sterne-Schwellen pro Boss-Level eigen |V|
| C11 | 3 bis 11 | Haben Level einen Namen auf der Karte (z. B. „1-2 Strandpromenade“) oder nur die Nummer? | Nummer und kurzer Name; Namen schlage ich mit den Leveln vor |V|
| C12 | 11 | Was passiert nach dem Sieg über den Mega-Boomer (Ende der Kampagne)? | „Alle Inseln geschafft!“ auf dem Ergebnisbildschirm, danach Karte; kein Abspann in v0.6.0 |V|

## Waffen

Fragen zum Epic #171 (Lastenheft F-SPF-05, Abschnitt 8). Heute gibt es eine Waffe mit 6 Schuss/s und Schaden 1. Bosse und Sterne-Schwellen sind darauf abgestimmt.

| # | Epic | Frage | Vorschlag | Entscheidung |
|---|---|---|---|---|
| W1 | #171 | Kommen weitere Waffen vor oder nach 1.0? | Nach 1.0 als v1.1.0; die Kampagne ist mit einer Waffe fertig abgestimmt, neue Waffen würden Bosse und Sterne verschieben |V|
| W2 | #171 | Welche Waffen gibt es? | Drei neue zur Startwaffe: Sturmgewehr (schneller, etwas Streuung), Schrotflinte (langsam, 5 Kugeln im Fächer, kurze Reichweite), Dual-Pistolen (abwechselnd links und rechts, sehr schnell, wenig Schaden) |V|
| W3 | #171 | Wodurch unterscheiden sich die Waffen? | Nur Feuerrate, Schaden, Streuung, Kugeln pro Schuss, Reichweite und Klang; kein Nachladen, keine Munition (passt zum Auto-Feuer, F-STE-02) |V|
| W4 | #171 | Wie werden Waffen freigeschaltet? | Je eine Waffe durch den Sieg über einen Boss (Koloss, Brutmutter, Mega-Boomer) |V|
| W5 | #171 | Wie wählt man die Waffe? | Vor dem Level im Menü, ähnlich dem Skin-Karussell; die Wahl wird gespeichert. Kein Wechsel im Level |Es sollte die möglichkeit geben, Waffen im Level zu wechseln|
| W6 | #171 | Gibt es Waffen auch als Pickup im Level (zeitlich begrenzt, wie ein Perk)? | Nein; Perks bleiben der einzige Pickup |Es gibt auch Waffen im Level|
| W8 | #171 | Wie hebt man eine Waffe im Level auf (zu W5, W6)? | | Waffen hängen als Silhouette an festen Stellen an der Wand. Man bleibt etwa 0,5 s davor stehen, ein Ring füllt sich, dann ist die Waffe aufgehoben; kostenlos. Sie bleibt bis Levelende, auch wenn sie noch nicht freigeschaltet ist |
| W9 | #171 | Wie viele Waffen hat man im Level dabei? | Zwei Slots: die vor dem Level gewählte Waffe und eine aufgehobene; eine neue Wandwaffe ersetzt den zweiten Slot |V|
| W10 | #171 | Wie wechselt man zwischen den zwei Slots? | Kleiner Tausch-Button neben dem Dash, erscheint erst mit zwei Waffen |V|
| W7 | #171 | Werden Bosse und Sterne-Schwellen pro Waffe abgestimmt? | Nein; alle Waffen haben etwa denselben Schaden pro Sekunde wie die Startwaffe (etwa 6) und unterscheiden sich im Spielgefühl, nicht in der Stärke |V|

## Aus Level 1-1 (vor dem Spieltest)

| # | Frage | Vorschlag | Entscheidung |
|---|---|---|---|
| P1 | Kill-Gates zählen global ab Levelstart (F-LVL-03). Ein zweites Kill-Gate später im Level ist deshalb oft schon offen, bevor der Spieler ankommt. Sollen Kill-Gates optional ab Betreten ihrer Zone zählen? | Option „ab Zone zählen“ für spätere Level; Lastenheft F-LVL-03 entsprechend anpassen |Dein Vorschlag|
| P2 | Die letzte Zone von Level 1-1 hat kein Tor; man kann am letzten Ansturm vorbei zum Ausgang laufen. Soll der Ausgang erst nach den Wellen der letzten Zone aufgehen? | Nach dem Spieltest entscheiden |Nach letzter Welle|
| P3 | Der Missions-Tracker zeigt in der letzten Zone nichts an. Soll dort „Zum Ausgang!“ stehen? | Ja |Ja|

## v1.0.0 – Release

Fragen zu den Epics #42, #52, #53 und #54, entschieden am 05.11.2026. Schon vorher entschieden: Bundle-ID `com.beratkrhn.rushops`, Mindestversion iOS 15 (Issue #13), keine Käufe, keine Werbung, keine Online-Funktionen (Lastenheft Abschnitt 7).

| # | Epic | Frage | Vorschlag | Entscheidung |
|---|---|---|---|---|
| R1 | #42 | Kommt Musik in 1.0 (G11 hatte sie zurückgestellt)? | Nach 1.0 verschieben; F-AUD-01 ist „Soll“ | V |
| R2 | #53 | In welchen Stores erscheint 1.0? | Erst iOS, Android später | Nur iOS; Android wird nicht veröffentlicht |
| R3 | #53 | Preis? | Kostenlos, ohne Käufe und Werbung | V |
| R4 | #53 | Wo liegt die Datenschutzerklärung? | GitHub Pages (Repo ist öffentlich) | V |
| R5 | #54 | Ist das Apple Developer Program vorhanden? | | Ja |
| R6 | #53 | Sprache des Store-Eintrags? | Nur Deutsch, passend zum Spiel | V |
| R7 | #53 | Motiv des App-Icons? | Spielerfigur mit Pistole, dahinter eine Zombie-Silhouette auf dunklem Grund, per Skript | V |
| R8 | #52 | Testgerät für NF-01? | | iPhone 14 oder neuer. Weil es keine Mittelklasse-Referenz ist, kommen Profiling im Editor, ein festes Frame-Budget und ein Stresslevel dazu |
| R9 | alle | Wann wird am Gerät gespielt und abgenommen? | | Einmal ganz am Ende. Die Spieltest-Issues #156, #160 und #178 sind gelöscht und gehen im Gesamtspieltest von v1.0.0 auf |
| R10 | #53 | Nur iPhone oder auch iPad? | Nur iPhone; auf dem iPad bleibt es über den Kompatibilitätsmodus installierbar | V |
| R11 | #54 | Was wird aus v0.7.0, v0.8.0 und v1.1.0, die nie einzeln veröffentlicht wurden? | Milestones schließen; ihre Inhalte (Insel 2 und 3, Waffen) erscheinen mit 1.0.0, Tags gibt es erst ab `v1.0.0` | V |

## Ab v1.0.0 – Endlos-Modus und weitere Epics

Entschieden am 06.11.2026 (Notion-Seite „Design-Fragen ab v1.0.0“). „V“ heißt „Vorschlag übernommen“; leere Antworten galten als V. Ersetzt R9 (Spieltest nur am Ende) durch A3. Ablauf und Roadmap in [`roadmap.md`](roadmap.md#ab-v100--erweiterungen).

### Allgemein

| # | Frage | Vorschlag | Entscheidung |
|---|---|---|---|
| A1 | Reihenfolge der Epics? | Erst Inhalte (Beute, Perks, Superwaffen, Hunde), dann Modi | Endlos-Modus zuerst, danach wie vorgeschlagen |
| A2 | Versionsnummern ohne Veröffentlichung? | Intern weiterzählen, Release-Version am Ende festlegen | V |
| A3 | Spieltest nur am Ende (R9)? | Nach jedem Epic kurz am Gerät | Getestet wird, wann Berat es sagt; kein fester Takt |
| A4 | Neue Inhalte auch in den 9 Leveln? | Gold, Perks, Superwaffen überall; Hunde im Endlos-Modus und ab v1.8.0 in neuen Zonen; Sterne in v1.9.0 neu | V |
| A5 | Vierte Insel? | Nicht in diesem Plan | V |

### v1.1.0 – Endlos-Modus

| # | Frage | Vorschlag | Entscheidung |
|---|---|---|---|
| E1 | Eine Arena oder Räume nacheinander? | Räume aus einem Pool | **Eine runde Arena** (Kolosseum) mit Brunnen in der Mitte; Zombies spawnen an festen Punkten im Norden, Osten, Süden und Westen |
| E2 | Rätsel im Endlos-Modus? | Ab und zu | **Nein**, nur Endlos |
| E3 | Bosse? | Alle 10 Runden einer der drei Bosse, jedes Mal mehr Leben | V |
| E4 | Wie wird es schwerer? | Höchstzahl 10 bleibt | Höchstzahl darf überschritten werden |
| E5 | Herzen? | +1 Herz nach jeder Runde | V |
| E6 | Welche Waffen? | Gewählte Waffe plus Wandwaffen | Alle 5 Runden wird eine weitere Waffe freigeschaltet; die Waffen hängen nebeneinander |
| E7 | Ab wann spielbar? | Nach dem Koloss | Von Anfang an |
| E8 | Wertung? | Score und Runde, lokale Top 10 | V |
| E9 | Belohnungen? | Skins für Runde 10, 25, 50 | V |
| E10 | Zwischenstand? | Nein | V |

### v1.2.0 – Beute

| # | Frage | Vorschlag | Entscheidung |
|---|---|---|---|
| B1 | Gold nur Punkte oder Währung? | Nur Punkte | **Währung**: ein Goldskin soll damit kaufbar sein |
| B2 | Gold und Multiplikator? | Punkte × Multiplikator, verlängert Kombo-Fenster | V |
| B3 | Wertstufen? | Münze und Edelstein | **Nur Gold** |
| B4 | Verschwindet Gold? | Nach 8 s, blinkt in den letzten 2 s | V |
| B5 | Magnet? | 1 Kachel, später als Upgrade größer | V |

### v1.3.0 – Begleiter und Strohmann

| # | Frage | Vorschlag | Entscheidung |
|---|---|---|---|
| K1 | Begleiter zeitlich oder bis Levelende? | 15 s, erneutes Aufsammeln setzt zurück | V |
| K2 | Kann er sterben? | Nein | V |
| K3 | Wie stark? | Halber Schaden pro Sekunde der Startwaffe, Reichweite 5 | V |
| K4 | Aussehen? | Kleine Kampfdrohne | V |
| K5 | Wo steht der Strohmann? | Am Aufsammelort | V |
| K6 | Ende des Strohmanns? | Explodiert ohne Spielerschaden, Kills zählen | V |
| K7 | Wirkt er auf Bosse? | Nein | V |
| K8 | Gewichte? | Begleiter 10, Strohmann 8, Drop-Chance bleibt 12 % | V |

### v1.4.0 – Superwaffen

| # | Frage | Vorschlag | Entscheidung |
|---|---|---|---|
| W11 | Slot-Waffen oder Superwaffen? | Seltene Superwaffen als Perk-Drop, 12 s | V |
| W12 | Rakete trifft Spieler? | Nein | V |
| W13 | Nachbrennen? | 1,5 s | V |
| W14 | Volle Wirkung auf Bosse? | Ja | V |
| W15 | Gewicht? | Je 4 | V |

### v1.5.0 – Hunde

| # | Frage | Vorschlag | Entscheidung |
|---|---|---|---|
| H1 | Gegner oder Begleiter? | Gegner | Gegner; im Endlos-Modus ist jede 6. Runde eine Hunderunde, in der nur Hunde kommen |
| H2 | Unterschied zum Runner? | Rudel von 3, angekündigter Sprung (0,4 s ducken) | V |
| H3 | Leben und Schaden? | Wie Runner, 1 Herz | V |
| H4 | Zählen zur Höchstzahl? | Ja, Rudel zählt 3 | V |
| H5 | Wo spawnen sie? | Normale Spawnpunkte, Rudel gemeinsam | V |
| H6 | Dieb-Gegner? | Nach dem Release | V |

### v1.6.0 – Rush-Meter

| # | Frage | Vorschlag | Entscheidung |
|---|---|---|---|
| RM1 | Rush-Meter überhaupt? | Ja | V |
| RM2 | Was macht der Rush? | 5 s Gegner-Zeitlupe 50 %, Dash ohne Abklingzeit | V, zusätzlich **mehr Schaden** |
| RM3 | Auslösen? | Eigener Button links oben | V |
| RM4 | Füllung? | 25 Kills, mit Multiplikator schneller, Treffer leert nicht | V |
| RM5 | Wo? | Kampagne und Endlos | V |
| RM6 | Zeitlupe auf Bosse? | Schwächer (75 %) | V |

### v1.7.0 – Endlos-Upgrades und Tageslauf

| # | Frage | Vorschlag | Entscheidung |
|---|---|---|---|
| T1 | Welche Upgrades? | 10 bis 12, z. B. Abpraller, +1 Herz, längere Perks, Gold-Magnet, Feuerspur, Begleiter dauerhaft | V |
| T2 | Wie oft? | 1 aus 3 nach jeder dritten Runde | V |
| T3 | Versuche im Tageslauf? | Beliebig viele, der beste zählt | V |
| T4 | Austricksen über die Uhr egal? | Ja | V |

### v1.8.0 – Herausforderungen und Rätsel mit Zombies

| # | Frage | Vorschlag | Entscheidung |
|---|---|---|---|
| CH1 | Herausforderungen statt Sterne? | Zusätzlich als Abzeichen, die Skins freischalten | V |
| CH2 | Mutatoren? | Nach dem Release | V |
| Z1 | Welche Rätsel? | Druckplatte (nur Brute), rissige Wand (nur Boomer) | V, mit **Hinweisen** |
| Z2 | Wo? | Endlos-Räume und je eine Zone pro Insel | Nur Kampagne (wegen E2) |

### Nachfragen

| # | Frage | Vorschlag | Entscheidung |
|---|---|---|---|
| N1 | Preis und Zahl der Goldskins? | Einer, „Goldrausch“, 2500 Gold | V |
| N2 | Bosse und Brunnen (L5)? | Runder Brunnen, Radius etwa 2 Kacheln; Bosse gleiten am Rand entlang | V |
| N3 | Höchstzahl im Endlos-Modus? | Start 10, +1 alle 5 Runden, höchstens 20, gegen Frame-Budget messen | V |
| N4 | Waffen alle 5 Runden? | Start mit Pistole; Runde 5 Schrotflinte, 10 Sturmgewehr, 15 Dual-Pistolen, unabhängig von der Kampagne | V |
| N5 | Was ist eine Runde? | Eine Welle bis alle Gegner tot; 4 s Pause mit „Runde X“ | V |
| N6 | Boss- und Hunderunde zugleich? | Boss geht vor, Hunderunde eine Runde später | V |
| N7 | Hunderunde vor v1.5.0? | Kommt mit dem Hunde-Epic | V |
| N8 | Mehr Schaden im Rush? | ×1,5, mit Doppelschaden ×3 | V |
| N9 | Hinweise für Zombie-Rätsel? | Symbol am Rätsel und beim ersten Mal Text im Missions-Tracker | V |
| N10 | Tileset der Arena? | Eigenes Kolosseum-Tileset aus der gemeinsamen Palette | V |
