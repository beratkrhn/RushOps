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

        /// <summary>Seconds in front of the weapon until it is picked up.</summary>
        public float PickupTime => _pickupTime;

        /// <summary>Distance from the spot in front of the weapon within which the ring fills.</summary>
        public float PickupRadius => _pickupRadius;
    }
}
