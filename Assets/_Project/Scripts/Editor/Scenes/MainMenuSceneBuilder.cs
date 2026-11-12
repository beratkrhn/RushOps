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
    /// „Story Mode“, „Endlos“, „Spielerauswahl“ and „Einstellungen“ inside the safe area; the version and, in development
    /// builds, a button to the test menu at the bottom. „Spielerauswahl“ opens the skin carousel (#130,
    /// <see cref="SkinSelectionBuilder"/>), „Waffenwahl“ the weapon carousel (#176, <see cref="WeaponSelectionBuilder"/>),
    /// „Einstellungen“ the settings screen (#126, <see cref="SettingsScreenBuilder"/>).
    /// Batchmode: <c>-executeMethod RushOps.Editor.Scenes.MainMenuSceneBuilder.Build</c>
    /// </summary>
    public static class MainMenuSceneBuilder
    {
        private const string ScenePath = "Assets/_Project/Scenes/" + MainMenu.SceneName + ".unity";
        private const string LogoFile = "Logo_12.png";
        private const float LogoY = 190f;
        private const float FigureY = 40f;
        private const float FirstButtonY = -80f;
        private const float ButtonStep = 50f;
        private const float FooterY = -295f;
        private const float BalanceMargin = 12f;
        private const float BalanceIconSize = 20f;
        private const float BalanceTextWidth = 56f;
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
            var skins = root.AddComponent<SkinCarousel>();
            var weapons = root.AddComponent<WeaponCarousel>();
            CreateSprite("Logo", safeArea, ArenaTestSceneBuilder.LoadUiSprite(LogoFile), new Vector2(0f, LogoY));
            MenuFigure figure = CreateFigure(safeArea);

            CreateButton("StoryButton", safeArea, font, "Story Mode", 0, true, true, menu.StartStory);
            Button endlessButton = CreateButton("EndlessButton", safeArea, font, "Endlos", 1, true, true, null);
            CreateButton("SkinsButton", safeArea, font, "Spielerauswahl", 2, false, true, skins.Open);
            CreateButton("WeaponsButton", safeArea, font, "Waffenwahl", 3, false, true, weapons.Open);
            CreateButton("SettingsButton", safeArea, font, "Einstellungen", 4, false, true, settings.Open);

            Text version = ArenaTestSceneBuilder.CreateText(
                "Version", safeArea, font, "v0.0.0", 1, new Vector2(140f, FooterY));
            version.color = ArenaTestSceneBuilder.PaletteSlate2;
            GameObject testButton = CreateTestMenuButton(safeArea, font, menu);

            var serialized = new SerializedObject(menu);
            serialized.FindProperty("_storyScene").stringValue = IslandMap.SceneName;
            serialized.FindProperty("_endlessScene").stringValue = EndlessArenaSceneBuilder.SceneName;
            serialized.FindProperty("_testMenuScene").stringValue = TestMenuSceneBuilder.SceneName;
            serialized.FindProperty("_introScene").stringValue = RushOps.Story.IntroScene.SceneName;
            serialized.FindProperty("_save").objectReferenceValue =
                ArenaTestSceneBuilder.LoadOrCreateConfig<RushOps.Save.SaveGameService>(ArenaTestSceneBuilder.SaveGameServicePath);
            serialized.FindProperty("_version").objectReferenceValue = version;
            serialized.FindProperty("_testMenuButton").objectReferenceValue = testButton;
            // The walk to the cabinet before an arcade run (issue #324); the whole menu shrinks away.
            serialized.FindProperty("_arcadeIntro").objectReferenceValue = RushOps.Editor.Arcade.ArcadeIntroBuilder.Create(safeArea, font);
            serialized.ApplyModifiedPropertiesWithoutUndo();

            var audio = new GameObject("Audio");
            ArenaTestSceneBuilder.CreateAudio(audio);
            GoldBalanceView menuBalance = CreateBalance(safeArea, font, "MenuBalance");
            SkinSelectionBuilder.Create(safeArea, font, skins, figure, menuBalance);
            WeaponSelectionBuilder.Create(safeArea, font, weapons);
            // „Endlos“ opens its area with „Starten“, „Tageslauf“ and the daily bests (issue #280).
            EndlessMenu endless = EndlessMenuBuilder.Create(safeArea, root, font, menu);
            UnityEventTools.AddPersistentListener(endlessButton.onClick, endless.Open);
            SettingsScreenBuilder.Create(safeArea, font, settings, audio.GetComponent<RushOps.Audio.AudioVolumes>());
            ArenaTestSceneBuilder.AddClickSounds(audio.GetComponent<RushOps.Audio.UiClickSound>());

            EditorSceneManager.SaveScene(scene, ScenePath);
            ArenaTestSceneBuilder.AddToBuildSettings(ScenePath, false);
            AssetDatabase.SaveAssets();
            Debug.Log($"[MainMenuSceneBuilder] Saved {ScenePath}");
        }

        // Coin icon and the spendable gold balance in the top right corner (F-GLD-06, issue #237).
        internal static GoldBalanceView CreateBalance(RectTransform parent, Font font, string name)
        {
            var topRight = new Vector2(1f, 1f);
            RectTransform row = ArenaTestSceneBuilder.CreateUiRect(name, parent, topRight, topRight);
            row.pivot = topRight;
            row.anchoredPosition = new Vector2(-BalanceMargin, -BalanceMargin);
            row.sizeDelta = Vector2.zero;

            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Sprites/Pickups/Gold_0.png");
            Text label = ArenaTestSceneBuilder.CreateText(name + "Text", row, font, "0", ArenaTestSceneBuilder.HudPixelScale, Vector2.zero);
            label.rectTransform.anchorMin = topRight;
            label.rectTransform.anchorMax = topRight;
            label.rectTransform.pivot = topRight;
            label.rectTransform.anchoredPosition = Vector2.zero;
            label.alignment = TextAnchor.MiddleRight;
            label.color = new Color32(246, 208, 77, 255);

            RectTransform icon = ArenaTestSceneBuilder.CreateUiRect("Icon", row, topRight, topRight);
            icon.pivot = topRight;
            icon.sizeDelta = new Vector2(BalanceIconSize, BalanceIconSize);
            icon.anchoredPosition = new Vector2(-BalanceTextWidth, 0f);
            var image = icon.gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;
            image.raycastTarget = false;

            var view = row.gameObject.AddComponent<GoldBalanceView>();
            var serialized = new SerializedObject(view);
            serialized.FindProperty("_save").objectReferenceValue =
                ArenaTestSceneBuilder.LoadOrCreateConfig<RushOps.Save.SaveGameService>(ArenaTestSceneBuilder.SaveGameServicePath);
            serialized.FindProperty("_label").objectReferenceValue = label;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return view;
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

        // The player facing the viewer; the skin carousel (#130) dresses it in the current skin.
        private static MenuFigure CreateFigure(RectTransform parent)
        {
            var idle = AssetDatabase.LoadAssetAtPath<DirectionalAnimation>(DirectionalAnimationBuilder.PlayerIdlePath);
            Sprite first = idle.GetFrame(Direction8.S, 0);
            Image image = CreateSprite("Player", parent, first, new Vector2(0f, FigureY));
            var figure = image.gameObject.AddComponent<MenuFigure>();
            var serialized = new SerializedObject(figure);
            serialized.FindProperty("_animation").objectReferenceValue = idle;
            serialized.FindProperty("_direction").enumValueIndex = (int)Direction8.S;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return figure;
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
