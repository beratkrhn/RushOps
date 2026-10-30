using NUnit.Framework;
using RushOps.Levels;

namespace RushOps.Tests.EditMode
{
    public class ZoneLogicTests
    {
        [Test]
        public void New_IsInactive()
        {
            Assert.AreEqual(ZoneState.Inactive, new ZoneLogic().State);
        }

        [Test]
        public void Enter_Inactive_ActivatesOnceAndRaisesActivated()
        {
            var zone = new ZoneLogic();
            int activated = 0;
            zone.Activated += () => activated++;

            Assert.IsTrue(zone.Enter());
            Assert.IsFalse(zone.Enter());

            Assert.AreEqual(ZoneState.Active, zone.State);
            Assert.AreEqual(1, activated);
        }

        [Test]
        public void Complete_Active_CompletesOnceAndRaisesCompleted()
        {
            var zone = new ZoneLogic();
            int completed = 0;
            zone.Completed += () => completed++;
            zone.Enter();

            Assert.IsTrue(zone.Complete());
            Assert.IsFalse(zone.Complete());

            Assert.AreEqual(ZoneState.Completed, zone.State);
            Assert.AreEqual(1, completed);
        }

        [Test]
        public void Enter_AfterComplete_StaysCompletedWithoutActivated()
        {
            var zone = new ZoneLogic();
            bool activated = false;
            zone.Activated += () => activated = true;
            zone.Complete();

            Assert.IsFalse(zone.Enter());

            Assert.AreEqual(ZoneState.Completed, zone.State);
            Assert.IsFalse(activated);
        }
    }
}
