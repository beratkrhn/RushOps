using RushOps.Editor.Art;
using RushOps.Editor.Scenes;
using RushOps.Story;
using RushOps.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RushOps.Editor.Story
{
    /// <summary>
    /// Builds the <c>Intro</c> scene (issue #332, F-STO-02): a black camera, a reference-sized canvas with the
    /// <see cref="CutscenePlayer"/> playing <c>IntroCutscene.asset</c>, and <see cref="IntroScene"/>, which opens the
    /// island map at the end. Updates the intro cutscene asset first.
    /// Batchmode: <c>-executeMethod RushOps.Editor.Story.IntroSceneBuilder.Build</c>
    /// </summary>
    public static class IntroSceneBuilder
    {
        /// <summary>Path of the intro scene.</summary>
        public const string ScenePath = "Assets/_Project/Scenes/" + IntroScene.SceneName + ".unity";

        // The panels are 180x320 and the canvas 360x640: every panel pixel is two reference units.
        private const int PanelScale = 2;
        private static readonly Color Background = new Color32(13, 11, 20, 255);

        [MenuItem("RushOps/Szenen/Intro erzeugen")]
        public static void Build()
        {
            IntroCutsceneBuilder.Build();
            // A new scene unloads unused assets, so the cutscene and its config are loaded only after it.
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var cutscene = AssetDatabase.LoadAssetAtPath<CutsceneDefinition>(IntroCutsceneBuilder.CutscenePath);
            CutsceneConfig config = StoryAssetsBuilder.LoadOrCreateCutsceneConfig();
            Font font = PixelFontBuilder.Build();

            var cameraGo = new GameObject("Main Camera");
            cameraGo.tag = "MainCamera";
            var camera = cameraGo.AddComponent<Camera>();
            camera.orthographic = true;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Background;

            var eventSystem = new GameObject("EventSystem", typeof(EventSystem));
            ArenaTestSceneBuilder.ForgetCachedDefaultUiActions();
            eventSystem.AddComponent<InputSystemUIInputModule>().AssignDefaultActions();

            var root = new GameObject("Intro", typeof(RectTransform));
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = true;
            var scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(ArenaTestSceneBuilder.ReferenceWidth, ArenaTestSceneBuilder.ReferenceHeight);
            scaler.matchWidthOrHeight = 0f;
            root.AddComponent<GraphicRaycaster>();

            CutscenePlayer player = CutscenePlayer.Create((RectTransform)root.transform, font, PixelFontBuilder.NativeSize,
                config, PanelScale, Background);
            var playerSettings = new SerializedObject(player);
            playerSettings.FindProperty("_cutscene").objectReferenceValue = cutscene;
            playerSettings.ApplyModifiedPropertiesWithoutUndo();

            var intro = root.AddComponent<IntroScene>();
            var introSettings = new SerializedObject(intro);
            introSettings.FindProperty("_player").objectReferenceValue = player;
            introSettings.FindProperty("_nextScene").stringValue = IslandMap.SceneName;
            introSettings.FindProperty("_save").objectReferenceValue =
                ArenaTestSceneBuilder.LoadOrCreateConfig<RushOps.Save.SaveGameService>(ArenaTestSceneBuilder.SaveGameServicePath);
            introSettings.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene, ScenePath);
            ArenaTestSceneBuilder.AddToBuildSettings(ScenePath, false);
            AssetDatabase.SaveAssets();
            Debug.Log($"[IntroSceneBuilder] Saved {ScenePath}");
        }
    }
}
