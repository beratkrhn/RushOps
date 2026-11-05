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
