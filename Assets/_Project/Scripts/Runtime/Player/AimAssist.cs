using RushOps.Combat;
using RushOps.Enemies;
using UnityEngine;

namespace RushOps.Player
{
    /// <summary>
    /// Aim assist (F-STE-06): corrects the shot direction towards the nearest living enemy that is close to the
    /// aim direction and in range, with angle and range from <see cref="WeaponConfig"/>.
    /// <see cref="PlayerWeapon"/> asks it for every shot; the rules live in <see cref="AimAssistMath"/>.
    /// </summary>
    public class AimAssist : MonoBehaviour
    {
        [Tooltip("Pools of all enemy types in the level.")]
        [SerializeField] private EnemyPool[] _pools = System.Array.Empty<EnemyPool>();

        private Vector2[] _targets = new Vector2[16];
        private PlayerWeapon _weapon;
        private bool? _enabledOverride;

        /// <summary>On or off as in the weapon config until set at runtime, e.g. by the settings menu.</summary>
        public bool Enabled
        {
            get
            {
                if (_enabledOverride.HasValue)
                {
                    return _enabledOverride.Value;
                }

                return _weapon != null && _weapon.Config != null && _weapon.Config.AimAssistEnabled;
            }
            set => _enabledOverride = value;
        }

        /// <summary>Pools of the enemies the assist aims at.</summary>
        public EnemyPool[] Pools
        {
            get => _pools;
            set => _pools = value ?? System.Array.Empty<EnemyPool>();
        }

        /// <summary>Shot direction for a shot from <paramref name="origin"/> along <paramref name="aim"/>.</summary>
        public Vector2 Adjust(Vector2 origin, Vector2 aim, WeaponConfig config)
        {
            if (_pools.Length == 0 || config == null || !Enabled)
            {
                return aim;
            }

            int count = CollectTargets();
            return AimAssistMath.Adjust(origin, aim, _targets, count, config.AimAssistAngle, config.AimAssistRange);
        }

        private void Awake()
        {
            _weapon = GetComponent<PlayerWeapon>();
        }

        private int CollectTargets()
        {
            int total = 0;
            foreach (EnemyPool pool in _pools)
            {
                total += pool != null ? pool.Active.Count : 0;
            }

            if (_targets.Length < total)
            {
                _targets = new Vector2[total * 2];
            }

            int count = 0;
            foreach (EnemyPool pool in _pools)
            {
                if (pool == null)
                {
                    continue;
                }

                var enemies = pool.Active;
                for (int i = 0; i < enemies.Count; i++)
                {
                    if (!enemies[i].IsDead)
                    {
                        _targets[count++] = enemies[i].AimPoint;
                    }
                }
            }

            return count;
        }
    }
}
