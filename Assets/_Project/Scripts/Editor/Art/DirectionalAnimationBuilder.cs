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

        /// <summary>Asset path of the walker's hit reaction (issue #100).</summary>
        public const string WalkerHitPath = AnimationsDir + "/Walker_Hit.asset";

        /// <summary>Asset path of the walker's death (issue #100).</summary>
        public const string WalkerDeathPath = AnimationsDir + "/Walker_Death.asset";

        /// <summary>Asset path of the runner's hit reaction (issue #100).</summary>
        public const string RunnerHitPath = AnimationsDir + "/Runner_Hit.asset";

        /// <summary>Asset path of the runner's death (issue #100).</summary>
        public const string RunnerDeathPath = AnimationsDir + "/Runner_Death.asset";

        /// <summary>Asset path of the brute's hit reaction (issue #101).</summary>
        public const string BruteHitPath = AnimationsDir + "/Brute_Hit.asset";

        /// <summary>Asset path of the brute's death (issue #101).</summary>
        public const string BruteDeathPath = AnimationsDir + "/Brute_Death.asset";

        /// <summary>Asset path of the boomer's hit reaction (issue #101).</summary>
        public const string BoomerHitPath = AnimationsDir + "/Boomer_Hit.asset";

        /// <summary>Asset path of the boomer's swelling near the player (issue #101).</summary>
        public const string BoomerSwellPath = AnimationsDir + "/Boomer_Swell.asset";

        /// <summary>Asset path of the boomer's burst (issue #101).</summary>
        public const string BoomerDeathPath = AnimationsDir + "/Boomer_Death.asset";

        /// <summary>Asset path of the runner's run animation.</summary>
        public const string RunnerRunPath = AnimationsDir + "/Runner_Run.asset";

        /// <summary>Asset path of the brute's walk animation.</summary>
        public const string BruteWalkPath = AnimationsDir + "/Brute_Walk.asset";

        /// <summary>Asset path of the boomer's walk animation.</summary>
        public const string BoomerWalkPath = AnimationsDir + "/Boomer_Walk.asset";

        private const string AnimationsDir = "Assets/_Project/Data/Animations";

        // Start values for new hit and death assets; tuned values in existing assets are kept.
        private const float HitFramesPerSecond = 12f;
        private const float DeathFramesPerSecond = 10f;
        private const float SwellFramesPerSecond = 12f;
        private const string SpritesDir = "Assets/_Project/Art/Sprites";

        [MenuItem("RushOps/Art/Richtungs-Animationen aktualisieren")]
        public static void BuildAll()
        {
            CreateOrUpdate(PlayerIdlePath, SpritesDir + "/Player", "Player_Idle");
            CreateOrUpdate(WalkerWalkPath, SpritesDir + "/Enemies", "Walker_Walk");
            CreateOrUpdate(WalkerHitPath, SpritesDir + "/Enemies", "Walker_Hit", false, HitFramesPerSecond);
            CreateOrUpdate(WalkerDeathPath, SpritesDir + "/Enemies", "Walker_Death", false, DeathFramesPerSecond);
            CreateOrUpdate(RunnerRunPath, SpritesDir + "/Enemies", "Runner_Run");
            CreateOrUpdate(RunnerHitPath, SpritesDir + "/Enemies", "Runner_Hit", false, HitFramesPerSecond);
            CreateOrUpdate(RunnerDeathPath, SpritesDir + "/Enemies", "Runner_Death", false, DeathFramesPerSecond);
            CreateOrUpdate(BruteWalkPath, SpritesDir + "/Enemies", "Brute_Walk");
            CreateOrUpdate(BruteHitPath, SpritesDir + "/Enemies", "Brute_Hit", false, HitFramesPerSecond);
            CreateOrUpdate(BruteDeathPath, SpritesDir + "/Enemies", "Brute_Death", false, DeathFramesPerSecond);
            CreateOrUpdate(BoomerWalkPath, SpritesDir + "/Enemies", "Boomer_Walk");
            CreateOrUpdate(BoomerHitPath, SpritesDir + "/Enemies", "Boomer_Hit", false, HitFramesPerSecond);
            CreateOrUpdate(BoomerSwellPath, SpritesDir + "/Enemies", "Boomer_Swell", true, SwellFramesPerSecond);
            CreateOrUpdate(BoomerDeathPath, SpritesDir + "/Enemies", "Boomer_Death", false, DeathFramesPerSecond);
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// Creates or updates the animation at <paramref name="assetPath"/> with the sprites
        /// <c>&lt;folder&gt;/&lt;prefix&gt;_&lt;DIR&gt;[_&lt;Frame&gt;].png</c> for all 8 directions.
        /// <paramref name="loops"/> is always applied; <paramref name="framesPerSecond"/> only to a new asset
        /// (a negative value keeps the asset default).
        /// </summary>
        public static DirectionalAnimation CreateOrUpdate(
            string assetPath, string folder, string prefix, bool loops = true, float framesPerSecond = -1f)
        {
            var animation = AssetDatabase.LoadAssetAtPath<DirectionalAnimation>(assetPath);
            bool created = animation == null;
            if (created)
            {
                EnsureFolder(Path.GetDirectoryName(assetPath).Replace('\\', '/'));
                animation = ScriptableObject.CreateInstance<DirectionalAnimation>();
                AssetDatabase.CreateAsset(animation, assetPath);
            }

            var serialized = new SerializedObject(animation);
            serialized.FindProperty("_loops").boolValue = loops;
            if (created && framesPerSecond >= 0f)
            {
                serialized.FindProperty("_framesPerSecond").floatValue = framesPerSecond;
            }

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
