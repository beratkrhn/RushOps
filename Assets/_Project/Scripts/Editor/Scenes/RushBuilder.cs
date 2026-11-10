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

        private const string SpriteDir = "Assets/_Project/Art/Sprites/UI";

        // Size of the rush button sprite and of its touch area in reference pixels; the perk slots are 28 px tall.
        private const float ButtonSize = 32f;
        private const float ButtonHitSize = 44f;
        private const float PerkSlotSize = 28f;

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
            CreateButton((RectTransform)canvas.Find("SafeArea"), rush, sfx);
            return rush;
        }

        // Round button below the row of active perks (F-HUD-09, N18): a transparent touch area of ButtonHitSize around
        // the 32 px disc, so it is easy to hit. It does not move with the perks.
        private static void CreateButton(RectTransform safeArea, PlayerRush rush, SfxPlayer sfx)
        {
            var perks = (RectTransform)safeArea.Find("ActivePerks");
            float perksTop = perks != null ? -perks.anchoredPosition.y : 32f;
            float top = perksTop + PerkSlotSize + ArenaTestSceneBuilder.PerkRowGap;
            float inset = (ButtonHitSize - ButtonSize) / 2f;
            var topLeft = new Vector2(0f, 1f);

            RectTransform hit = ArenaTestSceneBuilder.CreateUiRect("RushButton", safeArea, topLeft, topLeft);
            hit.pivot = topLeft;
            hit.sizeDelta = new Vector2(ButtonHitSize, ButtonHitSize);
            hit.anchoredPosition = new Vector2(ArenaTestSceneBuilder.HudMargin - inset, -(top - inset));
            var hitImage = hit.gameObject.AddComponent<Image>();
            hitImage.color = new Color(1f, 1f, 1f, 0f);
            var button = hit.gameObject.AddComponent<Button>();
            button.transition = Selectable.Transition.None;
            button.targetGraphic = hitImage;

            var center = new Vector2(0.5f, 0.5f);
            RectTransform visual = ArenaTestSceneBuilder.CreateUiRect("Visual", hit, center, center);
            visual.sizeDelta = new Vector2(ButtonSize, ButtonSize);
            Sprite empty = LoadSprite("Rush_Button.png");
            var background = visual.gameObject.AddComponent<Image>();
            background.sprite = empty;
            background.raycastTarget = false;
            RectTransform ringRect = ArenaTestSceneBuilder.CreateUiRect("Ring", visual, Vector2.zero, Vector2.one);
            var ring = ringRect.gameObject.AddComponent<Image>();
            Sprite fillRing = LoadSprite("Rush_Ring.png");
            ring.sprite = fillRing;
            ring.raycastTarget = false;
            ring.type = Image.Type.Filled;
            ring.fillMethod = Image.FillMethod.Radial360;
            ring.fillOrigin = (int)Image.Origin360.Top;
            ring.fillClockwise = true;
            ring.fillAmount = 0f;

            var view = hit.gameObject.AddComponent<RushButtonView>();
            UnityEditor.Events.UnityEventTools.AddPersistentListener(button.onClick, view.Tap);
            var click = sfx != null ? sfx.GetComponent<UiClickSound>() : null;
            if (click != null)
            {
                UnityEditor.Events.UnityEventTools.AddPersistentListener(button.onClick, click.Play);
            }
            var serialized = new SerializedObject(view);
            serialized.FindProperty("_rush").objectReferenceValue = rush;
            serialized.FindProperty("_background").objectReferenceValue = background;
            serialized.FindProperty("_ring").objectReferenceValue = ring;
            serialized.FindProperty("_visual").objectReferenceValue = visual;
            serialized.FindProperty("_emptySprite").objectReferenceValue = empty;
            serialized.FindProperty("_fullSprite").objectReferenceValue = LoadSprite("Rush_Button_Full.png");
            serialized.FindProperty("_fillRing").objectReferenceValue = fillRing;
            serialized.FindProperty("_activeRing").objectReferenceValue = LoadSprite("Rush_Ring_Active.png");
            serialized.FindProperty("_sfx").objectReferenceValue = sfx;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Sprite LoadSprite(string file)
        {
            string path = $"{SpriteDir}/{file}";
            return AssetDatabase.LoadAssetAtPath<Sprite>(path)
                ?? throw new System.InvalidOperationException($"Sprite not found at {path}; run Tools/gen_pixelart.py");
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

            SerializedProperty full = serialized.FindProperty("_fullClip");
            if (full.objectReferenceValue == null)
            {
                full.objectReferenceValue = ArenaTestSceneBuilder.LoadClip("RushFull.wav");
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
