#if UNITY_IOS && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif
using UnityEngine;

namespace RushOps.Feel
{
    /// <summary>
    /// Sends a pulse to the phone (issue #300): on iOS a short impact through <c>UIImpactFeedbackGenerator</c>
    /// (plugin <c>Assets/_Project/Plugins/iOS/RushOpsHaptics.mm</c>), on Android the system vibration, in the editor nothing.
    /// </summary>
    public static class HapticOutput
    {
#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void RushOps_HapticImpact(int style);
#endif

        /// <summary>Plays the pulse of <paramref name="kind"/>.</summary>
        public static void Play(HapticKind kind)
        {
#if UNITY_IOS && !UNITY_EDITOR
            // UIImpactFeedbackStyle: 1 medium, 2 heavy.
            RushOps_HapticImpact(kind == HapticKind.Damage ? 2 : 1);
#elif UNITY_ANDROID && !UNITY_EDITOR
            Handheld.Vibrate();
#endif
        }
    }
}
