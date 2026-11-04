using NUnit.Framework;
using RushOps.Levels;

namespace RushOps.Tests.EditMode
{
    public class IslandThemeMathTests
    {
        [Test]
        public void FloorVariant_IsDeterministicAndInRange()
        {
            for (int x = 0; x < 30; x++)
            {
                for (int y = 0; y < 30; y++)
                {
                    int variant = IslandThemeMath.FloorVariant(x, y, 3, 0.7f);
                    Assert.That(variant, Is.InRange(0, 2));
                    Assert.AreEqual(variant, IslandThemeMath.FloorVariant(x, y, 3, 0.7f));
                }
            }
        }

        [Test]
        public void FloorVariant_PlainShareRoughlyHolds()
        {
            int plain = 0;
            const int size = 60;
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    plain += IslandThemeMath.FloorVariant(x, y, 3, 0.7f) == 0 ? 1 : 0;
                }
            }

            Assert.That(plain / (float)(size * size), Is.InRange(0.6f, 0.8f));
        }

        [TestCase(0)]
        [TestCase(1)]
        public void FloorVariant_OneOrNoVariant_ReturnsZero(int count)
        {
            Assert.AreEqual(0, IslandThemeMath.FloorVariant(5, 7, count, 0.2f));
        }

        [Test]
        public void Decoration_FollowsDensity()
        {
            int decorated = 0;
            const int size = 100;
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    int kind = IslandThemeMath.Decoration(x, y, 5, 0.05f);
                    Assert.That(kind, Is.InRange(-1, 4));
                    decorated += kind >= 0 ? 1 : 0;
                }
            }

            Assert.That(decorated / (float)(size * size), Is.InRange(0.03f, 0.07f));
        }

        [Test]
        public void Decoration_NoneOrZeroDensity_ReturnsMinusOne()
        {
            Assert.AreEqual(-1, IslandThemeMath.Decoration(3, 4, 0, 0.5f));
            Assert.AreEqual(-1, IslandThemeMath.Decoration(3, 4, 5, 0f));
        }

        [TestCase(0f, 0)]
        [TestCase(0.3f, 1)]
        [TestCase(0.5f, 2)]
        [TestCase(1f, 4)]
        [TestCase(2f, 4)]
        [TestCase(-1f, 0)]
        public void GateFrame_MapsSinkToFrames(float sink, int expected)
        {
            Assert.AreEqual(expected, IslandThemeMath.GateFrame(sink, 5));
        }
    }
}
