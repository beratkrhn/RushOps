using NUnit.Framework;
using RushOps.Levels;

namespace RushOps.Tests.EditMode
{
    public class AllConditionsTests
    {
        private sealed class Flag : IGateCondition
        {
            public bool IsMet { get; set; }
        }

        [Test]
        public void MetOnlyWhenAllAreMet()
        {
            var kills = new Flag();
            var puzzle = new Flag();
            var all = new AllConditions(new IGateCondition[] { kills, puzzle });

            Assert.IsFalse(all.IsMet);
            kills.IsMet = true;
            Assert.IsFalse(all.IsMet, "puzzle still open");
            puzzle.IsMet = true;
            Assert.IsTrue(all.IsMet);
        }

        [Test]
        public void EmptyOrMissing_NeverMet()
        {
            Assert.IsFalse(new AllConditions(null).IsMet);
            Assert.IsFalse(new AllConditions(new IGateCondition[0]).IsMet);
            Assert.IsFalse(new AllConditions(new IGateCondition[] { new Flag { IsMet = true }, null }).IsMet);
        }

        [Test]
        public void KillGoalAndPuzzle_OpenGateTogether()
        {
            var counter = new KillCounter();
            var puzzle = new Flag();
            var gate = new GateLogic(new AllConditions(new IGateCondition[] { new KillGoal(counter, 1), puzzle }), 0f);
            counter.Register();
            gate.Tick(0.1f);
            Assert.AreEqual(GateState.Closed, gate.State);

            puzzle.IsMet = true;
            gate.Tick(0.1f);

            Assert.AreEqual(GateState.Open, gate.State);
        }
    }
}
