using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace RushOps.Puzzles
{
    /// <summary>
    /// Hold the line (F-RAE-01, F-RAE-02, issue #31): a glowing circle on the floor fills while the player stands in
    /// it and pauses while the player is outside; full means solved. The progress shows as a disc growing inside the
    /// ring, a brightening light and a bar in the mission tracker. Rules in <see cref="HoldProgress"/>, values in
    /// <see cref="HoldZoneConfig"/>.
    /// </summary>
    public class HoldZonePuzzle : Puzzle
    {
        // The ring and fill sprites are drawn for this radius in units.
        private const float SpriteRadius = 2f;

        [SerializeField] private HoldZoneConfig _holdConfig;
        [Tooltip("Whose feet must be inside the circle.")]
        [SerializeField] private Transform _player;
        [SerializeField] private SpriteRenderer _ring;
        [Tooltip("Disc scaled from 0 to the full circle with the progress.")]
        [SerializeField] private SpriteRenderer _fill;
        [SerializeField] private Light2D _light;

        private HoldProgress _progress;
        private float _shownProgress = -1f;

        /// <summary>Progress 0..1.</summary>
        public float Progress => Hold.Progress;

        /// <summary>True while the player stands in the circle.</summary>
        public bool PlayerInside { get; private set; }

        /// <summary>Radius of the circle in units.</summary>
        public float Radius => _holdConfig != null ? _holdConfig.Radius : SpriteRadius;

        /// <inheritdoc />
        public override PuzzleDisplay Display =>
            PuzzleDisplay.WithProgress(_holdConfig != null ? _holdConfig.Text : string.Empty, Progress);

        private HoldProgress Hold => _progress ??= new HoldProgress(_holdConfig != null ? _holdConfig.Duration : 0f);

        /// <inheritdoc />
        protected override void OnSolved()
        {
            Color solved = _holdConfig != null ? _holdConfig.SolvedColor : Color.green;
            if (_ring != null)
            {
                _ring.color = solved;
            }

            if (_fill != null)
            {
                _fill.color = new Color(solved.r, solved.g, solved.b, _fill.color.a);
            }

            if (_light != null)
            {
                _light.color = solved;
            }
        }

        private void Update()
        {
            if (State == PuzzleState.Active)
            {
                PlayerInside = _player != null && HoldProgress.IsInside(transform.position, Radius, _player.position);
                if (Hold.Tick(Time.deltaTime, PlayerInside))
                {
                    Solve();
                }
            }
            else
            {
                PlayerInside = false;
            }

            ShowProgress();
        }

        // Only when the progress changed, so a paused zone costs nothing.
        private void ShowProgress()
        {
            float progress = Progress;
            if (Mathf.Approximately(progress, _shownProgress))
            {
                return;
            }

            _shownProgress = progress;
            float scale = Radius / SpriteRadius;
            if (_ring != null)
            {
                _ring.transform.localScale = new Vector3(scale, scale, 1f);
            }

            if (_fill != null)
            {
                _fill.transform.localScale = new Vector3(scale * progress, scale * progress, 1f);
                _fill.enabled = progress > 0f;
                Color color = _fill.color;
                color.a = _holdConfig != null ? _holdConfig.FillAlpha : color.a;
                _fill.color = color;
            }

            if (_light != null && _holdConfig != null)
            {
                _light.intensity = Mathf.Lerp(_holdConfig.LightIdle, _holdConfig.LightFull, progress);
                _light.pointLightOuterRadius = Radius * 1.25f;
            }
        }
    }
}
