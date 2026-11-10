using RushOps.Combat;
using UnityEngine;

namespace RushOps.Enemies
{
    /// <summary>Balancing values of one enemy type (F-GEG-04).</summary>
    [CreateAssetMenu(fileName = "EnemyDefinition", menuName = "RushOps/Enemy Definition")]
    public class EnemyDefinition : ScriptableObject
    {
        [Tooltip("Hit points; one projectile hit removes the weapon's damage.")]
        [SerializeField, Min(1)] private int _health = 30;

        [Tooltip("Top speed in units (tiles) per second.")]
        [SerializeField, Min(0f)] private float _moveSpeed = 2.5f;

        [Tooltip("Hearts the player loses when touching this enemy.")]
        [SerializeField, Min(0)] private int _contactDamage = 1;

        [Tooltip("Seconds the body fades out after its death animation (or shrinks and fades without one) before the enemy returns to the pool.")]
        [SerializeField, Min(0f)] private float _deathDuration = 0.25f;

        [Tooltip("Score points for a kill (counted from v0.2.0).")]
        [SerializeField, Min(0)] private int _points = 10;

        [Tooltip("Gold a kill drops (F-GLD-01).")]
        [SerializeField, Min(0)] private int _gold = 1;

        [Tooltip("Bosses do not count towards the limit of enemies alive at once (F-GEG-03).")]
        [SerializeField] private bool _isBoss;

        [Header("Body")]
        [Tooltip("Physics mass; heavier enemies shove lighter ones aside instead of getting stuck behind them. The player weighs 10.")]
        [SerializeField, Min(0.01f)] private float _mass = 1f;

        [Tooltip("Share of the hit knockback this enemy takes: 1 = full, 0 = none.")]
        [SerializeField, Range(0f, 1f)] private float _knockbackScale = 1f;

        [Tooltip("How much this enemy steers away from nearby enemies: 1 = full separation, 0 = walks straight through the crowd.")]
        [SerializeField, Range(0f, 1f)] private float _separationScale = 1f;

        [Header("Pack and lunge")]
        [Tooltip("Comes in packs and lunges at the player (dog, issue #262); none for other enemies.")]
        [SerializeField] private DogConfig _dog;

        [Header("Explosion")]
        [Tooltip("Blows up when it dies or touches the player (boomer, issue #22); none if empty.")]
        [SerializeField] private ExplosionDefinition _explosion;

        [Header("Sound")]
        [Tooltip("Placeholder sound when the enemy appears; none if empty.")]
        [SerializeField] private AudioClip _spawnClip;

        [Tooltip("Placeholder sound when the enemy first comes within the approach distance; none if empty.")]
        [SerializeField] private AudioClip _approachClip;

        [Tooltip("Distance to the player in units (tiles) that triggers the approach sound, once per life.")]
        [SerializeField, Min(0f)] private float _approachDistance = 4f;

        /// <summary>Hit points.</summary>
        public int Health => _health;

        /// <summary>Top speed in units per second.</summary>
        public float MoveSpeed => _moveSpeed;

        /// <summary>Hearts the player loses when touching this enemy.</summary>
        public int ContactDamage => _contactDamage;

        /// <summary>Seconds the body fades out after its death animation before the enemy returns to the pool.</summary>
        public float DeathDuration => _deathDuration;

        /// <summary>Score points for a kill.</summary>
        public int Points => _points;

        /// <summary>Gold a kill drops.</summary>
        public int Gold => _gold;

        /// <summary>True for bosses, which do not count towards the enemy limit.</summary>
        public bool IsBoss => _isBoss;

        /// <summary>Physics mass of the body.</summary>
        public float Mass => _mass;

        /// <summary>Share of the hit knockback this enemy takes, 0..1.</summary>
        public float KnockbackScale => _knockbackScale;

        /// <summary>Share of the separation from nearby enemies this enemy follows, 0..1.</summary>
        public float SeparationScale => _separationScale;

        /// <summary>Pack and lunge values of a dog; null for enemies that neither come in packs nor lunge.</summary>
        public DogConfig Dog => _dog;

        /// <summary>Enemies of this type that spawn together; 1 for all but the dog.</summary>
        public int PackSize => _dog != null ? _dog.PackSize : 1;

        /// <summary>Distance of the pack members from their common spawn point; 0 without pack.</summary>
        public float PackSpread => _dog != null ? _dog.PackSpread : 0f;

        /// <summary>Explosion on death and on touching the player; null for enemies that do not explode.</summary>
        public ExplosionDefinition Explosion => _explosion;

        /// <summary>Sound when the enemy appears; may be null.</summary>
        public AudioClip SpawnClip => _spawnClip;

        /// <summary>Sound when the enemy first comes within <see cref="ApproachDistance"/>; may be null.</summary>
        public AudioClip ApproachClip => _approachClip;

        /// <summary>Distance to the player in units that triggers <see cref="ApproachClip"/>.</summary>
        public float ApproachDistance => _approachDistance;
    }
}
