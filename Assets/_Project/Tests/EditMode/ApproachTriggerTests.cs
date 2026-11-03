using NUnit.Framework;
using RushOps.Enemies;

namespace RushOps.Tests.EditMode
{
    public class ApproachTriggerTests
    {
        [Test]
        public void Check_OutsideDistance_DoesNotFire()
        {
            var trigger = new ApproachTrigger();
            trigger.Reset(4f);

            Assert.IsFalse(trigger.Check(16.1f));
            Assert.IsFalse(trigger.HasFired);
        }

        [Test]
        public void Check_FiresOnceWhenWithinDistance()
        {
            var trigger = new ApproachTrigger();
            trigger.Reset(4f);

            Assert.IsTrue(trigger.Check(16f));
            Assert.IsFalse(trigger.Check(1f), "fires only once per life");
            Assert.IsFalse(trigger.Check(100f));
            Assert.IsFalse(trigger.Check(1f), "leaving and coming back does not fire again");
            Assert.IsTrue(trigger.HasFired);
        }

        [Test]
        public void Reset_ArmsAgainForTheNextLife()
        {
            var trigger = new ApproachTrigger();
            trigger.Reset(4f);
            trigger.Check(1f);

            trigger.Reset(4f);

            Assert.IsFalse(trigger.HasFired);
            Assert.IsTrue(trigger.Check(1f));
        }

        [Test]
        public void Check_ZeroDistance_NeverFires()
        {
            var trigger = new ApproachTrigger();
            trigger.Reset(0f);

            Assert.IsFalse(trigger.Check(0f));
        }

        [Test]
        public void Check_BeforeReset_NeverFires()
        {
            Assert.IsFalse(new ApproachTrigger().Check(0f));
        }
    }
}
