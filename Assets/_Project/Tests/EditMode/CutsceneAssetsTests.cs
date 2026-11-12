using NUnit.Framework;
using RushOps.Editor.Story;
using RushOps.Story;
using UnityEditor;
using UnityEngine;

namespace RushOps.Tests.EditMode
{
    public class CutsceneAssetsTests
    {
        [Test]
        public void CutsceneConfig_ExistsWithPositiveTiming()
        {
            var config = AssetDatabase.LoadAssetAtPath<CutsceneConfig>(StoryAssetsBuilder.CutsceneConfigPath);

            Assert.IsNotNull(config, StoryAssetsBuilder.CutsceneConfigPath);
            Assert.Greater(config.Timing.BoxDelay, 0f);
            Assert.Greater(config.Timing.BoxFade, 0f);
            Assert.Greater(config.Timing.PanelFade, 0f);
            Assert.Greater(config.HintBlink, 0f);
        }

        [Test]
        public void Definition_CountsBoxesPerPanel()
        {
            var definition = ScriptableObject.CreateInstance<CutsceneDefinition>();
            definition.SetContent(new Vector2Int(180, 320), new[]
            {
                new CutscenePanel(null, new[] { Box("A"), Box("B") }),
                new CutscenePanel(null, null),
                new CutscenePanel(null, new[] { Box("C"), Box("D"), Box("E") }),
            });

            CollectionAssert.AreEqual(new[] { 2, 0, 3 }, definition.BoxCounts());
            Assert.AreEqual(3, definition.MaxBoxes);
            Assert.AreEqual("D", definition.GetPanel(2).GetBox(1).Text);
            Object.DestroyImmediate(definition);
        }

        [Test]
        public void Box_HasSpeakerOnlyForSpeech()
        {
            var narration = new CutsceneBox(CutsceneBoxKind.Narration, "HELD", Color.white, "x", Vector2Int.zero, 100);
            var speech = new CutsceneBox(CutsceneBoxKind.Speech, "HELD", Color.white, "x", Vector2Int.zero, 100);
            var silent = new CutsceneBox(CutsceneBoxKind.Speech, "", Color.white, "x", Vector2Int.zero, 100);

            Assert.IsFalse(narration.HasSpeaker);
            Assert.IsTrue(speech.HasSpeaker);
            Assert.IsFalse(silent.HasSpeaker);
        }

        private static CutsceneBox Box(string text)
        {
            return new CutsceneBox(CutsceneBoxKind.Narration, "", Color.white, text, Vector2Int.zero, 100);
        }
    }
}
