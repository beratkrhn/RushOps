using NUnit.Framework;
using RushOps.Combat;
using UnityEngine;

namespace RushOps.Tests.EditMode
{
    public class AimAssistMathTests
    {
        private const float Angle = 10f;
        private const float Range = 8f;

        private static Vector2 At(float degrees, float distance)
        {
            float radians = degrees * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)) * distance;
        }

        [Test]
        public void Pick_TargetInsideCone_IsChosen()
        {
            Vector2[] targets = { At(8f, 5f) };

            Assert.AreEqual(0, AimAssistMath.Pick(Vector2.zero, Vector2.right, targets, 1, Angle, Range));
        }

        [Test]
        public void Pick_TargetOutsideCone_IsIgnored()
        {
            Vector2[] targets = { At(12f, 5f), At(-15f, 3f) };

            Assert.AreEqual(-1, AimAssistMath.Pick(Vector2.zero, Vector2.right, targets, 2, Angle, Range));
        }

        [Test]
        public void Pick_TargetOutOfRange_IsIgnored()
        {
            Vector2[] targets = { At(0f, 9f) };

            Assert.AreEqual(-1, AimAssistMath.Pick(Vector2.zero, Vector2.right, targets, 1, Angle, Range));
        }

        [Test]
        public void Pick_TargetBehind_IsIgnored()
        {
            Vector2[] targets = { At(180f, 2f) };

            Assert.AreEqual(-1, AimAssistMath.Pick(Vector2.zero, Vector2.right, targets, 1, Angle, Range));
        }

        [Test]
        public void Pick_SeveralInCone_ChoosesNearest()
        {
            Vector2[] targets = { At(1f, 6f), At(-9f, 2f), At(5f, 4f) };

            Assert.AreEqual(1, AimAssistMath.Pick(Vector2.zero, Vector2.right, targets, 3, Angle, Range));
        }

        [Test]
        public void Pick_IgnoresEntriesBeyondCount()
        {
            Vector2[] targets = { At(0f, 6f), At(0f, 1f) };

            Assert.AreEqual(0, AimAssistMath.Pick(Vector2.zero, Vector2.right, targets, 1, Angle, Range));
        }

        [Test]
        public void Pick_WorksFromAnyOrigin()
        {
            var origin = new Vector2(10f, 20f);
            Vector2[] targets = { origin + At(95f, 3f) };

            Assert.AreEqual(0, AimAssistMath.Pick(origin, Vector2.up, targets, 1, Angle, Range));
        }

        [Test]
        public void Pick_NoAimOrNoCone_ChoosesNothing()
        {
            Vector2[] targets = { At(0f, 3f) };

            Assert.AreEqual(-1, AimAssistMath.Pick(Vector2.zero, Vector2.zero, targets, 1, Angle, Range));
            Assert.AreEqual(-1, AimAssistMath.Pick(Vector2.zero, Vector2.right, targets, 1, 0f, Range));
        }

        [Test]
        public void Adjust_PointsAtChosenTarget()
        {
            Vector2[] targets = { new Vector2(4f, 0.5f) };

            Vector2 direction = AimAssistMath.Adjust(Vector2.zero, Vector2.right, targets, 1, Angle, Range);

            Assert.AreEqual(new Vector2(4f, 0.5f).normalized.x, direction.x, 1e-5f);
            Assert.AreEqual(new Vector2(4f, 0.5f).normalized.y, direction.y, 1e-5f);
        }

        [Test]
        public void Adjust_NoTarget_KeepsAim()
        {
            Vector2 direction = AimAssistMath.Adjust(Vector2.zero, new Vector2(0f, 2f), new Vector2[0], 0, Angle, Range);

            Assert.AreEqual(Vector2.up, direction);
        }
    }
}
