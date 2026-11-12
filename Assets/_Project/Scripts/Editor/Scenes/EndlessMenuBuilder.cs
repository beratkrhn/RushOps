using RushOps.Endless;
using RushOps.Save;
using RushOps.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace RushOps.Editor.Scenes
{
    /// <summary>
    /// Builds the area „Endlos“ of the main menu (issue #280, F-UI-02, F-END-14, N22): „Starten“ and below it
    /// „Tageslauf“ (two menu buttons do not fit side by side in portrait), today's best, the list of the last 7 days and
    /// „Zurück“, wired to the <see cref="EndlessMenu"/>.
    /// </summary>
    internal static class EndlessMenuBuilder
    {
        private const float TitleY = 238f;
        // Switch „Darstellung: Normal | Arcade“ (issue #325): caption above a button that shows the choice.
        private const float DisplayTitleY = 200f;
        private const float DisplayY = 164f;
        private const float StartY = 112f;
        private const float DailyY = 62f;
        private const float TodayY = 22f;
        private const float ListTitleY = -12f;
        private const float HeadingY = -34f;
        private const float ColumnTop = -44f;
        private const float BackY = -200f;
        private static readonly float[] ColumnX = { -85f, 15f, 95f };

        public static EndlessMenu Create(RectTransform safeArea, GameObject root, Font font, MainMenu menu)
        {
            RectTransform panel = ArenaTestSceneBuilder.CreateUiRect("EndlessPanel", safeArea, Vector2.zero, Vector2.one);
            var background = panel.gameObject.AddComponent<Image>();
            Color color = ArenaTestSceneBuilder.PaletteNight;
            color.a = 1f;
            background.color = color;

            var endless = root.AddComponent<EndlessMenu>();
            ArenaTestSceneBuilder.CreateText("Title", panel, font, "ENDLOS", ArenaTestSceneBuilder.TitlePixelScale,
                new Vector2(0f, TitleY));
            ArenaTestSceneBuilder.CreateText("DisplayTitle", panel, font, EndlessText.DisplayTitle,
                ArenaTestSceneBuilder.HudPixelScale, new Vector2(0f, DisplayTitleY));
            Button display = MainMenuSceneBuilder.CreateButton("DisplayButton", panel, font, EndlessText.Display(false),
                new Vector2(0f, DisplayY), false, true, endless.ToggleDisplay);
            MainMenuSceneBuilder.CreateButton("StartButton", panel, font, "Starten", new Vector2(0f, StartY), true, true,
                menu.StartEndless);
            MainMenuSceneBuilder.CreateButton("DailyButton", panel, font, EndlessText.Daily, new Vector2(0f, DailyY), false, true,
                menu.StartDaily);
            Text today = ArenaTestSceneBuilder.CreateText("Today", panel, font, EndlessText.DailyToday(null),
                ArenaTestSceneBuilder.HudPixelScale, new Vector2(0f, TodayY));
            Text listTitle = ArenaTestSceneBuilder.CreateText("LastDaysTitle", panel, font, EndlessText.LastDaysTitle,
                ArenaTestSceneBuilder.HudPixelScale, new Vector2(0f, ListTitleY));
            listTitle.color = ArenaTestSceneBuilder.PaletteYellow;

            var columns = new Text[EndlessText.DailyHeadings.Length];
            for (int i = 0; i < columns.Length; i++)
            {
                Text heading = ArenaTestSceneBuilder.CreateText($"Heading{i}", panel, font, EndlessText.DailyHeadings[i], 1,
                    new Vector2(ColumnX[i], HeadingY));
                heading.color = ArenaTestSceneBuilder.PaletteSlate2;
                columns[i] = ArenaTestSceneBuilder.CreateText($"Column{i}", panel, font, "-", 1, new Vector2(ColumnX[i], ColumnTop));
                columns[i].rectTransform.pivot = new Vector2(0.5f, 1f);
                columns[i].alignment = TextAnchor.UpperCenter;
            }

            MainMenuSceneBuilder.CreateButton("BackButton", panel, font, "Zurück", new Vector2(0f, BackY), true, true,
                endless.Close);

            var serialized = new SerializedObject(endless);
            serialized.FindProperty("_save").objectReferenceValue =
                ArenaTestSceneBuilder.LoadOrCreateConfig<SaveGameService>(ArenaTestSceneBuilder.SaveGameServicePath);
            serialized.FindProperty("_panel").objectReferenceValue = panel.gameObject;
            serialized.FindProperty("_today").objectReferenceValue = today;
            serialized.FindProperty("_display").objectReferenceValue = display.GetComponentInChildren<Text>();
            SerializedProperty list = serialized.FindProperty("_columns");
            list.arraySize = columns.Length;
            for (int i = 0; i < columns.Length; i++)
            {
                list.GetArrayElementAtIndex(i).objectReferenceValue = columns[i];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            panel.gameObject.SetActive(false);
            return endless;
        }
    }
}
