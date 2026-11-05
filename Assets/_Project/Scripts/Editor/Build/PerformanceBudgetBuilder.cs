using RushOps.App;
using RushOps.Editor.Scenes;
using UnityEditor;

namespace RushOps.Editor.Build
{
    /// <summary>
    /// Creates the frame budget (issue #190) with its starting values, unless it exists.
    /// Batchmode: <c>-executeMethod RushOps.Editor.Build.PerformanceBudgetBuilder.Build</c>
    /// </summary>
    public static class PerformanceBudgetBuilder
    {
        /// <summary>Path of the budget asset.</summary>
        public const string Path = "Assets/_Project/Data/Performance/PerformanceBudget.asset";

        [MenuItem("RushOps/Daten/Leistungsbudget erzeugen")]
        public static void Build()
        {
            ArenaTestSceneBuilder.LoadOrCreateConfig<PerformanceBudget>(Path);
        }
    }
}
