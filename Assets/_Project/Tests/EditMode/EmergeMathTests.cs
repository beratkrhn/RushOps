using NUnit.Framework;
using RushOps.Enemies;

namespace RushOps.Tests.EditMode
{
    /// <summary>Crawling out of a breach (issue #313, F-END-15).</summary>
    public class EmergeMathTests
    {
        [TestCase(0f, 0f)]
        [TestCase(0.45f, 0.5f)]
        [TestCase(0.9f, 1f)]
        [TestCase(5f, 1f)]
        public void Progress_RunsFromZeroToOne(float elapsed, float expected)
        {
            Assert.AreEqual(expected, EmergeMath.Progress(elapsed, 0.9f), 1e-5f);
        }

        [Test]
        public void Progress_WithoutDuration_IsDoneAtOnce()
        {
            Assert.AreEqual(1f, EmergeMath.Progress(0f, 0f));
        }

        [Test]
        public void Scale_RisesFromTheStartHeightToFull()
        {
            Assert.AreEqual(EmergeMath.StartScale, EmergeMath.Scale(0f), 1e-5f);
            Assert.AreEqual(1f, EmergeMath.Scale(1f), 1e-5f);
            Assert.Greater(EmergeMath.Scale(0.6f), EmergeMath.Scale(0.3f));
        }

        [Test]
        public void Alpha_FadesInAndStaysInRange()
        {
            Assert.AreEqual(EmergeMath.StartAlpha, EmergeMath.Alpha(0f), 1e-5f);
            Assert.AreEqual(1f, EmergeMath.Alpha(1f), 1e-5f);
            Assert.AreEqual(1f, EmergeMath.Alpha(4f), 1e-5f);
            Assert.AreEqual(EmergeMath.StartAlpha, EmergeMath.Alpha(-1f), 1e-5f);
        }
    }
}
