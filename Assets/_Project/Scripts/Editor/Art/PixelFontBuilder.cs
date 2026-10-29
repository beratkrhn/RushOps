using System;
using UnityEditor;
using UnityEngine;

namespace RushOps.Editor.Art
{
    /// <summary>
    /// Builds the bitmap font asset of the pixel font from the glyph atlas and metrics written by
    /// <c>Tools/gen_pixelart.py</c> (issue #11). The font works with uGUI <c>Text</c>; it renders at its
    /// native pixel size, so text is scaled by whole numbers through its <c>RectTransform</c>.
    /// Existing assets are updated in place and keep their GUIDs.
    /// </summary>
    public static class PixelFontBuilder
    {
        /// <summary>Asset path of the font.</summary>
        public const string FontPath = FontsDir + "/PixelFont.fontsettings";

        /// <summary>
        /// Font size that renders the glyphs at their pixel size. <c>Text</c> scales bitmap fonts by its
        /// font size relative to this, so texts use exactly this size and scale through their transform.
        /// </summary>
        public const int NativeSize = 10;

        private const string FontsDir = "Assets/_Project/Art/Fonts";
        private const string AtlasPath = FontsDir + "/PixelFont.png";
        private const string MetricsPath = FontsDir + "/PixelFont.json";
        private const string MaterialPath = FontsDir + "/PixelFont.mat";
        private const string UiShader = "UI/Default";

        [MenuItem("RushOps/Art/Pixel-Schrift aktualisieren")]
        public static Font Build()
        {
            var atlas = AssetDatabase.LoadAssetAtPath<Texture2D>(AtlasPath);
            var metricsAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(MetricsPath);
            if (atlas == null || metricsAsset == null)
            {
                throw new InvalidOperationException($"Pixel font atlas or metrics missing in {FontsDir}; run Tools/gen_pixelart.py");
            }

            FontMetrics metrics = JsonUtility.FromJson<FontMetrics>(metricsAsset.text);

            var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (material == null)
            {
                material = new Material(Shader.Find(UiShader));
                AssetDatabase.CreateAsset(material, MaterialPath);
            }

            material.mainTexture = atlas;
            EditorUtility.SetDirty(material);

            var font = AssetDatabase.LoadAssetAtPath<Font>(FontPath);
            if (font == null)
            {
                font = new Font("PixelFont");
                AssetDatabase.CreateAsset(font, FontPath);
            }

            font.material = material;
            font.characterInfo = CharacterInfos(metrics);
            var serialized = new SerializedObject(font);
            SetFloat(serialized, "m_LineSpacing", metrics.lineHeight);
            SetFloat(serialized, "m_FontSize", NativeSize);
            SetFloat(serialized, "m_Ascent", metrics.ascent);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(font);
            AssetDatabase.SaveAssets();
            return font;
        }

        private static CharacterInfo[] CharacterInfos(FontMetrics metrics)
        {
            float width = metrics.atlasWidth;
            float height = metrics.atlasHeight;
            var infos = new CharacterInfo[metrics.glyphs.Length + 1];
            infos[0] = new CharacterInfo { index = ' ', advance = metrics.spaceAdvance };
            for (int i = 0; i < metrics.glyphs.Length; i++)
            {
                GlyphMetrics glyph = metrics.glyphs[i];
                // The atlas rows run top-down; texture coordinates run bottom-up.
                float u0 = glyph.x / width;
                float u1 = (glyph.x + glyph.width) / width;
                float vTop = 1f - glyph.y / height;
                float vBottom = 1f - (glyph.y + glyph.height) / height;
                infos[i + 1] = new CharacterInfo
                {
                    index = glyph.@char,
                    advance = glyph.advance,
                    minX = 0,
                    maxX = glyph.width,
                    minY = glyph.minY,
                    maxY = glyph.minY + glyph.height,
                    uvBottomLeft = new Vector2(u0, vBottom),
                    uvBottomRight = new Vector2(u1, vBottom),
                    uvTopLeft = new Vector2(u0, vTop),
                    uvTopRight = new Vector2(u1, vTop),
                };
            }

            return infos;
        }

        private static void SetFloat(SerializedObject serialized, string name, float value)
        {
            SerializedProperty property = serialized.FindProperty(name);
            if (property != null)
            {
                property.floatValue = value;
            }
        }

        // Shape of PixelFont.json; field names follow the JSON.
#pragma warning disable 0649
        [Serializable]
        private class FontMetrics
        {
            public int atlasWidth;
            public int atlasHeight;
            public int lineHeight;
            public int ascent;
            public int spaceAdvance;
            public GlyphMetrics[] glyphs;
        }

        [Serializable]
        private class GlyphMetrics
        {
            public int @char;
            public int x;
            public int y;
            public int width;
            public int height;
            public int minY;
            public int advance;
        }
#pragma warning restore 0649
    }
}
