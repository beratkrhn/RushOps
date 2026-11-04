using System;

namespace RushOps.Save
{
    /// <summary>Statistics across all games in the save game (F-DAT-04, design question M6).</summary>
    [Serializable]
    public class StatsData
    {
        /// <summary>Enemies killed in total.</summary>
        public int TotalKills;

        /// <summary>Times the player died.</summary>
        public int Deaths;

        /// <summary>Levels finished, counting repeats.</summary>
        public int LevelsCompleted;

        /// <summary>Highest combo multiplier reached.</summary>
        public int BestCombo;

        /// <summary>Time spent in levels, in seconds.</summary>
        public float PlayTimeSeconds;
    }
}
