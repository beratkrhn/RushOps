using RushOps.Save;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RushOps.Story
{
    /// <summary>
    /// The intro scene (issue #332, F-STO-02): plays the intro cutscene and opens the island map when it ends, played
    /// through or skipped. Then the intro counts as seen and is saved (issue #333, F-DAT-03), so „Story Mode“ goes
    /// straight to the map next time. Thin adapter around <see cref="CutscenePlayer"/>.
    /// </summary>
    public class IntroScene : MonoBehaviour
    {
        /// <summary>Name of the intro scene.</summary>
        public const string SceneName = "Intro";

        [SerializeField] private CutscenePlayer _player;
        [SerializeField] private SaveGameService _save;
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
            if (_save != null && StoryRules.MarkIntroSeen(_save.Data.Story))
            {
                _save.Save();
            }

            Time.timeScale = 1f;
            SceneManager.LoadScene(_nextScene);
        }
    }
}
