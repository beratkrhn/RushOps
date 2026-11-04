using RushOps.Characters;
using UnityEngine;
using UnityEngine.UI;

namespace RushOps.UI
{
    /// <summary>
    /// A character shown in a menu (issue #125): plays one direction of a <see cref="DirectionalAnimation"/> on a UI
    /// <see cref="Image"/>, in unscaled time. The skin selection swaps the animation through <see cref="Show"/>.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class MenuFigure : MonoBehaviour
    {
        [SerializeField] private DirectionalAnimation _animation;
        [SerializeField] private Direction8 _direction = Direction8.S;

        private Image _image;
        private float _time;
        private int _frame = -1;

        /// <summary>The animation currently shown.</summary>
        public DirectionalAnimation Animation => _animation;

        /// <summary>Shows <paramref name="animation"/> from its first frame.</summary>
        public void Show(DirectionalAnimation animation)
        {
            _animation = animation;
            _time = 0f;
            _frame = -1;
            Apply();
        }

        private void Awake()
        {
            _image = GetComponent<Image>();
            Apply();
        }

        private void Update()
        {
            _time += Time.unscaledDeltaTime;
            Apply();
        }

        private void Apply()
        {
            if (_animation == null || _image == null || _animation.FrameCount(_direction) == 0)
            {
                return;
            }

            int frame = _animation.FrameIndexAt(_time, _direction);
            if (frame != _frame)
            {
                _frame = frame;
                _image.sprite = _animation.GetFrame(_direction, frame);
            }
        }
    }
}
