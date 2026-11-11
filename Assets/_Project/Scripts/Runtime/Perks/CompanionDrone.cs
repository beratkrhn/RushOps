using System.Collections.Generic;
using RushOps.Audio;
using RushOps.Combat;
using RushOps.Enemies;
using RushOps.Player;
using RushOps.UI;
using UnityEngine;

namespace RushOps.Perks
{
    /// <summary>
    /// The combat drone of the companion perk (F-PRK-05, F-PRK-06, issue #245): collecting the perk makes it appear next to the
    /// player, follow smoothly and shoot at the nearest enemy in range for <see cref="CompanionConfig.Duration"/>
    /// seconds; collecting the perk again restarts the timer, there is only ever this one drone. It cannot be hurt
    /// (it has no collider). Its bullets come from the player's <see cref="ProjectilePool"/> and deal the damage of
    /// the start weapon, so kills count like the player's own (score, kill gates, gold, perk drops). It disappears
    /// when time runs out, when the player dies and when the level ends. Thin adapter; the rules live in
    /// <see cref="CompanionRules"/>, the values in <see cref="CompanionConfig"/>.
    /// </summary>
    public class CompanionDrone : MonoBehaviour
    {
        [SerializeField] private CompanionConfig _config;
        [Tooltip("Reports the collected perks.")]
        [SerializeField] private PlayerPerks _perks;
        [Tooltip("Player whose weapon gives the damage and projectile settings of the drone.")]
        [SerializeField] private PlayerWeapon _weapon;
        [SerializeField] private PlayerHealth _health;
        [Tooltip("Ends the drone when the level is over.")]
        [SerializeField] private LevelResults _results;
        [SerializeField] private ProjectilePool _projectiles;
        [Tooltip("Layers that stop the drone's bullets: walls and enemies.")]
        [SerializeField] private LayerMask _obstacles;
        [Tooltip("Pools of all enemy types in the level.")]
        [SerializeField] private EnemyPool[] _pools = System.Array.Empty<EnemyPool>();
        [SerializeField] private SpriteRenderer _renderer;
        [Tooltip("Hover frames in playback order.")]
        [SerializeField] private Sprite[] _frames = System.Array.Empty<Sprite>();
        [SerializeField] private SfxPlayer _sfx;
        [SerializeField] private AudioClip _shotClip;

        private readonly CompanionRules _rules = new CompanionRules();
        private float _durationFactor = 1f;
        private readonly List<CompanionTarget> _candidates = new List<CompanionTarget>(128);
        private Transform _player;
        private float _age;

        /// <summary>True while the drone is there.</summary>
        public bool IsActive => _rules.IsActive;

        /// <summary>Seconds the drone stays.</summary>
        public float Remaining => _rules.Remaining;

        /// <summary>Raised once per bullet the drone fires, with its direction.</summary>
        public event System.Action<Vector2> Fired;

        /// <summary>
        /// Factor on the duration of the drone started from now on, e.g. 1.3 with the longer perks upgrade of the endless run
        /// (F-END-12); 1 by default, never below 0. A running one keeps its time.
        /// </summary>
        public float DurationFactor
        {
            get => _durationFactor;
            set => _durationFactor = Mathf.Max(0f, value);
        }

        /// <summary>Starts the drone next to the player, or restarts its timer if it is already there.</summary>
        public void Activate()
        {
            if (_config == null || _player == null)
            {
                return;
            }

            if (!_rules.IsActive)
            {
                transform.position = FollowTarget();
                _age = 0f;
            }

            _rules.Activate(_config.Duration * _durationFactor);
            Refresh();
        }

        /// <summary>
        /// Whether the drone stays for the rest of the run (permanent companion upgrade, F-END-12). Setting it starts the
        /// drone next to the player if it is not there; clearing it removes the drone.
        /// </summary>
        public bool Permanent
        {
            get => _rules.IsPermanent;
            set
            {
                if (value == _rules.IsPermanent)
                {
                    return;
                }

                if (!value)
                {
                    Dismiss();
                    return;
                }

                if (_config == null || _player == null)
                {
                    return;
                }

                if (!_rules.IsActive)
                {
                    transform.position = FollowTarget();
                    _age = 0f;
                }

                _rules.ActivatePermanent();
                Refresh();
            }
        }

        /// <summary>Removes the drone, e.g. at level end or death.</summary>
        public void Dismiss()
        {
            _rules.Clear();
            Refresh();
        }

        private void Awake()
        {
            _player = _perks != null ? _perks.transform : null;
            if (_renderer != null && _config != null)
            {
                _renderer.transform.localPosition = new Vector3(0f, _config.HoverHeight, 0f);
            }

            Refresh();
        }

        private void OnEnable()
        {
            if (_perks != null)
            {
                _perks.Applied += OnApplied;
            }

            if (_health != null)
            {
                _health.Died += Dismiss;
            }

            if (_results != null)
            {
                _results.Completed += OnCompleted;
                _results.Failed += Dismiss;
            }
        }

        private void OnDisable()
        {
            if (_perks != null)
            {
                _perks.Applied -= OnApplied;
            }

            if (_health != null)
            {
                _health.Died -= Dismiss;
            }

            if (_results != null)
            {
                _results.Completed -= OnCompleted;
                _results.Failed -= Dismiss;
            }
        }

        private void OnApplied(PerkDefinition perk)
        {
            if (perk.Effect == PerkEffect.Companion)
            {
                Activate();
            }
        }

        private void OnCompleted(int stars, int score)
        {
            Dismiss();
        }

        private void Update()
        {
            if (!_rules.IsActive)
            {
                return;
            }

            float dt = Time.deltaTime;
            if (_rules.Tick(dt))
            {
                Refresh();
                return;
            }

            transform.position = CompanionRules.Follow(transform.position, FollowTarget(), _config.FollowSpeed, dt);
            _age += dt;
            AnimateSprite();
            Shoot(dt);
        }

        // Behind the player's left shoulder, so it stays out of the line of fire.
        private Vector2 FollowTarget()
        {
            Vector2 offset = new Vector2(-0.7f, 0.7f) * _config.FollowDistance;
            return (Vector2)_player.position + offset;
        }

        private void AnimateSprite()
        {
            if (_renderer == null || _frames.Length == 0)
            {
                return;
            }

            _renderer.sprite = _frames[Mathf.FloorToInt(_age * _config.FrameRate) % _frames.Length];
        }

        private void Shoot(float dt)
        {
            WeaponDefinition weapon = _weapon != null && _weapon.Config != null ? _weapon.Config.StartWeapon : null;
            if (weapon == null || _projectiles == null)
            {
                return;
            }

            Vector2 origin = transform.position;
            int target = FindTarget(origin);
            if (target < 0)
            {
                return;
            }

            float rate = CompanionRules.ShotsPerSecond(
                weapon.FireRate, weapon.Damage, weapon.ProjectilesPerShot, _config.DamageShare);
            int shots = _rules.ShotsDue(dt, rate);
            if (shots == 0)
            {
                return;
            }

            Vector2 direction = _candidates[target].Position - origin;
            if (direction.sqrMagnitude < 0.0001f)
            {
                direction = Vector2.up;
            }

            // The rush makes every player damage stronger, the drone's too (F-RSH-05, N17).
            int damage = WeaponMath.ShotDamage(weapon.Damage, _weapon.ExtraDamageFactor);
            for (int i = 0; i < shots; i++)
            {
                _projectiles.Launch(origin, direction, weapon.ProjectileSpeed, _config.Range + 1f, _obstacles,
                    damage, false, ShotLook.Normal, weapon.ProjectileSprite);
                Fired?.Invoke(direction);
            }

            if (_sfx != null)
            {
                _sfx.Play(_shotClip, _config.ShotVolume, 0.05f);
            }
        }

        // Fills the candidate buffers from all pools and returns the index of the nearest enemy in range.
        private int FindTarget(Vector2 origin)
        {
            _candidates.Clear();
            foreach (EnemyPool pool in _pools)
            {
                if (pool == null)
                {
                    continue;
                }

                IReadOnlyList<Enemy> active = pool.Active;
                for (int i = 0; i < active.Count; i++)
                {
                    Enemy enemy = active[i];
                    _candidates.Add(new CompanionTarget(enemy.AimPoint, !enemy.IsDead));
                }
            }

            return CompanionRules.FindTarget(origin, _candidates, _config.Range);
        }

        private void Refresh()
        {
            if (_renderer == null)
            {
                return;
            }

            _renderer.enabled = _rules.IsActive;
            if (_renderer.enabled && _frames.Length > 0)
            {
                _renderer.sprite = _frames[0];
            }
        }
    }
}
