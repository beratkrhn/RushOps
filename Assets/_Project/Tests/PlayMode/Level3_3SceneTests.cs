using System.Collections;
using NUnit.Framework;
using RushOps.Bosses;
using RushOps.Levels;
using RushOps.Player;
using RushOps.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RushOps.Tests.PlayMode
{
    public class Level3_3SceneTests
    {
        private const string SceneName = "Level_3-3";
        private static readonly Vector2 ArenaSpot = new Vector2(14f, 45f);
        // Far from the boss, at the bottom of the arena.
        private static readonly Vector2 PuddleSpot = new Vector2(4f, 42f);

        private MegaBoomer _boss;

        [UnitySetUp]
        public IEnumerator LoadScene()
        {
            SaveGameIsolation.Reset();
            yield return SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);
            yield return null;
            foreach (MegaBoomer body in Object.FindObjectsByType<MegaBoomer>(FindObjectsSortMode.None))
            {
                if (!body.IsHalf)
                {
                    _boss = body;
                }
            }

            Assert.IsNotNull(_boss, "no Mega-Boomer in level 3-3");
        }

        [UnityTearDown]
        public IEnumerator Unfreeze()
        {
            Time.timeScale = 1f;
            yield return null;
        }

        [UnityTest]
        public IEnumerator Scene_HasTwoZonesBeforeTheArena_AndTheMegaBoomerWithTwoHiddenHalves()
        {
            yield return null;

            Assert.AreEqual(3, Object.FindObjectsByType<Zone>(FindObjectsSortMode.None).Length);
            Assert.AreEqual(2, Object.FindObjectsByType<Gate>(FindObjectsSortMode.None).Length);
            Assert.IsNull(Object.FindAnyObjectByType<LevelExit>(), "a boss level ends with the boss");
            Assert.IsInstanceOf<MegaBoomerDefinition>(_boss.Definition);
            Assert.AreEqual("Mega-Boomer", _boss.Definition.DisplayName);
            Assert.AreEqual(1000, _boss.Definition.Points, "decision C10");
            var values = (MegaBoomerDefinition)_boss.Definition;
            Assert.AreEqual(1f, values.PuddleWarmUp, "decision C9b");
            Assert.AreEqual(6f, values.PuddleLifetime, "decision C9b");
            Assert.AreEqual(1, values.PuddleDamage, "decision C9b");
            Assert.AreEqual(0.5f, values.SplitHealthShare, "decision C9a");
            Assert.AreEqual(2, _boss.Halves.Length);
            foreach (MegaBoomer half in _boss.Halves)
            {
                Assert.IsTrue(half.IsHalf);
                Assert.IsFalse(half.gameObject.activeSelf, "halves wait for the split");
            }

            Assert.AreEqual("Krater", Object.FindAnyObjectByType<LevelResults>().Level.DisplayName);
        }

        [UnityTest]
        public IEnumerator MegaBoomer_LeavesPuddles_ThatGlowFirst_ThenHurt_ThenDryUp()
        {
            yield return EnterArena();
            yield return WaitFor(() => _boss.PuddleCount > 0, 4f, "no puddle after the interval");
            Puddle puddle = FindActivePuddle();
            Assert.AreEqual(PuddleState.Warm, puddle.State);
            Assert.AreEqual(0, puddle.ContactDamage, "decision C9b: harmless while it glows");

            yield return new WaitForSeconds(1.1f);
            Assert.AreEqual(PuddleState.Active, puddle.State);
            Assert.AreEqual(1, puddle.ContactDamage);

            yield return new WaitForSeconds(5f);
            Assert.IsFalse(puddle.gameObject.activeSelf, "decision C9b: gone after 6 s");
        }

        [UnityTest]
        public IEnumerator HotPuddle_CostsTheTouchingPlayerAHeart()
        {
            yield return EnterArena();
            var pool = Object.FindAnyObjectByType<PuddlePool>();
            Puddle puddle = pool.Spawn(PuddleSpot, (MegaBoomerDefinition)_boss.Definition);
            var health = GameObject.Find("Player").GetComponent<PlayerHealth>();
            int hearts = health.Current;

            Teleport(PuddleSpot);
            yield return new WaitForSeconds(0.5f);
            Assert.AreEqual(hearts, health.Current, "no damage while it glows");

            yield return WaitFor(() => health.Current < hearts, 1.5f, "the hot puddle did not hurt");
            Assert.AreEqual(hearts - 1, health.Current);
            Assert.AreEqual(PuddleState.Active, puddle.State);
        }

        [UnityTest]
        public IEnumerator AtHalfHealth_ItSplitsIntoTwoHalves_ThatShareTheBar_AndBothMustFall()
        {
            yield return EnterArena();
            var fight = Object.FindAnyObjectByType<BossFight>();
            int max = _boss.MaxHealth;
            _boss.TakeHit(max / 2 + 1, Vector2.up);
            int rest = _boss.CurrentHealth;
            yield return WaitFor(() => _boss.IsRemoved, 2f, "it did not split at half health");

            Assert.IsFalse(_boss.gameObject.activeSelf);
            MegaBoomer first = _boss.Halves[0];
            MegaBoomer second = _boss.Halves[1];
            Assert.IsTrue(first.gameObject.activeSelf && second.gameObject.activeSelf);
            Assert.IsTrue(first.IsActive && second.IsActive, "the halves chase the player");
            Assert.AreEqual(rest, first.CurrentHealth + second.CurrentHealth, "decision C9a: half the rest each");
            Assert.LessOrEqual(Mathf.Abs(first.CurrentHealth - second.CurrentHealth), 1);
            Assert.AreEqual(rest, fight.CurrentHealth, "one shared health bar");
            Assert.Less(first.transform.localScale.x, 1f, "the halves are smaller");

            first.TakeHit(first.MaxHealth, Vector2.up);
            Assert.IsFalse(fight.IsWon, "both halves must fall");
            second.TakeHit(second.MaxHealth, Vector2.up);
            Assert.IsTrue(fight.IsWon);

            var results = Object.FindAnyObjectByType<LevelResults>();
            yield return new WaitForSecondsRealtime(_boss.Definition.VictoryDelay + 0.3f);
            Assert.IsTrue(results.ShowsDone);
            Assert.AreEqual("Alle Inseln geschafft!", results.MessageShown, "decision C12");
        }

        private IEnumerator EnterArena()
        {
            Teleport(ArenaSpot);
            yield return null;
            yield return null;
            Assert.IsTrue(Object.FindAnyObjectByType<BossFight>().IsRunning, "the fight did not start");
        }

        private static void Teleport(Vector2 position)
        {
            var body = GameObject.Find("Player").GetComponent<Rigidbody2D>();
            body.position = position;
            body.transform.position = position;
        }

        private static Puddle FindActivePuddle()
        {
            foreach (Puddle puddle in Object.FindObjectsByType<Puddle>(FindObjectsSortMode.None))
            {
                if (puddle.gameObject.activeInHierarchy)
                {
                    return puddle;
                }
            }

            Assert.Fail("no puddle on the floor");
            return null;
        }

        private static IEnumerator WaitFor(System.Func<bool> condition, float seconds, string message)
        {
            float waited = 0f;
            while (!condition() && waited < seconds)
            {
                waited += Time.deltaTime;
                yield return null;
            }

            Assert.IsTrue(condition(), message);
        }
    }
}
