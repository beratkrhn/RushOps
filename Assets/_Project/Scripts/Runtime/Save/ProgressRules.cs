using System;

namespace RushOps.Save
{
    /// <summary>
    /// Rules for the level records in the save game (F-DAT-04, F-SCO-03): finishing a level marks it as done and keeps
    /// the best stars and the best score; a worse result never lowers them.
    /// </summary>
    public static class ProgressRules
    {
        /// <summary>The record of <paramref name="levelId"/>, or null if the level was never finished.</summary>
        public static LevelRecord Find(ProgressData progress, string levelId)
        {
            if (progress?.Levels == null || string.IsNullOrEmpty(levelId))
            {
                return null;
            }

            foreach (LevelRecord record in progress.Levels)
            {
                if (record != null && record.LevelId == levelId)
                {
                    return record;
                }
            }

            return null;
        }

        /// <summary>True if <paramref name="levelId"/> was finished at least once.</summary>
        public static bool IsCompleted(ProgressData progress, string levelId)
        {
            LevelRecord record = Find(progress, levelId);
            return record != null && record.Completed;
        }

        /// <summary>Stars reached in <paramref name="levelId"/>, 0 if never finished.</summary>
        public static int StarsOf(ProgressData progress, string levelId)
        {
            LevelRecord record = Find(progress, levelId);
            return record != null && record.Completed ? record.Stars : 0;
        }

        /// <summary>Best stars of all finished levels added up (milestone skins, issue #128).</summary>
        public static int TotalStars(ProgressData progress)
        {
            if (progress?.Levels == null)
            {
                return 0;
            }

            int total = 0;
            foreach (LevelRecord record in progress.Levels)
            {
                if (record != null && record.Completed)
                {
                    total += record.Stars;
                }
            }

            return total;
        }

        /// <summary>
        /// Marks <paramref name="levelId"/> as finished with <paramref name="stars"/> and <paramref name="score"/>,
        /// keeping better earlier results.
        /// </summary>
        /// <returns>The level's record.</returns>
        public static LevelRecord RecordCompletion(ProgressData progress, string levelId, int stars, int score)
        {
            if (progress == null)
            {
                throw new ArgumentNullException(nameof(progress));
            }

            if (string.IsNullOrEmpty(levelId))
            {
                throw new ArgumentException("A level needs an id.", nameof(levelId));
            }

            LevelRecord record = Find(progress, levelId);
            if (record == null)
            {
                record = new LevelRecord { LevelId = levelId };
                progress.Levels.Add(record);
            }

            record.Completed = true;
            record.Stars = Math.Max(record.Stars, Math.Max(0, stars));
            record.BestScore = Math.Max(record.BestScore, Math.Max(0, score));
            return record;
        }
    }
}
