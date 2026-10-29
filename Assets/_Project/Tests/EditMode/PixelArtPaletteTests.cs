using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace RushOps.Tests.EditMode
{
    /// <summary>Checks the art from <c>Tools/gen_pixelart.py</c> and <c>Tools/import_character.py</c> against ADR 0006.</summary>
    public class PixelArtPaletteTests
    {
        private const string ArtDir = "Assets/_Project/Art";
        private const string PalettePath = ArtDir + "/Palettes/RushOps_Palette.png";
        private const int WalkerFrames = 4;
        private static readonly string[] Directions = { "E", "NE", "N", "NW", "W", "SW", "S", "SE" };

        [Test]
        public void Palette_Has16To32DistinctOpaqueColors()
        {
            Color32[] colors = LoadPixels(PalettePath, out _, out _);
            var distinct = new HashSet<Color32>(colors);

            Assert.That(colors.Length, Is.InRange(16, 32));
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

        [TestCase("Sprites/Projectiles/Bullet.png", 16, 16)]
        [TestCase("Sprites/Effects/MuzzleFlash.png", 16, 16)]
        [TestCase("Tiles/Floor_0.png", 32, 32)]
        [TestCase("Tiles/Floor_1.png", 32, 32)]
        [TestCase("Tiles/Floor_2.png", 32, 32)]
        [TestCase("Tiles/Wall_Top.png", 32, 32)]
        [TestCase("Tiles/Wall_Front.png", 32, 32)]
        [TestCase("Sprites/Effects/SpawnMarker.png", 32, 32)]
        [TestCase("Sprites/UI/Stick_Base.png", 32, 32)]
        [TestCase("Sprites/UI/Stick_Knob.png", 16, 16)]
        [TestCase("Sprites/UI/Dash_Button.png", 24, 24)]
        [TestCase("Sprites/UI/Dash_Cooldown.png", 24, 24)]
        public void RequiredArt_ExistsWithExpectedSize(string relativePath, int width, int height)
        {
            LoadPixels(ArtDir + "/" + relativePath, out int actualWidth, out int actualHeight);

            Assert.AreEqual(width, actualWidth, "width");
            Assert.AreEqual(height, actualHeight, "height");
        }

        [Test]
        public void Characters_HaveAllDirectionsAt64x64()
        {
            foreach (string direction in Directions)
            {
                AssertSize($"Sprites/Player/Player_Idle_{direction}.png", 64, 64);
                for (int frame = 0; frame < WalkerFrames; frame++)
                {
                    AssertSize($"Sprites/Enemies/Walker_Walk_{direction}_{frame}.png", 64, 64);
                }
            }
        }

        [Test]
        public void WalkerFrames_DifferWithinEachDirection()
        {
            foreach (string direction in Directions)
            {
                var frames = new HashSet<string>();
                for (int frame = 0; frame < WalkerFrames; frame++)
                {
                    byte[] bytes = File.ReadAllBytes($"{ArtDir}/Sprites/Enemies/Walker_Walk_{direction}_{frame}.png");
                    frames.Add(System.Convert.ToBase64String(bytes));
                }

                Assert.GreaterOrEqual(frames.Count, 3, $"Walker {direction} barely animates");
            }
        }

        private static void AssertSize(string relativePath, int width, int height)
        {
            LoadPixels(ArtDir + "/" + relativePath, out int actualWidth, out int actualHeight);
            Assert.AreEqual(width, actualWidth, relativePath + " width");
            Assert.AreEqual(height, actualHeight, relativePath + " height");
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
