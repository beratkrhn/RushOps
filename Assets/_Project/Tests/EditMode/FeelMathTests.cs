using NUnit.Framework;
using RushOps.Cameras;
using RushOps.Feel;
using UnityEngine;

namespace RushOps.Tests.EditMode
{
    public class FeelMathTests
    {
        [Test]
        public void ShakeOffset_DuringShake_StaysWithinStrength()
        {
            bool moved = false;
            for (float t = 0f; t < 0.2f; t += 0.01f)
            {
                Vector2 offset = CameraFollowMath.ShakeOffset(t, 0.2f, 0.15f, 25f);
                Assert.LessOrEqual(Mathf.Abs(offset.x), 0.15f + 1e-5f);
                Assert.LessOrEqual(Mathf.Abs(offset.y), 0.15f + 1e-5f);
                moved |= offset.sqrMagnitude > 1e-6f;
            }

            Assert.IsTrue(moved);
        }

        [TestCase(0.2f)]
        [TestCase(1f)]
        [TestCase(-0.1f)]
        public void ShakeOffset_OutsideShake_IsZero(float elapsed)
        {
            Assert.AreEqual(Vector2.zero, CameraFollowMath.ShakeOffset(elapsed, 0.2f, 0.15f, 25f));
        }

        [Test]
        public void ShakeOffset_FadesOut()
        {
            float early = 0f;
            float late = 0f;
            for (int i = 0; i < 20; i++)
            {
                early = Mathf.Max(early, CameraFollowMath.ShakeOffset(0.001f * i, 1f, 1f, 25f).magnitude);
                late = Mathf.Max(late, CameraFollowMath.ShakeOffset(0.9f + 0.001f * i, 1f, 1f, 25f).magnitude);
            }

            Assert.Less(late, 0.05f);
            Assert.Greater(early, late);
        }

        [TestCase(0f, 1f)]
        [TestCase(0.07f, 1f)]
        [TestCase(0.08f, 0f)]
        [TestCase(-1f, 0f)]
        public void FlashAmount_FullUntilDuration(float elapsed, float expected)
        {
            Assert.AreEqual(expected, FeelMath.FlashAmount(elapsed, 0.08f));
        }

        [TestCase(0f, 1f)]
        [TestCase(0.05f, 0.5f)]
        [TestCase(0.1f, 0f)]
        public void KnockbackFactor_EasesOut(float elapsed, float expected)
        {
            Assert.AreEqual(expected, FeelMath.KnockbackFactor(elapsed, 0.1f), 1e-5f);
        }

        [TestCase(0f, 0.94f)]
        [TestCase(0.5f, 1f)]
        [TestCase(1f, 1.06f)]
        public void Pitch_VariesAroundOne(float random, float expected)
        {
            Assert.AreEqual(expected, FeelMath.Pitch(random, 0.06f), 1e-5f);
        }
    }
}
