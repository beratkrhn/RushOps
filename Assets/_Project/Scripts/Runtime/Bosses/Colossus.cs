using RushOps.Characters;
using UnityEngine;

namespace RushOps.Bosses
{
    /// <summary>
    /// Boss of level 1-3 (issue #151, Lastenheft 2.4): a giant brute that chases the player, charges up for a second
    /// with a red line on the floor in its rush direction (L7) and then rushes straight ahead until it hits a wall.
    /// There it stands dazed (C7), the window to shoot. The rush hurts like any contact (1 heart). Rules in
    /// <see cref="ColossusLogic"/>, values in <see cref="ColossusDefinition"/>.
    /// </summary>
    public class Colossus : Boss
    {
        [Tooltip("Floor line shown during the charge up; a sprite one unit long, stretched to the line length.")]
        [SerializeField] private SpriteRenderer _line;
        [Tooltip("Layers that stop the rush, usually the walls.")]
        [SerializeField] private LayerMask _wallLayers;
        [SerializeField] private DirectionalAnimation _walk;
        [SerializeField] private DirectionalAnimation _stun;
        [Tooltip("A rush slower than this share of the rush speed counts as blocked by a wall.")]
        [SerializeField, Range(0f, 1f)] private float _blockedSpeedShare = 0.2f;

        private ColossusLogic _logic;
        private float _blockedTime;
        private Vector2 _lastPosition;

        /// <summary>Current phase.</summary>
        public ColossusPhase Phase => Logic.Phase;

        /// <summary>Locked direction of the coming or running rush.</summary>
        public Vector2 ChargeDirection => Logic.ChargeDirection;

        /// <summary>True while the floor line shows.</summary>
        public bool ShowsLine => _line != null && _line.enabled;

        private ColossusDefinition Values => Definition as ColossusDefinition;

        private ColossusLogic Logic => _logic ??= Values != null
            ? new ColossusLogic(Values.ChaseTime, Values.WindupTime, Values.MaxChargeTime, Values.StunTime)
            : new ColossusLogic(2.5f, 1f, 2.5f, 1.5f);

        /// <inheritdoc />
        protected override void OnActivated()
        {
            base.OnActivated();
            ShowLine(false);
        }

        /// <inheritdoc />
        protected override void TickBehaviour(float deltaTime)
        {
            Vector2 toTarget = Target != null ? (Vector2)Target.position - Body.position : Vector2.zero;
            if (Logic.Tick(deltaTime, toTarget))
            {
                OnPhaseChanged();
            }

            switch (Logic.Phase)
            {
                case ColossusPhase.Chase:
                    Chase(Definition != null ? Definition.MoveSpeed : 0f);
                    break;
                case ColossusPhase.Charge:
                    float speed = (Values != null ? Values.ChargeSpeed : 0f) * TimeFactor;
                    Body.linearVelocity = Logic.ChargeDirection * speed;
                    CheckBlocked(deltaTime, speed);
                    break;
                default:
                    Body.linearVelocity = Vector2.zero;
                    break;
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            CheckWall(collision);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            CheckWall(collision);
        }

        private void CheckWall(Collision2D collision)
        {
            if (Logic.Phase == ColossusPhase.Charge && (_wallLayers.value & (1 << collision.gameObject.layer)) != 0
                && Logic.HitWall())
            {
                OnPhaseChanged();
            }
        }

        // A rush pressed against a wall corner may never report a fresh collision; slowing down counts too.
        private void CheckBlocked(float deltaTime, float speed)
        {
            bool blocked = Logic.PhaseTime > 0.1f && Body.linearVelocity.magnitude < speed * _blockedSpeedShare
                && Body.position == _lastPosition;
            _blockedTime = blocked ? _blockedTime + deltaTime : 0f;
            _lastPosition = Body.position;
            if (_blockedTime > 0.15f && Logic.HitWall())
            {
                _blockedTime = 0f;
                OnPhaseChanged();
            }
        }

        private void OnPhaseChanged()
        {
            ColossusPhase phase = Logic.Phase;
            if (phase == ColossusPhase.Windup)
            {
                Body.linearVelocity = Vector2.zero;
                if (View != null)
                {
                    View.SetFacing(Logic.ChargeDirection);
                }
            }

            ShowLine(phase == ColossusPhase.Windup);
            if (View == null)
            {
                return;
            }

            View.SetAlert(phase == ColossusPhase.Windup);
            View.SetMoving(phase == ColossusPhase.Charge);
            View.SetAnimations(phase == ColossusPhase.Stunned && _stun != null ? _stun : _walk, null, null, null);
        }

        private void ShowLine(bool visible)
        {
            if (_line == null)
            {
                return;
            }

            _line.enabled = visible;
            if (!visible)
            {
                return;
            }

            Vector2 direction = Logic.ChargeDirection;
            float length = Values != null ? Values.LineLength : 10f;
            RaycastHit2D hit = Physics2D.Raycast(Body.position, direction, length, _wallLayers);
            if (hit.collider != null)
            {
                length = hit.distance;
            }

            Transform line = _line.transform;
            line.position = Body.position + direction * (length * 0.5f);
            line.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
            // The boss is scaled up; the line keeps world size.
            Vector3 parentScale = transform.lossyScale;
            line.localScale = new Vector3(length / parentScale.x, 1f / parentScale.y, 1f);
        }
    }
}
