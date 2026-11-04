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
    }
}
