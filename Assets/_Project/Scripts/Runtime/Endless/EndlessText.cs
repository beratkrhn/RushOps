using System.Collections.Generic;
using System.Globalization;
using System.Text;
using RushOps.Save;
using RushOps.UI;

namespace RushOps.Endless
{
    /// <summary>Texts of the endless mode (F-END-03, F-END-08, F-HUD-03).</summary>
    public static class EndlessText
    {
        /// <summary>Title of the result screen at the end of a run.</summary>
        public const string RunOver = "Lauf vorbei";

        /// <summary>Line on the result screen when the run beat the best score (issue #211).</summary>
        public const string NewBest = "Neuer Bestwert!";

        /// <summary>Heading of the top list on the result screen.</summary>
        public const string TopTitle = "Top 10";

        /// <summary>Heading of the top list after an arcade run, which has its own list (F-ARC-08).</summary>
        public const string ArcadeTopTitle = "Top 10 Arcade";

        /// <summary>Caption of the switch between Normal and Arcade in the endless area (F-ARC-08).</summary>
        public const string DisplayTitle = "Darstellung";

        /// <summary>Label of the display switch: „Arcade“ or „Normal“.</summary>
        public static string Display(bool arcade)
        {
            return arcade ? "Arcade" : "Normal";
        }

        /// <summary>Column headings of the top list: place, score, round, date.</summary>
        public static readonly string[] TopHeadings = { "Platz", "Score", "Runde", "Datum" };

        /// <summary>The run's place on the result screen, e.g. „Platz 3 der Top 10“ or „Nicht in den Top 10“.</summary>
        public static string Rank(int rank)
        {
            return rank > 0 ? $"Platz {rank.ToString(CultureInfo.InvariantCulture)} der Top 10" : "Nicht in den Top 10";
        }

        /// <summary>Place column of the top list, e.g. „3.“.</summary>
        public static string Place(int rank)
        {
            return rank.ToString(CultureInfo.InvariantCulture) + ".";
        }

        /// <summary>
        /// One column of the top list as lines: the heading of <see cref="TopHeadings"/>, then one line per entry. The
        /// line of place <paramref name="highlightRank"/> is coloured <paramref name="highlightHex"/> (rich text, e.g.
        /// <c>#f6d04d</c>); 0 highlights nothing.
        /// </summary>
        /// <param name="column">0 place, 1 score, 2 round, 3 date.</param>
        public static string TopColumn(IReadOnlyList<EndlessScore> top, int column, int highlightRank, string highlightHex)
        {
            var text = new StringBuilder(TopHeadings[column]);
            int count = top != null ? top.Count : 0;
            for (int i = 0; i < count; i++)
            {
                EndlessScore entry = top[i];
                string cell = column switch
                {
                    0 => Place(i + 1),
                    1 => ScoreText.Score(entry != null ? entry.Score : 0),
                    2 => (entry != null ? entry.Round : 0).ToString(CultureInfo.InvariantCulture),
                    _ => Date(entry?.Date),
                };
                text.Append('\n');
                if (i + 1 == highlightRank && !string.IsNullOrEmpty(highlightHex))
                {
                    text.Append("<color=").Append(highlightHex).Append('>').Append(cell).Append("</color>");
                }
                else
                {
                    text.Append(cell);
                }
            }

            return text.ToString();
        }

        /// <summary>Date column of the top list: <c>yyyy-MM-dd</c> as „07.11.26“; other text unchanged.</summary>
        public static string Date(string isoDate)
        {
            if (string.IsNullOrEmpty(isoDate) || isoDate.Length != 10 || isoDate[4] != '-' || isoDate[7] != '-')
            {
                return isoDate ?? string.Empty;
            }

            return $"{isoDate.Substring(8, 2)}.{isoDate.Substring(5, 2)}.{isoDate.Substring(2, 2)}";
        }

        /// <summary>The large display before a round and the line on the result screen, e.g. „Runde 3“.</summary>
        public static string Round(int round)
        {
            return $"Runde {round}";
        }

        /// <summary>Line below „Runde X“ in the break before a dog round (F-END-10).</summary>
        public const string DogRound = "Hunderunde!";

        /// <summary>Notice in the mission tracker when a wall weapon unlocks, e.g. „Neue Waffe: Schrotflinte“.</summary>
        public static string NewWeapon(string weaponName)
        {
            return $"Neue Waffe: {weaponName}";
        }

        /// <summary>The tracker as above with the notice <paramref name="notice"/> as a third line, if there is one.</summary>
        public static string Tracker(int round, int remaining, string notice)
        {
            return string.IsNullOrEmpty(notice) ? Tracker(round, remaining) : $"{Tracker(round, remaining)}\n{notice}";
        }

        /// <summary>Mission tracker during a round: the round and the enemies still to beat, e.g. „Runde 3\nNoch 8 Gegner“.</summary>
        public static string Tracker(int round, int remaining)
        {
            return remaining == 1
                ? $"Runde {round}\nNoch 1 Gegner"
                : $"Runde {round}\nNoch {remaining} Gegner";
        }

        /// <summary>
        /// Mission tracker during a dog round: the round and the dogs still to beat, e.g. „Runde 6\nNoch 9 Hunde“, with
        /// the notice <paramref name="notice"/> as a third line, if there is one.
        /// </summary>
        public static string DogTracker(int round, int remaining, string notice = null)
        {
            string text = remaining == 1
                ? $"Runde {round}\nNoch 1 Hund"
                : $"Runde {round}\nNoch {remaining} Hunde";
            return string.IsNullOrEmpty(notice) ? text : $"{text}\n{notice}";
        }

        /// <summary>
        /// Mission tracker during a boss round: „Runde X“ and „Boss besiegen“; a notice takes the second line while it
        /// shows, so the text keeps two lines and clears the boss health bar below it.
        /// </summary>
        public static string BossTracker(int round, string notice)
        {
            return string.IsNullOrEmpty(notice) ? $"Runde {round}\nBoss besiegen" : $"Runde {round}\n{notice}";
        }

        /// <summary>Title over the upgrade cards (F-END-11).</summary>
        public const string ChooseUpgrade = "Wähle ein Upgrade";

        /// <summary>Title of the upgrade list in the pause overlay.</summary>
        public const string UpgradesTitle = "Upgrades";

        /// <summary>Shown in the pause overlay before the first upgrade of the run.</summary>
        public const string NoUpgrades = "Noch keine Upgrades";

        /// <summary>Level line of an upgrade card or list entry, e.g. „Stufe 2/3“.</summary>
        public static string UpgradeLevel(int level, int maxLevel)
        {
            return $"Stufe {level}/{maxLevel}";
        }

        /// <summary>
        /// The upgrades taken in the run, one per line with their level („Abpraller 2/3“), in the order of the config;
        /// <see cref="NoUpgrades"/> without any. Allocates; call when the pause overlay opens, not per frame.
        /// </summary>
        public static string UpgradeList(RunUpgrades upgrades)
        {
            var text = new System.Text.StringBuilder();
            if (upgrades != null)
            {
                foreach (UpgradeDefinition upgrade in upgrades.Definitions)
                {
                    int level = upgrades.Level(upgrade);
                    if (level <= 0)
                    {
                        continue;
                    }

                    if (text.Length > 0)
                    {
                        text.Append('\n');
                    }

                    text.Append(upgrade.DisplayName).Append(' ').Append(level).Append('/').Append(upgrade.MaxLevel);
                }
            }

            return text.Length > 0 ? text.ToString() : NoUpgrades;
        }

        /// <summary>Name of the daily run (F-END-13, N22).</summary>
        public const string Daily = "Tageslauf";

        /// <summary>Shown on the result screen when a daily run beat the best of its day.</summary>
        public const string NewDailyBest = "Neuer Tagesbestwert!";

        /// <summary>Title of the list of the last days in the endless area.</summary>
        public const string LastDaysTitle = "Letzte 7 Tage";

        /// <summary>Column headings of the list of the last days.</summary>
        public static readonly string[] DailyHeadings = { "Datum", "Score", "Runde" };

        /// <summary>
        /// Line under „Runde X“ in the break: „Tageslauf“ with the date in a daily run, „Hunderunde!“ before a dog round,
        /// both on two lines; null for a normal round of a normal run.
        /// </summary>
        public static string BannerSubtitle(bool dogRound, string dailyDate)
        {
            string daily = string.IsNullOrEmpty(dailyDate) ? null : $"{Daily} {Date(dailyDate)}";
            if (daily == null)
            {
                return dogRound ? DogRound : null;
            }

            return dogRound ? daily + "\n" + DogRound : daily;
        }

        /// <summary>Line with the best of the day on the result screen, e.g. „Tagesbestwert: 1200“.</summary>
        public static string DailyBest(int score)
        {
            return $"Tagesbestwert: {score}";
        }

        /// <summary>Today's best in the endless area, e.g. „Heute: 1200, Runde 7“, or that there is none yet.</summary>
        public static string DailyToday(DailyScore today)
        {
            return today != null ? $"Heute: {today.Score}, Runde {today.Round}" : "Heute noch kein Tageslauf";
        }

        /// <summary>
        /// One column (0 date, 1 score, 2 round) of the list of the last days, newest first, one line per day; a dash
        /// without any day. Allocates; call when the endless area opens.
        /// </summary>
        public static string DailyColumn(IReadOnlyList<DailyScore> days, int column)
        {
            if (days == null || days.Count == 0)
            {
                return column == 0 ? "-" : string.Empty;
            }

            var text = new StringBuilder();
            for (int i = 0; i < days.Count; i++)
            {
                if (i > 0)
                {
                    text.Append('\n');
                }

                DailyScore day = days[i];
                text.Append(column switch
                {
                    0 => Date(day.Date),
                    1 => day.Score.ToString(CultureInfo.InvariantCulture),
                    _ => day.Round.ToString(CultureInfo.InvariantCulture),
                });
            }

            return text.ToString();
        }
    }
}
