using System;
using RushOps.Bosses;
using RushOps.Combat;
using RushOps.Enemies;
using UnityEngine;

namespace RushOps.Endless
{
    /// <summary>
    /// Balancing values of the endless mode (F-END-03 to F-END-07, F-END-10 to F-END-12, decisions E3 to E6, N3 to N6, T2, N19). All are start values;
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

        [Tooltip("Emergency brake: most enemies alive at once in the endless mode, bosses do not count. 0 = no limit (F-END-04, S1); the spawn interval is the only brake.")]
        [SerializeField, Min(0)] private int _maxActiveLimit;

        [Header("Bosses")]
        [Tooltip("Every how many rounds a boss round comes (E3).")]
        [SerializeField, Min(1)] private int _bossEveryRounds = 10;

        [Tooltip("Bosses in the order they come; starts over after the last.")]
        [SerializeField] private BossDefinition[] _bosses = Array.Empty<BossDefinition>();

        [Tooltip("Health added per further boss appearance in the run, as share of the boss's health: 0.25 = +25 %.")]
        [SerializeField, Min(0f)] private float _bossHealthGrowth = 0.25f;

        [Header("Dogs")]
        [Tooltip("Every how many rounds a dog round with dogs only comes (F-END-10); 0 = never. A boss round goes first and moves it one round on (N6).")]
        [SerializeField, Min(0)] private int _dogRoundEveryRounds = 6;

        [Tooltip("The only enemy of a dog round; it comes in packs (H1, H4).")]
        [SerializeField] private EnemyDefinition _dogEnemy;

        [Header("Weapons")]
        [Tooltip("Weapon every run starts with (F-END-07).")]
        [SerializeField] private WeaponDefinition _startWeapon;

        [Tooltip("Weapons on the arena wall and the round from which each is available (N4).")]
        [SerializeField] private EndlessWeaponRound[] _wallWeapons = Array.Empty<EndlessWeaponRound>();

        [Tooltip("Seconds the mission tracker shows „Neue Waffe: …“ when a wall weapon unlocks (issue #209).")]
        [SerializeField, Min(0f)] private float _newWeaponNoticeSeconds = 4f;

        [Header("Upgrades")]
        [Tooltip("Every how many cleared rounds an upgrade choice comes; boss rounds always bring one (F-END-11, T2).")]
        [SerializeField, Min(1)] private int _upgradeEveryRounds = 3;

        [Tooltip("Different upgrades offered per choice (F-END-11).")]
        [SerializeField, Min(1)] private int _upgradeOffers = 3;

        [Tooltip("The run upgrades, in a fixed order; the daily run draws from this order (F-END-12, N19).")]
        [SerializeField] private UpgradeDefinition[] _upgrades = Array.Empty<UpgradeDefinition>();

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

        /// <summary>Emergency brake for enemies alive at once; 0 means no limit (F-END-04).</summary>
        public int MaxActiveLimit => _maxActiveLimit;

        /// <summary>Every how many rounds a boss comes.</summary>
        public int BossEveryRounds => Math.Max(1, _bossEveryRounds);

        /// <summary>Bosses in order.</summary>
        public BossDefinition[] Bosses => _bosses ?? Array.Empty<BossDefinition>();

        /// <summary>Health added per further boss appearance, as share.</summary>
        public float BossHealthGrowth => _bossHealthGrowth;

        /// <summary>Every how many rounds a dog round comes; 0 = never.</summary>
        public int DogRoundEveryRounds => Math.Max(0, _dogRoundEveryRounds);

        /// <summary>The only enemy of a dog round; null = no dog rounds.</summary>
        public EnemyDefinition DogEnemy => _dogEnemy;

        /// <summary>Weapon every run starts with.</summary>
        public WeaponDefinition StartWeapon => _startWeapon;

        /// <summary>Seconds the tracker shows the notice of a newly unlocked wall weapon.</summary>
        public float NewWeaponNoticeSeconds => _newWeaponNoticeSeconds;

        /// <summary>Weapons on the wall with their rounds.</summary>
        public EndlessWeaponRound[] WallWeapons => _wallWeapons ?? Array.Empty<EndlessWeaponRound>();

        /// <summary>Every how many cleared rounds an upgrade choice comes.</summary>
        public int UpgradeEveryRounds => Math.Max(1, _upgradeEveryRounds);

        /// <summary>Different upgrades offered per choice.</summary>
        public int UpgradeOffers => Math.Max(1, _upgradeOffers);

        /// <summary>The run upgrades in order.</summary>
        public UpgradeDefinition[] Upgrades => _upgrades ?? Array.Empty<UpgradeDefinition>();

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

        /// <summary>Sets spawn interval and the emergency brake (0 = none), for editor scripts and tests.</summary>
        public void ConfigureSpawning(float intervalFirstRound, float intervalStep, float intervalMin, int maxActiveLimit)
        {
            _spawnIntervalFirstRound = Mathf.Max(0f, intervalFirstRound);
            _spawnIntervalStep = Mathf.Max(0f, intervalStep);
            _spawnIntervalMin = Mathf.Max(0f, intervalMin);
            _maxActiveLimit = Math.Max(0, maxActiveLimit);
        }

        /// <summary>Sets the boss values, for editor scripts and tests.</summary>
        public void ConfigureBosses(int everyRounds, BossDefinition[] bosses, float healthGrowth)
        {
            _bossEveryRounds = Math.Max(1, everyRounds);
            _bosses = bosses ?? Array.Empty<BossDefinition>();
            _bossHealthGrowth = Mathf.Max(0f, healthGrowth);
        }

        /// <summary>Sets the dog rounds, for editor scripts and tests.</summary>
        public void ConfigureDogRounds(int everyRounds, EnemyDefinition dog)
        {
            _dogRoundEveryRounds = Math.Max(0, everyRounds);
            _dogEnemy = dog;
        }

        /// <summary>Sets the weapons, for editor scripts and tests.</summary>
        public void ConfigureWeapons(WeaponDefinition startWeapon, EndlessWeaponRound[] wallWeapons)
        {
            _startWeapon = startWeapon;
            _wallWeapons = wallWeapons ?? Array.Empty<EndlessWeaponRound>();
        }

        /// <summary>Sets the upgrade values, for editor scripts and tests.</summary>
        public void ConfigureUpgrades(int everyRounds, int offers, UpgradeDefinition[] upgrades)
        {
            _upgradeEveryRounds = Math.Max(1, everyRounds);
            _upgradeOffers = Math.Max(1, offers);
            _upgrades = upgrades ?? Array.Empty<UpgradeDefinition>();
        }
    }
}
