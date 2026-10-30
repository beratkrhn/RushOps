using System;

namespace RushOps.Levels
{
    /// <summary>
    /// State of one zone (F-LVL-02): inactive until the player enters, then active until its exit gate opens.
    /// A zone completes even if its gate opens before the player arrived; it never becomes active after that.
    /// </summary>
    public class ZoneLogic
    {
        /// <summary>Current state.</summary>
        public ZoneState State { get; private set; } = ZoneState.Inactive;

        /// <summary>Raised once when the zone becomes active.</summary>
        public event Action Activated;

        /// <summary>Raised once when the zone is completed.</summary>
        public event Action Completed;

        /// <summary>Activates an inactive zone, e.g. when the player enters it. Returns true if the state changed.</summary>
        public bool Enter()
        {
            if (State != ZoneState.Inactive)
            {
                return false;
            }

            State = ZoneState.Active;
            Activated?.Invoke();
            return true;
        }

        /// <summary>Completes the zone, e.g. when its exit gate opened. Returns true if the state changed.</summary>
        public bool Complete()
        {
            if (State == ZoneState.Completed)
            {
                return false;
            }

            State = ZoneState.Completed;
            Completed?.Invoke();
            return true;
        }
    }
}
