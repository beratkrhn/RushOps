using System.Collections.Generic;
using NUnit.Framework;
using RushOps.Editor.Art;
using UnityEditor;

namespace RushOps.Tests.EditMode
{
    public class PixelArtImportTests
    {
        [TestCase("Assets/_Project/Art/Sprites/Player/Player.png", true)]
        [TestCase("Assets/_Project/Art/Tiles/Floor_0.PNG", true)]
        [TestCase("Assets/_Project/Art/Palettes/RushOps_Palette.png", true)]
        [TestCase("Assets/_Project/Art/Tiles/Floor_0.asset", false)]
        [TestCase("Assets/_Project/Prefabs/Icon.png", false)]
        [TestCase("Assets/Art/Player.png", false)]
        [TestCase("", false)]
        [TestCase(null, false)]
        public void AppliesTo_ChecksFolderAndExtension(string path, bool expected)
        {
            Assert.AreEqual(expected, PixelArtImportRules.AppliesTo(path));
        }

        [Test]
        public void ArtTextures_UsePixelArtImportSettings()
        {
            var wrong = new List<string>();
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/_Project/Art" });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!PixelArtImportRules.AppliesTo(path))
                {
                    continue;
                }

                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                if (!PixelArtImportRules.Matches(importer))
                {
                    wrong.Add(path);
                }
            }

            Assert.IsNotEmpty(guids, "No textures found under Art");
            Assert.IsEmpty(wrong, "Wrong import settings: " + string.Join(", ", wrong));
        }
    }
}
