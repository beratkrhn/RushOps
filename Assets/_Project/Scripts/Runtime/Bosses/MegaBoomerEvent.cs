namespace RushOps.Bosses
{
    /// <summary>What happens in one step of <see cref="MegaBoomerLogic"/>.</summary>
    public enum MegaBoomerEvent
    {
        /// <summary>Nothing new.</summary>
        None,

        /// <summary>A puddle is left at the feet.</summary>
        Puddle,

        /// <summary>The split starts: the body stops and tears open.</summary>
        SplitStarted,

        /// <summary>The split is over: the halves take over.</summary>
        Split,
    }
}
