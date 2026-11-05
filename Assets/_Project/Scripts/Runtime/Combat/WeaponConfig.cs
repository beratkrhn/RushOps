using UnityEngine;

namespace RushOps.Combat
{
    /// <summary>
    /// Settings shared by all weapons: the start weapon and the aim assist (F-STE-06). The values of a single weapon
    /// live in its <see cref="WeaponDefinition"/> (issue #173).
    /// </summary>
    [CreateAssetMenu(fileName = "WeaponConfig", menuName = "RushOps/Weapon Config")]
    public class WeaponConfig : ScriptableObject
    {
        [Tooltip("Weapon in the first slot when a level starts.")]
        [SerializeField] private WeaponDefinition _startWeapon;

        [Header("Aim assist (F-STE-06)")]
        [Tooltip("Correct shots towards an enemy close to the aim direction.")]
        [SerializeField] private bool _aimAssistEnabled = true;

        [Tooltip("Enemies at most this many degrees off the aim direction are aimed at.")]
        [SerializeField, Range(0f, 45f)] private float _aimAssistAngle = 10f;

        [Tooltip("Share of the active weapon's range within which enemies are aimed at.")]
        [SerializeField, Range(0f, 1f)] private float _aimAssistRangeFactor = 0.8f;

        /// <summary>Weapon in the first slot when a level starts.</summary>
        public WeaponDefinition StartWeapon => _startWeapon;

        /// <summary>True if shots are corrected towards enemies close to the aim direction.</summary>
        public bool AimAssistEnabled => _aimAssistEnabled;

        /// <summary>Largest angle in degrees between aim direction and an enemy the assist corrects to.</summary>
        public float AimAssistAngle => _aimAssistAngle;

        /// <summary>Share of the active weapon's range within which the assist corrects to enemies.</summary>
        public float AimAssistRangeFactor => _aimAssistRangeFactor;
    }
}
