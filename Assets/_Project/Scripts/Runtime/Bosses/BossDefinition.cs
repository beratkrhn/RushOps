using UnityEngine;

namespace RushOps.Bosses
{
    /// <summary>Balancing values of one boss (F-BOS-01, F-BOS-02, decisions C5, C9, C10).</summary>
    [CreateAssetMenu(fileName = "BossDefinition", menuName = "RushOps/Boss Definition")]
    public class BossDefinition : ScriptableObject
    {
        [Tooltip("Name in the boss health bar (German).")]
        [SerializeField] private string _displayName = "Boss";

        [Tooltip("Hit points; chosen so the fight lasts as decided without perks (C5).")]
        [SerializeField, Min(1)] private int _health = 100;

        [Tooltip("Speed in units (tiles) per second while chasing the player.")]
        [SerializeField, Min(0f)] private float _moveSpeed = 2f;

        [Tooltip("Hearts the player loses when touching the boss.")]
        [SerializeField, Min(0)] private int _contactDamage = 1;

        [Tooltip("Score points for beating the boss, without combo multiplier (C10).")]
        [SerializeField, Min(0)] private int _points = 1000;

        [Tooltip("Share of health left at which one random perk drops, from high to low (C9).")]
        [SerializeField] private float[] _perkThresholds = { 0.66f, 0.33f };

        [Tooltip("Seconds the body fades out after its death animation.")]
        [SerializeField, Min(0f)] private float _deathDuration = 0.6f;

        [Tooltip("Seconds from the last body's death until „Level geschafft!“ shows.")]
        [SerializeField, Min(0f)] private float _victoryDelay = 1.5f;

        [Tooltip("Physics mass; far above the player's 10, so the player cannot push the boss.")]
        [SerializeField, Min(0.01f)] private float _mass = 200f;

        /// <summary>Name in the boss health bar.</summary>
        public string DisplayName => _displayName;

        /// <summary>Hit points.</summary>
        public int Health => _health;

        /// <summary>Chase speed in units per second.</summary>
        public float MoveSpeed => _moveSpeed;

        /// <summary>Hearts the player loses on contact.</summary>
        public int ContactDamage => _contactDamage;

        /// <summary>Score points for beating the boss.</summary>
        public int Points => _points;

        /// <summary>Health shares (0..1) at which a perk drops, from high to low.</summary>
        public float[] PerkThresholds => _perkThresholds ?? System.Array.Empty<float>();

        /// <summary>Seconds the body fades after its death animation.</summary>
        public float DeathDuration => _deathDuration;

        /// <summary>Seconds from the last death until the level is done.</summary>
        public float VictoryDelay => _victoryDelay;

        /// <summary>Physics mass of the body.</summary>
        public float Mass => _mass;

        /// <summary>Sets the main values; for builders and tests.</summary>
        public void Configure(string displayName, int health, float moveSpeed, int contactDamage, int points)
        {
            _displayName = displayName;
            _health = Mathf.Max(1, health);
            _moveSpeed = Mathf.Max(0f, moveSpeed);
            _contactDamage = Mathf.Max(0, contactDamage);
            _points = Mathf.Max(0, points);
        }
    }
}
