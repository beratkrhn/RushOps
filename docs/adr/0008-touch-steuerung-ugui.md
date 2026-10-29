# ADR 0008: Touch-Steuerung als On-Screen-Controls in uGUI

- **Status:** angenommen
- **Datum:** 2026-10-29

## Kontext

Auf dem Handy steuert der Spieler mit zwei virtuellen Sticks und einem Dash-Button (F-STE-01 bis F-STE-05, F-HUD-07). Die Spielerkomponenten lesen bereits die Input-Actions `Player/Move`, `Player/Aim` und `Player/Dash`, die an das Gamepad gebunden sind. Die Sticks sollen fest oder floating (am Berührungspunkt der jeweiligen Bildschirmhälfte) sein, beide gleichzeitig bedienbar und innerhalb der Safe Area liegen.

## Entscheidung

- Die Touch-Steuerung besteht aus **On-Screen-Controls des Input Systems** (`OnScreenControl`). Sie schreiben in ein virtuelles Gamepad (`<Gamepad>/leftStick`, `<Gamepad>/rightStick`, `<Gamepad>/buttonSouth`). Die Spielerkomponenten bleiben unverändert und wissen nichts von Touch.
- Die Sticks sind eine eigene Komponente `VirtualStick` (abgeleitet von `OnScreenControl`) statt des mitgelieferten `OnScreenStick`. Die Regeln (Heimposition, Floating-Ursprung, Klemmen an die Zone, Auslenkung) liegen in der reinen Klasse `VirtualStickMath`.
- Der Dash-Button ist ein mitgeliefertes `OnScreenButton`; `DashButtonView` platziert ihn und zeigt die Abklingzeit.
- Die UI läuft auf **uGUI** (Canvas „Screen Space – Overlay“, `CanvasScaler` mit Referenz 360×640, an die Breite angepasst) mit `InputSystemUIInputModule`. Ein `SafeAreaFitter` setzt die Anker auf `Screen.safeArea`.
- Die Touch-Steuerung ist nur sichtbar, wenn ein Touchscreen vorhanden ist (Handy, Device Simulator); im Game View bleiben Maus und Tastatur unverändert (F-STE-07).

## Begründung und Alternativen

- **`OnScreenStick`**: kennt zwar einen dynamischen Ursprung, aber nur in einem Radius um den Stick, nicht in einer ganzen Bildschirmhälfte, und kein Klemmen an die Safe Area. Eine eigene Ableitung ist klein und lässt sich testen.
- **Touch direkt auswerten** (`EnhancedTouch`) und die Werte an die Spielerkomponenten geben: eine zweite Eingabeschiene neben den Actions; Gamepad, Tastatur und Touch liefen getrennt.
- **UI Toolkit**: On-Screen-Controls setzen auf das uGUI-Event-System (`IPointerDownHandler`). Für das HUD (#11) kann UI Toolkit später noch gewählt werden; die Touch-Steuerung bleibt dann ein eigener Canvas.

## Konsequenzen

- Jede neue Touch-Eingabe ist eine Bindung im Input-Actions-Asset plus ein On-Screen-Control; die Spiellogik ändert sich nicht.
- Sobald ein On-Screen-Control aktiv ist, existiert ein virtuelles Gamepad. Code, der `Gamepad.current` abfragt, sieht es; deshalb immer über Actions lesen.
- Mehrere Finger laufen über die Pointer-IDs des Event-Systems; jeder Stick folgt nur der Berührung, die ihn gegriffen hat.
- Modus und Größen stehen in `TouchControlsConfig`; das Einstellungsmenü (v0.5.0) ändert sie und ruft `VirtualStick.Refresh()` auf.
