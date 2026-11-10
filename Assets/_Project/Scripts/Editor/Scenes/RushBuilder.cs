using RushOps.Audio;
using RushOps.Bosses;
using RushOps.Enemies;
using RushOps.Rush;
using RushOps.Scoring;
using RushOps.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace RushOps.Editor.Scenes
{
    /// <summary>
    /// Adds the rush to an arena (issue #270): a <see cref="PlayerRush"/> on the player wired to the score, the results,
    /// all enemy pools and the player's <see cref="SfxPlayer"/>, the editor key, and the full-screen
    /// <see cref="RushOverlay"/> behind the HUD. Boss fights join through <see cref="AddBossFight"/>.
    /// </summary>
    internal static class RushBuilder
    {
        internal const string ConfigPath = "Assets/_Project/Data/Player/RushConfig.asset";

        // Width of the edge frame in reference pixels of the 360×640 canvas.
        private const float EdgeWidth = 6f;

        public static PlayerRush Create(Transform player, LevelResults results, EnemyPool[] enemies, LevelScore score,
            SfxPlayer sfx, RectTransform canvas)
        {
            RushConfig config = LoadConfig();
            var rush = player.gameObject.AddComponent<PlayerRush>();
            var serialized = new SerializedObject(rush);
            serialized.FindProperty("_config").objectReferenceValue = config;
            serialized.FindProperty("_score").objectReferenceValue = score;
            serialized.FindProperty("_results").objectReferenceValue = results;
            SerializedProperty pools = serialized.FindProperty("_pools");
            pools.arraySize = enemies.Length;
            for (int i = 0; i < enemies.Length; i++)
            {
                pools.GetArrayElementAtIndex(i).objectReferenceValue = enemies[i];
            }

            serialized.FindProperty("_sfx").objectReferenceValue = sfx;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            var key = player.gameObject.AddComponent<DebugRushKey>();
            var serializedKey = new SerializedObject(key);
            serializedKey.FindProperty("_rush").objectReferenceValue = rush;
            serializedKey.ApplyModifiedPropertiesWithoutUndo();

            // Full screen beyond the safe area like the nuke flash, but behind all HUD elements.
            RectTransform rect = ArenaTestSceneBuilder.CreateUiRect("RushOverlay", canvas, Vector2.zero, Vector2.one);
            rect.SetAsFirstSibling();
            Image tint = CreateImage(rect, config);
            var edges = new Image[4];
            edges[0] = CreateEdge(rect, config, "Top", new Vector2(0f, 1f), Vector2.one, new Vector2(0f, -EdgeWidth), Vector2.zero);
            edges[1] = CreateEdge(rect, config, "Bottom", Vector2.zero, new Vector2(1f, 0f), Vector2.zero, new Vector2(0f, EdgeWidth));
            edges[2] = CreateEdge(rect, config, "Left", Vector2.zero, new Vector2(0f, 1f), Vector2.zero, new Vector2(EdgeWidth, 0f));
            edges[3] = CreateEdge(rect, config, "Right", new Vector2(1f, 0f), Vector2.one, new Vector2(-EdgeWidth, 0f), Vector2.zero);
            var overlay = rect.gameObject.AddComponent<RushOverlay>();
            var serializedOverlay = new SerializedObject(overlay);
            serializedOverlay.FindProperty("_rush").objectReferenceValue = rush;
            serializedOverlay.FindProperty("_tint").objectReferenceValue = tint;
            SerializedProperty list = serializedOverlay.FindProperty("_edges");
            list.arraySize = edges.Length;
            for (int i = 0; i < edges.Length; i++)
            {
                list.GetArrayElementAtIndex(i).objectReferenceValue = edges[i];
            }

            serializedOverlay.ApplyModifiedPropertiesWithoutUndo();
            return rush;
        }

        private static Image CreateImage(RectTransform rect, RushConfig config)
        {
            var image = rect.gameObject.AddComponent<Image>();
            image.color = config.TintColor;
            image.raycastTarget = false;
            image.enabled = false;
            return image;
        }

        private static Image CreateEdge(RectTransform parent, RushConfig config, string name, Vector2 anchorMin,
            Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            RectTransform rect = ArenaTestSceneBuilder.CreateUiRect("Edge" + name, parent, anchorMin, anchorMax);
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            return CreateImage(rect, config);
        }

        /// <summary>Lets the rush of the open scene slow <paramref name="fight"/> down; nothing without a rush.</summary>
        public static void AddBossFight(BossFight fight)
        {
            var rush = Object.FindAnyObjectByType<PlayerRush>(FindObjectsInactive.Include);
            if (rush == null || fight == null)
            {
                return;
            }

            var serialized = new SerializedObject(rush);
            SerializedProperty fights = serialized.FindProperty("_bossFights");
            fights.arraySize++;
            fights.GetArrayElementAtIndex(fights.arraySize - 1).objectReferenceValue = fight;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        // The config keeps tuned values; only missing sounds are filled in.
        private static RushConfig LoadConfig()
        {
            RushConfig config = ArenaTestSceneBuilder.LoadOrCreateConfig<RushConfig>(ConfigPath);
            var serialized = new SerializedObject(config);
            SerializedProperty start = serialized.FindProperty("_startClip");
            SerializedProperty end = serialized.FindProperty("_endClip");
            if (start.objectReferenceValue == null)
            {
                start.objectReferenceValue = ArenaTestSceneBuilder.LoadClip("RushStart.wav");
            }

            if (end.objectReferenceValue == null)
            {
                end.objectReferenceValue = ArenaTestSceneBuilder.LoadClip("RushEnd.wav");
            }

            if (serialized.ApplyModifiedPropertiesWithoutUndo())
            {
                EditorUtility.SetDirty(config);
            }

            return config;
        }
    }
}
