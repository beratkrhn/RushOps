using System;
using RushOps.Arcade;
using RushOps.Editor.Scenes;
using UnityEditor;
using UnityEngine;

namespace RushOps.Editor.Arcade
{
    /// <summary>
    /// Creates or updates <c>Data/Arcade/ArcadeConfig.asset</c> (issue #321): copies the rectangles from
    /// <c>Art/Sprites/Arcade/Arcade_Layout.json</c> (written by <c>Tools/gen_arcade.py</c>) and links the sprites.
    /// Tuned values (screen resolution, HUD size, colors) survive; layout and sprites always follow the artwork.
    /// Batchmode: <c>-executeMethod RushOps.Editor.Arcade.ArcadeConfigBuilder.Build</c>
    /// </summary>
    public static class ArcadeConfigBuilder
    {
        /// <summary>Path of the config asset.</summary>
        public const string ConfigPath = "Assets/_Project/Data/Arcade/ArcadeConfig.asset";

        /// <summary>Folder of the arcade sprites.</summary>
        public const string SpritesDir = "Assets/_Project/Art/Sprites/Arcade";

        /// <summary>Layout written by the generator.</summary>
        public const string LayoutPath = SpritesDir + "/Arcade_Layout.json";

        private static readonly string[] StickNames = { "C", "E", "NE", "N", "NW", "W", "SW", "S", "SE" };

        [MenuItem("RushOps/Arcade/Arcade-Daten aktualisieren")]
        public static void Build()
        {
            Get();
        }

        /// <summary>The arcade config, created or updated first.</summary>
        public static ArcadeConfig Get()
        {
            var config = ArenaTestSceneBuilder.LoadOrCreateConfig<ArcadeConfig>(ConfigPath);
            var json = AssetDatabase.LoadAssetAtPath<TextAsset>(LayoutPath);
            if (json == null)
            {
                throw new InvalidOperationException($"{LayoutPath} not found; run Tools/gen_arcade.py");
            }

            LayoutFile layout = JsonUtility.FromJson<LayoutFile>(json.text);
            var deco = new RectInt[layout.decoButtons.Length];
            var decoSprites = new Sprite[layout.decoButtons.Length];
            for (int i = 0; i < deco.Length; i++)
            {
                deco[i] = layout.decoButtons[i].ToRect();
                decoSprites[i] = LoadSprite("Arcade_Button_" + layout.decoButtons[i].name);
            }

            config.SetLayout(new Vector2Int(layout.reference.width, layout.reference.height), layout.marquee.ToRect(),
                layout.bezel.ToRect(), layout.screen.ToRect(), layout.panel.ToRect(), layout.stickLeft.ToRect(),
                layout.stickRight.ToRect(), layout.dashButton.ToRect(), deco, layout.swapButton.ToRect());

            var sticks = new Sprite[StickNames.Length];
            for (int i = 0; i < sticks.Length; i++)
            {
                sticks[i] = LoadSprite("Arcade_Stick_" + StickNames[i]);
            }

            config.SetSprites(LoadSprite("Arcade_Marquee"), LoadSprite("Arcade_Bezel"), LoadSprite("Arcade_ScreenFrame"),
                LoadSprite("Arcade_ScreenGloss"), LoadSprite("Arcade_ControlPanel"), LoadSprite("Arcade_Body"), sticks,
                LoadSprite("Arcade_DashButton"), LoadSprite("Arcade_DashButton_Pressed"), decoSprites);
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
            return config;
        }

        private static Sprite LoadSprite(string name)
        {
            string path = $"{SpritesDir}/{name}.png";
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
            {
                throw new InvalidOperationException($"Sprite not found at {path}; run Tools/gen_arcade.py");
            }

            return sprite;
        }

        // Shape of Arcade_Layout.json, for JsonUtility.
#pragma warning disable 0649
        [Serializable]
        private class LayoutFile
        {
            public Size reference;
            public JsonRect marquee;
            public JsonRect bezel;
            public JsonRect screen;
            public JsonRect panel;
            public JsonRect stickLeft;
            public JsonRect stickRight;
            public JsonRect dashButton;
            public JsonRect[] decoButtons = Array.Empty<JsonRect>();
            public JsonRect swapButton;
        }

        [Serializable]
        private class Size
        {
            public int width;
            public int height;
        }

        [Serializable]
        private class JsonRect
        {
            public string name;
            public int x;
            public int y;
            public int width;
            public int height;

            public RectInt ToRect()
            {
                return new RectInt(x, y, width, height);
            }
        }
#pragma warning restore 0649
    }
}
