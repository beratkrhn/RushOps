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

## Aus Level 1-1 (vor dem Spieltest)

| # | Frage | Vorschlag | Entscheidung |
|---|---|---|---|
| P1 | Kill-Gates zählen global ab Levelstart (F-LVL-03). Ein zweites Kill-Gate später im Level ist deshalb oft schon offen, bevor der Spieler ankommt. Sollen Kill-Gates optional ab Betreten ihrer Zone zählen? | Option „ab Zone zählen“ für spätere Level; Lastenheft F-LVL-03 entsprechend anpassen |Dein Vorschlag|
| P2 | Die letzte Zone von Level 1-1 hat kein Tor; man kann am letzten Ansturm vorbei zum Ausgang laufen. Soll der Ausgang erst nach den Wellen der letzten Zone aufgehen? | Nach dem Spieltest entscheiden |Nach letzter Welle|
| P3 | Der Missions-Tracker zeigt in der letzten Zone nichts an. Soll dort „Zum Ausgang!“ stehen? | Ja |Ja|
