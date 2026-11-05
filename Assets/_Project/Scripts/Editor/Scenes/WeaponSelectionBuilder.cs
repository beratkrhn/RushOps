using RushOps.Combat;
using RushOps.Editor.Combat;
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
    /// Builds „Waffenwahl“ in the main menu (issue #176, decision W5) like the skin selection: title, the strip of weapon
    /// icons with arrows, name, unlock condition, position, „Auswählen“ and „Zurück“, wired to the
    /// <see cref="WeaponCarousel"/>. The icons are copied from a template at runtime, one per weapon of the catalog.
    /// </summary>
    internal static class WeaponSelectionBuilder
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
        private const int IconScale = 3;
        private static readonly Vector2 ArrowTouchArea = new Vector2(48f, 96f);

        public static void Create(RectTransform parent, Font font, WeaponCarousel carousel)
        {
            RectTransform panel = ArenaTestSceneBuilder.CreateUiRect("WeaponsPanel", parent, Vector2.zero, Vector2.one);
            var background = panel.gameObject.AddComponent<Image>();
            Color color = ArenaTestSceneBuilder.PaletteNight;
            color.a = 0.97f;
            background.color = color;

            ArenaTestSceneBuilder.CreateText("Title", panel, font, "WAFFENWAHL", ArenaTestSceneBuilder.TitlePixelScale,
                new Vector2(0f, TitleY));

            var center = new Vector2(0.5f, 0.5f);
            RectTransform strip = ArenaTestSceneBuilder.CreateUiRect("Strip", panel, center, center);
            strip.sizeDelta = Vector2.zero;
            strip.anchoredPosition = new Vector2(0f, StripY);
            Sprite pistol = ArenaTestSceneBuilder.LoadUiSprite("Weapon_Pistol.png");
            Image template = MainMenuSceneBuilder.CreateSprite("WeaponTemplate", strip, pistol, Vector2.zero);
            template.rectTransform.sizeDelta = pistol.rect.size * IconScale;
            template.gameObject.SetActive(false);

            Button previous = CreateArrow("PreviousButton", panel, new Vector2(-ArrowX, StripY), 90f, carousel.Previous);
            Button next = CreateArrow("NextButton", panel, new Vector2(ArrowX, StripY), -90f, carousel.Next);

            Text name = ArenaTestSceneBuilder.CreateText("Name", panel, font, "Pistole", ArenaTestSceneBuilder.HudPixelScale,
                new Vector2(0f, NameY));
            name.color = ArenaTestSceneBuilder.PaletteYellow;
            Text hint = ArenaTestSceneBuilder.CreateText("Hint", panel, font, string.Empty, ArenaTestSceneBuilder.HudPixelScale,
                new Vector2(0f, HintY));
            Text counter = ArenaTestSceneBuilder.CreateText("Counter", panel, font, SkinHint.Counter(0, 1), 1,
                new Vector2(0f, CounterY));
            counter.color = ArenaTestSceneBuilder.PaletteSlate2;
            Button select = MainMenuSceneBuilder.CreateButton("SelectButton", panel, font, SkinHint.Choose,
                new Vector2(0f, SelectY), false, true, carousel.Select);
            MainMenuSceneBuilder.CreateButton("BackButton", panel, font, "Zurück", new Vector2(0f, BackY), true, true,
                carousel.Close);

            var serialized = new SerializedObject(carousel);
            serialized.FindProperty("_save").objectReferenceValue =
                ArenaTestSceneBuilder.LoadOrCreateConfig<SaveGameService>(ArenaTestSceneBuilder.SaveGameServicePath);
            serialized.FindProperty("_catalog").objectReferenceValue = WeaponCatalogBuilder.Catalog();
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
            serialized.ApplyModifiedPropertiesWithoutUndo();

            panel.gameObject.SetActive(false);
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
