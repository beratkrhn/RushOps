using System;
using RushOps.Audio;
using RushOps.Navigation;
using UnityEngine;

namespace RushOps.Levels
{
    /// <summary>
    /// A gate between two zones (F-LVL-02): a block of cells around the transform that stops the player, enemies
    /// and shots, and keeps the flow field from leading through it. It opens once its <see cref="IGateCondition"/>
    /// is met: the bars shake and sink into the floor with a sound, then the passage is free and the flow field
    /// is rebuilt.
    /// </summary>
    public class Gate : MonoBehaviour
    {
        [SerializeField] private GateConfig _config;
        [Tooltip("Component on this or another object that implements IGateCondition. Without one the gate stays closed.")]
        [SerializeField] private MonoBehaviour _condition;
        [Tooltip("Width and height of the gate in cells (units), centered on the transform.")]
        [SerializeField] private Vector2Int _size = new Vector2Int(4, 1);
        [Tooltip("Collider that blocks the passage while the gate is closed.")]
        [SerializeField] private Collider2D _blocker;
        [Tooltip("Bar sprites that shake and sink when the gate opens.")]
        [SerializeField] private SpriteRenderer[] _bars;
        [SerializeField] private FlowFieldNavigator _navigator;
        [SerializeField] private SfxPlayer _sfx;
        [SerializeField] private AudioClip _openClip;

        private GateLogic _logic;
        private Vector3[] _barPositions;

        /// <summary>Raised once when the condition is met and the opening starts.</summary>
        public event Action<Gate> Opening;

        /// <summary>Raised once when the passage is free.</summary>
        public event Action<Gate> Opened;

        /// <summary>Current state.</summary>
        public GateState State => _logic != null ? _logic.State : GateState.Closed;

        /// <summary>The condition the gate waits for, or null.</summary>
        public IGateCondition Condition => _condition as IGateCondition;

        /// <summary>Area the gate blocks, in world units.</summary>
        public Bounds Area => new Bounds(transform.position, new Vector3(_size.x, _size.y, 0f));

        private void Awake()
        {
            float duration = _config != null ? _config.OpenDuration : 0f;
            _logic = new GateLogic(Condition, duration);
            _logic.Opening += OnOpening;
            _logic.Opened += OnOpened;

            int count = _bars != null ? _bars.Length : 0;
            _barPositions = new Vector3[count];
            for (int i = 0; i < count; i++)
            {
                _barPositions[i] = _bars[i] != null ? _bars[i].transform.localPosition : Vector3.zero;
            }
        }

        private void Start()
        {
            if (_navigator != null && _logic.State != GateState.Open)
            {
                _navigator.SetBlocked(Area, true);
            }
        }

        private void Update()
        {
            if (_logic.State == GateState.Open)
            {
                return;
            }

            _logic.Tick(Time.deltaTime);
            if (_logic.State == GateState.Opening)
            {
                UpdateBars(_logic.OpenProgress);
            }
        }

        private void OnValidate()
        {
            if (_condition != null && !(_condition is IGateCondition))
            {
                Debug.LogWarning($"{name}: {_condition.GetType().Name} does not implement IGateCondition", this);
                _condition = null;
            }
        }

        private void OnOpening()
        {
            if (_sfx != null && _config != null)
            {
                _sfx.Play(_openClip, _config.OpenVolume, 0f);
            }

            Opening?.Invoke(this);
        }

        private void OnOpened()
        {
            UpdateBars(1f);
            if (_blocker != null)
            {
                _blocker.enabled = false;
            }

            if (_navigator != null)
            {
                _navigator.SetBlocked(Area, false);
            }

            Opened?.Invoke(this);
        }

        // Bars are pivoted in their center: shrinking them while moving them down keeps their foot on the floor.
        private void UpdateBars(float progress)
        {
            float duration = _config != null ? _config.OpenDuration : 0f;
            float shakeDuration = _config != null ? _config.ShakeDuration : 0f;
            float sink = GateMath.Sink(progress, duration, shakeDuration);
            float shake = GateMath.Shake(progress, duration, shakeDuration, _config != null ? _config.ShakeAmplitude : 0f);
            for (int i = 0; i < _barPositions.Length; i++)
            {
                SpriteRenderer bar = _bars[i];
                if (bar == null)
                {
                    continue;
                }

                float height = 1f - sink;
                bar.enabled = height > 0f;
                bar.transform.localScale = new Vector3(1f, height, 1f);
                bar.transform.localPosition = _barPositions[i] + new Vector3(shake, -0.5f * sink, 0f);
            }
        }
    }
}
