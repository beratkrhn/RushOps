using RushOps.Endless;
using RushOps.Levels;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RushOps.UI
{
    /// <summary>
    /// Developer menu listing the test levels (issue #35), reached from the main menu in development builds
    /// (issue #125). Its buttons call <see cref="Load"/> with the index of a level.
    /// </summary>
    public class TestMenu : MonoBehaviour
    {
        [SerializeField] private LevelDefinition[] _levels = System.Array.Empty<LevelDefinition>();

        /// <summary>Scene of the endless arena.</summary>
        public const string EndlessScene = "Endless_Arena";

        /// <summary>The listed levels.</summary>
        public LevelDefinition[] Levels => _levels;

        /// <summary>Loads the level at <paramref name="index"/>.</summary>
        public void Load(int index)
        {
            if (index < 0 || index >= _levels.Length || _levels[index] == null)
            {
                return;
            }

            Time.timeScale = 1f;
            SceneManager.LoadScene(_levels[index].SceneName);
        }

        /// <summary>Starts an endless run at <paramref name="round"/> (issue #207).</summary>
        public void LoadEndless(int round)
        {
            EndlessStart.Round = round;
            EndlessStart.Daily = false;
            Time.timeScale = 1f;
            SceneManager.LoadScene(EndlessScene);
        }
    }
}
