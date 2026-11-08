namespace RushOps.Perks
{
    /// <summary>
    /// State of the super weapon (F-PRK-09, issue #253, W11): picking one up replaces the active weapon for a
    /// while, picking it up again restarts the timer, and when it ends the previous weapon is active again. The
    /// state is an overlay on top of the weapon slots and never touches them, so no slot is lost or swapped.
    /// Collecting the other super weapon switches to it with a fresh timer.
    /// </summary>
    public sealed class SuperWeaponState
    {
        private readonly PerkTimer _timer = new PerkTimer();

        /// <summary>The super weapon in the player's hands; <see cref="SuperWeaponKind.None"/> when there is none.</summary>
        public SuperWeaponKind Kind { get; private set; }

        /// <summary>True while a super weapon replaces the normal weapon.</summary>
        public bool IsActive => Kind != SuperWeaponKind.None && _timer.IsRunning;

        /// <summary>Seconds left; 0 when not active.</summary>
        public float Remaining => _timer.Remaining;

        /// <summary>
        /// Takes <paramref name="kind"/> into the hands for <paramref name="duration"/> seconds, or restarts the
        /// timer if it is already there. Ignored for <see cref="SuperWeaponKind.None"/> or a duration of 0 or less.
        /// </summary>
        public void Activate(SuperWeaponKind kind, float duration)
        {
            if (kind == SuperWeaponKind.None || duration <= 0f)
            {
                return;
            }

            Kind = kind;
            _timer.Restart(duration);
        }

        /// <summary>Removes the super weapon, e.g. at level end, death or restart.</summary>
        public void Clear()
        {
            Kind = SuperWeaponKind.None;
            _timer.Stop();
        }

        /// <summary>Advances the timer. Returns true if the super weapon ran out with this step.</summary>
        public bool Tick(float deltaTime)
        {
            if (!_timer.Tick(deltaTime))
            {
                return false;
            }

            Kind = SuperWeaponKind.None;
            return true;
        }
    }
}
