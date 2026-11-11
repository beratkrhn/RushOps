using RushOps.Endless;
using RushOps.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;

namespace RushOps.Editor.Scenes
{
    /// <summary>
    /// Builds the upgrade choice of the endless arena (issue #279, F-END-11): a dim panel with „Wähle ein Upgrade“ and
    /// three large cards (icon, name, level, description), below the HUD so the pause button stays on top, and the list
    /// of taken upgrades in the pause overlay.
    /// </summary>
    internal static class UpgradeChoiceBuilder
    {
        private const int CardCount = 3;
        private static readonly Vector2 CardSize = new Vector2(300f, 84f);
        private const float CardGap = 12f;
        private const float TextWidth = 216f;

        /// <summary>Adds the choice and the pause list for <paramref name="controller"/> to the HUD of the open scene.</summary>
        public static UpgradeChoiceView Create(RectTransform safeArea, EndlessRunController controller, PauseController pause,
            Font font)
        {
            var root = (RectTransform)safeArea.parent;
            RectTransform panel = ArenaTestSceneBuilder.CreateDimPanel("UpgradeChoicePanel", root);
            // Under the safe area: hearts, score and the pause button stay visible and tappable over the dim panel.
            panel.SetSiblingIndex(safeArea.GetSiblingIndex());
            float step = CardSize.y + CardGap;
            ArenaTestSceneBuilder.CreateText("Title", panel, font, EndlessText.ChooseUpgrade, ArenaTestSceneBuilder.HudPixelScale,
                new Vector2(0f, step * 1.5f + 18f));

            // The view stays active and switches the panel; on the panel itself it would stop listening once hidden.
            var view = ArenaTestSceneBuilder.CreateUiRect("UpgradeChoice", root, Vector2.zero, Vector2.zero)
                .gameObject.AddComponent<UpgradeChoiceView>();
            var cards = new UpgradeCardView[CardCount];
            for (int i = 0; i < CardCount; i++)
            {
                cards[i] = CreateCard(panel, view, font, new Vector2(0f, step * (1 - i)), i);
            }

            var serialized = new SerializedObject(view);
            serialized.FindProperty("_controller").objectReferenceValue = controller;
            serialized.FindProperty("_pause").objectReferenceValue = pause;
            serialized.FindProperty("_panel").objectReferenceValue = panel.gameObject;
            SerializedProperty list = serialized.FindProperty("_cards");
            list.arraySize = CardCount;
            for (int i = 0; i < CardCount; i++)
            {
                list.GetArrayElementAtIndex(i).objectReferenceValue = cards[i];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            panel.gameObject.SetActive(false);
            CreatePauseList(root, controller, font);
            return view;
        }

        private static UpgradeCardView CreateCard(RectTransform parent, UpgradeChoiceView view, Font font, Vector2 position,
            int index)
        {
            var center = new Vector2(0.5f, 0.5f);
            RectTransform card = ArenaTestSceneBuilder.CreateUiRect($"Card_{index}", parent, center, center);
            card.sizeDelta = CardSize;
            card.anchoredPosition = position;
            var image = card.gameObject.AddComponent<Image>();
            image.color = ArenaTestSceneBuilder.PaletteSlate2;
            var button = card.gameObject.AddComponent<Button>();
            button.targetGraphic = image;

            RectTransform iconRect = ArenaTestSceneBuilder.CreateUiRect("Icon", card, center, center);
            iconRect.sizeDelta = new Vector2(48f, 48f);
            iconRect.anchoredPosition = new Vector2(-CardSize.x / 2f + 12f + 24f, 0f);
            var icon = iconRect.gameObject.AddComponent<Image>();
            icon.raycastTarget = false;
            icon.preserveAspect = true;

            float textLeft = -CardSize.x / 2f + 72f;
            Text name = LeftText("Name", card, font, ArenaTestSceneBuilder.HudPixelScale, new Vector2(textLeft, 24f), 16f);
            Text level = LeftText("Level", card, font, 1, new Vector2(textLeft, 6f), 12f);
            level.color = ArenaTestSceneBuilder.PaletteYellow;
            Text description = LeftText("Description", card, font, 1, new Vector2(textLeft, -18f), 26f);
            description.horizontalOverflow = HorizontalWrapMode.Wrap;
            description.alignment = TextAnchor.UpperLeft;
            description.rectTransform.pivot = new Vector2(0f, 1f);
            description.rectTransform.anchoredPosition = new Vector2(textLeft, -4f);

            var cardView = card.gameObject.AddComponent<UpgradeCardView>();
            var serialized = new SerializedObject(cardView);
            serialized.FindProperty("_choice").objectReferenceValue = view;
            serialized.FindProperty("_icon").objectReferenceValue = icon;
            serialized.FindProperty("_name").objectReferenceValue = name;
            serialized.FindProperty("_level").objectReferenceValue = level;
            serialized.FindProperty("_description").objectReferenceValue = description;
            serialized.FindProperty("_button").objectReferenceValue = button;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            UnityEventTools.AddPersistentListener(button.onClick, cardView.Pick);
            return cardView;
        }

        // Left-aligned text starting at x of the card, a line of <paramref name="height"/> reference units.
        private static Text LeftText(string name, RectTransform card, Font font, int scale, Vector2 position, float height)
        {
            Text text = ArenaTestSceneBuilder.CreateText(name, card, font, string.Empty, scale, position);
            RectTransform rect = text.rectTransform;
            rect.pivot = new Vector2(0f, 0.5f);
            rect.sizeDelta = new Vector2(TextWidth, height) / scale;
            rect.anchoredPosition = position;
            text.alignment = TextAnchor.MiddleLeft;
            return text;
        }

        // Below the buttons of the pause overlay: „Upgrades“ and one line per upgrade taken.
        private static void CreatePauseList(RectTransform root, EndlessRunController controller, Font font)
        {
            Transform overlay = root.Find("PauseOverlay");
            if (overlay == null)
            {
                throw new System.InvalidOperationException("HUD has no PauseOverlay");
            }

            var parent = (RectTransform)overlay;
            Text title = ArenaTestSceneBuilder.CreateText("UpgradesTitle", parent, font, EndlessText.UpgradesTitle,
                ArenaTestSceneBuilder.HudPixelScale, new Vector2(0f, -140f));
            title.color = ArenaTestSceneBuilder.PaletteYellow;
            Text list = ArenaTestSceneBuilder.CreateText("UpgradeList", parent, font, EndlessText.NoUpgrades, 1,
                new Vector2(0f, -156f));
            list.rectTransform.pivot = new Vector2(0.5f, 1f);
            list.rectTransform.sizeDelta = new Vector2(TextWidth, 150f);
            list.alignment = TextAnchor.UpperCenter;

            var view = list.gameObject.AddComponent<UpgradeListView>();
            var serialized = new SerializedObject(view);
            serialized.FindProperty("_controller").objectReferenceValue = controller;
            serialized.FindProperty("_list").objectReferenceValue = list;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
