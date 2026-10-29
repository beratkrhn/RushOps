# RushOps

Mobiler 2D-Top-down-Twin-Stick-Shooter im Pixelart-Stil für iOS und Android. Offline, Hochformat, Deutsch.

Gegnerhorden ausweichen, Perks taktisch einsetzen und Action-Rätsel lösen, während die Zombies weiter anrücken.

## Status

In Entwicklung. Aktueller Meilenstein: **v0.1.0 – Vertical Slice** (siehe [`docs/backlog/v0.1.0.md`](docs/backlog/v0.1.0.md)).

## Dokumentation

- [Lastenheft](docs/lastenheft.md)
- [Architekturentscheidungen (ADRs)](docs/adr/)
- [Changelog](CHANGELOG.md)
- [Arbeitsweise und Konventionen](CLAUDE.md)

## Einrichtung

Voraussetzungen: Unity Hub mit **Unity 6000.3.10f1** (Module iOS und Android), Git, Git LFS.

```bash
git lfs install          # einmalig pro Rechner
git clone https://github.com/beratkrhn/RushOps.git
```

Danach das Projekt im Unity Hub über „Add project from disk“ öffnen.

Optional, aber empfohlen: Unitys Smart Merge für Szenen- und Prefab-Konflikte einrichten (einmalig pro Rechner):

```bash
git config --global merge.unityyamlmerge.name "Unity SmartMerge"
git config --global merge.unityyamlmerge.driver \
  "'/Applications/Unity/Hub/Editor/6000.3.10f1/Unity.app/Contents/Tools/UnityYAMLMerge' merge -p %O %B %A %A"
git config --global merge.unityyamlmerge.recursive binary
```

## Roadmap

| Version | Inhalt |
|---|---|
| 0.1.0 | Vertical Slice: Bewegung, Schießen, Dash, Walker, Herzen, Testarena |
| 0.2.0 | Alle Gegnertypen, Perks, Kill-Gates, Wellen |
| 0.3.0 | Rätsel: Zonen-Kontrolle, Schalter, Objekt-Transport |
| 0.4.0 | Pixelart- und Sound-Pass |
| 0.5.0 | Menüs, Savegame, Skins, Inselkarte |
| 0.6.0 | Level-Content und Bosse |
| 1.0.0 | Release |
