using UnityEngine;
using UnityEngine.InputSystem;

namespace RushOps.Rush
{
    /// <summary>
    /// Editor and debug control of the rush (F-STE-07, issue #270): the key fills the meter and triggers the rush at
    /// once, so it can be tried without making kills. <see cref="FillAndTrigger"/> also serves debug buttons.
    /// </summary>
    public class DebugRushKey : MonoBehaviour
    {
        [SerializeField] private PlayerRush _rush;
        [SerializeField] private Key _key = Key.R;

        /// <summary>Fills the meter and starts the rush; does nothing while a rush runs.</summary>
        public void FillAndTrigger()
        {
            if (_rush == null || _rush.IsActive)
            {
                return;
            }

            _rush.FillUp();
            _rush.TryTrigger();
        }

        private void Update()
        {
            Keyboard keyboard = Application.isEditor ? Keyboard.current : null;
            if (keyboard != null && keyboard[_key].wasPressedThisFrame)
            {
                FillAndTrigger();
            }
        }
    }
}
