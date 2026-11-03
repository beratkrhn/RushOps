using System.Collections;
using NUnit.Framework;
using RushOps.Enemies;
using RushOps.Perks;
using RushOps.Player;
using RushOps.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RushOps.Tests.PlayMode
{
    public class PerkEffectTests
    {
        private const string SceneName = "Arena_Test";

        private PlayerPerks _perks;
        private PlayerMovement _movement;
        private PlayerHealth _health;
        private PerkPickupPool _pickups;
        private ActivePerksView _view;

        [UnitySetUp]
        public IEnumerator LoadScene()
        {
            yield return SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);
            yield return null;

            _perks = Object.FindFirstObjectByType<PlayerPerks>();
            _pickups = Object.FindFirstObjectByType<PerkPickupPool>();
            _view = Object.FindFirstObjectByType<ActivePerksView>();
            Assert.IsNotNull(_perks, "Arena_Test has no PlayerPerks");
            Assert.IsNotNull(_view, "Arena_Test has no ActivePerksView");
            _movement = _perks.GetComponent<PlayerMovement>();
            _health = _perks.GetComponent<PlayerHealth>();
            Object.FindFirstObjectByType<WaveDirector>().enabled = false;
            Object.FindFirstObjectByType<LevelRestarter>().enabled = false;
            _movement.GetComponent<PlayerMoveInput>().enabled = false;
            _movement.GetComponent<PlayerAimInput>().enabled = false;
        }

        [Test]
        public void PerkDefinitions_HaveEffectStartValuesOfLastenheft()
        {
            PerkDefinition speed = Perk("speed_boost");
            Assert.AreEqual(PerkEffect.SpeedBoost, speed.Effect);
            Assert.AreEqual(1.5f, speed.Magnitude, 1e-5f);
            Assert.AreEqual(15f, speed.Duration, 1e-5f);

            PerkDefinition medkit = Perk("medkit");
            Assert.AreEqual(PerkEffect.Medkit, medkit.Effect);
            Assert.AreEqual(PerkKind.Instant, medkit.Kind);
            Assert.AreEqual(2f, medkit.Magnitude, 1e-5f);
        }

        [UnityTest]
        public IEnumerator CollectedPickup_AppliesPerk()
        {
            PerkPickup pickup = _pickups.Spawn(new Vector2(34.5f, 18.5f), Perk("speed_boost"));

            pickup.Collect();
            yield return null;

            Assert.IsTrue(_perks.Active.IsActive("speed_boost"));
        }

        [UnityTest]
        public IEnumerator SpeedBoost_RaisesWalkingSpeed_UntilExpiry()
        {
            float normal = _movement.Config.MoveSpeed;

            _perks.Apply(Perk("speed_boost"));
            _movement.MoveInput = Vector2.up;
            yield return new WaitForSeconds(0.5f);

            Assert.AreEqual(normal * 1.5f, _movement.Velocity.magnitude, 0.05f);

            _perks.Active.Tick(15f);
            yield return new WaitForSeconds(0.5f);

            Assert.AreEqual(1f, _movement.SpeedMultiplier, 1e-5f);
            Assert.AreEqual(normal, _movement.Velocity.magnitude, 0.05f);
        }

        [Test]
        public void Medkit_RestoresTwoHearts()
        {
            _health.TakeDamage(3);
            int hurt = _health.Current;

            _perks.Apply(Perk("medkit"));

            Assert.AreEqual(hurt + 2, _health.Current);
            Assert.AreEqual(0, _perks.Active.Count, "medkit acts at once and shows no timer");
        }

        [Test]
        public void Medkit_DoesNotHealAboveMaximum()
        {
            _health.TakeDamage(1);

            _perks.Apply(Perk("medkit"));

            Assert.AreEqual(_health.Max, _health.Current);
        }

        [UnityTest]
        public IEnumerator Hud_ShowsRunningPerksWithShrinkingTimer()
        {
            Assert.AreEqual(0, _view.ShownCount);

            _perks.Apply(Perk("speed_boost"));
            _perks.Apply(Perk("double_damage"));
            yield return null;

            Assert.AreEqual(2, _view.ShownCount);
            Assert.AreSame(Perk("speed_boost").Icon, _view.Icon(0).sprite);
            Assert.AreSame(Perk("double_damage").Icon, _view.Icon(1).sprite);
            Assert.IsTrue(_view.Icon(0).gameObject.activeInHierarchy);

            _perks.Active.Tick(7.5f);
            yield return null;

            Assert.AreEqual(0.5f, _view.Timer(0).fillAmount, 0.02f);
            Assert.AreEqual(0.25f, _view.Timer(1).fillAmount, 0.02f);

            _perks.Active.Tick(2.5f);
            yield return null;

            Assert.AreEqual(1, _view.ShownCount, "expired perk leaves the HUD");
            Assert.AreSame(Perk("speed_boost").Icon, _view.Icon(0).sprite);
            Assert.IsFalse(_view.Icon(1).gameObject.activeInHierarchy);
        }

        [UnityTest]
        public IEnumerator Hud_RefreshedPerk_FillsTimerAgain()
        {
            _perks.Apply(Perk("speed_boost"));
            _perks.Active.Tick(10f);
            yield return null;
            Assert.Less(_view.Timer(0).fillAmount, 0.4f);

            _perks.Apply(Perk("speed_boost"));
            yield return null;

            Assert.AreEqual(1, _view.ShownCount);
            Assert.Greater(_view.Timer(0).fillAmount, 0.99f);
        }

        private PerkDefinition Perk(string id)
        {
            foreach (PerkDefinition perk in _pickups.Config.Perks)
            {
                if (perk.Id == id)
                {
                    return perk;
                }
            }

            Assert.Fail($"LootConfig has no perk {id}");
            return null;
        }
    }
}
