using NUnit.Framework;
using RushOps.Perks;

namespace RushOps.Tests.EditMode
{
    /// <summary>Fire damage over time of the flamethrower (issue #254, W13).</summary>
    public class FlameBurnsTests
    {
        private const float Flame = 90f;
        private const float Afterburn = 30f;
        private const float AfterburnDuration = 1.5f;
        private const float Tick = 0.2f;

        private sealed class Target
        {
        }

        [Test]
        public void InTheFlame_PaysOutTheDamagePerSecondInWholePointsEveryTick()
        {
            var burns = new FlameBurns<Target>(4);
            var target = new Target();
            int total = 0;
            int ticks = 0;
            for (int frame = 0; frame < 60; frame++)
            {
                burns.Expose(target, Flame, 1f / 60f, AfterburnDuration);
                if (burns.Tick(1f / 60f, Afterburn, Tick))
                {
                    ticks++;
                    total += burns.TakeDamage(0);
                }
            }

            Assert.AreEqual(5, ticks, "one second has five ticks of 0.2 s");
            Assert.AreEqual(90, total, 1, "a second in the flame pays out the damage per second");
        }

        [Test]
        public void BetweenTicks_NoDamageIsDue()
        {
            var burns = new FlameBurns<Target>(4);
            burns.Expose(new Target(), Flame, 0.05f, AfterburnDuration);

            Assert.IsFalse(burns.Tick(0.05f, Afterburn, Tick));
        }

        [Test]
        public void AfterLeavingTheFlame_BurnsOnForTheAfterburn_ThenGoesOut()
        {
            var burns = new FlameBurns<Target>(4);
            var target = new Target();
            burns.Expose(target, Flame, 0.2f, AfterburnDuration);
            burns.Tick(0.2f, Afterburn, Tick);
            burns.TakeDamage(0);

            int afterburn = 0;
            for (int i = 0; i < 7; i++)
            {
                Assert.IsTrue(burns.IsBurning(target), $"stopped burning after {i * 0.2f:0.0} s");
                burns.Tick(0.2f, Afterburn, Tick);
                afterburn += burns.Count > 0 ? burns.TakeDamage(0) : 0;
            }

            burns.Tick(0.2f, Afterburn, Tick);
            afterburn += burns.Count > 0 ? burns.TakeDamage(0) : 0;

            Assert.AreEqual(45, afterburn, 1, "1.5 s afterburn at 30 per second");
            burns.Tick(0.2f, Afterburn, Tick);
            Assert.IsFalse(burns.IsBurning(target));
            Assert.IsFalse(burns.Contains(target), "a burnt out target is dropped");
        }

        [Test]
        public void StayingInTheFlame_RefreshesTheAfterburn_InsteadOfStackingIt()
        {
            var burns = new FlameBurns<Target>(4);
            var target = new Target();
            for (int i = 0; i < 10; i++)
            {
                burns.Expose(target, Flame, 0.2f, AfterburnDuration);
                burns.Tick(0.2f, Afterburn, Tick);
                burns.TakeDamage(0);
            }

            // Left the flame now: 1.5 s afterburn, not ten times that.
            for (int i = 0; i < 8; i++)
            {
                burns.Tick(0.2f, Afterburn, Tick);
                if (burns.Count > 0)
                {
                    burns.TakeDamage(0);
                }
            }

            Assert.IsFalse(burns.IsBurning(target));
        }

        [Test]
        public void InTheFlame_NoAfterburnDamageOnTop()
        {
            var burns = new FlameBurns<Target>(4);
            var target = new Target();
            burns.Expose(target, Flame, 0.2f, AfterburnDuration);
            burns.Tick(0.2f, Afterburn, Tick);

            Assert.AreEqual(18, burns.TakeDamage(0), "0.2 s of flame only");
        }

        [Test]
        public void ExposedTwiceInOneFrame_CountsOnce()
        {
            var burns = new FlameBurns<Target>(4);
            var target = new Target();
            burns.Expose(target, Flame, 0.2f, AfterburnDuration);
            burns.Expose(target, Flame, 0.2f, AfterburnDuration);
            burns.Tick(0.2f, Afterburn, Tick);

            Assert.AreEqual(1, burns.Count);
            Assert.AreEqual(18, burns.TakeDamage(0));
        }

        [Test]
        public void SeveralTargets_BurnIndependently()
        {
            var burns = new FlameBurns<Target>(4);
            var a = new Target();
            var b = new Target();
            burns.Expose(a, Flame, 0.2f, AfterburnDuration);
            burns.Tick(0.2f, Afterburn, Tick);
            burns.Expose(b, Flame, 0.2f, AfterburnDuration);
            burns.Tick(0.2f, Afterburn, Tick);

            Assert.AreEqual(2, burns.Count);
            Assert.IsTrue(burns.IsBurning(a));
            Assert.IsTrue(burns.IsBurning(b));
        }

        [Test]
        public void FullCapacity_IgnoresFurtherTargets()
        {
            var burns = new FlameBurns<Target>(2);
            burns.Expose(new Target(), Flame, 0.1f, AfterburnDuration);
            burns.Expose(new Target(), Flame, 0.1f, AfterburnDuration);
            var third = new Target();
            burns.Expose(third, Flame, 0.1f, AfterburnDuration);

            Assert.AreEqual(2, burns.Count);
            Assert.IsFalse(burns.Contains(third));
        }

        [Test]
        public void RemoveAt_PutsOutOnlyThatTarget()
        {
            var burns = new FlameBurns<Target>(4);
            var a = new Target();
            var b = new Target();
            burns.Expose(a, Flame, 0.1f, AfterburnDuration);
            burns.Expose(b, Flame, 0.1f, AfterburnDuration);

            burns.RemoveAt(0);

            Assert.IsFalse(burns.Contains(a));
            Assert.IsTrue(burns.Contains(b));
            Assert.AreSame(b, burns.Target(0));
        }

        [Test]
        public void Clear_PutsOutEveryFire()
        {
            var burns = new FlameBurns<Target>(4);
            var target = new Target();
            burns.Expose(target, Flame, 0.1f, AfterburnDuration);

            burns.Clear();

            Assert.AreEqual(0, burns.Count);
            Assert.IsFalse(burns.IsBurning(target));
        }

        [Test]
        public void NoTargetOrNoTime_DoesNothing()
        {
            var burns = new FlameBurns<Target>(4);
            burns.Expose(null, Flame, 0.1f, AfterburnDuration);
            burns.Expose(new Target(), Flame, 0f, AfterburnDuration);

            Assert.AreEqual(0, burns.Count);
            Assert.IsFalse(burns.Tick(0f, Afterburn, Tick));
        }
    }
}
