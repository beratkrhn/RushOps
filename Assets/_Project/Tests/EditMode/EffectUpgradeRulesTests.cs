using System.Collections.Generic;
using NUnit.Framework;
using RushOps.Combat;
using RushOps.Endless;
using RushOps.Perks;
using UnityEditor;
using UnityEngine;

namespace RushOps.Tests.EditMode
{
    /// <summary>Pure rules of the upgrades with new effects (issue #278): bounce, fire trail tiles, permanent drone.</summary>
    public class EffectUpgradeRulesTests
    {
        [Test]
        public void Bounce_ReflectsAtTheSameAngle()
        {
            Vector2 down = WeaponMath.Bounce(new Vector2(1f, -1f).normalized, Vector2.up);
            Assert.AreEqual(new Vector2(1f, 1f).normalized.x, down.x, 1e-5f);
            Assert.AreEqual(new Vector2(1f, 1f).normalized.y, down.y, 1e-5f);

            Vector2 back = WeaponMath.Bounce(Vector2.left, Vector2.right * 3f);
            Assert.AreEqual(1f, back.x, 1e-5f);
            Assert.AreEqual(0f, back.y, 1e-5f);

            Assert.AreEqual(Vector2.left, WeaponMath.Bounce(Vector2.left, Vector2.zero));
        }

        [Test]
        public void Tiles_FirstOfADashAtOnce_ThenEverySpacing()
        {
            var tiles = new FireTrailTiles(8);

            Assert.IsTrue(tiles.TryLay(Vector2.zero, 0.5f, 1.5f));
            Assert.IsFalse(tiles.TryLay(new Vector2(0.3f, 0f), 0.5f, 1.5f));
            Assert.IsTrue(tiles.TryLay(new Vector2(0.6f, 0f), 0.5f, 1.5f));
            Assert.AreEqual(2, tiles.Count);

            tiles.BreakChain();
            Assert.IsTrue(tiles.TryLay(new Vector2(0.7f, 0f), 0.5f, 1.5f), "a new dash starts its chain at once");
        }

        [Test]
        public void Tiles_GoOutAfterTheirTime_OldestFirst()
        {
            var tiles = new FireTrailTiles(8);
            tiles.TryLay(Vector2.zero, 0.5f, 1.5f);
            tiles.Tick(0.5f);
            tiles.TryLay(Vector2.right, 0.5f, 1.5f);

            tiles.Tick(0.9f);
            Assert.AreEqual(2, tiles.Count);
            tiles.Tick(0.1f);
            Assert.AreEqual(1, tiles.Count, "the first tile burned 1.5 s");
            Assert.AreEqual(Vector2.right, tiles.Position(0));
            Assert.AreEqual(0.5f, tiles.Remaining(0), 1e-4f);
            tiles.Tick(0.5f);
            Assert.AreEqual(0, tiles.Count);
        }

        [Test]
        public void Tiles_WhenFull_TheOldestMakesRoom()
        {
            var tiles = new FireTrailTiles(3);
            for (int i = 0; i < 4; i++)
            {
                tiles.TryLay(new Vector2(i, 0f), 0.5f, 1.5f);
            }

            Assert.AreEqual(3, tiles.Count);
            Assert.AreEqual(new Vector2(1f, 0f), tiles.Position(0));
            Assert.AreEqual(new Vector2(3f, 0f), tiles.Position(2));
        }

        [Test]
        public void Tiles_IsBurning_AndClear()
        {
            var tiles = new FireTrailTiles(4);
            tiles.TryLay(Vector2.zero, 0.5f, 1.5f);

            Assert.IsTrue(tiles.IsBurning(new Vector2(0.4f, 0f), 0.45f));
            Assert.IsFalse(tiles.IsBurning(new Vector2(0.5f, 0f), 0.45f));
            tiles.Clear();
            Assert.AreEqual(0, tiles.Count);
            Assert.IsTrue(tiles.TryLay(new Vector2(0.1f, 0f), 0.5f, 1.5f), "clear also ends the chain");
        }

        [Test]
        public void Companion_Permanent_NeverRunsOut_UntilCleared()
        {
            var rules = new CompanionRules();
            rules.Activate(15f);
            rules.ActivatePermanent();

            Assert.IsFalse(rules.Tick(1000f));
            Assert.IsTrue(rules.IsActive);
            Assert.IsTrue(rules.IsPermanent);
            rules.Activate(15f);
            Assert.IsTrue(rules.IsPermanent, "a picked-up companion perk does not end the permanent drone");

            rules.Clear();
            Assert.IsFalse(rules.IsActive);
            Assert.IsFalse(rules.IsPermanent);
        }

        [Test]
        public void PerkPool_Excluded_NeverPicksIt_OthersGrow()
        {
            var companion = Perk(PerkEffect.Companion, 10f);
            var shield = Perk(PerkEffect.Shield, 10f);
            var pool = new PerkPool(new List<PerkDefinition> { companion, shield }, new SystemRandomSource(3));
            try
            {
                pool.Excluded = PerkEffect.Companion;

                Assert.AreEqual(10f, pool.TotalWeight);
                for (float roll = 0f; roll <= 1f; roll += 0.05f)
                {
                    Assert.AreSame(shield, pool.Pick(roll));
                }

                pool.Excluded = null;
                Assert.AreEqual(20f, pool.TotalWeight);
                Assert.AreSame(companion, pool.Pick(0f));
            }
            finally
            {
                Object.DestroyImmediate(companion);
                Object.DestroyImmediate(shield);
            }
        }

        [Test]
        public void FireTrailConfig_StartValues()
        {
            var config = AssetDatabase.LoadAssetAtPath<FireTrailConfig>("Assets/_Project/Data/Endless/FireTrailConfig.asset");
            Assert.IsNotNull(config, "run RushOps → Szenen → Endlos-Arena erzeugen");

            Assert.AreEqual(1.5f, config.Duration);
            Assert.AreEqual(30f, config.DamagePerSecond);
            Assert.AreEqual(0.2f, config.TickInterval, 1e-6f);
            Assert.IsNotNull(config.CrackleClip);
        }

        private static PerkDefinition Perk(PerkEffect effect, float weight)
        {
            var perk = ScriptableObject.CreateInstance<PerkDefinition>();
            var serialized = new SerializedObject(perk);
            serialized.FindProperty("_effect").intValue = (int)effect;
            serialized.FindProperty("_weight").floatValue = weight;
            serialized.FindProperty("_kind").intValue = (int)PerkKind.Timed;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return perk;
        }
    }
}
