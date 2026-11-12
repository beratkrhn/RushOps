using UnityEngine;

namespace RushOps.Story
{
    /// <summary>Timing and look of cutscenes (issue #331); all values are start values, tuned on the device.</summary>
    [CreateAssetMenu(fileName = "CutsceneConfig", menuName = "RushOps/Cutscene Config")]
    public class CutsceneConfig : ScriptableObject
    {
        [Header("Timing (seconds)")]
        [Tooltip("Wait between a panel showing and its first box.")]
        [SerializeField, Min(0f)] private float _firstBoxDelay = 0.35f;
        [Tooltip("Wait between two boxes of the same panel.")]
        [SerializeField, Min(0f)] private float _boxDelay = 0.9f;
        [Tooltip("How long a box takes to fade in.")]
        [SerializeField, Min(0f)] private float _boxFade = 0.25f;
        [Tooltip("How long a panel fades in from and out to black.")]
        [SerializeField, Min(0f)] private float _panelFade = 0.35f;
        [Tooltip("Half period of the blinking „Weiter“ hint.")]
        [SerializeField, Min(0.05f)] private float _hintBlink = 0.5f;

        [Header("Boxes (panel pixels)")]
        [Tooltip("A box slides up by this much while it fades in.")]
        [SerializeField, Min(0f)] private float _boxSlide = 4f;
        [SerializeField, Min(0)] private int _boxPadding = 4;
        [Tooltip("Height of the speaker line.")]
        [SerializeField, Min(0)] private int _speakerLineHeight = 10;

        [Header("Colors (palette)")]
        [SerializeField] private Color _narrationFill = new Color32(255, 236, 214, 255);
        [SerializeField] private Color _speechFill = new Color32(244, 241, 232, 255);
        [SerializeField] private Color _outline = new Color32(13, 11, 20, 255);
        [SerializeField] private Color _text = new Color32(31, 29, 43, 255);

        /// <summary>The durations for <see cref="CutscenePlayback"/>.</summary>
        public CutsceneTiming Timing => new CutsceneTiming(_firstBoxDelay, _boxDelay, _boxFade, _panelFade);

        /// <summary>Half period of the blinking hint in seconds.</summary>
        public float HintBlink => _hintBlink;

        /// <summary>Slide distance of a box while it fades in, in panel pixels.</summary>
        public float BoxSlide => _boxSlide;

        /// <summary>Inner margin of a box in panel pixels.</summary>
        public int BoxPadding => _boxPadding;

        /// <summary>Height of the speaker line in panel pixels.</summary>
        public int SpeakerLineHeight => _speakerLineHeight;

        /// <summary>Fill of narration boxes.</summary>
        public Color NarrationFill => _narrationFill;

        /// <summary>Fill of speech boxes.</summary>
        public Color SpeechFill => _speechFill;

        /// <summary>Outline of the boxes.</summary>
        public Color Outline => _outline;

        /// <summary>Color of the box text.</summary>
        public Color Text => _text;
    }
}
