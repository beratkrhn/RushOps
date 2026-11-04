# ADR 0009: Insel-Tilesets über IslandTheme

- **Status:** angenommen
- **Datum:** 2026-11-04

## Kontext

Jede Insel bekommt ein eigenes Tileset: Strand, düstere Stadt, Vulkan (F-GFX-01, F-LVL-06, Issues #104 bis #106). Dazu gehört ein eigenes Tor, z. B. eine Holzpalisade oder ein Rolltor. Die Szenen entstehen per Editor-Skript. Bisher kannten die Builder nur ein festes Stein-Tileset und ein Gitter-Tor, das in den Boden sinkt.

## Entscheidung

- Das ScriptableObject `IslandTheme` (`Data/Islands/<Insel>.asset`) beschreibt das Aussehen einer Insel:
  - Bodenvarianten mit dem Anteil des schlichten Bodens
  - Außenwand (Oberseite, Vorderseite)
  - Hindernisse (Oberseite, Vorderseite), das sind die inneren Wände des Layouts
  - flache Dekorationen ohne Kollision, mit Dichte
  - Tor-Frames von geschlossen bis offen
- `ArenaTestSceneBuilder.BuildScene` bekommt den Pfad des Themes. `IslandThemes` (Editor) legt die Assets an und verknüpft die Sprites aus `Tools/gen_pixelart.py` (`Tiles/<Insel>/`). Abgestimmte Werte wie Bodenanteil und Dichte bleiben beim Neubau erhalten.
- Bodenvariante und Dekoration je Zelle kommen deterministisch aus `IslandThemeMath` (Hash der Zelle). Ein neu gebautes Level sieht also gleich aus.
- Hat das Theme mindestens zwei Tor-Frames, spielt `Gate` sie beim Öffnen ab. Mit nur einem Frame sinkt das Tor wie bisher in den Boden.
- Dekorationen liegen in einer eigenen Tilemap `Decor` ohne Collider, über dem Boden und unter den Bodenmarkierungen. Die Sortierreihenfolge ist: Boden −1, Dekoration 0, Markierungen 1, Figuren 2.
- Das Theme `Default` behält den bisherigen Stein-Look für die Testarenen.

## Begründung und Alternativen

- **Tile Palettes und Rule Tiles pro Insel:** Das wäre komfortabler für Handarbeit im Editor. Die Level entstehen aber per Skript, und Rule Tiles bräuchten mehr Sprites pro Insel. Verworfen.
- **Ein Tileset-Ordner nach Namenskonvention ohne ScriptableObject:** Dann hätten Dichte und Bodenanteil keinen Platz in den Daten (ADR 0004). Verworfen.
- **Tor als Animator-Clip:** Das bringt mehr Assets und passt nicht zur bestehenden `GateLogic`. Frames nach Öffnungsfortschritt sind einfacher und testbar.

## Konsequenzen

- Eine neue Insel braucht Sprites in `Tiles/<Insel>/` (gleiche Dateinamen) und einen Eintrag in `IslandThemes`.
- Level-Builder wählen ihre Insel über den Theme-Pfad in `BuildScene`.
