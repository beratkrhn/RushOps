# ADR 0005: Spielstand als versioniertes JSON

- **Status:** angenommen
- **Datum:** 2026-10-29

## Kontext

Gespeichert werden nur Meta-Fortschritt und Einstellungen (Lastenheft Kapitel 4). Das Spiel ist offline. Der Spielstand muss spätere Updates überleben und darf bei Beschädigung nicht zum Absturz führen.

## Entscheidung

- Eine JSON-Datei `save.json` in `Application.persistentDataPath`, serialisiert mit `JsonUtility`.
- Feld `version` im Spielstand; beim Laden migrieren Migrationsschritte ältere Versionen schrittweise auf die aktuelle.
- **Atomares Schreiben**: erst `save.json.tmp` schreiben, dann die alte Datei als `save.json.bak` sichern und die neue an ihre Stelle verschieben.
- Beim Laden: `save.json`, bei Fehler `save.json.bak`, sonst neuer Spielstand.

## Begründung und Alternativen

- **PlayerPrefs**: ungeeignet für verschachtelte Daten, schlecht testbar, keine Versionierung. Verworfen.
- **Binärformat**: schwer zu debuggen, kein Mehrwert bei dieser Datenmenge.

## Konsequenzen

- Der Spielstand ist für Nutzer lesbar und editierbar; bei einem Offline-Spiel ohne Wettbewerb akzeptabel.
- Laden, Speichern und Migration laufen über eine reine C#-Klasse mit EditMode-Tests.
