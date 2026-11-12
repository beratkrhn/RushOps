using System.Collections;
using NUnit.Framework;
using RushOps.Endless;
using RushOps.Enemies;
using RushOps.Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RushOps.Tests.PlayMode
{
    /// <summary>The spawn points of the endless arena are breaches; enemies crawl out of them (issue #313, F-END-15).</summary>
    public class SpawnBreachTests
    {
        private const string SceneName = "Endless_Arena";
        private const float Timeout = 25f;

        private EndlessRunController _controller;
        private WaveDirector _director;

        [UnitySetUp]
        public IEnumerator LoadScene()
        {
            SaveGameIsolation.Reset();
            EndlessStart.Round = 1;
            yield return SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);
            yield return null;
            _controller = Object.FindAnyObjectByType<EndlessRunController>();
            _director = Object.FindAnyObjectByType<WaveDirector>();
            var health = Object.FindAnyObjectByType<PlayerHealth>();
            health.GetComponent<PlayerMoveInput>().enabled = false;
            health.GetComponent<PlayerAimInput>().enabled = false;
        }

        [TearDown]
        public void Reset()
        {
            Time.timeScale = 1f;
            EndlessStart.Round = 1;
            SaveGameIsolation.Reset();
        }

        [Test]
        public void EverySpawnPointOfTheArena_IsABreach()
        {
            SpawnPoint[] points = Object.FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);

            Assert.AreEqual(12, points.Length, "three per gate");
            foreach (SpawnPoint point in points)
            {
                Assert.IsTrue(point.IsBreach, point.name);
                Assert.IsFalse(point.IsOpen, $"{point.name} starts closed");
            }
        }

        [UnityTest]
        public IEnumerator AnEnemyOfABreach_CrawlsOutFirst_ThenChases()
        {
            Enemy crawler = null;
            SpawnPoint breach = null;
            bool emergingWhenReported = false;
            _director.Spawned += (enemy, point) =>
            {
                if (crawler == null)
                {
                    crawler = enemy;
                    breach = point;
                    emergingWhenReported = enemy.IsEmerging;
                }
            };
            _controller.BeginRun(1);
            float end = Time.realtimeSinceStartup + Timeout;
            while (crawler == null)
            {
                Assert.Less(Time.realtimeSinceStartup, end, "timeout waiting for the first enemy");
                yield return null;
            }

            Assert.IsTrue(emergingWhenReported, "the enemy already crawls when the spawn is reported");
            Assert.IsTrue(breach.IsBreach);
            Assert.IsTrue(breach.IsOpen, "the breach is open while the enemy crawls out");
            Assert.IsFalse(crawler.CanBeHit, "no hits while crawling out");
            Assert.IsFalse(crawler.CanBeHitByExplosion);
            Assert.AreEqual(0, crawler.ContactDamage, "no contact damage while crawling out");
            Assert.IsFalse(crawler.TakeHit(1, Vector2.up), "a shot does not count");
            Vector2 start = crawler.transform.position;
            var sprites = crawler.GetComponentInChildren<SpriteRenderer>();
            Sprite[] frames = crawler.Definition.EmergeFrames;
            Assert.AreEqual(6, frames.Length, "the walker has its own crawl-out frames (issue #314)");
            CollectionAssert.Contains(frames, sprites.sprite, "shows a crawl-out frame");

            yield return new WaitForSeconds(_director.Config.EmergeDuration + 0.3f);

            Assert.IsFalse(crawler.IsEmerging);
            Assert.IsTrue(crawler.CanBeHit);
            Assert.Greater(crawler.ContactDamage, 0);
            Assert.AreEqual(1f, crawler.transform.localScale.y, 1e-4f);
            CollectionAssert.DoesNotContain(frames, sprites.sprite, "the walking animation took over again");
            Assert.Greater(((Vector2)crawler.transform.position - start).sqrMagnitude, 0f, "it chases now");
        }
    }
}
