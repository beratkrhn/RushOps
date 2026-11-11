namespace RushOps.Feel
{
    /// <summary>What a vibration pulse stands for (issue #300, F-AUD-05, N32).</summary>
    public enum HapticKind
    {
        /// <summary>The player lost a heart: a short, strong pulse.</summary>
        Damage,

        /// <summary>A boomer, rocket or nuke went off on screen: a medium pulse.</summary>
        Explosion,
    }
}
