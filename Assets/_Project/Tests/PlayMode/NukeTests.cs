using System.Collections;
using NUnit.Framework;
using RushOps.Cameras;
using RushOps.Enemies;
using RushOps.Levels;
using RushOps.Perks;
using RushOps.Player;
using RushOps.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RushOps.Tests.PlayMode
{
    public class NukeTests
    {
        private const string SceneName = "Arena_Test";

        private NukeSystem _nuke;
        private CameraFollow _camera;
        private PerkPickupPool _pickups;
        private LevelKills _kills;

        [UnitySetUp]
        public IEnumerator LoadScene()
        {
            yield return SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);
            yield return null;

            _nuke = Object.FindFirstObjectByType<NukeSystem>();
            Assert.IsNotNull(_nuke, "Arena_Test has no NukeSystem");
            _camera = Object.FindFirstObjectByType<CameraFollow>();
            _pickups = Object.FindFirstObjectByType<PerkPickupPool>();
            _kills = Object.FindFirstObjectByType<LevelKills>();
            Object.FindFirstObjectByType<WaveDirector>().enabled = false;
            Object.FindFirstObjectByType<LevelRestarter>().enabled = false;
            var player = Object.FindFirstObjectByType<PlayerPerks>();
            player.GetComponent<PlayerMoveInput>().enabled = false;
            player.GetComponent<PlayerAimInput>().enabled = false;
            player.GetComponent<PlayerWeapon>().enabled = false;
            // Nothing may come close to the player and touch it during the test.
            yield return new WaitForSeconds(0.2f);
        }

        [TearDown]
        public void RestoreTime()
        {
            Time.timeScale = 1f;
        }

        [Test]
        public void Definition_IsRareInstantNuke()
        {
            PerkDefinition nuke = Nuke();
            Assert.AreEqual(PerkKind.Instant, nuke.Kind);
            Assert.AreEqual(3f, nuke.Weight, 1e-5f);
        }

        [UnityTest]
        public IEnumerator CollectedNuke_KillsVisibleEnemies_SparesOffscreenOnes()
        {
            Vector2 view = _camera.transform.position;
            Vector2 half = _camera.HalfExtents;
            Enemy visibleA = Place(EnemyPools.Walker, view + new Vector2(-2f, 3f));
            Enemy visibleB = Place(EnemyPools.Brute, view + new Vector2(2f, -3f));
            Enemy offscreen = Place(EnemyPools.Walker, OffscreenPoint(view, half));
            yield return null;

            _pickups.Spawn(view + new Vector2(0f, -6f), Nuke()).Collect();
            yield return null;

            Assert.IsTrue(visibleA.IsDead || !visibleA.gameObject.activeSelf, "visible walker survived the nuke");
            Assert.IsTrue(visibleB.IsDead || !visibleB.gameObject.activeSelf, "visible brute survived the nuke");
            Assert.IsFalse(offscreen.IsDead, "enemy outside the view was killed");
            Assert.IsTrue(offscreen.gameObject.activeSelf);
        }

        [UnityTest]
        public IEnumerator NukeKills_CountForKillGates()
        {
            Vector2 view = _camera.transform.position;
            Place(EnemyPools.Walker, view + new Vector2(-2f, 3f));
            Place(EnemyPools.Runner, view + new Vector2(2f, 3f));
            Place(EnemyPools.Walker, view + new Vector2(0f, -4f));
            yield return null;
            int before = _kills.Counter.Count;

            int killed = _nuke.Detonate();

            Assert.AreEqual(3, killed);
            Assert.AreEqual(before + 3, _kills.Counter.Count, "nuke kills must count for kill gates");
        }

        [UnityTest]
        public IEnumerator Nuke_FlashesScreenAndShakesCamera()
        {
            var flash = Object.FindFirstObjectByType<ScreenFlash>();
            Assert.IsNotNull(flash, "Arena_Test has no ScreenFlash");
            Assert.AreEqual(0f, flash.CurrentAlpha);

            _nuke.Detonate();
            yield return null;

            Assert.Greater(flash.CurrentAlpha, 0.3f, "screen did not flash");
            bool shook = false;
            float shakeTimeout = Time.realtimeSinceStartup + 0.3f;
            while (Time.realtimeSinceStartup < shakeTimeout && !shook)
            {
                shook = _camera.ShakeOffset.sqrMagnitude > 1e-6f;
                yield return null;
            }

            Assert.IsTrue(shook, "camera did not shake");
            float timeout = Time.realtimeSinceStartup + 2f;
            while (Time.realtimeSinceStartup < timeout && flash.CurrentAlpha > 0f)
            {
                yield return null;
            }

            Assert.AreEqual(0f, flash.CurrentAlpha, "flash did not fade out");
        }

        // Above or below the view, whichever lies inside the level.
        private Vector2 OffscreenPoint(Vector2 view, Vector2 half)
        {
            var above = new Vector2(view.x, view.y + half.y + 3f);
            return _camera.Bounds.Contains(above) ? above : new Vector2(view.x, view.y - half.y - 3f);
        }

        private static Enemy Place(string type, Vector2 position)
        {
            Enemy enemy = EnemyPools.Of(type).Spawn(position);
            enemy.GetComponent<EnemyMovement>().Stop();
            return enemy;
        }

        private PerkDefinition Nuke()
        {
            foreach (PerkDefinition perk in _pickups.Config.Perks)
            {
                if (perk.Effect == PerkEffect.Nuke)
                {
                    return perk;
                }
            }

            Assert.Fail("LootConfig has no nuke");
            return null;
        }
    }
}
