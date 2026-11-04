using UnityEngine;

namespace RushOps.Perks
{
    /// <summary>Feedback of the nuke perk (issue #27): screen flash, camera shake and sound.</summary>
    [CreateAssetMenu(fileName = "NukeConfig", menuName = "RushOps/Nuke Config")]
    public class NukeConfig : ScriptableObject
    {
        [Header("Screen flash")]
        [Tooltip("Opacity of the white flash at its start, 0..1.")]
        [SerializeField, Range(0f, 1f)] private float _flashAlpha = 0.9f;

        [Tooltip("Seconds of real time the flash fades out over.")]
        [SerializeField, Min(0f)] private float _flashDuration = 0.5f;

        [Header("Camera shake")]
        [Tooltip("Largest camera offset in units; stronger than a boomer blast.")]
        [SerializeField, Min(0f)] private float _shakeStrength = 0.5f;

        [Tooltip("Seconds the camera shakes.")]
        [SerializeField, Min(0f)] private float _shakeDuration = 0.6f;

        [Tooltip("How fast the shake wobbles, in noise steps per second.")]
        [SerializeField, Min(0f)] private float _shakeFrequency = 30f;

        [Header("Sound")]
        [SerializeField, Range(0f, 1f)] private float _volume = 1f;

        /// <summary>Opacity of the flash at its start.</summary>
        public float FlashAlpha => _flashAlpha;

        /// <summary>Seconds the flash fades out over.</summary>
        public float FlashDuration => _flashDuration;

        /// <summary>Largest camera offset in units.</summary>
        public float ShakeStrength => _shakeStrength;

        /// <summary>Seconds the camera shakes.</summary>
        public float ShakeDuration => _shakeDuration;

        /// <summary>Noise steps per second of the shake.</summary>
        public float ShakeFrequency => _shakeFrequency;

        /// <summary>Volume of the nuke sound.</summary>
        public float Volume => _volume;
    }
}
