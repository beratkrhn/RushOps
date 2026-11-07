using System.Collections;
using NUnit.Framework;
using RushOps.Enemies;
using RushOps.Loot;
using RushOps.Navigation;
using RushOps.Player;
using RushOps.Scoring;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RushOps.Tests.PlayMode
{
    public class GoldDropTests
    {
        private const string SceneName = "Arena_Test";
        // Free floor along the east wall of Arena_Test, far from the player's start.
        private static readonly Vector2 FreeSpot = new Vector2(34.5f, 18.5f);
        // A cell of the outer wall ring.
        private static readonly Vector2 WallSpot = new Vector2(0.5f, 18.5f);

        private GoldDropper _dropper;
        private GoldPickupPool _coins;
        private GoldCollector _collector;
        private FlowFieldNavigator _navigator;
        private PlayerHealth _player;

        [UnitySetUp]
        public IEnumerator LoadScene()
        {
            yield return SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);
            yield return null;

            _dropper = Object.FindFirstObjectByType<GoldDropper>();
            _coins = Object.FindFirstObjectByType<GoldPickupPool>();
            _collector = Object.FindFirstObjectByType<GoldCollector>();
            _navigator = Object.FindFirstObjectByType<FlowFieldNavigator>();
            _player = Object.FindFirstObjectByType<PlayerHealth>();
            Assert.IsNotNull(_dropper, "Arena_Test has no GoldDropper");
            Assert.IsNotNull(_coins, "Arena_Test has no GoldPickupPool");
            Assert.IsNotNull(_collector, "Arena_Test has no GoldCollector");
            // These tests place enemies and coins themselves.
            Object.FindFirstObjectByType<WaveDirector>().enabled = false;
            Object.FindFirstObjectByType<LevelRestarter>().enabled = false;
            _player.GetComponent<PlayerMoveInput>().enabled = false;
            _player.GetComponent<PlayerAimInput>().enabled = false;
        }

        [TearDown]
        public void RestoreTime()
        {
            Time.timeScale = 1f;
        }

        [Test]
        public void GoldConfig_HasStartValues()
        {
            GoldConfig config = _coins.Config;

            Assert.AreEqual(25, config.PointsPerCoin);
            Assert.AreEqual(8f, config.Lifetime, 1e-5f);
            Assert.AreEqual(2f, config.BlinkDuration, 1e-5f);
            Assert.AreEqual(1f, config.MagnetRange, 1e-5f);
            Assert.AreEqual(8f, config.FlightSpeed, 1e-5f);
            Assert.AreEqual(0.5f, config.ScatterRadius, 1e-5f);
        }

        [UnityTest]
        public IEnumerator EnemyDeath_DropsItsGoldAsSingleCoinsAroundTheDeathSpot(
            [Values(EnemyPools.Walker, EnemyPools.Runner, EnemyPools.Boomer, EnemyPools.Brute)] string type)
        {
            EnemyPool pool = EnemyPools.Of(type);
            Enemy enemy = pool.Spawn(FreeSpot);
            yield return null;
            Vector2 deathSpot = enemy.transform.position;
            int expected = pool.Definition.Gold;

            enemy.Kill();

            Assert.AreEqual(expected, _coins.CountActive, $"{type} drops {expected} coins");
            foreach (GoldPickup coin in _coins.Active)
            {
                Assert.That(Vector2.Distance(deathSpot, coin.transform.position),
                    Is.LessThanOrEqualTo(_coins.Config.ScatterRadius + 1e-3f));
            }
        }

        [Test]
        public void Drop_OnWall_MovesCoinsToWalkableCells()
        {
            int landed = _dropper.Drop(WallSpot, 3);

            Assert.Greater(landed, 0);
            foreach (GoldPickup coin in _coins.Active)
            {
                Vector2 spot = coin.transform.position;
                Assert.IsTrue(_navigator.Field.IsWalkable(_navigator.CellOf(spot)), $"coin landed on a blocked cell at {spot}");
            }
        }

        [UnityTest]
        public IEnumerator CoinNearPlayer_FliesToThePlayer_AndPickupRaisesBalanceAndScore()
        {
            Vector2 player = _player.transform.position;
            int balance = _collector.Wallet.Balance;
            ScoreKeeper score = Object.FindFirstObjectByType<LevelScore>().Keeper;
            int points = score.Score;
            GoldPickup coin = _coins.Spawn(player + Vector2.right * 0.9f);
            float before = Vector2.Distance(coin.transform.position, player);

            yield return new WaitForFixedUpdate();
            yield return null;
            Assert.IsTrue(coin.IsActive ? Vector2.Distance(coin.transform.position, player) < before : true,
                "the magnet pulls the coin");

            for (int i = 0; i < 30 && _collector.Wallet.Balance == balance; i++)
            {
                yield return new WaitForFixedUpdate();
            }

            Assert.AreEqual(balance + 1, _collector.Wallet.Balance, "the coin credits the wallet");
            Assert.AreEqual(points + _coins.Config.PointsPerCoin, score.Score, "25 points at multiplier ×1");
            Assert.AreEqual(0, _coins.CountActive, "the collected coin returns to the pool");
        }

        [UnityTest]
        public IEnumerator CoinOutOfMagnetRange_StaysPut()
        {
            Vector2 player = _player.transform.position;
            GoldPickup coin = _coins.Spawn(player + Vector2.right * 3f);
            Vector2 start = coin.transform.position;

            for (int i = 0; i < 10; i++)
            {
                yield return new WaitForFixedUpdate();
            }

            Assert.AreEqual(start, (Vector2)coin.transform.position);
            Assert.IsTrue(coin.IsActive);
        }

        [UnityTest]
        public IEnumerator UncollectedCoin_BlinksThenDisappearsAfterLifetime()
        {
            GoldPickup coin = _coins.Spawn(FreeSpot);
            GoldConfig config = _coins.Config;
            bool hiddenBeforeBlinking = false;
            bool hiddenWhileBlinking = false;
            Time.timeScale = 8f;

            float start = Time.time;
            while (coin.IsActive && Time.time - start < config.Lifetime + 2f)
            {
                if (!coin.IsVisible)
                {
                    if (coin.Lifetime.IsBlinking)
                    {
                        hiddenWhileBlinking = true;
                    }
                    else
                    {
                        hiddenBeforeBlinking = true;
                    }
                }

                yield return null;
            }

            Assert.IsFalse(coin.IsActive, "coin never disappeared");
            Assert.AreEqual(config.Lifetime, Time.time - start, 0.5f);
            Assert.IsFalse(hiddenBeforeBlinking, "coin blinked too early");
            Assert.IsTrue(hiddenWhileBlinking, "coin did not blink before disappearing");
            Assert.AreEqual(0, _coins.CountActive);
        }
    }
}
