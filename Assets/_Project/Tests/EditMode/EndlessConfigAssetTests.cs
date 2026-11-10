using NUnit.Framework;
using RushOps.Endless;
using UnityEditor;

namespace RushOps.Tests.EditMode
{
    /// <summary>Checks the start values in Data/Endless/EndlessConfig.asset (issue #205, backlog v1.1.0).</summary>
    public class EndlessConfigAssetTests
    {
        private const string ConfigPath = "Assets/_Project/Data/Endless/EndlessConfig.asset";

        private static EndlessConfig Load()
        {
            var config = AssetDatabase.LoadAssetAtPath<EndlessConfig>(ConfigPath);
            Assert.IsNotNull(config, $"missing {ConfigPath}; run RushOps → Daten → Endlos-Konfiguration erzeugen");
            return config;
        }

        [Test]
        public void Rounds_FourSecondPause_OneHeart()
        {
            EndlessConfig config = Load();

            Assert.AreEqual(4f, config.PauseBetweenRounds);
            Assert.AreEqual(1, config.HeartsPerRound);
        }

        [Test]
        public void Enemies_WalkersFirst_RunnersBoomersBrutesLater()
        {
            EndlessConfig config = Load();

            Assert.AreEqual(6, config.EnemiesFirstRound);
            Assert.AreEqual(2, config.EnemiesPerRound);
            Assert.AreEqual("Walker", config.BaseEnemy.name);
            Assert.AreEqual(3, config.EnemyShares.Length);
            Assert.AreEqual("Runner", config.EnemyShares[0].Enemy.name);
            Assert.AreEqual(3, config.EnemyShares[0].FirstRound);
            Assert.AreEqual("Boomer", config.EnemyShares[1].Enemy.name);
            Assert.AreEqual(5, config.EnemyShares[1].FirstRound);
            Assert.AreEqual("Brute", config.EnemyShares[2].Enemy.name);
            Assert.AreEqual(7, config.EnemyShares[2].FirstRound);
            foreach (EndlessEnemyShare share in config.EnemyShares)
            {
                Assert.Greater(share.StartShare, 0f, share.Enemy.name);
                Assert.GreaterOrEqual(share.MaxShare, share.StartShare, share.Enemy.name);
            }
        }

        [Test]
        public void Spawning_IntervalAndLimit()
        {
            EndlessConfig config = Load();

            Assert.AreEqual(0.8f, config.SpawnIntervalFirstRound, 1e-5f);
            Assert.AreEqual(0.05f, config.SpawnIntervalStep, 1e-5f);
            Assert.AreEqual(0.3f, config.SpawnIntervalMin, 1e-5f);
            Assert.AreEqual(10, config.MaxActiveStart);
            Assert.AreEqual(1, config.MaxActiveStep);
            Assert.AreEqual(5, config.MaxActiveEveryRounds);
            Assert.AreEqual(20, config.MaxActiveCap);
        }

        [Test]
        public void Bosses_EveryTenRounds_ColossusBroodMotherMegaBoomer()
        {
            EndlessConfig config = Load();

            Assert.AreEqual(10, config.BossEveryRounds);
            Assert.AreEqual(0.25f, config.BossHealthGrowth, 1e-5f);
            Assert.AreEqual(3, config.Bosses.Length);
            Assert.AreEqual("Koloss", config.Bosses[0].name);
            Assert.AreEqual("Brutmutter", config.Bosses[1].name);
            Assert.AreEqual("MegaBoomer", config.Bosses[2].name);
        }

        [Test]
        public void DogRounds_EverySixthRound_Dogs()
        {
            EndlessConfig config = Load();

            Assert.AreEqual(6, config.DogRoundEveryRounds);
            Assert.IsNotNull(config.DogEnemy, "run RushOps → Daten → Endlos-Konfiguration erzeugen");
            Assert.AreEqual("Dog", config.DogEnemy.name);
            Assert.AreEqual(3, config.DogEnemy.PackSize);
        }

        [Test]
        public void Weapons_PistolThenShotgunRifleDualPistols()
        {
            EndlessConfig config = Load();

            Assert.AreEqual("Pistol", config.StartWeapon.Id);
            Assert.AreEqual(3, config.WallWeapons.Length);
            Assert.AreEqual(5, config.WallWeapons[0].Round);
            Assert.AreEqual("Shotgun", config.WallWeapons[0].Weapon.Id);
            Assert.AreEqual(10, config.WallWeapons[1].Round);
            Assert.AreEqual("AssaultRifle", config.WallWeapons[1].Weapon.Id);
            Assert.AreEqual(15, config.WallWeapons[2].Round);
            Assert.AreEqual("DualPistols", config.WallWeapons[2].Weapon.Id);
        }
    }
}
