using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace RushOps.Tests.EditMode
{
    /// <summary>Checks the generated art from <c>Tools/gen_pixelart.py</c> against ADR 0002.</summary>
    public class PixelArtPaletteTests
    {
        private const string ArtDir = "Assets/_Project/Art";
        private const string PalettePath = ArtDir + "/Palettes/RushOps_Palette.png";

        // Temporary art from issue #2 predates the palette.
        private const string PlaceholderFolder = "/Placeholder/";

        [Test]
        public void Palette_Has16To24DistinctOpaqueColors()
        {
            Color32[] colors = LoadPixels(PalettePath, out _, out _);
            var distinct = new HashSet<Color32>(colors);

            Assert.That(colors.Length, Is.InRange(16, 24));
            Assert.AreEqual(colors.Length, distinct.Count, "Palette contains duplicates");
            foreach (Color32 color in colors)
            {
                Assert.AreEqual(255, color.a, "Palette colors must be opaque");
            }
        }

        [Test]
        public void ArtPngs_UseOnlyPaletteColors()
        {
            var palette = new HashSet<Color32>(LoadPixels(PalettePath, out _, out _));
            var offenders = new List<string>();

            foreach (string path in Directory.GetFiles(ArtDir, "*.png", SearchOption.AllDirectories))
            {
                string normalized = path.Replace('\\', '/');
                if (normalized.Contains(PlaceholderFolder))
                {
                    continue;
                }

                foreach (Color32 pixel in LoadPixels(normalized, out _, out _))
                {
                    bool transparent = pixel.a == 0;
                    bool inPalette = pixel.a == 255 && palette.Contains(new Color32(pixel.r, pixel.g, pixel.b, 255));
                    if (!transparent && !inPalette)
                    {
                        offenders.Add($"{normalized} ({pixel})");
                        break;
                    }
                }
            }

            Assert.IsEmpty(offenders, "Colors outside the palette: " + string.Join(", ", offenders));
        }

        [TestCase("Sprites/Player/Player.png", 16, 16)]
        [TestCase("Sprites/Enemies/Walker_Walk_0.png", 16, 16)]
        [TestCase("Sprites/Enemies/Walker_Walk_1.png", 16, 16)]
        [TestCase("Sprites/Enemies/Walker_Walk_2.png", 16, 16)]
        [TestCase("Sprites/Enemies/Walker_Walk_3.png", 16, 16)]
        [TestCase("Sprites/Projectiles/Bullet.png", 8, 8)]
        [TestCase("Sprites/Effects/MuzzleFlash.png", 8, 8)]
        [TestCase("Tiles/Floor_0.png", 16, 16)]
        [TestCase("Tiles/Floor_1.png", 16, 16)]
        [TestCase("Tiles/Floor_2.png", 16, 16)]
        [TestCase("Tiles/Wall.png", 16, 16)]
        public void RequiredArt_ExistsWithExpectedSize(string relativePath, int width, int height)
        {
            LoadPixels(ArtDir + "/" + relativePath, out int actualWidth, out int actualHeight);

            Assert.AreEqual(width, actualWidth, "width");
            Assert.AreEqual(height, actualHeight, "height");
        }

        [Test]
        public void WalkerFrames_AreDifferent()
        {
            var frames = new List<string>();
            for (int i = 0; i < 4; i++)
            {
                byte[] bytes = File.ReadAllBytes($"{ArtDir}/Sprites/Enemies/Walker_Walk_{i}.png");
                frames.Add(System.Convert.ToBase64String(bytes));
            }

            Assert.AreNotEqual(frames[0], frames[1]);
            Assert.AreNotEqual(frames[0], frames[2]);
            Assert.AreNotEqual(frames[1], frames[3]);
        }

        // Reads the PNG file itself, so the result does not depend on import settings.
        private static Color32[] LoadPixels(string path, out int width, out int height)
        {
            Assert.IsTrue(File.Exists(path), $"Missing file: {path}");
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            try
            {
                Assert.IsTrue(texture.LoadImage(File.ReadAllBytes(path)), $"Not a valid PNG: {path}");
                width = texture.width;
                height = texture.height;
                return texture.GetPixels32();
            }
            finally
            {
                Object.DestroyImmediate(texture);
            }
        }
    }
}
