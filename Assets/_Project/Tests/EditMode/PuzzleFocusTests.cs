using NUnit.Framework;
using RushOps.Puzzles;

namespace RushOps.Tests.EditMode
{
    public class PuzzleFocusTests
    {
        private static readonly PuzzleDisplayKind[] HoldThenSwitches =
            { PuzzleDisplayKind.Progress, PuzzleDisplayKind.Text };

        [Test]
        public void NothingRunning_ShowsTheFirstUnsolved()
        {
            var states = new[] { PuzzleState.Active, PuzzleState.Active };

            Assert.AreEqual(0, PuzzleFocus.IndexToShow(states, HoldThenSwitches));
        }

        [Test]
        public void RunningCountdown_WinsOverAnEarlierPuzzle()
        {
            var states = new[] { PuzzleState.Active, PuzzleState.Active };
            var kinds = new[] { PuzzleDisplayKind.Progress, PuzzleDisplayKind.Countdown };

            Assert.AreEqual(1, PuzzleFocus.IndexToShow(states, kinds));
        }

        [Test]
        public void SolvedPuzzles_AreSkipped()
        {
            var states = new[] { PuzzleState.Solved, PuzzleState.Active };

            Assert.AreEqual(1, PuzzleFocus.IndexToShow(states, HoldThenSwitches));
        }

        [Test]
        public void AllSolved_ShowsNone()
        {
            var states = new[] { PuzzleState.Solved, PuzzleState.Solved };

            Assert.AreEqual(-1, PuzzleFocus.IndexToShow(states, HoldThenSwitches));
        }
    }
}
