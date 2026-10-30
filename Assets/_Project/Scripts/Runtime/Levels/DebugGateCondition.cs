using UnityEngine;

namespace RushOps.Levels
{
    /// <summary>
    /// Test condition for gates (issue #17): met a few seconds after a zone became active, or at once via the
    /// context menu. Stands in for kill gates and puzzles in test levels.
    /// </summary>
    public class DebugGateCondition : MonoBehaviour, IGateCondition
    {
        [Tooltip("Zone whose activation starts the countdown; without one it starts with the scene.")]
        [SerializeField] private Zone _zone;
        [Tooltip("Seconds after the zone became active until the condition is met.")]
        [SerializeField, Min(0f)] private float _delay = 5f;

        private float _elapsed;
        private bool _forced;

        /// <inheritdoc />
        public bool IsMet => _forced || _elapsed >= _delay;

        /// <summary>Meets the condition at once.</summary>
        [ContextMenu("Jetzt erfüllen")]
        public void Force()
        {
            _forced = true;
        }

        private void Update()
        {
            if (_zone == null || _zone.State != ZoneState.Inactive)
            {
                _elapsed += Time.deltaTime;
            }
        }
    }
}
