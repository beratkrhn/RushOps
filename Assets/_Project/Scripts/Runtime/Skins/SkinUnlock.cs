namespace RushOps.Skins
{
    /// <summary>
    /// A skin's id together with its unlock condition, as plain data for <see cref="SkinRules"/> (issue #128).
    /// </summary>
    public readonly struct SkinUnlock
    {
        /// <summary>Creates the condition of skin <paramref name="skinId"/>.</summary>
        public SkinUnlock(string skinId, SkinUnlockKind kind, string levelId = null,
            SkinMilestone milestone = SkinMilestone.TotalKills, int threshold = 0)
        {
            SkinId = skinId;
            Kind = kind;
            LevelId = levelId;
            Milestone = milestone;
            Threshold = threshold;
        }

        /// <summary>Stable id of the skin in the save game.</summary>
        public string SkinId { get; }

        /// <summary>How the skin is unlocked.</summary>
        public SkinUnlockKind Kind { get; }

        /// <summary>Level to finish for <see cref="SkinUnlockKind.LevelCompleted"/>.</summary>
        public string LevelId { get; }

        /// <summary>Statistic counted for <see cref="SkinUnlockKind.Milestone"/>.</summary>
        public SkinMilestone Milestone { get; }

        /// <summary>
        /// Value <see cref="Milestone"/> has to reach; for <see cref="SkinUnlockKind.EndlessRound"/> the round to reach.
        /// </summary>
        public int Threshold { get; }

        /// <summary>A default skin, available from the start.</summary>
        public static SkinUnlock Default(string skinId) => new SkinUnlock(skinId, SkinUnlockKind.Default);

        /// <summary>A skin unlocked by finishing <paramref name="levelId"/>.</summary>
        public static SkinUnlock Level(string skinId, string levelId) =>
            new SkinUnlock(skinId, SkinUnlockKind.LevelCompleted, levelId);

        /// <summary>A skin unlocked once <paramref name="milestone"/> reaches <paramref name="threshold"/>.</summary>
        public static SkinUnlock AtMilestone(string skinId, SkinMilestone milestone, int threshold) =>
            new SkinUnlock(skinId, SkinUnlockKind.Milestone, null, milestone, threshold);

        /// <summary>A skin unlocked once an endless run reaches <paramref name="round"/> (issue #211).</summary>
        public static SkinUnlock AtEndlessRound(string skinId, int round) =>
            new SkinUnlock(skinId, SkinUnlockKind.EndlessRound, null, SkinMilestone.TotalKills, round);
    }
}
