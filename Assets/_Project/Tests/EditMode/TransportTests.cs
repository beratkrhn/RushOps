using NUnit.Framework;
using RushOps.Puzzles;

namespace RushOps.Tests.EditMode
{
    public class TransportTests
    {
        [Test]
        public void CarrySlot_HoldsOneItemAtATime()
        {
            var slot = new CarrySlot();
            var first = new object();

            Assert.IsTrue(slot.TryPickUp(first));
            Assert.IsFalse(slot.TryPickUp(new object()), "hands are full");
            Assert.AreSame(first, slot.Item);
        }

        [Test]
        public void CarrySlot_HandIn_FreesHands()
        {
            var slot = new CarrySlot();
            var item = new object();
            slot.TryPickUp(item);

            Assert.AreSame(item, slot.HandIn());
            Assert.IsFalse(slot.IsCarrying);
            Assert.IsNull(slot.HandIn());
        }

        [Test]
        public void CarrySlot_NullItem_NotPickedUp()
        {
            Assert.IsFalse(new CarrySlot().TryPickUp(null));
        }

        [Test]
        public void Goal_DefaultsToAtLeastOneCell()
        {
            Assert.AreEqual(1, new TransportGoal(0).Required);
        }

        [Test]
        public void Goal_CompleteAfterRequiredCells()
        {
            var goal = new TransportGoal(3);
            goal.Deliver();
            goal.Deliver();

            Assert.IsFalse(goal.IsComplete);
            Assert.AreEqual(2f / 3f, goal.Progress, 1e-5f);
            Assert.IsTrue(goal.Deliver());
            Assert.IsTrue(goal.IsComplete);
            Assert.IsFalse(goal.Deliver(), "extra cells do not count");
            Assert.AreEqual(3, goal.Delivered);
        }
    }
}
