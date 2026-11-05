using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace RushOps.Editor.Build
{
    /// <summary>
    /// Sets the app icon drawn by <c>Tools/gen_icon.py</c> (issue #191, decision R7) as the default icon of the
    /// player. iOS scales it to every size it needs, including the 1024 px App Store icon.
    /// Batchmode: <c>-executeMethod RushOps.Editor.Build.AppIconSetup.Apply</c>
    /// </summary>
    public static class AppIconSetup
    {
        /// <summary>Path of the 1024x1024 icon.</summary>
        public const string IconPath = "Assets/_Project/Art/Icon/AppIcon.png";

        /// <summary>Edge length Apple asks for in App Store Connect.</summary>
        public const int Size = 1024;

        [MenuItem("RushOps/Build/App-Icon setzen")]
        public static void Apply()
        {
            var icon = AssetDatabase.LoadAssetAtPath<Texture2D>(IconPath);
            if (icon == null)
            {
                throw new System.InvalidOperationException($"No icon at {IconPath}; run Tools/gen_icon.py first");
            }

            PlayerSettings.SetIcons(NamedBuildTarget.Unknown, new[] { icon }, IconKind.Any);
            AssetDatabase.SaveAssets();
            Debug.Log($"[AppIconSetup] App icon set from {IconPath}");
        }
    }
}
