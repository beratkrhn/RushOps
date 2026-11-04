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
    
        [TestCase(0f, 0)]
        [TestCase(0.09f, 0)]
        [TestCase(0.11f, 1)]
        [TestCase(0.45f, 4)]
        [TestCase(10f, 4)]
        public void OneShotFrameIndex_HoldsLastFrame(float time, int expected)
        {
            Assert.AreEqual(expected, FrameAnimationMath.OneShotFrameIndex(time, 10f, 5));
        }

        [Test]
        public void OneShotFrameIndex_SingleFrame_ReturnsZero()
        {
            Assert.AreEqual(0, FrameAnimationMath.OneShotFrameIndex(3f, 10f, 1));
        }

        [TestCase(10f, 5, 0.5f)]
        [TestCase(12f, 2, 1f / 6f)]
        [TestCase(0f, 5, 0f)]
        [TestCase(10f, 0, 0f)]
        public void Duration_IsFramesOverRate(float framesPerSecond, int frameCount, float expected)
        {
            Assert.AreEqual(expected, FrameAnimationMath.Duration(framesPerSecond, frameCount), 1e-5f);
        }
    }
}
