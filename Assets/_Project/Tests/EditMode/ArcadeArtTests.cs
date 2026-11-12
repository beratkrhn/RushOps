using NUnit.Framework;
using RushOps.Editor.Art;
using UnityEditor;
using UnityEngine;

namespace RushOps.Tests.EditMode
{
    public class ArcadeArtTests
    {
        private const string Folder = "Assets/_Project/Art/Sprites/Arcade/";

        private static readonly string[] Sprites =
        {
            "Arcade_Marquee", "Arcade_Bezel", "Arcade_ScreenFrame", "Arcade_ScreenGloss", "Arcade_ControlPanel",
            "Arcade_Body", "Arcade_StickBase", "Arcade_StickShaft", "Arcade_StickBall", "Arcade_Stick_C",
            "Arcade_Stick_E", "Arcade_Stick_NE", "Arcade_Stick_N", "Arcade_Stick_NW", "Arcade_Stick_W",
            "Arcade_Stick_SW", "Arcade_Stick_S", "Arcade_Stick_SE", "Arcade_DashButton", "Arcade_DashButton_Pressed",
            "Arcade_Button_Red", "Arcade_Button_Blue", "Arcade_Button_Green",
        };

        [TestCaseSource(nameof(Sprites))]
        public void Sprite_ExistsWithPixelArtImport(string name)
        {
            string path = Folder + name + ".png";
            Assert.IsNotNull(AssetDatabase.LoadAssetAtPath<Sprite>(path), path);

            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            Assert.IsTrue(PixelArtImportRules.Matches(importer), "Wrong import settings: " + path);
            Assert.AreEqual(32f, importer.spritePixelsPerUnit);
            Assert.AreEqual(FilterMode.Point, importer.filterMode);
            Assert.AreEqual(TextureImporterCompression.Uncompressed, importer.textureCompression);
        }

        [Test]
        public void CabinetParts_FillTheReferenceGrid()
        {
            var marquee = AssetDatabase.LoadAssetAtPath<Sprite>(Folder + "Arcade_Marquee.png");
            var bezel = AssetDatabase.LoadAssetAtPath<Sprite>(Folder + "Arcade_Bezel.png");
            var panel = AssetDatabase.LoadAssetAtPath<Sprite>(Folder + "Arcade_ControlPanel.png");
            var frame = AssetDatabase.LoadAssetAtPath<Sprite>(Folder + "Arcade_ScreenFrame.png");

            Assert.AreEqual(360f, marquee.rect.width);
            Assert.AreEqual(640f, marquee.rect.height + bezel.rect.height + panel.rect.height);
            Assert.AreEqual(new Vector2(320f, 320f), frame.rect.size);
        }

        [Test]
        public void Layout_IsImportedAsText()
        {
            Assert.IsNotNull(AssetDatabase.LoadAssetAtPath<TextAsset>(Folder + "Arcade_Layout.json"));
        }
    }
}
