# Roadmap v0.4.0 bis v1.0.0 (Epics)

Diese Blöcke sind bewusst grob. Sie werden erst nach dem Spieltest von v0.3.0 in Stories zerlegt, weil die Erfahrungen am Gerät das Design noch verändern. Auf GitHub sind sie als Issues mit den Labels `epic` und `needs-design` angelegt und werden von „Mach das nächste Issue“ übersprungen.

Ein Epic wird zerlegt, indem eine Backlog-Datei `docs/backlog/vX.Y.Z.md` im Format der bisherigen angelegt, daraus Story-Issues erzeugt und das Epic-Issue mit Verweis auf die Stories geschlossen wird.

## v0.4.0 – Grafik und Sound

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

## v0.6.0 – Inhalte und Bosse

### Level 1-2, 2-1, 2-2, 3-1, 3-2 (#48)
Die übrigen regulären Level mit steigender Schwierigkeit.
*Offen:* Schwierigkeitskurve, neue Kombinationen von Rätseln.

### Boss: Koloss (Level 1-3) (#49)
Lädt sich auf und stürmt geradeaus; lehrt das Timing des Dash.

### Boss: Brutmutter (Level 2-3) (#50)
Spawnt laufend Runner; Schaden machen unter Kontrolle der Spawns.

### Boss: Mega-Boomer (Level 3-3) (#51)
Explosive Pfützen, teilt sich bei halber Gesundheit in zwei Boomer.

## v1.0.0 – Release

### Performance und Gerätetests (#52)
Profiling auf schwächeren Geräten, Testmatrix iOS und Android, Akkuverbrauch, Speicher.

### Store-Vorbereitung (#53)
Bundle-ID, App-Icons, Screenshots, Store-Texte, Datenschutzerklärung, Altersfreigabe, Mindestversionen von iOS und Android.

### Release 1.0.0 (#54)
Finale Builds, Einreichung in App Store und Google Play, Tag `v1.0.0`.
