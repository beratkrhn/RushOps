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
    }
}
