using NUnit.Framework;
using RushOps.Perks;
using UnityEngine;

namespace RushOps.Tests.EditMode
{
    public class SuperWeaponRulesTests
    {
        private const float Range = 3.5f;
        private const float Cone = 60f;

        [Test]
        public void KindOf_MapsOnlyTheSuperWeaponPerks()
        {
            Assert.AreEqual(SuperWeaponKind.Flamethrower, SuperWeaponState.KindOf(PerkEffect.Flamethrower));
            Assert.AreEqual(SuperWeaponKind.RocketLauncher, SuperWeaponState.KindOf(PerkEffect.RocketLauncher));
            Assert.AreEqual(SuperWeaponKind.None, SuperWeaponState.KindOf(PerkEffect.RapidFire));
            Assert.AreEqual(SuperWeaponKind.None, SuperWeaponState.KindOf(PerkEffect.Companion));
        }

        [Test]
        public void State_StartsInactive()
        {
            var state = new SuperWeaponState();

            Assert.IsFalse(state.IsActive);
            Assert.AreEqual(SuperWeaponKind.None, state.Kind);
        }

        [Test]
        public void State_ReplacesTheWeaponForTheDuration_ThenGivesItBack()
        {
            var state = new SuperWeaponState();
            state.Activate(SuperWeaponKind.Flamethrower, 12f);

            Assert.IsFalse(state.Tick(11.9f));
            Assert.IsTrue(state.IsActive);
            Assert.AreEqual(SuperWeaponKind.Flamethrower, state.Kind);
            Assert.IsTrue(state.Tick(0.1f));
            Assert.IsFalse(state.IsActive);
            Assert.AreEqual(SuperWeaponKind.None, state.Kind);
        }

        [Test]
        public void State_PickUpAgain_RestartsTheTimer()
        {
            var state = new SuperWeaponState();
            state.Activate(SuperWeaponKind.RocketLauncher, 12f);
            state.Tick(10f);

            state.Activate(SuperWeaponKind.RocketLauncher, 12f);

            Assert.AreEqual(12f, state.Remaining, 1e-4f);
            Assert.AreEqual(SuperWeaponKind.RocketLauncher, state.Kind);
        }

        [Test]
        public void State_PickUpTheOtherOne_SwitchesWithAFreshTimer()
        {
            var state = new SuperWeaponState();
            state.Activate(SuperWeaponKind.Flamethrower, 12f);
            state.Tick(5f);

            state.Activate(SuperWeaponKind.RocketLauncher, 12f);

            Assert.AreEqual(SuperWeaponKind.RocketLauncher, state.Kind);
            Assert.AreEqual(12f, state.Remaining, 1e-4f);
        }

        [Test]
        public void State_NoneOrZeroDuration_IsIgnored()
        {
            var state = new SuperWeaponState();
            state.Activate(SuperWeaponKind.None, 12f);
            state.Activate(SuperWeaponKind.Flamethrower, 0f);

            Assert.IsFalse(state.IsActive);
        }

        [Test]
        public void State_Clear_EndsItWithoutWaiting()
        {
            var state = new SuperWeaponState();
            state.Activate(SuperWeaponKind.Flamethrower, 12f);

            state.Clear();

            Assert.IsFalse(state.IsActive);
            Assert.IsFalse(state.Tick(1f));
        }

        [Test]
        public void Flame_HitsInsideConeAndRange()
        {
            Assert.IsTrue(FlamethrowerRules.InFlame(Vector2.zero, Vector2.right, Range, Cone, new Vector2(2f, 0.5f)));
        }

        [Test]
        public void Flame_HitsExactlyOnTheRange()
        {
            Assert.IsTrue(FlamethrowerRules.InFlame(Vector2.zero, Vector2.right, Range, Cone, new Vector2(Range, 0f)));
        }

        [Test]
        public void Flame_MissesBeyondTheRange()
        {
            Assert.IsFalse(FlamethrowerRules.InFlame(Vector2.zero, Vector2.right, Range, Cone, new Vector2(Range + 0.01f, 0f)));
        }

        [Test]
        public void Flame_HitsExactlyOnTheConeEdge()
        {
            var edge = new Vector2(Mathf.Cos(30f * Mathf.Deg2Rad), Mathf.Sin(30f * Mathf.Deg2Rad)) * 2f;

            Assert.IsTrue(FlamethrowerRules.InFlame(Vector2.zero, Vector2.right, Range, Cone, edge));
        }

        [Test]
        public void Flame_MissesJustOutsideTheCone()
        {
            var outside = new Vector2(Mathf.Cos(31f * Mathf.Deg2Rad), Mathf.Sin(31f * Mathf.Deg2Rad)) * 2f;

            Assert.IsFalse(FlamethrowerRules.InFlame(Vector2.zero, Vector2.right, Range, Cone, outside));
        }

        [Test]
        public void Flame_MissesBehindThePlayer()
        {
            Assert.IsFalse(FlamethrowerRules.InFlame(Vector2.zero, Vector2.right, Range, Cone, new Vector2(-1f, 0f)));
        }

        [Test]
        public void Flame_HitsAnEnemyAtTheOrigin()
        {
            Assert.IsTrue(FlamethrowerRules.InFlame(Vector2.zero, Vector2.right, Range, Cone, Vector2.zero));
        }

        [Test]
        public void Flame_NoAimOrNoCone_HitsNothing()
        {
            Assert.IsFalse(FlamethrowerRules.InFlame(Vector2.zero, Vector2.zero, Range, Cone, Vector2.right));
            Assert.IsFalse(FlamethrowerRules.InFlame(Vector2.zero, Vector2.right, Range, 0f, Vector2.right));
        }

        [Test]
        public void Flame_BossTakesFullDamage()
        {
            Assert.AreEqual(FlamethrowerRules.Damage(90f, 0.1f, false), FlamethrowerRules.Damage(90f, 0.1f, true), 1e-5f);
            Assert.AreEqual(9f, FlamethrowerRules.Damage(90f, 0.1f, true), 1e-4f);
        }

        [Test]
        public void Afterburn_BurnsForTheDuration_ThenStops()
        {
            var burn = new AfterburnTimer();
            burn.Refresh(1.5f);

            float damage = burn.Tick(1f, 30f) + burn.Tick(0.5f, 30f);

            Assert.AreEqual(45f, damage, 1e-3f);
            Assert.IsFalse(burn.IsBurning);
            Assert.AreEqual(0f, burn.Tick(1f, 30f));
        }

        [Test]
        public void Afterburn_StepPastTheEnd_OnlyCountsTheTimeLeft()
        {
            var burn = new AfterburnTimer();
            burn.Refresh(0.5f);

            Assert.AreEqual(15f, burn.Tick(2f, 30f), 1e-3f);
            Assert.IsFalse(burn.IsBurning);
        }

        [Test]
        public void Afterburn_HitAgain_RefreshesInsteadOfStacking()
        {
            var burn = new AfterburnTimer();
            burn.Refresh(1.5f);
            burn.Tick(1f, 30f);

            burn.Refresh(1.5f);

            Assert.AreEqual(1.5f, burn.Remaining, 1e-4f);
        }

        [Test]
        public void Afterburn_KeepsBurningAfterTheFlamethrowerRanOut()
        {
            var state = new SuperWeaponState();
            var burn = new AfterburnTimer();
            state.Activate(SuperWeaponKind.Flamethrower, 12f);
            burn.Refresh(1.5f);
            burn.Tick(1f, 30f);

            Assert.IsTrue(state.Tick(12f));

            Assert.IsTrue(burn.IsBurning);
            Assert.AreEqual(15f, burn.Tick(1f, 30f), 1e-3f);
        }

        [Test]
        public void Afterburn_Extinguish_Stops()
        {
            var burn = new AfterburnTimer();
            burn.Refresh(1.5f);

            burn.Extinguish();

            Assert.IsFalse(burn.IsBurning);
        }

        [Test]
        public void Rocket_HitsInsideTheRadius_AndOnTheEdge()
        {
            // Radius 2 is exact as a float, so the edge case is not at the mercy of rounding.
            Assert.IsTrue(RocketRules.Hits(Vector2.zero, 2f, new Vector2(1f, 0f), true));
            Assert.IsTrue(RocketRules.Hits(Vector2.zero, 2f, new Vector2(2f, 0f), true));
        }

        [Test]
        public void Rocket_MissesOutsideTheRadius_AndTheDead()
        {
            Assert.IsFalse(RocketRules.Hits(Vector2.zero, 1.6f, new Vector2(1.7f, 0f), true));
            Assert.IsFalse(RocketRules.Hits(Vector2.zero, 1.6f, Vector2.zero, false));
        }

        [Test]
        public void Rocket_BossTakesFullDamage()
        {
            Assert.AreEqual(40, RocketRules.Damage(40, true));
            Assert.AreEqual(RocketRules.Damage(40, false), RocketRules.Damage(40, true));
        }

        [Test]
        public void Rocket_NeverHurtsThePlayer_EvenInTheMiddle()
        {
            // The player stands in the center; the rule for the player does not depend on the radius at all.
            Assert.IsTrue(RocketRules.Hits(Vector2.zero, 1.6f, Vector2.zero, true));
            Assert.AreEqual(0, RocketRules.PlayerDamage);
        }
    

        [Test]
        public void Rocket_LargeBody_CountsWhenItsEdgeReachesIn()
        {
            // Center far outside, nearest hurtbox point inside: a boss with a wide body is caught.
            Assert.IsTrue(RocketRules.InBlast(Vector2.zero, 1.6f, new Vector2(3f, 0f), new Vector2(1.5f, 0f)));
            Assert.IsTrue(RocketRules.InBlast(Vector2.zero, 1.6f, new Vector2(1f, 0f), new Vector2(3f, 0f)));
            Assert.IsFalse(RocketRules.InBlast(Vector2.zero, 1.6f, new Vector2(3f, 0f), new Vector2(2f, 0f)));
        }

        [Test]
        public void Rocket_DoubleDamage_DoublesTheBlast()
        {
            Assert.AreEqual(40, RocketRules.ScaledDamage(40, 1f));
            Assert.AreEqual(80, RocketRules.ScaledDamage(40, 2f));
            Assert.AreEqual(0, RocketRules.ScaledDamage(40, 0f));
        }

        [Test]
        public void Rocket_PushesAwayFromTheCenter()
        {
            Assert.AreEqual(Vector2.right, RocketRules.PushDirection(Vector2.zero, new Vector2(2f, 0f)));
            Assert.AreEqual(Vector2.up, RocketRules.PushDirection(Vector2.one, Vector2.one));
        }
    }
}
