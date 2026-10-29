using System;
using RushOps.Combat;
using RushOps.Feel;
using RushOps.Navigation;
using UnityEngine;

namespace RushOps.Enemies
{
    /// <summary>
    /// A pooled enemy (F-GEG-01, F-GEG-04, NF-02): health and contact damage from its <see cref="EnemyDefinition"/>,
    /// takes projectile hits with a white flash and a knockback (issue #12), and on death plays a short
    /// shrink-and-fade effect before it hands itself back to its pool. Movement is done by
    /// <see cref="EnemyMovement"/>; this component switches it on and off. The flash needs a material with
    /// the <c>RushOps/Sprite Flash</c> shader on the renderer.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(EnemyMovement))]
    public class Enemy : MonoBehaviour, IDamageable, IContactDamage
    {
        [SerializeField] private EnemyDefinition _definition;
        [SerializeField] private FeelConfig _feel;
        [SerializeField] private SpriteRenderer _renderer;

        private static readonly int FlashAmountId = Shader.PropertyToID("_FlashAmount");

        private Health _health;
        private EnemyMovement _movement;
        private Rigidbody2D _body;
        private Collider2D _collider;
        private Action<Enemy> _release;
        private float _dyingTime = -1f;
        private float _sinceHit = -1f;
        private MaterialPropertyBlock _block;

        /// <summary>Raised on every hit that counted, including the killing one (then <see cref="IsDead"/> is true).</summary>
        public event Action<Enemy> Hit;

        /// <summary>Raised when the enemy dies, before the death effect.</summary>
        public event Action<Enemy> Died;

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
            SetAlpha(1f);
            _movement.Begin(navigator, _definition != null ? _definition.MoveSpeed : 0f);
        }

        /// <inheritdoc />
        public bool TakeHit(int damage, Vector2 direction)
        {
            if (_health == null || !_health.TakeDamage(damage))
            {
                return false;
            }

            SetFlash(0f);
            if (_health.IsDead)
            {
                Die();
            }
            else if (_feel != null)
            {
                _movement.Knockback(direction.normalized * _feel.KnockbackSpeed, _feel.KnockbackDuration);
            }

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
                return;
            }

            _dyingTime += Time.deltaTime;
            float duration = _definition != null ? _definition.DeathDuration : 0f;
            float t = duration > 0f ? Mathf.Clamp01(_dyingTime / duration) : 1f;
            transform.localScale = Vector3.one * (1f - 0.6f * t);
            SetAlpha(1f - t);
            if (t >= 1f)
            {
                Finish();
            }
        }

        private void Die()
        {
            _movement.Stop();
            _body.linearVelocity = Vector2.zero;
            _body.simulated = false;
            _dyingTime = 0f;
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
