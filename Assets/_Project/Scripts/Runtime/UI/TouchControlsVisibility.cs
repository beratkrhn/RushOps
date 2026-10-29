using UnityEngine;
using UnityEngine.InputSystem;

namespace RushOps.UI
{
    /// <summary>
    /// Shows the touch controls only while a touchscreen is present (phone, Device Simulator), so mouse and
    /// keyboard keep working unchanged in the editor's Game view (F-STE-07).
    /// </summary>
    public class TouchControlsVisibility : MonoBehaviour
    {
        [Tooltip("Root of the touch controls; activated only while a touchscreen is present.")]
        [SerializeField] private GameObject _controls;
        [Tooltip("Show the controls even without a touchscreen, e.g. to check the layout with the mouse.")]
        [SerializeField] private bool _alwaysShow;

        private void OnEnable()
        {
            InputSystem.onDeviceChange += OnDeviceChange;
            Apply();
        }

        private void OnDisable()
        {
            InputSystem.onDeviceChange -= OnDeviceChange;
        }

        private void OnDeviceChange(InputDevice device, InputDeviceChange change)
        {
            if (device is Touchscreen)
            {
                Apply();
            }
        }

        private void Apply()
        {
            if (_controls != null)
            {
                _controls.SetActive(_alwaysShow || Touchscreen.current != null);
            }
        }
    }
}
