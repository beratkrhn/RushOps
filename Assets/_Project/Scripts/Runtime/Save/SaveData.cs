using System;

namespace RushOps.Save
{
    /// <summary>
    /// The whole save game (F-DAT-04, ADR 0005): meta progress, statistics and settings, written as JSON by
    /// <see cref="SaveGameStore"/>. Plain data only; the rules that change it live elsewhere. New fields may be added
    /// without a new <see cref="Version"/> as long as their default is right for older saves; renaming or reshaping
    /// fields needs a version step and an <see cref="ISaveMigration"/>.
    /// </summary>
    /// <remarks>Public fields because <c>JsonUtility</c> serializes fields, and their names are the JSON keys.</remarks>
    [Serializable]
    public class SaveData
    {
        /// <summary>Format version written by this build.</summary>
        public const int CurrentVersion = 7;

        /// <summary>Format version of this save.</summary>
        public int Version = CurrentVersion;

        /// <summary>Finished levels with stars and best scores.</summary>
        public ProgressData Progress = new ProgressData();

        /// <summary>Statistics across all games.</summary>
        public StatsData Stats = new StatsData();

        /// <summary>The player's settings (issue #126).</summary>
        public SettingsData Settings = new SettingsData();

        /// <summary>The chosen and the unlocked skins (issue #128).</summary>
        public SkinsData Skins = new SkinsData();

        /// <summary>The chosen and the unlocked weapons (issue #176, version 2).</summary>
        public WeaponsData Weapons = new WeaponsData();

        /// <summary>Best scores of the endless mode (issue #211, version 3).</summary>
        public EndlessData Endless = new EndlessData();

        /// <summary>The gold account (issue #236, version 4).</summary>
        public GoldData Gold = new GoldData();

        /// <summary>Best daily runs of the last days (issue #276, version 5).</summary>
        public DailyData Daily = new DailyData();

        /// <summary>Puzzle hints already shown (issue #287, version 6).</summary>
        public HintsData Hints = new HintsData();

        /// <summary>Story progress beyond the levels, e.g. the intro (issue #333, no version step).</summary>
        public StoryData Story = new StoryData();

        /// <summary>Display choice and own lists of the arcade mode (F-ARC-08, since version 7).</summary>
        public ArcadeData Arcade = new ArcadeData();
    }
}
