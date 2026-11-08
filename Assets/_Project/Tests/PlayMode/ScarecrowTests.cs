using System.Collections;
using NUnit.Framework;
using RushOps.Enemies;
using RushOps.Levels;
using RushOps.Navigation;
using RushOps.Perks;
using RushOps.Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RushOps.Tests.PlayMode
{
    /// <summary>The scarecrow of the scarecrow perk (issue #246).</summary>
    public class ScarecrowTests
    {
        private const string SceneName = "Arena_Test";

        // Real time throughout: the hit stop of the game feel resets the time scale, so it cannot be sped up.
        private const float BlastTimeout = 10f;

        private Scarecrow _scarecrow;
        private PerkPickupPool _pickups;
        private PlayerHealth _health;
        private FlowFieldNavigator _navigator;

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
        public IEnumerator Pickup_LuresEnemiesAwayFromThePlayer_ThenTheBlastKillsThem()
        {
            Assert.IsFalse(_scarecrow.IsActive, "scarecrow stands before the pickup");
            Vector2 player = _health.transform.position;
            Vector2 spot = player + new Vector2(0f, -5f);

            _pickups.Spawn(spot, Scarecrow()).Collect();
            yield return null;

            Assert.IsTrue(_scarecrow.IsActive, "pickup did not place the scarecrow");
            Assert.Less(((Vector2)_scarecrow.transform.position - spot).magnitude, 0.75f, "scarecrow is not at the pickup spot");
            Assert.IsTrue(_scarecrow.GetComponent<SpriteRenderer>().enabled, "scarecrow is not visible");
            Assert.IsTrue(_navigator.HasLure, "the navigator does not lure to the scarecrow");

            // Between the player and the scarecrow: chasing the player would mean walking up, the lure pulls them down.
            Enemy left = EnemyPools.Of(EnemyPools.Walker).Spawn(player + new Vector2(-1.5f, -1.5f));
            Enemy right = EnemyPools.Of(EnemyPools.Walker).Spawn(player + new Vector2(1.5f, -1.5f));
            yield return new WaitForSeconds(0.5f);

            Assert.Less(left.GetComponent<EnemyMovement>().Velocity.y, 0f, "left walker does not head for the scarecrow");
            Assert.Less(right.GetComponent<EnemyMovement>().Velocity.y, 0f, "right walker does not head for the scarecrow");

            int hearts = _health.Current;
            yield return WaitForBlast();

            Assert.IsTrue(Gone(left) && Gone(right), "the blast did not kill the lured walkers");
            Assert.AreEqual(hearts, _health.Current, "the player lost health");
            Assert.IsFalse(_navigator.HasLure, "enemies are still lured after the blast");
            Assert.IsFalse(_scarecrow.GetComponent<SpriteRenderer>().enabled, "scarecrow is still visible");
        }

        [UnityTest]
        public IEnumerator Blast_SparesThePlayerInsideTheRadius_AndItsKillsCount()
        {
            LevelKills kills = Object.FindFirstObjectByType<LevelKills>();
            Vector2 player = _health.transform.position;
            _pickups.Spawn(player, Scarecrow()).Collect();

            // Held in place inside the radius, out of reach of the player so no contact damage interferes.
            Enemy a = EnemyPools.Of(EnemyPools.Walker).Spawn(player + new Vector2(2f, 0f));
            Enemy b = EnemyPools.Of(EnemyPools.Runner).Spawn(player + new Vector2(-2f, 0.5f));
            a.GetComponent<EnemyMovement>().Stop();
            b.GetComponent<EnemyMovement>().Stop();
            int before = kills.Counter.Count;
            int hearts = _health.Current;

            yield return WaitForBlast();

            Assert.IsTrue(a.IsDead || Gone(a), "walker in the radius survived");
            Assert.IsTrue(b.IsDead || Gone(b), "runner in the radius survived");
            Assert.AreEqual(before + 2, kills.Counter.Count, "the blast's kills did not count");
            Assert.AreEqual(hearts, _health.Current, "the player inside the radius lost health");
        }

        [UnityTest]
        public IEnumerator EnemyOutsideTheRadius_IsNotHit()
        {
            Vector2 player = _health.transform.position;
            _pickups.Spawn(player, Scarecrow()).Collect();
            Enemy far = EnemyPools.Of(EnemyPools.Walker).Spawn(player + new Vector2(0f, 7f));
            far.GetComponent<EnemyMovement>().Stop();
            int health = far.CurrentHealth;

            yield return WaitForBlast();

            Assert.AreEqual(health, far.CurrentHealth, "an enemy beyond the radius was hit");
        }

        [UnityTest]
        public IEnumerator SecondPickup_MovesAndRestartsIt_AndKeepsOne()
        {
            Vector2 player = _health.transform.position;
            _pickups.Spawn(player + new Vector2(0f, -4f), Scarecrow()).Collect();
            yield return new WaitForSeconds(0.5f);
            float before = _scarecrow.Remaining;

            Vector2 second = player + new Vector2(0f, 4f);
            _pickups.Spawn(second, Scarecrow()).Collect();
            yield return null;

            Assert.Greater(_scarecrow.Remaining, before, "second pickup did not restart the timer");
            Assert.Less(((Vector2)_scarecrow.transform.position - second).magnitude, 0.75f, "scarecrow did not move to the new spot");
            Assert.Less((_navigator.LurePosition - second).magnitude, 0.75f, "the lure stayed at the old spot");
            Assert.AreEqual(1, Object.FindObjectsByType<Scarecrow>(FindObjectsSortMode.None).Length);
        }

        [UnityTest]
        public IEnumerator Scarecrow_DisappearsWithoutBlastWhenThePlayerDies()
        {
            Vector2 player = _health.transform.position;
            _pickups.Spawn(player + new Vector2(0f, -4f), Scarecrow()).Collect();
            bool blasted = false;
            _scarecrow.Detonated += _ => blasted = true;
            yield return null;

            _health.TakeDamage(99);
            yield return null;

            Assert.IsFalse(_scarecrow.IsActive, "scarecrow stayed after the player died");
            Assert.IsFalse(_navigator.HasLure, "enemies are still lured after the player died");
            Assert.IsFalse(blasted, "the scarecrow exploded on the player's death");
        }

        [UnityTest]
        public IEnumerator Warning_BlinksInTheLastSecond()
        {
            _pickups.Spawn((Vector2)_health.transform.position + new Vector2(0f, -4f), Scarecrow()).Collect();
            var renderer = _scarecrow.GetComponent<SpriteRenderer>();
            Sprite normal = renderer.sprite;

            float timeout = Time.realtimeSinceStartup + BlastTimeout;
            while (Time.realtimeSinceStartup < timeout && _scarecrow.IsActive && _scarecrow.Remaining > 0.95f)
            {
                yield return null;
            }

            yield return null;
            Assert.IsTrue(_scarecrow.IsActive);
            Assert.AreNotEqual(normal, renderer.sprite, "no warning look at the start of the last second");
            Assert.AreEqual("Scarecrow_Warning", renderer.sprite.name);
        }

        [UnityTest]
        public IEnumerator Fountain_PushesTheScarecrowOntoAWalkableCell()
        {
            yield return SceneManager.LoadSceneAsync("Endless_Arena", LoadSceneMode.Single);
            yield return null;
            Prepare();
            Vector2 fountain = GameObject.Find("Fountain").transform.position;

            _pickups.Spawn(fountain, Scarecrow()).Collect();
            yield return null;

            Assert.IsTrue(_scarecrow.IsActive);
            Vector2 placed = _scarecrow.transform.position;
            Assert.Greater((placed - fountain).magnitude, 1f, "scarecrow stands in the fountain");
            Assert.AreNotEqual(
                FlowField.Unreachable, _navigator.Field.Distance(_navigator.CellOf(placed)), "the spot is not walkable");
        }

        [UnityTest]
        public IEnumerator EveryLevelAndTheEndlessArena_HaveAScarecrow([ValueSource(nameof(Scenes))] string scene)
        {
            yield return SceneManager.LoadSceneAsync(scene, LoadSceneMode.Single);
            yield return null;
            Assert.AreEqual(1, Object.FindObjectsByType<Scarecrow>(FindObjectsSortMode.None).Length, scene);
        }

        private static readonly string[] Scenes =
        {
            "Level_1-1", "Level_1-2", "Level_1-3", "Level_2-1", "Level_2-2", "Level_2-3",
            "Level_3-1", "Level_3-2", "Level_3-3", "Endless_Arena",
        };

        private IEnumerator WaitForBlast()
        {
            bool blasted = false;
            void OnDetonated(Vector2 _) => blasted = true;
            _scarecrow.Detonated += OnDetonated;
            float timeout = Time.realtimeSinceStartup + BlastTimeout;
            while (Time.realtimeSinceStartup < timeout && !blasted)
            {
                yield return null;
            }

            _scarecrow.Detonated -= OnDetonated;
            Assert.IsTrue(blasted, "the scarecrow did not explode");
            yield return null;
        }

        private static bool Gone(Enemy enemy)
        {
            return enemy.IsDead || !enemy.gameObject.activeSelf;
        }

        private void Prepare()
        {
            _scarecrow = Object.FindFirstObjectByType<Scarecrow>();
            Assert.IsNotNull(_scarecrow, $"{SceneManager.GetActiveScene().name} has no Scarecrow");
            _pickups = Object.FindFirstObjectByType<PerkPickupPool>();
            _navigator = Object.FindFirstObjectByType<FlowFieldNavigator>();
            var perks = Object.FindFirstObjectByType<PlayerPerks>();
            _health = perks.GetComponent<PlayerHealth>();
            var director = Object.FindFirstObjectByType<WaveDirector>();
            if (director != null)
            {
                director.enabled = false;
            }

            var restarter = Object.FindFirstObjectByType<LevelRestarter>();
            if (restarter != null)
            {
                restarter.enabled = false;
            }

            perks.GetComponent<PlayerMoveInput>().enabled = false;
            perks.GetComponent<PlayerAimInput>().enabled = false;
            perks.GetComponent<PlayerWeapon>().enabled = false;
        }

        // Not in the drop pool yet (issue #247), so the definition comes straight from its asset.
        private static PerkDefinition Scarecrow()
        {
            PerkDefinition perk = null;
#if UNITY_EDITOR
            perk = UnityEditor.AssetDatabase.LoadAssetAtPath<PerkDefinition>("Assets/_Project/Data/Perks/Scarecrow.asset");
#endif
            Assert.IsNotNull(perk, "Scarecrow perk asset not found");
            return perk;
        }
    }
}
