using RushOps.Save;
using RushOps.Story;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RushOps.UI
{
    /// <summary>
    /// Main menu (issue #125, F-UI-02): Story Mode opens the intro the first time (#333) and the island map (#127) after that, Endlos the endless arena (#208), Spielerauswahl, Einstellungen. Shows
    /// the build version and, in development builds only, a button to the test menu. Buttons call <see cref="StartStory"/> and
    /// <see cref="OpenTestMenu"/>.
    /// </summary>
    public class MainMenu : MonoBehaviour
    {
        /// <summary>Name of the main menu scene.</summary>
        public const string SceneName = "MainMenu";

        [Tooltip("Scene „Story Mode“ opens: the island map.")]
        [SerializeField] private string _storyScene = "IslandMap";
        [Tooltip("Scene „Endlos“ opens: the endless arena, available from the start (E7).")]
        [SerializeField] private string _endlessScene = "Endless_Arena";
        [SerializeField] private string _testMenuScene = "TestMenu";
        [Tooltip("Scene „Story Mode“ opens the first time: the intro (issue #333).")]
        [SerializeField] private string _introScene = "Intro";
        [SerializeField] private SaveGameService _save;

        [Tooltip("Shows the build version, e.g. „v0.5.0“.")]
        [SerializeField] private Text _version;

        [Tooltip("Only shown in development builds (and in the editor).")]
        [SerializeField] private GameObject _testMenuButton;

        /// <summary>„Story Mode“: the intro until it was seen, then the island map (issue #333).</summary>
        public void StartStory()
        {
            StoryData story = _save != null ? _save.Data.Story : null;
            Load(StoryRules.SceneOnStoryStart(story, _introScene, _storyScene));
        }

        /// <summary>Starts an endless run in the arena at round 1.</summary>
        public void StartEndless()
        {
            RushOps.Endless.EndlessStart.Round = 1;
            RushOps.Endless.EndlessStart.Daily = false;
            RushOps.Endless.EndlessStart.Arcade = ArcadeChosen();
            Load(_endlessScene);
        }

        /// <summary>
        /// Starts the daily run in the arena (F-END-13, N22): the same start all day, its best goes into the daily list,
        /// not into the top 10.
        /// </summary>
        public void StartDaily()
        {
            RushOps.Endless.EndlessStart.Round = 1;
            RushOps.Endless.EndlessStart.Daily = true;
            RushOps.Endless.EndlessStart.Arcade = ArcadeChosen();
            Load(_endlessScene);
        }

        /// <summary>Opens the developer test menu.</summary>
        public void OpenTestMenu()
        {
            Load(_testMenuScene);
        }

        private void Awake()
        {
            if (_version != null)
            {
                _version.text = "v" + Application.version;
            }

            if (_testMenuButton != null)
            {
                _testMenuButton.SetActive(Debug.isDebugBuild);
            }
        }

        // The display chosen in the endless area (F-ARC-08); the campaign never uses it (AR7).
        private bool ArcadeChosen()
        {
            return _save != null && ArcadeRecords.IsSelected(_save.Data);
        }

        private static void Load(string scene)
        {
            if (string.IsNullOrEmpty(scene))
            {
                return;
            }

            Time.timeScale = 1f;
            SceneManager.LoadScene(scene);
        }
    }
}
