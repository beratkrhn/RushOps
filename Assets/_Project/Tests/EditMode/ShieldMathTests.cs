using NUnit.Framework;
using RushOps.Perks;
using UnityEngine;

namespace RushOps.Tests.EditMode
{
    public class ShieldMathTests
    {
        [TestCase(8f)]
        [TestCase(2.01f)]
        public void IsBarrierVisible_BeforeWarning_AlwaysVisible(float remaining)
        {
            Assert.IsTrue(ShieldMath.IsBarrierVisible(remaining, 2f, 0.1f));
        }

        [TestCase(0f)]
        [TestCase(-1f)]
        public void IsBarrierVisible_NoShieldLeft_Hidden(float remaining)
        {
            Assert.IsFalse(ShieldMath.IsBarrierVisible(remaining, 2f, 0.1f));
        }

        [TestCase(1.95f, false)]
        [TestCase(1.85f, true)]
        [TestCase(1.75f, false)]
        [TestCase(0.05f, true)]
        public void IsBarrierVisible_InWarning_Flickers(float remaining, bool expected)
        {
            Assert.AreEqual(expected, ShieldMath.IsBarrierVisible(remaining, 2f, 0.1f));
        }

        [Test]
        public void IsBarrierVisible_NoWarningTime_VisibleUntilEnd()
        {
            Assert.IsTrue(ShieldMath.IsBarrierVisible(0.01f, 0f, 0.1f));
        }

        [Test]
        public void PushDirection_PointsFromPlayerToOther()
        {
            Vector2 direction = ShieldMath.PushDirection(new Vector2(1f, 1f), new Vector2(4f, 5f));

            Assert.AreEqual(0.6f, direction.x, 1e-5f);
            Assert.AreEqual(0.8f, direction.y, 1e-5f);
        }

        [Test]
        public void PushDirection_SamePoint_PushesUp()
        {
            Assert.AreEqual(Vector2.up, ShieldMath.PushDirection(Vector2.one, Vector2.one));
        }
    }
}
