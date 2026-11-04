namespace RushOps.Puzzles
{
    /// <summary>How many energy cells a generator still needs (F-RAE-06, issue #33).</summary>
    public sealed class TransportGoal
    {
        /// <param name="required">Cells needed, at least 1.</param>
        public TransportGoal(int required)
        {
            Required = required > 1 ? required : 1;
        }

        /// <summary>Cells needed.</summary>
        public int Required { get; }

        /// <summary>Cells handed in so far, at most <see cref="Required"/>.</summary>
        public int Delivered { get; private set; }

        /// <summary>True once enough cells were handed in.</summary>
        public bool IsComplete => Delivered >= Required;

        /// <summary>Delivered cells as a fraction 0..1.</summary>
        public float Progress => (float)Delivered / Required;

        /// <summary>Counts one handed-in cell.</summary>
        /// <returns>True if it counted, false once the goal is already complete.</returns>
        public bool Deliver()
        {
            if (IsComplete)
            {
                return false;
            }

            Delivered++;
            return true;
        }
    }
}
