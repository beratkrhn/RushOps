using UnityEngine;

namespace RushOps.Perks
{
    /// <summary>
    /// Look and push of the shield perk (issue #26). Its duration comes from the shield's
    /// <see cref="PerkDefinition"/>.
    /// </summary>
    [CreateAssetMenu(fileName = "ShieldConfig", menuName = "RushOps/Shield Config")]
    public class ShieldConfig : ScriptableObject
    {
        [Tooltip("Seconds before the shield ends during which the barrier flickers.")]
        [SerializeField, Min(0f)] private float _warningTime = 2f;

        [Tooltip("Seconds the barrier stays on or off while it flickers.")]
        [SerializeField, Min(0f)] private float _blinkInterval = 0.1f;

        [Tooltip("Speed in units per second an enemy touching the shield is pushed away.")]
        [SerializeField, Min(0f)] private float _pushSpeed = 5f;

        [Tooltip("Seconds the push lasts; it eases out over this time.")]
        [SerializeField, Min(0f)] private float _pushDuration = 0.15f;

        /// <summary>Seconds before the end during which the barrier flickers.</summary>
        public float WarningTime => _warningTime;

        /// <summary>Seconds the barrier stays on or off while it flickers.</summary>
        public float BlinkInterval => _blinkInterval;

        /// <summary>Push speed of a touching enemy in units per second.</summary>
        public float PushSpeed => _pushSpeed;

        /// <summary>Seconds the push lasts.</summary>
        public float PushDuration => _pushDuration;
    }
}
