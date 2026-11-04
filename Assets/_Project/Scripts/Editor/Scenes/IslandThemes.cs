using System.Collections.Generic;
using RushOps.Levels;
using UnityEditor;
using UnityEngine;

namespace RushOps.Editor.Scenes
{
    /// <summary>
    /// Creates and refreshes the <see cref="IslandTheme"/> assets from the sprites of <c>Tools/gen_pixelart.py</c>
    /// (issue #104). Sprite references are refreshed on every run; tuned values (floor share, decoration density)
    /// stay as they are.
    /// </summary>
    internal static class IslandThemes
    {
        /// <summary>The stone look of the test arenas.</summary>
        public const string DefaultPath = ThemesDir + "/Default.asset";

        /// <summary>Island 1: beach.</summary>
        public const string StrandPath = ThemesDir + "/Strand.asset";

        /// <summary>Island 2: dark city (issue #105).</summary>
        public const string StadtPath = ThemesDir + "/Stadt.asset";

        /// <summary>Island 3: volcano (issue #106).</summary>
        public const string VulkanPath = ThemesDir + "/Vulkan.asset";

        private const string ThemesDir = "Assets/_Project/Data/Islands";
        private const string TilesDir = "Assets/_Project/Art/Tiles";
        private const string DefaultGatePath = "Assets/_Project/Art/Sprites/Level/Gate_Bars.png";

        // Start values of new themes.
        private const float DefaultPlainFloorShare = 0.7f;
        private const float DefaultDecorationDensity = 0.05f;

        /// <summary>Loads the theme at <paramref name="path"/>, creating it first if needed, with fresh sprite references.</summary>
        public static IslandTheme LoadOrCreate(string path)
        {
            switch (path)
            {
                case StrandPath:
                    return Island(StrandPath, "Strand", "Strand");
                case StadtPath:
                    return Island(StadtPath, "Düstere Stadt", "Stadt");
                case VulkanPath:
                    return Island(VulkanPath, "Vulkan", "Vulkan");
                default:
                    return Default();
            }
        }

        private static IslandTheme Default()
        {
            IslandTheme theme = Load(DefaultPath, out SerializedObject serialized, out bool created);
            serialized.FindProperty("_displayName").stringValue = "Testarena";
            SetSprites(serialized, "_floor", Numbered($"{TilesDir}/Floor_"));
            SetSprite(serialized, "_wallTop", $"{TilesDir}/Wall_Top.png");
            SetSprite(serialized, "_wallFront", $"{TilesDir}/Wall_Front.png");
            SetSprite(serialized, "_obstacleTop", null);
            SetSprite(serialized, "_obstacleFront", null);
            SetSprites(serialized, "_decorations", new List<Sprite>());
            SetSprites(serialized, "_gateFrames", new List<Sprite> { Require(DefaultGatePath) });
            return Save(theme, serialized, created);
        }

        // An island tileset under Tiles/<folder>/ (gen_pixelart.island_files).
        private static IslandTheme Island(string path, string displayName, string folder)
        {
            string dir = $"{TilesDir}/{folder}";
            IslandTheme theme = Load(path, out SerializedObject serialized, out bool created);
            serialized.FindProperty("_displayName").stringValue = displayName;
            SetSprites(serialized, "_floor", Numbered($"{dir}/Floor_"));
            SetSprite(serialized, "_wallTop", $"{dir}/Wall_Top.png");
            SetSprite(serialized, "_wallFront", $"{dir}/Wall_Front.png");
            SetSprite(serialized, "_obstacleTop", $"{dir}/Obstacle_Top.png");
            SetSprite(serialized, "_obstacleFront", $"{dir}/Obstacle_Front.png");
            SetSprites(serialized, "_decorations", Numbered($"{dir}/Decor_"));
            SetSprites(serialized, "_gateFrames", Numbered($"{dir}/Gate_"));
            return Save(theme, serialized, created);
        }

        private static IslandTheme Load(string path, out SerializedObject serialized, out bool created)
        {
            var theme = AssetDatabase.LoadAssetAtPath<IslandTheme>(path);
            created = theme == null;
            if (created)
            {
                theme = ArenaTestSceneBuilder.LoadOrCreateConfig<IslandTheme>(path);
            }

            serialized = new SerializedObject(theme);
            if (created)
            {
                serialized.FindProperty("_plainFloorShare").floatValue = DefaultPlainFloorShare;
                serialized.FindProperty("_decorationDensity").floatValue = DefaultDecorationDensity;
            }

            return theme;
        }

        private static IslandTheme Save(IslandTheme theme, SerializedObject serialized, bool created)
        {
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(theme);
            if (created)
            {
                AssetDatabase.SaveAssets();
            }

            return theme;
        }

        private static void SetSprite(SerializedObject serialized, string property, string spritePath)
        {
            serialized.FindProperty(property).objectReferenceValue = spritePath != null ? Require(spritePath) : null;
        }

        private static void SetSprites(SerializedObject serialized, string property, List<Sprite> sprites)
        {
            SerializedProperty list = serialized.FindProperty(property);
            list.arraySize = sprites.Count;
            for (int i = 0; i < sprites.Count; i++)
            {
                list.GetArrayElementAtIndex(i).objectReferenceValue = sprites[i];
            }
        }

        // "<prefix>0.png", "<prefix>1.png", ... until one is missing.
        private static List<Sprite> Numbered(string prefix)
        {
            var sprites = new List<Sprite>();
            for (int i = 0; ; i++)
            {
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{prefix}{i}.png");
                if (sprite == null)
                {
                    if (i == 0)
                    {
                        throw new System.InvalidOperationException($"No sprites {prefix}0.png; run Tools/gen_pixelart.py");
                    }

                    return sprites;
                }

                sprites.Add(sprite);
            }
        }

        private static Sprite Require(string path)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
            {
                throw new System.InvalidOperationException($"Sprite not found at {path}; run Tools/gen_pixelart.py");
            }

            return sprite;
        }
    }
}
