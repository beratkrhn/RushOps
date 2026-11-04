using System;
using RushOps.Characters;
using RushOps.Combat;
using RushOps.Enemies;
using RushOps.Feel;
using UnityEngine;

namespace RushOps.Bosses
{
    /// <summary>
    /// One body of a boss (F-BOS-01, F-BOS-02): health and contact damage from its <see cref="BossDefinition"/>, a
    /// white flash on hits, no knockback. Insta-kill hits deal only normal damage and the nuke does not reach it, since
    /// bosses are in no enemy pool; double damage works as for any target. Once <see cref="Activate"/>d it walks
    /// straight at its target (decision L5, arenas have no obstacles). Bosses with own moves override
    /// <see cref="TickBehaviour"/>. The <see cref="BossFight"/> runs health bar, perk drops and victory.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class Boss : MonoBehaviour, IDamageable, IContactDamage
    {
        [SerializeField] private BossDefinition _definition;
        [SerializeField] private FeelConfig _feel;
        [SerializeField] private SpriteRenderer _renderer;
        [Tooltip("Plays walk, hit and death animations; optional.")]
        [SerializeField] private DirectionalSpriteAnimator _view;
        [Tooltip("Stops this close to the target's feet instead of pushing into it.")]
        [SerializeField, Min(0f)] private float _stopDistance = 0.5f;

        private static readonly int FlashAmountId = Shader.PropertyToID("_FlashAmount");

        private Health _health;
        private Rigidbody2D _body;
        private Collider2D _collider;
        private Transform _target;
        private MaterialPropertyBlock _block;
        private float _sinceHit = -1f;
        private float _dyingTime = -1f;
        private float _deathAnimationTime;
        private int _maxHealth;

        /// <summary>Raised on every hit that counted, including the killing one.</summary>
        public event Action<Boss> Hit;

        /// <summary>Raised once when the boss dies, before its death animation.</summary>
        public event Action<Boss> Died;

        /// <summary>Balancing values.</summary>
        public BossDefinition Definition => _definition;

        /// <summary>Current hit points.</summary>
        public int CurrentHealth => Health.Current;

        /// <summary>Hit points at full health; for a body split off a boss its share of the health.</summary>
        public int MaxHealth => Health.Max;

        /// <summary>True from the killing hit on.</summary>
        public bool IsDead => Health.IsDead;

        /// <summary>True between <see cref="Activate"/> and death.</summary>
        public bool IsActive { get; private set; }

        /// <summary>True while the white hit flash shows.</summary>
        public bool IsFlashing => _sinceHit >= 0f;

        /// <summary>The chased target, set by <see cref="Activate"/>.</summary>
        public Transform Target => _target;

        /// <summary>Center of the body, where shots should go; the transform sits at the feet.</summary>
        public Vector2 AimPoint => _collider != null ? (Vector2)_collider.bounds.center : (Vector2)transform.position;

        /// <summary>Physics body; behaviours set its velocity.</summary>
        protected Rigidbody2D Body => _body;

        /// <summary>The animator, or null.</summary>
        protected DirectionalSpriteAnimator View => _view;

        /// <inheritdoc />
        public virtual int ContactDamage => _definition != null && !IsDead && IsActive ? _definition.ContactDamage : 0;

        private Health Health => _health ??= new Health(_maxHealth > 0 ? _maxHealth : _definition != null ? _definition.Health : 1);

        /// <summary>Gives the body <paramref name="health"/> hit points instead of the definition's, e.g. after a split.</summary>
        public void SetHealth(int health)
        {
            _maxHealth = Mathf.Max(1, health);
            _health = new Health(_maxHealth);
        }

        /// <summary>Starts the fight: from now on the boss chases <paramref name="target"/>.</summary>
        public void Activate(Transform target)
        {
            if (IsActive || IsDead)
            {
                return;
            }

            _target = target;
            IsActive = true;
            OnActivated();
        }

        /// <inheritdoc />
        public bool TakeHit(int damage, Vector2 direction, bool lethal = false)
        {
            if (!IsActive || IsDead)
            {
                return false;
            }

            if (!Health.TakeDamage(WeaponMath.HitDamage(damage, lethal, true, Health.Current)))
            {
                return false;
            }

            SetFlash(0f);
            if (Health.IsDead)
            {
                Die();
            }
            else if (_view != null)
            {
                _view.PlayHit();
            }

            Hit?.Invoke(this);
            return true;
        }

        /// <summary>Called once when the fight starts.</summary>
        protected virtual void OnActivated()
        {
            if (_view != null)
            {
                _view.PlayLoop();
            }
        }

        /// <summary>
        /// Moves the boss each physics step while active and alive. The default walks straight at the target at the
        /// definition's speed.
        /// </summary>
        protected virtual void TickBehaviour(float deltaTime)
        {
            Chase(_definition != null ? _definition.MoveSpeed : 0f);
        }

        /// <summary>Walks straight at the target at <paramref name="speed"/>, facing the way it goes.</summary>
        protected void Chase(float speed)
        {
            Vector2 direction = _target != null
                ? BossMath.ChaseDirection(_body.position, _target.position, _stopDistance)
                : Vector2.zero;
            _body.linearVelocity = direction * speed;
            if (_view != null && direction != Vector2.zero)
            {
                _view.SetFacing(direction);
            }
        }

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            _collider = GetComponent<Collider2D>();
            if (_definition != null)
            {
                _body.mass = _definition.Mass;
            }
        }

        private void FixedUpdate()
        {
            if (IsActive && !IsDead)
            {
                TickBehaviour(Time.fixedDeltaTime);
            }
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
            float t = EnemyDeathMath.FadeProgress(
                _dyingTime, _deathAnimationTime, _definition != null ? _definition.DeathDuration : 0f);
            SetAlpha(1f - t);
            if (_dyingTime >= _deathAnimationTime && t >= 1f)
            {
                _dyingTime = -1f;
                gameObject.SetActive(false);
            }
        }

        private void Die()
        {
            IsActive = false;
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
