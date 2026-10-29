using NUnit.Framework;
using RushOps.Navigation;
using UnityEngine;

namespace RushOps.Tests.EditMode
{
    public class SteeringMathTests
    {
        [Test]
        public void Separation_NeighbourInsideRadius_PushesAway()
        {
            var positions = new[] { Vector2.zero, new Vector2(0.5f, 0f) };

            Vector2 push = SteeringMath.Separation(positions[0], positions, 2, 0, 1f);

            Assert.AreEqual(-0.5f, push.x, 1e-5f);
            Assert.AreEqual(0f, push.y, 1e-5f);
        }

        [Test]
        public void Separation_NeighbourOutsideRadius_NoPush()
        {
            var positions = new[] { Vector2.zero, new Vector2(2f, 0f) };

            Assert.AreEqual(Vector2.zero, SteeringMath.Separation(positions[0], positions, 2, 0, 1f));
        }

        [Test]
        public void Separation_SkipsSelfAndEntriesBeyondCount()
        {
            var positions = new[] { Vector2.zero, new Vector2(0.5f, 0f), new Vector2(0f, 0.1f) };

            Vector2 push = SteeringMath.Separation(positions[0], positions, 2, 0, 1f);

            Assert.AreEqual(0f, push.y, 1e-5f, "third entry is beyond count");
        }

        [Test]
        public void Separation_SamePosition_PairPushesInDifferentNonOppositeDirections()
        {
            var positions = new[] { Vector2.one, Vector2.one };

            Vector2 first = SteeringMath.Separation(positions[0], positions, 2, 0, 1f);
            Vector2 second = SteeringMath.Separation(positions[1], positions, 2, 1, 1f);

            Assert.AreEqual(1f, first.magnitude, 1e-5f);
            Assert.AreEqual(1f, second.magnitude, 1e-5f);
            float dot = Vector2.Dot(first, second);
            Assert.Less(dot, 0.99f, "different directions");
            Assert.Greater(dot, -0.99f, "not opposite, physics would cancel that");
        }

        [Test]
        public void Combine_IsAtMostLengthOne()
        {
            Vector2 direction = SteeringMath.Combine(Vector2.right, new Vector2(0f, 3f), 1f);

            Assert.AreEqual(1f, direction.magnitude, 1e-5f);
            Assert.Greater(direction.y, direction.x);
        }

        [Test]
        public void Combine_NoSeparation_KeepsPath()
        {
            Assert.AreEqual(Vector2.up, SteeringMath.Combine(Vector2.up, Vector2.zero, 2f));
        }
    }
}
