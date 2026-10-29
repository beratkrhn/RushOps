using UnityEngine;

namespace RushOps.Characters
{
    /// <summary>
    /// Shows the frame of a <see cref="DirectionalAnimation"/> that matches the current facing.
    /// Characters are not rotated (ADR 0006); other components call <see cref="SetFacing"/> instead.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class DirectionalSpriteAnimator : MonoBehaviour
    {
        [SerializeField] private DirectionalAnimation _animation;
        [SerializeField] private Direction8 _direction = Direction8.S;

        private SpriteRenderer _renderer;
        private float _time;

        /// <summary>The direction the sprite currently faces.</summary>
        public Direction8 Direction => _direction;

        /// <summary>Faces the sprite towards <paramref name="facing"/>; a zero vector keeps the current direction.</summary>
        public void SetFacing(Vector2 facing)
        {
            _direction = Direction8Math.FromVector(facing, _direction);
        }

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            if (_animation == null)
            {
                return;
            }

            _time += Time.deltaTime;
            int index = FrameAnimationMath.FrameIndex(_time, _animation.FramesPerSecond, _animation.FrameCount(_direction));
            Sprite sprite = _animation.GetFrame(_direction, index);
            if (sprite != null && _renderer.sprite != sprite)
            {
                _renderer.sprite = sprite;
            }
        }
    }
}
