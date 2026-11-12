# Story von RushOps

Diese Datei hält die Geschichte der Kampagne fest, damit Cutscenes, Level, Dialoge und Texte zusammenpassen. Sie ist die Grundlage für Cutscene-Skripte und Level-Ideen. Entscheidungen stehen in [`backlog/design-fragen.md`](backlog/design-fragen.md) (S1 bis S8). Stand: 12.11.2026, **Entwurf**; Namen sind Arbeitstitel.

**Ton:** düster und ernst. Keine Gags, auch wenn die Welt satirische Züge hat (Gier, Hype, Gerüchte).

## Kurzfassung

Ein mächtiger, einflussreicher Mann setzt ein Gerücht in die Welt: Der **Toke**, ein extrem seltener Token, bringt seinem Besitzer alles, was er will: Geld, Kleidung, Essen, Besitz. Die Menschen glauben ihm und suchen überall, in Truhen, Wasserfällen, auf Inseln und Schiffen, vergeblich. Sie wissen nicht, dass der Toke in Wirklichkeit ein Gift ist, das sie in Echtzeit von innen zerstört. Nur der Held hört auf zu suchen und warnt die anderen, doch niemand hört auf ihn. Am Ende sind alle tot und zu **Zombies** geworden. Der Mann hat sein Ziel erreicht und weiß, dass nur noch der Held lebt.

Den Toke gibt es wirklich, und nur der Mann hat ihn. Er ist zugleich **das Gift und das Heilmittel**. Der Held macht sich auf den Weg zum Mann, um den Toke zu bekommen und die Menschen zu retten.

## Figuren

| Figur | Rolle | Stand |
|---|---|---|
| **Held** | Die Spielfigur. Hat aufgehört, den Toke zu suchen, und hat die anderen gewarnt. Sein Antrieb: Er weiß jetzt sicher, dass es den Toke gibt und dass er heilt; er will die Menschen retten. | Name offen |
| **Der Mann** | Einflussreich, mächtig, hat das Gerücht gestreut und besitzt den Toke. Endgegner. | Name offen; Motiv offen (S4) |
| **Die Menschen** | Suchten den Toke, starben daran und sind jetzt die Zombies. Dass Zombies Gold fallen lassen, passt dazu: Sie tragen ihre Besitztümer noch bei sich. | festgelegt |
| **Die drei Bosse** | Koloss, Brutmutter und Mega-Boomer (Lastenheft 2.4) | Zuordnung siehe unten (Vorschlag) |

## Der Toke

- Ein extrem seltener Token.
- Wirkt als **Gift** in Echtzeit von innen und tötet.
- Ist zugleich das **Heilmittel**; nur der Mann hat ihn (S2).
- Der Mann selbst bleibt unversehrt; wie, ist offen (S4).

## Verlauf in Akten (Vorschlag)

Die drei Inseln sind die drei Akte; Themen aus Design-Frage G6.

| Akt | Insel | Boss | Erzählidee (Vorschlag, nicht entschieden) |
|---|---|---|---|
| 1 | Strand | Koloss | Hier begann die Jagd am Wasser, an Schiffen und Wracks. Der Boss war einer, der das Gerücht dort verbreitet hat. |
| 2 | Düstere Stadt | Brutmutter | Die Stadt ist leer gesucht, überall aufgebrochene Truhen und Safes. Der Boss war der Händler, der vom Gerücht lebte. |
| 3 | Vulkan | Mega-Boomer | Der Weg zum Mann führt durch den Vulkan. Der Boss bewacht den Zugang. |
| Finale | Sitz des Mannes | der Mann | Wahrheit über den Toke; Ende offen (S5). |

Die drei Bosse könnten **Handlanger des Mannes** gewesen sein, die selbst vom Toke genommen haben und zu Monstern wurden. Das verbindet die Spielfiguren mit der Geschichte, ist aber ein Vorschlag (S6).

## Intro-Cutscene (Storyboard, 12.11.2026)

Sieben Standbilder in Pixelart mit Textboxen, im Stil des Spiels (Spieler-Sprite, Walker-Zombies, Projektpalette, Pixelschrift). Die Bilder liegen unter [`art/cutscene-intro/`](art/cutscene-intro/) (Übersicht: `storyboard_sheet.png`, Skript: `gen_intro_storyboard.py`) und sind noch Entwürfe; Hintergründe und der Mann werden später fein ausgearbeitet.

| Bild | Inhalt | Text |
|---|---|---|
| 1 | Der Mann auf einer riesigen Leinwand, in der Hand der goldene Toke; unten die Menge | „Es begann mit einem Satz.“ Der Mann: „Der Toke. Wer ihn besitzt, hat alles, was er sich wünscht: Gold, Kleidung, Essen, Besitz.“ „Den Toke trug er längst selbst in der Hand.“ |
| 2 | Die Suche in Truhen, an Wasserfällen, auf Inseln und Schiffen; alle gesund | „Alle machten sich auf die Suche …“ „Tag 1. Alle gesund.“ |
| 3 | Erste Kranke mit grünen Adern, die weitersuchen | „Wochen später. Die Ersten wurden krank. …“ „Sie suchten trotzdem weiter.“ |
| 4 | Der Held ruft der Menge zu | Held: „Retired die Suche nach dem Toke!“ Menge: „Geh weg. Du bist nur neidisch.“ |
| 5 | Die Menge verwandelt sich Stück für Stück in Zombies | „Einer nach dem anderen. Tag für Tag.“ … „Wo sie zusammenbrachen, standen sie als Zombies wieder auf.“ |
| 6 | Der Mann im Fenster, der Toke golden, unten die Zombies, der Held ganz klein | Der Mann: „Der Toke vergiftet jeden, der ihn sucht. Und nur er kann sie heilen. Ich habe ihn.“ |
| 7 | Showdown: **oben der Held**, **unten der Mann mit Teufelsflügeln** und dem goldenen Toke | Held: „assalamu aleykum wa rahmetullahi wa barakatuhu“. Der Mann: „wa aleykum ya ars“ |

Regeln für die Bilder: Der **Toke leuchtet immer golden**; der Mann hat erst im letzten Bild Flügel und Hörner; das Gesicht des Mannes bleibt bis zum Finale unsichtbar.

## Cutscenes (Plan, Details später)

| Wann | Inhalt |
|---|---|
| Intro | Das Gerücht, die Jagd, die Warnungen des Helden, das Sterben, der Held allein |
| Nach Boss 1 bis 3 | Kurze Szene mit einem Hinweis zum Toke bzw. zum Mann |
| Finale | Der Mann, die Wahrheit, das Ende |

Der Stil steht fest: Pixelart-Standbilder mit Textboxen (S7); Kamerafahrten und kleine Effekte kommen beim Zerlegen dazu.

## Offene Punkte

- Name und Aussehen des Helden und des Mannes
- Warum tötet der Mann alle (S4)? Wie bleibt er selbst unversehrt?
- Wie endet die Geschichte (S5)? Gelingt die Rettung, ganz oder teilweise?
- Zuordnung der Bosse zur Geschichte (S6)
- Wie viel Bewegung die Cutscenes bekommen (Kamerafahrt, Flackern, Wackeln)
