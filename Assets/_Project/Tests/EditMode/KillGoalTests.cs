using NUnit.Framework;
using RushOps.Levels;

namespace RushOps.Tests.EditMode
{
    public class KillGoalTests
    {
        [Test]
        public void BelowRequired_IsNotMet()
        {
            var counter = new KillCounter();
            var goal = new KillGoal(counter, 3);
            counter.Register();
            counter.Register();

            Assert.IsFalse(goal.IsMet);
            Assert.AreEqual(2, goal.Current);
        }

        [Test]
        public void ReachingRequired_IsMetAndCurrentStopsAtRequired()
        {
            var counter = new KillCounter();
            var goal = new KillGoal(counter, 3);
            for (int i = 0; i < 5; i++)
            {
                counter.Register();
            }

            Assert.IsTrue(goal.IsMet);
            Assert.AreEqual(3, goal.Current);
        }

        [Test]
        public void KillsBeforeGoalWasCreated_Count()
        {
            // Counted globally from level start, not from when a zone or gate became relevant.
            var counter = new KillCounter();
            counter.Register();
            counter.Register();

            var goal = new KillGoal(counter, 2);

            Assert.IsTrue(goal.IsMet);
        }

        [Test]
        public void RequiredBelowOne_NeedsOneKill()
        {
            var counter = new KillCounter();
            var goal = new KillGoal(counter, 0);

            Assert.AreEqual(1, goal.Required);
            Assert.IsFalse(goal.IsMet);
        }

        [Test]
        public void WithoutCounter_IsNeverMet()
        {
            Assert.IsFalse(new KillGoal(null, 1).IsMet);
        }

        [Test]
        public void Gate_OpensOnceGoalReached()
        {
            var counter = new KillCounter();
            var gate = new GateLogic(new KillGoal(counter, 2), 0f);
            counter.Register();
            gate.Tick(0.1f);
            Assert.AreEqual(GateState.Closed, gate.State);

            counter.Register();
            gate.Tick(0.1f);

            Assert.AreEqual(GateState.Open, gate.State);
        }
    }
}
