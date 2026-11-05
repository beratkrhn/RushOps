# App-Store-Eintrag (App Store Connect)

Alle Texte und Angaben für die Einreichung (Issue #193). Grundlage sind die Entscheidungen R2, R3, R4, R6 und R10 in [`docs/backlog/design-fragen.md`](../backlog/design-fragen.md#v100--release). Der Store-Eintrag ist nur auf Deutsch (R6). Die Längen sind gezählt; Apple zählt Zeichen, nicht Bytes.

## Allgemein

| Feld | Wert |
|---|---|
| Plattform | iOS, nur iPhone (R2, R10) |
| Bundle-ID | `com.beratkrhn.rushops` |
| SKU | `rushops-ios` |
| Primärsprache | Deutsch |
| Kategorie | Spiele › Action, zweite Kategorie Spiele › Arcade |
| Preis | kostenlos (R3), keine In-App-Käufe |
| Copyright | `2026 <dein vollständiger Name>` (**offen**: Apple zeigt hier den Namen des Rechteinhabers) |
| Verfügbarkeit | **offen, entscheidest du** (siehe unten) |

## Texte

**Name** (7 von 30 Zeichen)

```
RushOps
```

**Untertitel** (25 von 30 Zeichen)

```
Zombiehorden im Pixelstil
```

**Werbetext** (161 von 170 Zeichen; lässt sich jederzeit ohne neue Prüfung ändern)

```
Drei Inseln, neun Level, drei Bosse: Halte die Zombiehorden mit zwei Daumen auf, schnapp dir Perks und Waffen und löse Rätsel unter Druck. Offline, ohne Werbung.
```

**Schlüsselwörter** (92 von 100 Zeichen, durch Kommas getrennt, ohne Leerzeichen; Wörter aus Name und Untertitel zählt Apple ohnehin)

```
zombie,shooter,pixel,retro,twin-stick,arcade,horde,offline,boss,action,top-down,rätsel,perks
```

**Beschreibung** (1612 von 4000 Zeichen)

```
Die Horde kommt. Du hast zwei Daumen und eine Pistole.

RushOps ist ein Top-down-Shooter im Pixelstil: Mit dem linken Stick läufst du, mit dem rechten zielst du, geschossen wird von selbst. Ein Dash bringt dich mit kurzer Unverwundbarkeit aus der Umzingelung. Die Zielhilfe lässt sich in den Einstellungen abschalten.

DREI INSELN, NEUN LEVEL
Vom sonnigen Strand über die düstere Stadt bis zum Vulkan. Jedes Level ist in Zonen geteilt, und jedes Tor will etwas von dir: genug Kills, eine gehaltene Zone, Schalter gegen die Uhr oder Energiezellen für den Generator. Und das alles, während die Wellen nicht aufhören.

VIER ZOMBIES, DREI BOSSE
Walker schlurfen, Runner rennen, Brutes stecken viel ein, und Boomer explodieren, wenn sie dir zu nahe kommen. Am Ende jeder Insel wartet ein Boss: der Koloss stürmt, die Brutmutter gebiert Runner am laufenden Band, und der Mega-Boomer hinterlässt Pfützen und teilt sich.

PERKS UND WAFFEN
Gegner lassen Perks fallen: Doppelschaden, Insta-Kill, Schnellfeuer, Schutzschild, Speed-Boost, Medkit oder die Nuke, die alle sichtbaren Zombies erledigt. Jeder Bosssieg schaltet eine neue Waffe frei: Schrotflinte, Sturmgewehr oder Dual-Pistolen. Im Level hängen weitere Waffen an der Wand, und mit einem Tipp wechselst du zwischen zwei Waffen.

PUNKTE, STERNE UND SKINS
Kills hintereinander treiben den Multiplikator hoch. Pro Level gibt es bis zu drei Sterne, und Inseln, Sterne und Kills schalten Skins für deine Figur frei.

FAIR UND OFFLINE
Keine Werbung, keine In-App-Käufe, kein Konto, kein Internet nötig. RushOps erfasst keine Daten.

Sprache: Deutsch. Nur im Hochformat.
```

## URLs (GitHub Pages, R4)

| Feld | URL |
|---|---|
| Support-URL | `https://beratkrhn.github.io/RushOps/support.html` |
| Datenschutzrichtlinie | `https://beratkrhn.github.io/RushOps/datenschutz.html` |
| Marketing-URL | leer lassen (optional) |

Die Seiten liegen unter [`site/`](../../site/) und gehen über den Workflow `.github/workflows/pages.yml` online. Vorher müssen die Platzhalter in `datenschutz.html` und `support.html` ausgefüllt sein (siehe „Offen“).

## App-Datenschutz („Datenschutz-Etikett“)

Antwort in App Store Connect: **„Nein, wir erfassen keine Daten von dieser App.“**

Begründung:
- RushOps ist vollständig offline (F-PLT-03). Es gibt keine Server, Konten, Werbung, Analyse- oder Absturzdienste.
- Der Spielstand liegt nur als JSON-Datei im App-Speicher auf dem Gerät (F-DAT-01). Er verlässt das Gerät nur über das iCloud- oder iTunes-Backup des Nutzers, und das zählt bei Apple nicht als Datenerfassung durch die App.
- Unity schickt nichts: Hardware-Statistik (`submitAnalytics`) und Engine Diagnostics sind aus, Unity Services sind nicht aktiv. Der EditMode-Test `PrivacySettingsTests` hält das fest.
- Kein Tracking, also keine App-Tracking-Transparency-Abfrage.

## Altersfreigabe (Fragebogen in App Store Connect)

| Frage | Antwort | Begründung |
|---|---|---|
| Cartoon- oder Fantasy-Gewalt | **Häufig** | Ständiges Schießen auf Zombies; Pixelfiguren in Comic-Optik |
| Realistische Gewalt | Keine | Keine realistischen Menschen, kein Blut, keine Verstümmelung |
| Ausgedehnte grafische oder sadistische realistische Gewalt | Keine | |
| Horror- oder Angstthemen | **Selten/schwach** | Zombies als Gegner, aber hell und comichaft |
| Obszöne oder vulgäre Sprache | Keine | |
| Sexuelle Inhalte, Nacktheit | Keine | |
| Alkohol, Tabak, Drogen | Keine | |
| Simuliertes Glücksspiel | Keine | Perk-Drops sind keine Glücksspiele: kein Einsatz, kein Kauf |
| Wettbewerbe, Glücksspiel um Geld | Keine | |
| Medizinische Informationen | Keine | Der Perk „Medkit“ ist nur ein Spielgegenstand |
| Nutzergenerierte Inhalte, Chat, Web-Zugriff | Nein | |
| In-App-Käufe, Werbung | Nein | |

**Erwartete Einstufung: 9+.** Häufige Cartoon- oder Fantasy-Gewalt hebt die Einstufung über 4+. Was Apple am Ende vergibt, zeigt App Store Connect nach dem Fragebogen.

## Exportkontrolle (Verschlüsselung)

RushOps nutzt keine eigene Verschlüsselung und keine Netzwerkverbindungen; das Spiel verwendet nur, was iOS selbst mitbringt. Damit gilt die Ausnahme, und es sind keine Unterlagen nötig. Story 6 (#194) setzt dafür `ITSAppUsesNonExemptEncryption = false` in der `Info.plist`, sodass App Store Connect beim Upload nicht nachfragt.

## Verfügbarkeit (entscheidest du)

| Möglichkeit | Für | Gegen |
|---|---|---|
| Alle Länder | Mehr Reichweite, kostet nichts | Spiel und Store-Text nur auf Deutsch, vermutlich schlechte Bewertungen außerhalb des Sprachraums |
| Deutschland, Österreich, Schweiz, Liechtenstein, Luxemburg | Passt zur Sprache | Weniger Reichweite |

Mein Vorschlag sind die deutschsprachigen Länder. Später lässt sich das jederzeit erweitern, ohne neue Prüfung.

## Screenshots und Icon

- Screenshots: [`docs/store/screenshots/`](screenshots/) (iPhone 6,9 Zoll, 1320×2868; Issue #192). Vor dem Hochladen #201 erledigen und neu aufnehmen.
- Icon: Das 1024er-Icon kommt aus dem Build (`Assets/_Project/Art/Icon/AppIcon.png`; Issue #191).

## Offen

- Copyright-Name im Store-Eintrag
- Verantwortlicher und Kontakt in `site/datenschutz.html` und `site/support.html` (Platzhalter `[…]`). Die DSGVO verlangt Name und Anschrift des Verantwortlichen.
- Danach GitHub Pages einschalten (Settings → Pages → Source: GitHub Actions) und den Workflow „Pages“ einmal starten
- Verfügbarkeit
