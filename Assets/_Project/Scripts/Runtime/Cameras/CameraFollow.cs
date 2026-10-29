using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;

namespace RushOps.Cameras
{
    /// <summary>
    /// Smoothly follows a target, keeps the view inside the level bounds and snaps to the pixel grid.
    /// <see cref="Shake"/> adds a short wobble on top, e.g. when the player takes damage.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private CameraConfig _config;
        [SerializeField] private Transform _target;
        [Tooltip("Tilemap whose bounds define the level area the camera must stay inside.")]
        [SerializeField] private Tilemap _boundsTilemap;

        private Camera _camera;
        private int _pixelsPerUnit;
        private Rect _bounds;
        private bool _hasBounds;
        private Vector2 _position;
        private Vector2 _velocity;
        private float _shakeElapsed = -1f;
        private float _shakeDuration;
        private float _shakeStrength;
        private float _shakeFrequency;

        /// <summary>The transform the camera follows. Setting it moves the camera to the target immediately.</summary>
        public Transform Target
        {
            get => _target;
            set
            {
                _target = value;
                SnapToTarget();
            }
        }

        /// <summary>Level area in world units the view is kept inside.</summary>
        public Rect Bounds => _bounds;

        /// <summary>Half width and half height of the visible area in world units.</summary>
        public Vector2 HalfExtents
        {
            get
            {
                float halfHeight = _camera.orthographicSize;
                return new Vector2(halfHeight * _camera.aspect, halfHeight);
            }
        }

        /// <summary>Current shake offset in units; zero when not shaking.</summary>
        public Vector2 ShakeOffset { get; private set; }

        /// <summary>
        /// Shakes the view for <paramref name="duration"/> seconds of real time, up to <paramref name="strength"/>
        /// units, fading out. A new shake replaces a running one. The shake may briefly show beyond the level bounds.
        /// </summary>
        public void Shake(float strength, float duration, float frequency)
        {
            _shakeElapsed = 0f;
            _shakeStrength = strength;
            _shakeDuration = duration;
            _shakeFrequency = frequency;
        }

        /// <summary>Overrides the level bounds, e.g. when a level is loaded at runtime.</summary>
        public void SetBounds(Rect bounds)
        {
            _bounds = bounds;
            _hasBounds = true;
        }

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            if (TryGetComponent(out PixelPerfectCamera pixelPerfectCamera))
            {
                _pixelsPerUnit = pixelPerfectCamera.assetsPPU;
            }

            if (_boundsTilemap != null)
            {
                SetBounds(GetWorldBounds(_boundsTilemap));
            }
        }

        private void Start()
        {
            SnapToTarget();
        }

        private void LateUpdate()
        {
            if (_target == null || _config == null)
            {
                return;
            }

            Vector2 desired = ClampIfBounded(_target.position);
            _position = Vector2.SmoothDamp(_position, desired, ref _velocity, _config.SmoothTime);
            UpdateShake();
            Apply(ClampIfBounded(_position));
        }

        // Real time, so the shake keeps going during a hitstop.
        private void UpdateShake()
        {
            if (_shakeElapsed < 0f)
            {
                return;
            }

            _shakeElapsed += Time.unscaledDeltaTime;
            ShakeOffset = CameraFollowMath.ShakeOffset(_shakeElapsed, _shakeDuration, _shakeStrength, _shakeFrequency);
            if (_shakeElapsed >= _shakeDuration)
            {
                _shakeElapsed = -1f;
                ShakeOffset = Vector2.zero;
            }
        }

        private void SnapToTarget()
        {
            if (_target == null || _camera == null)
            {
                return;
            }

            _velocity = Vector2.zero;
            Apply(ClampIfBounded(_target.position));
        }

        private Vector2 ClampIfBounded(Vector2 center)
        {
            return _hasBounds ? CameraFollowMath.ClampToBounds(center, _bounds, HalfExtents) : center;
        }

        private void Apply(Vector2 position)
        {
            _position = position;
            // The offset is not stored in _position, so the follow does not drift with the shake.
            Vector2 snapped = CameraFollowMath.SnapToPixelGrid(position + ShakeOffset, _pixelsPerUnit);
            transform.position = new Vector3(snapped.x, snapped.y, transform.position.z);
        }

        private static Rect GetWorldBounds(Tilemap tilemap)
        {
            Bounds local = tilemap.localBounds;
            Vector3 min = tilemap.transform.TransformPoint(local.min);
            Vector3 max = tilemap.transform.TransformPoint(local.max);
            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }
    }
}
