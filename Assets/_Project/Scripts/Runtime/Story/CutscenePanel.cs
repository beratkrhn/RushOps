using System;
using UnityEngine;

namespace RushOps.Story
{
    /// <summary>One picture of a cutscene with its text boxes in reading order (issue #331).</summary>
    [Serializable]
    public class CutscenePanel
    {
        [SerializeField] private Sprite _image;
        [SerializeField] private CutsceneBox[] _boxes = Array.Empty<CutsceneBox>();

        /// <summary>Creates an empty panel (for serialization).</summary>
        public CutscenePanel()
        {
        }

        /// <summary>Creates a panel.</summary>
        public CutscenePanel(Sprite image, CutsceneBox[] boxes)
        {
            _image = image;
            _boxes = boxes ?? Array.Empty<CutsceneBox>();
        }

        /// <summary>The full-screen picture.</summary>
        public Sprite Image => _image;

        /// <summary>Number of text boxes.</summary>
        public int BoxCount => _boxes != null ? _boxes.Length : 0;

        /// <summary>Text box <paramref name="index"/> in reading order.</summary>
        public CutsceneBox GetBox(int index)
        {
            return _boxes[index];
        }
    }
}
