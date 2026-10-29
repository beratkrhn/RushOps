using System.Collections.Generic;
using System.IO;
using RushOps.Characters;
using UnityEditor;
using UnityEngine;

namespace RushOps.Editor.Art
{
    /// <summary>
    /// Fills <see cref="DirectionalAnimation"/> assets from sprites named after ADR 0006:
    /// <c>&lt;Prefix&gt;_&lt;DIR&gt;.png</c> (one pose) or <c>&lt;Prefix&gt;_&lt;DIR&gt;_&lt;Frame&gt;.png</c>.
    /// Existing assets keep their tuned frame rate; only the sprite references are replaced.
    /// </summary>
    public static class DirectionalAnimationBuilder
    {
        /// <summary>Asset path of the player's idle animation.</summary>
        public const string PlayerIdlePath = AnimationsDir + "/Player_Idle.asset";

        /// <summary>Asset path of the walker's walk animation.</summary>
        public const string WalkerWalkPath = AnimationsDir + "/Walker_Walk.asset";

        private const string AnimationsDir = "Assets/_Project/Data/Animations";
        private const string SpritesDir = "Assets/_Project/Art/Sprites";

        [MenuItem("RushOps/Art/Richtungs-Animationen aktualisieren")]
        public static void BuildAll()
        {
            CreateOrUpdate(PlayerIdlePath, SpritesDir + "/Player", "Player_Idle");
            CreateOrUpdate(WalkerWalkPath, SpritesDir + "/Enemies", "Walker_Walk");
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// Creates or updates the animation at <paramref name="assetPath"/> with the sprites
        /// <c>&lt;folder&gt;/&lt;prefix&gt;_&lt;DIR&gt;[_&lt;Frame&gt;].png</c> for all 8 directions.
        /// </summary>
        public static DirectionalAnimation CreateOrUpdate(string assetPath, string folder, string prefix)
        {
            var animation = AssetDatabase.LoadAssetAtPath<DirectionalAnimation>(assetPath);
            if (animation == null)
            {
                EnsureFolder(Path.GetDirectoryName(assetPath).Replace('\\', '/'));
                animation = ScriptableObject.CreateInstance<DirectionalAnimation>();
                AssetDatabase.CreateAsset(animation, assetPath);
            }

            var serialized = new SerializedObject(animation);
            SerializedProperty directions = serialized.FindProperty("_directions");
            directions.arraySize = Direction8Math.Count;
            for (int d = 0; d < Direction8Math.Count; d++)
            {
                List<Sprite> frames = FindFrames(folder, $"{prefix}_{(Direction8)d}");
                if (frames.Count == 0)
                {
                    Debug.LogWarning($"[DirectionalAnimationBuilder] No sprites for {prefix}_{(Direction8)d} in {folder}");
                }

                SerializedProperty list = directions.GetArrayElementAtIndex(d).FindPropertyRelative("_frames");
                list.arraySize = frames.Count;
                for (int i = 0; i < frames.Count; i++)
                {
                    list.GetArrayElementAtIndex(i).objectReferenceValue = frames[i];
                }
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(animation);
            return animation;
        }

        // A single pose "<stem>.png" or numbered frames "<stem>_0.png", "<stem>_1.png", ...
        private static List<Sprite> FindFrames(string folder, string stem)
        {
            var frames = new List<Sprite>();
            var single = AssetDatabase.LoadAssetAtPath<Sprite>($"{folder}/{stem}.png");
            if (single != null)
            {
                frames.Add(single);
                return frames;
            }

            for (int i = 0; ; i++)
            {
                var frame = AssetDatabase.LoadAssetAtPath<Sprite>($"{folder}/{stem}_{i}.png");
                if (frame == null)
                {
                    return frames;
                }

                frames.Add(frame);
            }
        }

        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder))
            {
                return;
            }

            string parent = Path.GetDirectoryName(folder).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(folder));
        }
    }
}
