using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace RushOps.Editor.Build
{
    /// <summary>
    /// Takes the App Store screenshots (issue #192) in the open editor: runs the explicit PlayMode fixture
    /// <c>StoreScreenshots</c>, which writes five PNGs of 1320x2868 to <c>docs/store/screenshots/</c>. Without an open
    /// editor <c>Tools/store-screenshots.sh</c> does the same in batch mode.
    /// </summary>
    public static class StoreScreenshotsMenu
    {
        private const string Fixture = "RushOps.Tests.PlayMode.StoreScreenshots";

        [MenuItem("RushOps/Store/Screenshots aufnehmen")]
        public static void Take()
        {
            if (!EditorUtility.DisplayDialog("Store-Screenshots",
                    "Spielt fünf Szenen im PlayMode an und schreibt die Bilder nach docs/store/screenshots/. Offene Szene vorher speichern.",
                    "Aufnehmen", "Abbrechen"))
            {
                return;
            }

            var api = ScriptableObject.CreateInstance<TestRunnerApi>();
            // Naming the fixture selects its explicit tests.
            api.Execute(new ExecutionSettings(new Filter { testMode = TestMode.PlayMode, testNames = new[] { Fixture } }));
        }
    }
}
