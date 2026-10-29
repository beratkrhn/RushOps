using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RushOps.Player
{
    /// <summary>
    /// Restarts the level when the player dies (F-SPF-03), after the short death moment from <see cref="PlayerConfig"/>.
    /// The delay runs in unscaled time, so a hitstop or pause does not stretch it.
    /// </summary>
    public class LevelRestarter : MonoBehaviour
    {
        [SerializeField] private PlayerHealth _player;

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

        private static IEnumerator Restart(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
