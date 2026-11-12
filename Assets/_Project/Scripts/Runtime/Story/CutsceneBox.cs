using System;
using UnityEngine;

namespace RushOps.Story
{
    /// <summary>
    /// One text box of a cutscene panel (issue #331, F-STO-02): who speaks, what is said, and where the box sits.
    /// Positions are in panel pixels from the top left corner of the panel image.
    /// </summary>
    [Serializable]
    public class CutsceneBox
    {
        [SerializeField] private CutsceneBoxKind _kind;
        [Tooltip("Name in the first line of a speech box, e.g. „HELD“. Empty for narration.")]
        [SerializeField] private string _speaker = "";
        [SerializeField] private Color32 _speakerColor = new Color32(200, 50, 60, 255);
        [SerializeField, TextArea] private string _text = "";
        [Tooltip("Top left corner in panel pixels, y downwards.")]
        [SerializeField] private Vector2Int _position;
        [Tooltip("Width in panel pixels; the height follows the text.")]
        [SerializeField] private int _width = 160;

        /// <summary>Creates an empty box (for serialization).</summary>
        public CutsceneBox()
        {
        }

        /// <summary>Creates a box.</summary>
        public CutsceneBox(CutsceneBoxKind kind, string speaker, Color32 speakerColor, string text, Vector2Int position, int width)
        {
            _kind = kind;
            _speaker = speaker ?? "";
            _speakerColor = speakerColor;
            _text = text ?? "";
            _position = position;
            _width = width;
        }

        /// <summary>Narration or speech.</summary>
        public CutsceneBoxKind Kind => _kind;

        /// <summary>Speaker shown in the first line; empty for narration.</summary>
        public string Speaker => _speaker;

        /// <summary>Color of the speaker's name.</summary>
        public Color32 SpeakerColor => _speakerColor;

        /// <summary>True when the box has a speaker line.</summary>
        public bool HasSpeaker => _kind == CutsceneBoxKind.Speech && !string.IsNullOrEmpty(_speaker);

        /// <summary>German text of the box.</summary>
        public string Text => _text;

        /// <summary>Top left corner in panel pixels, y downwards.</summary>
        public Vector2Int Position => _position;

        /// <summary>Width in panel pixels.</summary>
        public int Width => _width;
    }
}
