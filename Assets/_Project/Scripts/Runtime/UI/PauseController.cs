using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RushOps.UI
{
    /// <summary>
    /// Pauses the game (F-HUD-06): stops the game time, shows the pause overlay with Resume and Restart,
    /// and pauses by itself when the app goes to the background (F-HUD-08). Buttons call <see cref="Pause"/>,
    /// <see cref="Resume"/> and <see cref="Restart"/>.
    /// </summary>
    public class PauseController : MonoBehaviour
    {
        [Tooltip("Overlay with the pause menu; shown only while paused.")]
        [SerializeField] private GameObject _overlay;

        /// <summary>Raised when the game pauses.</summary>
        public event Action Paused;

        /// <summary>Raised when the game continues.</summary>
        public event Action Resumed;

        /// <summary>True while the game is paused.</summary>
        public bool IsPaused { get; private set; }

        /// <summary>Stops the game time and shows the overlay.</summary>
        public void Pause()
        {
            if (IsPaused)
            {
                return;
            }

            IsPaused = true;
            Time.timeScale = 0f;
            SetOverlay(true);
            Paused?.Invoke();
        }

        /// <summary>Hides the overlay and lets the game time run again.</summary>
        public void Resume()
        {
            if (!IsPaused)
            {
                return;
            }

            IsPaused = false;
            Time.timeScale = 1f;
            SetOverlay(false);
            Resumed?.Invoke();
        }

        /// <summary>Restarts the current level.</summary>
        public void Restart()
        {
            IsPaused = false;
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void Awake()
        {
            SetOverlay(false);
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused)
            {
                Pause();
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            // On phones losing focus means a call, the notification shade or the app switcher.
            // In the editor clicking another window would pause all the time.
            if (!hasFocus && Application.isMobilePlatform)
            {
                Pause();
            }
        }

        private void OnDestroy()
        {
            // A paused level that gets unloaded must not leave the next one frozen.
            if (IsPaused)
            {
                Time.timeScale = 1f;
            }
        }

        private void SetOverlay(bool visible)
        {
            if (_overlay != null)
            {
                _overlay.SetActive(visible);
            }
        }
    }
}
