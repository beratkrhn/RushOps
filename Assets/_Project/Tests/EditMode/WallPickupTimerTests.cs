using NUnit.Framework;
using RushOps.Combat;

namespace RushOps.Tests.EditMode
{
    public class WallPickupTimerTests
    {
        [Test]
        public void StandingInFront_FillsTheRing_AndPicksUpAfterTheTime()
        {
            // Decision W8: about 0.5 s in front of the weapon.
            var timer = new WallPickupTimer(0.5f);

            Assert.IsFalse(timer.Tick(0.25f, true, true));
            Assert.AreEqual(0.5f, timer.Progress, 1e-5f);
            Assert.IsFalse(timer.Tick(0.2f, true, true));
            Assert.IsTrue(timer.Tick(0.05f, true, true));
            Assert.AreEqual(0f, timer.Progress, "the ring starts empty again");
        }

        [Test]
        public void SteppingAway_EmptiesTheRing()
        {
            var timer = new WallPickupTimer(0.5f);
            timer.Tick(0.4f, true, true);

            Assert.IsFalse(timer.Tick(0.01f, false, true));
            Assert.AreEqual(0f, timer.Progress);
            Assert.IsFalse(timer.Tick(0.4f, true, true), "the time starts over");
        }

        [Test]
        public void WithTheWeaponInHand_NothingHappens()
        {
            var timer = new WallPickupTimer(0.5f);

            Assert.IsFalse(timer.Tick(2f, true, false));
            Assert.AreEqual(0f, timer.Progress);
        }

        [Test]
        public void ZeroTime_PicksUpAtOnce()
        {
            Assert.IsTrue(new WallPickupTimer(0f).Tick(0.01f, true, true));
        }
    }
}
