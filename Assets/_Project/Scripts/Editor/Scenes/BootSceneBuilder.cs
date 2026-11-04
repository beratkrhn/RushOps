using System.Collections.Generic;
using RushOps.App;
using RushOps.Save;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RushOps.Editor.Scenes
{
    /// <summary>
    /// Builds the start scene <c>Boot</c> (issue #124): dark screen with the RUSHOPS logo in the middle, a black cover
    /// for the fade and the <see cref="IntroSequence"/>. The scene is the first in the build settings.
    /// Batchmode: <c>-executeMethod RushOps.Editor.Scenes.BootSceneBuilder.Build</c>
    /// </summary>
    public static class BootSceneBuilder
    {
        /// <summary>Name of the start scene.</summary>
        public const string SceneName = "Boot";

        private const string ScenePath = "Assets/_Project/Scenes/" + SceneName + ".unity";
        private const string IntroConfigPath = "Assets/_Project/Data/UI/IntroConfig.asset";
        private const string LogoFramePath = "Assets/_Project/Art/Sprites/UI/Logo_{0}.png";
        private const float LogoOffsetY = 40f;
        private static readonly Color CoverColor = new Color32(13, 11, 20, 0);

        [MenuItem("RushOps/Szenen/Boot erzeugen")]
        public static void Build()
        {
            IntroConfig config = LoadOrCreateIntroConfig();
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var cameraGo = new GameObject("Main Camera");
            cameraGo.tag = "MainCamera";
            var camera = cameraGo.AddComponent<Camera>();
            camera.orthographic = true;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = ArenaTestSceneBuilder.PaletteNight;

            var root = new GameObject("Intro", typeof(RectTransform));
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = true;
            var scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(ArenaTestSceneBuilder.ReferenceWidth, ArenaTestSceneBuilder.ReferenceHeight);
            scaler.matchWidthOrHeight = 0f;
            var rect = (RectTransform)root.transform;

            Sprite first = config.Frames.Length > 0 ? config.Frames[0] : null;
            Image logo = CreateImage("Logo", rect, first, Color.white);
            var logoRect = (RectTransform)logo.transform;
            if (first != null)
            {
                logoRect.sizeDelta = first.rect.size * ArenaTestSceneBuilder.HudPixelScale;
            }

            logoRect.anchoredPosition = new Vector2(0f, LogoOffsetY);

            Image cover = CreateImage("Cover", rect, null, CoverColor);
            var coverRect = (RectTransform)cover.transform;
            coverRect.anchorMin = Vector2.zero;
            coverRect.anchorMax = Vector2.one;
            coverRect.sizeDelta = Vector2.zero;

            var sequence = root.AddComponent<IntroSequence>();
            var serialized = new SerializedObject(sequence);
            serialized.FindProperty("_config").objectReferenceValue = config;
            serialized.FindProperty("_save").objectReferenceValue =
                ArenaTestSceneBuilder.LoadOrCreateConfig<SaveGameService>(ArenaTestSceneBuilder.SaveGameServicePath);
            serialized.FindProperty("_logo").objectReferenceValue = logo;
            serialized.FindProperty("_cover").objectReferenceValue = cover;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene, ScenePath);
            MakeFirstInBuild(ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log($"[BootSceneBuilder] Saved {ScenePath} with {config.Frames.Length} logo frames");
        }

        // The frames are generated art; the timing values stay as they are once tuned.
        private static IntroConfig LoadOrCreateIntroConfig()
        {
            var config = ArenaTestSceneBuilder.LoadOrCreateConfig<IntroConfig>(IntroConfigPath);
            var frames = new List<Sprite>();
            for (int i = 0; ; i++)
            {
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(string.Format(LogoFramePath, i));
                if (sprite == null)
                {
                    break;
                }

                frames.Add(sprite);
            }

            var serialized = new SerializedObject(config);
            SerializedProperty list = serialized.FindProperty("_frames");
            list.arraySize = frames.Count;
            for (int i = 0; i < frames.Count; i++)
            {
                list.GetArrayElementAtIndex(i).objectReferenceValue = frames[i];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(config);
            return config;
        }

        private static Image CreateImage(string name, RectTransform parent, Sprite sprite, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            var image = go.AddComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static void MakeFirstInBuild(string path)
        {
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            scenes.RemoveAll(s => s.path == path);
            scenes.Insert(0, new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
