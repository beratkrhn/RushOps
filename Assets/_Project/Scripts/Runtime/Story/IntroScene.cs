using UnityEngine;
using UnityEngine.SceneManagement;

namespace RushOps.Story
{
    /// <summary>
    /// The intro scene (issue #332, F-STO-02): plays the intro cutscene and opens the island map when it ends, played
    /// through or skipped. Thin adapter around <see cref="CutscenePlayer"/>.
    /// </summary>
    public class IntroScene : MonoBehaviour
    {
        /// <summary>Name of the intro scene.</summary>
        public const string SceneName = "Intro";

        [SerializeField] private CutscenePlayer _player;
        [Tooltip("Scene after the intro: the island map.")]
        [SerializeField] private string _nextScene = "IslandMap";

        private bool _leaving;

        private void OnEnable()
        {
            if (_player != null)
            {
                _player.Finished += OnFinished;
            }
        }

        private void OnDisable()
        {
            if (_player != null)
            {
                _player.Finished -= OnFinished;
            }
        }

        private void OnFinished()
        {
            if (_leaving || string.IsNullOrEmpty(_nextScene))
            {
                return;
            }

            _leaving = true;
            Time.timeScale = 1f;
            SceneManager.LoadScene(_nextScene);
        }
    }
}
