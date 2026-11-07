using System;
using System.Collections.Generic;

namespace RushOps.Save
{
    /// <summary>
    /// Rules of the endless best scores (issue #211, F-END-08, decision E8): every finished run counts, the top list keeps
    /// the <see cref="MaxEntries"/> best runs by score, then by round; on a tie the older run stays ahead.
    /// </summary>
    public static class EndlessRecords
    {
        /// <summary>Length of the top list (F-END-08: local top 10).</summary>
        public const int MaxEntries = 10;

        /// <summary>
        /// Counts the run, raises the highest round and sorts the run into the top list of <paramref name="data"/>.
        /// </summary>
        /// <param name="date">Day of the run as <c>yyyy-MM-dd</c>.</param>
        /// <returns>The place of the run and whether it is a new best score.</returns>
        public static EndlessRecord Record(EndlessData data, int score, int round, string date)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            score = Math.Max(0, score);
            round = Math.Max(0, round);
            data.Top ??= new List<EndlessScore>();
            data.Runs++;
            data.HighestRound = Math.Max(data.HighestRound, round);

            int best = data.Top.Count > 0 ? data.Top[0].Score : -1;
            int index = data.Top.Count;
            for (int i = 0; i < data.Top.Count; i++)
            {
                if (IsBetter(score, round, data.Top[i]))
                {
                    index = i;
                    break;
                }
            }

            bool isNewBest = score > 0 && score > best;
            if (index >= MaxEntries)
            {
                return new EndlessRecord(score, round, 0, isNewBest);
            }

            data.Top.Insert(index, new EndlessScore(score, round, date));
            if (data.Top.Count > MaxEntries)
            {
                data.Top.RemoveRange(MaxEntries, data.Top.Count - MaxEntries);
            }

            return new EndlessRecord(score, round, index + 1, isNewBest);
        }

        /// <summary>The best score of all runs; 0 without a run.</summary>
        public static int BestScore(EndlessData data)
        {
            return data?.Top != null && data.Top.Count > 0 ? data.Top[0].Score : 0;
        }

        // Strictly better only: an equal run goes behind the older ones.
        private static bool IsBetter(int score, int round, EndlessScore other)
        {
            if (other == null)
            {
                return true;
            }

            return score != other.Score ? score > other.Score : round > other.Round;
        }
    }
}
