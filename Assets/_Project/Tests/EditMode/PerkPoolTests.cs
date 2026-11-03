using System.Collections.Generic;
using NUnit.Framework;
using RushOps.Perks;
using UnityEditor;
using UnityEngine;

namespace RushOps.Tests.EditMode
{
    public class PerkPoolTests
    {
        // Weights of the Lastenheft (F-PRK-02): speed, medkit, double damage, rapid fire, shield, insta-kill, nuke.
        private static readonly float[] LastenheftWeights = { 25f, 20f, 20f, 15f, 10f, 7f, 3f };

        private readonly List<PerkDefinition> _perks = new List<PerkDefinition>();

        [TearDown]
        public void DestroyPerks()
        {
            foreach (PerkDefinition perk in _perks)
            {
                Object.DestroyImmediate(perk);
            }

            _perks.Clear();
        }

        [Test]
        public void TryDrop_RollBelowChance_Drops()
        {
            PerkDefinition perk = CreatePerk(10f);
            var pool = new PerkPool(new[] { perk }, new FixedRandom(0.07f, 0.5f));

            Assert.IsTrue(pool.TryDrop(0.08f, out PerkDefinition dropped));
            Assert.AreSame(perk, dropped);
        }

        [Test]
        public void TryDrop_RollAtOrAboveChance_DropsNothing()
        {
            var pool = new PerkPool(new[] { CreatePerk(10f) }, new FixedRandom(0.08f));

            Assert.IsFalse(pool.TryDrop(0.08f, out PerkDefinition dropped));
            Assert.IsNull(dropped);
        }

        [Test]
        public void TryDrop_ZeroChanceOrNoWeight_NeverDrops()
        {
            var empty = new PerkPool(new[] { CreatePerk(0f) }, new FixedRandom(0f));
            var none = new PerkPool(new[] { CreatePerk(10f) }, new FixedRandom(0f));

            Assert.IsFalse(empty.TryDrop(1f, out _));
            Assert.IsFalse(none.TryDrop(0f, out _));
        }

        [Test]
        public void TryDrop_OverManyKills_MatchesDropChance()
        {
            var pool = new PerkPool(new[] { CreatePerk(10f) }, new SystemRandomSource(1234));
            const int kills = 100000;
            int drops = 0;
            for (int i = 0; i < kills; i++)
            {
                if (pool.TryDrop(0.08f, out _))
                {
                    drops++;
                }
            }

            Assert.AreEqual(0.08, drops / (double)kills, 0.005);
        }

        [Test]
        public void Pick_LaysOutWeightsInListOrder()
        {
            PerkDefinition a = CreatePerk(1f);
            PerkDefinition b = CreatePerk(3f);
            var pool = new PerkPool(new[] { a, b }, new FixedRandom(0f));

            Assert.AreEqual(4f, pool.TotalWeight, 1e-5f);
            Assert.AreSame(a, pool.Pick(0f));
            Assert.AreSame(a, pool.Pick(0.24f));
            Assert.AreSame(b, pool.Pick(0.25f));
            Assert.AreSame(b, pool.Pick(1f), "a roll of 1 still picks a perk");
        }

        [Test]
        public void Pick_SkipsZeroWeightAndNullEntries()
        {
            PerkDefinition never = CreatePerk(0f);
            PerkDefinition always = CreatePerk(5f);
            var pool = new PerkPool(new[] { never, null, always }, new FixedRandom(0f));

            Assert.AreSame(always, pool.Pick(0f));
            Assert.AreSame(always, pool.Pick(0.99f));
        }

        [Test]
        public void Pick_OverManyDraws_FollowsLastenheftWeights()
        {
            var perks = new PerkDefinition[LastenheftWeights.Length];
            float total = 0f;
            for (int i = 0; i < perks.Length; i++)
            {
                perks[i] = CreatePerk(LastenheftWeights[i]);
                total += LastenheftWeights[i];
            }

            var pool = new PerkPool(perks, new SystemRandomSource(42));
            const int draws = 200000;
            var counts = new Dictionary<PerkDefinition, int>();
            for (int i = 0; i < draws; i++)
            {
                PerkDefinition perk = pool.Pick();
                counts.TryGetValue(perk, out int count);
                counts[perk] = count + 1;
            }

            for (int i = 0; i < perks.Length; i++)
            {
                counts.TryGetValue(perks[i], out int count);
                Assert.AreEqual(LastenheftWeights[i] / total, count / (double)draws, 0.005, $"perk {i} (weight {LastenheftWeights[i]})");
            }
        }

        [Test]
        public void SystemRandomSource_StaysBelowOne()
        {
            var random = new SystemRandomSource(7);
            for (int i = 0; i < 10000; i++)
            {
                float value = random.Value();
                Assert.GreaterOrEqual(value, 0f);
                Assert.Less(value, 1f);
            }
        }

        private PerkDefinition CreatePerk(float weight)
        {
            var perk = ScriptableObject.CreateInstance<PerkDefinition>();
            var serialized = new SerializedObject(perk);
            serialized.FindProperty("_weight").floatValue = weight;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            _perks.Add(perk);
            return perk;
        }

        // Returns the given values in turn, then repeats the last one.
        private sealed class FixedRandom : IRandomSource
        {
            private readonly float[] _values;
            private int _next;

            public FixedRandom(params float[] values)
            {
                _values = values;
            }

            public float Value()
            {
                float value = _values[Mathf.Min(_next, _values.Length - 1)];
                _next++;
                return value;
            }
        }
    }
}
