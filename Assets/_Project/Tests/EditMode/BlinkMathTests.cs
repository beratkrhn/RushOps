using NUnit.Framework;
using RushOps.Combat;

namespace RushOps.Tests.EditMode
{
    public class BlinkMathTests
    {
        [TestCase(0f, false)]
        [TestCase(0.05f, false)]
        [TestCase(0.15f, true)]
        [TestCase(0.25f, false)]
        [TestCase(0.35f, true)]
        public void IsVisible_AlternatesPerInterval_StartingHidden(float elapsed, bool expected)
        {
            Assert.AreEqual(expected, BlinkMath.IsVisible(elapsed, 0.1f));
        }

        [Test]
        public void IsVisible_NoInterval_AlwaysVisible()
        {
            Assert.IsTrue(BlinkMath.IsVisible(0.05f, 0f));
        }
    }
}
