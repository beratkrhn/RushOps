using UnityEngine;

namespace RushOps.Puzzles
{
    /// <summary>Values of a hold-the-line zone (F-RAE-01, issue #31).</summary>
    [CreateAssetMenu(fileName = "HoldZoneConfig", menuName = "RushOps/Hold Zone Config")]
    public class HoldZoneConfig : ScriptableObject
    {
        [Tooltip("Seconds the player must stand in the zone to solve it.")]
        [SerializeField, Min(0f)] private float _duration = 10f;

        [Tooltip("Radius of the zone in units. The ring sprite is drawn for 2 units and gets scaled for other radii.")]
        [SerializeField, Min(0.5f)] private float _radius = 2f;

        [Tooltip("Text in the mission tracker.")]
        [SerializeField] private string _text = "Zone halten";

        [Header("Look")]
        [Tooltip("Opacity of the disc that grows with the progress.")]
        [SerializeField, Range(0f, 1f)] private float _fillAlpha = 0.35f;

        [Tooltip("Light intensity before any progress.")]
        [SerializeField, Min(0f)] private float _lightIdle = 0.4f;

        [Tooltip("Light intensity at full progress.")]
        [SerializeField, Min(0f)] private float _lightFull = 1.4f;

        [Tooltip("Tint of ring and light once the zone is solved.")]
        [SerializeField] private Color _solvedColor = new Color32(141, 178, 84, 255);

        /// <summary>Seconds inside needed to solve the zone.</summary>
        public float Duration => _duration;

        /// <summary>Radius of the zone in units.</summary>
        public float Radius => _radius;

        /// <summary>Text in the mission tracker.</summary>
        public string Text => _text;

        /// <summary>Opacity of the progress disc.</summary>
        public float FillAlpha => _fillAlpha;

        /// <summary>Light intensity before any progress.</summary>
        public float LightIdle => _lightIdle;

        /// <summary>Light intensity at full progress.</summary>
        public float LightFull => _lightFull;

        /// <summary>Tint of ring and light once solved.</summary>
        public Color SolvedColor => _solvedColor;
    }
}
