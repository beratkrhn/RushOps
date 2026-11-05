using System.Collections.Generic;
using RushOps.Combat;
using RushOps.Save;
using UnityEngine;
using UnityEngine.UI;

namespace RushOps.UI
{
    /// <summary>„Neue Waffe“ on „Level geschafft!“ after a boss victory unlocked a weapon (issue #176, W4).</summary>
    public class UnlockedWeaponView : MonoBehaviour
    {
        [SerializeField] private LevelProgressRecorder _recorder;
        [SerializeField] private GameObject _root;
        [SerializeField] private Text _name;
        [SerializeField] private Image _icon;

        /// <summary>The weapon shown; null until one was unlocked.</summary>
        public WeaponDefinition Shown { get; private set; }

        private void Awake()
        {
            if (_root != null)
            {
                _root.SetActive(false);
            }
        }

        private void OnEnable()
        {
            if (_recorder != null)
            {
                _recorder.WeaponsUnlocked += OnWeaponsUnlocked;
            }
        }

        private void OnDisable()
        {
            if (_recorder != null)
            {
                _recorder.WeaponsUnlocked -= OnWeaponsUnlocked;
            }
        }

        private void OnWeaponsUnlocked(IReadOnlyList<WeaponDefinition> weapons)
        {
            if (weapons.Count == 0 || weapons[0] == null)
            {
                return;
            }

            Shown = weapons[0];
            if (_name != null)
            {
                _name.text = Shown.DisplayName;
            }

            if (_icon != null)
            {
                _icon.sprite = Shown.Icon;
                _icon.enabled = Shown.Icon != null;
            }

            if (_root != null)
            {
                _root.SetActive(true);
            }
        }
    }
}
