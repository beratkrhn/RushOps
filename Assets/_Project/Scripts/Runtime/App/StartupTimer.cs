using RushOps.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RushOps.App
{
    /// <summary>
    /// Logs the time from app start to the first main menu in development builds on the device (NF-05, issue #189).
    /// The editor counts from its own start, so it logs nothing there.
    /// </summary>
    public static class StartupTimer
    {
        /// <summary>Seconds from app start to the first main menu; negative until it loaded.</summary>
        public static float SecondsToMainMenu { get; private set; } = -1f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Hook()
        {
            if (!Debug.isDebugBuild || Application.isEditor)
            {
                return;
            }

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != MainMenu.SceneName)
            {
                return;
            }

            SceneManager.sceneLoaded -= OnSceneLoaded;
            SecondsToMainMenu = Time.realtimeSinceStartup;
            Debug.Log($"[RushOps] Hauptmenü nach {SecondsToMainMenu:0.00} s geladen (NF-05: unter 5 s)");
        }
    }
}
