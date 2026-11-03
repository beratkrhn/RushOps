using System.Collections.Generic;
using NUnit.Framework;
using RushOps.Combat;
using UnityEngine;

namespace RushOps.Tests.EditMode
{
    public class ExplosionTargetingTests
    {
        private sealed class FakeTarget : IExplosionTarget
        {
            public FakeTarget(float x, float y, bool canBeHit = true)
            {
                ExplosionPoint = new Vector2(x, y);
                CanBeHitByExplosion = canBeHit;
            }

            public Vector2 ExplosionPoint { get; }

            public bool CanBeHitByExplosion { get; }
        }

        [TestCase(0f, 0f, true)]
        [TestCase(2.5f, 0f, true)]
        [TestCase(0f, -2.5f, true)]
        [TestCase(1.7f, 1.7f, true)]
        [TestCase(1.8f, 1.8f, false)]
        [TestCase(2.51f, 0f, false)]
        public void IsInRadius_CountsEdgeAsInside(float x, float y, bool expected)
        {
            Assert.AreEqual(expected, ExplosionTargeting.IsInRadius(Vector2.zero, 2.5f, new Vector2(x, y)));
        }

        [Test]
        public void IsInRadius_ZeroRadius_HitsNothing()
        {
            Assert.IsFalse(ExplosionTargeting.IsInRadius(Vector2.one, 0f, Vector2.one));
        }

        [Test]
        public void Collect_PicksTargetsInsideRadiusAroundCenter()
        {
            var center = new Vector2(10f, 5f);
            var near = new FakeTarget(11f, 5f);
            var edge = new FakeTarget(10f, 7.5f);
            var far = new FakeTarget(13f, 5f);
            var diagonalOut = new FakeTarget(12f, 7f);
            var results = new List<FakeTarget>();

            ExplosionTargeting.Collect(center, 2.5f, new[] { near, far, edge, diagonalOut }, results);

            CollectionAssert.AreEqual(new[] { near, edge }, results);
        }

        [Test]
        public void Collect_SkipsTargetsThatCannotBeHitAndNulls()
        {
            var dying = new FakeTarget(0.5f, 0f, false);
            var alive = new FakeTarget(-0.5f, 0f);
            var results = new List<FakeTarget>();

            ExplosionTargeting.Collect(Vector2.zero, 2.5f, new[] { dying, null, alive }, results);

            CollectionAssert.AreEqual(new[] { alive }, results);
        }

        [Test]
        public void Collect_AppendsAcrossSeveralLists()
        {
            var a = new FakeTarget(1f, 0f);
            var b = new FakeTarget(0f, 1f);
            var results = new List<FakeTarget>();

            ExplosionTargeting.Collect(Vector2.zero, 2f, new[] { a }, results);
            ExplosionTargeting.Collect(Vector2.zero, 2f, new[] { b }, results);
            ExplosionTargeting.Collect<FakeTarget>(Vector2.zero, 2f, null, results);

            CollectionAssert.AreEqual(new[] { a, b }, results);
        }
    }
}
