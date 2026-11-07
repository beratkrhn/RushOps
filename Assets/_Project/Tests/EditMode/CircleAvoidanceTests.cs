using NUnit.Framework;
using RushOps.Bosses;
using UnityEngine;

namespace RushOps.Tests.EditMode
{
    /// <summary>Bosses slide along the round fountain instead of getting stuck (issue #210, decision N2).</summary>
    public class CircleAvoidanceTests
    {
        private static readonly Vector2 Center = new Vector2(16f, 16f);
        private const float Clearance = 2.85f;

        [Test]
        public void FreeWay_KeepsTheDirection()
        {
            var position = new Vector2(10f, 24f);
            var target = new Vector2(22f, 24f);
            Vector2 direction = (target - position).normalized;

            Assert.IsFalse(CircleAvoidance.Blocks(position, direction, target, Center, Clearance));
            Assert.AreEqual(direction, CircleAvoidance.Steer(position, direction, target, Center, Clearance));
        }

        [Test]
        public void TargetInFrontOfTheObstacle_DoesNotBlock()
        {
            var position = new Vector2(16f, 24f);
            var target = new Vector2(16f, 21f);
            Vector2 direction = Vector2.down;

            Assert.IsFalse(CircleAvoidance.Blocks(position, direction, target, Center, Clearance));
        }

        [Test]
        public void MovingAway_DoesNotBlock()
        {
            var position = new Vector2(16f, 19f);
            var target = new Vector2(16f, 26f);

            Assert.IsFalse(CircleAvoidance.Blocks(position, Vector2.up, target, Center, Clearance));
        }

        [Test]
        public void ObstacleInTheWay_HeadsForTheTangentPoint_OnTheTargetsSide()
        {
            // Target slightly west of the axis behind the fountain: pass it on the west.
            var position = new Vector2(16f, 24f);
            var target = new Vector2(15.5f, 8f);
            Vector2 direction = (target - position).normalized;

            Vector2 steered = CircleAvoidance.Steer(position, direction, target, Center, Clearance);

            Assert.AreEqual(1f, steered.magnitude, 1e-4f);
            Assert.Less(steered.x, 0f, "passes on the west, where the target is");
            float distanceToLine = Mathf.Abs(Cross(steered, Center - position));
            Assert.AreEqual(Clearance, distanceToLine, 1e-3f, "the new line just touches the clearance");
        }

        [Test]
        public void TargetOnTheOtherSide_PassesOnTheOtherSide()
        {
            var position = new Vector2(16f, 24f);
            var target = new Vector2(16.5f, 8f);
            Vector2 direction = (target - position).normalized;

            Assert.Greater(CircleAvoidance.Steer(position, direction, target, Center, Clearance).x, 0f);
        }

        [Test]
        public void TargetExactlyBehindTheCenter_PicksOneSide_Deterministically()
        {
            var position = new Vector2(16f, 24f);
            var target = new Vector2(16f, 8f);

            Vector2 first = CircleAvoidance.Steer(position, Vector2.down, target, Center, Clearance);
            Vector2 second = CircleAvoidance.Steer(position, Vector2.down, target, Center, Clearance);

            Assert.AreEqual(first, second);
            Assert.AreNotEqual(0f, first.x, "turns aside instead of pushing into the fountain");
        }

        [Test]
        public void OnTheEdge_FollowsTheTangent()
        {
            // Touching the clearance at its north: moving along the edge, neither in nor out.
            var position = Center + Vector2.up * Clearance;
            var target = new Vector2(15.5f, 8f);
            Vector2 direction = (target - position).normalized;

            Vector2 steered = CircleAvoidance.Steer(position, direction, target, Center, Clearance);

            Assert.AreEqual(0f, Vector2.Dot(steered, Vector2.up), 1e-2f, "neither in nor out");
            Assert.Less(steered.x, 0f);
        }

        [Test]
        public void Inside_IsPushedBackOut()
        {
            var position = Center + Vector2.up * (Clearance - 0.3f);
            var target = new Vector2(15.5f, 8f);
            Vector2 direction = (target - position).normalized;

            Vector2 steered = CircleAvoidance.Steer(position, direction, target, Center, Clearance);

            Assert.Greater(Vector2.Dot(steered, Vector2.up), 0f, "away from the center");
        }

        [Test]
        public void WalkingWithSteering_GetsAroundWithoutEnteringTheObstacle()
        {
            var position = new Vector2(16f, 24f);
            var target = new Vector2(15.5f, 8f);
            const float step = 1.4f * 0.02f;
            float closest = float.MaxValue;
            int steps = 0;
            while ((target - position).magnitude > 0.5f && steps < 5000)
            {
                Vector2 direction = (target - position).normalized;
                position += CircleAvoidance.Steer(position, direction, target, Center, Clearance) * step;
                closest = Mathf.Min(closest, (position - Center).magnitude);
                steps++;
            }

            Assert.Less((target - position).magnitude, 0.5f, "reaches the target behind the fountain");
            Assert.GreaterOrEqual(closest, Clearance - 0.05f, "never presses into the fountain");
            Assert.Less(steps, 1000, "no detour far beyond the straight way of about 16 units");
        }

        [Test]
        public void ZeroDirection_StaysZero()
        {
            Assert.AreEqual(Vector2.zero,
                CircleAvoidance.Steer(new Vector2(16f, 24f), Vector2.zero, new Vector2(16f, 8f), Center, Clearance));
        }

        private static float Cross(Vector2 a, Vector2 b)
        {
            return a.x * b.y - a.y * b.x;
        }
    }
}
