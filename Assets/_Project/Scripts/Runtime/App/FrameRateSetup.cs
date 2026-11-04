using UnityEngine;

namespace RushOps.App
{
    /// <summary>
    /// Asks for 60 frames per second before the first scene loads (NF-01). Without it, iOS and Android run Unity
    /// games at 30 FPS.
    /// </summary>
    public static class FrameRateSetup
    {
        /// <summary>Target frame rate from NF-01.</summary>
        public const int TargetFrameRate = 60;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Apply()
        {
            Application.targetFrameRate = TargetFrameRate;
        }
    }
}
