using UnityEngine;

namespace RushOps.Levels
{
    /// <summary>
    /// Gate condition for kill gates (F-LVL-03): met once the level counted the set number of kills. Without a
    /// zone every kill since level start counts, in any zone; with <see cref="CountFromZone"/> set only the kills
    /// after the player entered that zone (decision P1).
    /// </summary>
    public class KillGateCondition : MonoBehaviour, IGateCondition
    {
        [SerializeField] private LevelKills _kills;
        [Tooltip("Kills needed to open the gate.")]
        [SerializeField, Min(1)] private int _requiredKills = 10;
        [Tooltip("If set, kills count only from when the player entered this zone; empty counts from level start.")]
        [SerializeField] private Zone _countFromZone;

        private KillGoal _goal;

        /// <summary>Kills needed to open the gate.</summary>
        public int RequiredKills => Goal.Required;

        /// <summary>Kills counted towards the goal, at most <see cref="RequiredKills"/>.</summary>
        public int CurrentKills => Goal.Current;

        /// <summary>Zone whose entry starts the count, or null to count from level start.</summary>
        public Zone CountFromZone => _countFromZone;

        /// <inheritdoc />
        public bool IsMet => Goal.IsMet;

        // Created on first use, so it works no matter which Awake runs first.
        private KillGoal Goal => _goal ??= new KillGoal(
            _kills != null ? _kills.Counter : null, _requiredKills, _countFromZone == null);

        private void OnEnable()
        {
            if (_countFromZone == null)
            {
                return;
            }

            if (_countFromZone.State != ZoneState.Inactive)
            {
                Goal.StartCounting();
            }
            else
            {
                _countFromZone.Activated += OnZoneActivated;
            }
        }

        private void OnDisable()
        {
            if (_countFromZone != null)
            {
                _countFromZone.Activated -= OnZoneActivated;
            }
        }

        private void OnZoneActivated(Zone zone)
        {
            zone.Activated -= OnZoneActivated;
            Goal.StartCounting();
        }
    }
}
