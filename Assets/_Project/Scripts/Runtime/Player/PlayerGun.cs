using UnityEngine;

namespace RushOps.Player
{
    /// <summary>
    /// Shows the gun in the player's hand: turns it towards <see cref="PlayerAim.FacingDirection"/>, keeps it upright,
    /// draws it behind the body when pointing away from the camera and hides it while dashing or dead. Placement comes
    /// from <see cref="PlayerWeapon"/>, so the shots leave the visible muzzle.
    /// </summary>
    public class PlayerGun : MonoBehaviour
    {
        [SerializeField] private PlayerAim _aim;
        [SerializeField] private PlayerWeapon _weapon;
        [SerializeField] private PlayerDash _dash;
        [SerializeField] private PlayerHealth _health;
        [SerializeField] private SpriteRenderer _renderer;

        [Tooltip("Sorting order inside the player's sorting group in front of the body.")]
        [SerializeField] private int _frontOrder = 1;

        [Tooltip("Sorting order inside the player's sorting group behind the body.")]
        [SerializeField] private int _behindOrder = -1;

        /// <summary>The gun's renderer.</summary>
        public SpriteRenderer Renderer => _renderer;

        private void LateUpdate()
        {
            if (_aim == null || _weapon == null || _renderer == null)
            {
                return;
            }

            bool hidden = (_dash != null && _dash.IsDashing) || (_health != null && _health.IsDead);
            _renderer.enabled = !hidden;
            if (hidden)
            {
                return;
            }

            GunPose pose = _weapon.GunPoseFor(_aim.FacingDirection);
            transform.localPosition = pose.Grip;
            transform.localRotation = Quaternion.Euler(0f, 0f, pose.Angle);
            _renderer.flipY = pose.FlipY;
            _renderer.sortingOrder = pose.BehindBody ? _behindOrder : _frontOrder;
        }
    }
}
