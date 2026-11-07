using RushOps.Editor.Art;
using RushOps.Levels;
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
    /// Builds the <c>TestMenu</c> scene (issue #35): one button per <see cref="LevelDefinition"/> under
    /// <c>Data/Levels</c>. „Weiter“ on the result screen leads here until the level selection exists. Run it after the
    /// level builders, so their definitions exist.
    /// Batchmode: <c>-executeMethod RushOps.Editor.Scenes.TestMenuSceneBuilder.Build</c>
    /// </summary>
    public static class TestMenuSceneBuilder
    {
        /// <summary>Name of the scene, as „Weiter“ loads it.</summary>
        public const string SceneName = "TestMenu";

        private const string ScenePath = "Assets/_Project/Scenes/" + SceneName + ".unity";
        private const string LevelsDir = "Assets/_Project/Data/Levels";
        private const float ButtonGap = 12f;

        [MenuItem("RushOps/Szenen/TestMenu erzeugen")]
        public static void Build()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            LevelDefinition[] levels = LoadLevels();
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
            var rect = (RectTransform)root.transform;

            var menu = root.AddComponent<TestMenu>();
            var serialized = new SerializedObject(menu);
            SerializedProperty list = serialized.FindProperty("_levels");
            list.arraySize = levels.Length;
            for (int i = 0; i < levels.Length; i++)
            {
                list.GetArrayElementAtIndex(i).objectReferenceValue = levels[i];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();

            ArenaTestSceneBuilder.CreateText("Title", rect, font, "Testmenü", ArenaTestSceneBuilder.TitlePixelScale, new Vector2(0f, 150f));
            float step = ArenaTestSceneBuilder.MenuButtonSize.y + ButtonGap;
            for (int i = 0; i < levels.Length; i++)
            {
                CreateLevelButton(rect, font, menu, i, levels[i].DisplayName, new Vector2(0f, 70f - i * step));
            }

            CreateEndlessButtons(rect, font, menu);

            // Sound player with the mixer volumes; every button clicks (issue #108).
            var audio = new GameObject("Audio");
            ArenaTestSceneBuilder.CreateAudio(audio);
            ArenaTestSceneBuilder.AddClickSounds(audio.GetComponent<RushOps.Audio.UiClickSound>());

            EditorSceneManager.SaveScene(scene, ScenePath);
            ArenaTestSceneBuilder.AddToBuildSettings(ScenePath, false);
            AssetDatabase.SaveAssets();
            Debug.Log($"[TestMenuSceneBuilder] Saved {ScenePath} with {levels.Length} levels");
        }

        // One row of small buttons under the title: an endless run from that round on (issue #207).
        private static void CreateEndlessButtons(RectTransform parent, Font font, TestMenu menu)
        {
            int[] rounds = { 1, 5, 10, 25 };
            const float gap = 4f;
            float width = (ArenaTestSceneBuilder.MenuButtonSize.x - gap * (rounds.Length - 1)) / rounds.Length;
            var center = new Vector2(0.5f, 0.5f);
            for (int i = 0; i < rounds.Length; i++)
            {
                var go = new GameObject($"Endless_Round{rounds[i]}", typeof(RectTransform));
                var rect = (RectTransform)go.transform;
                rect.SetParent(parent, false);
                rect.anchorMin = center;
                rect.anchorMax = center;
                rect.sizeDelta = new Vector2(width, ArenaTestSceneBuilder.MenuButtonSize.y);
                float x = (i - (rounds.Length - 1) * 0.5f) * (width + gap);
                rect.anchoredPosition = new Vector2(x, 110f);
                var image = go.AddComponent<Image>();
                image.color = ArenaTestSceneBuilder.PaletteSlate2;
                var button = go.AddComponent<Button>();
                button.targetGraphic = image;
                UnityEventTools.AddIntPersistentListener(button.onClick, menu.LoadEndless, rounds[i]);
                ArenaTestSceneBuilder.CreateText("Label", rect, font, $"E{rounds[i]}", 1, Vector2.zero);
            }
        }

        private static LevelDefinition[] LoadLevels()
        {
            string[] guids = AssetDatabase.FindAssets("t:LevelDefinition", new[] { LevelsDir });
            var levels = new System.Collections.Generic.List<LevelDefinition>(guids.Length);
            foreach (string guid in guids)
            {
                var level = AssetDatabase.LoadAssetAtPath<LevelDefinition>(AssetDatabase.GUIDToAssetPath(guid));
                if (level != null)
                {
                    levels.Add(level);
                }
            }

            levels.Sort((a, b) => string.CompareOrdinal(a.SceneName, b.SceneName));
            return levels.ToArray();
        }

        private static void CreateLevelButton(RectTransform parent, Font font, TestMenu menu, int index, string label, Vector2 position)
        {
            var center = new Vector2(0.5f, 0.5f);
            var go = new GameObject($"Level_{index + 1}", typeof(RectTransform));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = center;
            rect.anchorMax = center;
            rect.sizeDelta = ArenaTestSceneBuilder.MenuButtonSize;
            rect.anchoredPosition = position;
            var image = go.AddComponent<Image>();
            image.color = ArenaTestSceneBuilder.PaletteSlate2;
            var button = go.AddComponent<Button>();
            button.targetGraphic = image;
            UnityEventTools.AddIntPersistentListener(button.onClick, menu.Load, index);
            ArenaTestSceneBuilder.CreateText("Label", rect, font, label, ArenaTestSceneBuilder.HudPixelScale, Vector2.zero);
        }
    }
}
