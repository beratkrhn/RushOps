using System.Collections.Generic;
using NUnit.Framework;
using RushOps.Bosses;
using RushOps.Combat;
using RushOps.Endless;
using RushOps.Enemies;
using UnityEngine;

namespace RushOps.Tests.EditMode
{
    /// <summary>Rules of the endless mode with the start values of the backlog (issue #205).</summary>
    public class EndlessRulesTests
    {
        private readonly List<Object> _created = new List<Object>();
        private EndlessConfig _config;
        private EndlessRules _rules;
        private EnemyDefinition _walker;
        private EnemyDefinition _runner;
        private EnemyDefinition _boomer;
        private EnemyDefinition _brute;
        private BossDefinition _colossus;
        private BossDefinition _broodMother;
        private BossDefinition _megaBoomer;
        private WeaponDefinition _pistol;
        private WeaponDefinition _shotgun;
        private WeaponDefinition _rifle;
        private WeaponDefinition _dual;

        [SetUp]
        public void Create()
        {
            _walker = Make<EnemyDefinition>("Walker");
            _runner = Make<EnemyDefinition>("Runner");
            _boomer = Make<EnemyDefinition>("Boomer");
            _brute = Make<EnemyDefinition>("Brute");
            _colossus = Make<BossDefinition>("Koloss");
            _broodMother = Make<BossDefinition>("Brutmutter");
            _megaBoomer = Make<BossDefinition>("MegaBoomer");
            _pistol = Make<WeaponDefinition>("Pistol");
            _shotgun = Make<WeaponDefinition>("Shotgun");
            _rifle = Make<WeaponDefinition>("AssaultRifle");
            _dual = Make<WeaponDefinition>("DualPistols");

            _config = Make<EndlessConfig>("EndlessConfig");
            _config.ConfigureRounds(4f, 1);
            _config.ConfigureEnemies(6, 2, _walker, new[]
            {
                new EndlessEnemyShare(_runner, 3, 0.15f, 0.02f, 0.30f),
                new EndlessEnemyShare(_boomer, 5, 0.10f, 0.01f, 0.20f),
                new EndlessEnemyShare(_brute, 7, 0.05f, 0.01f, 0.15f),
            });
            _config.ConfigureSpawning(0.8f, 0.05f, 0.3f, 10, 1, 5, 20);
            _config.ConfigureBosses(10, new[] { _colossus, _broodMother, _megaBoomer }, 0.25f);
            _config.ConfigureWeapons(_pistol, new[]
            {
                new EndlessWeaponRound(5, _shotgun),
                new EndlessWeaponRound(10, _rifle),
                new EndlessWeaponRound(15, _dual),
            });
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

        private T Make<T>(string name) where T : ScriptableObject
        {
            var o = ScriptableObject.CreateInstance<T>();
            o.name = name;
            _created.Add(o);
            return o;
        }

        private static int CountOf(WaveEntry[] entries, EnemyDefinition enemy)
        {
            int count = 0;
            foreach (WaveEntry entry in entries)
            {
                if (entry.Enemy == enemy)
                {
                    count += entry.Count;
                }
            }

            return count;
        }

        private static int Total(WaveEntry[] entries)
        {
            int total = 0;
            foreach (WaveEntry entry in entries)
            {
                total += entry.Count;
            }

            return total;
        }

        // --- Enemy count ---

        [TestCase(1, 6)]
        [TestCase(2, 8)]
        [TestCase(9, 22)]
        [TestCase(11, 26)]
        [TestCase(29, 62)]
        public void EnemyCount_SixInRoundOne_PlusTwoPerRound(int round, int expected)
        {
            Assert.AreEqual(expected, _rules.EnemyCount(round));
            Assert.AreEqual(expected, Total(_rules.EnemyMix(round)));
        }

        [Test]
        public void RoundsBelowOne_CountAsRoundOne()
        {
            Assert.AreEqual(6, _rules.EnemyCount(0));
            Assert.AreEqual(6, _rules.EnemyCount(-5));
            Assert.AreEqual(0.8f, _rules.SpawnInterval(0), 1e-5f);
            Assert.AreEqual(1, _rules.Build(-1).Number);
        }

        // --- Enemy mix ---

        [TestCase(1)]
        [TestCase(2)]
        public void FirstTwoRounds_WalkersOnly(int round)
        {
            WaveEntry[] mix = _rules.EnemyMix(round);

            Assert.AreEqual(1, mix.Length);
            Assert.AreSame(_walker, mix[0].Enemy);
            Assert.AreEqual(_rules.EnemyCount(round), mix[0].Count);
        }

        [Test]
        public void Round3_RunnersJoin()
        {
            WaveEntry[] mix = _rules.EnemyMix(3);

            // 10 enemies, 15 % runners.
            Assert.AreEqual(2, CountOf(mix, _runner));
            Assert.AreEqual(8, CountOf(mix, _walker));
            Assert.AreEqual(0, CountOf(mix, _boomer));
            Assert.AreEqual(0, CountOf(mix, _brute));
        }

        [Test]
        public void Round4_NoBoomersYet_Round5_BoomersJoin()
        {
            Assert.AreEqual(0, CountOf(_rules.EnemyMix(4), _boomer));

            WaveEntry[] mix = _rules.EnemyMix(5);

            // 14 enemies: 19 % runners, 10 % boomers.
            Assert.AreEqual(3, CountOf(mix, _runner));
            Assert.AreEqual(1, CountOf(mix, _boomer));
            Assert.AreEqual(0, CountOf(mix, _brute));
            Assert.AreEqual(10, CountOf(mix, _walker));
        }

        [Test]
        public void Round6_NoBrutesYet_Round7_AtLeastOneBrute()
        {
            Assert.AreEqual(0, CountOf(_rules.EnemyMix(6), _brute));

            WaveEntry[] mix = _rules.EnemyMix(7);

            // 18 enemies: 23 % runners, 12 % boomers, 5 % brutes (0.9, at least one).
            Assert.AreEqual(4, CountOf(mix, _runner));
            Assert.AreEqual(2, CountOf(mix, _boomer));
            Assert.AreEqual(1, CountOf(mix, _brute));
            Assert.AreEqual(11, CountOf(mix, _walker));
        }

        [Test]
        public void Mix_BaseTypeFirst_ThenTableOrder()
        {
            WaveEntry[] mix = _rules.EnemyMix(9);

            Assert.AreEqual(4, mix.Length);
            Assert.AreSame(_walker, mix[0].Enemy);
            Assert.AreSame(_runner, mix[1].Enemy);
            Assert.AreSame(_boomer, mix[2].Enemy);
            Assert.AreSame(_brute, mix[3].Enemy);
        }

        [Test]
        public void HeavyShares_GrowWithTheRound_UpToTheirLimit()
        {
            EndlessEnemyShare runner = _config.EnemyShares[0];

            Assert.AreEqual(0f, EndlessRules.Share(runner, 2));
            Assert.AreEqual(0.15f, EndlessRules.Share(runner, 3), 1e-5f);
            Assert.AreEqual(0.25f, EndlessRules.Share(runner, 8), 1e-5f);
            Assert.AreEqual(0.29f, EndlessRules.Share(runner, 10), 1e-5f);
            Assert.AreEqual(0.30f, EndlessRules.Share(runner, 11), 1e-5f, "0.31 capped");
            Assert.AreEqual(0.30f, EndlessRules.Share(runner, 500), 1e-5f);

            float previous = 0f;
            for (int round = 3; round < 60; round++)
            {
                if (_rules.IsBossRound(round))
                {
                    continue;
                }

                WaveEntry[] mix = _rules.EnemyMix(round);
                float heavyShare = 1f - CountOf(mix, _walker) / (float)Total(mix);
                Assert.GreaterOrEqual(heavyShare, previous - 0.06f, $"round {round}: heavy share should not drop");
                previous = heavyShare;
            }
        }

        [Test]
        public void Round30Region_AllSharesAtTheirLimit()
        {
            WaveEntry[] mix = _rules.EnemyMix(31);

            // 66 enemies: 30 % runners, 20 % boomers, 15 % brutes.
            Assert.AreEqual(66, Total(mix));
            Assert.AreEqual(20, CountOf(mix, _runner));
            Assert.AreEqual(13, CountOf(mix, _boomer));
            Assert.AreEqual(10, CountOf(mix, _brute));
            Assert.AreEqual(23, CountOf(mix, _walker));
        }

        [Test]
        public void SharesAboveHundredPercent_ScaledDown_NoWalkersLeft()
        {
            _config.ConfigureEnemies(10, 0, _walker, new[]
            {
                new EndlessEnemyShare(_runner, 1, 0.8f, 0f, 0.8f),
                new EndlessEnemyShare(_brute, 1, 0.8f, 0f, 0.8f),
            });

            WaveEntry[] mix = _rules.EnemyMix(1);

            Assert.AreEqual(10, Total(mix));
            Assert.AreEqual(0, CountOf(mix, _walker));
            Assert.AreEqual(5, CountOf(mix, _runner));
            Assert.AreEqual(5, CountOf(mix, _brute));
        }

        [Test]
        public void MinimumOfOne_NeverExceedsTheTotal()
        {
            _config.ConfigureEnemies(2, 0, _walker, new[]
            {
                new EndlessEnemyShare(_runner, 1, 0.01f, 0f, 0.01f),
                new EndlessEnemyShare(_boomer, 1, 0.01f, 0f, 0.01f),
                new EndlessEnemyShare(_brute, 1, 0.01f, 0f, 0.01f),
            });

            WaveEntry[] mix = _rules.EnemyMix(1);

            Assert.AreEqual(2, Total(mix));
            Assert.AreEqual(0, CountOf(mix, _walker));
        }

        // --- Spawn interval and limit ---

        [TestCase(1, 0.8f)]
        [TestCase(2, 0.75f)]
        [TestCase(10, 0.35f)]
        [TestCase(11, 0.3f)]
        [TestCase(20, 0.3f)]
        [TestCase(1000, 0.3f)]
        public void SpawnInterval_ShrinksPerRound_DownTo03(int round, float expected)
        {
            Assert.AreEqual(expected, _rules.SpawnInterval(round), 1e-5f);
        }

        [TestCase(1, 10)]
        [TestCase(4, 10)]
        [TestCase(5, 11)]
        [TestCase(9, 11)]
        [TestCase(10, 12)]
        [TestCase(20, 14)]
        [TestCase(30, 16)]
        [TestCase(49, 19)]
        [TestCase(50, 20)]
        [TestCase(1000, 20)]
        public void MaxActive_TenPlusOneEveryFiveRounds_AtMostTwenty(int round, int expected)
        {
            Assert.AreEqual(expected, _rules.MaxActive(round));
        }

        // --- Bosses ---

        [TestCase(1, false)]
        [TestCase(9, false)]
        [TestCase(10, true)]
        [TestCase(11, false)]
        [TestCase(20, true)]
        [TestCase(30, true)]
        [TestCase(35, false)]
        [TestCase(1000, true)]
        public void BossRound_EveryTenthRound(int round, bool expected)
        {
            Assert.AreEqual(expected, _rules.IsBossRound(round));
            Assert.AreEqual(expected, _rules.Build(round).IsBossRound);
        }

        [Test]
        public void Bosses_InOrder_ThenOverAgain()
        {
            Assert.IsNull(_rules.Boss(5));
            Assert.AreSame(_colossus, _rules.Boss(10));
            Assert.AreSame(_broodMother, _rules.Boss(20));
            Assert.AreSame(_megaBoomer, _rules.Boss(30));
            Assert.AreSame(_colossus, _rules.Boss(40));
            Assert.AreSame(_broodMother, _rules.Boss(50));
            Assert.AreSame(_colossus, _rules.Boss(1000), "appearance 100 → 99 % 3 = 0");
        }

        [TestCase(5, 1f)]
        [TestCase(10, 1f)]
        [TestCase(20, 1.25f)]
        [TestCase(30, 1.5f)]
        [TestCase(40, 1.75f)]
        [TestCase(100, 3.25f)]
        public void BossHealth_PlusQuarterPerFurtherAppearance(int round, float expected)
        {
            Assert.AreEqual(expected, _rules.BossHealthFactor(round), 1e-5f);
        }

        [Test]
        public void BossHealth_ScalesTheBaseHealth()
        {
            Assert.AreEqual(2500, _rules.BossHealth(2500, 10));
            Assert.AreEqual(3125, _rules.BossHealth(2500, 20));
            Assert.AreEqual(2500, _rules.BossHealth(2500, 15), "no boss round");
            Assert.AreEqual(1, _rules.BossHealth(0, 10), "at least 1");
        }

        [TestCase(10)]
        [TestCase(20)]
        [TestCase(30)]
        public void BossRound_NoRegularEnemies(int round)
        {
            Assert.AreEqual(0, _rules.EnemyCount(round));
            Assert.AreEqual(0, _rules.EnemyMix(round).Length);
            Assert.AreEqual(0, _rules.Build(round).EnemyCount);
        }

        [Test]
        public void NoBossesSet_NoBossRounds()
        {
            _config.ConfigureBosses(10, null, 0.25f);

            Assert.IsFalse(_rules.IsBossRound(10));
            Assert.IsNull(_rules.Boss(10));
            Assert.AreEqual(1f, _rules.BossHealthFactor(10));
            Assert.AreEqual(24, _rules.EnemyCount(10));
        }

        // --- Weapons ---

        [Test]
        public void Weapons_PistolFromTheStart_WallWeaponsFromTheirRound()
        {
            CollectionAssert.AreEqual(new[] { _pistol }, _rules.AvailableWeapons(1));
            CollectionAssert.AreEqual(new[] { _pistol }, _rules.AvailableWeapons(4));
            CollectionAssert.AreEqual(new[] { _pistol, _shotgun }, _rules.AvailableWeapons(5));
            CollectionAssert.AreEqual(new[] { _pistol, _shotgun }, _rules.AvailableWeapons(9));
            CollectionAssert.AreEqual(new[] { _pistol, _shotgun, _rifle }, _rules.AvailableWeapons(10));
            CollectionAssert.AreEqual(new[] { _pistol, _shotgun, _rifle, _dual }, _rules.AvailableWeapons(15));
            CollectionAssert.AreEqual(new[] { _pistol, _shotgun, _rifle, _dual }, _rules.AvailableWeapons(1000));
        }

        [Test]
        public void IsWeaponAvailable_LockedBeforeItsRound()
        {
            Assert.IsTrue(_rules.IsWeaponAvailable(_pistol, 1));
            Assert.IsFalse(_rules.IsWeaponAvailable(_shotgun, 4));
            Assert.IsTrue(_rules.IsWeaponAvailable(_shotgun, 5));
            Assert.IsFalse(_rules.IsWeaponAvailable(_dual, 14));
            Assert.IsTrue(_rules.IsWeaponAvailable(_dual, 15));
            Assert.IsFalse(_rules.IsWeaponAvailable(null, 20));
        }

        [Test]
        public void NewWeapon_OnlyInItsRound()
        {
            Assert.IsNull(_rules.NewWeapon(1));
            Assert.AreSame(_shotgun, _rules.NewWeapon(5));
            Assert.IsNull(_rules.NewWeapon(6));
            Assert.AreSame(_rifle, _rules.NewWeapon(10));
            Assert.AreSame(_dual, _rules.NewWeapon(15));
            Assert.IsNull(_rules.NewWeapon(20));
        }

        // --- Whole rounds ---

        [Test]
        public void Build_Round1()
        {
            EndlessRound round = _rules.Build(1);

            Assert.AreEqual(1, round.Number);
            Assert.AreEqual(6, round.EnemyCount);
            Assert.AreEqual(0.8f, round.SpawnInterval, 1e-5f);
            Assert.AreEqual(10, round.MaxActive);
            Assert.IsFalse(round.IsBossRound);
            Assert.IsNull(round.Boss);
            Assert.AreEqual(1f, round.BossHealthFactor);
            CollectionAssert.AreEqual(new[] { _pistol }, round.Weapons);
            Assert.IsNull(round.NewWeapon);
        }

        [Test]
        public void Build_Round10_ColossusAndRifle()
        {
            EndlessRound round = _rules.Build(10);

            Assert.IsTrue(round.IsBossRound);
            Assert.AreSame(_colossus, round.Boss);
            Assert.AreEqual(1f, round.BossHealthFactor);
            Assert.AreEqual(12, round.MaxActive);
            Assert.AreSame(_rifle, round.NewWeapon);
            Assert.AreEqual(3, round.Weapons.Length);
        }

        [Test]
        public void Build_Round20_And30()
        {
            EndlessRound twenty = _rules.Build(20);
            EndlessRound thirty = _rules.Build(30);

            Assert.AreSame(_broodMother, twenty.Boss);
            Assert.AreEqual(1.25f, twenty.BossHealthFactor, 1e-5f);
            Assert.AreEqual(14, twenty.MaxActive);
            Assert.AreSame(_megaBoomer, thirty.Boss);
            Assert.AreEqual(1.5f, thirty.BossHealthFactor, 1e-5f);
            Assert.AreEqual(16, thirty.MaxActive);
            Assert.AreEqual(4, thirty.Weapons.Length);
        }

        [Test]
        public void VeryHighRounds_StayInRange()
        {
            foreach (int round in new[] { 999, 10001, int.MaxValue })
            {
                EndlessRound built = _rules.Build(round);

                Assert.AreEqual(20, built.MaxActive, $"round {round}");
                Assert.AreEqual(0.3f, built.SpawnInterval, 1e-5f, $"round {round}");
                Assert.GreaterOrEqual(built.EnemyCount, 0, $"round {round}");
            }

            WaveEntry[] mix = _rules.EnemyMix(999);
            Assert.AreEqual(2002, Total(mix));
            Assert.AreEqual(601, CountOf(mix, _runner));
            Assert.AreEqual(400, CountOf(mix, _boomer));
            Assert.AreEqual(300, CountOf(mix, _brute));
        }

        // --- Wave for the existing spawner ---

        [Test]
        public void CreateWave_OneWave_NoRepeat()
        {
            WaveDefinition wave = _rules.CreateWave(7);
            _created.Add(wave);

            Assert.AreEqual(1, wave.Waves.Length);
            Assert.AreEqual(18, wave.Waves[0].TotalCount);
            Assert.AreEqual(0.5f, wave.SpawnInterval, 1e-5f);
            Assert.AreEqual(AfterLastWave.End, wave.AfterLastWave);
        }

        [Test]
        public void CreateWave_RunsThroughWaveSequence_ThenFinishes()
        {
            WaveDefinition wave = _rules.CreateWave(1);
            _created.Add(wave);
            var sequence = new WaveSequence(wave);
            int spawned = 0;
            sequence.SpawnRequested += _ => spawned++;

            sequence.Start();
            for (int i = 0; i < 20; i++)
            {
                sequence.Tick(0.8f);
            }

            Assert.AreEqual(6, spawned);
            for (int i = 0; i < 6; i++)
            {
                sequence.NotifyKilled();
            }

            sequence.Tick(0.1f);
            Assert.AreEqual(WavePhase.Finished, sequence.Phase);
        }

        [Test]
        public void CreateWave_BossRound_IsEmpty()
        {
            WaveDefinition wave = _rules.CreateWave(10);
            _created.Add(wave);

            Assert.AreEqual(0, wave.Waves.Length);
        }
    }
}
