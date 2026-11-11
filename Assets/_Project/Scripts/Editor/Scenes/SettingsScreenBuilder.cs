using RushOps.Audio;
using RushOps.Combat;
using RushOps.Save;
using RushOps.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;

namespace RushOps.Editor.Scenes
{
    /// <summary>
    /// Builds the settings screen and the credits of the main menu (issue #126, F-UI-04) and wires them to the
    /// <see cref="SettingsMenu"/>: three volume sliders, aim assist, credits and „Zurück“. No stick settings
    /// (design question M3).
    /// </summary>
    internal static class SettingsScreenBuilder
    {
        private const string CreditsConfigPath = "Assets/_Project/Data/UI/CreditsConfig.asset";
        private const float TitleY = 220f;
        private const float FirstSliderY = 150f;
        private const float SliderStep = 60f;
        private const float SliderLabelGap = 22f;
        private static readonly Vector2 SliderSize = new Vector2(200f, 28f);
        private const float TrackHeight = 8f;
        private static readonly Color PaletteSlate0 = new Color32(46, 44, 61, 255);
        private static readonly Color PaletteOrange = new Color32(232, 134, 47, 255);

        public static void Create(RectTransform parent, Font font, SettingsMenu menu, AudioVolumes volumes)
        {
            RectTransform settingsPanel = CreatePanel("SettingsPanel", parent, 0.94f);
            ArenaTestSceneBuilder.CreateText("Title", settingsPanel, font, "EINSTELLUNGEN", ArenaTestSceneBuilder.TitlePixelScale,
                new Vector2(0f, TitleY));
            Slider master = CreateVolumeSlider("Master", settingsPanel, font, "Gesamt", 0, menu.SetMasterVolume);
            Slider music = CreateVolumeSlider("Music", settingsPanel, font, "Musik", 1, menu.SetMusicVolume);
            Slider effects = CreateVolumeSlider("Effects", settingsPanel, font, "Effekte", 2, menu.SetEffectsVolume);
            Button aim = MainMenuSceneBuilder.CreateButton("AimAssistButton", settingsPanel, font, SettingsMenu.AimAssistText(true),
                new Vector2(0f, -40f), false, true, menu.ToggleAimAssist);
            Button vibration = MainMenuSceneBuilder.CreateButton("VibrationButton", settingsPanel, font,
                SettingsMenu.VibrationText(true), new Vector2(0f, -95f), false, true, menu.ToggleVibration);
            MainMenuSceneBuilder.CreateButton("CreditsButton", settingsPanel, font, "Credits", new Vector2(0f, -150f), false, true,
                menu.OpenCredits);
            MainMenuSceneBuilder.CreateButton("BackButton", settingsPanel, font, "Zurück", new Vector2(0f, -200f), true, true,
                menu.Close);

            RectTransform creditsPanel = CreatePanel("CreditsPanel", parent, 1f);
            ArenaTestSceneBuilder.CreateText("Title", creditsPanel, font, "CREDITS", ArenaTestSceneBuilder.TitlePixelScale,
                new Vector2(0f, TitleY));
            Text credits = ArenaTestSceneBuilder.CreateText("Lines", creditsPanel, font, string.Empty,
                ArenaTestSceneBuilder.HudPixelScale, new Vector2(0f, 40f));
            MainMenuSceneBuilder.CreateButton("BackButton", creditsPanel, font, "Zurück", new Vector2(0f, -200f), true, true,
                menu.CloseCredits);

            var serialized = new SerializedObject(menu);
            serialized.FindProperty("_save").objectReferenceValue =
                ArenaTestSceneBuilder.LoadOrCreateConfig<SaveGameService>(ArenaTestSceneBuilder.SaveGameServicePath);
            serialized.FindProperty("_volumes").objectReferenceValue = volumes;
            serialized.FindProperty("_audioConfig").objectReferenceValue =
                ArenaTestSceneBuilder.LoadOrCreateConfig<AudioConfig>(ArenaTestSceneBuilder.AudioConfigPath);
            serialized.FindProperty("_weaponConfig").objectReferenceValue =
                ArenaTestSceneBuilder.LoadOrCreateConfig<WeaponConfig>(ArenaTestSceneBuilder.WeaponConfigPath);
            serialized.FindProperty("_credits").objectReferenceValue =
                ArenaTestSceneBuilder.LoadOrCreateConfig<CreditsConfig>(CreditsConfigPath);
            serialized.FindProperty("_settingsPanel").objectReferenceValue = settingsPanel.gameObject;
            serialized.FindProperty("_creditsPanel").objectReferenceValue = creditsPanel.gameObject;
            serialized.FindProperty("_master").objectReferenceValue = master;
            serialized.FindProperty("_music").objectReferenceValue = music;
            serialized.FindProperty("_effects").objectReferenceValue = effects;
            serialized.FindProperty("_aimAssistLabel").objectReferenceValue = aim.GetComponentInChildren<Text>();
            serialized.FindProperty("_vibrationLabel").objectReferenceValue = vibration.GetComponentInChildren<Text>();
            serialized.FindProperty("_creditsText").objectReferenceValue = credits;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            settingsPanel.gameObject.SetActive(false);
            creditsPanel.gameObject.SetActive(false);
        }

        // Full-screen panel that blocks the menu behind it.
        private static RectTransform CreatePanel(string name, RectTransform parent, float alpha)
        {
            RectTransform panel = ArenaTestSceneBuilder.CreateUiRect(name, parent, Vector2.zero, Vector2.one);
            var image = panel.gameObject.AddComponent<Image>();
            Color color = ArenaTestSceneBuilder.PaletteNight;
            color.a = alpha;
            image.color = color;
            return panel;
        }

        private static Slider CreateVolumeSlider(string name, RectTransform parent, Font font, string label, int row,
            UnityEngine.Events.UnityAction<float> action)
        {
            float y = FirstSliderY - row * SliderStep;
            ArenaTestSceneBuilder.CreateText(name + "Label", parent, font, label, ArenaTestSceneBuilder.HudPixelScale, new Vector2(0f, y));

            var center = new Vector2(0.5f, 0.5f);
            RectTransform rect = ArenaTestSceneBuilder.CreateUiRect(name + "Slider", parent, center, center);
            rect.sizeDelta = SliderSize;
            rect.anchoredPosition = new Vector2(0f, y - SliderLabelGap);

            Sprite knob = ArenaTestSceneBuilder.LoadUiSprite("Slider_Knob.png");
            float inset = knob.rect.width * ArenaTestSceneBuilder.HudPixelScale * 0.5f;

            RectTransform track = ArenaTestSceneBuilder.CreateUiRect("Background", rect, new Vector2(0f, 0.5f), new Vector2(1f, 0.5f));
            track.sizeDelta = new Vector2(0f, TrackHeight);
            track.gameObject.AddComponent<Image>().color = PaletteSlate0;

            RectTransform fillArea = ArenaTestSceneBuilder.CreateUiRect("Fill Area", rect, new Vector2(0f, 0.5f), new Vector2(1f, 0.5f));
            fillArea.sizeDelta = new Vector2(-2f * inset, TrackHeight);
            fillArea.anchoredPosition = Vector2.zero;
            RectTransform fill = ArenaTestSceneBuilder.CreateUiRect("Fill", fillArea, Vector2.zero, Vector2.one);
            fill.sizeDelta = new Vector2(inset, 0f);
            fill.anchoredPosition = new Vector2(-inset * 0.5f, 0f);
            var fillImage = fill.gameObject.AddComponent<Image>();
            fillImage.color = PaletteOrange;
            fillImage.raycastTarget = false;

            RectTransform handleArea = ArenaTestSceneBuilder.CreateUiRect("Handle Slide Area", rect, Vector2.zero, Vector2.one);
            handleArea.sizeDelta = new Vector2(-2f * inset, 0f);
            RectTransform handle = ArenaTestSceneBuilder.CreateUiRect("Handle", handleArea, Vector2.zero, Vector2.one);
            handle.sizeDelta = new Vector2(knob.rect.width * ArenaTestSceneBuilder.HudPixelScale, 0f);
            var handleImage = handle.gameObject.AddComponent<Image>();
            handleImage.sprite = knob;

            var slider = rect.gameObject.AddComponent<Slider>();
            slider.fillRect = fill;
            slider.handleRect = handle;
            slider.targetGraphic = handleImage;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 1f;
            UnityEventTools.AddPersistentListener(slider.onValueChanged, action);
            return slider;
        }
    }
}
