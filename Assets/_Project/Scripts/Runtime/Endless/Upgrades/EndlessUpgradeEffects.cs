using RushOps.Loot;
using RushOps.Perks;
using RushOps.Player;
using RushOps.Rush;
using UnityEngine;

namespace RushOps.Endless
{
    /// <summary>
    /// Thin adapter that hands the combined values of the run upgrades (<see cref="RunUpgrades"/>, F-END-12) to the
    /// player and the loot systems of the endless arena: extra hearts, move speed, fire rate, damage, dash cooldown,
    /// perk durations, perk drop chance, gold magnet and the rush meter. Works on the <see cref="RunUpgrades.Changed"/>
    /// event only, so nothing runs per frame. Only the endless arena has it; the campaign keeps all factors at 1.
    /// </summary>
    public class EndlessUpgradeEffects : MonoBehaviour
    {
        [SerializeField] private EndlessRunController _controller;
        [SerializeField] private PlayerHealth _health;
        [SerializeField] private PlayerMovement _movement;
        [SerializeField] private PlayerWeapon _weapon;
        [SerializeField] private PlayerDash _dash;
        [SerializeField] private PlayerPerks _perks;
        [SerializeField] private PlayerSuperWeapon _superWeapon;
        [SerializeField] private CompanionDrone _drone;
        [SerializeField] private Scarecrow _scarecrow;
        [SerializeField] private PlayerRush _rush;
        [SerializeField] private PerkDropper _perkDropper;
        [SerializeField] private GoldPickupPool _gold;

        private RunUpgrades _upgrades;

        /// <summary>The upgrades whose values are applied; null before enable.</summary>
        public RunUpgrades Upgrades => _upgrades;

        private void OnEnable()
        {
            if (_controller == null)
            {
                return;
            }

            _upgrades = _controller.Upgrades;
            _upgrades.Changed += Apply;
            Apply();
        }

        private void OnDisable()
        {
            if (_upgrades != null)
            {
                _upgrades.Changed -= Apply;
            }
        }

        /// <summary>Hands the current values to every system; called on every change of the upgrades.</summary>
        public void Apply()
        {
            if (_upgrades == null)
            {
                return;
            }

            if (_health != null)
            {
                _health.ExtraMax = _upgrades.ExtraHearts;
            }

            if (_movement != null)
            {
                _movement.UpgradeMultiplier = _upgrades.MoveSpeedFactor;
            }

            if (_weapon != null)
            {
                _weapon.UpgradeFireRateFactor = _upgrades.FireRateFactor;
                _weapon.UpgradeDamageFactor = _upgrades.DamageFactor;
            }

            if (_dash != null)
            {
                _dash.CooldownFactor = _upgrades.DashCooldownFactor;
            }

            float duration = _upgrades.PerkDurationFactor;
            if (_perks != null)
            {
                _perks.DurationFactor = duration;
            }

            if (_superWeapon != null)
            {
                _superWeapon.DurationFactor = duration;
            }

            if (_drone != null)
            {
                _drone.DurationFactor = duration;
            }

            if (_scarecrow != null)
            {
                _scarecrow.DurationFactor = duration;
            }

            if (_rush != null)
            {
                _rush.KillsReduction = _upgrades.RushKillsReduction;
            }

            if (_perkDropper != null)
            {
                _perkDropper.ChanceBonus = _upgrades.PerkChanceBonus;
            }

            if (_gold != null)
            {
                _gold.MagnetBonus = _upgrades.MagnetRadiusBonus;
            }
        }
    }
}
