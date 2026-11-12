using System;
using UnityEngine;

namespace RushOps.Story
{
    /// <summary>
    /// A cutscene (issue #331, F-STO-02): pictures in order, each with its text boxes. Built from data by the editor
    /// (e.g. <c>Data/Story/IntroCutscene.asset</c>) and played by <see cref="CutscenePlayer"/>.
    /// </summary>
    [CreateAssetMenu(fileName = "Cutscene", menuName = "RushOps/Cutscene")]
    public class CutsceneDefinition : ScriptableObject
    {
        [Tooltip("Size of every panel image in pixels; box positions use this grid.")]
        [SerializeField] private Vector2Int _panelSize = new Vector2Int(180, 320);
        [SerializeField] private CutscenePanel[] _panels = Array.Empty<CutscenePanel>();

        /// <summary>Size of the panel images in pixels.</summary>
        public Vector2Int PanelSize => _panelSize;

        /// <summary>Number of panels.</summary>
        public int PanelCount => _panels != null ? _panels.Length : 0;

        /// <summary>Panel <paramref name="index"/>.</summary>
        public CutscenePanel GetPanel(int index)
        {
            return _panels[index];
        }

        /// <summary>The most boxes any panel has.</summary>
        public int MaxBoxes
        {
            get
            {
                int max = 0;
                for (int i = 0; i < PanelCount; i++)
                {
                    max = Math.Max(max, _panels[i].BoxCount);
                }

                return max;
            }
        }

        /// <summary>Box count of every panel, for <see cref="CutscenePlayback"/>.</summary>
        public int[] BoxCounts()
        {
            var counts = new int[PanelCount];
            for (int i = 0; i < counts.Length; i++)
            {
                counts[i] = _panels[i].BoxCount;
            }

            return counts;
        }

        /// <summary>Replaces the content; used by the editor builder and by tests.</summary>
        public void SetContent(Vector2Int panelSize, CutscenePanel[] panels)
        {
            _panelSize = panelSize;
            _panels = panels ?? Array.Empty<CutscenePanel>();
        }
    }
}
