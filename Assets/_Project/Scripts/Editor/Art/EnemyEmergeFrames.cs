using UnityEditor;
using UnityEngine;
using RushOps.Enemies;

namespace RushOps.Editor.Art
{
    /// <summary>
    /// Assigns the crawl-out frames <c>Sprites/Enemies/&lt;Type&gt;_Emerge_&lt;F&gt;.png</c> (made by
    /// <c>Tools/gen_pixelart.py</c>) to the enemy definitions (issue #314, F-END-15).
    /// </summary>
    public static class EnemyEmergeFrames
    {
        private const string SpritesDir = "Assets/_Project/Art/Sprites/Enemies";
        private const string DataDir = "Assets/_Project/Data/Enemies";
        private const int FrameCount = 6;

        private static readonly string[] Types = { "Walker", "Runner", "Brute", "Boomer", "Dog" };

        /// <summary>Sets the crawl-out frames on Walker, Runner, Brute, Boomer and Dog.</summary>
        [MenuItem("RushOps/Gegner/Kriech-Frames zuweisen")]
        public static void Assign()
        {
            AssetDatabase.Refresh();
            foreach (string type in Types)
            {
                var definition = AssetDatabase.LoadAssetAtPath<EnemyDefinition>($"{DataDir}/{type}.asset");
                if (definition == null)
                {
                    throw new System.InvalidOperationException($"No enemy definition at {DataDir}/{type}.asset");
                }

                var frames = new Sprite[FrameCount];
                for (int i = 0; i < FrameCount; i++)
                {
                    string path = $"{SpritesDir}/{type}_Emerge_{i}.png";
                    frames[i] = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                    if (frames[i] == null)
                    {
                        throw new System.InvalidOperationException($"Sprite not found at {path}; run Tools/gen_pixelart.py");
                    }
                }

                var serialized = new SerializedObject(definition);
                SerializedProperty list = serialized.FindProperty("_emergeFrames");
                list.arraySize = FrameCount;
                for (int i = 0; i < FrameCount; i++)
                {
                    list.GetArrayElementAtIndex(i).objectReferenceValue = frames[i];
                }

                serialized.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(definition);
            }

            AssetDatabase.SaveAssets();
        }
    }
}
