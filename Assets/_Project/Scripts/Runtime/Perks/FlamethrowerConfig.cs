using UnityEngine;

namespace RushOps.Perks
{
    /// <summary>Start values of the flamethrower super weapon (F-PRK-09, F-PRK-10, issue #253, W11, W13, W14).</summary>
    [CreateAssetMenu(fileName = "FlamethrowerConfig", menuName = "RushOps/Flamethrower Config")]
    public class FlamethrowerConfig : ScriptableObject
    {
        [Tooltip("Seconds the flamethrower replaces the weapon; collecting it again restarts it (W11).")]
        [SerializeField, Min(0f)] private float _duration = 12f;

        [Tooltip("Range of the flame in tiles.")]
        [SerializeField, Min(0f)] private float _range = 3.5f;

        [Tooltip("Opening angle of the flame cone in degrees.")]
        [SerializeField, Range(1f, 180f)] private float _coneAngle = 50f;

        [Tooltip("Damage per second to every enemy in the flame, bosses included (W14).")]
        [SerializeField, Min(0f)] private float _damagePerSecond = 90f;

        [Tooltip("Seconds an enemy keeps burning after leaving the flame (W13).")]
        [SerializeField, Min(0f)] private float _afterburnDuration = 1.5f;

        [Tooltip("Damage per second while an enemy burns after the flame.")]
        [SerializeField, Min(0f)] private float _afterburnDamagePerSecond = 30f;

        [Tooltip("Seconds between two damage ticks; the damage per second is paid out in whole points at this rhythm (issue #254).")]
        [SerializeField, Min(0.02f)] private float _tickInterval = 0.2f;

        [Tooltip("Frames per second of the flame loop.")]
        [SerializeField, Min(1f)] private float _flameFrameRate = 16f;

        [Tooltip("Seconds per frame of the small fire on a burning enemy.")]
        [SerializeField, Min(0.01f)] private float _burnFrameDuration = 0.06f;

        [Tooltip("Volume of the looping flame sound.")]
        [SerializeField, Range(0f, 1f)] private float _soundVolume = 0.45f;

        /// <summary>Seconds the flamethrower stays in the hands.</summary>
        public float Duration => _duration;

        /// <summary>Flame range in tiles.</summary>
        public float Range => _range;

        /// <summary>Opening angle of the cone in degrees.</summary>
        public float ConeAngle => _coneAngle;

        /// <summary>Damage per second in the flame.</summary>
        public float DamagePerSecond => _damagePerSecond;

        /// <summary>Seconds of afterburn.</summary>
        public float AfterburnDuration => _afterburnDuration;

        /// <summary>Damage per second of the afterburn.</summary>
        public float AfterburnDamagePerSecond => _afterburnDamagePerSecond;

        /// <summary>Seconds between two damage ticks.</summary>
        public float TickInterval => _tickInterval;

        /// <summary>Frames per second of the flame loop.</summary>
        public float FlameFrameRate => _flameFrameRate;

        /// <summary>Seconds per frame of the fire on a burning enemy.</summary>
        public float BurnFrameDuration => _burnFrameDuration;

        /// <summary>Volume of the looping flame sound.</summary>
        public float SoundVolume => _soundVolume;
    }
}
