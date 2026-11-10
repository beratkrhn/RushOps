using System.Collections;
using NUnit.Framework;
using RushOps.Enemies;
using RushOps.Player;
using RushOps.Rush;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace RushOps.Tests.PlayMode
{
    /// <summary>The rush button with its meter in the HUD (issue #271, F-HUD-09, F-RSH-01, F-RSH-02, N18).</summary>
    public class RushButtonTests
    {
        private PlayerRush _rush;
        private RushButtonView _button;
        private Vector2 _center;

        private IEnumerator Load(string scene)
        {
            SaveGameIsolation.Reset();
            yield return SceneManager.LoadSceneAsync(scene, LoadSceneMode.Single);
            yield return null;
            var director = Object.FindAnyObjectByType<WaveDirector>();
            if (director != null)
            {
                director.enabled = false;
            }

            var restarter = Object.FindAnyObjectByType<LevelRestarter>();
            if (restarter != null)
            {
                restarter.enabled = false;
            }

            var player = Object.FindAnyObjectByType<PlayerMovement>();
            _center = player.transform.position;
            _rush = player.GetComponent<PlayerRush>();
            _button = Object.FindAnyObjectByType<RushButtonView>();
            Assert.IsNotNull(_button, $"no rush button in {scene}");
        }

        [UnityTearDown]
        public IEnumerator Restore()
        {
            Time.timeScale = 1f;
            SaveGameIsolation.Reset();
            yield return null;
        }

        [UnityTest]
        public IEnumerator Kills_FillTheRing_FullButtonLightsUp_TapStartsRush_RingRunsDown()
        {
            yield return Load("Arena_Test");
            Button button = _button.GetComponent<Button>();
            yield return null;
            Assert.AreEqual(0f, _button.RingFill, 1e-5f, "the ring starts filled");
            Assert.IsFalse(_button.ShowsFull);

            button.onClick.Invoke();
            Assert.IsFalse(_rush.IsActive, "a tap on the empty button started the rush");

            for (int i = 0; i < 3; i++)
            {
                EnemyPools.Of(EnemyPools.Walker).Spawn(_center + new Vector2(-3f + 3f * i, 6f)).Kill();
            }

            yield return null;
            Assert.Greater(_button.RingFill, 0f, "kills did not fill the ring");
            Assert.Less(_button.RingFill, 1f);

            _rush.FillUp();
            yield return null;
            Assert.IsTrue(_button.ShowsFull, "the full button does not light up");
            Assert.AreEqual(1f, _button.RingFill, 1e-5f);
            float smallest = float.MaxValue;
            float largest = 0f;
            Transform visual = _button.transform.Find("Visual");
            for (float end = Time.realtimeSinceStartup + 0.7f; Time.realtimeSinceStartup < end;)
            {
                smallest = Mathf.Min(smallest, visual.localScale.x);
                largest = Mathf.Max(largest, visual.localScale.x);
                yield return null;
            }

            Assert.Greater(largest - smallest, 0.05f, "the full button does not pulse");

            button.onClick.Invoke();
            Assert.IsTrue(_rush.IsActive, "a tap on the full button did not start the rush");
            yield return new WaitForSeconds(1f);
            Assert.IsTrue(_button.ShowsRush);
            Assert.IsFalse(_button.ShowsFull);
            Assert.Less(_button.RingFill, 0.9f, "the ring does not run down");
            Assert.Greater(_button.RingFill, 0.6f);

            float stop = Time.time + 5f;
            while (Time.time < stop && _rush.IsActive)
            {
                yield return null;
            }

            yield return null;
            Assert.IsFalse(_button.ShowsRush);
            Assert.AreEqual(0f, _button.RingFill, 1e-5f, "the button is not empty after the rush");
            Assert.AreEqual(1f, visual.localScale.x, 1e-5f);
        }

        [UnityTest]
        public IEnumerator Button_BelowThePerks_ClearOfHeartsPerksAndPause_InsideSafeArea()
        {
            yield return Load("Arena_Test");
            Rect button = HudRects.OnScreen((RectTransform)HudRects.Find("RushButton").Find("Visual"));
            Rect perks = HudRects.OnScreen(HudRects.Find("Perk_0"));
            Assert.Less(button.yMax, perks.yMin + 0.5f, "the button is not below the perk row");
            Assert.AreEqual(perks.xMin, button.xMin, 1f, "the button is not aligned with the perks on the left");
            HudRects.AssertApart("RushButton", button, "Hearts", HudRects.OnScreen(HudRects.Find("Hearts")));
            HudRects.AssertApart("RushButton", button, "Perk_0", perks);
            HudRects.AssertApart("RushButton", button, "PauseButton", HudRects.OnScreen(HudRects.Find("PauseButton")));

            Rect safe = Screen.safeArea;
            Assert.IsTrue(safe.Contains(button.min) && safe.Contains(button.max), $"button {button} outside {safe}");
        }

        [UnityTest]
        public IEnumerator Button_StaysPut_WhenPerksRun()
        {
            yield return Load("Arena_Test");
            var rect = (RectTransform)HudRects.Find("RushButton");
            Vector3 before = rect.position;
            var perk = UnityEditor.AssetDatabase.LoadAssetAtPath<Perks.PerkDefinition>("Assets/_Project/Data/Perks/SpeedBoost.asset");
            Assert.IsNotNull(perk);
            Object.FindAnyObjectByType<PlayerMovement>().GetComponent<Perks.PlayerPerks>().Apply(perk);
            yield return null;

            Assert.AreEqual(before, rect.position);
        }

        [UnityTest]
        public IEnumerator EveryGameScene_HasTheButton()
        {
            foreach (string scene in new[] { "Level_1-1", "Level_2-2", "Level_3-3", "Endless_Arena" })
            {
                yield return Load(scene);
            }
        }
    }
}
