using UnityEngine;

namespace RushOps.Puzzles
{
    /// <summary>Values all puzzles share (issue #30). The puzzle types add their own configs.</summary>
    [CreateAssetMenu(fileName = "PuzzleConfig", menuName = "RushOps/Puzzle Config")]
    public class PuzzleConfig : ScriptableObject
    {
        [Tooltip("Volume of the sound when a puzzle is solved.")]
        [SerializeField, Range(0f, 1f)] private float _solvedVolume = 0.9f;

        /// <summary>Volume of the sound when a puzzle is solved.</summary>
        public float SolvedVolume => _solvedVolume;
    }
}
