using RushOps.Levels;
using RushOps.Save;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RushOps.UI
{
    /// <summary>
    /// Main menu (issue #125, F-UI-02): Story Mode, Spielerauswahl, Einstellungen. Until the island map exists (#127)
    /// „Story Mode“ starts the next open level of the <see cref="Campaign"/> directly. Shows the build version and,
    /// in development builds only, a button to the test menu. Buttons call <see cref="StartStory"/> and
    /// <see cref="OpenTestMenu"/>.
    /// </summary>
    public class MainMenu : MonoBehaviour
    {
        /// <summary>Name of the main menu scene.</summary>
        public const string SceneName = "MainMenu";

        [SerializeField] private Campaign _campaign;
        [SerializeField] private SaveGameService _save;
        [SerializeField] private string _testMenuScene = "TestMenu";

        [Tooltip("Shows the build version, e.g. „v0.5.0“.")]
        [SerializeField] private Text _version;

        [Tooltip("Only shown in development builds (and in the editor).")]
        [SerializeField] private GameObject _testMenuButton;

        /// <summary>Scene „Story Mode“ loads now; empty if the campaign has no playable level.</summary>
        public string StoryScene
        {
            get
            {
                if (_campaign == null || _save == null
                    || !_campaign.Unlocks.TryGetLevelToPlay(_save.Data.Progress, out int island, out int level))
                {
                    return string.Empty;
                }

                LevelDefinition definition = _campaign.LevelAt(island, level);
                return definition != null ? definition.SceneName : string.Empty;
            }
        }

        /// <summary>Starts the story („Story Mode“).</summary>
        public void StartStory()
        {
            Load(StoryScene);
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
