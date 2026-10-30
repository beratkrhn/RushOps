namespace RushOps.Levels
{
    /// <summary>Condition of a kill gate (F-LVL-03): met once the level's <see cref="KillCounter"/> reached the required kills.</summary>
    public class KillGoal : IGateCondition
    {
        private readonly KillCounter _counter;

        /// <summary>Creates a goal of <paramref name="required"/> kills on <paramref name="counter"/>; at least one kill.</summary>
        public KillGoal(KillCounter counter, int required)
        {
            _counter = counter;
            Required = required > 0 ? required : 1;
        }

        /// <summary>Kills needed to open the gate.</summary>
        public int Required { get; }

        /// <summary>Kills counted towards the goal, at most <see cref="Required"/>.</summary>
        public int Current => _counter == null ? 0 : _counter.Count < Required ? _counter.Count : Required;

        /// <inheritdoc />
        public bool IsMet => Current >= Required;
    }
}
