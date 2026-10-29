using RushOps.Combat;
using UnityEngine;

namespace RushOps.Enemies
{
    /// <summary>Hurts the player on touch with the contact damage of its <see cref="EnemyDefinition"/>.</summary>
    public class EnemyContactDamage : MonoBehaviour, IContactDamage
    {
        [SerializeField] private EnemyDefinition _definition;

        /// <inheritdoc />
        public int ContactDamage => _definition != null ? _definition.ContactDamage : 0;

        /// <summary>Enemy type this component takes its damage from.</summary>
        public EnemyDefinition Definition
        {
            get => _definition;
            set => _definition = value;
        }
    }
}
