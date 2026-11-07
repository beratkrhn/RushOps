using RushOps.Characters;
using RushOps.Editor.Art;
using RushOps.Editor.Skins;
using RushOps.Save;
using RushOps.Skins;
using RushOps.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;

namespace RushOps.Editor.Scenes
{
    /// <summary>
    /// Builds „Spielerauswahl“ in the main menu (issue #130, F-UI-03) and wires it to the <see cref="SkinCarousel"/>:
    /// title, the strip of walking figures with arrows, name, unlock hint, position, „Auswählen“ and „Zurück“.
    /// The figures are copied from a template at runtime, one per skin of the catalog.
    /// </summary>
    internal static class SkinSelectionBuilder
    {
        private const string ConfigPath = "Assets/_Project/Data/UI/SkinCarouselConfig.asset";
        private const float TitleY = 220f;
        private const float StripY = 60f;
        private const float ArrowX = 150f;
        private const float NameY = -40f;
        private const float HintY = -70f;
        private const float CounterY = -95f;
        private const float SelectY = -140f;
        private const float BackY = -200f;
        private const float ConfirmTextWidth = 150f;
        private static readonly Vector2 ArrowTouchArea = new Vector2(48f, 96f);
        private static readonly Color PaletteYellow = new Color32(246, 208, 77, 255);

        public static void Create(RectTransform parent, Font font, SkinCarousel carousel, MenuFigure menuFigure,
            GoldBalanceView menuBalance)
        {
            RectTransform panel = ArenaTestSceneBuilder.CreateUiRect("SkinsPanel", parent, Vector2.zero, Vector2.one);
            var background = panel.gameObject.AddComponent<Image>();
            Color color = ArenaTestSceneBuilder.PaletteNight;
            color.a = 0.97f;
            background.color = color;

            ArenaTestSceneBuilder.CreateText("Title", panel, font, "SPIELERAUSWAHL", ArenaTestSceneBuilder.TitlePixelScale,
                new Vector2(0f, TitleY));

            var center = new Vector2(0.5f, 0.5f);
            RectTransform strip = ArenaTestSceneBuilder.CreateUiRect("Strip", panel, center, center);
            strip.sizeDelta = Vector2.zero;
            strip.anchoredPosition = new Vector2(0f, StripY);
            MenuFigure template = CreateTemplate(strip);

            Button previous = CreateArrow("PreviousButton", panel, new Vector2(-ArrowX, StripY), 90f, carousel.Previous);
            Button next = CreateArrow("NextButton", panel, new Vector2(ArrowX, StripY), -90f, carousel.Next);

            Text name = ArenaTestSceneBuilder.CreateText("Name", panel, font, "Standard", ArenaTestSceneBuilder.HudPixelScale,
                new Vector2(0f, NameY));
            name.color = PaletteYellow;
            Text hint = ArenaTestSceneBuilder.CreateText("Hint", panel, font, string.Empty, ArenaTestSceneBuilder.HudPixelScale,
                new Vector2(0f, HintY));
            Text counter = ArenaTestSceneBuilder.CreateText("Counter", panel, font, SkinHint.Counter(0, 1), 1,
                new Vector2(0f, CounterY));
            counter.color = ArenaTestSceneBuilder.PaletteSlate2;
            Button select = MainMenuSceneBuilder.CreateButton("SelectButton", panel, font, SkinHint.Choose,
                new Vector2(0f, SelectY), false, true, carousel.Select);
            Button buy = MainMenuSceneBuilder.CreateButton("BuyButton", panel, font, SkinHint.Buy,
                new Vector2(0f, SelectY), true, true, carousel.Buy);
            buy.gameObject.SetActive(false);
            GoldBalanceView balance = MainMenuSceneBuilder.CreateBalance(panel, font, "SkinsBalance");
            MainMenuSceneBuilder.CreateButton("BackButton", panel, font, "Zurück", new Vector2(0f, BackY), true, true,
                carousel.Close);

            RectTransform confirm = CreateConfirm(panel, font, carousel, out Text confirmText);

            var serialized = new SerializedObject(carousel);
            serialized.FindProperty("_buy").objectReferenceValue = buy;
            serialized.FindProperty("_buyLabel").objectReferenceValue = buy.GetComponentInChildren<Text>();
            serialized.FindProperty("_confirmPanel").objectReferenceValue = confirm.gameObject;
            serialized.FindProperty("_confirmText").objectReferenceValue = confirmText;
            SerializedProperty views = serialized.FindProperty("_balanceViews");
            views.arraySize = 2;
            views.GetArrayElementAtIndex(0).objectReferenceValue = menuBalance;
            views.GetArrayElementAtIndex(1).objectReferenceValue = balance;
            serialized.FindProperty("_save").objectReferenceValue =
                ArenaTestSceneBuilder.LoadOrCreateConfig<SaveGameService>(ArenaTestSceneBuilder.SaveGameServicePath);
            serialized.FindProperty("_catalog").objectReferenceValue = SkinCatalogBuilder.Build();
            serialized.FindProperty("_config").objectReferenceValue =
                ArenaTestSceneBuilder.LoadOrCreateConfig<SkinCarouselConfig>(ConfigPath);
            serialized.FindProperty("_panel").objectReferenceValue = panel.gameObject;
            serialized.FindProperty("_strip").objectReferenceValue = strip;
            serialized.FindProperty("_itemTemplate").objectReferenceValue = template;
            serialized.FindProperty("_name").objectReferenceValue = name;
            serialized.FindProperty("_hint").objectReferenceValue = hint;
            serialized.FindProperty("_counter").objectReferenceValue = counter;
            serialized.FindProperty("_select").objectReferenceValue = select;
            serialized.FindProperty("_selectLabel").objectReferenceValue = select.GetComponentInChildren<Text>();
            serialized.FindProperty("_previous").objectReferenceValue = previous;
            serialized.FindProperty("_next").objectReferenceValue = next;
            serialized.FindProperty("_menuFigure").objectReferenceValue = menuFigure;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            panel.gameObject.SetActive(false);
        }

        // Dimmed overlay with the purchase question and „Kaufen“ / „Abbrechen“ (issue #237).
        private static RectTransform CreateConfirm(RectTransform panel, Font font, SkinCarousel carousel, out Text question)
        {
            RectTransform overlay = ArenaTestSceneBuilder.CreateUiRect("ConfirmPanel", panel, Vector2.zero, Vector2.one);
            var dim = overlay.gameObject.AddComponent<Image>();
            Color color = ArenaTestSceneBuilder.PaletteNight;
            color.a = 0.92f;
            dim.color = color;

            question = ArenaTestSceneBuilder.CreateText("Question", overlay, font, "Skin für 0 Gold kaufen?",
                ArenaTestSceneBuilder.HudPixelScale, new Vector2(0f, 40f));
            // Wraps inside the portrait width instead of running off the screen.
            question.horizontalOverflow = HorizontalWrapMode.Wrap;
            question.rectTransform.sizeDelta = new Vector2(ConfirmTextWidth, 60f);
            question.color = PaletteYellow;

            MainMenuSceneBuilder.CreateButton("ConfirmYesButton", overlay, font, SkinHint.Buy, new Vector2(0f, -30f), true, true,
                carousel.ConfirmBuy);
            MainMenuSceneBuilder.CreateButton("ConfirmNoButton", overlay, font, "Abbrechen", new Vector2(0f, -90f), false, true,
                carousel.CancelBuy);
            overlay.gameObject.SetActive(false);
            return overlay;
        }

        // Inactive figure the carousel copies for every skin.
        private static MenuFigure CreateTemplate(RectTransform strip)
        {
            var idle = AssetDatabase.LoadAssetAtPath<DirectionalAnimation>(DirectionalAnimationBuilder.PlayerIdlePath);
            Image image = MainMenuSceneBuilder.CreateSprite("SkinTemplate", strip, idle.GetFrame(Direction8.S, 0), Vector2.zero);
            var figure = image.gameObject.AddComponent<MenuFigure>();
            var serialized = new SerializedObject(figure);
            serialized.FindProperty("_animation").objectReferenceValue = idle;
            serialized.FindProperty("_direction").enumValueIndex = (int)Direction8.S;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            image.gameObject.SetActive(false);
            return figure;
        }

        // The map arrow turned sideways, inside a larger touch area; it fades out at either end of the list.
        private static Button CreateArrow(string name, RectTransform parent, Vector2 position, float degrees,
            UnityEngine.Events.UnityAction action)
        {
            var center = new Vector2(0.5f, 0.5f);
            RectTransform area = ArenaTestSceneBuilder.CreateUiRect(name, parent, center, center);
            area.sizeDelta = ArrowTouchArea;
            area.anchoredPosition = position;
            var hit = area.gameObject.AddComponent<Image>();
            hit.color = Color.clear;

            Image arrow = MainMenuSceneBuilder.CreateSprite("Arrow", area, ArenaTestSceneBuilder.LoadUiSprite("Map_Arrow.png"), Vector2.zero);
            arrow.rectTransform.localRotation = Quaternion.Euler(0f, 0f, degrees);

            var button = area.gameObject.AddComponent<Button>();
            button.targetGraphic = arrow;
            ColorBlock colors = button.colors;
            colors.disabledColor = new Color(1f, 1f, 1f, 0.2f);
            button.colors = colors;
            UnityEventTools.AddPersistentListener(button.onClick, action);
            return button;
        }
    }
}
