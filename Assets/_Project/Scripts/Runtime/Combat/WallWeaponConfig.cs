using UnityEngine;

namespace RushOps.Combat
{
    /// <summary>Values of the weapons hanging on walls (issue #175, decision W8).</summary>
    [CreateAssetMenu(fileName = "WallWeaponConfig", menuName = "RushOps/Wall Weapon Config")]
    public class WallWeaponConfig : ScriptableObject
    {
        [Tooltip("Seconds the player stands in front of a wall weapon until it is picked up (W8).")]
        [SerializeField, Min(0f)] private float _pickupTime = 0.5f;

        [Tooltip("Distance in units from the spot in front of the weapon within which the ring fills.")]
        [SerializeField, Min(0.1f)] private float _pickupRadius = 0.8f;

        [Tooltip("Tint of the silhouette while the weapon is locked: a dark shape (endless mode, issue #209).")]
        [SerializeField] private Color _lockedTint = new Color(0.15f, 0.15f, 0.2f, 1f);

        [Tooltip("Tint the silhouette flashes with when the weapon unlocks, fading to white.")]
        [SerializeField] private Color _unlockGlowTint = new Color(1f, 0.92f, 0.45f, 1f);

        [Tooltip("Seconds the unlock glow lasts.")]
        [SerializeField, Min(0f)] private float _unlockGlowSeconds = 0.8f;

        /// <summary>Seconds in front of the weapon until it is picked up.</summary>
        public float PickupTime => _pickupTime;

        /// <summary>Distance from the spot in front of the weapon within which the ring fills.</summary>
        public float PickupRadius => _pickupRadius;

        /// <summary>Tint of the silhouette of a locked weapon.</summary>
        public Color LockedTint => _lockedTint;

        /// <summary>Tint the silhouette flashes with when the weapon unlocks.</summary>
        public Color UnlockGlowTint => _unlockGlowTint;

        /// <summary>Seconds the unlock glow lasts.</summary>
        public float UnlockGlowSeconds => _unlockGlowSeconds;
    }
}
