using RushOps.Arcade;
using RushOps.Cameras;
using RushOps.Player;
using RushOps.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace RushOps.Editor.Arcade
{
    /// <summary>
    /// Adds the arcade cabinet to the endless arena (issue #321): the inactive display camera and cabinet canvas,
    /// and <see cref="ArcadeCabinet"/>, which turns them on when the run starts in the arcade mode. The control panel
    /// (issue #322) reuses the touch controls of the game: a <see cref="VirtualStick"/> on each half of the panel
    /// feeds the same gamepad sticks, the dash button the same gamepad button.
    /// </summary>
    internal static class ArcadeCabinetBuilder
    {
        // Above the game, below the touch controls (10) and the HUD (20).
        private const int CabinetSortingOrder = 5;

        // Same control paths as the touch controls of ArenaTestSceneBuilder.
        private const string MoveStickControl = "<Gamepad>/leftStick";
        private const string AimStickControl = "<Gamepad>/rightStick";
        private const string DashButtonControl = "<Gamepad>/buttonSouth";
        private const string UiSprites = "Assets/_Project/Art/Sprites/UI/";

        /// <summary>Creates the cabinet in the open scene and returns its controller.</summary>
        public static ArcadeCabinet Create()
        {
            ArcadeConfig config = ArcadeConfigBuilder.Get();
            Camera gameCamera = Object.FindAnyObjectByType<CameraFollow>().GetComponent<Camera>();
            Canvas hud = Object.FindAnyObjectByType<PauseController>().GetComponent<Canvas>();

            var root = new GameObject("Arcade");
            Camera display = CreateDisplayCamera(root.transform, config);

            var canvasObject = new GameObject("ArcadeCanvas", typeof(RectTransform));
            canvasObject.transform.SetParent(root.transform, false);
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = CabinetSortingOrder;
            canvasObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            canvasObject.AddComponent<GraphicRaycaster>();

            Image background = CreateImage("Background", canvasObject.transform, config.BodySprite);
            var backgroundRect = (RectTransform)background.transform;
            backgroundRect.anchorMin = Vector2.zero;
            backgroundRect.anchorMax = Vector2.one;
            backgroundRect.offsetMin = Vector2.zero;
            backgroundRect.offsetMax = Vector2.zero;
            background.type = Image.Type.Tiled;

            var cabinet = new GameObject("Cabinet", typeof(RectTransform)).GetComponent<RectTransform>();
            cabinet.SetParent(canvasObject.transform, false);
            cabinet.anchorMin = Vector2.zero;
            cabinet.anchorMax = Vector2.zero;
            cabinet.pivot = Vector2.zero;
            cabinet.sizeDelta = config.Reference;

            var screenObject = new GameObject("Screen", typeof(RectTransform));
            screenObject.transform.SetParent(cabinet, false);
            Place((RectTransform)screenObject.transform, config.Screen);
            var screen = screenObject.AddComponent<RawImage>();
            screen.raycastTarget = false;

            Place(CreateImage("ScreenFrame", cabinet, config.ScreenFrameSprite), config.Screen);
            Place(CreateImage("Bezel", cabinet, config.BezelSprite), config.Bezel);
            Place(CreateImage("Marquee", cabinet, config.MarqueeSprite), config.Marquee);
            Place(CreateImage("Panel", cabinet, config.PanelSprite), config.Panel);
            TouchControlsConfig sticks = Object.FindAnyObjectByType<VirtualStick>().Config;
            RectInt panel = config.Panel;
            int half = panel.width / 2;
            VirtualStick move = CreateZone("MoveZone", cabinet, new RectInt(panel.x, panel.y, half, panel.height),
                StickSide.Left, MoveStickControl, sticks);
            VirtualStick aim = CreateZone("AimZone", cabinet, new RectInt(panel.x + half, panel.y, panel.width - half,
                panel.height), StickSide.Right, AimStickControl, sticks);
            CreateStickView("StickLeft", cabinet, config, config.StickLeft, move);
            CreateStickView("StickRight", cabinet, config, config.StickRight, aim);
            for (int i = 0; i < config.DecoButtons.Length; i++)
            {
                Place(CreateImage($"DecoButton_{i + 1}", cabinet, config.DecoSprites[i]), config.DecoButtons[i]);
            }

            // Drawn after the zones, so they get the touches on them first.
            CreateDashButton(cabinet, config, Object.FindAnyObjectByType<PlayerDash>());
            CreateSwapButton(cabinet, config, Object.FindAnyObjectByType<PlayerWeapon>());

            var controller = root.AddComponent<ArcadeCabinet>();
            var serialized = new SerializedObject(controller);
            serialized.FindProperty("_config").objectReferenceValue = config;
            serialized.FindProperty("_gameCamera").objectReferenceValue = gameCamera;
            serialized.FindProperty("_displayCamera").objectReferenceValue = display;
            serialized.FindProperty("_cabinetCanvas").objectReferenceValue = canvas;
            serialized.FindProperty("_cabinet").objectReferenceValue = cabinet;
            serialized.FindProperty("_background").objectReferenceValue = background;
            serialized.FindProperty("_screen").objectReferenceValue = screen;
            serialized.FindProperty("_hud").objectReferenceValue = hud;
            serialized.FindProperty("_touchControls").objectReferenceValue =
                Object.FindAnyObjectByType<TouchControlsVisibility>().gameObject;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            display.gameObject.SetActive(false);
            canvasObject.SetActive(false);
            return controller;
        }

        // A transparent half of the panel that takes the touches of one stick; the stick floats where the thumb lands.
        private static VirtualStick CreateZone(string name, Transform parent, RectInt rect, StickSide side,
            string controlPath, TouchControlsConfig config)
        {
            Image area = CreateImage(name, parent, null);
            area.color = Color.clear;
            area.raycastTarget = true;
            Place(area, rect);
            var stick = area.gameObject.AddComponent<VirtualStick>();
            // Set the serialized path: the controlPath setter would create a virtual gamepad in the editor.
            var serialized = new SerializedObject(stick);
            serialized.FindProperty("_controlPath").stringValue = controlPath;
            serialized.FindProperty("_config").objectReferenceValue = config;
            serialized.FindProperty("_side").enumValueIndex = (int)side;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return stick;
        }

        private static void CreateStickView(string name, Transform parent, ArcadeConfig config, RectInt rect, VirtualStick stick)
        {
            Image image = CreateImage(name, parent, config.StickSprites[0]);
            Place(image, rect);
            var view = image.gameObject.AddComponent<ArcadeStickView>();
            var serialized = new SerializedObject(view);
            serialized.FindProperty("_config").objectReferenceValue = config;
            serialized.FindProperty("_stick").objectReferenceValue = stick;
            serialized.FindProperty("_image").objectReferenceValue = image;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreateDashButton(Transform parent, ArcadeConfig config, PlayerDash dash)
        {
            Image image = CreateImage("DashButton", parent, config.DashSprite);
            image.raycastTarget = true;
            Place(image, config.DashButton);
            var button = new SerializedObject(image.gameObject.AddComponent<OnScreenButton>());
            button.FindProperty("m_ControlPath").stringValue = DashButtonControl;
            button.ApplyModifiedPropertiesWithoutUndo();
            var view = image.gameObject.AddComponent<ArcadeDashButtonView>();
            var serialized = new SerializedObject(view);
            serialized.FindProperty("_config").objectReferenceValue = config;
            serialized.FindProperty("_dash").objectReferenceValue = dash;
            serialized.FindProperty("_image").objectReferenceValue = image;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        // The weapon swap of the game (issue #175, W10) on the panel; WeaponSwapButton hides it with one weapon.
        private static void CreateSwapButton(Transform parent, ArcadeConfig config, PlayerWeapon weapon)
        {
            Image plate = CreateImage("SwapButton", parent, LoadUiSprite("Swap_Button.png"));
            plate.raycastTarget = true;
            Place(plate, config.SwapButton);
            var button = plate.gameObject.AddComponent<Button>();
            button.targetGraphic = plate;
            Image icon = CreateImage("Icon", plate.transform, LoadUiSprite("Weapon_Pistol.png"));
            var iconRect = (RectTransform)icon.transform;
            iconRect.sizeDelta = new Vector2(config.SwapButton.width - 6f, (config.SwapButton.width - 6f) * 0.5f);
            icon.preserveAspect = true;
            plate.enabled = false;
            icon.enabled = false;

            var view = plate.gameObject.AddComponent<WeaponSwapButton>();
            UnityEventTools.AddPersistentListener(button.onClick, view.Swap);
            var serialized = new SerializedObject(view);
            serialized.FindProperty("_weapon").objectReferenceValue = weapon;
            serialized.FindProperty("_background").objectReferenceValue = plate;
            serialized.FindProperty("_icon").objectReferenceValue = icon;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Sprite LoadUiSprite(string file)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(UiSprites + file);
            if (sprite == null)
            {
                throw new System.InvalidOperationException($"Sprite not found at {UiSprites}{file}");
            }

            return sprite;
        }

        /// <summary>Puts <paramref name="graphic"/> at a rectangle of the reference grid (origin top left).</summary>
        internal static void Place(Graphic graphic, RectInt rect)
        {
            Place((RectTransform)graphic.transform, rect);
        }

        /// <summary>Puts <paramref name="transform"/> at a rectangle of the reference grid (origin top left).</summary>
        internal static void Place(RectTransform transform, RectInt rect)
        {
            transform.anchorMin = new Vector2(0f, 1f);
            transform.anchorMax = new Vector2(0f, 1f);
            transform.pivot = new Vector2(0f, 1f);
            transform.anchoredPosition = new Vector2(rect.x, -rect.y);
            transform.sizeDelta = new Vector2(rect.width, rect.height);
        }

        /// <summary>An image without raycasts, as all cabinet parts are only drawn.</summary>
        internal static Image CreateImage(string name, Transform parent, Sprite sprite)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var image = go.AddComponent<Image>();
            image.sprite = sprite;
            image.raycastTarget = false;
            return image;
        }

        // Draws only the canvases: no layer, a clear in the background color, after the game camera.
        private static Camera CreateDisplayCamera(Transform parent, ArcadeConfig config)
        {
            var go = new GameObject("ArcadeDisplayCamera");
            go.transform.SetParent(parent, false);
            go.transform.position = new Vector3(0f, 0f, -10f);
            var camera = go.AddComponent<Camera>();
            camera.orthographic = true;
            camera.cullingMask = 0;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = config.Background;
            camera.depth = 10f;
            go.AddComponent<UniversalAdditionalCameraData>();
            return camera;
        }
    }
}
