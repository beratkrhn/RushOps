using RushOps.UI;
using UnityEngine;

namespace RushOps.Feel
{
    /// <summary>
    /// Freezes the game time for a moment (issue #12), measured in real time. Leaves the time alone while the
    /// game is paused and never resumes a game that got paused during the freeze.
    /// </summary>
    public class Hitstop : MonoBehaviour
    {
        [SerializeField] private PauseController _pause;

        private float _remaining;

        /// <summary>True while the game is frozen by a hitstop.</summary>
        public bool IsActive => _remaining > 0f;

        /// <summary>Freezes for <paramref name="duration"/> seconds, or extends a running freeze to that length.</summary>
        public void Trigger(float duration)
        {
            if (duration <= 0f || IsPaused)
            {
                return;
            }

            _remaining = Mathf.Max(_remaining, duration);
            Time.timeScale = 0f;
        }

        private bool IsPaused => _pause != null && _pause.IsPaused;

        private void Update()
        {
            if (_remaining <= 0f)
            {
                return;
            }

            _remaining -= Time.unscaledDeltaTime;
            if (_remaining <= 0f)
            {
                End();
            }
        }

        private void OnDisable()
        {
            if (IsActive)
            {
                End();
            }
        }

        private void End()
        {
            _remaining = 0f;
            if (!IsPaused)
            {
                Time.timeScale = 1f;
            }
        }
    }
}
