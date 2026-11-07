using System.Collections;
using NUnit.Framework;
using RushOps.Combat;
using RushOps.Enemies;
using RushOps.Levels;
using RushOps.Perks;
using RushOps.Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RushOps.Tests.PlayMode
{
    /// <summary>The combat drone of the companion perk (issue #245).</summary>
    public class CompanionDroneTests
    {
        private const string SceneName = "Arena_Test";

        private CompanionDrone _drone;
        private PerkPickupPool _pickups;
        private PlayerPerks _player;

        [UnitySetUp]
        public IEnumerator LoadScene()
        {
            yield return SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);
            yield return null;
            Prepare();
        }

        [TearDown]
        public void RestoreTime()
        {
            Time.timeScale = 1f;
        }

        [UnityTest]
        public IEnumerator Pickup_SpawnsDroneNextToPlayer_ItKillsAnEnemyInRange_AndLeavesAfterItsTime()
        {
            Assert.IsFalse(_drone.IsActive, "drone exists before the pickup");
            Vector2 playerPosition = _player.transform.position;

            _pickups.Spawn(playerPosition + new Vector2(0f, -6f), Companion()).Collect();
            yield return null;

            Assert.IsTrue(_drone.IsActive, "pickup did not start the drone");
            Assert.Less(((Vector2)_drone.transform.position - playerPosition).magnitude, 2f, "drone is not next to the player");
            Assert.IsTrue(_drone.GetComponentInChildren<SpriteRenderer>().enabled, "drone is not visible");

            Enemy enemy = EnemyPools.Of(EnemyPools.Walker).Spawn(playerPosition + new Vector2(2.5f, 2f));
            enemy.GetComponent<EnemyMovement>().Stop();
            float timeout = Time.realtimeSinceStartup + 8f;
            while (Time.realtimeSinceStartup < timeout && !enemy.IsDead && enemy.gameObject.activeSelf)
            {
                yield return null;
            }

            Assert.IsTrue(enemy.IsDead || !enemy.gameObject.activeSelf, "the drone did not kill the enemy in range");

            // Real time: the hit stop of the game feel resets the time scale, so it cannot be sped up.
            float end = Time.realtimeSinceStartup + 17f;
            while (Time.realtimeSinceStartup < end && _drone.IsActive)
            {
                yield return null;
            }

            Assert.IsFalse(_drone.IsActive, "drone stayed after its time");
            Assert.IsFalse(_drone.GetComponentInChildren<SpriteRenderer>().enabled, "drone is still visible");
        }

        [UnityTest]
        public IEnumerator DroneKills_CountForKillGates()
        {
            LevelKills kills = Object.FindFirstObjectByType<LevelKills>();
            int before = kills.Counter.Count;
            Vector2 playerPosition = _player.transform.position;
            _pickups.Spawn(playerPosition + new Vector2(0f, -6f), Companion()).Collect();
            Enemy enemy = EnemyPools.Of(EnemyPools.Walker).Spawn(playerPosition + new Vector2(2.5f, 2f));
            enemy.GetComponent<EnemyMovement>().Stop();

            float timeout = Time.realtimeSinceStartup + 8f;
            while (Time.realtimeSinceStartup < timeout && !enemy.IsDead)
            {
                yield return null;
            }

            Assert.IsTrue(enemy.IsDead);
            Assert.AreEqual(before + 1, kills.Counter.Count, "the drone's kill did not count");
        }

        [UnityTest]
        public IEnumerator EnemyOutOfRange_IsNotShot()
        {
            Vector2 playerPosition = _player.transform.position;
            _pickups.Spawn(playerPosition + new Vector2(0f, -6f), Companion()).Collect();
            Enemy enemy = EnemyPools.Of(EnemyPools.Walker).Spawn(playerPosition + new Vector2(0f, 8f));
            enemy.GetComponent<EnemyMovement>().Stop();
            int health = enemy.CurrentHealth;

            yield return new WaitForSeconds(1f);

            Assert.AreEqual(health, enemy.CurrentHealth, "an enemy beyond the range was hit");
        }

        [UnityTest]
        public IEnumerator SecondPickup_RestartsTimer_AndKeepsOneDrone()
        {
            Vector2 spot = (Vector2)_player.transform.position + new Vector2(0f, -6f);
            _pickups.Spawn(spot, Companion()).Collect();
            yield return new WaitForSeconds(0.5f);
            float before = _drone.Remaining;

            _pickups.Spawn(spot, Companion()).Collect();
            yield return null;

            Assert.Greater(_drone.Remaining, before, "second pickup did not restart the timer");
            Assert.AreEqual(1, Object.FindObjectsByType<CompanionDrone>(FindObjectsSortMode.None).Length);
        }

        [UnityTest]
        public IEnumerator Drone_DisappearsWhenThePlayerDies()
        {
            _pickups.Spawn((Vector2)_player.transform.position + new Vector2(0f, -6f), Companion()).Collect();
            yield return null;
            Assert.IsTrue(_drone.IsActive);

            _player.GetComponent<PlayerHealth>().TakeDamage(99);
            yield return null;

            Assert.IsFalse(_drone.IsActive, "drone stayed after the player died");
        }

        [UnityTest]
        public IEnumerator DroneBullets_ComeFromTheProjectilePool()
        {
            // Bullets are launched through the pool the player uses, so the pool's count shows the drone's shots.
            var pool = Object.FindFirstObjectByType<ProjectilePool>();
            Vector2 playerPosition = _player.transform.position;
            _pickups.Spawn(playerPosition + new Vector2(0f, -6f), Companion()).Collect();
            Enemy enemy = EnemyPools.Of(EnemyPools.Brute).Spawn(playerPosition + new Vector2(2.5f, 2f));
            enemy.GetComponent<EnemyMovement>().Stop();

            bool flew = false;
            float timeout = Time.realtimeSinceStartup + 3f;
            while (Time.realtimeSinceStartup < timeout && !flew)
            {
                flew = pool.CountActive > 0;
                yield return null;
            }

            Assert.IsTrue(flew, "no bullet of the drone was seen in the pool");
        }

        [UnityTest]
        public IEnumerator EveryLevelAndTheEndlessArena_HaveADrone(
            [ValueSource(nameof(Scenes))] string scene)
        {
            yield return SceneManager.LoadSceneAsync(scene, LoadSceneMode.Single);
            yield return null;
            Assert.AreEqual(1, Object.FindObjectsByType<CompanionDrone>(FindObjectsSortMode.None).Length, scene);
        }

        private static readonly string[] Scenes =
        {
            "Level_1-1", "Level_1-2", "Level_1-3", "Level_2-1", "Level_2-2", "Level_2-3",
            "Level_3-1", "Level_3-2", "Level_3-3", "Endless_Arena",
        };

        private void Prepare()
        {
            _drone = Object.FindFirstObjectByType<CompanionDrone>();
            Assert.IsNotNull(_drone, "Arena_Test has no CompanionDrone");
            _pickups = Object.FindFirstObjectByType<PerkPickupPool>();
            _player = Object.FindFirstObjectByType<PlayerPerks>();
            Object.FindFirstObjectByType<WaveDirector>().enabled = false;
            Object.FindFirstObjectByType<LevelRestarter>().enabled = false;
            _player.GetComponent<PlayerMoveInput>().enabled = false;
            _player.GetComponent<PlayerAimInput>().enabled = false;
            _player.GetComponent<PlayerWeapon>().enabled = false;
        }

        // Not in the drop pool yet (issue #247), so the definition comes straight from its asset.
        private static PerkDefinition Companion()
        {
            PerkDefinition perk = null;
#if UNITY_EDITOR
            perk = UnityEditor.AssetDatabase.LoadAssetAtPath<PerkDefinition>("Assets/_Project/Data/Perks/Companion.asset");
#endif
            Assert.IsNotNull(perk, "Companion perk asset not found");
            return perk;
        }
    }
}
