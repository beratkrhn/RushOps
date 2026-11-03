using System.Collections;
using NUnit.Framework;
using RushOps.Enemies;
using RushOps.Levels;
using RushOps.Navigation;
using RushOps.Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RushOps.Tests.PlayMode
{
    public class BruteTests
    {
        private const string SceneName = "Arena_Test";

        private EnemyPool _brutes;
        private EnemyPool _walkers;
        private FlowFieldNavigator _navigator;
        private PlayerHealth _player;

        [UnitySetUp]
        public IEnumerator LoadScene()
        {
            yield return SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);
            yield return null;

            _brutes = EnemyPools.Of(EnemyPools.Brute);
            _walkers = EnemyPools.Of(EnemyPools.Walker);
            _navigator = Object.FindFirstObjectByType<FlowFieldNavigator>();
            _player = Object.FindFirstObjectByType<PlayerHealth>();
            // These tests place brutes themselves; touching the player must not reload the scene.
            Object.FindFirstObjectByType<WaveDirector>().enabled = false;
            Object.FindFirstObjectByType<LevelRestarter>().enabled = false;
            _player.GetComponent<PlayerMoveInput>().enabled = false;
            _player.GetComponent<PlayerAimInput>().enabled = false;
        }

        [Test]
        public void Definition_IsSlowToughAndHeavy()
        {
            EnemyDefinition brute = _brutes.Definition;
            EnemyDefinition walker = _walkers.Definition;

            Assert.Less(brute.MoveSpeed, walker.MoveSpeed, "brute must be slower than the walker");
            Assert.Greater(brute.Health, walker.Health, "brute must be tougher than the walker");
            Assert.AreEqual(2, brute.ContactDamage);
            Assert.Greater(brute.Points, walker.Points);
            Assert.Greater(brute.Mass, walker.Mass, "brute must outweigh the walker to shove it aside");
            Assert.Less(brute.KnockbackScale, 0.5f, "hits must barely push the brute back");
            Assert.IsFalse(brute.IsBoss);
        }

        [UnityTest]
        public IEnumerator Brute_HasBiggerHitboxThanWalker()
        {
            Enemy brute = _brutes.Spawn(new Vector2(5.5f, 5.5f));
            Enemy walker = _walkers.Spawn(new Vector2(35.5f, 5.5f));
            yield return null;

            Assert.Greater(Radius(brute), Radius(walker) * 1.4f);
            Assert.Less(Radius(brute), 0.5f, "the body must stay below half a tile to follow paths along walls");
        }

        [Test]
        public void Waves_UseBrutes()
        {
            WaveDefinition waves = Object.FindFirstObjectByType<Zone>().Waves;
            int brutes = 0;
            foreach (Wave wave in waves.Waves)
            {
                foreach (WaveEntry entry in wave.Entries)
                {
                    if (entry.Enemy == _brutes.Definition)
                    {
                        brutes += entry.Count;
                    }
                }
            }

            Assert.Greater(brutes, 0, "Arena_Test waves bring no brutes");
        }

        [UnityTest]
        public IEnumerator Hit_KnocksBruteBackFarLessThanWalker()
        {
            // Player to the left, so both chase towards -X and the hits push them towards +X.
            Teleport(_player.GetComponent<Rigidbody2D>(), new Vector2(3.5f, 24.5f));
            yield return null;
            Enemy brute = _brutes.Spawn(new Vector2(12.5f, 24.5f));
            Enemy walker = _walkers.Spawn(new Vector2(12.5f, 27.5f));
            yield return new WaitForFixedUpdate();

            brute.TakeHit(1, Vector2.right);
            walker.TakeHit(1, Vector2.right);
            yield return new WaitForFixedUpdate();

            float bruteSpeed = brute.GetComponent<EnemyMovement>().Velocity.x;
            float walkerSpeed = walker.GetComponent<EnemyMovement>().Velocity.x;
            Assert.Greater(walkerSpeed, 0f, "walker was not knocked back");
            Assert.Less(bruteSpeed, walkerSpeed * 0.5f, "brute knocked back almost as hard as the walker");
        }

        [UnityTest]
        public IEnumerator Touch_CostsTwoHearts()
        {
            Teleport(_player.GetComponent<Rigidbody2D>(), new Vector2(35.5f, 22.5f));
            yield return null;
            int before = _player.Current;
            _brutes.Spawn(new Vector2(35.5f, 20.5f));

            float timeout = Time.time + 4f;
            while (Time.time < timeout && _player.Current == before)
            {
                yield return null;
            }

            Assert.AreEqual(before - 2, _player.Current);
        }

        [UnityTest]
        public IEnumerator Brute_WalkerInTheWay_ShovesItAside()
        {
            // A free column along the east wall; the walker stands still a little off the brute's line. Reaching the
            // player needs the walker out of the way: pushed ahead, it would stay wedged between brute and player.
            Teleport(_player.GetComponent<Rigidbody2D>(), new Vector2(35.5f, 26.5f));
            yield return null;
            Enemy walker = _walkers.Spawn(new Vector2(35.7f, 22.5f));
            walker.GetComponent<EnemyMovement>().Stop();
            Enemy brute = _brutes.Spawn(new Vector2(35.5f, 18.5f));
            Vector2 walkerStart = walker.transform.position;

            yield return ChaseUntilClose(brute, 10f, 1.1f);

            Assert.Greater(Mathf.Abs(walker.transform.position.x - walkerStart.x), 0.2f,
                $"walker was not pushed aside, it is at {walker.transform.position}");
        }

        [UnityTest]
        public IEnumerator Brute_BehindObstacle_ReachesPlayerWithoutEnteringWalls()
        {
            // Player above the 8x2 obstacle at (16..23, 20..21), brute straight below it.
            Teleport(_player.GetComponent<Rigidbody2D>(), new Vector2(20.5f, 23.5f));
            yield return null;
            Enemy brute = _brutes.Spawn(new Vector2(20.5f, 17.5f));

            yield return ChaseUntilClose(brute, 15f, 1.2f);
        }

        [UnityTest]
        public IEnumerator Brute_AroundWallEnd_ReachesPlayerWithoutEnteringWalls()
        {
            // The 2x8 obstacle at (8..9, 30..37) stands between them; the brute has to round its end.
            Teleport(_player.GetComponent<Rigidbody2D>(), new Vector2(11.5f, 33.5f));
            yield return null;
            Enemy brute = _brutes.Spawn(new Vector2(6.5f, 33.5f));

            yield return ChaseUntilClose(brute, 15f, 1.2f);
        }

        // Fails if the brute's body center ever is inside a wall cell or it does not get within reach in time.
        private IEnumerator ChaseUntilClose(Enemy brute, float seconds, float reach)
        {
            var body = brute.GetComponent<Collider2D>();
            float start = Vector2.Distance(brute.transform.position, _player.transform.position);
            float timeout = Time.time + seconds;
            while (Time.time < timeout && Vector2.Distance(brute.transform.position, _player.transform.position) > reach)
            {
                Vector2 center = body.bounds.center;
                Assert.IsTrue(_navigator.Field.IsWalkable(_navigator.CellOf(center)), $"brute inside a wall at {center}");
                yield return null;
            }

            Assert.LessOrEqual(Vector2.Distance(brute.transform.position, _player.transform.position), reach,
                $"brute stuck at {brute.transform.position}, started {start} away");
        }

        private static float Radius(Enemy enemy)
        {
            return enemy.GetComponent<CircleCollider2D>().radius;
        }

        private static void Teleport(Rigidbody2D body, Vector2 position)
        {
            body.position = position;
            body.transform.position = position;
        }
    }
}
