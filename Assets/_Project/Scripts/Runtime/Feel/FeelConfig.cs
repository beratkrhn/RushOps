using UnityEngine;

namespace RushOps.Feel
{
    /// <summary>Strengths and durations of the hit feedback (issue #12).</summary>
    [CreateAssetMenu(fileName = "FeelConfig", menuName = "RushOps/Feel Config")]
    public class FeelConfig : ScriptableObject
    {
        [Header("Enemy hit")]
        [Tooltip("Seconds an enemy shows white after a hit.")]
        [SerializeField, Min(0f)] private float _hitFlashDuration = 0.08f;

        [Tooltip("Speed in units per second an enemy is pushed back along the shot direction.")]
        [SerializeField, Min(0f)] private float _knockbackSpeed = 4f;

        [Tooltip("Seconds the push back lasts; it eases out over this time.")]
        [SerializeField, Min(0f)] private float _knockbackDuration = 0.1f;

        [Header("Player damage")]
        [Tooltip("Largest camera offset in units when the player takes damage.")]
        [SerializeField, Min(0f)] private float _shakeStrength = 0.15f;

        [Tooltip("Seconds the camera shakes.")]
        [SerializeField, Min(0f)] private float _shakeDuration = 0.2f;

        [Tooltip("How fast the shake wobbles, in noise steps per second.")]
        [SerializeField, Min(0f)] private float _shakeFrequency = 25f;

        [Header("Enemy death")]
        [Tooltip("Freeze the game for a moment when an enemy dies.")]
        [SerializeField] private bool _hitstopEnabled = true;

        [Tooltip("Seconds of the freeze in real time, about 3 frames at 60 FPS.")]
        [SerializeField, Min(0f)] private float _hitstopDuration = 0.05f;

        [Header("Sound")]
        [SerializeField, Range(0f, 1f)] private float _shotVolume = 0.35f;
        [SerializeField, Range(0f, 1f)] private float _hitVolume = 0.6f;
        [SerializeField, Range(0f, 1f)] private float _deathVolume = 0.8f;

        [Tooltip("Random pitch change per sound, e.g. 0.06 for up to 6 percent up or down.")]
        [SerializeField, Range(0f, 0.5f)] private float _pitchVariation = 0.06f;

        /// <summary>Seconds an enemy shows white after a hit.</summary>
        public float HitFlashDuration => _hitFlashDuration;

        /// <summary>Push back speed of a hit enemy in units per second.</summary>
        public float KnockbackSpeed => _knockbackSpeed;

        /// <summary>Seconds the push back lasts.</summary>
        public float KnockbackDuration => _knockbackDuration;

        /// <summary>Largest camera offset in units when the player takes damage.</summary>
        public float ShakeStrength => _shakeStrength;

        /// <summary>Seconds the camera shakes.</summary>
        public float ShakeDuration => _shakeDuration;

        /// <summary>Noise steps per second of the shake.</summary>
        public float ShakeFrequency => _shakeFrequency;

        /// <summary>True if an enemy death freezes the game for a moment.</summary>
        public bool HitstopEnabled => _hitstopEnabled;

        /// <summary>Seconds of the freeze in real time.</summary>
        public float HitstopDuration => _hitstopDuration;

        /// <summary>Volume of a shot.</summary>
        public float ShotVolume => _shotVolume;

        /// <summary>Volume of an enemy hit.</summary>
        public float HitVolume => _hitVolume;

        /// <summary>Volume of an enemy death.</summary>
        public float DeathVolume => _deathVolume;

        /// <summary>Random pitch change per sound as a fraction.</summary>
        public float PitchVariation => _pitchVariation;
    }
}
