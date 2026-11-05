using System;
using RushOps.Levels;
using UnityEngine;

namespace RushOps.Puzzles
{
    /// <summary>
    /// Gate condition for puzzle gates (F-LVL-04, issue #30): met once all its puzzles are solved. Without puzzles it
    /// is never met.
    /// </summary>
    public class PuzzleSolvedCondition : MonoBehaviour, IGateCondition
    {
        [Tooltip("Puzzles that must all be solved to open the gate.")]
        [SerializeField] private Puzzle[] _puzzles = Array.Empty<Puzzle>();

        // Reused by Shown, so the tracker reading it every frame allocates nothing.
        private PuzzleState[] _states;
        private PuzzleDisplayKind[] _kinds;

        /// <summary>Puzzles that must all be solved, in their order.</summary>
        public System.Collections.Generic.IReadOnlyList<Puzzle> Puzzles => _puzzles ?? Array.Empty<Puzzle>();

        /// <inheritdoc />
        public bool IsMet
        {
            get
            {
                if (_puzzles == null || _puzzles.Length == 0)
                {
                    return false;
                }

                return Current == null;
            }
        }

        /// <summary>The first puzzle not solved yet, in the given order; null once all are solved.</summary>
        public Puzzle Current
        {
            get
            {
                if (_puzzles == null)
                {
                    return null;
                }

                foreach (Puzzle puzzle in _puzzles)
                {
                    if (puzzle != null && puzzle.State != PuzzleState.Solved)
                    {
                        return puzzle;
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// The puzzle the mission tracker shows: one with a running countdown first, else <see cref="Current"/>
        /// (<see cref="PuzzleFocus"/>); null once all are solved.
        /// </summary>
        public Puzzle Shown
        {
            get
            {
                if (_puzzles == null || _puzzles.Length == 0)
                {
                    return null;
                }

                if (_states == null || _states.Length != _puzzles.Length)
                {
                    _states = new PuzzleState[_puzzles.Length];
                    _kinds = new PuzzleDisplayKind[_puzzles.Length];
                }

                for (int i = 0; i < _puzzles.Length; i++)
                {
                    Puzzle puzzle = _puzzles[i];
                    _states[i] = puzzle != null ? puzzle.State : PuzzleState.Solved;
                    _kinds[i] = puzzle != null ? puzzle.Display.Kind : PuzzleDisplayKind.Text;
                }

                int index = PuzzleFocus.IndexToShow(_states, _kinds);
                return index >= 0 ? _puzzles[index] : null;
            }
        }
    }
}
