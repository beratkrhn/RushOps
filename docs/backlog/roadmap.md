# Roadmap v0.4.0 bis v1.0.0 (Epics)

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

## v1.0.0 – Release

In Stories zerlegt: [`v1.0.0.md`](v1.0.0.md) (#189 bis #196), Entscheidungen R1 bis R11 in [`design-fragen.md`](design-fragen.md#v100--release). Die Epics #52 bis #54 sind damit erledigt. Erscheint nur im App Store für das iPhone (R2, R10); die Spieltests von v0.7.0, v0.8.0 und v1.1.0 gehen im Gesamtspieltest von v1.0.0 auf (R9).

### Performance und Gerätetests (#52)
Profiling im Editor mit Stresslevel und Frame-Budget, Messung auf dem iPhone, Akkuverbrauch, Speicher.

### Store-Vorbereitung (#53)
App-Icon, Screenshots, Store-Texte, Datenschutzerklärung, Altersfreigabe.

### Release 1.0.0 (#54)
Release-Build, TestFlight, Gesamtspieltest, Einreichung im App Store, Tag `v1.0.0`.

## Nach v1.0.0

### Musik (#42)
Ein Menütrack und je ein loopbarer Action-Track pro Insel, per Skript erzeugt (G11, G12). Nach 1.0 verschoben (R1), noch `needs-design`.

### Waffensystem (#171)
In Stories zerlegt: [`v1.1.0.md`](v1.1.0.md). Entscheidungen W1 bis W10 in [`design-fragen.md`](design-fragen.md#waffen).
