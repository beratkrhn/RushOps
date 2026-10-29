namespace RushOps.Combat
{
    /// <summary>
    /// Decides how many shots an automatic weapon fires per frame.
    /// The first shot after a pause fires immediately; holding the trigger then fires at a steady rate,
    /// independent of the frame rate. Releasing the trigger does not reset a running cooldown,
    /// so tapping cannot fire faster than holding.
    /// </summary>
    public class FireRateTimer
    {
        private float _cooldown;

        /// <summary>Seconds until the next shot is allowed; 0 when ready.</summary>
        public float Cooldown => _cooldown;

        /// <summary>
        /// Advances the timer by <paramref name="deltaTime"/> and returns the number of shots to fire now.
        /// Returns 0 while the trigger is released or <paramref name="shotsPerSecond"/> is not positive.
        /// </summary>
        public int Tick(float deltaTime, bool triggerHeld, float shotsPerSecond)
        {
            _cooldown -= deltaTime;

            if (!triggerHeld || shotsPerSecond <= 0f)
            {
                // Idle time does not bank shots.
                if (_cooldown < 0f)
                {
                    _cooldown = 0f;
                }

                return 0;
            }

            float interval = 1f / shotsPerSecond;
            int shots = 0;
            while (_cooldown <= 0f)
            {
                shots++;
                _cooldown += interval;
            }

            return shots;
        }

        /// <summary>Makes the weapon ready to fire immediately.</summary>
        public void Reset()
        {
            _cooldown = 0f;
        }
    }
}
