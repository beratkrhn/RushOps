using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RushOps.Player
{
    /// <summary>
    /// Ends the level when the player dies (F-SPF-03), after the short death moment from <see cref="PlayerConfig"/>:
    /// hands over to an <see cref="IDeathHandler"/> such as the „Gescheitert“ screen (issue #35), or reloads the level
    /// without one. The delay runs in unscaled time, so a hitstop or pause does not stretch it.
    /// </summary>
    public class LevelRestarter : MonoBehaviour
    {
        [SerializeField] private PlayerHealth _player;
        [Tooltip("Shown after the death moment, e.g. the result screens; empty reloads the level.")]
        [SerializeField] private MonoBehaviour _deathHandler;

        private bool _restarting;

        private void OnEnable()
        {
            if (_player != null)
            {
                _player.Died += OnDied;
            }
        }

        private void OnDisable()
        {
            if (_player != null)
            {
                _player.Died -= OnDied;
            }
        }

        private void OnDied()
        {
            if (!_restarting)
            {
                _restarting = true;
                PlayerMovement movement = _player.GetComponent<PlayerMovement>();
                float delay = movement != null && movement.Config != null ? movement.Config.DeathRestartDelay : 0f;
                StartCoroutine(Restart(delay));
            }
        }

        private IEnumerator Restart(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            if (_deathHandler is IDeathHandler handler)
            {
                handler.HandleDeath();
            }
            else
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }
    }
}
