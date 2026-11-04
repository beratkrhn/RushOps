using UnityEngine;

namespace RushOps.Characters
{
    /// <summary>
    /// Shows the frame of a <see cref="DirectionalAnimation"/> that matches the current facing.
    /// Characters are not rotated (ADR 0006); other components call <see cref="SetFacing"/> instead.
    /// Besides the looping base animation it plays an optional hit reaction and death (issue #100).
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class DirectionalSpriteAnimator : MonoBehaviour
    {
        [Tooltip("Looping base animation, e.g. walking.")]
        [SerializeField] private DirectionalAnimation _animation;

        [Tooltip("Loop that replaces the base loop while moving, e.g. the player walking instead of standing; none if empty.")]
        [SerializeField] private DirectionalAnimation _move;

        [Tooltip("Loop that replaces the base loop while alerted, e.g. the boomer swelling near the player; none if empty.")]
        [SerializeField] private DirectionalAnimation _alert;

        [Tooltip("Short hit reaction, played once; none if empty.")]
        [SerializeField] private DirectionalAnimation _hit;

        [Tooltip("Death, played once and held on its last frame; none if empty.")]
        [SerializeField] private DirectionalAnimation _death;

        [SerializeField] private Direction8 _direction = Direction8.S;

        private SpriteRenderer _renderer;
        private readonly SpriteAnimationState _state = new SpriteAnimationState();
        private float _moveRate = 1f;

        /// <summary>The direction the sprite currently faces.</summary>
        public Direction8 Direction => _direction;

        /// <summary>The animation currently playing.</summary>
        public SpriteAnimationMode Mode => _state.Mode;

        /// <summary>True while the alert loop is requested.</summary>
        public bool IsAlert => _state.IsAlert;

        /// <summary>True if a death animation is assigned.</summary>
        public bool HasDeath => _death != null && _death.FrameCount(_direction) > 0;

        /// <summary>Seconds the death animation takes in the current direction; 0 without one.</summary>
        public float DeathDuration => HasDeath ? _death.Duration(_direction) : 0f;

        /// <summary>Faces the sprite towards <paramref name="facing"/>; a zero vector keeps the current direction.</summary>
        public void SetFacing(Vector2 facing)
        {
            _direction = Direction8Math.FromVector(facing, _direction);
        }

        /// <summary>Restarts the looping base animation, e.g. when a pooled character is reused.</summary>
        public void PlayLoop()
        {
            _state.PlayLoop();
            ShowCurrentFrame();
        }

        /// <summary>
        /// Plays the move loop instead of the base loop while <paramref name="moving"/>, at <paramref name="rate"/> times
        /// its frame rate, so the steps match the walking speed (issue #102).
        /// </summary>
        public void SetMoving(bool moving, float rate = 1f)
        {
            _moveRate = rate > 0f ? rate : 1f;
            _state.SetMoving(moving);
        }

        /// <summary>
        /// Plays the alert loop instead of the base loop while <paramref name="alert"/> is true; without an alert
        /// animation the base loop keeps playing.
        /// </summary>
        public void SetAlert(bool alert)
        {
            _state.SetAlert(alert);
            ShowCurrentFrame();
        }

        /// <summary>Plays the hit reaction once, unless there is none or the character is dying.</summary>
        public void PlayHit()
        {
            if (_hit != null && _hit.FrameCount(_direction) > 0 && _state.PlayHit())
            {
                ShowCurrentFrame();
            }
        }

        /// <summary>Plays the death once; it then holds its last frame until <see cref="PlayLoop"/>.</summary>
        public void PlayDeath()
        {
            if (HasDeath)
            {
                _state.PlayDeath();
                ShowCurrentFrame();
            }
        }

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            bool walking = _state.Mode == SpriteAnimationMode.Loop && _state.IsMoving && !_state.IsAlert && _move != null;
            _state.Tick(Time.deltaTime * (walking ? _moveRate : 1f), _hit != null ? _hit.Duration(_direction) : 0f);
            ShowCurrentFrame();
        }

        private void ShowCurrentFrame()
        {
            DirectionalAnimation animation = _state.Mode switch
            {
                SpriteAnimationMode.Hit => _hit,
                SpriteAnimationMode.Death => _death,
                _ => _state.IsAlert && _alert != null ? _alert : _state.IsMoving && _move != null ? _move : _animation,
            };
            if (animation == null || _renderer == null)
            {
                return;
            }

            Sprite sprite = animation.GetFrame(_direction, animation.FrameIndexAt(_state.Time, _direction));
            if (sprite != null && _renderer.sprite != sprite)
            {
                _renderer.sprite = sprite;
            }
        }
    }
}
