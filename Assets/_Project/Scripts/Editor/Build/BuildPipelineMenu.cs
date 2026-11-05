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
    /// Batchmode: <c>-executeMethod RushOps.Editor.Build.BuildPipelineMenu.BuildAndroidDevelopment</c> or
    /// <c>BuildIosDevelopment</c>; <c>Tools/ios-install.sh</c> signs the Xcode project and installs it on an iPhone.
    /// </summary>
    public static class BuildPipelineMenu
    {
        /// <summary>Bundle ID for iOS and Android (decided 2026-10-30).</summary>
        public const string BundleId = "com.beratkrhn.rushops";

        private const AndroidSdkVersions AndroidMinSdk = AndroidSdkVersions.AndroidApiLevel26;
        private const string IosMinVersion = "15.0";
        private const string BuildsDir = "Builds/Android";
        private const string IosBuildsDir = "Builds/iOS";

        /// <summary>Apple team that signs development builds (personal team of the developer account).</summary>
        public const string AppleTeamId = "WAM6U438Y8";

        [MenuItem("RushOps/Build/Player-Einstellungen anwenden")]
        public static void ApplyPlayerSettings()
        {
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, BundleId);
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS, BundleId);
            PlayerSettings.Android.minSdkVersion = AndroidMinSdk;
            PlayerSettings.iOS.targetOSVersionString = IosMinVersion;
            PlayerSettings.iOS.appleDeveloperTeamID = AppleTeamId;
            PlayerSettings.iOS.appleEnableAutomaticSigning = true;

            // Portrait only, no rotation (F-PLT-02).
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;

            // 64-bit ARM is required by Google Play and covers current phones.
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            AppIconSetup.Apply();
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

        [MenuItem("RushOps/Build/iOS-Xcode-Projekt (Development) bauen")]
        public static void BuildIosDevelopment()
        {
            BuildIos(true);
        }

        [MenuItem("RushOps/Build/iOS-Xcode-Projekt (Release) bauen")]
        public static void BuildIosRelease()
        {
            BuildIos(false);
        }

        // Unity exports an Xcode project; Xcode or Tools/ios-install.sh compiles, signs and installs it.
        private static void BuildIos(bool development)
        {
            ApplyPlayerSettings();
            string suffix = development ? "-dev" : string.Empty;
            string path = $"{IosBuildsDir}/RushOps-{PlayerSettings.bundleVersion}{suffix}";
            Directory.CreateDirectory(IosBuildsDir);
            Run(new BuildPlayerOptions
            {
                scenes = EnabledScenes(),
                locationPathName = path,
                target = BuildTarget.iOS,
                targetGroup = BuildTargetGroup.iOS,
                options = development ? BuildOptions.Development : BuildOptions.None,
            }, "iOS", path);
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

            Run(options, "Android", path);
        }

        private static void Run(BuildPlayerOptions options, string platform, string path)
        {
            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;
            if (summary.result != BuildResult.Succeeded)
            {
                throw new BuildFailedException($"{platform} build {summary.result}: {summary.totalErrors} errors");
            }

            Debug.Log($"[BuildPipelineMenu] Built {path} ({summary.totalSize / (1024 * 1024)} MB in {summary.totalTime:mm\\:ss})");
        }

        // The start scene with the logo comes first (issue #124); the test menu stays in the build for developers.
        private static string[] EnabledScenes()
        {
            var scenes = new System.Collections.Generic.List<string>();
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (!scene.enabled)
                {
                    continue;
                }

                if (Path.GetFileNameWithoutExtension(scene.path) == Scenes.BootSceneBuilder.SceneName)
                {
                    scenes.Insert(0, scene.path);
                }
                else
                {
                    scenes.Add(scene.path);
                }
            }

            return scenes.ToArray();
        }
    }
}
