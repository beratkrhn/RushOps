# ADR 0005: Spielstand als versioniertes JSON

- **Status:** angenommen
- **Datum:** 2026-10-29

## Kontext

Gespeichert werden nur Meta-Fortschritt und Einstellungen (Lastenheft Kapitel 4). Das Spiel ist offline. Der Spielstand muss spätere Updates überleben und darf bei Beschädigung nicht zum Absturz führen.

## Entscheidung

- Eine JSON-Datei `save.json` in `Application.persistentDataPath`, serialisiert mit `JsonUtility`.
- Feld `Version` im Spielstand; beim Laden migrieren Migrationsschritte ältere Versionen schrittweise auf die aktuelle.
- **Atomares Schreiben**: erst `save.json.tmp` schreiben, dann die alte Datei als `save.json.bak` sichern und die neue an ihre Stelle verschieben.
- Beim Laden: `save.json`, bei Fehler `save.json.bak`, sonst neuer Spielstand.

## Begründung und Alternativen

- **PlayerPrefs**: ungeeignet für verschachtelte Daten, schlecht testbar, keine Versionierung. Verworfen.
- **Binärformat**: schwer zu debuggen, kein Mehrwert bei dieser Datenmenge.

## Konsequenzen

- Der Spielstand ist für Nutzer lesbar und editierbar; bei einem Offline-Spiel ohne Wettbewerb akzeptabel.
- Laden, Speichern und Migration laufen über eine reine C#-Klasse mit EditMode-Tests.

## Umsetzung (Issue #122, 04.11.2026)

- Namespace `RushOps.Save`. `SaveData` mit `Progress` (Level-Einträge) und `Stats`; die Felder sind öffentlich, weil `JsonUtility` Felder serialisiert und ihre Namen die JSON-Schlüssel sind.
- `SaveGameStore` liest und schreibt über `ISaveFileSystem` (`DiskSaveFileSystem` auf dem Gerät, im Speicher in den Tests). Laden wirft nie; `LastSource` sagt, ob Hauptdatei, Backup oder ein neuer Spielstand verwendet wurde.
- Ist die Hauptdatei unlesbar und wurde das Backup geladen, macht das nächste Speichern die kaputte Datei **nicht** zum Backup, sondern legt sie als `save.json.broken` ab. So geht das gute Backup nicht verloren.
- Ein Spielstand mit **neuerer** Version als der des Builds wird nicht gelesen (wie eine beschädigte Datei).
- Migrationsschritte (`ISaveMigration`) arbeiten auf dem JSON-Text, eine Stufe pro Version; `SaveMigrator` prüft die Kette. Neue Felder mit passendem Standardwert brauchen keine neue Version.
- `SaveGameService` (ScriptableObject `Data/Save/SaveGameService.asset`) ist der Zugang für alle Szenen: Er lädt beim ersten Zugriff und hält den Spielstand, bis die App endet. Szenen referenzieren das Asset statt einen Manager zu suchen.
- Freigeschaltete Inseln und Level (F-DAT-04) werden nicht eigens gespeichert, sondern aus den geschafften Leveln und der Reihenfolge im `Campaign`-Asset abgeleitet (`CampaignUnlocks`, Issue #123). So können Spielstand und Freischaltung nicht auseinanderlaufen, und ein neu eingefügtes Level ist sofort richtig eingeordnet. Die Level-ID im Spielstand ist der Szenenname.

## Version 2 (Issue #176, 05.11.2026)

- Neuer Block `Weapons` (`WeaponsData`): gewählte Waffe (`Current`, leer = Startwaffe) und freigeschaltete Waffen (`Unlocked`, Waffen-IDs).
- Erster Migrationsschritt `SaveMigrationV1ToV2`: Er legt den Block leer an und lässt alles andere unverändert.
- Welche Waffe welcher Bosssieg freischaltet, steht nicht im Spielstand, sondern in der `WeaponDefinition` (`UnlockLevel`). Bosse, die vor dem Update besiegt wurden, schalten ihre Waffe beim nächsten Prüfen frei (Öffnen der Waffenwahl, Levelende). Die Migration muss die Zuordnung deshalb nicht kennen.
