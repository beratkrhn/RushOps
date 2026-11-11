using System;
using RushOps.Player;
using RushOps.UI;
using UnityEngine;

namespace RushOps.Perks
{
    /// <summary>
    /// Puts a super weapon into the player's hands (F-PRK-09, issue #254, W11): collecting its perk holsters the
    /// <see cref="PlayerWeapon"/> for the configured duration, collecting it again restarts the timer, and when it runs
    /// out the previous weapon fires again; the weapon slots are never touched. Death, level end and restart end it
    /// as well. The super weapons themselves (e.g. <see cref="Flamethrower"/>) watch <see cref="Kind"/>. Thin adapter;
    /// the rules live in <see cref="SuperWeaponState"/>, the durations in the configs.
    /// </summary>
    [RequireComponent(typeof(PlayerWeapon))]
    public class PlayerSuperWeapon : MonoBehaviour
    {
        [Tooltip("Reports the collected perks.")]
        [SerializeField] private PlayerPerks _perks;
        [SerializeField] private PlayerHealth _health;
        [Tooltip("Ends the super weapon when the level is over.")]
        [SerializeField] private LevelResults _results;
        [Tooltip("Duration of the flamethrower; without it the flamethrower perk does nothing.")]
        [SerializeField] private FlamethrowerConfig _flamethrower;
        [Tooltip("Duration of the rocket launcher; without it the rocket launcher perk does nothing.")]
        [SerializeField] private RocketLauncherConfig _rocketLauncher;

        private readonly SuperWeaponState _state = new SuperWeaponState();
        private PlayerWeapon _weapon;
        private float _durationFactor = 1f;

        /// <summary>Raised when a super weapon came into the hands or was refreshed.</summary>
        public event Action<SuperWeaponKind> Started;

        /// <summary>Raised when the super weapon left the hands: time ran out, death, level end.</summary>
        public event Action Ended;

        /// <summary>The super weapon in the hands; <see cref="SuperWeaponKind.None"/> while the normal weapon fires.</summary>
        public SuperWeaponKind Kind => _state.IsActive ? _state.Kind : SuperWeaponKind.None;

        /// <summary>True while a super weapon replaces the normal weapon.</summary>
        public bool IsActive => _state.IsActive;

        /// <summary>Seconds left; 0 when not active.</summary>
        public float Remaining => _state.Remaining;

        /// <summary>
        /// Factor on the duration of the super weapons started from now on, e.g. 1.3 with the longer perks upgrade of the endless run
        /// (F-END-12); 1 by default, never below 0. A running one keeps its time.
        /// </summary>
        public float DurationFactor
        {
            get => _durationFactor;
            set => _durationFactor = Mathf.Max(0f, value);
        }

        /// <summary>Takes <paramref name="kind"/> into the hands, or restarts its timer. Ignored without its config.</summary>
        public void Activate(SuperWeaponKind kind)
        {
            float duration = Duration(kind) * _durationFactor;
            if (duration <= 0f)
            {
                return;
            }

            _state.Activate(kind, duration);
            ApplyHolster();
            Started?.Invoke(kind);
        }

        /// <summary>Removes the super weapon at once; the normal weapon fires again.</summary>
        public void End()
        {
            bool wasActive = _state.IsActive;
            _state.Clear();
            ApplyHolster();
            if (wasActive)
            {
                Ended?.Invoke();
            }
        }

        private float Duration(SuperWeaponKind kind)
        {
            return kind switch
            {
                SuperWeaponKind.Flamethrower => _flamethrower != null ? _flamethrower.Duration : 0f,
                SuperWeaponKind.RocketLauncher => _rocketLauncher != null ? _rocketLauncher.Duration : 0f,
                _ => 0f,
            };
        }

        private void Awake()
        {
            _weapon = GetComponent<PlayerWeapon>();
        }

        private void OnEnable()
        {
            if (_perks != null)
            {
                _perks.Applied += OnApplied;
            }

            if (_health != null)
            {
                _health.Died += End;
            }

            if (_results != null)
            {
                _results.Completed += OnCompleted;
                _results.Failed += End;
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
                _health.Died -= End;
            }

            if (_results != null)
            {
                _results.Completed -= OnCompleted;
                _results.Failed -= End;
            }
        }

        private void Update()
        {
            if (_state.IsActive && _state.Tick(Time.deltaTime))
            {
                ApplyHolster();
                Ended?.Invoke();
            }
        }

        private void OnApplied(PerkDefinition perk)
        {
            switch (perk.Effect)
            {
                case PerkEffect.Flamethrower:
                    Activate(SuperWeaponKind.Flamethrower);
                    break;
                case PerkEffect.RocketLauncher:
                    Activate(SuperWeaponKind.RocketLauncher);
                    break;
            }
        }

        private void OnCompleted(int stars, int score)
        {
            End();
        }

        private void ApplyHolster()
        {
            if (_weapon != null)
            {
                _weapon.Holstered = _state.IsActive;
            }
        }
    }
}
