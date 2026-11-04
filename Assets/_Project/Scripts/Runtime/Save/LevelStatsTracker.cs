using System;

namespace RushOps.Save
{
    /// <summary>
    /// Moves the statistics of a running level into the save game (design question M6). A level may be committed
    /// several times (app goes to the background, then the level ends); every commit adds only what happened since
    /// the last one, so nothing is counted twice.
    /// </summary>
    public sealed class LevelStatsTracker
    {
        private int _committedKills;
        private float _committedTime;

        /// <summary>
        /// Adds the kills and time since the last commit to <paramref name="stats"/> and raises the best combo.
        /// </summary>
        /// <param name="levelKills">Kills since level start.</param>
        /// <param name="levelTime">Seconds played since level start.</param>
        /// <param name="levelBestCombo">Highest combo multiplier since level start.</param>
        /// <returns>True if <paramref name="stats"/> changed.</returns>
        public bool CommitTo(StatsData stats, int levelKills, float levelTime, int levelBestCombo)
        {
            if (stats == null)
            {
                throw new ArgumentNullException(nameof(stats));
            }

            int kills = Math.Max(0, levelKills - _committedKills);
            float time = Math.Max(0f, levelTime - _committedTime);
            _committedKills = Math.Max(_committedKills, levelKills);
            _committedTime = Math.Max(_committedTime, levelTime);

            stats.TotalKills += kills;
            stats.PlayTimeSeconds += time;
            bool betterCombo = levelBestCombo > stats.BestCombo;
            if (betterCombo)
            {
                stats.BestCombo = levelBestCombo;
            }

            return kills > 0 || time > 0f || betterCombo;
        }
    }
}
