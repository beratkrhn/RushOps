using UnityEditor;

namespace RushOps.Editor.Art
{
    /// <summary>
    /// Enforces <see cref="PixelArtImportRules"/> on every texture import under <c>Assets/_Project/Art/</c>,
    /// so generated PNGs never need manual import settings.
    /// </summary>
    public class PixelArtImportPostprocessor : AssetPostprocessor
    {
        // Bump when the rules change, so Unity reimports affected textures.
        private const uint Version = 1;

        public override uint GetVersion()
        {
            return Version;
        }

        private void OnPreprocessTexture()
        {
            if (!PixelArtImportRules.AppliesTo(assetPath))
            {
                return;
            }

            PixelArtImportRules.Apply((TextureImporter)assetImporter);
        }
    }
}
