using RushOps.Arcade;
using RushOps.Editor.Scenes;
using RushOps.Editor.Skins;
using RushOps.Save;
using RushOps.Skins;
using RushOps.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;

namespace RushOps.Editor.Arcade
{
    /// <summary>
    /// Adds the transition to the cabinet to the main menu (issue #324): an inactive overlay with the arcade hall, a
    /// small cabinet, the figure and a full-screen skip button, plus an always active black image for fading.
    /// </summary>
    internal static class ArcadeIntroBuilder
    {
        // Above the menu, its areas and dialogs.
        private const int OverlaySortingOrder = 50;
        private const int FadeSortingOrder = 60;
        // The figure at 2.5 times its pixels: it stands in front of the control panel and leaves the screen visible.
        private const float FigureSize = 160f;
        private const float FloorHeight = 0.42f;
        private const float HintY = 24f;
        private const string HintText = "Tippen zum Überspringen";
        private static readonly Color Floor = new Color32(46, 44, 61, 255);
        private static readonly Color FloorLine = new Color32(59, 58, 79, 255);
        private static readonly Color ScreenOff = new Color32(13, 43, 69, 255);
        private static readonly Color HintColor = new Color32(143, 142, 163, 255);

        /// <summary>Creates the transition and returns it; <paramref name="menu"/> is the content that shrinks away.</summary>
        public static ArcadeIntro Create(RectTransform menu, Font font)
        {
            ArcadeConfig config = ArcadeConfigBuilder.Get();
            var root = new GameObject("ArcadeIntro");
            var intro = root.AddComponent<ArcadeIntro>();

            GameObject overlay = CreateCanvas("ArcadeIntroCanvas", root.transform, OverlaySortingOrder, true);
            RectTransform hallRect = ArenaTestSceneBuilder.CreateUiRect("Hall", overlay.transform, Vector2.zero, Vector2.one);
            var hall = hallRect.gameObject.AddComponent<CanvasGroup>();
            hall.blocksRaycasts = false;
            Stretch(ColorImage("Wall", hallRect, ArenaTestSceneBuilder.PaletteNight), Vector2.zero, Vector2.one);
            Stretch(ColorImage("Floor", hallRect, Floor), Vector2.zero, new Vector2(1f, FloorHeight));
            Image line = ColorImage("FloorLine", hallRect, FloorLine);
            Stretch(line, new Vector2(0f, FloorHeight), new Vector2(1f, FloorHeight));
            line.rectTransform.sizeDelta = new Vector2(0f, 2f);

            RectTransform cabinet = CreateCabinet(hallRect, config);
            Image figure = ArcadeCabinetBuilder.CreateImage("Figure", hallRect, PlayerIdleNorth());
            var figureRect = figure.rectTransform;
            figureRect.anchorMin = new Vector2(0.5f, 0.5f);
            figureRect.anchorMax = new Vector2(0.5f, 0.5f);
            figureRect.pivot = new Vector2(0.5f, 0f);
            figureRect.sizeDelta = new Vector2(FigureSize, FigureSize);
            figureRect.anchoredPosition = config.IntroFigureStart;

            Text hint = ArenaTestSceneBuilder.CreateText("SkipHint", hallRect, font, HintText, 1, Vector2.zero);
            hint.rectTransform.anchorMin = new Vector2(0.5f, 0f);
            hint.rectTransform.anchorMax = new Vector2(0.5f, 0f);
            hint.rectTransform.anchoredPosition = new Vector2(0f, HintY);
            hint.color = HintColor;

            // Last, so it takes every tap on the overlay.
            Image skipArea = ColorImage("Skip", overlay.transform, Color.clear);
            skipArea.raycastTarget = true;
            Stretch(skipArea, Vector2.zero, Vector2.one);
            var skip = skipArea.gameObject.AddComponent<Button>();
            skip.transition = Selectable.Transition.None;
            UnityEventTools.AddPersistentListener(skip.onClick, intro.Skip);

            GameObject fade = CreateCanvas("ArcadeFadeCanvas", root.transform, FadeSortingOrder, false);
            Image black = ColorImage("Black", fade.transform, Color.black);
            Stretch(black, Vector2.zero, Vector2.one);
            black.enabled = false;

            var serialized = new SerializedObject(intro);
            serialized.FindProperty("_config").objectReferenceValue = config;
            serialized.FindProperty("_save").objectReferenceValue =
                ArenaTestSceneBuilder.LoadOrCreateConfig<SaveGameService>(ArenaTestSceneBuilder.SaveGameServicePath);
            serialized.FindProperty("_catalog").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<SkinCatalog>(SkinCatalogBuilder.CatalogPath);
            serialized.FindProperty("_menu").objectReferenceValue = menu;
            serialized.FindProperty("_overlay").objectReferenceValue = overlay;
            serialized.FindProperty("_hall").objectReferenceValue = hall;
            serialized.FindProperty("_cabinet").objectReferenceValue = cabinet;
            serialized.FindProperty("_figure").objectReferenceValue = figure;
            serialized.FindProperty("_black").objectReferenceValue = black;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            overlay.SetActive(false);
            return intro;
        }

        // The cabinet of the run, pivoted at the centre of its screen so the dive can scale into it.
        private static RectTransform CreateCabinet(RectTransform parent, ArcadeConfig config)
        {
            var cabinet = new GameObject("Cabinet", typeof(RectTransform)).GetComponent<RectTransform>();
            cabinet.SetParent(parent, false);
            cabinet.anchorMin = new Vector2(0.5f, 0.5f);
            cabinet.anchorMax = new Vector2(0.5f, 0.5f);
            cabinet.sizeDelta = config.Reference;
            RectInt screen = config.Screen;
            cabinet.pivot = new Vector2((screen.x + screen.width * 0.5f) / config.Reference.x,
                1f - (screen.y + screen.height * 0.5f) / config.Reference.y);
            cabinet.anchoredPosition = config.IntroCabinetPosition;
            cabinet.localScale = Vector3.one * config.IntroCabinetScale;

            ArcadeCabinetBuilder.Place(ColorImage("Screen", cabinet, ScreenOff), screen);
            ArcadeCabinetBuilder.Place(ArcadeCabinetBuilder.CreateImage("ScreenFrame", cabinet, config.ScreenFrameSprite), screen);
            Image gloss = ArcadeCabinetBuilder.CreateImage("ScreenGloss", cabinet, config.ScreenGlossSprite);
            ArcadeCabinetBuilder.Place(gloss, screen);
            gloss.color = new Color(1f, 1f, 1f, config.CrtGloss);
            ArcadeCabinetBuilder.Place(ArcadeCabinetBuilder.CreateImage("Bezel", cabinet, config.BezelSprite), config.Bezel);
            ArcadeCabinetBuilder.Place(ArcadeCabinetBuilder.CreateImage("Marquee", cabinet, config.MarqueeSprite), config.Marquee);
            ArcadeCabinetBuilder.Place(ArcadeCabinetBuilder.CreateImage("Panel", cabinet, config.PanelSprite), config.Panel);
            ArcadeCabinetBuilder.Place(ArcadeCabinetBuilder.CreateImage("StickLeft", cabinet, config.StickSprites[0]), config.StickLeft);
            ArcadeCabinetBuilder.Place(ArcadeCabinetBuilder.CreateImage("StickRight", cabinet, config.StickSprites[0]), config.StickRight);
            ArcadeCabinetBuilder.Place(ArcadeCabinetBuilder.CreateImage("DashButton", cabinet, config.DashSprite), config.DashButton);
            for (int i = 0; i < config.DecoButtons.Length; i++)
            {
                ArcadeCabinetBuilder.Place(
                    ArcadeCabinetBuilder.CreateImage($"DecoButton_{i + 1}", cabinet, config.DecoSprites[i]), config.DecoButtons[i]);
            }

            return cabinet;
        }

        private static GameObject CreateCanvas(string name, Transform parent, int sortingOrder, bool raycasts)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(ArenaTestSceneBuilder.ReferenceWidth, ArenaTestSceneBuilder.ReferenceHeight);
            scaler.matchWidthOrHeight = 0f;
            if (raycasts)
            {
                go.AddComponent<GraphicRaycaster>();
            }

            return go;
        }

        private static Image ColorImage(string name, Transform parent, Color color)
        {
            Image image = ArcadeCabinetBuilder.CreateImage(name, parent, null);
            image.color = color;
            return image;
        }

        private static void Stretch(Image image, Vector2 min, Vector2 max)
        {
            RectTransform rect = image.rectTransform;
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static Sprite PlayerIdleNorth()
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Sprites/Player/Player_Idle_N.png");
        }
    }
}
