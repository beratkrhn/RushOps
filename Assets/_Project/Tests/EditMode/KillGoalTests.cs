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
            // By default counted globally from level start, not from when a zone or gate became relevant.
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

        [Test]
        public void CountFromZone_CountsNothingBeforeStart()
        {
            var counter = new KillCounter();
            var goal = new KillGoal(counter, 2, countFromLevelStart: false);
            counter.Register();
            counter.Register();
            counter.Register();

            Assert.IsFalse(goal.IsCounting);
            Assert.AreEqual(0, goal.Current);
            Assert.IsFalse(goal.IsMet);
        }

        [Test]
        public void CountFromZone_CountsOnlyKillsAfterStart()
        {
            // Decision P1: a later kill gate is not already open when the player arrives.
            var counter = new KillCounter();
            var goal = new KillGoal(counter, 3, countFromLevelStart: false);
            for (int i = 0; i < 5; i++)
            {
                counter.Register();
            }

            goal.StartCounting();
            counter.Register();
            counter.Register();
            Assert.AreEqual(2, goal.Current);
            Assert.IsFalse(goal.IsMet);

            counter.Register();
            Assert.IsTrue(goal.IsMet);
        }

        [Test]
        public void StartCountingAgain_KeepsKillsCountedSoFar()
        {
            var counter = new KillCounter();
            var goal = new KillGoal(counter, 5, countFromLevelStart: false);
            goal.StartCounting();
            counter.Register();
            counter.Register();

            goal.StartCounting();

            Assert.AreEqual(2, goal.Current);
        }

        [Test]
        public void StartCounting_OnLevelStartGoal_ChangesNothing()
        {
            var counter = new KillCounter();
            var goal = new KillGoal(counter, 5);
            counter.Register();

            goal.StartCounting();

            Assert.AreEqual(1, goal.Current);
        }

        [Test]
        public void CountFromZone_WithoutCounter_StartsSafely()
        {
            var goal = new KillGoal(null, 1, countFromLevelStart: false);
            goal.StartCounting();

            Assert.IsFalse(goal.IsMet);
        }
    }
}
