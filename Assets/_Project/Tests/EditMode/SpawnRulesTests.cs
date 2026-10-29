using NUnit.Framework;
using RushOps.Enemies;
using UnityEngine;

namespace RushOps.Tests.EditMode
{
    public class SpawnRulesTests
    {
        private static readonly Rect View = new Rect(0f, 0f, 10f, 20f);

        [Test]
        public void IsAllowed_InsideView_Skipped()
        {
            Assert.IsFalse(SpawnRules.IsAllowed(new Vector2(5f, 15f), View, new Vector2(5f, 2f), 4f));
        }

        [Test]
        public void IsAllowed_OutsideViewButNearPlayer_Skipped()
        {
            Assert.IsFalse(SpawnRules.IsAllowed(new Vector2(12f, 10f), View, new Vector2(9f, 10f), 4f));
        }

        [Test]
        public void IsAllowed_OutsideViewAndFar_Allowed()
        {
            Assert.IsTrue(SpawnRules.IsAllowed(new Vector2(30f, 10f), View, new Vector2(5f, 10f), 4f));
        }

        [Test]
        public void View_IsGrownByMargin()
        {
            Rect view = SpawnRules.View(new Vector2(10f, 20f), new Vector2(5f, 8f), 1f);

            Assert.AreEqual(new Rect(4f, 11f, 12f, 18f), view);
        }

        [TestCase(10, 3, 2, 5)]
        [TestCase(10, 10, 0, 0)]
        [TestCase(10, 8, 4, 0)]
        public void FreeSlots_CountsAliveAndAnnounced(int max, int alive, int announced, int expected)
        {
            Assert.AreEqual(expected, SpawnRules.FreeSlots(max, alive, announced));
        }

        [Test]
        public void Pick_NoCandidates_ReturnsMinusOne()
        {
            Assert.AreEqual(-1, SpawnRules.Pick(new int[4], 0, 0.5f));
        }

        [TestCase(0f, 7)]
        [TestCase(0.49f, 7)]
        [TestCase(0.5f, 2)]
        [TestCase(0.999f, 2)]
        [TestCase(1f, 2)]
        public void Pick_MapsRandomOntoCandidates(float random, int expected)
        {
            var candidates = new[] { 7, 2, 9 };

            Assert.AreEqual(expected, SpawnRules.Pick(candidates, 2, random));
        }
    }
}
