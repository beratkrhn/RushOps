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
    /// <summary>The flamethrower super weapon (issue #254).</summary>
    public class FlamethrowerTests
    {
        private const string SceneName = "Arena_Test";

        private PlayerPerks _player;
        private PlayerAim _aim;
        private PlayerWeapon _weapon;
        private PlayerSuperWeapon _superWeapon;
        private Flamethrower _flamethrower;

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
        public IEnumerator Pickup_BurnsEnemiesInTheCone_TheyBurnOn_ThenTheOldWeaponIsBack()
        {
            var old = _weapon.ActiveWeapon;
            Vector2 center = _player.transform.position;
            var pickups = Object.FindFirstObjectByType<PerkPickupPool>();
            pickups.Spawn(center + new Vector2(0f, -6f), Perk("Flamethrower")).Collect();
            yield return null;

            Assert.AreEqual(SuperWeaponKind.Flamethrower, _superWeapon.Kind, "pickup did not bring the flamethrower");
            Assert.IsTrue(_weapon.Holstered, "the normal weapon still fires");
            Assert.IsTrue(_flamethrower.GetComponentInChildren<SpriteRenderer>().enabled, "no flamethrower in the hand");

            Enemy[] inCone = { Brute(center + new Vector2(2f, 0f)), Brute(center + new Vector2(2.6f, 0.5f)), Brute(center + new Vector2(2.6f, -0.5f)) };
            Enemy behind = Brute(center + new Vector2(-2.5f, 0f));
            int full = behind.CurrentHealth;

            _aim.AimInput = Vector2.right;
            yield return WaitRealtime(1f);

            Assert.IsTrue(_flamethrower.IsFlaming, "no flame while aiming");
            foreach (Enemy enemy in inCone)
            {
                Assert.Less(enemy.CurrentHealth, full, $"{enemy.name} in the cone took no damage");
            }

            Assert.AreEqual(full, behind.CurrentHealth, "an enemy behind the player was burnt");
            Assert.IsTrue(_flamethrower.GetComponent<AudioSource>().isPlaying, "no flame sound");

            _aim.AimInput = Vector2.zero;
            yield return null;
            Assert.IsFalse(_flamethrower.IsFlaming);
            Assert.IsFalse(_flamethrower.GetComponent<AudioSource>().isPlaying, "flame sound did not stop");
            int afterFlame = inCone[0].CurrentHealth;
            Assert.IsTrue(_flamethrower.IsBurning(inCone[0]), "W13: no afterburn after the flame");
            yield return WaitRealtime(0.8f);
            Assert.Less(inCone[0].CurrentHealth, afterFlame, "W13: the afterburn did no damage");

            // Real time: the hit stop of the game feel resets the time scale, so it cannot be sped up.
            float end = Time.realtimeSinceStartup + 13f;
            while (Time.realtimeSinceStartup < end && _superWeapon.IsActive)
            {
                yield return null;
            }

            Assert.IsFalse(_superWeapon.IsActive, "the flamethrower stayed after 12 s");
            Assert.IsFalse(_weapon.Holstered, "the old weapon does not fire again");
            Assert.AreSame(old, _weapon.ActiveWeapon, "the old weapon is not active again");
            Assert.IsFalse(_flamethrower.GetComponentInChildren<SpriteRenderer>().enabled, "flamethrower still in the hand");
        }

        [UnityTest]
        public IEnumerator Kills_CountForKillGates()
        {
            LevelKills kills = Object.FindFirstObjectByType<LevelKills>();
            int before = kills.Counter.Count;
            Vector2 center = _player.transform.position;
            _player.Apply(Perk("Flamethrower"));
            Enemy walker = Spawn(EnemyPools.Walker, center + new Vector2(2f, 0f));

            _aim.AimInput = Vector2.right;
            float timeout = Time.realtimeSinceStartup + 3f;
            while (Time.realtimeSinceStartup < timeout && !walker.IsDead)
            {
                yield return null;
            }

            Assert.IsTrue(walker.IsDead, "the flame did not kill a walker");
            Assert.AreEqual(before + 1, kills.Counter.Count, "the flame's kill did not count");
        }

        [UnityTest]
        public IEnumerator InstaKill_KillsInTheFirstTick()
        {
            Vector2 center = _player.transform.position;
            _player.Apply(Perk("InstaKill"));
            _player.Apply(Perk("Flamethrower"));
            Enemy brute = Brute(center + new Vector2(2f, 0f));

            _aim.AimInput = Vector2.right;
            yield return WaitRealtime(0.5f);

            Assert.IsTrue(brute.IsDead, "insta-kill did not apply to the flame");
        }

        [UnityTest]
        public IEnumerator DoubleDamage_DoublesTheFlame()
        {
            Vector2 center = _player.transform.position;
            _player.Apply(Perk("Flamethrower"));
            var normal = new TickDamage(Brute(center + new Vector2(2f, 0f)));
            _aim.AimInput = Vector2.right;
            yield return normal.Wait();
            _aim.AimInput = Vector2.zero;
            normal.Enemy.Kill();

            _player.Apply(Perk("DoubleDamage"));
            var raised = new TickDamage(Brute(center + new Vector2(2f, 0f)));
            _aim.AimInput = Vector2.right;
            yield return raised.Wait();

            Assert.Greater(normal.Damage, 0, "the flame did no damage");
            Assert.GreaterOrEqual(raised.Damage, normal.Damage * 1.8f, $"double damage: {raised.Damage} vs {normal.Damage}");
        }

        // Damage of one full tick: the health lost between the second and the third hit (the first tick may be short).
        private sealed class TickDamage
        {
            private readonly int[] _health = new int[3];
            private int _hits;

            public TickDamage(Enemy enemy)
            {
                Enemy = enemy;
                enemy.Hit += OnHit;
            }

            public Enemy Enemy { get; }

            public int Damage => _health[1] - _health[2];

            public IEnumerator Wait()
            {
                float timeout = Time.realtimeSinceStartup + 3f;
                while (_hits < 3 && Time.realtimeSinceStartup < timeout)
                {
                    yield return null;
                }

                Enemy.Hit -= OnHit;
                Assert.AreEqual(3, _hits, "the flame did not hit three times");
            }

            private void OnHit(Enemy enemy)
            {
                if (_hits < _health.Length)
                {
                    _health[_hits++] = enemy.CurrentHealth;
                }
            }
        }

        [UnityTest]
        public IEnumerator Death_EndsTheFlamethrower_AndPutsOutTheFires()
        {
            Vector2 center = _player.transform.position;
            _player.Apply(Perk("Flamethrower"));
            Brute(center + new Vector2(2f, 0f));
            _aim.AimInput = Vector2.right;
            yield return WaitRealtime(0.3f);
            Assert.Greater(_flamethrower.BurningCount, 0);

            _player.GetComponent<PlayerHealth>().TakeDamage(99);
            yield return null;

            Assert.IsFalse(_superWeapon.IsActive, "the flamethrower stayed after death");
            Assert.IsFalse(_flamethrower.IsFlaming);
            Assert.AreEqual(0, _flamethrower.BurningCount, "fires burn on after death");
        }

        [UnityTest]
        public IEnumerator SecondPickup_RestartsTheTimer()
        {
            _player.Apply(Perk("Flamethrower"));
            yield return WaitRealtime(0.5f);
            float before = _superWeapon.Remaining;

            _player.Apply(Perk("Flamethrower"));

            Assert.Greater(_superWeapon.Remaining, before, "second pickup did not restart the timer");
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
            // Right in front of the player, so the flame reaches the boss at once.
            Vector2 spot = (Vector2)_player.transform.position + new Vector2(1.5f, 0f);
            boss.GetComponent<Rigidbody2D>().position = spot;
            boss.transform.position = spot;
            _player.Apply(Perk("Shield"));
            _player.Apply(Perk("Flamethrower"));
            int full = boss.CurrentHealth;

            float end = Time.realtimeSinceStartup + 1f;
            while (Time.realtimeSinceStartup < end)
            {
                Vector2 nozzle = (Vector2)_player.transform.position + Vector2.up * 0.7f;
                _aim.AimInput = (boss.AimPoint - nozzle).normalized;
                yield return null;
            }

            // At least four ticks of 0.2 s at 90 per second: no reduction for bosses (W14).
            Assert.GreaterOrEqual(full - boss.CurrentHealth, 72, "the boss did not take the full flame damage");
        }

        [UnityTest]
        public IEnumerator EveryLevelAndTheEndlessArena_HaveAFlamethrower([ValueSource(nameof(Scenes))] string scene)
        {
            yield return SceneManager.LoadSceneAsync(scene, LoadSceneMode.Single);
            yield return null;
            Assert.AreEqual(1, Object.FindObjectsByType<Flamethrower>(FindObjectsSortMode.None).Length, scene);
            Assert.IsNotNull(Object.FindAnyObjectByType<PlayerSuperWeapon>(), scene);
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
            _superWeapon = _player.GetComponent<PlayerSuperWeapon>();
            Assert.IsNotNull(_superWeapon, "player has no PlayerSuperWeapon");
            _flamethrower = _player.GetComponentInChildren<Flamethrower>();
            Assert.IsNotNull(_flamethrower, "player has no Flamethrower");
            var director = Object.FindFirstObjectByType<WaveDirector>();
            if (director != null)
            {
                director.enabled = false;
            }

            Object.FindFirstObjectByType<LevelRestarter>().enabled = false;
            _player.GetComponent<PlayerMoveInput>().enabled = false;
            _player.GetComponent<PlayerAimInput>().enabled = false;
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

        private static IEnumerator WaitRealtime(float seconds)
        {
            float end = Time.realtimeSinceStartup + seconds;
            while (Time.realtimeSinceStartup < end)
            {
                yield return null;
            }
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
