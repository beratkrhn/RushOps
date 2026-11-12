using RushOps.UI;
using UnityEngine;
using UnityEngine.UI;

namespace RushOps.Arcade
{
    /// <summary>
    /// Tilts a drawn joystick of the cabinet with the thumb (F-ARC-04): reads the value of its
    /// <see cref="VirtualStick"/>, which feeds the same gamepad stick as in the normal mode, and shows the sprite of
    /// that direction. On release it swings briefly the other way before it rests again.
    /// </summary>
    public class ArcadeStickView : MonoBehaviour
    {
        [SerializeField] private ArcadeConfig _config;
        [SerializeField] private VirtualStick _stick;
        [SerializeField] private Image _image;

        private int _shownSprite = -1;
        private int _lastStep;
        private int _springStep;
        private float _springLeft;

        /// <summary>Index of the shown stick sprite: 0 at rest, 1 to 8 for E to SE.</summary>
        public int ShownSprite => _shownSprite;

        /// <summary>The stick whose value the view shows.</summary>
        public VirtualStick Stick => _stick;

        private void OnEnable()
        {
            _lastStep = 0;
            _springLeft = 0f;
            Show(0);
        }

        private void LateUpdate()
        {
            if (_config == null || _stick == null)
            {
                return;
            }

            int steps = _config.TiltSteps;
            int step = ArcadeStickMath.Step(_stick.Value, _config.TiltThreshold, steps);
            if (step == 0 && _lastStep != 0)
            {
                _springStep = ArcadeStickMath.Opposite(_lastStep, steps);
                _springLeft = _config.SpringBackTime;
            }
            else if (step != 0)
            {
                _springLeft = 0f;
            }

            _lastStep = step;
            int shown = step;
            if (step == 0 && _springLeft > 0f)
            {
                _springLeft -= Time.unscaledDeltaTime;
                shown = _springStep;
            }

            Show(ArcadeStickMath.SpriteIndex(shown, steps));
        }

        private void Show(int index)
        {
            if (index == _shownSprite || _image == null || _config == null || index >= _config.StickSprites.Length)
            {
                return;
            }

            _shownSprite = index;
            _image.sprite = _config.StickSprites[index];
        }
    }
}
