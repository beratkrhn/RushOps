namespace RushOps.Perks
{
    /// <summary>Countdown of a timed perk that restarts when the perk is collected again (F-PRK-04).</summary>
    public sealed class PerkTimer
    {
        // Timers that are whole multiples of the time step end on that step despite float rounding.
        private const float Epsilon = 1e-4f;

        /// <summary>Seconds left; 0 when not running.</summary>
        public float Remaining { get; private set; }

        /// <summary>True while time is left.</summary>
        public bool IsRunning => Remaining > 0f;

        /// <summary>Starts or restarts the countdown with <paramref name="duration"/> seconds; 0 or less stops it.</summary>
        public void Restart(float duration)
        {
            Remaining = duration > 0f ? duration : 0f;
        }

        /// <summary>Stops the countdown without waiting.</summary>
        public void Stop()
        {
            Remaining = 0f;
        }

        /// <summary>Advances the countdown by <paramref name="deltaTime"/> seconds.</summary>
        /// <returns>True if the countdown ran out with this step.</returns>
        public bool Tick(float deltaTime)
        {
            if (Remaining <= 0f || deltaTime <= 0f)
            {
                return false;
            }

            Remaining -= deltaTime;
            if (Remaining > Epsilon)
            {
                return false;
            }

            Remaining = 0f;
            return true;
        }
    }
}
