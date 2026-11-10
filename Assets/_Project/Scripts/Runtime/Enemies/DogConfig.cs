using System;
using UnityEngine;

namespace RushOps.Enemies
{
    /// <summary>
    /// Pack and lunge values of the zombie dog (F-GEG-05, F-GEG-06, decisions H2, H4, H5). All are start values;
    /// <see cref="DogLunge"/> and <see cref="PackRules"/> apply them.
    /// </summary>
    [CreateAssetMenu(fileName = "DogConfig", menuName = "RushOps/Dog Config")]
    public class DogConfig : ScriptableObject
    {
        [Header("Pack")]
        [Tooltip("Dogs that spawn together; a pack takes this many places of the enemy limit (H4).")]
        [SerializeField, Min(1)] private int _packSize = 3;

        [Tooltip("Distance in units (tiles) of the dogs of a pack from their common spawn point (H5).")]
        [SerializeField, Min(0f)] private float _packSpread = 0.5f;

        [Header("Lunge")]
        [Tooltip("Distance to the player in units (tiles) at which a chasing dog crouches, edge included.")]
        [SerializeField, Min(0f)] private float _lungeTriggerDistance = 3f;

        [Tooltip("Seconds the dog crouches and stands still before it lunges (F-GEG-06).")]
        [SerializeField, Min(0f)] private float _crouchDuration = 0.4f;

        [Tooltip("Longest lunge in units (tiles); the lunge stops earlier at the player's old position.")]
        [SerializeField, Min(0f)] private float _lungeDistance = 3f;

        [Tooltip("Seconds a lunge over the full lunge distance takes; sets the lunge speed.")]
        [SerializeField, Min(0.01f)] private float _lungeDuration = 0.3f;

        [Tooltip("Seconds the dog stands still after a lunge before it chases again.")]
        [SerializeField, Min(0f)] private float _recoveryDuration = 1.5f;

        /// <summary>Dogs per pack, at least 1.</summary>
        public int PackSize => Math.Max(1, _packSize);

        /// <summary>Distance of the dogs of a pack from their common spawn point.</summary>
        public float PackSpread => _packSpread;

        /// <summary>Distance to the player at which a chasing dog crouches, edge included.</summary>
        public float LungeTriggerDistance => _lungeTriggerDistance;

        /// <summary>Seconds of crouching before the lunge.</summary>
        public float CrouchDuration => _crouchDuration;

        /// <summary>Longest lunge in units.</summary>
        public float LungeDistance => _lungeDistance;

        /// <summary>Seconds a lunge over <see cref="LungeDistance"/> takes, above 0.</summary>
        public float LungeDuration => Mathf.Max(0.01f, _lungeDuration);

        /// <summary>Lunge speed in units per second.</summary>
        public float LungeSpeed => _lungeDistance / LungeDuration;

        /// <summary>Seconds of standing still after a lunge.</summary>
        public float RecoveryDuration => _recoveryDuration;

        /// <summary>Sets the pack values, for editor scripts and tests.</summary>
        public void ConfigurePack(int packSize, float packSpread)
        {
            _packSize = Math.Max(1, packSize);
            _packSpread = Mathf.Max(0f, packSpread);
        }

        /// <summary>Sets the lunge values, for editor scripts and tests.</summary>
        public void ConfigureLunge(float triggerDistance, float crouchDuration, float lungeDistance, float lungeDuration,
            float recoveryDuration)
        {
            _lungeTriggerDistance = Mathf.Max(0f, triggerDistance);
            _crouchDuration = Mathf.Max(0f, crouchDuration);
            _lungeDistance = Mathf.Max(0f, lungeDistance);
            _lungeDuration = Mathf.Max(0.01f, lungeDuration);
            _recoveryDuration = Mathf.Max(0f, recoveryDuration);
        }
    }
}
