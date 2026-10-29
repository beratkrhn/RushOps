# ADR 0001: Unity 6 mit URP 2D

- **Status:** angenommen
- **Datum:** 2026-10-29

## Kontext

RushOps ist ein 2D-Pixelart-Spiel für iOS und Android, entwickelt auf macOS. Benötigt werden gute Mobile-Performance, 2D-Leuchteffekte und eine langfristig gepflegte Render-Pipeline.

## Entscheidung

Unity **6000.3.10f1** mit der **Universal Render Pipeline (2D Renderer)**, dem neuen **Input System** und dem **Unity Test Framework**. Assets werden als Text serialisiert (Force Text).

## Begründung und Alternativen

- **Built-in Render Pipeline**: wird nicht mehr aktiv weiterentwickelt, kein 2D-Licht. Verworfen.
- **URP 2D**: 2D-Licht für Perks, Explosionen und Zonen, Pixel Perfect Camera, für Mobile optimiert.
- **Godot**: ebenfalls geeignet, aber Unity bietet bessere Mobile-Toolchain und Dokumentation, und das Projekt ist bereits angelegt.

## Konsequenzen

- Shader und Materialien müssen URP-kompatibel sein (2D-Lit- oder Unlit-Sprite-Shader).
- Text-Serialisierung ermöglicht Diffs und Bearbeitung durch Claude, Szenen-Merges bleiben aber konfliktträchtig. Daher: Szenen klein halten, Inhalte in Prefabs auslagern.
