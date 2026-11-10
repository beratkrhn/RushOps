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
        public void DogTracker_ShowsRoundAndDogsLeft_WithSingular_AndNotice()
        {
            Assert.AreEqual("Runde 6\nNoch 9 Hunde", EndlessText.DogTracker(6, 9));
            Assert.AreEqual("Runde 6\nNoch 1 Hund", EndlessText.DogTracker(6, 1));
            Assert.AreEqual("Runde 6\nNoch 0 Hunde", EndlessText.DogTracker(6, 0));
            Assert.AreEqual("Runde 6\nNoch 3 Hunde\nNeue Waffe: Schrotflinte",
                EndlessText.DogTracker(6, 3, EndlessText.NewWeapon("Schrotflinte")));
            Assert.AreEqual("Hunderunde!", EndlessText.DogRound);
        }

        [Test]
        public void NewWeapon_NamesTheWeapon_AndJoinsTheTrackerAsThirdLine()
        {
            Assert.AreEqual("Neue Waffe: Schrotflinte", EndlessText.NewWeapon("Schrotflinte"));
            Assert.AreEqual("Runde 5\nNoch 8 Gegner\nNeue Waffe: Schrotflinte",
                EndlessText.Tracker(5, 8, EndlessText.NewWeapon("Schrotflinte")));
            Assert.AreEqual(EndlessText.Tracker(5, 8), EndlessText.Tracker(5, 8, null));
            Assert.AreEqual(EndlessText.Tracker(5, 8), EndlessText.Tracker(5, 8, string.Empty));
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

        [Test]
        public void BossTracker_ShowsRoundAndTheBoss_TheNoticeTakesTheSecondLine()
        {
            Assert.AreEqual("Runde 10\nBoss besiegen", EndlessText.BossTracker(10, null));
            Assert.AreEqual("Runde 10\nNeue Waffe: Sturmgewehr",
                EndlessText.BossTracker(10, EndlessText.NewWeapon("Sturmgewehr")));
        }
    }
}
