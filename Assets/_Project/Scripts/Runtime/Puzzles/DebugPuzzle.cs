using UnityEngine;

namespace RushOps.Puzzles
{
    /// <summary>
    /// Test puzzle (issue #30): fills a progress bar over a few seconds once active and is solved when it is full, or
    /// at once via the context menu. Stands in for the real puzzle types in test levels.
    /// </summary>
    public class DebugPuzzle : Puzzle
    {
        [Tooltip("Seconds after activation until the puzzle is solved.")]
        [SerializeField, Min(0f)] private float _duration = 20f;
        [Tooltip("Text in the mission tracker.")]
        [SerializeField] private string _text = "Testrätsel";

        private float _elapsed;

        /// <inheritdoc />
        public override PuzzleDisplay Display =>
            PuzzleDisplay.WithProgress(_text, _duration > 0f ? _elapsed / _duration : 1f);

        /// <summary>Solves the puzzle at once, activating it first if needed.</summary>
        [ContextMenu("Jetzt lösen")]
        public void Force()
        {
            Activate();
            Solve();
        }

        private void Update()
        {
            if (State != PuzzleState.Active)
            {
                return;
            }

            _elapsed += Time.deltaTime;
            if (_elapsed >= _duration)
            {
                Solve();
            }
        }
    }
}
