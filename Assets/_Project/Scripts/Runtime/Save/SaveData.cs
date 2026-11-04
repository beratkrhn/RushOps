using System;

namespace RushOps.Save
{
    /// <summary>
    /// The whole save game (F-DAT-04, ADR 0005): meta progress and statistics, written as JSON by
    /// <see cref="SaveGameStore"/>. Plain data only; the rules that change it live elsewhere. New fields may be added
    /// without a new <see cref="Version"/> as long as their default is right for older saves; renaming or reshaping
    /// fields needs a version step and an <see cref="ISaveMigration"/>.
    /// </summary>
    /// <remarks>Public fields because <c>JsonUtility</c> serializes fields, and their names are the JSON keys.</remarks>
    [Serializable]
    public class SaveData
    {
        /// <summary>Format version written by this build.</summary>
        public const int CurrentVersion = 1;

        /// <summary>Format version of this save.</summary>
        public int Version = CurrentVersion;

        /// <summary>Finished levels with stars and best scores.</summary>
        public ProgressData Progress = new ProgressData();

        /// <summary>Statistics across all games.</summary>
        public StatsData Stats = new StatsData();
    }
}
