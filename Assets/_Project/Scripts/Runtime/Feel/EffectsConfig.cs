using UnityEngine;

namespace RushOps.Feel
{
    /// <summary>Look and timing of the combat effects and their accent lights (issue #107, F-GFX-04).</summary>
    [CreateAssetMenu(fileName = "EffectsConfig", menuName = "RushOps/Effects Config")]
    public class EffectsConfig : ScriptableObject
    {
        [Header("Muzzle flash")]
        [Tooltip("Seconds the muzzle flash shows per shot.")]
        [SerializeField, Min(0f)] private float _muzzleFlashDuration = 0.05f;

        [Tooltip("Intensity of the muzzle flash light; 0 turns it off.")]
        [SerializeField, Min(0f)] private float _muzzleLightIntensity = 1.2f;

        [Header("Hit sparks")]
        [Tooltip("Frames of the sparks where a shot hits a wall or an enemy.")]
        [SerializeField] private Sprite[] _hitSparkFrames = new Sprite[0];

        [Tooltip("Seconds per spark frame.")]
        [SerializeField, Min(0.01f)] private float _hitSparkFrameDuration = 0.04f;

        [Header("Death")]
        [Tooltip("Frames of the splatter when an enemy without explosion dies.")]
        [SerializeField] private Sprite[] _deathFrames = new Sprite[0];

        [Tooltip("Seconds per splatter frame.")]
        [SerializeField, Min(0.01f)] private float _deathFrameDuration = 0.06f;

        /// <summary>Seconds the muzzle flash shows per shot.</summary>
        public float MuzzleFlashDuration => _muzzleFlashDuration;

        /// <summary>Intensity of the muzzle flash light.</summary>
        public float MuzzleLightIntensity => _muzzleLightIntensity;

        /// <summary>Frames of the hit sparks.</summary>
        public Sprite[] HitSparkFrames => _hitSparkFrames;

        /// <summary>Seconds per spark frame.</summary>
        public float HitSparkFrameDuration => _hitSparkFrameDuration;

        /// <summary>Frames of the death splatter.</summary>
        public Sprite[] DeathFrames => _deathFrames;

        /// <summary>Seconds per splatter frame.</summary>
        public float DeathFrameDuration => _deathFrameDuration;
    }
}
