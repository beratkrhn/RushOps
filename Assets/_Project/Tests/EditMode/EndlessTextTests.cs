using NUnit.Framework;
using RushOps.Endless;

namespace RushOps.Tests.EditMode
{
    /// <summary>Texts and start round of the endless mode (issue #207).</summary>
    public class EndlessTextTests
    {
        [TearDown]
        public void ResetStart()
        {
            EndlessStart.Round = 1;
        }

        [Test]
        public void Round_IsRundeAndTheNumber()
        {
            Assert.AreEqual("Runde 7", EndlessText.Round(7));
        }

        [Test]
        public void Tracker_ShowsRoundAndEnemiesLeft_WithSingular()
        {
            Assert.AreEqual("Runde 3\nNoch 8 Gegner", EndlessText.Tracker(3, 8));
            Assert.AreEqual("Runde 3\nNoch 1 Gegner", EndlessText.Tracker(3, 1));
            Assert.AreEqual("Runde 3\nNoch 0 Gegner", EndlessText.Tracker(3, 0));
        }

        [Test]
        public void Start_DefaultsToRoundOne_AndIsConsumedOnce()
        {
            Assert.AreEqual(1, EndlessStart.Consume());

            EndlessStart.Round = 10;
            Assert.AreEqual(10, EndlessStart.Consume());
            Assert.AreEqual(1, EndlessStart.Consume(), "the next run starts at round 1 again");
        }

        [Test]
        public void Start_BelowOneCountsAsOne()
        {
            EndlessStart.Round = -4;

            Assert.AreEqual(1, EndlessStart.Consume());
        }
    }
}
