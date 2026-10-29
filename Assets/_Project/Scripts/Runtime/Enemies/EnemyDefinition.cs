using UnityEngine;

namespace RushOps.Enemies
{
    /// <summary>Balancing values of one enemy type (F-GEG-04).</summary>
    [CreateAssetMenu(fileName = "EnemyDefinition", menuName = "RushOps/Enemy Definition")]
    public class EnemyDefinition : ScriptableObject
    {
        [Tooltip("Hit points; one projectile hit removes the weapon's damage.")]
        [SerializeField, Min(1)] private int _health = 3;

        [Tooltip("Top speed in units (tiles) per second.")]
        [SerializeField, Min(0f)] private float _moveSpeed = 2.5f;

        [Tooltip("Hearts the player loses when touching this enemy.")]
        [SerializeField, Min(0)] private int _contactDamage = 1;

        [Tooltip("Seconds of the death effect before the enemy returns to the pool.")]
        [SerializeField, Min(0f)] private float _deathDuration = 0.25f;

        [Tooltip("Score points for a kill (counted from v0.2.0).")]
        [SerializeField, Min(0)] private int _points = 10;

        /// <summary>Hit points.</summary>
        public int Health => _health;

        /// <summary>Top speed in units per second.</summary>
        public float MoveSpeed => _moveSpeed;

        /// <summary>Hearts the player loses when touching this enemy.</summary>
        public int ContactDamage => _contactDamage;

        /// <summary>Seconds of the death effect before the enemy returns to the pool.</summary>
        public float DeathDuration => _deathDuration;

        /// <summary>Score points for a kill.</summary>
        public int Points => _points;
    }
}
