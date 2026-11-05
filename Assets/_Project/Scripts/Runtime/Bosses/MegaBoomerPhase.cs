namespace RushOps.Bosses
{
    /// <summary>What the Mega-Boomer does right now (issue #159).</summary>
    public enum MegaBoomerPhase
    {
        /// <summary>Walks at the player and leaves puddles.</summary>
        Walk,

        /// <summary>Stands still while it tears into two halves; they appear when the phase ends.</summary>
        Split,

        /// <summary>Has split; this body is gone and its halves fight on.</summary>
        Done,
    }
}
