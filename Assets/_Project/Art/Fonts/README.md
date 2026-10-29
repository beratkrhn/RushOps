# Pixel-Schrift

`PixelFont` ist die Schrift für HUD und Menüs (Issue #11).

- **Herkunft:** selbst gezeichnet für RushOps. Die Glyphen stehen als 5×7-Raster in [`Tools/pixel_font.py`](../../../../Tools/pixel_font.py). `Tools/gen_pixelart.py` erzeugt daraus `PixelFont.png` (Atlas) und `PixelFont.json` (Metriken), das Editor-Menü `RushOps → Art → Pixel-Schrift aktualisieren` daraus `PixelFont.fontsettings` und `PixelFont.mat`.
- **Lizenz:** Teil des Projekts RushOps, keine fremden Rechte. Frei nutzbar im Projekt und in Builds; keine Namensnennung nötig.
- **Zeichen:** A–Z, a–z, Ä Ö Ü ä ö ü ß, 0–9 und `. , : ; ! ? - + = / ( ) ' " % ×`. Fehlende Zeichen werden nicht angezeigt; neue Glyphen in `Tools/pixel_font.py` ergänzen.
- **Verwendung:** uGUI `Text` mit diesem Font. Die Schrift wird in ihrer Pixelgröße gerendert (Großbuchstaben 7 px, Zeilenhöhe 10 px); vergrößert wird über die Skalierung des `RectTransform` in ganzen Zahlen (2×, 3×), nicht über die Schriftgröße.
