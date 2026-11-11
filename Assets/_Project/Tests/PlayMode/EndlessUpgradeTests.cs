using System.Collections;
using NUnit.Framework;
using RushOps.Endless;
using RushOps.Loot;
using RushOps.Perks;
using RushOps.Player;
using RushOps.Rush;
using RushOps.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace RushOps.Tests.PlayMode
{
    /// <summary>
    /// The numeric run upgrades in the endless arena (issue #277, F-END-12): each one changes its value measurably, the
    /// perk and rush factors multiply with it, and a new run starts without upgrades.
    /// </summary>
    public class EndlessUpgradeTests
    {
        private const float Tolerance = 1e-4f;

        private EndlessRunController _controller;
        private DebugUpgradeKey _debug;
        private PlayerHealth _health;
        private PlayerMovement _movement;
        private PlayerWeapon _weapon;
        private PlayerDash _dash;
        private PlayerPerks _perks;
        private PlayerRush _rush;
        private PerkDropper _dropper;
        private GoldPickupPool _gold;
        private CompanionDrone _drone;

        [UnitySetUp]
        public IEnumerator LoadScene()
        {
            SaveGameIsolation.Reset();
            EndlessStart.Round = 1;
            yield return SceneManager.LoadSceneAsync("Endless_Arena", LoadSceneMode.Single);
            yield return null;
            _controller = Object.FindAnyObjectByType<EndlessRunController>();
            _debug = _controller.GetComponent<DebugUpgradeKey>();
            _health = Object.FindAnyObjectByType<PlayerHealth>();
            _movement = _health.GetComponent<PlayerMovement>();
            _weapon = _health.GetComponent<PlayerWeapon>();
            _dash = _health.GetComponent<PlayerDash>();
            _perks = _health.GetComponent<PlayerPerks>();
            _rush = Object.FindAnyObjectByType<PlayerRush>();
            _dropper = Object.FindAnyObjectByType<PerkDropper>();
            _gold = Object.FindAnyObjectByType<GoldPickupPool>();
            _drone = Object.FindAnyObjectByType<CompanionDrone>();
            _health.GetComponent<PlayerMoveInput>().enabled = false;
            _health.GetComponent<PlayerAimInput>().enabled = false;
            Assert.IsNotNull(_controller.GetComponent<EndlessUpgradeEffects>(), "the arena hands the upgrades on");
            Assert.IsNotNull(_debug, "the arena has the debug control");
        }

        [TearDown]
        public void TearDown()
        {
            Time.timeScale = 1f;
            EndlessStart.Round = 1;
            SaveGameIsolation.Reset();
        }

        [UnityTest]
        public IEnumerator ExtraHeart_RaisesTheMaximumAndHeals()
        {
            Assert.AreEqual(5, _health.Max);
            _health.TakeDamage(2);
            yield return null;

            Assert.IsTrue(_debug.Grant("extra_heart"));

            Assert.AreEqual(6, _health.Max);
            Assert.AreEqual(4, _health.Current);
            Assert.AreEqual(6, _controller.Run.MaxHearts);
            Assert.AreEqual(4, _controller.Run.Hearts);
        }

        [UnityTest]
        public IEnumerator ThreeExtraHearts_HudShowsEightWithoutOverlap()
        {
            for (int i = 0; i < 3; i++)
            {
                Assert.IsTrue(_debug.Grant("extra_heart"));
            }

            yield return null;
            Assert.AreEqual(8, _health.Max);
            Assert.AreEqual(8, Object.FindAnyObjectByType<HeartsView>().ShownHearts);

            Transform row = Object.FindAnyObjectByType<HeartsView>().transform;
            Rect score = TextRect(Object.FindAnyObjectByType<ScoreView>().GetComponent<Text>());
            var rushButton = (RectTransform)Object.FindAnyObjectByType<RushButtonView>().transform;
            var perks = (RectTransform)Object.FindAnyObjectByType<ActivePerksView>().transform;
            int shown = 0;
            for (int i = 0; i < row.childCount; i++)
            {
                var heart = row.GetChild(i).GetComponent<Image>();
                if (!heart.enabled)
                {
                    continue;
                }

                shown++;
                Rect rect = WorldRect(heart.rectTransform);
                Assert.IsFalse(rect.Overlaps(score), $"{heart.name} overlaps the score");
                Assert.IsFalse(rect.Overlaps(WorldRect(rushButton)), $"{heart.name} overlaps the rush button");
                Assert.Greater(rect.yMin, WorldRect(perks).yMax - Tolerance, $"{heart.name} reaches into the perk row");
            }

            Assert.AreEqual(8, shown);
        }

        [UnityTest]
        public IEnumerator LongerPerks_TimedPerksLastLonger()
        {
            Assert.IsTrue(_debug.Grant("longer_perks"));
            yield return null;
            PerkDefinition speed = Perk("speed_boost");

            _perks.Apply(speed);

            Assert.AreEqual(speed.Duration * 1.3f, Active("speed_boost").Duration, Tolerance);
            Assert.AreEqual(1.3f, _perks.DurationFactor, Tolerance);
            _drone.Activate();
            Assert.Greater(_drone.Remaining, 15f * 1.29f, "the drone lasts longer too");
            Assert.AreEqual(1.3f, Object.FindAnyObjectByType<PlayerSuperWeapon>().DurationFactor, Tolerance);
            Assert.AreEqual(1.3f, Object.FindAnyObjectByType<Scarecrow>().DurationFactor, Tolerance);
        }

        [UnityTest]
        public IEnumerator GoldMagnet_ReachesOneTileFurther()
        {
            float range = _gold.Config.MagnetRange;
            Assert.IsTrue(_debug.Grant("gold_magnet"));
            yield return null;

            GoldPickup coin = _gold.Spawn((Vector2)_health.transform.position + new Vector2(5f, 0f));

            Assert.AreEqual(range + 1f, coin.MagnetRange, Tolerance);
            Assert.IsTrue(_debug.Grant("gold_magnet"));
            Assert.AreEqual(range + 2f, coin.MagnetRange, Tolerance, "coins already on the floor follow");
        }

        [UnityTest]
        public IEnumerator QuickFeet_MultipliesWithSpeedBoost()
        {
            float speed = _movement.MoveSpeed;
            Assert.IsTrue(_debug.Grant("quick_feet"));
            yield return null;
            Assert.AreEqual(speed * 1.1f, _movement.MoveSpeed, Tolerance);

            PerkDefinition boost = Perk("speed_boost");
            _perks.Apply(boost);

            Assert.AreEqual(speed * 1.1f * boost.Magnitude, _movement.MoveSpeed, Tolerance);
        }

        [UnityTest]
        public IEnumerator FireRate_MultipliesWithRapidFire()
        {
            float rate = _weapon.FireRate;
            Assert.IsTrue(_debug.Grant("fire_rate"));
            yield return null;
            Assert.AreEqual(rate * 1.15f, _weapon.FireRate, Tolerance);

            PerkDefinition rapid = Perk("rapid_fire");
            _perks.Apply(rapid);

            Assert.AreEqual(rate * 1.15f * rapid.Magnitude, _weapon.FireRate, Tolerance);
        }

        [UnityTest]
        public IEnumerator Damage_MultipliesWithDoubleDamageAndRush()
        {
            Assert.IsTrue(_debug.Grant("damage"));
            yield return null;
            Assert.AreEqual(1.15f, _weapon.DamageMultiplier, Tolerance);

            _perks.Apply(Perk("double_damage"));
            Assert.AreEqual(2f * 1.15f, _weapon.DamageMultiplier, Tolerance);

            _rush.FillUp();
            Assert.IsTrue(_rush.TryTrigger());
            yield return null;
            Assert.AreEqual(2f * 1.15f * _rush.Config.DamageFactor, _weapon.DamageMultiplier, Tolerance);
            Assert.AreEqual(1.15f * _rush.Config.DamageFactor, _weapon.ExtraDamageFactor, Tolerance,
                "drone and scarecrow get rush and upgrade");
        }

        [UnityTest]
        public IEnumerator PerkLuck_AddsThreePercentagePoints()
        {
            float chance = _dropper.DropChance;
            Assert.IsTrue(_debug.Grant("perk_luck"));
            yield return null;

            Assert.AreEqual(chance + 0.03f, _dropper.DropChance, Tolerance);
        }

        [UnityTest]
        public IEnumerator FastRush_FullAt20_AndAMeterAbove20IsFullAtOnce()
        {
            Assert.AreEqual(25, _rush.Meter.FillTarget);
            _rush.Meter.RegisterKill(22);
            Assert.IsFalse(_rush.Meter.IsFull);

            Assert.IsTrue(_debug.Grant("fast_rush"));
            yield return null;

            Assert.AreEqual(20, _rush.Meter.FillTarget);
            Assert.IsTrue(_rush.Meter.IsFull);
        }

        [UnityTest]
        public IEnumerator ShortDash_ShorterCooldown_SameIFrames()
        {
            PlayerConfig config = _movement.Config;
            Assert.AreEqual(config.DashCooldown, _dash.NextCooldown, Tolerance);
            Assert.IsTrue(_debug.Grant("short_dash"));
            Assert.IsTrue(_debug.Grant("short_dash"));
            yield return null;
            Assert.AreEqual(config.DashCooldown * 0.5f, _dash.NextCooldown, Tolerance);

            _movement.MoveInput = Vector2.right;
            Assert.IsTrue(_dash.TryDash());
            Assert.IsTrue(_dash.IsInvulnerable);
            Assert.AreEqual(config.DashCooldown * 0.5f, _dash.CooldownRemaining, Tolerance);
        }

        [UnityTest]
        public IEnumerator NewRun_StartsWithoutUpgrades()
        {
            float speed = _movement.MoveSpeed;
            float rate = _weapon.FireRate;
            float chance = _dropper.DropChance;
            float cooldown = _dash.NextCooldown;
            foreach (string id in new[] { "extra_heart", "longer_perks", "gold_magnet", "quick_feet", "fire_rate", "damage",
                "perk_luck", "fast_rush", "short_dash" })
            {
                Assert.IsTrue(_debug.Grant(id), id);
            }

            yield return null;
            Assert.AreEqual(9, _controller.Upgrades.TotalLevels);

            _controller.BeginRun(1);
            yield return null;

            Assert.AreEqual(0, _controller.Upgrades.TotalLevels);
            Assert.AreEqual(5, _health.Max);
            Assert.AreEqual(5, _controller.Run.MaxHearts);
            Assert.AreEqual(speed, _movement.MoveSpeed, Tolerance);
            Assert.AreEqual(rate, _weapon.FireRate, Tolerance);
            Assert.AreEqual(1f, _weapon.DamageMultiplier, Tolerance);
            Assert.AreEqual(chance, _dropper.DropChance, Tolerance);
            Assert.AreEqual(cooldown, _dash.NextCooldown, Tolerance);
            Assert.AreEqual(25, _rush.Meter.FillTarget);
            Assert.AreEqual(1f, _perks.DurationFactor, Tolerance);
            Assert.AreEqual(0f, _gold.MagnetBonus, Tolerance);
        }

        private ActivePerk Active(string id)
        {
            ActivePerks active = _perks.Active;
            for (int i = 0; i < active.Count; i++)
            {
                if (active[i].Id == id)
                {
                    return active[i];
                }
            }

            Assert.Fail($"{id} is not running");
            return default;
        }

        private static PerkDefinition Perk(string id)
        {
            foreach (PerkDefinition perk in Resources.FindObjectsOfTypeAll<PerkDefinition>())
            {
                if (perk.Id == id)
                {
                    return perk;
                }
            }

            Assert.Fail($"perk {id} not loaded");
            return null;
        }

        // The drawn text, not its box: centred in the box with the preferred width of a long score.
        private static Rect TextRect(Text text)
        {
            string shown = text.text;
            text.text = RushOps.UI.ScoreText.Score(9999999);
            float width = text.preferredWidth * text.rectTransform.lossyScale.x;
            text.text = shown;
            Rect box = WorldRect(text.rectTransform);
            return Rect.MinMaxRect(box.center.x - width / 2f, box.yMin, box.center.x + width / 2f, box.yMax);
        }

        private static Rect WorldRect(RectTransform rect)
        {
            var corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            return Rect.MinMaxRect(corners[0].x, corners[0].y, corners[2].x, corners[2].y);
        }
    }
}
