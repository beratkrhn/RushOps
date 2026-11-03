using System;
using System.Collections.Generic;
using RushOps.Combat;
using RushOps.Player;
using UnityEngine;

namespace RushOps.Perks
{
    /// <summary>
    /// Applies the perks the player collects (F-PRK-04): instant perks act at once (medkit), timed perks run in
    /// <see cref="ActivePerks"/> and modify the player while they last (speed boost; double damage, rapid fire and
    /// insta-kill on the <see cref="PlayerWeapon"/>, issue #25). Thin adapter; the timer rules
    /// live in <see cref="ActivePerks"/>, the values in the <see cref="PerkDefinition"/>s.
    /// </summary>
    [RequireComponent(typeof(PlayerMovement))]
    public class PlayerPerks : MonoBehaviour
    {
        [Tooltip("Reports the collected pickups.")]
        [SerializeField] private PerkPickupPool _pickups;

        private readonly ActivePerks _active = new ActivePerks();
        private readonly Dictionary<string, PerkDefinition> _definitions = new Dictionary<string, PerkDefinition>();
        private PlayerMovement _movement;
        private PlayerHealth _health;
        private PlayerWeapon _weapon;

        /// <summary>Raised when a perk was applied, also when a running perk was refreshed.</summary>
        public event Action<PerkDefinition> Applied;

        /// <summary>Raised when a timed perk ran out.</summary>
        public event Action<PerkDefinition> Expired;

        /// <summary>The running timed perks, in the order of their first pickup.</summary>
        public ActivePerks Active => _active;

        /// <summary>Pool whose collected pickups this component applies.</summary>
        public PerkPickupPool Pickups
        {
            get => _pickups;
            set
            {
                Unsubscribe();
                _pickups = value;
                if (isActiveAndEnabled)
                {
                    Subscribe();
                }
            }
        }

        /// <summary>Definition of a perk that was applied before, e.g. for its HUD icon; null if unknown.</summary>
        public PerkDefinition Definition(string id)
        {
            return id != null && _definitions.TryGetValue(id, out PerkDefinition perk) ? perk : null;
        }

        /// <summary>Applies <paramref name="perk"/> as if the player collected it.</summary>
        public void Apply(PerkDefinition perk)
        {
            if (perk == null)
            {
                return;
            }

            if (perk.Kind == PerkKind.Timed)
            {
                _definitions[perk.Id] = perk;
                _active.Activate(perk.Id, perk.Effect, perk.Magnitude, perk.Duration);
                ApplyModifiers();
            }
            else
            {
                ApplyInstant(perk);
            }

            Applied?.Invoke(perk);
        }

        private void Awake()
        {
            _movement = GetComponent<PlayerMovement>();
            _health = GetComponent<PlayerHealth>();
            _weapon = GetComponent<PlayerWeapon>();
            _active.Expired += OnExpired;
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void Update()
        {
            _active.Tick(Time.deltaTime);
        }

        private void Subscribe()
        {
            if (_pickups != null)
            {
                _pickups.Collected += OnCollected;
            }
        }

        private void Unsubscribe()
        {
            if (_pickups != null)
            {
                _pickups.Collected -= OnCollected;
            }
        }

        private void OnCollected(PerkPickup pickup)
        {
            Apply(pickup.Perk);
        }

        private void ApplyInstant(PerkDefinition perk)
        {
            switch (perk.Effect)
            {
                case PerkEffect.Medkit:
                    if (_health != null)
                    {
                        _health.Heal(Mathf.RoundToInt(perk.Magnitude));
                    }

                    break;
            }
        }

        private void OnExpired(string id)
        {
            ApplyModifiers();
            Expired?.Invoke(Definition(id));
        }

        // Called when the set of running perks changes, not every frame.
        private void ApplyModifiers()
        {
            _movement.SpeedMultiplier = _active.Multiplier(PerkEffect.SpeedBoost);
            if (_weapon != null)
            {
                _weapon.Modifiers = new ShotModifiers(
                    _active.Multiplier(PerkEffect.DoubleDamage),
                    _active.Multiplier(PerkEffect.RapidFire),
                    _active.HasEffect(PerkEffect.InstaKill));
            }
        }
    }
}
