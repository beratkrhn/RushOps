using System.Collections.Generic;
using NUnit.Framework;
using RushOps.Bosses;
using RushOps.Endless;
using RushOps.Enemies;
using UnityEngine;

namespace RushOps.Tests.EditMode
{
    /// <summary>Dog rounds of the endless mode with the start values of the backlog v1.5.0 (issue #262, F-END-10, N6).</summary>
    public class EndlessDogRoundTests
    {
        private readonly List<Object> _created = new List<Object>();
        private EndlessConfig _config;
        private EndlessRules _rules;
        private EnemyDefinition _walker;
        private EnemyDefinition _runner;
        private EnemyDefinition _dog;

        [SetUp]
        public void Create()
        {
            _walker = Make<EnemyDefinition>("Walker");
            _runner = Make<EnemyDefinition>("Runner");
            var dogConfig = Make<DogConfig>("DogConfig");
            dogConfig.ConfigurePack(3, 0.5f);
            _dog = Make<EnemyDefinition>("Dog");
            var serialized = new UnityEditor.SerializedObject(_dog);
            serialized.FindProperty("_dog").objectReferenceValue = dogConfig;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            _config = Make<EndlessConfig>("EndlessConfig");
            _config.ConfigureEnemies(6, 2, _walker, new[] { new EndlessEnemyShare(_runner, 3, 0.15f, 0.02f, 0.30f) });
            _config.ConfigureSpawning(0.8f, 0.05f, 0.3f, 0);
            _config.ConfigureBosses(10, new[] { Make<BossDefinition>("Koloss") }, 0.25f);
            _config.ConfigureDogRounds(6, _dog);
            _rules = new EndlessRules(_config);
        }

        [TearDown]
        public void Destroy()
        {
            foreach (Object o in _created)
            {
                Object.DestroyImmediate(o);
            }

            _created.Clear();
        }

        [TestCase(6)]
        [TestCase(12)]
        [TestCase(18)]
        [TestCase(24)]
        [TestCase(36)]
        public void EverySixthRound_IsADogRound(int round)
        {
            Assert.IsTrue(_rules.IsDogRound(round));
            Assert.IsTrue(_rules.Build(round).IsDogRound);
        }

        [TestCase(1)]
        [TestCase(5)]
        [TestCase(7)]
        [TestCase(11)]
        [TestCase(13)]
        [TestCase(21)]
        [TestCase(32)]
        public void OtherRounds_AreNoDogRounds(int round)
        {
            Assert.IsFalse(_rules.IsDogRound(round));
            Assert.IsFalse(_rules.Build(round).IsDogRound);
        }

        [Test]
        public void Round30_BossFirst_Round31_DogRound()
        {
            Assert.IsTrue(_rules.IsBossRound(30));
            Assert.IsFalse(_rules.IsDogRound(30), "boss rounds never have dogs");
            Assert.AreEqual(0, _rules.EnemyMix(30).Length);
            Assert.IsTrue(_rules.IsDogRound(31), "the dog round follows one round later (N6)");
            Assert.IsTrue(_rules.IsDogRound(36), "then every sixth round again");
            Assert.IsTrue(_rules.IsBossRound(60));
            Assert.IsTrue(_rules.IsDogRound(61));
        }

        [Test]
        public void Round10And20_BossRounds_NoShiftedDogRound()
        {
            Assert.IsFalse(_rules.IsDogRound(11));
            Assert.IsFalse(_rules.IsDogRound(21));
        }

        [Test]
        public void DogRound_DogsOnly()
        {
            WaveEntry[] mix = _rules.EnemyMix(12);

            Assert.AreEqual(1, mix.Length);
            Assert.AreSame(_dog, mix[0].Enemy);
        }

        [Test]
        public void DogRound_CountAlreadyFullPacks_StaysTheSame()
        {
            // Round 6: 6 + 2 * 5 = 16, round 12: 6 + 2 * 11 = 28 -> neither a multiple of 3.
            // Round 31: 6 + 2 * 30 = 66, a multiple of 3.
            Assert.AreEqual(66, _rules.EnemyCount(31));
            Assert.AreEqual(66, _rules.EnemyMix(31)[0].Count);
        }

        [TestCase(6, 18)]
        [TestCase(12, 30)]
        [TestCase(18, 42)]
        [TestCase(24, 54)]
        public void DogRound_CountNotAMultipleOfThree_RoundedUpToFullPacks(int round, int expected)
        {
            Assert.AreEqual(expected, _rules.EnemyCount(round));
            Assert.AreEqual(expected, _rules.Build(round).EnemyCount);
            Assert.AreEqual(0, _rules.EnemyCount(round) % 3);
        }

        [Test]
        public void RoundsWithoutDogs_Unchanged()
        {
            Assert.AreEqual(14, _rules.EnemyCount(5));
            Assert.AreEqual(18, _rules.EnemyCount(7));
            foreach (WaveEntry entry in _rules.EnemyMix(7))
            {
                Assert.AreNotSame(_dog, entry.Enemy);
            }
        }

        [Test]
        public void WithoutDog_NoDogRounds()
        {
            _config.ConfigureDogRounds(6, null);

            Assert.IsFalse(_rules.IsDogRound(6));
            Assert.AreEqual(16, _rules.EnemyCount(6));
        }

        [Test]
        public void EveryZeroRounds_NoDogRounds()
        {
            _config.ConfigureDogRounds(0, _dog);

            Assert.IsFalse(_rules.IsDogRound(6));
            Assert.IsFalse(_rules.IsDogRound(31));
        }

        [Test]
        public void WithoutBosses_Round30_IsADogRound_Round31_IsNot()
        {
            _config.ConfigureBosses(10, new BossDefinition[0], 0.25f);

            Assert.IsTrue(_rules.IsDogRound(30));
            Assert.IsFalse(_rules.IsDogRound(31));
        }

        [Test]
        public void CreateWave_DogRound_OneWaveOfDogs()
        {
            WaveDefinition wave = _rules.CreateWave(6);
            _created.Add(wave);

            Assert.AreEqual(1, wave.Waves.Length);
            Assert.AreEqual(1, wave.Waves[0].Entries.Length);
            Assert.AreSame(_dog, wave.Waves[0].Entries[0].Enemy);
            Assert.AreEqual(18, wave.Waves[0].Entries[0].Count);
        }

        private T Make<T>(string name) where T : ScriptableObject
        {
            var o = ScriptableObject.CreateInstance<T>();
            o.name = name;
            _created.Add(o);
            return o;
        }
    }
}
