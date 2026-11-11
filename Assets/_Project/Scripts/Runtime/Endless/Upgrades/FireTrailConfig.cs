using UnityEngine;

namespace RushOps.Endless
{
    /// <summary>
    /// The fire trail of the dash (fire trail upgrade, F-END-12, issue #278). All values are start values; the damage
    /// matches the afterburn of the flamethrower.
    /// </summary>
    [CreateAssetMenu(fileName = "FireTrailConfig", menuName = "RushOps/Fire Trail Config")]
    public class FireTrailConfig : ScriptableObject
    {
        [Tooltip("Seconds a burning tile stays.")]
        [SerializeField, Min(0f)] private float _duration = 1.5f;

        [Tooltip("Damage per second to an enemy standing in the fire, before damage factors.")]
        [SerializeField, Min(0f)] private float _damagePerSecond = 30f;

        [Tooltip("Seconds between two payouts of whole damage points.")]
        [SerializeField, Min(0.01f)] private float _tickInterval = 0.2f;

        [Tooltip("Distance in tiles between two burning tiles along the dash.")]
        [SerializeField, Min(0.05f)] private float _spacing = 0.5f;

        [Tooltip("Radius in tiles around a burning tile in which an enemy body burns.")]
        [SerializeField, Min(0.05f)] private float _radius = 0.45f;

        [Tooltip("Most burning tiles at once; the oldest goes out first.")]
        [SerializeField, Min(1)] private int _maxTiles = 32;

        [Tooltip("Seconds per frame of the flame loop.")]
        [SerializeField, Min(0.01f)] private float _frameDuration = 0.08f;

        [Tooltip("Crackle played when a dash lays a trail.")]
        [SerializeField] private AudioClip _crackleClip;

        [SerializeField, Range(0f, 1f)] private float _crackleVolume = 0.5f;

        /// <summary>Seconds a burning tile stays.</summary>
        public float Duration => _duration;

        /// <summary>Damage per second in the fire.</summary>
        public float DamagePerSecond => _damagePerSecond;

        /// <summary>Seconds between payouts.</summary>
        public float TickInterval => _tickInterval;

        /// <summary>Distance between two tiles.</summary>
        public float Spacing => _spacing;

        /// <summary>Burning radius of a tile.</summary>
        public float Radius => _radius;

        /// <summary>Most burning tiles at once.</summary>
        public int MaxTiles => Mathf.Max(1, _maxTiles);

        /// <summary>Seconds per flame frame.</summary>
        public float FrameDuration => _frameDuration;

        /// <summary>Crackle of a dash with trail; may be null.</summary>
        public AudioClip CrackleClip => _crackleClip;

        /// <summary>Volume of the crackle.</summary>
        public float CrackleVolume => _crackleVolume;

        /// <summary>Sets the numbers, for editor scripts and tests.</summary>
        public void Configure(float duration, float damagePerSecond, float tickInterval, float spacing, float radius,
            int maxTiles, float frameDuration)
        {
            _duration = Mathf.Max(0f, duration);
            _damagePerSecond = Mathf.Max(0f, damagePerSecond);
            _tickInterval = Mathf.Max(0.01f, tickInterval);
            _spacing = Mathf.Max(0.05f, spacing);
            _radius = Mathf.Max(0.05f, radius);
            _maxTiles = Mathf.Max(1, maxTiles);
            _frameDuration = Mathf.Max(0.01f, frameDuration);
        }

        /// <summary>Sets the sound, for editor scripts.</summary>
        public void ConfigureSound(AudioClip crackle, float volume)
        {
            _crackleClip = crackle;
            _crackleVolume = Mathf.Clamp01(volume);
        }
    }
}
