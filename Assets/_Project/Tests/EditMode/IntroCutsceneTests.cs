using System.Linq;
using NUnit.Framework;
using RushOps.Editor.Story;
using RushOps.Story;
using UnityEditor;
using UnityEngine;

namespace RushOps.Tests.EditMode
{
    /// <summary>The intro cutscene built from <c>Tools/gen_cutscenes.py</c> (issue #332). Palette: PixelArtPaletteTests.</summary>
    public class IntroCutsceneTests
    {
        private static CutsceneDefinition Load()
        {
            var cutscene = AssetDatabase.LoadAssetAtPath<CutsceneDefinition>(IntroCutsceneBuilder.CutscenePath);
            Assert.IsNotNull(cutscene, IntroCutsceneBuilder.CutscenePath);
            return cutscene;
        }

        [Test]
        public void Intro_HasSevenPanelsOf180x320()
        {
            CutsceneDefinition cutscene = Load();

            Assert.AreEqual(7, cutscene.PanelCount);
            Assert.AreEqual(new Vector2Int(180, 320), cutscene.PanelSize);
            for (int i = 0; i < cutscene.PanelCount; i++)
            {
                Sprite image = cutscene.GetPanel(i).Image;
                Assert.IsNotNull(image, $"panel {i + 1}");
                Assert.AreEqual(new Vector2(180f, 320f), image.rect.size, $"panel {i + 1}");
            }
        }

        [Test]
        public void EveryBox_HasTextAndStaysInsideItsPanel()
        {
            CutsceneDefinition cutscene = Load();

            for (int i = 0; i < cutscene.PanelCount; i++)
            {
                CutscenePanel panel = cutscene.GetPanel(i);
                Assert.Greater(panel.BoxCount, 0, $"panel {i + 1}");
                for (int b = 0; b < panel.BoxCount; b++)
                {
                    CutsceneBox box = panel.GetBox(b);
                    Assert.IsFalse(string.IsNullOrWhiteSpace(box.Text), $"panel {i + 1} box {b + 1}");
                    Assert.GreaterOrEqual(box.Position.x, 0);
                    Assert.GreaterOrEqual(box.Position.y, 0);
                    Assert.LessOrEqual(box.Position.x + box.Width, cutscene.PanelSize.x, $"panel {i + 1} box {b + 1}");
                    Assert.Less(box.Position.y, cutscene.PanelSize.y, $"panel {i + 1} box {b + 1}");
                }
            }
        }

        [Test]
        public void Warning_HeroCallsToRetireTheSearch()
        {
            CutscenePanel panel = Load().GetPanel(3);

            CutsceneBox hero = Enumerable.Range(0, panel.BoxCount).Select(panel.GetBox).First(b => b.Speaker == "HELD");
            Assert.AreEqual("Retired die Suche nach dem Toke!", hero.Text);
        }

        [Test]
        public void Showdown_HeroSpeaksFirstAndStandsAboveTheMan()
        {
            CutscenePanel showdown = Load().GetPanel(6);
            CutsceneBox[] boxes = Enumerable.Range(0, showdown.BoxCount).Select(showdown.GetBox).ToArray();

            int hero = System.Array.FindIndex(boxes, b => b.Speaker == "HELD");
            int man = System.Array.FindIndex(boxes, b => b.Speaker == "DER MANN");
            Assert.GreaterOrEqual(hero, 0);
            Assert.Greater(man, hero, "the hero greets first");
            Assert.Less(boxes[hero].Position.y, boxes[man].Position.y, "the hero is on top");
            Assert.AreEqual("assalamu aleykum wa rahmetullahi wa barakatuhu", boxes[hero].Text);
            Assert.AreEqual("wa aleykum ya ars", boxes[man].Text);
        }

        [Test]
        public void IntroScene_IsInTheBuild()
        {
            Assert.IsTrue(EditorBuildSettings.scenes.Any(s => s.path == IntroSceneBuilder.ScenePath && s.enabled));
        }
    }
}
