using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RushOps.UI
{
    /// <summary>
    /// Main menu (issue #125, F-UI-02): Story Mode opens the island map (#127), Spielerauswahl, Einstellungen. Shows
    /// the build version and, in development builds only, a button to the test menu. Buttons call <see cref="StartStory"/> and
    /// <see cref="OpenTestMenu"/>.
    /// </summary>
    public class MainMenu : MonoBehaviour
    {
        /// <summary>Name of the main menu scene.</summary>
        public const string SceneName = "MainMenu";

        [Tooltip("Scene „Story Mode“ opens: the island map.")]
        [SerializeField] private string _storyScene = "IslandMap";
        [SerializeField] private string _testMenuScene = "TestMenu";

        [Tooltip("Shows the build version, e.g. „v0.5.0“.")]
        [SerializeField] private Text _version;

        [Tooltip("Only shown in development builds (and in the editor).")]
        [SerializeField] private GameObject _testMenuButton;

        /// <summary>Opens the island map („Story Mode“).</summary>
        public void StartStory()
        {
            Load(_storyScene);
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
