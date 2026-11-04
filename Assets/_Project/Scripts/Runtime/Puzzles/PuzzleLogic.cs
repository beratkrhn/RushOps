using System;

namespace RushOps.Puzzles
{
    /// <summary>
    /// State of one puzzle (F-LVL-04, issue #30): inactive until activated, then active until solved. Only an
    /// active puzzle can be solved, and a solved puzzle stays solved.
    /// </summary>
    public sealed class PuzzleLogic
    {
        /// <summary>Current state.</summary>
        public PuzzleState State { get; private set; } = PuzzleState.Inactive;

        /// <summary>Raised on every state change, with the new state.</summary>
        public event Action<PuzzleState> StateChanged;

        /// <summary>Raised once when the puzzle becomes active.</summary>
        public event Action Activated;

        /// <summary>Raised once when the puzzle is solved.</summary>
        public event Action Solved;

        /// <summary>Activates an inactive puzzle, e.g. when its zone becomes active.</summary>
        /// <returns>True if the state changed.</returns>
        public bool Activate()
        {
            if (State != PuzzleState.Inactive)
            {
                return false;
            }

            State = PuzzleState.Active;
            StateChanged?.Invoke(State);
            Activated?.Invoke();
            return true;
        }

        /// <summary>Solves an active puzzle.</summary>
        /// <returns>True if the state changed.</returns>
        public bool Solve()
        {
            if (State != PuzzleState.Active)
            {
                return false;
            }

            State = PuzzleState.Solved;
            StateChanged?.Invoke(State);
            Solved?.Invoke();
            return true;
        }
    }
}
