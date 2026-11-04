using System;
using RushOps.Characters;
using RushOps.Combat;
using RushOps.Feel;
using RushOps.Navigation;
using UnityEngine;

namespace RushOps.Enemies
{
    /// <summary>
    /// A pooled enemy (F-GEG-01, F-GEG-04, NF-02): health and contact damage from its <see cref="EnemyDefinition"/>,
    /// takes projectile hits with a white flash, a knockback (issue #12) and a hit reaction, and on death plays its
    /// death animation to the end and fades out before it hands itself back to its pool (issue #100); without a
    /// death animation it shrinks while fading. Once per life it reports coming close to
    /// the chased target (<see cref="Approached"/>, issue #20). An enemy with an <see cref="EnemyDefinition.Explosion"/>
    /// dies as soon as it touches its target, so it blows up there (boomer, issue #22). Movement is done by
    /// <see cref="EnemyMovement"/>; this component switches it on and off. The flash needs a material with
    /// the <c>RushOps/Sprite Flash</c> shader on the renderer.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(EnemyMovement))]
    public class Enemy : MonoBehaviour, IDamageable, IContactDamage, IExplosionTarget, IKnockbackable
    {
        [SerializeField] private EnemyDefinition _definition;
        [SerializeField] private FeelConfig _feel;
        [SerializeField] private SpriteRenderer _renderer;

        [Tooltip("Plays the hit and death animations; optional.")]
        [SerializeField] private DirectionalSpriteAnimator _view;

        private static readonly int FlashAmountId = Shader.PropertyToID("_FlashAmount");

        private Health _health;
        private EnemyMovement _movement;
        private Rigidbody2D _body;
        private Collider2D _collider;
        private Action<Enemy> _release;
        private float _dyingTime = -1f;
        private float _deathAnimationTime;
        private float _sinceHit = -1f;
        private MaterialPropertyBlock _block;
        private readonly ApproachTrigger _approach = new ApproachTrigger();
        private Transform _target;

        /// <summary>Raised on every hit that counted, including the killing one (then <see cref="IsDead"/> is true).</summary>
        public event Action<Enemy> Hit;

        /// <summary>Raised when the enemy dies, before the death effect.</summary>
        public event Action<Enemy> Died;

        /// <summary>Raised once per life when the enemy first comes within <see cref="EnemyDefinition.ApproachDistance"/> of its target.</summary>
        public event Action<Enemy> Approached;

        /// <summary>True while the enemy shows the white hit flash.</summary>
        public bool IsFlashing => _sinceHit >= 0f;

        /// <summary>Enemy type.</summary>
        public EnemyDefinition Definition => _definition;

        /// <summary>Current hit points.</summary>
        public int CurrentHealth => _health != null ? _health.Current : 0;

        /// <summary>True from the killing hit until the enemy returns to the pool.</summary>
        public bool IsDead => _health != null && _health.IsDead;

        /// <summary>Center of the body, where shots should go; the transform sits at the feet.</summary>
        public Vector2 AimPoint => _collider != null ? (Vector2)_collider.bounds.center : (Vector2)transform.position;

        /// <summary>Seconds from the killing hit until the enemy returns to its pool: death animation plus fade.</summary>
        public float DeathTime => EnemyDeathMath.TotalDuration(
            _view != null ? _view.DeathDuration : 0f, _definition != null ? _definition.DeathDuration : 0f);

        /// <inheritdoc />
        public Vector2 ExplosionPoint => AimPoint;

        /// <inheritdoc />
        public bool CanBeHitByExplosion => _health != null && !_health.IsDead && isActiveAndEnabled;

        /// <inheritdoc />
        public int ContactDamage => _definition != null && !IsDead ? _definition.ContactDamage : 0;

        /// <summary>
        /// Places the enemy at <paramref name="position"/> with full health, chasing via <paramref name="navigator"/>.
        /// <paramref name="release"/> is called once the death effect ended; without it the enemy deactivates itself.
        /// </summary>
        public void Spawn(Vector2 position, FlowFieldNavigator navigator, Action<Enemy> release)
        {
            _release = release;
            _health = new Health(_definition != null ? _definition.Health : 1);
            _dyingTime = -1f;
            SetFlash(-1f);
            transform.position = position;
            transform.localScale = Vector3.one;
            _body.position = position;
            _body.linearVelocity = Vector2.zero;
            _body.simulated = true;
            if (_definition != null)
            {
                _body.mass = _definition.Mass;
            }

            SetAlpha(1f);
            if (_view != null)
            {
                _view.PlayLoop();
            }

            _target = navigator != null ? navigator.Target : null;
            _approach.Reset(_definition != null ? _definition.ApproachDistance : 0f);
            _movement.Begin(
                navigator, _definition != null ? _definition.MoveSpeed : 0f, _definition != null ? _definition.SeparationScale : 1f);
        }

        /// <inheritdoc />
        public bool TakeHit(int damage, Vector2 direction, bool lethal = false)
        {
            if (_health == null)
            {
                return false;
            }

            bool isBoss = _definition != null && _definition.IsBoss;
            if (!_health.TakeDamage(WeaponMath.HitDamage(damage, lethal, isBoss, _health.Current)))
            {
                return false;
            }

            SetFlash(0f);
            if (_health.IsDead)
            {
                Die();
            }
            else
            {
                if (_view != null)
                {
                    _view.PlayHit();
                }

                ApplyHitKnockback(direction);
            }

            Hit?.Invoke(this);
            return true;
        }

        private void ApplyHitKnockback(Vector2 direction)
        {
            if (_feel != null)
            {
                float scale = _definition != null ? _definition.KnockbackScale : 1f;
                Vector2 velocity = FeelMath.KnockbackVelocity(direction, _feel.KnockbackSpeed, scale);
                if (velocity != Vector2.zero)
                {
                    _movement.Knockback(velocity, _feel.KnockbackDuration);
                }
            }
        }

        /// <inheritdoc />
        public bool Push(Vector2 direction, float speed, float duration)
        {
            if (_health == null || _health.IsDead)
            {
                return false;
            }

            float scale = _definition != null ? _definition.KnockbackScale : 1f;
            Vector2 velocity = FeelMath.KnockbackVelocity(direction, speed, scale);
            if (velocity == Vector2.zero || duration <= 0f)
            {
                return false;
            }

            _movement.Knockback(velocity, duration);
            return true;
        }

        /// <summary>Kills the enemy at once, whatever its health, e.g. when a boomer touches the player.</summary>
        /// <returns>True if the enemy was alive.</returns>
        public bool Kill()
        {
            if (_health == null || _health.IsDead)
            {
                return false;
            }

            _health.TakeDamage(_health.Current);
            SetFlash(0f);
            Die();
            Hit?.Invoke(this);
            return true;
        }

        private void Awake()
        {
            _movement = GetComponent<EnemyMovement>();
            _body = GetComponent<Rigidbody2D>();
            _collider = GetComponent<Collider2D>();
        }

        private void Update()
        {
            if (_sinceHit >= 0f)
            {
                _sinceHit += Time.deltaTime;
                if (FeelMath.FlashAmount(_sinceHit, _feel != null ? _feel.HitFlashDuration : 0f) <= 0f)
                {
                    SetFlash(-1f);
                }
            }

            if (_dyingTime < 0f)
            {
                CheckApproach();
                return;
            }

            _dyingTime += Time.deltaTime;
            float fade = _definition != null ? _definition.DeathDuration : 0f;
            float t = EnemyDeathMath.FadeProgress(_dyingTime, _deathAnimationTime, fade);
            if (_deathAnimationTime <= 0f)
            {
                transform.localScale = Vector3.one * (1f - 0.6f * t);
            }

            SetAlpha(1f - t);
            if (_dyingTime >= _deathAnimationTime && t >= 1f)
            {
                Finish();
            }
        }

        // Explosive enemies blow up on touching the player they chase.
        private void OnCollisionEnter2D(Collision2D collision)
        {
            TryDetonate(collision);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            TryDetonate(collision);
        }

        private void TryDetonate(Collision2D collision)
        {
            if (_target != null && _definition != null && _definition.Explosion != null && !IsDead
                && collision.rigidbody != null && collision.rigidbody.transform == _target)
            {
                Kill();
            }
        }

        private void CheckApproach()
        {
            if (_target == null || _approach.HasFired)
            {
                return;
            }

            float sqrDistance = ((Vector2)_target.position - (Vector2)transform.position).sqrMagnitude;
            if (_approach.Check(sqrDistance))
            {
                // The boomer swells as the visual side of its warning sound (issue #101).
                if (_view != null)
                {
                    _view.SetAlert(true);
                }

                Approached?.Invoke(this);
            }
        }

        private void Die()
        {
            _movement.Stop();
            _body.linearVelocity = Vector2.zero;
            _body.simulated = false;
            _dyingTime = 0f;
            _deathAnimationTime = 0f;
            if (_view != null)
            {
                _view.PlayDeath();
                _deathAnimationTime = _view.DeathDuration;
            }

            Died?.Invoke(this);
        }

        private void Finish()
        {
            _dyingTime = -1f;
            Action<Enemy> release = _release;
            _release = null;
            if (release != null)
            {
                release(this);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        // 0 starts the flash, a negative value ends it.
        private void SetFlash(float sinceHit)
        {
            bool wasFlashing = IsFlashing;
            _sinceHit = sinceHit;
            if (_renderer == null || wasFlashing == IsFlashing)
            {
                return;
            }

            _block ??= new MaterialPropertyBlock();
            _renderer.GetPropertyBlock(_block);
            _block.SetFloat(FlashAmountId, IsFlashing ? 1f : 0f);
            _renderer.SetPropertyBlock(_block);
        }

        private void SetAlpha(float alpha)
        {
            if (_renderer != null)
            {
                Color color = _renderer.color;
                color.a = alpha;
                _renderer.color = color;
            }
        }
    }
}
