namespace RushOps.Puzzles
{
    /// <summary>
    /// When a zombie puzzle sends its next zombie (issue #289, F-RAE-10, F-RAE-12): the first one right away, then one
    /// every interval, but never more than the cap alive at once. A send that the cap holds back happens as soon as one
    /// of them dies.
    /// </summary>
    public sealed class ReinforcementTimer
    {
        // Intervals that are whole multiples of the time step end on that step despite float rounding.
        private const float Epsilon = 1e-4f;

        private readonly float _interval;
        private readonly int _max;
        private float _untilNext;

        /// <param name="interval">Seconds between two zombies; 0 or less sends whenever the cap allows.</param>
        /// <param name="max">Most of them alive at once; at least 1.</param>
        public ReinforcementTimer(float interval, int max)
        {
            _interval = interval > 0f ? interval : 0f;
            _max = max > 0 ? max : 1;
        }

        /// <summary>Seconds until the next send is due; 0 when due.</summary>
        public float UntilNext => _untilNext;

        /// <summary>Advances by <paramref name="deltaTime"/> with <paramref name="alive"/> sent zombies alive.</summary>
        /// <returns>True if one zombie should be sent now.</returns>
        public bool Tick(float deltaTime, int alive)
        {
            if (deltaTime > 0f && _untilNext > 0f)
            {
                _untilNext -= deltaTime;
                if (_untilNext < Epsilon)
                {
                    _untilNext = 0f;
                }
            }

            if (_untilNext > 0f || alive >= _max)
            {
                return false;
            }

            _untilNext = _interval;
            return true;
        }
    }
}
