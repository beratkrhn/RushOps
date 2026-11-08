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
    /// <summary>Companion and scarecrow in the HUD and the test arena (issue #247).</summary>
    public class PerkHudTests
    {
        private const string PerksDir = "Assets/_Project/Data/Perks/";

        private PlayerPerks _perks;

        [UnitySetUp]
        public IEnumerator LoadScene()
        {
            SaveGameIsolation.Reset();
            yield return SceneManager.LoadSceneAsync("Arena_Test", LoadSceneMode.Single);
            yield return null;
            Object.FindFirstObjectByType<WaveDirector>().enabled = false;
            _perks = Object.FindFirstObjectByType<PlayerPerks>();
        }

        [TearDown]
        public void Cleanup()
        {
            SaveGameIsolation.Reset();
        }

        [UnityTest]
        public IEnumerator Hud_WithCompanionAndScarecrow_ShowsTwoTimerSlots()
        {
            _perks.Apply(Load("Companion"));
            _perks.Apply(Load("Scarecrow"));
            yield return null;
            yield return null;

            var view = Object.FindFirstObjectByType<ActivePerksView>();
            Assert.AreEqual(2, view.ShownCount);
            Assert.IsNotNull(view.Icon(0).sprite);
            Assert.IsNotNull(view.Icon(1).sprite);
            Assert.AreEqual(1f, view.Timer(0).fillAmount, 0.05f);
        }

        [UnityTest]
        public IEnumerator Notice_OnPickup_ShowsTheNameThenHides()
        {
            var notice = Object.FindFirstObjectByType<PerkNameNotice>();
            Assert.IsNotNull(notice, "Arena_Test has no PerkNameNotice");

            _perks.Apply(Load("Companion"));
            Assert.AreEqual("Begleiter", notice.Shown);
            Assert.Greater(notice.CurrentAlpha, 0.9f);

            _perks.Apply(Load("Scarecrow"));
            Assert.AreEqual("Strohmann", notice.Shown);

            float until = Time.realtimeSinceStartup + 3f;
            while (Time.realtimeSinceStartup < until && notice.CurrentAlpha > 0f)
            {
                yield return null;
            }

            Assert.AreEqual(0f, notice.CurrentAlpha);
        }

        [Test]
        public void DebugPerkSpawn_CyclesThroughThePoolAndDropsBothNewPerks()
        {
            var spawn = Object.FindFirstObjectByType<DebugPerkSpawn>();
            var pickups = Object.FindFirstObjectByType<PerkPickupPool>();
            Assert.IsNotNull(spawn, "Arena_Test has no DebugPerkSpawn");

            bool companion = false;
            bool scarecrow = false;
            int count = pickups.Config.Perks.Length;
            for (int i = 0; i < count; i++)
            {
                string id = spawn.Next.Id;
                companion |= id == "companion";
                scarecrow |= id == "scarecrow";
                spawn.Spawn();
            }

            Assert.IsTrue(companion && scarecrow);
            Assert.AreEqual(count, pickups.CountActive);
        }

        private static PerkDefinition Load(string asset)
        {
            return AssetDatabase.LoadAssetAtPath<PerkDefinition>(PerksDir + asset + ".asset");
        }
    }
}
