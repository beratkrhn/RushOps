using NUnit.Framework;
using RushOps.Combat;

namespace RushOps.Tests.EditMode
{
    public class HealthTests
    {
        [Test]
        public void New_StartsAtMax()
        {
            var health = new Health(5, 1f);

            Assert.AreEqual(5, health.Current);
            Assert.AreEqual(5, health.Max);
            Assert.IsFalse(health.IsDead);
            Assert.IsFalse(health.IsInvulnerable);
        }

        [Test]
        public void TakeDamage_RemovesPointsAndRaisesChanged()
        {
            var health = new Health(5, 1f);
            int reported = -1;
            health.Changed += (current, max) => reported = current;

            Assert.IsTrue(health.TakeDamage(2));

            Assert.AreEqual(3, health.Current);
            Assert.AreEqual(3, reported);
        }

        [Test]
        public void TakeDamage_DuringInvulnerability_IsIgnored()
        {
            var health = new Health(5, 1f);
            health.TakeDamage(1);

            health.Tick(0.5f);

            Assert.IsTrue(health.IsInvulnerable);
            Assert.IsFalse(health.TakeDamage(1));
            Assert.AreEqual(4, health.Current);
        }

        [Test]
        public void TakeDamage_AfterInvulnerabilityEnds_Counts()
        {
            var health = new Health(5, 1f);
            health.TakeDamage(1);

            for (int i = 0; i < 50; i++)
            {
                health.Tick(0.02f);
            }

            Assert.IsFalse(health.IsInvulnerable);
            Assert.IsTrue(health.TakeDamage(1));
            Assert.AreEqual(3, health.Current);
        }

        [Test]
        public void TakeDamage_WithoutInvulnerability_EveryHitCounts()
        {
            var health = new Health(3);

            health.TakeDamage(1);
            health.TakeDamage(1);

            Assert.AreEqual(1, health.Current);
        }

        [TestCase(0)]
        [TestCase(-2)]
        public void TakeDamage_NonPositive_DoesNothing(int amount)
        {
            var health = new Health(5, 1f);

            Assert.IsFalse(health.TakeDamage(amount));
            Assert.AreEqual(5, health.Current);
            Assert.IsFalse(health.IsInvulnerable);
        }

        [Test]
        public void TakeDamage_ToZero_DiesOnce()
        {
            var health = new Health(2, 1f);
            int deaths = 0;
            health.Died += () => deaths++;

            health.TakeDamage(5);
            health.Tick(2f);
            health.TakeDamage(1);

            Assert.AreEqual(0, health.Current);
            Assert.IsTrue(health.IsDead);
            Assert.AreEqual(1, deaths);
        }

        [Test]
        public void Heal_StopsAtMax()
        {
            var health = new Health(5);
            health.TakeDamage(3);

            Assert.AreEqual(3, health.Heal(10));
            Assert.AreEqual(5, health.Current);
            Assert.AreEqual(0, health.Heal(1));
        }

        [Test]
        public void Heal_WhenDead_DoesNothing()
        {
            var health = new Health(1);
            health.TakeDamage(1);

            Assert.AreEqual(0, health.Heal(1));
            Assert.IsTrue(health.IsDead);
        }

        [Test]
        public void New_MaxBelowOne_IsOne()
        {
            Assert.AreEqual(1, new Health(0).Max);
        }
    }
}
