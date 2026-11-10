using System.Collections;
using NUnit.Framework;
using RushOps.Bosses;
using RushOps.Enemies;
using RushOps.Levels;
using RushOps.Perks;
using RushOps.Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RushOps.Tests.PlayMode
{
    /// <summary>The rocket launcher super weapon (issue #255).</summary>
    public class RocketLauncherTests
    {
        private const string SceneName = "Arena_Test";

        private PlayerPerks _player;
        private PlayerAim _aim;
        private PlayerWeapon _weapon;
        private PlayerHealth _health;
        private PlayerSuperWeapon _superWeapon;
        private RocketLauncher _launcher;

        [UnitySetUp]
        public IEnumerator LoadScene()
        {
            SaveGameIsolation.Reset();
            yield return SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);
            yield return null;
            Prepare();
        }

        [UnityTearDown]
        public IEnumerator RestoreTime()
        {
            Time.timeScale = 1f;
            yield return null;
        }

        [UnityTest]
        public IEnumerator Pickup_RocketBlowsUpAGroup_PlayerUnhurt_ThenTheOldWeaponIsBack()
        {
            var old = _weapon.ActiveWeapon;
            int hearts = _health.Current;
            Vector2 center = _player.transform.position;
            var pickups = Object.FindFirstObjectByType<PerkPickupPool>();
            pickups.Spawn(center + new Vector2(0f, -6f), Perk("RocketLauncher")).Collect();
            yield return null;

            Assert.AreEqual(SuperWeaponKind.RocketLauncher, _superWeapon.Kind, "pickup did not bring the rocket launcher");
            Assert.IsTrue(_weapon.Holstered, "the normal weapon still fires");
            Assert.IsTrue(_launcher.GetComponentInChildren<SpriteRenderer>().enabled, "no rocket launcher in the hand");

            Enemy[] group =
            {
                Walker(center + new Vector2(4f, 0f)), Walker(center + new Vector2(4.6f, 0.6f)), Walker(center + new Vector2(4.6f, -0.6f)),
            };
            Enemy far = Walker(center + new Vector2(4f, 4f));
            int explosions = 0;
            _launcher.Exploded += _ => explosions++;

            _aim.AimInput = Vector2.right;
            float timeout = Time.realtimeSinceStartup + 3f;
            while (Time.realtimeSinceStartup < timeout && explosions == 0)
            {
                yield return null;
            }

            _aim.AimInput = Vector2.zero;
            Assert.Greater(explosions, 0, "no rocket exploded");
            foreach (Enemy enemy in group)
            {
                Assert.IsTrue(enemy.IsDead, $"{enemy.name} in the radius survived");
            }

            Assert.IsFalse(far.IsDead, "an enemy outside the radius died");
            Assert.AreEqual(hearts, _health.Current, "W12: the player was hurt");

            // Real time: the hit stop of the game feel resets the time scale, so it cannot be sped up.
            float end = Time.realtimeSinceStartup + 13f;
            while (Time.realtimeSinceStartup < end && _superWeapon.IsActive)
            {
                yield return null;
            }

            // The weapon hides its hand in its next update, which may come after the timer's in this frame.
            yield return null;
            Assert.IsFalse(_superWeapon.IsActive, "the rocket launcher stayed after 12 s");
            Assert.IsFalse(_weapon.Holstered, "the old weapon does not fire again");
            Assert.AreSame(old, _weapon.ActiveWeapon, "the old weapon is not active again");
            Assert.IsFalse(_launcher.GetComponentInChildren<SpriteRenderer>().enabled, "rocket launcher still in the hand");
        }

        [UnityTest]
        public IEnumerator BlastOnThePlayer_HitsEnemies_NeverThePlayer()
        {
            int hearts = _health.Current;
            Vector2 center = _player.transform.position;
            Enemy next = Walker(center + new Vector2(0.8f, 0f));
            yield return Settle();

            _launcher.Explode(center);
            yield return null;

            Assert.IsTrue(next.IsDead, "the enemy next to the player survived the blast");
            Assert.AreEqual(hearts, _health.Current, "W12: the blast hurt the player");
        }

        [UnityTest]
        public IEnumerator Kills_CountForKillGates()
        {
            LevelKills kills = Object.FindFirstObjectByType<LevelKills>();
            int before = kills.Counter.Count;
            Vector2 center = _player.transform.position;
            Enemy a = Walker(center + new Vector2(3f, 0f));
            Enemy b = Walker(center + new Vector2(3.5f, 0.5f));
            yield return Settle();

            _player.Apply(Perk("RocketLauncher"));
            _launcher.Explode(a.AimPoint);
            yield return null;

            Assert.IsTrue(a.IsDead && b.IsDead, "the blast did not kill both walkers");
            Assert.AreEqual(before + 2, kills.Counter.Count, "the blast's kills did not count");
        }

        [UnityTest]
        public IEnumerator DoubleDamage_DoublesTheBlast()
        {
            Vector2 center = _player.transform.position;
            Enemy normal = Brute(center + new Vector2(3f, 0f));
            yield return Settle();
            int full = normal.CurrentHealth;
            _launcher.Explode(normal.AimPoint);
            int normalDamage = full - normal.CurrentHealth;
            normal.Kill();

            _player.Apply(Perk("DoubleDamage"));
            Enemy raised = Brute(center + new Vector2(3f, 0f));
            yield return Settle();
            _launcher.Explode(raised.AimPoint);
            int raisedDamage = full - raised.CurrentHealth;

            Assert.AreEqual(40, normalDamage, "the blast did not deal the configured damage");
            Assert.AreEqual(normalDamage * 2, raisedDamage, "double damage did not apply to the blast");
        }

        [UnityTest]
        public IEnumerator InstaKill_KillsABrute()
        {
            Vector2 center = _player.transform.position;
            Enemy brute = Brute(center + new Vector2(3f, 0f));
            yield return Settle();
            _player.Apply(Perk("InstaKill"));

            _launcher.Explode(brute.AimPoint);
            yield return null;

            Assert.IsTrue(brute.IsDead, "insta-kill did not apply to the blast");
        }

        [UnityTest]
        public IEnumerator Death_EndsTheLauncher_AndClearsRocketsInFlight()
        {
            _player.Apply(Perk("RocketLauncher"));
            // Nothing in the way, so the first rocket is still in the air.
            _aim.AimInput = Vector2.down;
            float timeout = Time.realtimeSinceStartup + 1f;
            while (Time.realtimeSinceStartup < timeout && _launcher.RocketsInFlight == 0)
            {
                yield return null;
            }

            Assert.Greater(_launcher.RocketsInFlight, 0, "no rocket in the air");

            _health.TakeDamage(99);
            yield return null;

            Assert.IsFalse(_superWeapon.IsActive, "the rocket launcher stayed after death");
            Assert.AreEqual(0, _launcher.RocketsInFlight, "rockets fly on after death");
        }

        [UnityTest]
        public IEnumerator Boss_TakesTheFullDamage()
        {
            yield return SceneManager.LoadSceneAsync("Boss_Test", LoadSceneMode.Single);
            yield return null;
            Prepare();
            var fight = Object.FindAnyObjectByType<BossFight>();
            Boss boss = fight.Bodies[0];
            var body = _player.GetComponent<Rigidbody2D>();
            body.position = new Vector2(12f, 20f);
            body.transform.position = body.position;
            yield return null;
            yield return null;
            Assert.IsTrue(fight.IsRunning, "fight did not start in the arena");
            int full = boss.CurrentHealth;

            _launcher.Explode(boss.AimPoint);

            Assert.AreEqual(40, full - boss.CurrentHealth, "W14: the boss did not take the full blast damage");
        }

        [UnityTest]
        public IEnumerator EveryLevelAndTheEndlessArena_HaveARocketLauncher([ValueSource(nameof(Scenes))] string scene)
        {
            yield return SceneManager.LoadSceneAsync(scene, LoadSceneMode.Single);
            yield return null;
            Assert.AreEqual(1, Object.FindObjectsByType<RocketLauncher>(FindObjectsSortMode.None).Length, scene);
            var superWeapon = Object.FindAnyObjectByType<PlayerSuperWeapon>();
            Assert.IsNotNull(superWeapon, scene);
            superWeapon.Activate(SuperWeaponKind.RocketLauncher);
            Assert.AreEqual(SuperWeaponKind.RocketLauncher, superWeapon.Kind, $"{scene}: no rocket launcher config");
        }

        private static readonly string[] Scenes =
        {
            "Level_1-1", "Level_1-2", "Level_1-3", "Level_2-1", "Level_2-2", "Level_2-3",
            "Level_3-1", "Level_3-2", "Level_3-3", "Endless_Arena",
        };

        private void Prepare()
        {
            _player = Object.FindFirstObjectByType<PlayerPerks>();
            _aim = _player.GetComponent<PlayerAim>();
            _weapon = _player.GetComponent<PlayerWeapon>();
            _health = _player.GetComponent<PlayerHealth>();
            _superWeapon = _player.GetComponent<PlayerSuperWeapon>();
            Assert.IsNotNull(_superWeapon, "player has no PlayerSuperWeapon");
            _launcher = _player.GetComponentInChildren<RocketLauncher>();
            Assert.IsNotNull(_launcher, "player has no RocketLauncher");
            var director = Object.FindFirstObjectByType<WaveDirector>();
            if (director != null)
            {
                director.enabled = false;
            }

            Object.FindFirstObjectByType<LevelRestarter>().enabled = false;
            _player.GetComponent<PlayerMoveInput>().enabled = false;
            _player.GetComponent<PlayerAimInput>().enabled = false;
        }

        // Physics picks up the spawned enemies at their spot, so the blast's overlap query finds them.
        private static IEnumerator Settle()
        {
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
        }

        private static Enemy Walker(Vector2 position)
        {
            return Spawn(EnemyPools.Walker, position);
        }

        private static Enemy Brute(Vector2 position)
        {
            return Spawn(EnemyPools.Brute, position);
        }

        private static Enemy Spawn(string type, Vector2 position)
        {
            Enemy enemy = EnemyPools.Of(type).Spawn(position);
            enemy.GetComponent<EnemyMovement>().Stop();
            return enemy;
        }

        private static PerkDefinition Perk(string asset)
        {
            PerkDefinition perk = null;
#if UNITY_EDITOR
            perk = UnityEditor.AssetDatabase.LoadAssetAtPath<PerkDefinition>($"Assets/_Project/Data/Perks/{asset}.asset");
#endif
            Assert.IsNotNull(perk, $"{asset} perk asset not found");
            return perk;
        }
    }
}
