using System.Collections.Generic;
using UnityEngine;

namespace RushOps.Levels
{
    /// <summary>
    /// Gate condition that waits for several others on the same gate (decision L2), e.g. a
    /// <see cref="KillGateCondition"/> and a puzzle condition. The mission tracker shows them one below the other.
    /// </summary>
    public class GateConditionGroup : MonoBehaviour, IGateCondition
    {
        [Tooltip("Conditions that must all be met; each must implement IGateCondition.")]
        [SerializeField] private MonoBehaviour[] _conditions = System.Array.Empty<MonoBehaviour>();

        private AllConditions _all;
        private IGateCondition[] _resolved;

        /// <summary>The conditions of the group; entries that are no gate condition are null.</summary>
        public IReadOnlyList<IGateCondition> Conditions => Resolved;

        /// <inheritdoc />
        public bool IsMet => (_all ??= new AllConditions(Resolved)).IsMet;

        /// <summary>The first condition of type <typeparamref name="T"/> in the group, or null.</summary>
        public T Find<T>() where T : class, IGateCondition
        {
            foreach (IGateCondition condition in Resolved)
            {
                if (condition is T match)
                {
                    return match;
                }
            }

            return null;
        }

        private IGateCondition[] Resolved
        {
            get
            {
                if (_resolved == null)
                {
                    int count = _conditions != null ? _conditions.Length : 0;
                    _resolved = new IGateCondition[count];
                    for (int i = 0; i < count; i++)
                    {
                        _resolved[i] = _conditions[i] as IGateCondition;
                    }
                }

                return _resolved;
            }
        }
    }
}
