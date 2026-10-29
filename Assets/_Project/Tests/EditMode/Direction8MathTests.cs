using NUnit.Framework;
using RushOps.Characters;
using UnityEngine;

namespace RushOps.Tests.EditMode
{
    public class Direction8MathTests
    {
        [TestCase(1f, 0f, Direction8.E)]
        [TestCase(1f, 1f, Direction8.NE)]
        [TestCase(0f, 1f, Direction8.N)]
        [TestCase(-1f, 1f, Direction8.NW)]
        [TestCase(-1f, 0f, Direction8.W)]
        [TestCase(-1f, -1f, Direction8.SW)]
        [TestCase(0f, -1f, Direction8.S)]
        [TestCase(1f, -1f, Direction8.SE)]
        public void FromVector_MainDirections(float x, float y, Direction8 expected)
        {
            Assert.AreEqual(expected, Direction8Math.FromVector(new Vector2(x, y), Direction8.S));
        }

        [TestCase(22.4f, Direction8.E)]
        [TestCase(22.6f, Direction8.NE)]
        [TestCase(-22.4f, Direction8.E)]
        [TestCase(-22.6f, Direction8.SE)]
        [TestCase(179f, Direction8.W)]
        [TestCase(-179f, Direction8.W)]
        [TestCase(250f, Direction8.S)]
        public void FromVector_SectorsAre45DegreesAroundEachDirection(float degrees, Direction8 expected)
        {
            float radians = degrees * Mathf.Deg2Rad;
            var vector = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));

            Assert.AreEqual(expected, Direction8Math.FromVector(vector, Direction8.S));
        }

        [Test]
        public void FromVector_IgnoresMagnitude()
        {
            Assert.AreEqual(Direction8.NE, Direction8Math.FromVector(new Vector2(0.01f, 0.01f), Direction8.S));
            Assert.AreEqual(Direction8.NE, Direction8Math.FromVector(new Vector2(500f, 500f), Direction8.S));
        }

        [Test]
        public void FromVector_ZeroVector_ReturnsFallback()
        {
            Assert.AreEqual(Direction8.NW, Direction8Math.FromVector(Vector2.zero, Direction8.NW));
        }

        [Test]
        public void ToVector_RoundTripsForAllDirections()
        {
            for (int d = 0; d < Direction8Math.Count; d++)
            {
                var direction = (Direction8)d;
                Vector2 vector = Direction8Math.ToVector(direction);

                Assert.AreEqual(1f, vector.magnitude, 1e-5f);
                Assert.AreEqual(direction, Direction8Math.FromVector(vector, Direction8.S));
            }
        }
    }
}
