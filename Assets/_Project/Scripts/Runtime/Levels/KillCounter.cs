using System;

namespace RushOps.Levels
{
    /// <summary>
    /// Kills in the current level (F-LVL-03): every enemy death counts once, whatever killed it (shot,
    /// explosion, nuke). Counted from level start for the whole level, not per zone.
    /// </summary>
    public class KillCounter
    {
        /// <summary>Kills so far.</summary>
        public int Count { get; private set; }

        /// <summary>Raised after every kill with the new count.</summary>
        public event Action<int> Changed;

        /// <summary>Counts one kill.</summary>
        public void Register()
        {
            Count++;
            Changed?.Invoke(Count);
        }
    }
}
