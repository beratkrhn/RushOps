# ADR 0003: Flow-Field statt NavMesh für die Gegnerbewegung

- **Status:** angenommen
- **Datum:** 2026-10-29

## Kontext

Alle Gegner haben dasselbe Ziel: den Spieler. Es sind bis zu 10 Gegner plus Bosse gleichzeitig unterwegs, auf Mobilgeräten. Unitys NavMesh unterstützt 2D nicht direkt (nur über Zusatzpakete).

## Entscheidung

Ein **Flow-Field** auf dem Kachelraster des Levels: Vom Spieler aus wird per Breitensuche (bzw. Dijkstra) für jede begehbare Kachel die Richtung zum Spieler berechnet. Jeder Gegner liest nur die Richtung seiner Kachel aus. Neuberechnung, wenn der Spieler die Kachel wechselt, höchstens alle 0,1 s. Dazu eine einfache Separation, damit sich Gegner nicht stapeln.

## Begründung und Alternativen

- **NavMesh mit NavMeshPlus**: Fremdpaket, pro Agent eigene Pfadsuche, mehr Overhead.
- **A\* pro Gegner**: Kosten wachsen mit der Gegnerzahl.
- **Flow-Field**: eine Berechnung für alle Gegner, Kosten unabhängig von der Gegnerzahl, gut testbar (reine Rasterlogik).

## Konsequenzen

- Hindernisse müssen auf dem Kachelraster liegen (Tilemap-Kollision).
- Geschlossene Tore sind unbegehbar; beim Öffnen wird das Feld neu berechnet.
- Die Rasterlogik lebt in einer reinen C#-Klasse und wird mit EditMode-Tests abgedeckt.
