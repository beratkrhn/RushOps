using System.Collections.Generic;
using RushOps.Save;
using RushOps.Skins;
using UnityEngine;
using UnityEngine.UI;

namespace RushOps.UI
{
    /// <summary>
    /// Shows a skin the level just unlocked on „Level geschafft!“ (issue #128): its name and the figure in it.
    /// Hidden until <see cref="LevelProgressRecorder.SkinsUnlocked"/> fires.
    /// </summary>
    public class UnlockedSkinView : MonoBehaviour
    {
        [SerializeField] private LevelProgressRecorder _recorder;
        [SerializeField] private GameObject _root;
        [SerializeField] private Text _label;
        [SerializeField] private MenuFigure _figure;

        /// <summary>The skin shown; null while hidden.</summary>
        public SkinDefinition Shown { get; private set; }

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
                _recorder.SkinsUnlocked += OnSkinsUnlocked;
            }
        }

        private void OnDisable()
        {
            if (_recorder != null)
            {
                _recorder.SkinsUnlocked -= OnSkinsUnlocked;
            }
        }

        private void OnSkinsUnlocked(IReadOnlyList<SkinDefinition> skins)
        {
            if (skins.Count == 0 || skins[0] == null)
            {
                return;
            }

            Shown = skins[0];
            if (_label != null)
            {
                _label.text = ResultText.NewSkin(Shown.DisplayName, skins.Count - 1);
            }

            if (_root != null)
            {
                _root.SetActive(true);
            }

            if (_figure != null)
            {
                _figure.Show(Shown.Walk != null ? Shown.Walk : Shown.Idle);
            }
        }
    }
}
