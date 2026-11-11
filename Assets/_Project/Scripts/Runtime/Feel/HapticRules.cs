namespace RushOps.Feel
{
    /// <summary>
    /// Decides which vibration pulses go out (issue #300, F-AUD-05): two pulses keep a minimum gap, so chains of
    /// explosions do not buzz without pause, but losing a heart always gets through right after an explosion.
    /// </summary>
    public class HapticRules
    {
        private readonly float _minGap;
        private bool _any;
        private float _last;
        private HapticKind _lastKind;

        /// <summary>Rules with <paramref name="minGapSeconds"/> between two pulses.</summary>
        public HapticRules(float minGapSeconds)
        {
            _minGap = minGapSeconds < 0f ? 0f : minGapSeconds;
        }

        /// <summary>
        /// True if a pulse of <paramref name="kind"/> at <paramref name="now"/> (seconds) should go out; it then counts
        /// as the last pulse.
        /// </summary>
        public bool Request(HapticKind kind, float now)
        {
            bool allowed = !_any || now - _last >= _minGap
                || (kind == HapticKind.Damage && _lastKind == HapticKind.Explosion);
            if (!allowed)
            {
                return false;
            }

            _any = true;
            _last = now;
            _lastKind = kind;
            return true;
        }
    }
}
