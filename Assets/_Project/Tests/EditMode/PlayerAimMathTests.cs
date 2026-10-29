using NUnit.Framework;
using RushOps.Player;
using UnityEngine;

namespace RushOps.Tests.EditMode
{
    public class PlayerAimMathTests
    {
        private const float Deadzone = 0.3f;

        [Test]
        public void IsAiming_InsideDeadzone_False()
        {
            Assert.IsFalse(PlayerAimMath.IsAiming(new Vector2(0.2f, 0f), Deadzone));
        }

        [Test]
        public void IsAiming_BeyondDeadzone_True()
        {
            Assert.IsTrue(PlayerAimMath.IsAiming(new Vector2(0f, -0.5f), Deadzone));
        }

        [Test]
        public void IsAiming_ZeroInputWithoutDeadzone_False()
        {
            Assert.IsFalse(PlayerAimMath.IsAiming(Vector2.zero, 0f));
        }

        [Test]
        public void FacingVector_Aiming_FollowsAimNotMove()
        {
            Vector2 result = PlayerAimMath.FacingVector(Vector2.right, Vector2.up, Deadzone);

            Assert.AreEqual(Vector2.up, result);
        }

        [Test]
        public void FacingVector_NotAiming_FollowsMove()
        {
            Vector2 result = PlayerAimMath.FacingVector(Vector2.left, new Vector2(0.1f, 0f), Deadzone);

            Assert.AreEqual(Vector2.left, result);
        }

        [Test]
        public void PointerAim_TargetAway_ReturnsUnitVectorTowardsTarget()
        {
            Vector2 result = PlayerAimMath.PointerAim(new Vector2(1f, 1f), new Vector2(1f, 5f));

            Assert.AreEqual(0f, Vector2.Distance(Vector2.up, result), 1e-5f);
        }

        [Test]
        public void PointerAim_SamePoint_ReturnsZero()
        {
            Assert.AreEqual(Vector2.zero, PlayerAimMath.PointerAim(Vector2.one, Vector2.one));
        }
    }
}
