using System;
using RushOps.Bosses;
using RushOps.Combat;
using RushOps.Enemies;
using UnityEngine;

namespace RushOps.Endless
{
    /// <summary>
    /// Balancing values of the endless mode (F-END-03 to F-END-07, decisions E3 to E6, N3 to N5). All are start values;
    /// <see cref="EndlessRules"/> turns them into the values of one round.
    /// </summary>
    [CreateAssetMenu(fileName = "EndlessConfig", menuName = "RushOps/Endless Config")]
    public class EndlessConfig : ScriptableObject
    {
        [Header("Rounds")]
        [Tooltip("Seconds of break with „Runde X“ before every round (N5).")]
        [SerializeField, Min(0f)] private float _pauseBetweenRounds = 4f;

        [Tooltip("Hearts the player gets back after every cleared round, up to the maximum (E5).")]
        [SerializeField, Min(0)] private int _heartsPerRound = 1;

        [Header("Enemies")]
        [Tooltip("Enemies in round 1.")]
        [SerializeField, Min(0)] private int _enemiesFirstRound = 6;

        [Tooltip("Enemies added with every further round.")]
        [SerializeField, Min(0)] private int _enemiesPerRound = 2;

        [Tooltip("Base type; fills up every round after the heavier types below.")]
        [SerializeField] private EnemyDefinition _baseEnemy;

        [Tooltip("Heavier types: from which round they come and how their share grows (F-END-04).")]
        [SerializeField] private EndlessEnemyShare[] _enemyShares = Array.Empty<EndlessEnemyShare>();

        [Header("Spawning")]
        [Tooltip("Seconds between two enemies in round 1.")]
        [SerializeField, Min(0f)] private float _spawnIntervalFirstRound = 0.8f;

        [Tooltip("Seconds the spawn interval shrinks with every further round.")]
        [SerializeField, Min(0f)] private float _spawnIntervalStep = 0.05f;

        [Tooltip("Shortest spawn interval in seconds.")]
        [SerializeField, Min(0f)] private float _spawnIntervalMin = 0.3f;

        [Tooltip("Most enemies alive at once in rounds 1 to 4; bosses do not count (N3).")]
        [SerializeField, Min(0)] private int _maxActiveStart = 10;

        [Tooltip("Added to the most enemies alive at once every few rounds.")]
        [SerializeField, Min(0)] private int _maxActiveStep = 1;

        [Tooltip("Every how many rounds the most enemies alive at once grows; it grows at round 5, 10, … for 5.")]
        [SerializeField, Min(1)] private int _maxActiveEveryRounds = 5;

        [Tooltip("Upper limit of enemies alive at once, checked against the frame budget (N3, issue #212).")]
        [SerializeField, Min(0)] private int _maxActiveCap = 20;

        [Header("Bosses")]
        [Tooltip("Every how many rounds a boss round comes (E3).")]
        [SerializeField, Min(1)] private int _bossEveryRounds = 10;

        [Tooltip("Bosses in the order they come; starts over after the last.")]
        [SerializeField] private BossDefinition[] _bosses = Array.Empty<BossDefinition>();

        [Tooltip("Health added per further boss appearance in the run, as share of the boss's health: 0.25 = +25 %.")]
        [SerializeField, Min(0f)] private float _bossHealthGrowth = 0.25f;

        [Header("Weapons")]
        [Tooltip("Weapon every run starts with (F-END-07).")]
        [SerializeField] private WeaponDefinition _startWeapon;

        [Tooltip("Weapons on the arena wall and the round from which each is available (N4).")]
        [SerializeField] private EndlessWeaponRound[] _wallWeapons = Array.Empty<EndlessWeaponRound>();

        /// <summary>Seconds of break before every round.</summary>
        public float PauseBetweenRounds => _pauseBetweenRounds;

        /// <summary>Hearts given back after every cleared round.</summary>
        public int HeartsPerRound => _heartsPerRound;

        /// <summary>Enemies in round 1.</summary>
        public int EnemiesFirstRound => _enemiesFirstRound;

        /// <summary>Enemies added per round.</summary>
        public int EnemiesPerRound => _enemiesPerRound;

        /// <summary>Base type that fills up every round.</summary>
        public EnemyDefinition BaseEnemy => _baseEnemy;

        /// <summary>Heavier types and their shares.</summary>
        public EndlessEnemyShare[] EnemyShares => _enemyShares ?? Array.Empty<EndlessEnemyShare>();

        /// <summary>Spawn interval in round 1.</summary>
        public float SpawnIntervalFirstRound => _spawnIntervalFirstRound;

        /// <summary>Shrink of the spawn interval per round.</summary>
        public float SpawnIntervalStep => _spawnIntervalStep;

        /// <summary>Shortest spawn interval.</summary>
        public float SpawnIntervalMin => _spawnIntervalMin;

        /// <summary>Most enemies alive at once at the start of a run.</summary>
        public int MaxActiveStart => _maxActiveStart;

        /// <summary>Growth of the most enemies alive at once.</summary>
        public int MaxActiveStep => _maxActiveStep;

        /// <summary>Every how many rounds the most enemies alive at once grows.</summary>
        public int MaxActiveEveryRounds => Math.Max(1, _maxActiveEveryRounds);

        /// <summary>Upper limit of enemies alive at once.</summary>
        public int MaxActiveCap => _maxActiveCap;

        /// <summary>Every how many rounds a boss comes.</summary>
        public int BossEveryRounds => Math.Max(1, _bossEveryRounds);

        /// <summary>Bosses in order.</summary>
        public BossDefinition[] Bosses => _bosses ?? Array.Empty<BossDefinition>();

        /// <summary>Health added per further boss appearance, as share.</summary>
        public float BossHealthGrowth => _bossHealthGrowth;

        /// <summary>Weapon every run starts with.</summary>
        public WeaponDefinition StartWeapon => _startWeapon;

        /// <summary>Weapons on the wall with their rounds.</summary>
        public EndlessWeaponRound[] WallWeapons => _wallWeapons ?? Array.Empty<EndlessWeaponRound>();

        /// <summary>Sets the round values, for editor scripts and tests.</summary>
        public void ConfigureRounds(float pauseBetweenRounds, int heartsPerRound)
        {
            _pauseBetweenRounds = Mathf.Max(0f, pauseBetweenRounds);
            _heartsPerRound = Math.Max(0, heartsPerRound);
        }

        /// <summary>Sets the enemy values, for editor scripts and tests.</summary>
        public void ConfigureEnemies(int firstRound, int perRound, EnemyDefinition baseEnemy, EndlessEnemyShare[] shares)
        {
            _enemiesFirstRound = Math.Max(0, firstRound);
            _enemiesPerRound = Math.Max(0, perRound);
            _baseEnemy = baseEnemy;
            _enemyShares = shares ?? Array.Empty<EndlessEnemyShare>();
        }

        /// <summary>Sets spawn interval and the most enemies alive at once, for editor scripts and tests.</summary>
        public void ConfigureSpawning(float intervalFirstRound, float intervalStep, float intervalMin, int maxActiveStart,
            int maxActiveStep, int maxActiveEveryRounds, int maxActiveCap)
        {
            _spawnIntervalFirstRound = Mathf.Max(0f, intervalFirstRound);
            _spawnIntervalStep = Mathf.Max(0f, intervalStep);
            _spawnIntervalMin = Mathf.Max(0f, intervalMin);
            _maxActiveStart = Math.Max(0, maxActiveStart);
            _maxActiveStep = Math.Max(0, maxActiveStep);
            _maxActiveEveryRounds = Math.Max(1, maxActiveEveryRounds);
            _maxActiveCap = Math.Max(0, maxActiveCap);
        }

        /// <summary>Sets the boss values, for editor scripts and tests.</summary>
        public void ConfigureBosses(int everyRounds, BossDefinition[] bosses, float healthGrowth)
        {
            _bossEveryRounds = Math.Max(1, everyRounds);
            _bosses = bosses ?? Array.Empty<BossDefinition>();
            _bossHealthGrowth = Mathf.Max(0f, healthGrowth);
        }

        /// <summary>Sets the weapons, for editor scripts and tests.</summary>
        public void ConfigureWeapons(WeaponDefinition startWeapon, EndlessWeaponRound[] wallWeapons)
        {
            _startWeapon = startWeapon;
            _wallWeapons = wallWeapons ?? Array.Empty<EndlessWeaponRound>();
        }
    }
}
