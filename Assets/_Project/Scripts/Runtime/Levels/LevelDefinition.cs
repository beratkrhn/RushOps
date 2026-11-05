using UnityEngine;

namespace RushOps.Levels
{
    /// <summary>
    /// One level (F-LVL-05, F-SCO-03, issue #35): its name, its scene and the scores for the stars. Reaching the goal
    /// always gives one star; the thresholds add the second and third.
    /// </summary>
    [CreateAssetMenu(fileName = "LevelDefinition", menuName = "RushOps/Level Definition")]
    public class LevelDefinition : ScriptableObject
    {
        [Tooltip("Name shown to the player (German).")]
        [SerializeField] private string _displayName = "";

        [Tooltip("Name of the scene in the build settings.")]
        [SerializeField] private string _sceneName = "";

        [Tooltip("Score needed for two stars.")]
        [SerializeField, Min(0)] private int _twoStarScore = 1000;

        [Tooltip("Score needed for three stars.")]
        [SerializeField, Min(0)] private int _threeStarScore = 2500;

        [Tooltip("Extra line on „Level geschafft!“, e.g. „Alle Inseln geschafft!“ after the last boss (C12); empty = none.")]
        [SerializeField] private string _completionMessage = "";

        /// <summary>Name shown to the player.</summary>
        public string DisplayName => _displayName;

        /// <summary>Name of the scene in the build settings.</summary>
        public string SceneName => _sceneName;

        /// <summary>Score needed for two stars.</summary>
        public int TwoStarScore => _twoStarScore;

        /// <summary>Score needed for three stars.</summary>
        public int ThreeStarScore => _threeStarScore;

        /// <summary>Extra line on the done screen; empty if none.</summary>
        public string CompletionMessage => _completionMessage ?? string.Empty;

        /// <summary>Stars for finishing this level with <paramref name="score"/>, 1 to 3.</summary>
        public int StarsFor(int score)
        {
            return StarRating.Stars(score, _twoStarScore, _threeStarScore);
        }
    }
}
