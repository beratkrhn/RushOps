using NUnit.Framework;
using RushOps.Player;
using UnityEngine;

namespace RushOps.Tests.EditMode
{
    public class PlayerDashMathTests
    {
        [Test]
        public void Direction_Moving_UsesMoveDirection()
        {
            Vector2 direction = PlayerDashMath.Direction(new Vector2(0.3f, 0.3f), Vector2.left);

            Assert.AreEqual(new Vector2(1f, 1f).normalized.x, direction.x, 1e-5f);
            Assert.AreEqual(new Vector2(1f, 1f).normalized.y, direction.y, 1e-5f);
        }

        [Test]
        public void Direction_Standing_UsesFacing()
        {
            Vector2 direction = PlayerDashMath.Direction(Vector2.zero, new Vector2(0f, -2f));

            Assert.AreEqual(Vector2.down, direction);
        }

        [Test]
        public void Direction_NothingKnown_ReturnsZero()
        {
            Assert.AreEqual(Vector2.zero, PlayerDashMath.Direction(Vector2.zero, Vector2.zero));
        }

        [Test]
        public void Speed_CoversDistanceInDuration()
        {
            Assert.AreEqual(15f, PlayerDashMath.Speed(3f, 0.2f), 1e-4f);
        }

        [Test]
        public void Speed_ZeroDuration_ReturnsZero()
        {
            Assert.AreEqual(0f, PlayerDashMath.Speed(3f, 0f));
        }
    }
}
