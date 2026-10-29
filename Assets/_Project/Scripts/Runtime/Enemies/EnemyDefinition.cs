using UnityEngine;

namespace RushOps.Enemies
{
    /// <summary>Balancing values of one enemy type (F-GEG-04).</summary>
    [CreateAssetMenu(fileName = "EnemyDefinition", menuName = "RushOps/Enemy Definition")]
    public class EnemyDefinition : ScriptableObject
    {
        [Tooltip("Hearts the player loses when touching this enemy.")]
        [SerializeField, Min(0)] private int _contactDamage = 1;

        /// <summary>Hearts the player loses when touching this enemy.</summary>
        public int ContactDamage => _contactDamage;
    }
}
