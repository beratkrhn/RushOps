namespace RushOps.Levels
{
    /// <summary>
    /// Condition of a kill gate (F-LVL-03): met once the level's <see cref="KillCounter"/> counted the required kills.
    /// By default every kill since level start counts; a goal created with <c>countFromLevelStart: false</c> counts
    /// only the kills after <see cref="StartCounting"/>, e.g. from when the player entered the gate's zone.
    /// </summary>
    public class KillGoal : IGateCondition
    {
        private readonly KillCounter _counter;
        private int _baseline;

        /// <summary>
        /// Creates a goal of <paramref name="required"/> kills on <paramref name="counter"/>; at least one kill. With
        /// <paramref name="countFromLevelStart"/> false the goal counts nothing until <see cref="StartCounting"/>.
        /// </summary>
        public KillGoal(KillCounter counter, int required, bool countFromLevelStart = true)
        {
            _counter = counter;
            Required = required > 0 ? required : 1;
            IsCounting = countFromLevelStart;
        }

        /// <summary>Kills needed to open the gate.</summary>
        public int Required { get; }

        /// <summary>True once kills count towards the goal.</summary>
        public bool IsCounting { get; private set; }

        /// <summary>Kills counted towards the goal, at most <see cref="Required"/>.</summary>
        public int Current
        {
            get
            {
                if (_counter == null || !IsCounting)
                {
                    return 0;
                }

                int kills = _counter.Count - _baseline;
                return kills < Required ? kills : Required;
            }
        }

        /// <inheritdoc />
        public bool IsMet => Current >= Required;

        /// <summary>
        /// Starts counting from the counter's current count. Does nothing if the goal already counts, so kills
        /// counted before are never lost.
        /// </summary>
        public void StartCounting()
        {
            if (IsCounting)
            {
                return;
            }

            _baseline = _counter != null ? _counter.Count : 0;
            IsCounting = true;
        }
    }
}
