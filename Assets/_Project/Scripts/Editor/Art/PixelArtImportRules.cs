using UnityEditor;
using UnityEngine;

namespace RushOps.Editor.Art
{
    /// <summary>
    /// Import rules for pixel art from ADR 0002 and ADR 0006: sprite, 32 PPU, point filter, no compression,
    /// no mip maps, characters pivoted at their feet. Applies to every PNG under <c>Assets/_Project/Art/</c>.
    /// </summary>
    public static class PixelArtImportRules
    {
        /// <summary>Folder whose PNGs are imported as pixel art.</summary>
        public const string ArtRoot = "Assets/_Project/Art/";

        /// <summary>Pixels per unit: one 32x32 tile equals one Unity unit.</summary>
        public const int PixelsPerUnit = 32;

        // Character sprites are pivoted bottom center so they can be sorted by their feet.
        private static readonly string[] FootPivotFolders =
        {
            ArtRoot + "Sprites/Player/",
            ArtRoot + "Sprites/Enemies/",
        };

        /// <summary>Returns true if the asset at <paramref name="assetPath"/> must use the pixel art import settings.</summary>
        public static bool AppliesTo(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
            {
                return false;
            }

            string path = assetPath.Replace('\\', '/');
            return path.StartsWith(ArtRoot, System.StringComparison.Ordinal)
                && path.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>Returns true if the sprite at <paramref name="assetPath"/> is a character pivoted at its feet.</summary>
        public static bool UsesFootPivot(string assetPath)
        {
            if (!AppliesTo(assetPath))
            {
                return false;
            }

            string path = assetPath.Replace('\\', '/');
            foreach (string folder in FootPivotFolders)
            {
                if (path.StartsWith(folder, System.StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>Sprite alignment for the asset at <paramref name="assetPath"/>.</summary>
        public static SpriteAlignment AlignmentFor(string assetPath)
        {
            return UsesFootPivot(assetPath) ? SpriteAlignment.BottomCenter : SpriteAlignment.Center;
        }

        /// <summary>Writes the pixel art settings into <paramref name="importer"/>.</summary>
        public static void Apply(TextureImporter importer)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = PixelsPerUnit;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;

            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)AlignmentFor(importer.assetPath);
            importer.SetTextureSettings(settings);
        }

        /// <summary>Returns true if <paramref name="importer"/> already matches the pixel art settings.</summary>
        public static bool Matches(TextureImporter importer)
        {
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);

            return importer.textureType == TextureImporterType.Sprite
                && settings.spriteAlignment == (int)AlignmentFor(importer.assetPath)
                && Mathf.Approximately(importer.spritePixelsPerUnit, PixelsPerUnit)
                && importer.filterMode == FilterMode.Point
                && importer.textureCompression == TextureImporterCompression.Uncompressed
                && !importer.mipmapEnabled;
        }
    }
}
