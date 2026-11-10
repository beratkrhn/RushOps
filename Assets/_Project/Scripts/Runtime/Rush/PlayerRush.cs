using System;
using RushOps.Audio;
using RushOps.Bosses;
using RushOps.Enemies;
using RushOps.Player;
using RushOps.Scoring;
using RushOps.UI;
using UnityEngine;

namespace RushOps.Rush
{
    /// <summary>
    /// The player's rush (F-RSH-01 to F-RSH-06, issue #270): kills of the level fill the <see cref="RushMeter"/>; a
    /// triggered rush slows all enemies and bosses down, lets the player dash without cooldown and raises the damage
    /// factor of the <see cref="PlayerWeapon"/>, which the super weapons, the drone and the scarecrow use too. The slow
    /// motion is a factor on the enemies, not <see cref="Time.timeScale"/>, so the player keeps full speed; the rush
    /// timer runs in scaled time, so the pause holds it. Death and level end end the rush. Thin adapter; the rules live
    /// in <see cref="RushMeter"/> and <see cref="RushRules"/>, the values in <see cref="RushConfig"/>.
    /// </summary>
    [RequireComponent(typeof(PlayerHealth))]
    public class PlayerRush : MonoBehaviour
    {
        [SerializeField] private RushConfig _config;
        [Tooltip("Its kills, with their combo multiplier, fill the meter.")]
        [SerializeField] private LevelScore _score;
        [Tooltip("Completing or failing the level ends the rush.")]
        [SerializeField] private LevelResults _results;
        [Tooltip("Pools of all enemy types in the level; their enemies are slowed during the rush.")]
        [SerializeField] private EnemyPool[] _pools = Array.Empty<EnemyPool>();
        [Tooltip("Boss fights of the level; their bodies are slowed less during the rush (RM6).")]
        [SerializeField] private BossFight[] _bossFights = Array.Empty<BossFight>();
        [SerializeField] private SfxPlayer _sfx;

        private RushMeter _meter;
        private PlayerHealth _health;
        private PlayerDash _dash;
        private PlayerWeapon _weapon;
        private Action<int> _onKill;
        private Action<Enemy> _onEnemySpawned;
        private Action<int, int> _onCompleted;
        private float _enemyFactor = 1f;

        /// <summary>Raised when a rush starts.</summary>
        public event Action Started;

        /// <summary>Raised when a rush ends.</summary>
        public event Action Ended;

        /// <summary>Fill and timer of the rush.</summary>
        public RushMeter Meter => _meter ??= new RushMeter(_config);

        /// <summary>Values of the rush; null if not set.</summary>
        public RushConfig Config => _config;

        /// <summary>True while the rush runs.</summary>
        public bool IsActive => Meter.IsActive;

        /// <summary>Starts the rush if the meter is full, e.g. from the rush button.</summary>
        /// <returns>True if a rush started.</returns>
        public bool TryTrigger()
        {
            return isActiveAndEnabled && Meter.TryTrigger();
        }

        /// <summary>Fills the meter at once; for tests and the debug controls.</summary>
        public void FillUp()
        {
            Meter.RegisterKill(Meter.FillTarget);
        }

        /// <summary>Sets the boss fights whose bodies the rush slows down, e.g. from a scene builder.</summary>
        public void SetBossFights(BossFight[] fights)
        {
            _bossFights = fights ?? Array.Empty<BossFight>();
            if (IsActive)
            {
                Apply(true);
            }
        }

        private void Awake()
        {
            _health = GetComponent<PlayerHealth>();
            _dash = GetComponent<PlayerDash>();
            _weapon = GetComponent<PlayerWeapon>();
            _onKill = OnKill;
            _onEnemySpawned = OnEnemySpawned;
            _onCompleted = OnCompleted;
            Meter.Started += OnStarted;
            Meter.Ended += OnEnded;
        }

        private void OnEnable()
        {
            if (_score != null)
            {
                _score.Keeper.KillRegistered += _onKill;
            }

            _health.Died += Meter.End;
            if (_results != null)
            {
                _results.Completed += _onCompleted;
                _results.Failed += Meter.End;
            }

            foreach (EnemyPool pool in _pools)
            {
                if (pool != null)
                {
                    pool.EnemySpawned += _onEnemySpawned;
                }
            }
        }

        private void OnDisable()
        {
            if (_score != null)
            {
                _score.Keeper.KillRegistered -= _onKill;
            }

            _health.Died -= Meter.End;
            if (_results != null)
            {
                _results.Completed -= _onCompleted;
                _results.Failed -= Meter.End;
            }

            foreach (EnemyPool pool in _pools)
            {
                if (pool != null)
                {
                    pool.EnemySpawned -= _onEnemySpawned;
                }
            }

            Meter.End();
        }

        private void Update()
        {
            Meter.Tick(Time.deltaTime);
        }

        private void OnKill(int multiplier)
        {
            Meter.RegisterKill(multiplier);
        }

        private void OnCompleted(int stars, int score)
        {
            Meter.End();
        }

        private void OnEnemySpawned(Enemy enemy)
        {
            enemy.TimeFactor = _enemyFactor;
        }

        private void OnStarted()
        {
            Apply(true);
            Play(_config != null ? _config.StartClip : null, _config != null ? _config.StartVolume : 0f);
            Started?.Invoke();
        }

        private void OnEnded()
        {
            Apply(false);
            Play(_config != null ? _config.EndClip : null, _config != null ? _config.EndVolume : 0f);
            Ended?.Invoke();
        }

        // Called when the rush starts or ends, not every frame.
        private void Apply(bool active)
        {
            _enemyFactor = RushRules.EnemySpeedFactor(active, false, _config);
            float bossFactor = RushRules.EnemySpeedFactor(active, true, _config);
            foreach (EnemyPool pool in _pools)
            {
                if (pool == null)
                {
                    continue;
                }

                for (int i = 0; i < pool.Active.Count; i++)
                {
                    pool.Active[i].TimeFactor = _enemyFactor;
                }
            }

            foreach (BossFight fight in _bossFights)
            {
                if (fight != null)
                {
                    fight.TimeFactor = bossFactor;
                }
            }

            if (_dash != null)
            {
                _dash.CooldownFree = active;
            }

            if (_weapon != null)
            {
                _weapon.DamageFactor = RushRules.DamageMultiplier(active, _config);
            }
        }

        private void Play(AudioClip clip, float volume)
        {
            if (_sfx != null && clip != null)
            {
                _sfx.Play(clip, volume, 0f);
            }
        }
    }
}
