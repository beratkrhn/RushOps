using System;
using System.Collections.Generic;
using System.Globalization;

namespace RushOps.Save
{
    /// <summary>
    /// Rules of the daily best scores (issue #276, F-END-14, decisions T3 and N22): any number of tries per day, the best
    /// counts by score, then by round; on a tie the earlier run stays. Only the last <see cref="MaxDays"/> days are kept,
    /// counted back from the newest day. A daily run never touches the endless top 10 (<see cref="EndlessRecords"/>).
    /// </summary>
    public static class DailyRecords
    {
        /// <summary>Days kept (F-END-14: the last 7 days).</summary>
        public const int MaxDays = 7;

        private const string DateFormat = "yyyy-MM-dd";

        /// <summary>
        /// Counts the try of <paramref name="date"/>, keeps the run if it is the best of the day and drops days older than
        /// the last <see cref="MaxDays"/>.
        /// </summary>
        /// <param name="date">Day of the run as <c>yyyy-MM-dd</c>.</param>
        /// <exception cref="ArgumentException">If <paramref name="date"/> is not <c>yyyy-MM-dd</c>.</exception>
        public static DailyRecord Record(DailyData data, int score, int round, string date)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (!TryParse(date, out DateTime day))
            {
                throw new ArgumentException($"Not a yyyy-MM-dd date: \"{date}\".", nameof(date));
            }

            score = Math.Max(0, score);
            round = Math.Max(0, round);
            data.Days ??= new List<DailyScore>();

            DailyScore entry = Find(data, date);
            bool isNewBest;
            if (entry == null)
            {
                entry = new DailyScore(date, score, round, 1);
                data.Days.Add(entry);
                isNewBest = true;
            }
            else
            {
                entry.Tries++;
                isNewBest = IsBetter(score, round, entry);
                if (isNewBest)
                {
                    entry.Score = score;
                    entry.Round = round;
                }
            }

            Prune(data, day);
            return new DailyRecord(date, score, round, entry.Score, entry.Round, entry.Tries, isNewBest);
        }

        /// <summary>The best run of <paramref name="date"/>, or null without a daily run that day.</summary>
        public static DailyScore Find(DailyData data, string date)
        {
            if (data?.Days == null)
            {
                return null;
            }

            foreach (DailyScore entry in data.Days)
            {
                if (entry != null && string.Equals(entry.Date, date, StringComparison.Ordinal))
                {
                    return entry;
                }
            }

            return null;
        }

        /// <summary>
        /// The best runs of the last <see cref="MaxDays"/> days up to <paramref name="today"/> (<c>yyyy-MM-dd</c>), newest
        /// first; days without a daily run are left out. Allocates; for the endless area.
        /// </summary>
        public static List<DailyScore> LastDays(DailyData data, string today)
        {
            var days = new List<DailyScore>();
            if (data?.Days == null || !TryParse(today, out DateTime end))
            {
                return days;
            }

            DateTime start = end.AddDays(-(MaxDays - 1));
            foreach (DailyScore entry in data.Days)
            {
                if (entry != null && TryParse(entry.Date, out DateTime day) && day >= start && day <= end)
                {
                    days.Add(entry);
                }
            }

            days.Sort((x, y) => string.CompareOrdinal(y.Date, x.Date));
            return days;
        }

        /// <summary>The best score of <paramref name="date"/>; 0 without a daily run that day.</summary>
        public static int BestScore(DailyData data, string date)
        {
            return Find(data, date)?.Score ?? 0;
        }

        // Newest first; drops broken entries and days before the window that ends at the newest day.
        private static void Prune(DailyData data, DateTime recorded)
        {
            DateTime newest = recorded;
            foreach (DailyScore entry in data.Days)
            {
                if (entry != null && TryParse(entry.Date, out DateTime day) && day > newest)
                {
                    newest = day;
                }
            }

            DateTime oldest = newest.AddDays(-(MaxDays - 1));
            data.Days.RemoveAll(e => e == null || !TryParse(e.Date, out DateTime day) || day < oldest);
            // yyyy-MM-dd sorts like the date itself.
            data.Days.Sort((a, b) => string.CompareOrdinal(b.Date, a.Date));
        }

        private static bool IsBetter(int score, int round, DailyScore best)
        {
            return score != best.Score ? score > best.Score : round > best.Round;
        }

        private static bool TryParse(string date, out DateTime day)
        {
            return DateTime.TryParseExact(date, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out day);
        }
    }
}
