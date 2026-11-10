using System.Collections;
using NUnit.Framework;
using RushOps.Enemies;
using RushOps.Perks;
using RushOps.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RushOps.Tests.PlayMode
{
    /// <summary>Flamethrower and rocket launcher in the HUD and the test arena (issue #256).</summary>
    public class SuperWeaponHudTests
    {
        private const string PerksDir = "Assets/_Project/Data/Perks/";

        private PlayerPerks _perks;
        private ActivePerksView _view;

        [UnitySetUp]
        public IEnumerator LoadScene()
        {
            SaveGameIsolation.Reset();
            yield return SceneManager.LoadSceneAsync("Arena_Test", LoadSceneMode.Single);
            yield return null;
            Object.FindFirstObjectByType<WaveDirector>().enabled = false;
            _perks = Object.FindFirstObjectByType<PlayerPerks>();
            _view = Object.FindFirstObjectByType<ActivePerksView>();
        }

        [TearDown]
        public void Cleanup()
        {
            SaveGameIsolation.Reset();
        }

        [UnityTest]
        public IEnumerator Hud_Flamethrower_ShowsItsTimerAndName()
        {
            PerkDefinition flamethrower = Load("Flamethrower");
            var notice = Object.FindFirstObjectByType<PerkNameNotice>();

            _perks.Apply(flamethrower);
            Assert.AreEqual("Flammenwerfer", notice.Shown);
            yield return new WaitForSeconds(3f);

            Assert.AreEqual(1, _view.ShownCount);
            Assert.AreSame(flamethrower.Icon, _view.Icon(0).sprite);
            // 3 of 12 seconds are gone.
            Assert.AreEqual(0.75f, _view.Timer(0).fillAmount, 0.05f);
        }

        [UnityTest]
        public IEnumerator Hud_RocketLauncherAfterFlamethrower_ShowsOnlyTheRocketLauncher()
        {
            PerkDefinition rocketLauncher = Load("RocketLauncher");
            var notice = Object.FindFirstObjectByType<PerkNameNotice>();
            var superWeapon = Object.FindFirstObjectByType<PlayerSuperWeapon>();

            _perks.Apply(Load("SpeedBoost"));
            _perks.Apply(Load("Flamethrower"));
            yield return new WaitForSeconds(1f);
            _perks.Apply(rocketLauncher);
            yield return null;
            yield return null;

            Assert.AreEqual("Raketenwerfer", notice.Shown);
            Assert.AreEqual(SuperWeaponKind.RocketLauncher, superWeapon.Kind);
            Assert.AreEqual(2, _view.ShownCount);
            Assert.AreSame(rocketLauncher.Icon, _view.Icon(1).sprite);
            Assert.AreEqual(1f, _view.Timer(1).fillAmount, 0.02f);
            Assert.IsFalse(_perks.Active.IsActive("flamethrower"));
        }

        [UnityTest]
        public IEnumerator Hud_AllTimedPerksAtOnce_FitInsideTheSafeArea()
        {
            foreach (PerkDefinition perk in Object.FindFirstObjectByType<PerkPickupPool>().Config.Perks)
            {
                if (perk.Kind == PerkKind.Timed)
                {
                    _perks.Apply(perk);
                }
            }

            yield return null;
            yield return null;

            // Every timed perk at once, but only one super weapon in the hands.
            Assert.AreEqual(8, _view.ShownCount);
            var safeArea = Object.FindFirstObjectByType<SafeAreaFitter>().GetComponent<RectTransform>();
            Rect area = ScreenRect(safeArea);
            for (int i = 0; i < _view.ShownCount; i++)
            {
                Rect slot = ScreenRect(_view.Timer(i).rectTransform);
                Assert.IsTrue(area.Contains(slot.min) && area.Contains(slot.max), $"slot {i} {slot} leaves {area}");
            }
        }

        [Test]
        public void DebugPerkSpawn_ReachesBothSuperWeapons()
        {
            var spawn = Object.FindFirstObjectByType<DebugPerkSpawn>();
            int count = Object.FindFirstObjectByType<PerkPickupPool>().Config.Perks.Length;

            bool flamethrower = false;
            bool rocketLauncher = false;
            for (int i = 0; i < count; i++)
            {
                string id = spawn.Next.Id;
                flamethrower |= id == "flamethrower";
                rocketLauncher |= id == "rocket_launcher";
                spawn.Spawn();
            }

            Assert.IsTrue(flamethrower && rocketLauncher);
        }

        private static Rect ScreenRect(RectTransform rect)
        {
            var corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            return Rect.MinMaxRect(corners[0].x, corners[0].y, corners[2].x, corners[2].y);
        }

        private static PerkDefinition Load(string asset)
        {
            return AssetDatabase.LoadAssetAtPath<PerkDefinition>(PerksDir + asset + ".asset");
        }
    }
}
