using NUnit.Framework;
using RushOps.Arcade;
using UnityEngine;

namespace RushOps.Tests.EditMode
{
    public class ArcadeStickMathTests
    {
        [TestCase(1f, 0f, 1)]
        [TestCase(1f, 1f, 2)]
        [TestCase(0f, 1f, 3)]
        [TestCase(-1f, 1f, 4)]
        [TestCase(-1f, 0f, 5)]
        [TestCase(-1f, -1f, 6)]
        [TestCase(0f, -1f, 7)]
        [TestCase(1f, -1f, 8)]
        [TestCase(1f, -0.3f, 1)]
        [TestCase(0.9f, -0.6f, 8)]
        public void Step_CutsTheValueIntoEightDirections(float x, float y, int expected)
        {
            Assert.AreEqual(expected, ArcadeStickMath.Step(new Vector2(x, y), 0.2f, 8));
        }

        [Test]
        public void Step_BelowTheThreshold_Rests()
        {
            Assert.AreEqual(0, ArcadeStickMath.Step(Vector2.zero, 0.2f, 8));
            Assert.AreEqual(0, ArcadeStickMath.Step(new Vector2(0.15f, 0.1f), 0.2f, 8));
            Assert.AreEqual(1, ArcadeStickMath.Step(new Vector2(0.25f, 0f), 0.2f, 8));
        }

        [Test]
        public void Step_WithFourSteps_UsesTheMainDirections()
        {
            Assert.AreEqual(1, ArcadeStickMath.Step(new Vector2(1f, 0.3f), 0.2f, 4));
            Assert.AreEqual(2, ArcadeStickMath.Step(new Vector2(0.3f, 1f), 0.2f, 4));
            Assert.AreEqual(4, ArcadeStickMath.Step(new Vector2(0f, -1f), 0.2f, 4));
        }

        [Test]
        public void Opposite_PointsTheOtherWay()
        {
            Assert.AreEqual(5, ArcadeStickMath.Opposite(1, 8));
            Assert.AreEqual(1, ArcadeStickMath.Opposite(5, 8));
            Assert.AreEqual(6, ArcadeStickMath.Opposite(2, 8));
            Assert.AreEqual(3, ArcadeStickMath.Opposite(1, 4));
            Assert.AreEqual(0, ArcadeStickMath.Opposite(0, 8));
        }

        [Test]
        public void SpriteIndex_MapsStepsOntoTheEightSprites()
        {
            Assert.AreEqual(0, ArcadeStickMath.SpriteIndex(0, 8));
            Assert.AreEqual(3, ArcadeStickMath.SpriteIndex(3, 8));
            Assert.AreEqual(3, ArcadeStickMath.SpriteIndex(2, 4));
            Assert.AreEqual(7, ArcadeStickMath.SpriteIndex(4, 4));
        }
    }
}
