using NUnit.Framework;
using RushOps.Characters;

namespace RushOps.Tests.EditMode
{
    public class FrameAnimationMathTests
    {
        [TestCase(0f, 0)]
        [TestCase(0.124f, 0)]
        [TestCase(0.126f, 1)]
        [TestCase(0.375f, 3)]
        [TestCase(0.5f, 0)]
        [TestCase(0.76f, 2)]
        public void FrameIndex_LoopsAtFrameRate(float time, int expected)
        {
            Assert.AreEqual(expected, FrameAnimationMath.FrameIndex(time, 8f, 4));
        }

        [TestCase(0f, 4)]
        [TestCase(-1f, 4)]
        public void FrameIndex_NoPositiveFrameRate_ReturnsFirstFrame(float framesPerSecond, int frameCount)
        {
            Assert.AreEqual(0, FrameAnimationMath.FrameIndex(3.3f, framesPerSecond, frameCount));
        }

        [TestCase(0)]
        [TestCase(1)]
        public void FrameIndex_SingleOrNoFrame_ReturnsZero(int frameCount)
        {
            Assert.AreEqual(0, FrameAnimationMath.FrameIndex(3.3f, 8f, frameCount));
        }
    }
}
