using System;
using System.IO;
using RushOps.Editor.Scenes;
using RushOps.Story;
using UnityEditor;
using UnityEngine;

namespace RushOps.Editor.Story
{
    /// <summary>
    /// Turns the intro panels of <c>Tools/gen_cutscenes.py</c> (issue #332) into <c>Data/Story/IntroCutscene.asset</c>:
    /// reads <c>Art/Cutscenes/Intro/Intro_Cutscene.json</c> with the German texts and the box layout and links the
    /// panel sprites. Run it after the script.
    /// Batchmode: <c>-executeMethod RushOps.Editor.Story.IntroCutsceneBuilder.Build</c>
    /// </summary>
    public static class IntroCutsceneBuilder
    {
        /// <summary>Folder of the intro panels and their JSON.</summary>
        public const string ArtDir = "Assets/_Project/Art/Cutscenes/Intro";

        /// <summary>Path of the intro cutscene asset.</summary>
        public const string CutscenePath = "Assets/_Project/Data/Story/IntroCutscene.asset";

        private const string JsonFile = "Intro_Cutscene.json";

        /// <summary>Builds or updates the intro cutscene asset and returns it.</summary>
        [MenuItem("RushOps/Story/Intro-Cutscene aktualisieren")]
        public static CutsceneDefinition Build()
        {
            string jsonPath = $"{ArtDir}/{JsonFile}";
            if (!File.Exists(jsonPath))
            {
                throw new InvalidOperationException($"{jsonPath} not found; run Tools/gen_cutscenes.py");
            }

            var data = JsonUtility.FromJson<CutsceneJson>(File.ReadAllText(jsonPath));
            var panels = new CutscenePanel[data.panels.Length];
            for (int i = 0; i < panels.Length; i++)
            {
                PanelJson panel = data.panels[i];
                string imagePath = $"{ArtDir}/{panel.image}";
                var image = AssetDatabase.LoadAssetAtPath<Sprite>(imagePath);
                if (image == null)
                {
                    throw new InvalidOperationException($"Panel sprite not found at {imagePath}");
                }

                var boxes = new CutsceneBox[panel.boxes.Length];
                for (int b = 0; b < boxes.Length; b++)
                {
                    BoxJson box = panel.boxes[b];
                    boxes[b] = new CutsceneBox(ParseKind(box.kind), box.speaker, ParseColor(box.speakerColor), box.text,
                        new Vector2Int(box.x, box.y), box.width);
                }

                panels[i] = new CutscenePanel(image, boxes);
            }

            CutsceneDefinition cutscene = ArenaTestSceneBuilder.LoadOrCreateConfig<CutsceneDefinition>(CutscenePath);
            cutscene.SetContent(new Vector2Int(data.panelWidth, data.panelHeight), panels);
            EditorUtility.SetDirty(cutscene);
            AssetDatabase.SaveAssets();
            Debug.Log($"[IntroCutsceneBuilder] {panels.Length} panels in {CutscenePath}");
            return cutscene;
        }

        private static CutsceneBoxKind ParseKind(string kind)
        {
            return Enum.TryParse(kind, out CutsceneBoxKind parsed) ? parsed : CutsceneBoxKind.Narration;
        }

        private static Color32 ParseColor(string hex)
        {
            return ColorUtility.TryParseHtmlString("#" + hex, out Color color) ? (Color32)color : new Color32(200, 50, 60, 255);
        }

#pragma warning disable 0649 // filled by JsonUtility
        [Serializable]
        private class CutsceneJson
        {
            public int panelWidth;
            public int panelHeight;
            public PanelJson[] panels;
        }

        [Serializable]
        private class PanelJson
        {
            public string image;
            public BoxJson[] boxes;
        }

        [Serializable]
        private class BoxJson
        {
            public string kind;
            public string speaker;
            public string speakerColor;
            public string text;
            public int x;
            public int y;
            public int width;
        }
#pragma warning restore 0649
    }
}
