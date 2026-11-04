using NUnit.Framework;
using RushOps.Perks;
using UnityEngine;

namespace RushOps.Tests.EditMode
{
    public class NukeTargetingTests
    {
        private static readonly Vector2 Center = new Vector2(10f, 20f);
        private static readonly Vector2 Half = new Vector2(5.625f, 10f);

        [TestCase(10f, 20f, true)]
        [TestCase(15.6f, 29.9f, true)]
        [TestCase(15.625f, 30f, true)]
        [TestCase(15.7f, 20f, false)]
        [TestCase(10f, 9.9f, false)]
        [TestCase(-10f, 20f, false)]
        public void IsInView_OnlyInsideTheCameraRect(float x, float y, bool expected)
        {
            Assert.AreEqual(expected, NukeTargeting.IsInView(Center, Half, new Vector2(x, y)));
        }

        [TestCase(true, false, true, true)]
        [TestCase(true, true, true, false)]
        [TestCase(true, false, false, false)]
        [TestCase(false, false, true, false)]
        public void Kills_LivingVisibleNonBossesOnly(bool alive, bool boss, bool inView, bool expected)
        {
            Assert.AreEqual(expected, NukeTargeting.Kills(alive, boss, inView));
        }
    }
}
