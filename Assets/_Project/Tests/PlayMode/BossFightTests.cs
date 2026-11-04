using System.Collections;
using NUnit.Framework;
using RushOps.Bosses;
using RushOps.Enemies;
using RushOps.Perks;
using RushOps.Player;
using RushOps.Scoring;
using RushOps.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RushOps.Tests.PlayMode
{
    public class BossFightTests
    {
        private const string SceneName = "Boss_Test";
        private static readonly Vector2 ArenaPoint = new Vector2(12f, 20f);

        private BossFight _fight;
        private Boss _boss;

        [UnitySetUp]
        public IEnumerator LoadScene()
        {
            SaveGameIsolation.Reset();
            yield return SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);
            yield return null;
            _fight = Object.FindAnyObjectByType<BossFight>();
            Assert.IsNotNull(_fight, "no boss fight in the scene");
            _boss = _fight.Bodies[0];
        }

        [UnityTearDown]
        public IEnumerator Unfreeze()
        {
            Time.timeScale = 1f;
            yield return null;
        }

        [UnityTest]
        public IEnumerator BeforeTheArena_BossWaits_BarHidden_ShotsDoNothing()
        {
            yield return null;

            Assert.IsFalse(_fight.IsRunning);
            Assert.IsFalse(_boss.IsActive);
            Assert.IsFalse(Bar.IsVisible);
            Assert.IsFalse(Door.IsClosed);
            Assert.IsFalse(_boss.TakeHit(5, Vector2.up));
            Assert.AreEqual(_boss.MaxHealth, _boss.CurrentHealth);
        }

        [UnityTest]
        public IEnumerator EnteringTheArena_ClosesDoor_ShowsBar_StartsBoss()
        {
            yield return EnterArena();

            Assert.IsTrue(_fight.IsRunning);
            Assert.IsTrue(Door.IsClosed, "door closes behind the player (C6)");
            Assert.IsTrue(Bar.IsVisible);
            Assert.AreEqual("Testboss", Bar.Name);
            Assert.AreEqual(1f, Bar.Fill);
            Assert.IsTrue(_boss.IsActive);

            float before = Vector2.Distance(_boss.transform.position, Player.position);
            yield return new WaitForSeconds(0.5f);
            Assert.Less(Vector2.Distance(_boss.transform.position, Player.position), before, "boss walks at the player (L5)");
            Assert.AreEqual(0, Object.FindAnyObjectByType<WaveDirector>().AliveCount, "the boss is no pooled enemy");
        }

        [UnityTest]
        public IEnumerator InstaKillHit_DealsNormalDamage_NukeSparesBoss()
        {
            yield return EnterArena();

            Assert.IsTrue(_boss.TakeHit(2, Vector2.up, lethal: true));
            Assert.AreEqual(_boss.MaxHealth - 2, _boss.CurrentHealth, "F-BOS-02: immune to insta-kill");
            Object.FindAnyObjectByType<NukeSystem>().Detonate();
            yield return null;
            Assert.IsFalse(_boss.IsDead, "F-BOS-02: immune to the nuke");
            Assert.AreEqual(_boss.MaxHealth - 2, _boss.CurrentHealth);
        }

        [UnityTest]
        public IEnumerator HealthThresholds_DropPerks_NeverNukeOrInstaKill()
        {
            yield return EnterArena();
            var pickups = Object.FindAnyObjectByType<PerkPickupPool>();
            // Nuke and insta-kill are not in the boss's pool at all, whatever the roll.
            _fight.SetRandomSource(new ConstantRandom(0.99f));
            int max = _boss.MaxHealth;

            _boss.TakeHit(max / 4, Vector2.up);
            Assert.AreEqual(0, pickups.CountActive, "75 % health: no perk yet");
            _boss.TakeHit(max / 10 + 1, Vector2.up);
            Assert.AreEqual(1, pickups.CountActive, "below 66 %: one perk (C9)");
            _boss.TakeHit(max / 3 + 1, Vector2.up);
            Assert.AreEqual(2, pickups.CountActive, "below 33 %: the second");
            foreach (PerkPickup pickup in pickups.Active)
            {
                Assert.IsTrue(BossMath.CanDrop(pickup.Perk), pickup.Perk.name);
            }

            Assert.AreEqual(BossMath.Fill(_boss.CurrentHealth, max), Bar.Fill, 1e-4f);
        }

        [UnityTest]
        public IEnumerator BeatingTheBoss_AddsItsPoints_AndFinishesTheLevel()
        {
            yield return EnterArena();
            ScoreKeeper score = Object.FindAnyObjectByType<LevelScore>().Keeper;
            int before = score.Score;

            _boss.TakeHit(_boss.MaxHealth, Vector2.up);

            Assert.IsTrue(_fight.IsWon);
            Assert.AreEqual(before + _boss.Definition.Points, score.Score, "C10: 1000 points, no multiplier");
            Assert.AreEqual(1000, _boss.Definition.Points);
            var results = Object.FindAnyObjectByType<LevelResults>();
            Assert.IsFalse(results.ShowsDone, "the victory waits for the death animation");
            // Realtime: the done screen freezes the game (timeScale 0).
            yield return new WaitForSecondsRealtime(_boss.Definition.VictoryDelay + 0.3f);
            Assert.IsTrue(results.ShowsDone, "F-LVL-05: beating the boss finishes the level");
        }

        [UnityTest]
        public IEnumerator TouchingTheBoss_CostsAHeart()
        {
            yield return EnterArena();
            var health = Player.GetComponent<PlayerHealth>();
            int hearts = health.Current;
            Teleport(_boss.transform.position + new Vector3(0f, -1.5f, 0f));

            float waited = 0f;
            while (health.Current == hearts && waited < 2f)
            {
                waited += Time.deltaTime;
                yield return null;
            }

            Assert.AreEqual(hearts - _boss.Definition.ContactDamage, health.Current);
        }

        private static Transform Player => GameObject.Find("Player").transform;

        private static BossHealthBar Bar => Object.FindAnyObjectByType<BossHealthBar>();

        private static ArenaDoor Door => Object.FindAnyObjectByType<ArenaDoor>();

        private IEnumerator EnterArena()
        {
            Teleport(ArenaPoint);
            yield return null;
            yield return null;
            Assert.IsTrue(_fight.IsRunning, "fight did not start in the arena");
        }

        private static void Teleport(Vector2 position)
        {
            var body = GameObject.Find("Player").GetComponent<Rigidbody2D>();
            body.position = position;
            body.transform.position = position;
        }

        private sealed class ConstantRandom : IRandomSource
        {
            private readonly float _value;

            public ConstantRandom(float value)
            {
                _value = value;
            }

            public float Value()
            {
                return _value;
            }
        }
    }
}
