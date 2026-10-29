using UnityEditor;
using UnityEngine;

namespace RushOps.Editor.Art
{
    /// <summary>
    /// Import rules for pixel art from ADR 0002: sprite, 16 PPU, point filter, no compression, no mip maps.
    /// Applies to every PNG under <c>Assets/_Project/Art/</c>.
    /// </summary>
    public static class PixelArtImportRules
    {
        /// <summary>Folder whose PNGs are imported as pixel art.</summary>
        public const string ArtRoot = "Assets/_Project/Art/";

        /// <summary>Pixels per unit: one 16x16 tile equals one Unity unit.</summary>
        public const int PixelsPerUnit = 16;

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
        }

        /// <summary>Returns true if <paramref name="importer"/> already matches the pixel art settings.</summary>
        public static bool Matches(TextureImporter importer)
        {
            return importer.textureType == TextureImporterType.Sprite
                && Mathf.Approximately(importer.spritePixelsPerUnit, PixelsPerUnit)
                && importer.filterMode == FilterMode.Point
                && importer.textureCompression == TextureImporterCompression.Uncompressed
                && !importer.mipmapEnabled;
        }
    }
}
