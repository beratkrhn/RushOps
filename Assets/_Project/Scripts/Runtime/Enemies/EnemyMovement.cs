using RushOps.Characters;
using RushOps.Navigation;
using UnityEngine;

namespace RushOps.Enemies
{
    /// <summary>
    /// Moves an enemy along the shared flow field (ADR 0003) with separation from the other enemies,
    /// and faces its sprite in the move direction. Walls and other enemies stop it through its collider.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyMovement : MonoBehaviour
    {
        [SerializeField] private DirectionalSpriteAnimator _view;

        private Rigidbody2D _body;
        private Collider2D _collider;
        private FlowFieldNavigator _navigator;
        private float _speed;
        private bool _moving;

        /// <summary>Velocity set in the last physics step.</summary>
        public Vector2 Velocity => _body != null ? _body.linearVelocity : Vector2.zero;

        /// <summary>Starts chasing via <paramref name="navigator"/> at <paramref name="speed"/> units per second.</summary>
        public void Begin(FlowFieldNavigator navigator, float speed)
        {
            Stop();
            _navigator = navigator;
            _speed = speed;
            _moving = navigator != null;
            if (_moving)
            {
                _navigator.Register(transform);
            }
        }

        /// <summary>Stops moving and leaves the separation.</summary>
        public void Stop()
        {
            if (_moving && _navigator != null)
            {
                _navigator.Unregister(transform);
            }

            _moving = false;
            if (_body != null)
            {
                _body.linearVelocity = Vector2.zero;
            }
        }

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            _collider = GetComponent<Collider2D>();
        }

        private void FixedUpdate()
        {
            if (!_moving)
            {
                return;
            }

            // Navigate with the body center; the transform sits at the feet below it.
            Vector2 center = (Vector2)transform.position + (_collider != null ? _collider.offset : Vector2.zero);
            Vector2 direction = _navigator.Steer(transform, center);
            _body.linearVelocity = direction * _speed;
            if (_view != null)
            {
                _view.SetFacing(direction);
            }
        }

        private void OnDisable()
        {
            Stop();
        }
    }
}
