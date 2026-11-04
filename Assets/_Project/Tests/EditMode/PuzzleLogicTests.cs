using System.Collections.Generic;
using NUnit.Framework;
using RushOps.Puzzles;

namespace RushOps.Tests.EditMode
{
    public class PuzzleLogicTests
    {
        private PuzzleLogic _logic;
        private List<PuzzleState> _changes;

        [SetUp]
        public void CreateLogic()
        {
            _logic = new PuzzleLogic();
            _changes = new List<PuzzleState>();
            _logic.StateChanged += _changes.Add;
        }

        [Test]
        public void Start_IsInactive()
        {
            Assert.AreEqual(PuzzleState.Inactive, _logic.State);
        }

        [Test]
        public void Activate_FromInactive_BecomesActiveOnce()
        {
            int activated = 0;
            _logic.Activated += () => activated++;

            Assert.IsTrue(_logic.Activate());
            Assert.IsFalse(_logic.Activate());

            Assert.AreEqual(PuzzleState.Active, _logic.State);
            Assert.AreEqual(1, activated);
        }

        [Test]
        public void Solve_WhileInactive_DoesNothing()
        {
            Assert.IsFalse(_logic.Solve());
            Assert.AreEqual(PuzzleState.Inactive, _logic.State);
            Assert.IsEmpty(_changes);
        }

        [Test]
        public void Solve_WhileActive_BecomesSolvedOnce()
        {
            int solved = 0;
            _logic.Solved += () => solved++;
            _logic.Activate();

            Assert.IsTrue(_logic.Solve());
            Assert.IsFalse(_logic.Solve());

            Assert.AreEqual(PuzzleState.Solved, _logic.State);
            Assert.AreEqual(1, solved);
        }

        [Test]
        public void Solved_StaysSolved()
        {
            _logic.Activate();
            _logic.Solve();

            Assert.IsFalse(_logic.Activate());
            Assert.AreEqual(PuzzleState.Solved, _logic.State);
        }

        [Test]
        public void StateChanged_ReportsEachChangeInOrder()
        {
            _logic.Activate();
            _logic.Solve();

            CollectionAssert.AreEqual(new[] { PuzzleState.Active, PuzzleState.Solved }, _changes);
        }
    }
}
