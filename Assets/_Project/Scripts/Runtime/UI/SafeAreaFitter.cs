using UnityEngine;

namespace RushOps.UI
{
    /// <summary>
    /// Fits its <see cref="RectTransform"/> to <see cref="Screen.safeArea"/> (F-PLT-05), so its children avoid
    /// notch, camera cutout and home indicator. Follows changes of the safe area, e.g. in the Device Simulator.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaFitter : MonoBehaviour
    {
        private RectTransform _rect;
        private Rect _appliedArea;
        private Vector2Int _appliedScreen;

        private void Awake()
        {
            _rect = (RectTransform)transform;
        }

        private void OnEnable()
        {
            Apply();
        }

        private void Update()
        {
            if (Screen.safeArea != _appliedArea || Screen.width != _appliedScreen.x || Screen.height != _appliedScreen.y)
            {
                Apply();
            }
        }

        private void Apply()
        {
            if (_rect == null)
            {
                _rect = (RectTransform)transform;
            }

            _appliedArea = Screen.safeArea;
            _appliedScreen = new Vector2Int(Screen.width, Screen.height);
            SafeAreaMath.Anchors(_appliedArea, _appliedScreen, out Vector2 min, out Vector2 max);
            _rect.anchorMin = min;
            _rect.anchorMax = max;
            _rect.offsetMin = Vector2.zero;
            _rect.offsetMax = Vector2.zero;
        }
    }
}
