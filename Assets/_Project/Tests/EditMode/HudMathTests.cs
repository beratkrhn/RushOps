using NUnit.Framework;
using RushOps.UI;

namespace RushOps.Tests.EditMode
{
    public class HudMathTests
    {
        [TestCase(0, 3, true)]
        [TestCase(2, 3, true)]
        [TestCase(3, 3, false)]
        [TestCase(0, 0, false)]
        public void IsHeartFull_LeftHeartsUpToCurrent(int index, int current, bool expected)
        {
            Assert.AreEqual(expected, HudMath.IsHeartFull(index, current));
        }

        [TestCase(3, true, true)]
        [TestCase(3, false, false)]
        [TestCase(4, true, true)]
        [TestCase(1, false, true)]
        public void IsHeartFullWhileFlashing_LostHeartsBlink(int index, bool redPhase, bool expected)
        {
            // From 5 to 3 hearts: hearts 3 and 4 were lost.
            Assert.AreEqual(expected, HudMath.IsHeartFullWhileFlashing(index, 3, 5, redPhase));
        }

        [TestCase(0f, true)]
        [TestCase(0.05f, true)]
        [TestCase(0.15f, false)]
        [TestCase(0.25f, true)]
        [TestCase(0.6f, false)]
        [TestCase(-1f, false)]
        public void IsFlashRed_StartsRedAndEndsAfterDuration(float sinceHit, bool expected)
        {
            Assert.AreEqual(expected, HudMath.IsFlashRed(sinceHit, 0.6f, 0.1f));
        }
    }
}
