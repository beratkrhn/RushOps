using System.Collections;
using NUnit.Framework;
using RushOps.Enemies;
using RushOps.Player;
using RushOps.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace RushOps.Tests.PlayMode
{
    public class HudTests
    {
        private const string SceneName = "Arena_Test";

        private PlayerHealth _player;
        private HeartsView _hearts;
        private PauseController _pause;

        [UnitySetUp]
        public IEnumerator LoadScene()
        {
            yield return SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);
            yield return null;

            _player = Object.FindFirstObjectByType<PlayerHealth>();
            _hearts = Object.FindFirstObjectByType<HeartsView>();
            _pause = Object.FindFirstObjectByType<PauseController>();
            Assert.IsNotNull(_hearts, "Arena_Test has no HeartsView");
            Assert.IsNotNull(_pause, "Arena_Test has no PauseController");
            Object.FindFirstObjectByType<EnemySpawner>().enabled = false;
        }

        [TearDown]
        public void RestoreTime()
        {
            Time.timeScale = 1f;
        }

        [Test]
        public void Hearts_ShowAllAtStart()
        {
            Assert.AreEqual(_player.Max, _hearts.ShownHearts);
            Assert.AreEqual(_player.Max, HeartImages().Length);
        }

        [UnityTest]
        public IEnumerator Hearts_AfterDamage_LostHeartBlinksThenShowsEmpty()
        {
            Image lost = HeartImages()[_player.Max - 1];
            Sprite full = lost.sprite;

            _player.TakeDamage(1);
            Assert.IsTrue(_hearts.IsFlashing);
            bool seenFull = false;
            bool seenEmpty = false;
            float until = Time.realtimeSinceStartup + 0.5f;
            while (Time.realtimeSinceStartup < until)
            {
                seenFull |= lost.sprite == full;
                seenEmpty |= lost.sprite != full;
                yield return null;
            }

            Assert.IsTrue(seenFull && seenEmpty, "lost heart blinks");
            yield return new WaitForSecondsRealtime(0.5f);

            Assert.IsFalse(_hearts.IsFlashing);
            Assert.AreNotEqual(full, lost.sprite, "lost heart shows empty");
            Assert.AreEqual(_player.Max - 1, _hearts.ShownHearts);
        }

        [UnityTest]
        public IEnumerator Pause_StopsTimeAndShowsOverlay_ResumeContinues()
        {
            GameObject overlay = OverlayObject();
            Assert.IsFalse(overlay.activeSelf);

            _pause.Pause();
            yield return null;

            Assert.IsTrue(_pause.IsPaused);
            Assert.AreEqual(0f, Time.timeScale);
            Assert.IsTrue(overlay.activeSelf);

            _pause.Resume();
            yield return null;

            Assert.IsFalse(_pause.IsPaused);
            Assert.AreEqual(1f, Time.timeScale);
            Assert.IsFalse(overlay.activeSelf);
        }

        [UnityTest]
        public IEnumerator PauseButton_Click_Pauses()
        {
            Button button = GameObject.Find("PauseButton").GetComponent<Button>();

            button.onClick.Invoke();
            yield return null;

            Assert.IsTrue(_pause.IsPaused);
        }

        [UnityTest]
        public IEnumerator AppToBackground_Pauses()
        {
            _pause.SendMessage("OnApplicationPause", true);
            yield return null;

            Assert.IsTrue(_pause.IsPaused);
        }

        [UnityTest]
        public IEnumerator Restart_ReloadsLevelAndRunsTime()
        {
            _pause.Pause();
            _player.TakeDamage(1);

            _pause.Restart();
            yield return null;
            yield return null;

            PlayerHealth restarted = Object.FindFirstObjectByType<PlayerHealth>();
            Assert.AreNotSame(_player, restarted);
            Assert.AreEqual(restarted.Max, restarted.Current);
            Assert.AreEqual(1f, Time.timeScale);
        }

        [Test]
        public void Hud_LiesInsideSafeArea()
        {
            Rect safe = Screen.safeArea;
            foreach (string name in new[] { "Hearts", "PauseButton" })
            {
                var rect = GameObject.Find(name).GetComponent<RectTransform>();
                var corners = new Vector3[4];
                rect.GetWorldCorners(corners);
                foreach (Vector3 corner in corners)
                {
                    Assert.IsTrue(corner.x >= safe.xMin - 0.5f && corner.x <= safe.xMax + 0.5f
                        && corner.y >= safe.yMin - 0.5f && corner.y <= safe.yMax + 0.5f, $"{name} corner {corner} outside {safe}");
                }
            }
        }

        private Image[] HeartImages()
        {
            return _hearts.GetComponentsInChildren<Image>();
        }

        private GameObject OverlayObject()
        {
            return _pause.transform.Find("PauseOverlay").gameObject;
        }
    }
}
