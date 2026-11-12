using RushOps.Arcade;
using RushOps.Cameras;
using RushOps.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace RushOps.Editor.Arcade
{
    /// <summary>
    /// Adds the arcade cabinet to the endless arena (issue #321): the inactive display camera and cabinet canvas,
    /// and <see cref="ArcadeCabinet"/>, which turns them on when the run starts in the arcade mode.
    /// </summary>
    internal static class ArcadeCabinetBuilder
    {
        // Above the game, below the touch controls (10) and the HUD (20).
        private const int CabinetSortingOrder = 5;

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
            Place(CreateImage("StickLeft", cabinet, config.StickSprites[0]), config.StickLeft);
            Place(CreateImage("StickRight", cabinet, config.StickSprites[0]), config.StickRight);
            Place(CreateImage("DashButton", cabinet, config.DashSprite), config.DashButton);
            for (int i = 0; i < config.DecoButtons.Length; i++)
            {
                Place(CreateImage($"DecoButton_{i + 1}", cabinet, config.DecoSprites[i]), config.DecoButtons[i]);
            }

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
            serialized.ApplyModifiedPropertiesWithoutUndo();

            display.gameObject.SetActive(false);
            canvasObject.SetActive(false);
            return controller;
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
