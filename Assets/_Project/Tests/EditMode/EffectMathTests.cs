using NUnit.Framework;
using RushOps.Feel;

namespace RushOps.Tests.EditMode
{
    public class EffectMathTests
    {
        [TestCase(0f, 0)]
        [TestCase(0.039f, 0)]
        [TestCase(0.041f, 1)]
        [TestCase(0.119f, 2)]
        [TestCase(0.121f, -1)]
        [TestCase(-1f, 0)]
        public void FrameAt_PlaysOnceThenEnds(float elapsed, int expected)
        {
            Assert.AreEqual(expected, EffectMath.FrameAt(elapsed, 0.04f, 3));
        }

        [Test]
        public void FrameAt_NoFramesOrDuration_IsOver()
        {
            Assert.AreEqual(-1, EffectMath.FrameAt(0f, 0.04f, 0));
            Assert.AreEqual(-1, EffectMath.FrameAt(0f, 0f, 3));
        }

        [TestCase(0f, 2f)]
        [TestCase(0.5f, 0.5f)]
        [TestCase(1f, 0f)]
        [TestCase(3f, 0f)]
        public void LightFade_FadesQuadraticallyFromPeak(float elapsed, float expected)
        {
            Assert.AreEqual(expected, EffectMath.LightFade(elapsed, 1f, 2f), 1e-5f);
        }
    }
}
