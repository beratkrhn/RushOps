using NUnit.Framework;
using RushOps.Bosses;
using RushOps.Perks;
using RushOps.Scoring;
using UnityEngine;

namespace RushOps.Tests.EditMode
{
    public class BossRulesTests
    {
        [Test]
        public void Thresholds_DropOncePerThreshold()
        {
            // Decision C9: one perk at 66 % and one at 33 % health.
            var thresholds = new BossPerkThresholds(new[] { 0.66f, 0.33f });

            Assert.AreEqual(0, thresholds.Advance(80, 100));
            Assert.AreEqual(1, thresholds.Advance(66, 100));
            Assert.AreEqual(0, thresholds.Advance(50, 100), "66 % drops only once");
            Assert.AreEqual(1, thresholds.Advance(30, 100));
            Assert.AreEqual(0, thresholds.Advance(10, 100));
            Assert.AreEqual(2, thresholds.Passed);
        }

        [Test]
        public void Thresholds_BigHitPassingBoth_DropsTwo()
        {
            var thresholds = new BossPerkThresholds(new[] { 0.66f, 0.33f });

            Assert.AreEqual(2, thresholds.Advance(20, 100));
        }

        [Test]
        public void Thresholds_DeadBossDropsNothing()
        {
            var thresholds = new BossPerkThresholds(new[] { 0.66f, 0.33f });

            Assert.AreEqual(0, thresholds.Advance(0, 100));
            Assert.AreEqual(0, new BossPerkThresholds(null).Advance(10, 100));
        }

        [Test]
        public void Fill_IsShareOfHealth()
        {
            Assert.AreEqual(1f, BossMath.Fill(100, 100));
            Assert.AreEqual(0.25f, BossMath.Fill(25, 100), 1e-5f);
            Assert.AreEqual(0f, BossMath.Fill(-3, 100));
            Assert.AreEqual(0f, BossMath.Fill(5, 0));
        }

        [Test]
        public void Chase_GoesStraightAtTarget_StopsWhenClose()
        {
            // Decision L5: no flow field in the open arena.
            Assert.AreEqual(Vector2.up, BossMath.ChaseDirection(Vector2.zero, new Vector2(0f, 5f), 0.5f));
            Assert.AreEqual(Vector2.zero, BossMath.ChaseDirection(Vector2.zero, new Vector2(0.3f, 0f), 0.5f));
        }

        [Test]
        public void DroppablePerks_LeaveOutNukeAndInstaKill()
        {
            PerkDefinition nuke = Perk(PerkEffect.Nuke);
            PerkDefinition instaKill = Perk(PerkEffect.InstaKill);
            PerkDefinition medkit = Perk(PerkEffect.Medkit);
            PerkDefinition doubleDamage = Perk(PerkEffect.DoubleDamage);
            try
            {
                CollectionAssert.AreEqual(
                    new[] { medkit, doubleDamage },
                    BossMath.DroppablePerks(new[] { nuke, medkit, instaKill, doubleDamage, null }));
                Assert.IsEmpty(BossMath.DroppablePerks(null));
            }
            finally
            {
                Object.DestroyImmediate(nuke);
                Object.DestroyImmediate(instaKill);
                Object.DestroyImmediate(medkit);
                Object.DestroyImmediate(doubleDamage);
            }
        }

        [Test]
        public void AddPoints_AddsWithoutMultiplierOrCombo()
        {
            var keeper = new ScoreKeeper(2f, 1, 4);
            keeper.RegisterKill(10);
            keeper.RegisterKill(10);
            int combo = keeper.ComboKills;

            keeper.AddPoints(1000);

            Assert.AreEqual(10 + 20 + 1000, keeper.Score);
            Assert.AreEqual(combo, keeper.ComboKills);
        }

        private static PerkDefinition Perk(PerkEffect effect)
        {
            var perk = ScriptableObject.CreateInstance<PerkDefinition>();
            var serialized = new UnityEditor.SerializedObject(perk);
            serialized.FindProperty("_effect").enumValueIndex = (int)effect;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return perk;
        }
    }
}
