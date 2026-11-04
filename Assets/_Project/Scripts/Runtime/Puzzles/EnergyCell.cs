using UnityEngine;

namespace RushOps.Puzzles
{
    /// <summary>
    /// An energy cell for the object transport (F-RAE-06, issue #33): lies on the floor until the player touches it,
    /// then rides along at the player's hold point until it is handed in at a <see cref="Generator"/>.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class EnergyCell : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _renderer;
        [Tooltip("Sorting order while carried, so the cell draws in front of the figure.")]
        [SerializeField] private int _carriedSortingOrder = 3;

        private Collider2D _trigger;

        /// <summary>True while the player carries the cell.</summary>
        public bool IsCarried { get; private set; }

        /// <summary>True once the cell was handed in.</summary>
        public bool IsHandedIn { get; private set; }

        /// <summary>Puts the cell into the hand at <paramref name="holdPoint"/>.</summary>
        public void AttachTo(Transform holdPoint)
        {
            IsCarried = true;
            _trigger.enabled = false;
            transform.SetParent(holdPoint, false);
            transform.localPosition = Vector3.zero;
            if (_renderer != null)
            {
                _renderer.sortingOrder = _carriedSortingOrder;
            }
        }

        /// <summary>Removes the cell once handed in; the generator shows it from then on.</summary>
        public void Consume()
        {
            IsCarried = false;
            IsHandedIn = true;
            gameObject.SetActive(false);
        }

        private void Awake()
        {
            _trigger = GetComponent<Collider2D>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (IsCarried || IsHandedIn)
            {
                return;
            }

            Rigidbody2D body = other.attachedRigidbody;
            if (body != null && body.TryGetComponent(out PlayerCarrier carrier))
            {
                carrier.TryPickUp(this);
            }
        }
    }
}
