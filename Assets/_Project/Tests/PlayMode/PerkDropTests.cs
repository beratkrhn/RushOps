using System.Collections;
using NUnit.Framework;
using RushOps.Enemies;
using RushOps.Navigation;
using RushOps.Perks;
using RushOps.Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RushOps.Tests.PlayMode
{
    public class PerkDropTests
    {
        private const string SceneName = "Arena_Test";
        // Free floor along the east wall of Arena_Test, far from the player's start.
        private static readonly Vector2 FreeSpot = new Vector2(34.5f, 18.5f);
        // A cell of the outer wall ring.
        private static readonly Vector2 WallSpot = new Vector2(0.5f, 18.5f);
        // Weights of the Lastenheft (F-PRK-02) in the order of LootConfig.
        private static readonly float[] Weights = { 25f, 20f, 20f, 15f, 10f, 7f, 3f };

        private PerkDropper _dropper;
        private PerkPickupPool _pickups;
        private FlowFieldNavigator _navigator;
        private PlayerHealth _player;
        private EnemyPool _walkers;

        [UnitySetUp]
        public IEnumerator LoadScene()
        {
            yield return SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);
            yield return null;

            _dropper = Object.FindFirstObjectByType<PerkDropper>();
            _pickups = Object.FindFirstObjectByType<PerkPickupPool>();
            _navigator = Object.FindFirstObjectByType<FlowFieldNavigator>();
            _player = Object.FindFirstObjectByType<PlayerHealth>();
            _walkers = EnemyPools.Of(EnemyPools.Walker);
            Assert.IsNotNull(_dropper, "Arena_Test has no PerkDropper");
            Assert.IsNotNull(_pickups, "Arena_Test has no PerkPickupPool");
            // These tests place enemies and perks themselves.
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
        public void LootConfig_HasStartValuesOfLastenheft()
        {
            LootConfig config = _pickups.Config;

            Assert.AreEqual(0.12f, config.DropChance, 1e-5f);
            Assert.AreEqual(10f, config.Lifetime, 1e-5f);
            Assert.AreEqual(3f, config.BlinkDuration, 1e-5f);
            Assert.AreEqual(Weights.Length, config.Perks.Length);
            for (int i = 0; i < Weights.Length; i++)
            {
                PerkDefinition perk = config.Perks[i];
                Assert.AreEqual(Weights[i], perk.Weight, 1e-5f, perk.name);
                Assert.IsNotNull(perk.Icon, $"{perk.name} has no icon");
                Assert.IsNotEmpty(perk.Id, $"{perk.name} has no id");
                Assert.IsNotEmpty(perk.DisplayName, $"{perk.name} has no name");
            }
        }

        [UnityTest]
        public IEnumerator EnemyDeath_WithSuccessfulRoll_DropsPerkWhereItDied()
        {
            _dropper.SetRandomSource(new ConstantRandom(0f));
            Enemy walker = _walkers.Spawn(FreeSpot);
            yield return null;
            Vector2 deathSpot = walker.transform.position;

            walker.Kill();

            Assert.AreEqual(1, _pickups.CountActive, "one perk per kill");
            PerkPickup pickup = _pickups.Active[0];
            Assert.That(Vector2.Distance(deathSpot, pickup.transform.position), Is.LessThan(1e-3f));
            Assert.IsNotNull(pickup.Perk);
            Assert.IsTrue(pickup.IsIconVisible);
        }

        [UnityTest]
        public IEnumerator EnemyDeath_WithFailedRoll_DropsNothing()
        {
            _dropper.SetRandomSource(new ConstantRandom(0.5f));
            Enemy walker = _walkers.Spawn(FreeSpot);
            yield return null;

            walker.Kill();

            Assert.AreEqual(0, _pickups.CountActive);
        }

        [Test]
        public void Drop_OnWall_MovesToWalkableCell()
        {
            PerkPickup pickup = _dropper.Drop(WallSpot, _pickups.Config.Perks[0]);

            Assert.IsNotNull(pickup);
            Vector2 spot = pickup.transform.position;
            Assert.IsTrue(_navigator.Field.IsWalkable(_navigator.CellOf(spot)), $"perk landed on a blocked cell at {spot}");
            Assert.That(Vector2.Distance(WallSpot, spot), Is.LessThan(1.5f), "moves only to a neighbouring cell");
        }

        [UnityTest]
        public IEnumerator PlayerTouch_CollectsPerk()
        {
            PerkPickup collected = null;
            _pickups.Collected += p => collected = p;
            PerkDefinition perk = _pickups.Config.Perks[1];

            _dropper.Drop(_player.transform.position, perk);
            for (int i = 0; i < 10 && collected == null; i++)
            {
                yield return new WaitForFixedUpdate();
            }

            Assert.IsNotNull(collected, "touching the perk did not collect it");
            Assert.AreSame(perk, collected.Perk);
            Assert.AreEqual(0, _pickups.CountActive, "collected perk returns to the pool");
        }

        [UnityTest]
        public IEnumerator EnemyTouch_DoesNotCollectPerk()
        {
            PerkPickup pickup = _dropper.Drop(FreeSpot, _pickups.Config.Perks[0]);
            Enemy walker = _walkers.Spawn(FreeSpot);
            for (int i = 0; i < 10; i++)
            {
                yield return new WaitForFixedUpdate();
            }

            Assert.IsTrue(pickup.IsActive, "an enemy picked up the perk");
            walker.Kill();
        }

        [UnityTest]
        public IEnumerator UncollectedPerk_BlinksThenDespawnsAfterLifetime()
        {
            PerkPickup pickup = _dropper.Drop(FreeSpot, _pickups.Config.Perks[0]);
            LootConfig config = _pickups.Config;
            bool hiddenBeforeBlinking = false;
            bool hiddenWhileBlinking = false;
            Time.timeScale = 8f;

            float start = Time.time;
            while (pickup.IsActive && Time.time - start < config.Lifetime + 2f)
            {
                if (!pickup.IsIconVisible)
                {
                    if (pickup.Lifetime.IsBlinking)
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

            float lived = Time.time - start;
            Assert.IsFalse(pickup.IsActive, "perk never despawned");
            Assert.AreEqual(config.Lifetime, lived, 0.5f);
            Assert.IsFalse(hiddenBeforeBlinking, "perk blinked too early");
            Assert.IsTrue(hiddenWhileBlinking, "perk did not blink before despawning");
            Assert.AreEqual(0, _pickups.CountActive);
        }

        private sealed class ConstantRandom : IRandomSource
        {
            private readonly float _value;

            public ConstantRandom(float value)
            {
                _value = value;
            }

            public float Value()
            {
                return _value;
            }
        }
    }
}
