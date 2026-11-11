using RushOps.Editor.Art;
using RushOps.Player;
using RushOps.Save;
using RushOps.Scoring;
using RushOps.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace RushOps.Editor.Scenes
{
    /// <summary>
    /// Adds the level badges to a campaign level (issue #288, F-SCO-04, F-SCO-05): the <see cref="LevelBadges"/> tracker,
    /// the badge row on „Level geschafft!“ between stars and score, and the three conditions in the pause overlay below
    /// its buttons. Also fills the icons of the <see cref="BadgeConfig"/>.
    /// </summary>
    internal static class BadgeBuilder
    {
        internal const string ConfigPath = "Assets/_Project/Data/Score/BadgeConfig.asset";

        // Done screen: the row sits between the stars (y 75) and the score (y 25).
        private const float RowY = 48f;
        private const float RowSpacing = 22f;

        // Pause overlay: title and three lines below the buttons, like the upgrade list of the endless arena.
        private const float ListTitleY = -140f;
        private const float ListFirstY = -164f;
        private const float ListLineStep = 20f;
        private const float ListIconX = -112f;
        private const float ListTextX = -98f;
        private const float ListTextWidth = 220f;

        private static readonly string[] IconNames = { "Unharmed", "Blitz", "Combo" };

        /// <summary>The badge config with its icons set.</summary>
        public static BadgeConfig LoadConfig()
        {
            var config = ArenaTestSceneBuilder.LoadOrCreateConfig<BadgeConfig>(ConfigPath);
            var serialized = new SerializedObject(config);
            SerializedProperty earned = serialized.FindProperty("_earnedIcons");
            SerializedProperty missing = serialized.FindProperty("_missingIcons");
            earned.arraySize = IconNames.Length;
            missing.arraySize = IconNames.Length;
            for (int i = 0; i < IconNames.Length; i++)
            {
                earned.GetArrayElementAtIndex(i).objectReferenceValue = ArenaTestSceneBuilder.LoadUiSprite($"Badge_{IconNames[i]}_On.png");
                missing.GetArrayElementAtIndex(i).objectReferenceValue = ArenaTestSceneBuilder.LoadUiSprite($"Badge_{IconNames[i]}_Off.png");
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(config);
            return config;
        }

        /// <summary>Adds the badges to the open level scene of <paramref name="arena"/>.</summary>
        public static LevelBadges Create(ArenaScene arena)
        {
            // The font build reimports assets, which leaves references loaded earlier stale: build it first.
            Font font = PixelFontBuilder.Build();
            BadgeConfig config = LoadConfig();
            LevelResults results = arena.Results;
            var serializedResults = new SerializedObject(results);
            var score = (LevelScore)serializedResults.FindProperty("_score").objectReferenceValue;
            var done = (GameObject)serializedResults.FindProperty("_donePanel").objectReferenceValue;

            var badges = new GameObject("LevelBadges").AddComponent<LevelBadges>();
            var serialized = new SerializedObject(badges);
            serialized.FindProperty("_config").objectReferenceValue = config;
            serialized.FindProperty("_level").objectReferenceValue = results.Level;
            serialized.FindProperty("_player").objectReferenceValue = arena.Player.GetComponent<PlayerHealth>();
            serialized.FindProperty("_score").objectReferenceValue = score;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            LevelProgressRecorder recorder = Object.FindFirstObjectByType<LevelProgressRecorder>();
            var serializedRecorder = new SerializedObject(recorder);
            serializedRecorder.FindProperty("_badges").objectReferenceValue = badges;
            serializedRecorder.ApplyModifiedPropertiesWithoutUndo();

            CreateRow((RectTransform)done.transform, recorder, config);
            CreatePauseList((RectTransform)results.transform, badges, recorder, config, font);
            return badges;
        }

        private static void CreateRow(RectTransform done, LevelProgressRecorder recorder, BadgeConfig config)
        {
            var center = new Vector2(0.5f, 0.5f);
            RectTransform row = ArenaTestSceneBuilder.CreateUiRect("Badges", done, center, center);
            row.sizeDelta = Vector2.zero;
            row.anchoredPosition = new Vector2(0f, RowY);
            var icons = new Image[IconNames.Length];
            for (int i = 0; i < icons.Length; i++)
            {
                icons[i] = CreateIcon($"Badge_{IconNames[i]}", row, config, i, new Vector2((i - 1) * RowSpacing, 0f));
            }

            var view = row.gameObject.AddComponent<BadgeRowView>();
            var serialized = new SerializedObject(view);
            serialized.FindProperty("_recorder").objectReferenceValue = recorder;
            serialized.FindProperty("_config").objectReferenceValue = config;
            SetArray(serialized.FindProperty("_icons"), icons);
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreatePauseList(RectTransform root, LevelBadges badges, LevelProgressRecorder recorder,
            BadgeConfig config, Font font)
        {
            Transform overlay = root.Find("PauseOverlay");
            if (overlay == null)
            {
                throw new System.InvalidOperationException("HUD has no PauseOverlay");
            }

            var parent = (RectTransform)overlay;
            var center = new Vector2(0.5f, 0.5f);
            RectTransform list = ArenaTestSceneBuilder.CreateUiRect("Badges", parent, center, center);
            list.sizeDelta = Vector2.zero;
            Text title = ArenaTestSceneBuilder.CreateText("Title", list, font, "Abzeichen", ArenaTestSceneBuilder.HudPixelScale,
                new Vector2(0f, ListTitleY));
            title.color = ArenaTestSceneBuilder.PaletteYellow;
            var icons = new Image[IconNames.Length];
            var labels = new Text[IconNames.Length];
            for (int i = 0; i < icons.Length; i++)
            {
                float y = ListFirstY - i * ListLineStep;
                icons[i] = CreateIcon($"Icon_{IconNames[i]}", list, config, i, new Vector2(ListIconX, y));
                Text label = ArenaTestSceneBuilder.CreateText($"Label_{IconNames[i]}", list, font, string.Empty, 1,
                    new Vector2(ListTextX, y));
                label.rectTransform.pivot = new Vector2(0f, 0.5f);
                label.rectTransform.sizeDelta = new Vector2(ListTextWidth, 16f);
                label.rectTransform.anchoredPosition = new Vector2(ListTextX, y);
                label.alignment = TextAnchor.MiddleLeft;
                labels[i] = label;
            }

            var view = list.gameObject.AddComponent<BadgeListView>();
            var serialized = new SerializedObject(view);
            serialized.FindProperty("_badges").objectReferenceValue = badges;
            serialized.FindProperty("_recorder").objectReferenceValue = recorder;
            SetArray(serialized.FindProperty("_icons"), icons);
            SerializedProperty labelArray = serialized.FindProperty("_labels");
            labelArray.arraySize = labels.Length;
            for (int i = 0; i < labels.Length; i++)
            {
                labelArray.GetArrayElementAtIndex(i).objectReferenceValue = labels[i];
            }

            serialized.FindProperty("_possibleColor").colorValue = ArenaTestSceneBuilder.PaletteWhite;
            serialized.FindProperty("_lostColor").colorValue = ArenaTestSceneBuilder.PaletteSlate2;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Image CreateIcon(string name, RectTransform parent, BadgeConfig config, int index, Vector2 position)
        {
            var center = new Vector2(0.5f, 0.5f);
            RectTransform rect = ArenaTestSceneBuilder.CreateUiRect(name, parent, center, center);
            Sprite sprite = config.Icon(BadgeRules.Order[index], false);
            rect.sizeDelta = sprite != null ? sprite.rect.size : new Vector2(16f, 16f);
            rect.anchoredPosition = position;
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.raycastTarget = false;
            return image;
        }

        private static void SetArray(SerializedProperty property, Image[] images)
        {
            property.arraySize = images.Length;
            for (int i = 0; i < images.Length; i++)
            {
                property.GetArrayElementAtIndex(i).objectReferenceValue = images[i];
            }
        }
    }
}
