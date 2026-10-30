using UnityEngine;

namespace RushOps.Levels
{
    /// <summary>
    /// Gate condition for kill gates (F-LVL-03): met once the level counted the set number of kills since its
    /// start, in any zone.
    /// </summary>
    public class KillGateCondition : MonoBehaviour, IGateCondition
    {
        [SerializeField] private LevelKills _kills;
        [Tooltip("Kills since level start needed to open the gate.")]
        [SerializeField, Min(1)] private int _requiredKills = 10;

        private KillGoal _goal;

        /// <summary>Kills needed to open the gate.</summary>
        public int RequiredKills => Goal.Required;

        /// <summary>Kills counted towards the goal, at most <see cref="RequiredKills"/>.</summary>
        public int CurrentKills => Goal.Current;

        /// <inheritdoc />
        public bool IsMet => Goal.IsMet;

        // Created on first use, so it works no matter which Awake runs first.
        private KillGoal Goal => _goal ??= new KillGoal(_kills != null ? _kills.Counter : null, _requiredKills);
    }
}
