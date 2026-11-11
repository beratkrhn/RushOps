using System;

namespace RushOps.Scoring
{
    /// <summary>The three badges every campaign level has (issue #287, F-SCO-04, CH1, N23), as flags.</summary>
    [Flags]
    public enum BadgeKind
    {
        /// <summary>No badge.</summary>
        None = 0,

        /// <summary>„Unversehrt“: level finished without losing a heart.</summary>
        Unharmed = 1,

        /// <summary>„Blitz“: level finished within its target time.</summary>
        Blitz = 2,

        /// <summary>„Kombo“: the combo target multiplier was reached in the level.</summary>
        Combo = 4,

        /// <summary>All three badges.</summary>
        All = Unharmed | Blitz | Combo,
    }
}
