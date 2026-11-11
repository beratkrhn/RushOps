using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RushOps.UI
{
    /// <summary>
    /// Pauses the game (F-HUD-06): stops the game time, shows the pause overlay with Resume, Restart and main menu,
    /// and pauses by itself when the app goes to the background (F-HUD-08). Buttons call <see cref="Pause"/>,
    /// <see cref="Resume"/>, <see cref="Restart"/> and <see cref="QuitToMenu"/>.
    /// </summary>
    public class PauseController : MonoBehaviour
    {
        [Tooltip("Overlay with the pause menu; shown only while paused.")]
        [SerializeField] private GameObject _overlay;

        [Tooltip("Scene the „Hauptmenü“ button loads (issue #125).")]
        [SerializeField] private string _menuScene = "MainMenu";

        /// <summary>Raised when the game pauses.</summary>
        public event Action Paused;

        /// <summary>Raised when the game continues.</summary>
        public event Action Resumed;

        private bool _paused;
        private bool _frozen;
        private bool _held;

        /// <summary>True while the game is paused or frozen by a result screen.</summary>
        public bool IsPaused
        {
            get => _paused || _frozen;
            private set => _paused = value;
        }

        /// <summary>
        /// True while a screen holds the game without the pause overlay, e.g. the upgrade choice (F-END-11). The pause
        /// button still works; resuming from it keeps the game held.
        /// </summary>
        public bool IsHeld => _held;

        /// <summary>True while the game time stands for any reason: paused, frozen or held.</summary>
        public bool StopsTime => _paused || _frozen || _held;

        /// <summary>Holds the game time for a screen that waits for the player, without the pause overlay.</summary>
        public void Hold()
        {
            _held = true;
            Time.timeScale = 0f;
        }

        /// <summary>Ends a <see cref="Hold"/>; the game time runs again unless paused or frozen.</summary>
        public void Release()
        {
            if (!_held)
            {
                return;
            }

            _held = false;
            if (!_paused && !_frozen)
            {
                Time.timeScale = 1f;
            }
        }

        /// <summary>True while a result screen freezes the game (issue #35); pausing and resuming do nothing then.</summary>
        public bool IsFrozen => _frozen;

        /// <summary>Stops the game time for a result screen, without the pause overlay. Resume cannot undo it.</summary>
        public void Freeze()
        {
            _frozen = true;
            _paused = false;
            SetOverlay(false);
            Time.timeScale = 0f;
        }

        /// <summary>Ends a freeze, e.g. before loading another scene.</summary>
        public void Unfreeze()
        {
            _frozen = false;
            Time.timeScale = 1f;
        }

        /// <summary>Stops the game time and shows the overlay.</summary>
        public void Pause()
        {
            if (IsPaused || _frozen)
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
            if (!_paused || _frozen)
            {
                return;
            }

            IsPaused = false;
            Time.timeScale = _held ? 0f : 1f;
            SetOverlay(false);
            Resumed?.Invoke();
        }

        /// <summary>Restarts the current level.</summary>
        public void Restart()
        {
            IsPaused = false;
            _frozen = false;
            _held = false;
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        /// <summary>Leaves the level for the main menu.</summary>
        public void QuitToMenu()
        {
            IsPaused = false;
            _frozen = false;
            _held = false;
            Time.timeScale = 1f;
            SceneManager.LoadScene(_menuScene);
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
            if (StopsTime)
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
