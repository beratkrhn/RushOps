using System;
using RushOps.Characters;
using RushOps.Player;
using UnityEngine;

namespace RushOps.Enemies
{
    /// <summary>
    /// The zombie dog (issue #263, F-GEG-05, F-GEG-06): a thin adapter that runs <see cref="DogLunge"/> on a pooled
    /// <see cref="Enemy"/>. It chases via the flow field like any enemy; in range of the player it crouches (own pose, a
    /// white flash and a growl), then leaps in a straight line to where the player stood and rests before it chases
    /// again. Health, hits, death, points and perks come from <see cref="Enemy"/>; the contact damage is dealt by
    /// <see cref="PlayerHealth"/> as with every enemy. While a scarecrow lures the enemies, dogs do not start a lunge.
    /// </summary>
    [RequireComponent(typeof(Enemy))]
    [RequireComponent(typeof(EnemyMovement))]
    public class Dog : MonoBehaviour
    {
        [SerializeField] private DirectionalSpriteAnimator _view;

        [Tooltip("Gallop while chasing.")]
        [SerializeField] private DirectionalAnimation _run;

        [Tooltip("Announced lunge: low, trembling pose.")]
        [SerializeField] private DirectionalAnimation _crouch;

        [Tooltip("Stretched out in the air.")]
        [SerializeField] private DirectionalAnimation _lunge;

        [Tooltip("Standing and panting during the recovery after a lunge.")]
        [SerializeField] private DirectionalAnimation _rest;

        [Tooltip("Layers that stop a lunge, usually the walls.")]
        [SerializeField] private LayerMask _wallLayers;

        private Enemy _enemy;
        private EnemyMovement _movement;
        private Rigidbody2D _body;
        private DogLunge _logic;
        private DogState _shown = DogState.Chase;
        private Transform _healthOf;
        private PlayerHealth _playerHealth;
        private bool _playerWasInvulnerable;

        /// <summary>Raised when the dog starts to crouch, for the growl.</summary>
        public event Action<Dog> Crouched;

        /// <summary>Raised when the dog leaps, for the snarl.</summary>
        public event Action<Dog> Lunged;

        /// <summary>The enemy this dog drives.</summary>
        public Enemy Enemy => _enemy;

        /// <summary>Current state of the lunge.</summary>
        public DogState State => _logic != null ? _logic.State : DogState.Chase;

        /// <summary>Where the player stood when the crouch began; the end point of the lunge.</summary>
        public Vector2 LungeTarget => _logic != null ? _logic.Target : Vector2.zero;

        private void Awake()
        {
            _enemy = GetComponent<Enemy>();
            _movement = GetComponent<EnemyMovement>();
            _body = GetComponent<Rigidbody2D>();
            DogConfig config = _enemy.Definition != null ? _enemy.Definition.Dog : null;
            if (config != null)
            {
                _logic = new DogLunge(config);
                _logic.Crouched += OnCrouched;
                _logic.Lunged += OnLunged;
            }
        }

        private void OnDestroy()
        {
            if (_logic != null)
            {
                _logic.Crouched -= OnCrouched;
                _logic.Lunged -= OnLunged;
            }
        }

        // The pool activates the dog before it spawns it; every life starts chasing.
        private void OnEnable()
        {
            _logic?.Reset();
            Show(DogState.Chase);
        }

        private void FixedUpdate()
        {
            Transform target = _enemy.Target;
            if (_logic == null || target == null || _enemy.IsDead)
            {
                return;
            }

            CacheHealth(target);
            // Read before the physics step, so a hit landed in this step does not count as invulnerable already.
            _playerWasInvulnerable = _playerHealth != null && _playerHealth.IsInvulnerable;

            // A lured dog walks to the scarecrow; it does not start a lunge at the player meanwhile.
            bool lured = _enemy.Navigator != null && _enemy.Navigator.HasLure;
            if (!(lured && _logic.State == DogState.Chase))
            {
                _logic.Tick(Time.fixedDeltaTime, _body.position, target.position);
            }

            if (_logic.State == DogState.Chase)
            {
                if (_movement.IsDriven)
                {
                    _movement.ResumeChase();
                }
            }
            else
            {
                _movement.Drive(_logic.Velocity);
            }

            Show(_logic.State);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            OnContact(collision);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            OnContact(collision);
        }

        private void OnContact(Collision2D collision)
        {
            if (_logic == null || _logic.State != DogState.Lunge)
            {
                return;
            }

            if ((_wallLayers.value & (1 << collision.gameObject.layer)) != 0)
            {
                _logic.NotifyBlocked();
                return;
            }

            Transform target = _enemy.Target;
            if (target != null && collision.rigidbody != null && collision.rigidbody.transform == target)
            {
                // The damage itself is dealt by PlayerHealth; this only ends a lunge that landed.
                _logic.OnPlayerContact(_enemy.Definition != null ? _enemy.Definition.ContactDamage : 0,
                    _playerWasInvulnerable);
            }
        }

        private void OnCrouched()
        {
            if (_view != null && _enemy.Target != null)
            {
                _view.SetFacing((Vector2)_enemy.Target.position - _body.position);
            }

            _enemy.Flash();
            Crouched?.Invoke(this);
        }

        private void OnLunged()
        {
            Lunged?.Invoke(this);
        }

        private void CacheHealth(Transform target)
        {
            if (_healthOf != target)
            {
                _healthOf = target;
                _playerHealth = target.GetComponent<PlayerHealth>();
            }
        }

        private void Show(DogState state)
        {
            if (_view == null || state == _shown)
            {
                return;
            }

            _shown = state;
            DirectionalAnimation loop = state switch
            {
                DogState.Crouch => _crouch,
                DogState.Lunge => _lunge,
                DogState.Recovery => _rest,
                _ => _run,
            };
            if (loop != null)
            {
                _view.SetAnimations(loop, null, null, null);
            }
        }
    }
}
