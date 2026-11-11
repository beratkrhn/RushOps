using UnityEngine;
using UnityEngine.InputSystem;

namespace RushOps.Endless
{
    /// <summary>
    /// Editor and debug control of the run upgrades (issue #277): the key grants the next upgrade of the config that is
    /// not maxed, so every upgrade can be tried without clearing rounds. <see cref="Grant"/> serves tests.
    /// </summary>
    public class DebugUpgradeKey : MonoBehaviour
    {
        [SerializeField] private EndlessRunController _controller;
        [SerializeField] private Key _key = Key.U;

        private int _next;

        /// <summary>Raises the upgrade with <paramref name="id"/> by one level; false if unknown or maxed.</summary>
        public bool Grant(string id)
        {
            return _controller != null && _controller.Upgrades.Take(id);
        }

        /// <summary>Raises the next upgrade in the order of the config that is not maxed; null if all are maxed.</summary>
        public UpgradeDefinition GrantNext()
        {
            if (_controller == null)
            {
                return null;
            }

            RunUpgrades upgrades = _controller.Upgrades;
            int count = upgrades.Definitions.Count;
            for (int i = 0; i < count; i++)
            {
                UpgradeDefinition upgrade = upgrades.Definitions[(_next + i) % count];
                if (upgrades.Take(upgrade))
                {
                    _next = (_next + i + 1) % count;
                    return upgrade;
                }
            }

            return null;
        }

        private void Update()
        {
            Keyboard keyboard = Application.isEditor ? Keyboard.current : null;
            if (keyboard != null && keyboard[_key].wasPressedThisFrame)
            {
                UpgradeDefinition upgrade = GrantNext();
                if (upgrade != null)
                {
                    Debug.Log($"Upgrade: {upgrade.DisplayName} → Stufe {_controller.Upgrades.Level(upgrade)}");
                }
            }
        }
    }
}
