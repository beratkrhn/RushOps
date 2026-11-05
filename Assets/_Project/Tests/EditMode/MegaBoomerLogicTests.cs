using NUnit.Framework;
using RushOps.Bosses;

namespace RushOps.Tests.EditMode
{
    public class MegaBoomerLogicTests
    {
        private const float Interval = 2.5f;
        private const float SplitShare = 0.5f;
        private const float SplitTime = 0.7f;

        [Test]
        public void Walking_LeavesAPuddle_EveryInterval()
        {
            var logic = new MegaBoomerLogic(Interval, SplitShare, SplitTime, true);

            Assert.AreEqual(MegaBoomerEvent.None, logic.Tick(Interval - 0.1f, 1f));
            Assert.AreEqual(MegaBoomerEvent.Puddle, logic.Tick(0.1f, 1f));
            Assert.AreEqual(MegaBoomerEvent.None, logic.Tick(Interval - 0.2f, 1f));
            Assert.AreEqual(MegaBoomerEvent.Puddle, logic.Tick(0.2f, 1f));
        }

        [Test]
        public void AtHalfHealth_ItStopsAndSplits_AfterTheSplitTime()
        {
            var logic = new MegaBoomerLogic(Interval, SplitShare, SplitTime, true);
            logic.Tick(0.1f, 0.6f);

            Assert.AreEqual(MegaBoomerEvent.SplitStarted, logic.Tick(0.1f, 0.5f), "decision C9a: at half health");
            Assert.AreEqual(MegaBoomerPhase.Split, logic.Phase);
            Assert.AreEqual(MegaBoomerEvent.None, logic.Tick(SplitTime - 0.1f, 0.5f));
            Assert.AreEqual(MegaBoomerEvent.Split, logic.Tick(0.1f, 0.5f));
            Assert.AreEqual(MegaBoomerPhase.Done, logic.Phase);
            Assert.AreEqual(MegaBoomerEvent.None, logic.Tick(10f, 0.4f), "splits only once");
        }

        [Test]
        public void WhileSplitting_NoPuddles()
        {
            var logic = new MegaBoomerLogic(0.2f, SplitShare, SplitTime, true);
            logic.Tick(0.01f, 0.4f);

            for (int i = 0; i < 6; i++)
            {
                Assert.AreNotEqual(MegaBoomerEvent.Puddle, logic.Tick(0.1f, 0.4f));
            }
        }

        [Test]
        public void AHalf_NeverSplits_ButLeavesPuddles()
        {
            var logic = new MegaBoomerLogic(Interval, SplitShare, SplitTime, false);

            Assert.AreEqual(MegaBoomerEvent.Puddle, logic.Tick(Interval, 0.1f));
            Assert.AreEqual(MegaBoomerPhase.Walk, logic.Phase);
        }

        [TestCase(181, 91, 90)]
        [TestCase(180, 90, 90)]
        [TestCase(1, 1, 1)]
        public void SplitHealth_GivesEachHalfHalfTheRest(int remaining, int first, int second)
        {
            Assert.AreEqual((first, second), MegaBoomerLogic.SplitHealth(remaining));
        }
    }
}
