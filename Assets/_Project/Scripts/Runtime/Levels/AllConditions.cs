using System.Collections.Generic;

namespace RushOps.Levels
{
    /// <summary>
    /// Gate condition made of several conditions (decision L2): met once all of them are met, e.g. a kill goal and a
    /// puzzle at the same gate. Empty or null conditions never open the gate.
    /// </summary>
    public class AllConditions : IGateCondition
    {
        private readonly IReadOnlyList<IGateCondition> _conditions;

        /// <summary>Creates a condition over <paramref name="conditions"/>.</summary>
        public AllConditions(IReadOnlyList<IGateCondition> conditions)
        {
            _conditions = conditions;
        }

        /// <inheritdoc />
        public bool IsMet
        {
            get
            {
                if (_conditions == null || _conditions.Count == 0)
                {
                    return false;
                }

                for (int i = 0; i < _conditions.Count; i++)
                {
                    if (_conditions[i] == null || !_conditions[i].IsMet)
                    {
                        return false;
                    }
                }

                return true;
            }
        }
    }
}
