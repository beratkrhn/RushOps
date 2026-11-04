using NUnit.Framework;
using RushOps.Enemies;

namespace RushOps.Tests.EditMode
{
    public class EnemyDeathMathTests
    {
        [Test]
        public void TotalDuration_IsAnimationPlusFade()
        {
            Assert.AreEqual(0.75f, EnemyDeathMath.TotalDuration(0.5f, 0.25f), 1e-5f);
            Assert.AreEqual(0.25f, EnemyDeathMath.TotalDuration(0f, 0.25f), 1e-5f);
            Assert.AreEqual(0f, EnemyDeathMath.TotalDuration(-1f, -1f), 1e-5f);
        }

        [TestCase(0f, 0f)]
        [TestCase(0.49f, 0f)]
        [TestCase(0.625f, 0.5f)]
        [TestCase(0.75f, 1f)]
        [TestCase(2f, 1f)]
        public void FadeProgress_StartsAfterTheAnimation(float dyingTime, float expected)
        {
            Assert.AreEqual(expected, EnemyDeathMath.FadeProgress(dyingTime, 0.5f, 0.25f), 1e-5f);
        }

        [Test]
        public void FadeProgress_WithoutFade_IsDoneWhenTheAnimationEnds()
        {
            Assert.AreEqual(0f, EnemyDeathMath.FadeProgress(0.4f, 0.5f, 0f));
            Assert.AreEqual(1f, EnemyDeathMath.FadeProgress(0.5f, 0.5f, 0f));
        }
    }
}
