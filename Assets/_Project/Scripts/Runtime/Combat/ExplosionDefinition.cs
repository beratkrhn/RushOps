using UnityEngine;

namespace RushOps.Combat
{
    /// <summary>
    /// Balancing and feedback of an area explosion, e.g. the boomer's (issue #22): radius, damage to the player and
    /// to enemies, delay of chain reactions, camera shake, sound and effect frames.
    /// </summary>
    [CreateAssetMenu(fileName = "ExplosionDefinition", menuName = "RushOps/Explosion Definition")]
    public class ExplosionDefinition : ScriptableObject
    {
        [Header("Damage")]
        [Tooltip("Radius in units (tiles) around the center; everything whose body center lies inside is hit.")]
        [SerializeField, Min(0f)] private float _radius = 2.5f;

        [Tooltip("Hearts the player loses when inside the radius. Dash i-frames and the hit invulnerability protect.")]
        [SerializeField, Min(0)] private int _playerDamage = 2;

        [Tooltip("Damage to every other enemy inside the radius, in weapon hit points.")]
        [SerializeField, Min(0)] private int _enemyDamage = 50;

        [Tooltip("Seconds before an explosive enemy killed by an explosion blows up itself, so chain reactions are visible.")]
        [SerializeField, Min(0f)] private float _chainDelay = 0.12f;

        [Header("Feedback")]
        [Tooltip("Largest camera offset in units.")]
        [SerializeField, Min(0f)] private float _shakeStrength = 0.3f;

        [Tooltip("Seconds the camera shakes.")]
        [SerializeField, Min(0f)] private float _shakeDuration = 0.35f;

        [Tooltip("How fast the shake wobbles, in noise steps per second.")]
        [SerializeField, Min(0f)] private float _shakeFrequency = 22f;

        [Tooltip("Sound of the blast; none if empty.")]
        [SerializeField] private AudioClip _clip;

        [SerializeField, Range(0f, 1f)] private float _volume = 0.9f;

        [Tooltip("Frames of the blast effect, drawn for the radius above, played once.")]
        [SerializeField] private Sprite[] _effectFrames = System.Array.Empty<Sprite>();

        [Tooltip("Seconds per effect frame.")]
        [SerializeField, Min(0.01f)] private float _effectFrameDuration = 0.05f;

        /// <summary>Radius in units.</summary>
        public float Radius => _radius;

        /// <summary>Hearts the player loses inside the radius.</summary>
        public int PlayerDamage => _playerDamage;

        /// <summary>Damage to enemies inside the radius.</summary>
        public int EnemyDamage => _enemyDamage;

        /// <summary>Seconds before an explosive enemy killed by an explosion blows up itself.</summary>
        public float ChainDelay => _chainDelay;

        /// <summary>Largest camera offset in units.</summary>
        public float ShakeStrength => _shakeStrength;

        /// <summary>Seconds the camera shakes.</summary>
        public float ShakeDuration => _shakeDuration;

        /// <summary>Noise steps per second of the shake.</summary>
        public float ShakeFrequency => _shakeFrequency;

        /// <summary>Sound of the blast; may be null.</summary>
        public AudioClip Clip => _clip;

        /// <summary>Volume of the blast sound.</summary>
        public float Volume => _volume;

        /// <summary>Frames of the blast effect.</summary>
        public Sprite[] EffectFrames => _effectFrames;

        /// <summary>Seconds per effect frame.</summary>
        public float EffectFrameDuration => _effectFrameDuration;
    }
}
