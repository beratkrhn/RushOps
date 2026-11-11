using System.Collections.Generic;
using RushOps.Editor.Art;
using RushOps.Levels;
using RushOps.Save;
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
    /// Builds the <c>IslandMap</c> scene (issue #127): the three island sprites stacked bottom to top, one level point
    /// per slot of the <see cref="Campaign"/>, a dimmed cover on every island, and the title, arrows and „Zurück“ in the
    /// safe area. Run after the main menu, it adds the scene to the build.
    /// Batchmode: <c>-executeMethod RushOps.Editor.Scenes.IslandMapSceneBuilder.Build</c>
    /// </summary>
    public static class IslandMapSceneBuilder
    {
        private const string ScenePath = "Assets/_Project/Scenes/" + IslandMap.SceneName + ".unity";
        private const string CampaignPath = "Assets/_Project/Data/Levels/Campaign.asset";
        private const string ConfigPath = "Assets/_Project/Data/UI/IslandMapConfig.asset";

        // Island sprites by campaign index (Tools/gen_pixelart.py, MAP_ISLANDS).
        private static readonly string[] IslandSprites = { "Map_Strand.png", "Map_Stadt.png", "Map_Vulkan.png" };

        // Level points in sprite pixels from the top left of a 160x320 island sprite; must match MAP_NODES.
        private static readonly Vector2Int[] NodePixels = { new Vector2Int(84, 222), new Vector2Int(52, 160), new Vector2Int(100, 100) };

        private static readonly Vector2 ArrowTouchArea = new Vector2(96f, 44f);
        private const float StarSpacing = 17f;
        private const float BelowNode = 30f;
        // Level name one text line under the stars (decision C11).
        private const float NameBelowStars = 11f;
        // Badges one row under the stars (issue #288, F-UI-08); the name moves below them.
        private const float BadgesBelowStars = 18f;
        private const float BadgeSpacing = 18f;
        private static readonly Color Sea = new Color32(13, 43, 69, 255);

        [MenuItem("RushOps/Szenen/IslandMap erzeugen")]
        public static void Build()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Font font = PixelFontBuilder.Build();

            // Load assets after the font build: it reimports assets, which leaves references loaded earlier stale.
            IslandMapConfig config = LoadOrCreateConfig();
            RushOps.Scoring.BadgeConfig badgeConfig = BadgeBuilder.LoadConfig();
            var campaign = AssetDatabase.LoadAssetAtPath<Campaign>(CampaignPath);

            var cameraGo = new GameObject("Main Camera");
            cameraGo.tag = "MainCamera";
            var camera = cameraGo.AddComponent<Camera>();
            camera.orthographic = true;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Sea;

            var eventSystem = new GameObject("EventSystem", typeof(EventSystem));
            ArenaTestSceneBuilder.ForgetCachedDefaultUiActions();
            eventSystem.AddComponent<InputSystemUIInputModule>().AssignDefaultActions();

            var root = new GameObject("Map", typeof(RectTransform));
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = true;
            var scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(ArenaTestSceneBuilder.ReferenceWidth, ArenaTestSceneBuilder.ReferenceHeight);
            scaler.matchWidthOrHeight = 0f;
            root.AddComponent<GraphicRaycaster>();
            var map = root.AddComponent<IslandMap>();

            var center = new Vector2(0.5f, 0.5f);
            RectTransform islands = ArenaTestSceneBuilder.CreateUiRect("Islands", root.transform, center, center);
            islands.sizeDelta = Vector2.zero;

            var nodes = new List<LevelNode>();
            var covers = new List<GameObject>();
            var coverTexts = new List<Text>();
            for (int i = 0; i < campaign.Islands.Length; i++)
            {
                Image island = MainMenuSceneBuilder.CreateSprite($"Island_{i + 1}", islands,
                    ArenaTestSceneBuilder.LoadUiSprite(IslandSprites[i]), new Vector2(0f, i * config.IslandSpacing));
                var islandRect = (RectTransform)island.transform;
                for (int j = 0; j < campaign.Islands[i].Levels.Length && j < NodePixels.Length; j++)
                {
                    nodes.Add(CreateNode(islandRect, font, map, i, j, NodePixels[j], badgeConfig));
                }

                RectTransform cover = ArenaTestSceneBuilder.CreateUiRect("LockedCover", islandRect, Vector2.zero, Vector2.one);
                var coverImage = cover.gameObject.AddComponent<Image>();
                Color dim = ArenaTestSceneBuilder.PaletteNight;
                dim.a = 0.7f;
                coverImage.color = dim;
                coverTexts.Add(ArenaTestSceneBuilder.CreateText("Condition", cover, font, string.Empty,
                    ArenaTestSceneBuilder.HudPixelScale, Vector2.zero));
                covers.Add(cover.gameObject);
            }

            RectTransform safeArea = ArenaTestSceneBuilder.CreateUiRect("SafeArea", root.transform, Vector2.zero, Vector2.one);
            safeArea.gameObject.AddComponent<SafeAreaFitter>();
            Text title = ArenaTestSceneBuilder.CreateText("Title", safeArea, font, "Insel 1", ArenaTestSceneBuilder.HudPixelScale,
                new Vector2(0f, 285f));
            GameObject up = CreateArrow("UpArrow", safeArea, new Vector2(0f, 250f), false, map.Next);
            GameObject down = CreateArrow("DownArrow", safeArea, new Vector2(0f, -232f), true, map.Previous);
            MainMenuSceneBuilder.CreateButton("BackButton", safeArea, font, "Zurück", new Vector2(0f, -280f), false, true, map.Back);

            var serialized = new SerializedObject(map);
            serialized.FindProperty("_campaign").objectReferenceValue = campaign;
            serialized.FindProperty("_save").objectReferenceValue =
                ArenaTestSceneBuilder.LoadOrCreateConfig<SaveGameService>(ArenaTestSceneBuilder.SaveGameServicePath);
            serialized.FindProperty("_config").objectReferenceValue = config;
            serialized.FindProperty("_badges").objectReferenceValue = badgeConfig;
            serialized.FindProperty("_menuScene").stringValue = MainMenu.SceneName;
            serialized.FindProperty("_islands").objectReferenceValue = islands;
            SetArray(serialized.FindProperty("_nodes"), nodes);
            SetArray(serialized.FindProperty("_lockedCovers"), covers);
            SetArray(serialized.FindProperty("_lockedTexts"), coverTexts);
            serialized.FindProperty("_title").objectReferenceValue = title;
            serialized.FindProperty("_upArrow").objectReferenceValue = up;
            serialized.FindProperty("_downArrow").objectReferenceValue = down;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            var audio = new GameObject("Audio");
            ArenaTestSceneBuilder.CreateAudio(audio);
            ArenaTestSceneBuilder.AddClickSounds(audio.GetComponent<RushOps.Audio.UiClickSound>());

            EditorSceneManager.SaveScene(scene, ScenePath);
            ArenaTestSceneBuilder.AddToBuildSettings(ScenePath, false);
            AssetDatabase.SaveAssets();
            Debug.Log($"[IslandMapSceneBuilder] Saved {ScenePath} with {nodes.Count} level points");
        }

        // Feel values stay as tuned; the sprites are generated art and always set.
        private static IslandMapConfig LoadOrCreateConfig()
        {
            var config = ArenaTestSceneBuilder.LoadOrCreateConfig<IslandMapConfig>(ConfigPath);
            var serialized = new SerializedObject(config);
            serialized.FindProperty("_nodeOpen").objectReferenceValue = ArenaTestSceneBuilder.LoadUiSprite("Map_Node_Open.png");
            serialized.FindProperty("_nodeDone").objectReferenceValue = ArenaTestSceneBuilder.LoadUiSprite("Map_Node_Done.png");
            serialized.FindProperty("_nodeLocked").objectReferenceValue = ArenaTestSceneBuilder.LoadUiSprite("Map_Node_Locked.png");
            serialized.FindProperty("_nodeSoon").objectReferenceValue = ArenaTestSceneBuilder.LoadUiSprite("Map_Node_Soon.png");
            serialized.FindProperty("_starFull").objectReferenceValue = ArenaTestSceneBuilder.LoadUiSprite("Star_Full.png");
            serialized.FindProperty("_starEmpty").objectReferenceValue = ArenaTestSceneBuilder.LoadUiSprite("Star_Empty.png");
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(config);
            return config;
        }

        private static LevelNode CreateNode(RectTransform island, Font font, IslandMap map, int islandIndex, int level, Vector2Int pixel,
            RushOps.Scoring.BadgeConfig badgeConfig)
        {
            float scale = ArenaTestSceneBuilder.HudPixelScale;
            Vector2 size = island.sizeDelta / scale;
            var position = new Vector2((pixel.x - size.x * 0.5f) * scale, (size.y * 0.5f - pixel.y) * scale);
            Image image = MainMenuSceneBuilder.CreateSprite($"Node_{islandIndex + 1}-{level + 1}", island,
                ArenaTestSceneBuilder.LoadUiSprite("Map_Node_Open.png"), position);
            image.raycastTarget = true;
            var button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            var colors = button.colors;
            colors.disabledColor = Color.white;
            button.colors = colors;
            var rect = (RectTransform)image.transform;

            Text label = ArenaTestSceneBuilder.CreateText("Label", rect, font, $"{islandIndex + 1}-{level + 1}", 1, Vector2.zero);
            label.color = new Color32(13, 11, 20, 255);
            Text soon = ArenaTestSceneBuilder.CreateText("Soon", rect, font, "bald", 1, new Vector2(0f, -BelowNode + 6f));
            Text levelName = ArenaTestSceneBuilder.CreateText("Name", rect, font, string.Empty, 1,
                new Vector2(0f, -BelowNode - BadgesBelowStars - NameBelowStars));
            levelName.enabled = false;
            var stars = new Image[StarRating.MaxStars];
            Sprite empty = ArenaTestSceneBuilder.LoadUiSprite("Star_Empty.png");
            for (int i = 0; i < stars.Length; i++)
            {
                var center = new Vector2(0.5f, 0.5f);
                RectTransform star = ArenaTestSceneBuilder.CreateUiRect($"Star_{i + 1}", rect, center, center);
                star.sizeDelta = empty.rect.size;
                star.anchoredPosition = new Vector2((i - 1) * StarSpacing, -BelowNode);
                stars[i] = star.gameObject.AddComponent<Image>();
                stars[i].sprite = empty;
                stars[i].raycastTarget = false;
            }

            var badges = new Image[RushOps.Scoring.BadgeRules.PerLevel];
            for (int i = 0; i < badges.Length; i++)
            {
                var center = new Vector2(0.5f, 0.5f);
                RectTransform badge = ArenaTestSceneBuilder.CreateUiRect($"Badge_{i + 1}", rect, center, center);
                Sprite off = badgeConfig.Icon(RushOps.Scoring.BadgeRules.Order[i], false);
                badge.sizeDelta = off != null ? off.rect.size : new Vector2(16f, 16f);
                badge.anchoredPosition = new Vector2((i - 1) * BadgeSpacing, -BelowNode - BadgesBelowStars);
                badges[i] = badge.gameObject.AddComponent<Image>();
                badges[i].sprite = off;
                badges[i].raycastTarget = false;
                badges[i].enabled = false;
            }

            var node = image.gameObject.AddComponent<LevelNode>();
            UnityEventTools.AddPersistentListener(button.onClick, node.Play);
            var serialized = new SerializedObject(node);
            serialized.FindProperty("_map").objectReferenceValue = map;
            serialized.FindProperty("_island").intValue = islandIndex;
            serialized.FindProperty("_level").intValue = level;
            serialized.FindProperty("_image").objectReferenceValue = image;
            serialized.FindProperty("_button").objectReferenceValue = button;
            serialized.FindProperty("_label").objectReferenceValue = label;
            serialized.FindProperty("_soon").objectReferenceValue = soon;
            serialized.FindProperty("_name").objectReferenceValue = levelName;
            SetArray(serialized.FindProperty("_stars"), stars);
            SetArray(serialized.FindProperty("_badges"), badges);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return node;
        }

        // A larger invisible touch area around the small arrow, for thumbs.
        private static GameObject CreateArrow(string name, RectTransform parent, Vector2 position, bool down,
            UnityEngine.Events.UnityAction action)
        {
            var center = new Vector2(0.5f, 0.5f);
            RectTransform area = ArenaTestSceneBuilder.CreateUiRect(name, parent, center, center);
            area.sizeDelta = ArrowTouchArea;
            area.anchoredPosition = position;
            var hit = area.gameObject.AddComponent<Image>();
            hit.color = Color.clear;
            var button = area.gameObject.AddComponent<Button>();
            button.targetGraphic = hit;
            UnityEventTools.AddPersistentListener(button.onClick, action);

            Image arrow = MainMenuSceneBuilder.CreateSprite("Arrow", area, ArenaTestSceneBuilder.LoadUiSprite("Map_Arrow.png"), Vector2.zero);
            if (down)
            {
                arrow.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 180f);
            }

            return area.gameObject;
        }

        private static void SetArray<T>(SerializedProperty property, IList<T> items) where T : Object
        {
            property.arraySize = items.Count;
            for (int i = 0; i < items.Count; i++)
            {
                property.GetArrayElementAtIndex(i).objectReferenceValue = items[i];
            }
        }
    }
}
