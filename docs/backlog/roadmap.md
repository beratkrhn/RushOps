# Roadmap (Epics)

Diese Blöcke sind bewusst grob. Sie werden erst nach dem Spieltest von v0.3.0 in Stories zerlegt, weil die Erfahrungen am Gerät das Design noch verändern. Auf GitHub sind sie als Issues mit den Labels `epic` und `needs-design` angelegt und werden von „Mach das nächste Issue“ übersprungen.

Ein Epic wird zerlegt, indem eine Backlog-Datei `docs/backlog/vX.Y.Z.md` im Format der bisherigen angelegt, daraus Story-Issues erzeugt und das Epic-Issue mit Verweis auf die Stories geschlossen wird.

## v0.4.0 – Grafik und Sound

In Stories zerlegt: [`v0.4.0.md`](v0.4.0.md) (#99 bis #109). Die Epics #38 bis #41 sind damit erledigt. Musik (#42) ist zurückgestellt (Entscheidung G11).

### Finale Palette und Figuren (#38)
Endgültige Palette, Spieler und alle vier Gegnertypen mit Animationen (Laufen, Treffer, Tod), Standard-Skin.
*Offen:* Stilrichtung und Stimmung (düster, bunt, comichaft), Referenzspiele.

### Tilesets für drei Inseln (#39)
Je ein thematisches Tileset pro Insel inklusive Wänden, Toren, Schaltern, Generator, Spawnpunkten und Dekoration.
*Offen:* Themen der drei Inseln.

### Effekte und 2D-Licht (#40)
Mündungsfeuer, Treffer, Explosionen, Perk-Leuchten, Zonen- und Rätsel-Licht, Todesanimationen, Partikel.

### Soundeffekte final (#41)
Alle Platzhalter-Sounds ersetzen, einheitlicher Retro-Klang, Audio-Mixer mit Gruppen Musik und Effekte.

### v1.3.0 – Begleiter und Strohmann

In Stories zerlegt: [`v1.3.0.md`](v1.3.0.md) (#244 bis #247, Epic #215). Entscheidungen K1 bis K8, Lastenheft F-PRK-05 bis F-PRK-08.

### v1.4.0 – Superwaffen

In Stories zerlegt: [`v1.4.0.md`](v1.4.0.md) (#253 bis #256, Epic #216). Entscheidungen W11 bis W15, Lastenheft F-PRK-09 bis F-PRK-12.

### v1.5.0 – Hunde

In Stories zerlegt: [`v1.5.0.md`](v1.5.0.md) (#262 bis #264, Epic #217). Entscheidungen H1 bis H6, N6, N7, Lastenheft F-GEG-05 bis F-GEG-07 und F-END-10.

### Musik (#42)
Ein Menütrack und je ein loopbarer Action-Track pro Insel.
*Offen:* selbst erzeugt oder lizenzfreie Packs.

## v0.5.0 – Menüs, Speichern, Meta-Progression

In Stories zerlegt: [`v0.5.0.md`](v0.5.0.md) (#122 bis #131). Die Epics #43 bis #47 sind damit erledigt.

### Logo-Animation und Hauptmenü (#43)
Startsequenz, Hauptmenü mit Story Mode, Spielerauswahl und Einstellungen.

### Einstellungen (#44)
Lautstärke (Gesamt, Musik, Effekte), Stick-Modus, Stick-Größe und -Position, Zielhilfe, Credits.

### Savegame (#45)
Umsetzung nach ADR 0005: Autosave-Trigger, Migration, Backup, Statistiken.

### Skins und Freischaltungen (#46)
Skin-Karussell, Freischaltung pro Level und über Meilensteine, Anzeige der Bedingungen gesperrter Skins.
*Offen:* Liste der Skins und Meilensteine.

### Inselkarte und Levelauswahl (#47)
Drei Inseln mit Brücken, sequenzielle Freischaltung, Sterne pro Level.

## v0.6.0 bis v0.8.0 – Inhalte und Bosse

In Stories zerlegt, je Insel ein Milestone (Entscheidung C1): [`v0.6.0.md`](v0.6.0.md) Insel 1 und Boss-Grundlage (#147 bis #152), [`v0.7.0.md`](v0.7.0.md) Insel 2 (#153 bis #156), [`v0.8.0.md`](v0.8.0.md) Insel 3 (#157 bis #160). Die Epics #48 bis #51 sind damit erledigt.

### Level 1-2, 2-1, 2-2, 3-1, 3-2 (#48)
Die übrigen regulären Level mit steigender Schwierigkeit.

### Boss: Koloss (Level 1-3) (#49)
Lädt sich auf und stürmt geradeaus; lehrt das Timing des Dash.

### Boss: Brutmutter (Level 2-3) (#50)
Spawnt laufend Runner; Schaden machen unter Kontrolle der Spawns.

### Boss: Mega-Boomer (Level 3-3) (#51)
Pfützen, teilt sich bei halber Gesundheit in zwei Boomer.

## v1.0.0 – Stand der Kampagne

In Stories zerlegt: [`v1.0.0.md`](v1.0.0.md) (#189 bis #196), Entscheidungen R1 bis R11 in [`design-fragen.md`](design-fragen.md#v100--release). Die Kampagne mit drei Inseln, Bossen und Waffen ([`v1.0.0-waffen.md`](v1.0.0-waffen.md)) ist fertig. Erledigt sind Leistungsmessung (#189, #190), App-Icon, Store-Screenshots und Store-Eintrag (#191 bis #193); die Epics #52 und #53 sind damit abgeschlossen.

**Entscheidung vom 06.11.2026:** Es wird noch nicht veröffentlicht. Release-Build, Gesamtspieltest und Einreichung (#194 bis #196, Epic #54) liegen im Milestone **Release** ganz am Ende. Am Gerät wird getestet, wann Berat es sagt (A3, ersetzt R9).

## Ab v1.0.0 – Erweiterungen

Entscheidungen A1 bis A5, B, K, W11 bis W15, H, E, RM, T, CH, Z und N1 bis N10 in [`design-fragen.md`](design-fragen.md#ab-v100--endlos-modus-und-weitere-epics). Jede Version ist ein Milestone mit einem Epic-Issue; zerlegt wird jeweils, wenn die vorherige fertig ist.

| Version | Epic | Kern |
|---|---|---|
| v1.1.0 | Endlos-Modus | runde Arena mit Brunnen, Runden ohne Ende, Bosse alle 10 Runden, Waffen alle 5 Runden, lokale Top 10 |
| v1.2.0 | Beute | Gold von Zombies als Währung, Goldskin kaufbar |
| v1.3.0 | Begleiter und Strohmann | zwei neue Perks |
| v1.4.0 | Superwaffen | Flammenwerfer und Raketenwerfer als seltene Drops |
| v1.5.0 | Hunde | neuer Gegnertyp, Hunderunde alle 6 Runden im Endlos-Modus |
| v1.6.0 | Rush-Meter | aufladbarer Spezialmodus mit Zeitlupe und mehr Schaden |
| v1.7.0 | Endlos-Upgrades und Tageslauf | Upgrade-Wahl nach jeder dritten Runde, täglicher Lauf offline |
| v1.8.0 | Herausforderungen und Rätsel mit Zombies | Abzeichen pro Level, Druckplatte und rissige Wand mit Hinweisen (nur Kampagne) |
| v1.9.0 | Feinschliff | Musik (#42), Haptik, Balancing, Sterne-Schwellen neu |
| Release | Release | #194 bis #196, Store-Material aktualisieren |

### v1.1.0 – Endlos-Modus

In Stories zerlegt: [`v1.1.0.md`](v1.1.0.md) (#205 bis #212, Epic #213). Die Epics der folgenden Versionen sind #214 bis #220; v1.9.0 enthält Musik (#42).

### v1.2.0 – Beute

In Stories zerlegt: [`v1.2.0.md`](v1.2.0.md) (#233 bis #237, Epic #214). Entscheidungen B1 bis B5, N1 und N11 bis N14, Lastenheft Abschnitt 2.11.

### Musik (#42)

Ein Menütrack, je ein loopbarer Action-Track pro Insel und einer für die Arena, per Skript erzeugt (G11, G12). Teil von v1.9.0.
