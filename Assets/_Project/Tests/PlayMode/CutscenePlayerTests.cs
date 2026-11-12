using System.Collections;
using NUnit.Framework;
using RushOps.Story;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace RushOps.Tests.PlayMode
{
    /// <summary>Plays a two-panel test cutscene through the uGUI player (issue #331).</summary>
    public class CutscenePlayerTests
    {
        private GameObject _canvas;
        private CutsceneConfig _config;
        private CutsceneDefinition _cutscene;
        private Texture2D _texture;

        [SetUp]
        public void SetUp()
        {
            _canvas = new GameObject("Canvas", typeof(RectTransform));
            var canvas = _canvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = _canvas.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(360f, 640f);

            _config = ScriptableObject.CreateInstance<CutsceneConfig>();
            _texture = new Texture2D(180, 320);
            Sprite sprite = Sprite.Create(_texture, new Rect(0f, 0f, 180f, 320f), new Vector2(0.5f, 0.5f), 32f);
            var hero = new Color32(47, 110, 168, 255);
            _cutscene = ScriptableObject.CreateInstance<CutsceneDefinition>();
            _cutscene.SetContent(new Vector2Int(180, 320), new[]
            {
                new CutscenePanel(sprite, new[]
                {
                    new CutsceneBox(CutsceneBoxKind.Narration, "", hero, "Es begann mit einem Satz.", new Vector2Int(4, 4), 172),
                    new CutsceneBox(CutsceneBoxKind.Speech, "HELD", hero, "Retired die Suche nach dem Toke!",
                        new Vector2Int(4, 40), 172),
                }),
                new CutscenePanel(sprite, new[]
                {
                    new CutsceneBox(CutsceneBoxKind.Narration, "", hero, "Nur noch einer.", new Vector2Int(4, 4), 172),
                }),
            });
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(_canvas);
            Object.Destroy(_cutscene);
            Object.Destroy(_config);
            Object.Destroy(_texture);
        }

        private CutscenePlayer CreatePlayer()
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return CutscenePlayer.Create((RectTransform)_canvas.transform, font, 10, _config, 2, Color.black);
        }

        [UnityTest]
        public IEnumerator Taps_PlayBothPanelsToTheEnd()
        {
            CutscenePlayer player = CreatePlayer();
            int finished = 0;
            player.Finished += () => finished++;
            player.Play(_cutscene);
            yield return null;
            yield return null;

            TapUntil(player, () => player.Playback.VisibleBoxes >= 1);
            yield return null;
            Assert.AreEqual("Es begann mit einem Satz.", player.GetBoxView(0).BodyText);
            Assert.AreEqual("", player.GetBoxView(0).SpeakerText);
            Assert.IsTrue(player.GetBoxView(0).gameObject.activeSelf);

            TapUntil(player, () => player.Playback.VisibleBoxes >= 2);
            yield return null;
            Assert.AreEqual("HELD", player.GetBoxView(1).SpeakerText);
            Assert.AreEqual("Retired die Suche nach dem Toke!", player.GetBoxView(1).BodyText);

            player.Tap(); // panel complete: fade out
            yield return WaitUntil(() => player.Playback.PanelIndex == 1, 2f);
            Assert.AreEqual("Nur noch einer.", player.GetBoxView(0).BodyText);
            Assert.IsFalse(player.GetBoxView(1).gameObject.activeSelf, "box of the first panel hidden");

            TapUntil(player, () => player.Playback.WaitsForTap);
            player.Tap();
            yield return WaitUntil(() => !player.IsPlaying, 2f);

            Assert.AreEqual(1, finished);
        }

        [UnityTest]
        public IEnumerator Boxes_AppearByThemselves_ThenTheHintBlinks()
        {
            CutscenePlayer player = CreatePlayer();
            player.Play(_cutscene);

            yield return WaitUntil(() => player.Playback.WaitsForTap, 5f);
            Assert.AreEqual(2, player.Playback.VisibleBoxes);
            yield return WaitUntil(() => player.HintVisible, 2f);
            yield return new WaitForSecondsRealtime(0.4f);
            Assert.AreEqual(1f, player.GetBoxView(1).Alpha, 1e-3f);
            Assert.AreEqual(0, player.Playback.PanelIndex, "waits for a tap");
        }

        [UnityTest]
        public IEnumerator Skip_EndsAtOnce()
        {
            CutscenePlayer player = CreatePlayer();
            int finished = 0;
            player.Finished += () => finished++;
            player.Play(_cutscene);
            yield return null;

            player.Skip();
            yield return null;

            Assert.IsFalse(player.IsPlaying);
            Assert.AreEqual(1, finished);
        }

        private static void TapUntil(CutscenePlayer player, System.Func<bool> condition)
        {
            for (int i = 0; i < 10 && !condition(); i++)
            {
                player.Tap();
            }

            Assert.IsTrue(condition(), "taps did not get there");
        }

        private static IEnumerator WaitUntil(System.Func<bool> condition, float timeout)
        {
            float end = Time.realtimeSinceStartup + timeout;
            while (!condition() && Time.realtimeSinceStartup < end)
            {
                yield return null;
            }

            Assert.IsTrue(condition(), "timed out");
        }
    }
}
