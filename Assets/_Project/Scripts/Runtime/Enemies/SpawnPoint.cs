using UnityEngine;

namespace RushOps.Enemies
{
    /// <summary>
    /// Fixed place where enemies appear (F-GEG-02), placed in the scene. Shows a marker on the floor while a
    /// spawn is announced; the <see cref="EnemySpawner"/> drives it.
    /// </summary>
    public class SpawnPoint : MonoBehaviour
    {
        [Tooltip("Floor marker shown before an enemy appears here.")]
        [SerializeField] private SpriteRenderer _marker;

        [Tooltip("True for a breach in the arena wall (F-END-15): enemies crawl out of it.")]
        [SerializeField] private bool _isBreach;

        [Tooltip("Renders the breach; optional.")]
        [SerializeField] private SpriteRenderer _hole;

        [SerializeField] private Sprite _holeClosed;
        [SerializeField] private Sprite _holeOpen;

        private float _openFor;
        private bool _announced;

        /// <summary>True if enemies crawl out here (F-END-15).</summary>
        public bool IsBreach => _isBreach;

        /// <summary>True while the breach shows its open sprite.</summary>
        public bool IsOpen => _announced || _openFor > 0f;

        /// <summary>Opens the breach while a spawn is announced here, closes it again; ignored without breach.</summary>
        public void SetAnnounced(bool announced)
        {
            _announced = _isBreach && announced;
            RefreshHole();
        }

        /// <summary>Keeps the breach open for <paramref name="seconds"/> while an enemy crawls out.</summary>
        public void HoldOpen(float seconds)
        {
            if (_isBreach)
            {
                _openFor = Mathf.Max(_openFor, seconds);
                RefreshHole();
            }
        }

        /// <summary>Where enemies appear (their feet).</summary>
        public Vector2 Position => transform.position;

        /// <summary>Shows or hides the marker.</summary>
        public void SetMarkerVisible(bool visible)
        {
            if (_marker != null && _marker.enabled != visible)
            {
                _marker.enabled = visible;
            }
        }

        private void Awake()
        {
            SetMarkerVisible(false);
            RefreshHole();
        }

        private void Update()
        {
            if (_openFor > 0f)
            {
                _openFor -= Time.deltaTime;
                if (_openFor <= 0f)
                {
                    _openFor = 0f;
                    RefreshHole();
                }
            }
        }

        private void RefreshHole()
        {
            if (_hole != null)
            {
                _hole.sprite = IsOpen ? _holeOpen : _holeClosed;
            }
        }
    }
}
