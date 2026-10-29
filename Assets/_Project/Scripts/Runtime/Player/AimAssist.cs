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
        [SerializeField] private EnemyPool _enemies;

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

        /// <summary>Enemies the assist aims at.</summary>
        public EnemyPool Enemies
        {
            get => _enemies;
            set => _enemies = value;
        }

        /// <summary>Shot direction for a shot from <paramref name="origin"/> along <paramref name="aim"/>.</summary>
        public Vector2 Adjust(Vector2 origin, Vector2 aim, WeaponConfig config)
        {
            if (_enemies == null || config == null || !Enabled)
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
            var enemies = _enemies.Active;
            if (_targets.Length < enemies.Count)
            {
                _targets = new Vector2[enemies.Count * 2];
            }

            int count = 0;
            for (int i = 0; i < enemies.Count; i++)
            {
                if (!enemies[i].IsDead)
                {
                    _targets[count++] = enemies[i].AimPoint;
                }
            }

            return count;
        }
    }
}
