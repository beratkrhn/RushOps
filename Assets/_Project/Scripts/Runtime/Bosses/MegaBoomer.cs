using System;
using UnityEngine;

namespace RushOps.Bosses
{
    /// <summary>
    /// Boss of level 3-3 (issue #159, Lastenheft 2.4): a giant boomer that walks at the player and leaves puddles that
    /// glow up and then hurt on contact (C9b). At half its health it stands still, tears apart and two halves take
    /// over, each with half of its remaining health; they share the health bar and both must fall (C9a). A half is a
    /// <see cref="MegaBoomer"/> without halves of its own: smaller, faster, and it leaves puddles too. Rules in
    /// <see cref="MegaBoomerLogic"/>, values in <see cref="MegaBoomerDefinition"/>.
    /// </summary>
    public class MegaBoomer : Boss
    {
        [Tooltip("Pool the puddles come from.")]
        [SerializeField] private PuddlePool _puddles;
        [Tooltip("The fight the halves join.")]
        [SerializeField] private BossFight _fight;
        [Tooltip("The two halves, inactive until the split; empty on a half.")]
        [SerializeField] private MegaBoomer[] _halves = Array.Empty<MegaBoomer>();
        [Tooltip("Layers a half must not appear in, usually the walls.")]
        [SerializeField] private LayerMask _wallLayers;
        [Tooltip("Radius kept clear of walls where a half appears.")]
        [SerializeField, Min(0f)] private float _halfClearance = 0.5f;

        private static readonly int[] HalfHealth = new int[2];

        private MegaBoomerLogic _logic;

        /// <summary>Raised when it starts to tear apart.</summary>
        public event Action<MegaBoomer> SplitStarted;

        /// <summary>Raised when the halves took over; this body is gone afterwards.</summary>
        public event Action<MegaBoomer> SplitDone;

        /// <summary>Current phase.</summary>
        public MegaBoomerPhase Phase => Logic.Phase;

        /// <summary>True for a half, which does not split again.</summary>
        public bool IsHalf => _halves == null || _halves.Length == 0;

        /// <summary>The halves that take over after the split.</summary>
        public MegaBoomer[] Halves => _halves;

        /// <summary>Puddles this body left in the fight.</summary>
        public int PuddleCount { get; private set; }

        private MegaBoomerDefinition Values => Definition as MegaBoomerDefinition;

        private MegaBoomerLogic Logic => _logic ??= Values != null
            ? new MegaBoomerLogic(Values.PuddleInterval, Values.SplitHealthShare, Values.SplitTime, !IsHalf)
            : new MegaBoomerLogic(2.5f, 0.5f, 0.7f, !IsHalf);

        private float HealthShare => MaxHealth > 0 ? CurrentHealth / (float)MaxHealth : 0f;

        private float Speed => (Definition != null ? Definition.MoveSpeed : 0f)
            * (IsHalf && Values != null ? Values.HalfSpeedFactor : 1f);

        /// <inheritdoc />
        protected override void TickBehaviour(float deltaTime)
        {
            switch (Logic.Tick(deltaTime, HealthShare))
            {
                case MegaBoomerEvent.Puddle:
                    LeavePuddle();
                    break;
                case MegaBoomerEvent.SplitStarted:
                    if (View != null)
                    {
                        View.SetAlert(true);
                    }

                    SplitStarted?.Invoke(this);
                    break;
                case MegaBoomerEvent.Split:
                    SplitApart();
                    return;
            }

            if (Logic.Phase == MegaBoomerPhase.Walk)
            {
                Chase(Speed);
            }
            else
            {
                Body.linearVelocity = Vector2.zero;
            }
        }

        private void LeavePuddle()
        {
            if (_puddles != null && _puddles.Spawn(Body.position, Values) != null)
            {
                PuddleCount++;
            }
        }

        private void SplitApart()
        {
            (HalfHealth[0], HalfHealth[1]) = MegaBoomerLogic.SplitHealth(CurrentHealth);
            Vector2 feet = Body.position;
            Vector2 offset = Vector2.right * (Values != null ? Values.HalfOffset : 1f);
            float scale = Values != null ? Values.HalfScale : 0.65f;
            for (int i = 0; i < _halves.Length && i < HalfHealth.Length; i++)
            {
                MegaBoomer half = _halves[i];
                if (half == null)
                {
                    continue;
                }

                Vector2 spot = feet + (i == 0 ? -offset : offset);
                if (Physics2D.OverlapCircle(spot, _halfClearance, _wallLayers) != null)
                {
                    spot = feet;
                }

                half.transform.position = spot;
                half.transform.localScale = new Vector3(scale, scale, 1f);
                half.gameObject.SetActive(true);
                half.SetHealth(HalfHealth[i]);
                if (_fight != null)
                {
                    _fight.AddBody(half);
                }
                else
                {
                    half.Activate(Target);
                }
            }

            SplitDone?.Invoke(this);
            Remove();
        }
    }
}
