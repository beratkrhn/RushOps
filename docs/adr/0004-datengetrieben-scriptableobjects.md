# ADR 0004: Spieldaten in ScriptableObjects

- **Status:** angenommen
- **Datum:** 2026-10-29

## Kontext

Balancing (Tempo, Schaden, Perk-Dauer, Drop-Chancen, Wellen) wird häufig angepasst, oft durch Tests am Gerät. Änderungen sollen ohne Codeänderung möglich und im Git-Verlauf nachvollziehbar sein.

## Entscheidung

Alle Balancing- und Inhaltswerte liegen in ScriptableObjects unter `Assets/_Project/Data/`, z. B. `PlayerConfig`, `EnemyDefinition`, `PerkDefinition`, `PerkPool`, `WaveDefinition`, `LevelDefinition`, `SkinDefinition`. Code liest Werte nur aus diesen Objekten.

## Begründung und Alternativen

- **Konstanten im Code**: jede Anpassung ist eine Codeänderung. Verworfen.
- **JSON-Konfiguration**: kein Inspector, keine Asset-Verweise (Sprites, Prefabs). Verworfen.
- **ScriptableObjects**: im Inspector bearbeitbar, als YAML versionierbar, können Assets referenzieren.

## Konsequenzen

- Laufzeitzustand (z. B. aktuelle Gesundheit) wird **nicht** in ScriptableObjects geschrieben, nur Konfiguration.
- Tests können eigene Instanzen per `ScriptableObject.CreateInstance` erzeugen.
