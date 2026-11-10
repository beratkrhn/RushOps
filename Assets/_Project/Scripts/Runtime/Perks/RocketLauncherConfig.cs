using UnityEngine;

namespace RushOps.Perks
{
    /// <summary>Start values of the rocket launcher super weapon (F-PRK-09, F-PRK-11, F-PRK-12, issue #253, W11, W12, W14).</summary>
    [CreateAssetMenu(fileName = "RocketLauncherConfig", menuName = "RushOps/Rocket Launcher Config")]
    public class RocketLauncherConfig : ScriptableObject
    {
        [Tooltip("Seconds the launcher replaces the weapon; collecting it again restarts it (W11).")]
        [SerializeField, Min(0f)] private float _duration = 12f;

        [Tooltip("Rockets per second.")]
        [SerializeField, Min(0f)] private float _fireRate = 1.5f;

        [Tooltip("Speed of a rocket in tiles per second.")]
        [SerializeField, Min(0f)] private float _rocketSpeed = 9f;

        [Tooltip("Range in tiles in which the launcher picks a target and a rocket flies.")]
        [SerializeField, Min(0f)] private float _range = 10f;

        [Tooltip("Radius in tiles of the explosion.")]
        [SerializeField, Min(0f)] private float _explosionRadius = 1.6f;

        [Tooltip("Damage the explosion deals to every enemy in the radius, bosses included (W14). Never hurts the player (W12).")]
        [SerializeField, Min(0)] private int _explosionDamage = 40;

        [Tooltip("Seconds per frame of the explosion animation.")]
        [SerializeField, Min(0.01f)] private float _explosionFrameDuration = 0.06f;

        [Tooltip("Volume of the launch sound, 0 to 1.")]
        [SerializeField, Range(0f, 1f)] private float _launchVolume = 0.7f;

        [Tooltip("Volume of the explosion sound, 0 to 1.")]
        [SerializeField, Range(0f, 1f)] private float _explosionVolume = 0.8f;

        /// <summary>Seconds the launcher stays in the hands.</summary>
        public float Duration => _duration;

        /// <summary>Rockets per second.</summary>
        public float FireRate => _fireRate;

        /// <summary>Rocket speed in tiles per second.</summary>
        public float RocketSpeed => _rocketSpeed;

        /// <summary>Target and flight range in tiles.</summary>
        public float Range => _range;

        /// <summary>Explosion radius in tiles.</summary>
        public float ExplosionRadius => _explosionRadius;

        /// <summary>Explosion damage to enemies.</summary>
        public int ExplosionDamage => _explosionDamage;

        /// <summary>Seconds per frame of the explosion animation.</summary>
        public float ExplosionFrameDuration => _explosionFrameDuration;

        /// <summary>Volume of the launch sound.</summary>
        public float LaunchVolume => _launchVolume;

        /// <summary>Volume of the explosion sound.</summary>
        public float ExplosionVolume => _explosionVolume;
    }
}
