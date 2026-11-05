using System.Collections.Generic;

namespace RushOps.Puzzles
{
    /// <summary>
    /// Which of several puzzles at one gate the mission tracker shows (decision L2, issue #153). A running countdown
    /// wins, so timed switches stay visible while another puzzle of the gate is still open (F-RAE-04); otherwise the
    /// first unsolved puzzle in order.
    /// </summary>
    public static class PuzzleFocus
    {
        /// <summary>
        /// Index of the puzzle to show, or -1 if all are solved. <paramref name="states"/> and
        /// <paramref name="kinds"/> hold one entry per puzzle in the gate's order.
        /// </summary>
        public static int IndexToShow(IReadOnlyList<PuzzleState> states, IReadOnlyList<PuzzleDisplayKind> kinds)
        {
            int first = -1;
            for (int i = 0; i < states.Count; i++)
            {
                if (states[i] == PuzzleState.Solved)
                {
                    continue;
                }

                if (kinds[i] == PuzzleDisplayKind.Countdown)
                {
                    return i;
                }

                if (first < 0)
                {
                    first = i;
                }
            }

            return first;
        }
    }
}
