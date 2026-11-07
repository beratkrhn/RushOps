using System.Collections;
using NUnit.Framework;
using RushOps.Bosses;
using RushOps.Combat;
using RushOps.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RushOps.Tests.PlayMode
{
    /// <summary>
    /// Drives a level hands-free for performance measurements (issues #189, #190): the player cannot die, holds fire
    /// with a chosen weapon and, in a boss level, stands in the arena.
    /// </summary>
    internal sealed class StressScenario
    {
        /// <summary>Scene with the heaviest case (built by <c>StressTestSceneBuilder</c>).</summary>
        public const string StressScene = "Stress_Test";

        /// <summary>Weapon with the most shots per second.</summary>
        public const string FastestWeapon = "DualPistols";

        /// <summary>Arena of the endless mode (issue #212).</summary>
        public const string EndlessScene = "Endless_Arena";

        // So many hit points that the extra boss of the endless stress case outlasts any measurement.
        private const int EndlessBossHealth = 1000000;

        private static readonly Vector2 StressArenaPoint = new Vector2(14f, 22f);

        private PlayerHealth _health;
        private PlayerAim _aim;
        private Transform _player;

        /// <summary>Boss fight of the scene, null in a regular level.</summary>
        public BossFight Fight { get; private set; }

        /// <summary>Loads <paramref name="scene"/> with an empty save game and takes over the player.</summary>
        public IEnumerator Load(string scene, string weapon = null)
        {
            SaveGameIsolation.Reset();
            yield return SceneManager.LoadSceneAsync(scene, LoadSceneMode.Single);
            yield return null;

            var restarter = Object.FindAnyObjectByType<LevelRestarter>();
            if (restarter != null)
            {
                // Death must not reload the scene in the middle of a measurement.
                restarter.enabled = false;
            }

            _health = Object.FindAnyObjectByType<PlayerHealth>();
            if (_health == null)
            {
                yield break;
            }

            _player = _health.transform;
            _aim = _health.GetComponent<PlayerAim>();
            _health.GetComponent<PlayerAimInput>().enabled = false;
            _health.GetComponent<PlayerMoveInput>().enabled = false;
            Fight = Object.FindAnyObjectByType<BossFight>();
            if (weapon != null)
            {
                Equip(weapon);
            }
        }

        /// <summary>Puts the player into the arena of <see cref="StressScene"/>, which starts the fight.</summary>
        public IEnumerator EnterStressArena()
        {
            Teleport(StressArenaPoint);
            yield return null;
            yield return null;
            Assert.IsNotNull(Fight, "the stress scene has no boss fight");
            Assert.IsTrue(Fight.IsRunning, "the fight did not start in the arena");
        }

        /// <summary>
        /// Turns the loaded <see cref="EndlessScene"/> into its heaviest case (issue #212, F-END-04, N3): starts the
        /// first regular round at the cap of enemies alive at once, gives the player <paramref name="weapon"/> and the
        /// perks of <paramref name="perkIds"/>, and on top lets the Mega-Boomer fight with puddles and explosions,
        /// which no real round does (boss rounds have no regular enemies). The boss cannot fall during a measurement.
        /// </summary>
        public IEnumerator EnterEndlessStress(string weapon, params string[] perkIds)
        {
            var controller = Object.FindAnyObjectByType<RushOps.Endless.EndlessRunController>();
            Assert.IsNotNull(controller, "no EndlessRunController in the arena");
            // The arena started a run of its own in Start; its rules tell the round.
            RushOps.Endless.EndlessRules rules = controller.Rules;
            Assert.IsNotNull(rules, "the arena did not start a run");
            FullRound = FirstRegularRoundAtCap(rules);
            controller.BeginRun(FullRound);
            Equip(weapon);
            float end = Time.realtimeSinceStartup + 30f;
            while (controller.Run.Phase != RushOps.Endless.EndlessPhase.Running)
            {
                Assert.Less(Time.realtimeSinceStartup, end, $"timeout waiting for round {FullRound}");
                Tick(false);
                yield return null;
            }

            BossDefinition mega = null;
            foreach (BossDefinition boss in rules.Config.Bosses)
            {
                if (boss is MegaBoomerDefinition)
                {
                    mega = boss;
                }
            }

            Assert.IsNotNull(mega, "the endless config has no Mega-Boomer");
            Fight = Object.FindAnyObjectByType<RushOps.Endless.EndlessBosses>().Spawn(mega, EndlessBossHealth);
            Assert.IsNotNull(Fight, "no Mega-Boomer template in the arena");
            Assert.IsTrue(Fight.IsRunning, "the extra boss fight did not start");

            // With the gun lowered the gates fill the arena up to the cap before the measurement starts.
            var director = Object.FindAnyObjectByType<RushOps.Enemies.WaveDirector>();
            while (director.AliveCount < director.MaxActive)
            {
                Assert.Less(Time.realtimeSinceStartup, end, $"timeout waiting for {director.MaxActive} enemies");
                Tick(false);
                yield return null;
            }

            foreach (string perkId in perkIds)
            {
                ApplyPerk(perkId);
            }
        }

        /// <summary>The round <see cref="EnterEndlessStress"/> plays: the first regular one at the cap.</summary>
        public int FullRound { get; private set; }

        private static int FirstRegularRoundAtCap(RushOps.Endless.EndlessRules rules)
        {
            int cap = Mathf.Max(rules.Config.MaxActiveCap, rules.Config.MaxActiveStart);
            for (int round = 1; round < 10000; round++)
            {
                if (rules.MaxActive(round) >= cap && !rules.IsBossRound(round) && rules.EnemyCount(round) >= cap)
                {
                    return round;
                }
            }

            Assert.Fail($"no round reaches the cap of {cap}");
            return 0;
        }

        /// <summary>Moves the player to <paramref name="position"/> (world units).</summary>
        public void Teleport(Vector2 position)
        {
            var body = _player.GetComponent<Rigidbody2D>();
            body.position = position;
            _player.position = position;
        }

        /// <summary>Gives the player <paramref name="perkId"/> as if he had picked it up.</summary>
        public void ApplyPerk(string perkId)
        {
            foreach (RushOps.Perks.PerkDefinition perk in Resources.FindObjectsOfTypeAll<RushOps.Perks.PerkDefinition>())
            {
                if (perk.Id == perkId)
                {
                    _player.GetComponent<RushOps.Perks.PlayerPerks>().Apply(perk);
                    return;
                }
            }

            Assert.Fail($"perk {perkId} is not loaded");
        }

        /// <summary>
        /// Call once per frame: refills the hearts and aims at the boss, or straight up without one. With
        /// <paramref name="fire"/> false the player lowers the gun.
        /// </summary>
        public void Tick(bool fire = true)
        {
            if (_health == null)
            {
                return;
            }

            _health.Heal(_health.Max);
            if (!fire)
            {
                _aim.AimInput = Vector2.zero;
                return;
            }

            Vector2 aim = Vector2.up;
            if (Fight != null && Fight.IsRunning)
            {
                for (int i = 0; i < Fight.Bodies.Count; i++)
                {
                    Boss body = Fight.Bodies[i];
                    if (body.IsDead || !body.isActiveAndEnabled)
                    {
                        continue;
                    }

                    Vector2 toBoss = (Vector2)body.transform.position - (Vector2)_player.position;
                    if (toBoss.sqrMagnitude > 0.01f)
                    {
                        aim = toBoss.normalized;
                    }

                    break;
                }
            }

            _aim.AimInput = aim;
        }

        private void Equip(string weapon)
        {
            foreach (WeaponDefinition definition in Resources.FindObjectsOfTypeAll<WeaponDefinition>())
            {
                if (definition.name == weapon)
                {
                    _player.GetComponent<PlayerWeapon>().Slots.Reset(definition);
                    return;
                }
            }

            Assert.Fail($"weapon {weapon} is not loaded");
        }
    }
}
