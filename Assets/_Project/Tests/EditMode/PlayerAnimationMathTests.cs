using NUnit.Framework;
using RushOps.Characters;
using RushOps.Player;

namespace RushOps.Tests.EditMode
{
    public class PlayerAnimationMathTests
    {
        [TestCase(0f, 0f)]
        [TestCase(0.5f, 0f)]
        [TestCase(2.5f, 0.5f)]
        [TestCase(5f, 1f)]
        [TestCase(20f, 1.6f)]
        public void WalkRate_FollowsSpeedWithThresholdAndCap(float speed, float expected)
        {
            Assert.AreEqual(expected, PlayerAnimationMath.WalkRate(speed, 5f, 0.15f, 1.6f), 1e-5f);
        }

        [Test]
        public void WalkRate_NoBaseSpeed_Stands()
        {
            Assert.AreEqual(0f, PlayerAnimationMath.WalkRate(3f, 0f, 0.15f, 1.6f));
        }

        [Test]
        public void State_SetMoving_RestartsLoopOnlyOnChange()
        {
            var state = new SpriteAnimationState();
            state.Tick(0.3f, 0.1f);

            state.SetMoving(true);
            Assert.IsTrue(state.IsMoving);
            Assert.AreEqual(0f, state.Time);

            state.Tick(0.2f, 0.1f);
            state.SetMoving(true);
            Assert.AreEqual(0.2f, state.Time, 1e-5f);
        }
    }
}
