using System.Collections.Generic;
using NUnit.Framework;
using RushOps.Perks;

namespace RushOps.Tests.EditMode
{
    public class ActivePerksTests
    {
        private ActivePerks _perks;
        private List<string> _expired;

        [SetUp]
        public void CreatePerks()
        {
            _perks = new ActivePerks();
            _expired = new List<string>();
            _perks.Expired += _expired.Add;
        }

        [Test]
        public void Activate_StartsTimerWithFullDuration()
        {
            Assert.IsTrue(_perks.Activate("speed_boost", PerkEffect.SpeedBoost, 1.5f, 15f));

            Assert.IsTrue(_perks.IsActive("speed_boost"));
            Assert.AreEqual(15f, _perks.Remaining("speed_boost"), 1e-5f);
            Assert.AreEqual(1f, _perks[0].Fraction, 1e-5f);
        }

        [Test]
        public void Tick_ExpiresAfterDuration_Once()
        {
            _perks.Activate("speed_boost", PerkEffect.SpeedBoost, 1.5f, 15f);

            _perks.Tick(14.9f);
            Assert.IsTrue(_perks.IsActive("speed_boost"));
            Assert.AreEqual(0.1f, _perks.Remaining("speed_boost"), 1e-4f);
            CollectionAssert.IsEmpty(_expired);

            _perks.Tick(0.1f);
            Assert.IsFalse(_perks.IsActive("speed_boost"), "expires on the step that reaches 15 s");
            Assert.AreEqual(0, _perks.Count);
            CollectionAssert.AreEqual(new[] { "speed_boost" }, _expired);

            _perks.Tick(1f);
            Assert.AreEqual(1, _expired.Count, "reports the expiry only once");
        }

        [Test]
        public void Tick_InFrameSteps_ExpiresOnTime()
        {
            _perks.Activate("speed_boost", PerkEffect.SpeedBoost, 1.5f, 15f);

            for (int i = 0; i < 15 * 60 - 1; i++)
            {
                _perks.Tick(1f / 60f);
            }

            Assert.IsTrue(_perks.IsActive("speed_boost"));
            _perks.Tick(1f / 60f);
            Assert.IsFalse(_perks.IsActive("speed_boost"));
        }

        [Test]
        public void Activate_SamePerkAgain_ResetsDurationWithoutSecondEntry()
        {
            _perks.Activate("speed_boost", PerkEffect.SpeedBoost, 1.5f, 15f);
            _perks.Tick(10f);

            Assert.IsFalse(_perks.Activate("speed_boost", PerkEffect.SpeedBoost, 1.5f, 15f), "refresh is no new perk");

            Assert.AreEqual(1, _perks.Count);
            Assert.AreEqual(15f, _perks.Remaining("speed_boost"), 1e-5f);
            _perks.Tick(14f);
            Assert.IsTrue(_perks.IsActive("speed_boost"), "runs 15 s from the refresh, not from the first pickup");
            CollectionAssert.IsEmpty(_expired);
        }

        [Test]
        public void Activate_SamePerkAgain_DoesNotStackMultiplier()
        {
            _perks.Activate("speed_boost", PerkEffect.SpeedBoost, 1.5f, 15f);
            _perks.Activate("speed_boost", PerkEffect.SpeedBoost, 1.5f, 15f);

            Assert.AreEqual(1.5f, _perks.Multiplier(PerkEffect.SpeedBoost), 1e-5f);
        }

        [Test]
        public void DifferentPerks_RunInParallelOnOwnTimers()
        {
            _perks.Activate("speed_boost", PerkEffect.SpeedBoost, 1.5f, 15f);
            _perks.Tick(5f);
            _perks.Activate("double_damage", PerkEffect.DoubleDamage, 2f, 10f);

            Assert.AreEqual(2, _perks.Count);
            Assert.AreEqual(10f, _perks.Remaining("speed_boost"), 1e-4f);
            Assert.AreEqual(10f, _perks.Remaining("double_damage"), 1e-4f);
            Assert.AreEqual(1.5f, _perks.Multiplier(PerkEffect.SpeedBoost), 1e-5f);
            Assert.AreEqual(2f, _perks.Multiplier(PerkEffect.DoubleDamage), 1e-5f);

            _perks.Tick(9f);
            _perks.Activate("double_damage", PerkEffect.DoubleDamage, 2f, 10f);
            _perks.Tick(1f);

            Assert.IsFalse(_perks.IsActive("speed_boost"));
            Assert.IsTrue(_perks.IsActive("double_damage"), "refreshing one perk does not affect the other");
            CollectionAssert.AreEqual(new[] { "speed_boost" }, _expired);
        }

        [Test]
        public void Perks_KeepOrderOfFirstPickup_AndCloseGapsOnExpiry()
        {
            _perks.Activate("a", PerkEffect.SpeedBoost, 1.5f, 5f);
            _perks.Activate("b", PerkEffect.DoubleDamage, 2f, 10f);
            _perks.Activate("c", PerkEffect.RapidFire, 2f, 10f);
            _perks.Activate("a", PerkEffect.SpeedBoost, 1.5f, 5f);

            Assert.AreEqual("a", _perks[0].Id, "a refresh keeps the slot");
            _perks.Tick(5f);

            Assert.AreEqual(2, _perks.Count);
            Assert.AreEqual("b", _perks[0].Id);
            Assert.AreEqual("c", _perks[1].Id);
        }

        [Test]
        public void SeveralExpireInOneStep_AllReported()
        {
            _perks.Activate("a", PerkEffect.SpeedBoost, 1.5f, 5f);
            _perks.Activate("b", PerkEffect.DoubleDamage, 2f, 5f);

            _perks.Tick(6f);

            Assert.AreEqual(0, _perks.Count);
            CollectionAssert.AreEquivalent(new[] { "a", "b" }, _expired);
        }

        [Test]
        public void Multiplier_WithoutRunningPerk_IsOne()
        {
            Assert.AreEqual(1f, _perks.Multiplier(PerkEffect.SpeedBoost));

            _perks.Activate("speed_boost", PerkEffect.SpeedBoost, 1.5f, 1f);
            _perks.Tick(1f);

            Assert.AreEqual(1f, _perks.Multiplier(PerkEffect.SpeedBoost), "back to normal after expiry");
        }

        [Test]
        public void Fraction_FallsWithRemainingTime()
        {
            _perks.Activate("speed_boost", PerkEffect.SpeedBoost, 1.5f, 15f);

            _perks.Tick(7.5f);

            Assert.AreEqual(0.5f, _perks[0].Fraction, 1e-5f);
        }

        [Test]
        public void Activate_WithoutDuration_StartsNothing()
        {
            Assert.IsFalse(_perks.Activate("medkit", PerkEffect.Medkit, 2f, 0f));

            Assert.AreEqual(0, _perks.Count);
        }

        [Test]
        public void Tick_NegativeOrZero_ChangesNothing()
        {
            _perks.Activate("speed_boost", PerkEffect.SpeedBoost, 1.5f, 15f);

            _perks.Tick(0f);
            _perks.Tick(-3f);

            Assert.AreEqual(15f, _perks.Remaining("speed_boost"), 1e-5f);
        }

        [Test]
        public void Clear_EndsAllWithoutExpiredEvent()
        {
            _perks.Activate("speed_boost", PerkEffect.SpeedBoost, 1.5f, 15f);

            _perks.Clear();

            Assert.AreEqual(0, _perks.Count);
            Assert.AreEqual(0f, _perks.Remaining("speed_boost"));
            CollectionAssert.IsEmpty(_expired);
        }

        [Test]
        public void HasEffect_TrueOnlyWhileAPerkWithThatEffectRuns()
        {
            Assert.IsFalse(_perks.HasEffect(PerkEffect.InstaKill));

            _perks.Activate("insta_kill", PerkEffect.InstaKill, 1f, 10f);
            Assert.IsTrue(_perks.HasEffect(PerkEffect.InstaKill));
            Assert.IsFalse(_perks.HasEffect(PerkEffect.DoubleDamage));

            _perks.Tick(10f);
            Assert.IsFalse(_perks.HasEffect(PerkEffect.InstaKill));
        }

        [Test]
        public void Multiplier_DifferentOffensivePerks_ActIndependently()
        {
            _perks.Activate("double_damage", PerkEffect.DoubleDamage, 2f, 10f);
            _perks.Activate("rapid_fire", PerkEffect.RapidFire, 2f, 10f);

            Assert.AreEqual(2f, _perks.Multiplier(PerkEffect.DoubleDamage), 1e-5f);
            Assert.AreEqual(2f, _perks.Multiplier(PerkEffect.RapidFire), 1e-5f);
        }
    }
}
