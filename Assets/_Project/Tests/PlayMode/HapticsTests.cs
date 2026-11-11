using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using RushOps.Combat;
using RushOps.Enemies;
using RushOps.Feel;
using RushOps.Levels;
using RushOps.Player;
using RushOps.Save;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RushOps.Tests.PlayMode
{
    /// <summary>Vibration on damage and explosions in the arena (issue #300, F-AUD-05, N32).</summary>
    public class HapticsTests
    {
        private const string ExplosionPath = "Assets/_Project/Data/Enemies/BoomerExplosion.asset";

        private Haptics _haptics;
        private PlayerHealth _player;
        private ExplosionSystem _explosions;
        private readonly List<HapticKind> _pulses = new List<HapticKind>();

        [UnitySetUp]
        public IEnumerator LoadScene()
        {
            SaveGameIsolation.Reset();
            yield return SceneManager.LoadSceneAsync("Arena_Test", LoadSceneMode.Single);
            yield return null;
            Object.FindFirstObjectByType<LevelRestarter>().enabled = false;
            Object.FindFirstObjectByType<WaveDirector>().enabled = false;
            _haptics = Object.FindFirstObjectByType<Haptics>();
            Assert.IsNotNull(_haptics, "the arena has no vibration");
            _player = Object.FindFirstObjectByType<PlayerHealth>();
            _explosions = Object.FindFirstObjectByType<ExplosionSystem>();
            _pulses.Clear();
            _haptics.Pulsed += _pulses.Add;
        }

        [TearDown]
        public void Unhook()
        {
            if (_haptics != null)
            {
                _haptics.Pulsed -= _pulses.Add;
            }

            SaveGameIsolation.Reset();
        }

        [UnityTest]
        public IEnumerator LosingAHeart_Vibrates()
        {
            Assert.IsTrue(_player.TakeDamage(1));
            yield return null;

            CollectionAssert.AreEqual(new[] { HapticKind.Damage }, _pulses);
        }

        [UnityTest]
        public IEnumerator ExplosionOnScreen_Vibrates_FarAway_DoesNot()
        {
            ExplosionDefinition explosion = null;
#if UNITY_EDITOR
            explosion = UnityEditor.AssetDatabase.LoadAssetAtPath<ExplosionDefinition>(ExplosionPath);
#endif
            Assert.IsNotNull(explosion);
            Vector2 player = _player.transform.position;

            _explosions.Explode(player + new Vector2(200f, 200f), explosion);
            yield return null;
            CollectionAssert.IsEmpty(_pulses, "an explosion far off screen should not vibrate");

            // Out of the blast radius, so only the explosion itself vibrates, not a lost heart.
            _explosions.Explode(player + new Vector2(4f, 0f), explosion);
            yield return null;
            CollectionAssert.AreEqual(new[] { HapticKind.Explosion }, _pulses);
        }

        [UnityTest]
        public IEnumerator SwitchedOff_NothingVibrates()
        {
            SettingsRules.SetVibration(SaveGameIsolation.Service.Data.Settings, false);

            Assert.IsTrue(_player.TakeDamage(1));
            yield return null;

            CollectionAssert.IsEmpty(_pulses);
        }
    }
}
