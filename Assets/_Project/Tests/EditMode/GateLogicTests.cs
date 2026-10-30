using NUnit.Framework;
using RushOps.Levels;

namespace RushOps.Tests.EditMode
{
    public class GateLogicTests
    {
        private class FakeCondition : IGateCondition
        {
            public bool IsMet { get; set; }
        }

        [Test]
        public void New_IsClosed()
        {
            var gate = new GateLogic(new FakeCondition(), 1f);

            Assert.AreEqual(GateState.Closed, gate.State);
            Assert.AreEqual(0f, gate.OpenProgress);
        }

        [Test]
        public void Tick_ConditionNotMet_StaysClosed()
        {
            var gate = new GateLogic(new FakeCondition(), 1f);

            gate.Tick(10f);

            Assert.AreEqual(GateState.Closed, gate.State);
        }

        [Test]
        public void Tick_WithoutCondition_StaysClosed()
        {
            var gate = new GateLogic(null, 1f);

            gate.Tick(10f);

            Assert.AreEqual(GateState.Closed, gate.State);
        }

        [Test]
        public void Tick_ConditionMet_StartsOpeningAndRaisesOpening()
        {
            var condition = new FakeCondition();
            var gate = new GateLogic(condition, 1f);
            int opening = 0;
            gate.Opening += () => opening++;

            condition.IsMet = true;
            gate.Tick(0.1f);

            Assert.AreEqual(GateState.Opening, gate.State);
            Assert.AreEqual(1, opening);
        }

        [Test]
        public void Tick_Opening_ProgressRisesUntilOpen()
        {
            var condition = new FakeCondition { IsMet = true };
            var gate = new GateLogic(condition, 1f);
            int opened = 0;
            gate.Opened += () => opened++;
            gate.Tick(0f);

            gate.Tick(0.4f);
            Assert.AreEqual(0.4f, gate.OpenProgress, 1e-5f);
            Assert.AreEqual(GateState.Opening, gate.State);

            gate.Tick(0.6f);
            Assert.AreEqual(GateState.Open, gate.State);
            Assert.AreEqual(1f, gate.OpenProgress);
            Assert.AreEqual(1, opened);
        }

        [Test]
        public void Tick_Open_StaysOpenWhenConditionFails()
        {
            var condition = new FakeCondition { IsMet = true };
            var gate = new GateLogic(condition, 0.5f);
            int opened = 0;
            gate.Opened += () => opened++;
            gate.Tick(0f);
            gate.Tick(0.5f);

            condition.IsMet = false;
            gate.Tick(1f);

            Assert.AreEqual(GateState.Open, gate.State);
            Assert.AreEqual(1, opened);
        }

        [Test]
        public void Tick_ZeroDuration_OpensAtOnce()
        {
            var gate = new GateLogic(new FakeCondition { IsMet = true }, 0f);
            int opening = 0;
            int opened = 0;
            gate.Opening += () => opening++;
            gate.Opened += () => opened++;

            gate.Tick(0.016f);

            Assert.AreEqual(GateState.Open, gate.State);
            Assert.AreEqual(1, opening);
            Assert.AreEqual(1, opened);
        }
    }
}
