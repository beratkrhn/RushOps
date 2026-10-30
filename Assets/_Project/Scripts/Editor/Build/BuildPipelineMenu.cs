using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace RushOps.Editor.Build
{
    /// <summary>
    /// Player settings and device builds (issue #13). <see cref="ApplyPlayerSettings"/> keeps the settings
    /// decided in the Lastenheft in code, so they are reproducible; the build methods apply them first.
    /// Batchmode: <c>-executeMethod RushOps.Editor.Build.BuildPipelineMenu.BuildAndroidDevelopment</c>
    /// </summary>
    public static class BuildPipelineMenu
    {
        /// <summary>Bundle ID for iOS and Android (decided 2026-10-30).</summary>
        public const string BundleId = "com.beratkrhn.rushops";

        private const AndroidSdkVersions AndroidMinSdk = AndroidSdkVersions.AndroidApiLevel26;
        private const string IosMinVersion = "15.0";
        private const string BuildsDir = "Builds/Android";

        [MenuItem("RushOps/Build/Player-Einstellungen anwenden")]
        public static void ApplyPlayerSettings()
        {
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, BundleId);
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS, BundleId);
            PlayerSettings.Android.minSdkVersion = AndroidMinSdk;
            PlayerSettings.iOS.targetOSVersionString = IosMinVersion;

            // Portrait only, no rotation (F-PLT-02).
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;

            // 64-bit ARM is required by Google Play and covers current phones.
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            AssetDatabase.SaveAssets();
            Debug.Log($"[BuildPipelineMenu] Player settings applied: {BundleId}, Android API {(int)AndroidMinSdk}+, iOS {IosMinVersion}+");
        }

        [MenuItem("RushOps/Build/Android-APK (Development) bauen")]
        public static void BuildAndroidDevelopment()
        {
            BuildAndroid(true);
        }

        [MenuItem("RushOps/Build/Android-APK (Release) bauen")]
        public static void BuildAndroidRelease()
        {
            BuildAndroid(false);
        }

        private static void BuildAndroid(bool development)
        {
            ApplyPlayerSettings();
            Directory.CreateDirectory(BuildsDir);
            string suffix = development ? "-dev" : string.Empty;
            string path = $"{BuildsDir}/RushOps-{PlayerSettings.bundleVersion}{suffix}.apk";
            EditorUserBuildSettings.buildAppBundle = false;

            var options = new BuildPlayerOptions
            {
                scenes = EnabledScenes(),
                locationPathName = path,
                target = BuildTarget.Android,
                targetGroup = BuildTargetGroup.Android,
                options = development ? BuildOptions.Development : BuildOptions.None,
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;
            if (summary.result != BuildResult.Succeeded)
            {
                throw new BuildFailedException($"Android build {summary.result}: {summary.totalErrors} errors");
            }

            Debug.Log($"[BuildPipelineMenu] Built {path} ({summary.totalSize / (1024 * 1024)} MB in {summary.totalTime:mm\\:ss})");
        }

        private static string[] EnabledScenes()
        {
            var scenes = new System.Collections.Generic.List<string>();
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled)
                {
                    scenes.Add(scene.path);
                }
            }

            return scenes.ToArray();
        }
    }
}
