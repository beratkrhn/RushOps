using RushOps.Characters;
using RushOps.Editor.Art;
using RushOps.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RushOps.Editor.Scenes
{
    /// <summary>
    /// Builds the <c>MainMenu</c> scene (issue #125, F-UI-02): logo, the player in the standard skin and the buttons
    /// „Story Mode“, „Spielerauswahl“ and „Einstellungen“ inside the safe area; the version and, in development
    /// builds, a button to the test menu at the bottom. Spielerauswahl (#130) is greyed
    /// out until its screen exists; „Einstellungen“ opens the settings screen (#126, <see cref="SettingsScreenBuilder"/>).
    /// Batchmode: <c>-executeMethod RushOps.Editor.Scenes.MainMenuSceneBuilder.Build</c>
    /// </summary>
    public static class MainMenuSceneBuilder
    {
        private const string ScenePath = "Assets/_Project/Scenes/" + MainMenu.SceneName + ".unity";
        private const string LogoFile = "Logo_12.png";
        private const float LogoY = 190f;
        private const float FigureY = 40f;
        private const float FirstButtonY = -90f;
        private const float ButtonStep = 55f;
        private const float FooterY = -295f;
        private static readonly Color PaletteOutline = new Color32(13, 11, 20, 255);
        private static readonly Color PaletteSlate3 = new Color32(107, 106, 128, 255);

        [MenuItem("RushOps/Szenen/MainMenu erzeugen")]
        public static void Build()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Font font = PixelFontBuilder.Build();

            var cameraGo = new GameObject("Main Camera");
            cameraGo.tag = "MainCamera";
            var camera = cameraGo.AddComponent<Camera>();
            camera.orthographic = true;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = ArenaTestSceneBuilder.PaletteNight;

            var eventSystem = new GameObject("EventSystem", typeof(EventSystem));
            ArenaTestSceneBuilder.ForgetCachedDefaultUiActions();
            eventSystem.AddComponent<InputSystemUIInputModule>().AssignDefaultActions();

            var root = new GameObject("Menu", typeof(RectTransform));
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = true;
            var scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(ArenaTestSceneBuilder.ReferenceWidth, ArenaTestSceneBuilder.ReferenceHeight);
            scaler.matchWidthOrHeight = 0f;
            root.AddComponent<GraphicRaycaster>();

            RectTransform safeArea = ArenaTestSceneBuilder.CreateUiRect("SafeArea", root.transform, Vector2.zero, Vector2.one);
            safeArea.gameObject.AddComponent<SafeAreaFitter>();

            var menu = root.AddComponent<MainMenu>();
            var settings = root.AddComponent<SettingsMenu>();
            CreateSprite("Logo", safeArea, ArenaTestSceneBuilder.LoadUiSprite(LogoFile), new Vector2(0f, LogoY));
            CreateFigure(safeArea);

            CreateButton("StoryButton", safeArea, font, "Story Mode", 0, true, true, menu.StartStory);
            CreateButton("SkinsButton", safeArea, font, "Spielerauswahl", 1, false, false, null);
            CreateButton("SettingsButton", safeArea, font, "Einstellungen", 2, false, true, settings.Open);

            Text version = ArenaTestSceneBuilder.CreateText(
                "Version", safeArea, font, "v0.0.0", 1, new Vector2(140f, FooterY));
            version.color = ArenaTestSceneBuilder.PaletteSlate2;
            GameObject testButton = CreateTestMenuButton(safeArea, font, menu);

            var serialized = new SerializedObject(menu);
            serialized.FindProperty("_storyScene").stringValue = IslandMap.SceneName;
            serialized.FindProperty("_testMenuScene").stringValue = TestMenuSceneBuilder.SceneName;
            serialized.FindProperty("_version").objectReferenceValue = version;
            serialized.FindProperty("_testMenuButton").objectReferenceValue = testButton;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            var audio = new GameObject("Audio");
            ArenaTestSceneBuilder.CreateAudio(audio);
            SettingsScreenBuilder.Create(safeArea, font, settings, audio.GetComponent<RushOps.Audio.AudioVolumes>());
            ArenaTestSceneBuilder.AddClickSounds(audio.GetComponent<RushOps.Audio.UiClickSound>());

            EditorSceneManager.SaveScene(scene, ScenePath);
            ArenaTestSceneBuilder.AddToBuildSettings(ScenePath, false);
            AssetDatabase.SaveAssets();
            Debug.Log($"[MainMenuSceneBuilder] Saved {ScenePath}");
        }

        internal static Image CreateSprite(string name, RectTransform parent, Sprite sprite, Vector2 position)
        {
            var center = new Vector2(0.5f, 0.5f);
            RectTransform rect = ArenaTestSceneBuilder.CreateUiRect(name, parent, center, center);
            rect.sizeDelta = sprite.rect.size * ArenaTestSceneBuilder.HudPixelScale;
            rect.anchoredPosition = position;
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.raycastTarget = false;
            return image;
        }

        // The player in the standard skin, facing the viewer; the skin selection (#130) swaps the animation.
        private static void CreateFigure(RectTransform parent)
        {
            var idle = AssetDatabase.LoadAssetAtPath<DirectionalAnimation>(DirectionalAnimationBuilder.PlayerIdlePath);
            Sprite first = idle.GetFrame(Direction8.S, 0);
            Image image = CreateSprite("Player", parent, first, new Vector2(0f, FigureY));
            var figure = image.gameObject.AddComponent<MenuFigure>();
            var serialized = new SerializedObject(figure);
            serialized.FindProperty("_animation").objectReferenceValue = idle;
            serialized.FindProperty("_direction").enumValueIndex = (int)Direction8.S;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        internal static Button CreateButton(string name, RectTransform parent, Font font, string label, int row, bool primary,
            bool interactable, UnityEngine.Events.UnityAction action)
        {
            return CreateButton(name, parent, font, label, new Vector2(0f, FirstButtonY - row * ButtonStep), primary, interactable, action);
        }

        internal static Button CreateButton(string name, RectTransform parent, Font font, string label, Vector2 position,
            bool primary, bool interactable, UnityEngine.Events.UnityAction action)
        {
            string look = primary ? "Button_Primary" : "Button";
            Sprite normal = ArenaTestSceneBuilder.LoadUiSprite(look + ".png");
            Image image = CreateSprite(name, parent, normal, position);
            image.raycastTarget = true;
            var button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.transition = Selectable.Transition.SpriteSwap;
            button.spriteState = new SpriteState
            {
                highlightedSprite = normal,
                selectedSprite = normal,
                pressedSprite = ArenaTestSceneBuilder.LoadUiSprite(look + "_Pressed.png"),
                disabledSprite = ArenaTestSceneBuilder.LoadUiSprite("Button_Disabled.png"),
            };
            button.interactable = interactable;
            if (action != null)
            {
                UnityEventTools.AddPersistentListener(button.onClick, action);
            }

            // The plate sits above the lip, two sprite pixels higher than the middle of the button.
            Text text = ArenaTestSceneBuilder.CreateText(
                "Label", (RectTransform)image.transform, font, label, ArenaTestSceneBuilder.HudPixelScale, new Vector2(0f, 4f));
            if (!interactable)
            {
                text.color = PaletteSlate3;
            }
            else if (primary)
            {
                text.color = PaletteOutline;
            }

            return button;
        }

        private static GameObject CreateTestMenuButton(RectTransform parent, Font font, MainMenu menu)
        {
            var center = new Vector2(0.5f, 0.5f);
            RectTransform rect = ArenaTestSceneBuilder.CreateUiRect("TestMenuButton", parent, center, center);
            rect.sizeDelta = new Vector2(70f, 24f);
            rect.anchoredPosition = new Vector2(-130f, FooterY);
            var image = rect.gameObject.AddComponent<Image>();
            image.color = ArenaTestSceneBuilder.PaletteSlate2;
            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            UnityEventTools.AddPersistentListener(button.onClick, menu.OpenTestMenu);
            ArenaTestSceneBuilder.CreateText("Label", rect, font, "Testmenü", 1, Vector2.zero);
            return rect.gameObject;
        }
    }
}
