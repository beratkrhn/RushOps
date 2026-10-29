using System.Collections;
using NUnit.Framework;
using RushOps.Player;
using RushOps.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace RushOps.Tests.PlayMode
{
    public class TouchControlsTests
    {
        private const string SceneName = "Arena_Test";

        private Touchscreen _touchscreen;
        private GameObject _safeArea;
        private VirtualStick _left;
        private VirtualStick _right;
        private PlayerMovement _player;

        [UnitySetUp]
        public IEnumerator LoadScene()
        {
            yield return SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);
            yield return null;

            _player = Object.FindFirstObjectByType<PlayerMovement>();
            Assert.IsNotNull(_player, "Arena_Test has no PlayerMovement");
            var visibility = Object.FindFirstObjectByType<TouchControlsVisibility>();
            Assert.IsNotNull(visibility, "Arena_Test has no TouchControlsVisibility");
            _safeArea = visibility.GetComponentInChildren<SafeAreaFitter>(true).gameObject;
        }

        [TearDown]
        public void RemoveTouchscreen()
        {
            if (_touchscreen != null)
            {
                InputSystem.RemoveDevice(_touchscreen);
                _touchscreen = null;
            }
        }

        [UnityTest]
        public IEnumerator Controls_ShowOnlyWithTouchscreen()
        {
            Assume.That(Touchscreen.current, Is.Null, "This machine has a touchscreen");
            Assert.IsFalse(_safeArea.activeSelf, "hidden without touchscreen");

            yield return AddTouchscreen();

            Assert.IsTrue(_safeArea.activeSelf, "shown with touchscreen");
        }

        [UnityTest]
        public IEnumerator LeftStick_DragRight_MovesPlayerRight()
        {
            yield return AddTouchscreen();
            Vector2 start = StickCenter(_left);

            _left.OnPointerDown(Pointer(0, start));
            _left.OnDrag(Pointer(0, start + new Vector2(Screen.width, 0f)));
            yield return null;
            yield return null;

            Assert.AreEqual(1f, _left.Value.x, 1e-3f);
            Assert.Greater(_player.MoveInput.x, 0.9f, "Player/Move reads the virtual left stick");

            _left.OnPointerUp(Pointer(0, start));
            yield return null;
            yield return null;

            Assert.AreEqual(Vector2.zero, _player.MoveInput, "released stick stops the player");
        }

        [UnityTest]
        public IEnumerator BothSticks_HeldAtOnce_EachFollowsItsOwnTouch()
        {
            yield return AddTouchscreen();
            Vector2 leftStart = StickCenter(_left);
            Vector2 rightStart = StickCenter(_right);

            _left.OnPointerDown(Pointer(0, leftStart));
            _right.OnPointerDown(Pointer(1, rightStart));
            _left.OnDrag(Pointer(0, leftStart + new Vector2(0f, Screen.height)));
            _right.OnDrag(Pointer(1, rightStart + new Vector2(-Screen.width, 0f)));
            // A drag of the other touch must not move a stick.
            _left.OnDrag(Pointer(1, leftStart + new Vector2(-Screen.width, 0f)));
            yield return null;

            Assert.AreEqual(1f, _left.Value.y, 1e-3f);
            Assert.AreEqual(-1f, _right.Value.x, 1e-3f);
            Assert.IsTrue(_left.IsPressed && _right.IsPressed);
        }

        [UnityTest]
        public IEnumerator FloatingStick_AppearsAtTouchPoint()
        {
            yield return AddTouchscreen();
            Assume.That(_right.Config.Mode, Is.EqualTo(StickMode.Floating));
            var zone = (RectTransform)_right.transform;
            Vector2 touch = zone.TransformPoint(zone.rect.center);

            _right.OnPointerDown(Pointer(0, touch));
            yield return null;

            Assert.AreEqual(touch.x, StickCenter(_right).x, 1f);
            Assert.AreEqual(touch.y, StickCenter(_right).y, 1f);
            Assert.AreEqual(0f, _right.Value.magnitude, 1e-3f);
        }

        [UnityTest]
        public IEnumerator Controls_LieInsideSafeArea()
        {
            yield return AddTouchscreen();
            Rect safe = Screen.safeArea;

            foreach (Graphic graphic in _safeArea.GetComponentsInChildren<Graphic>())
            {
                var corners = new Vector3[4];
                graphic.rectTransform.GetWorldCorners(corners);
                foreach (Vector3 corner in corners)
                {
                    Assert.IsTrue(Inside(safe, corner), $"{graphic.name} at {corner} outside {safe}");
                }
            }
        }

        [UnityTest]
        public IEnumerator DashButton_AfterDash_ShowsCooldown()
        {
            yield return AddTouchscreen();
            var dash = _player.GetComponent<PlayerDash>();
            Image overlay = _safeArea.GetComponentInChildren<DashButtonView>().transform.Find("Cooldown").GetComponent<Image>();

            Assert.IsFalse(overlay.enabled, "no cooldown before the first dash");
            Assert.IsTrue(dash.TryDash());
            yield return null;

            Assert.IsTrue(overlay.enabled, "cooldown visible");
            Assert.Greater(overlay.fillAmount, 0.5f);

            yield return new WaitForSeconds(_player.Config.DashCooldown + 0.1f);

            Assert.IsFalse(overlay.enabled, "cooldown gone when the dash is ready");
        }

        private IEnumerator AddTouchscreen()
        {
            _touchscreen = InputSystem.AddDevice<Touchscreen>();
            yield return null;

            foreach (VirtualStick stick in _safeArea.GetComponentsInChildren<VirtualStick>())
            {
                if (stick.name == "LeftStick")
                {
                    _left = stick;
                }
                else if (stick.name == "RightStick")
                {
                    _right = stick;
                }
            }

            Assert.IsNotNull(_left, "no LeftStick");
            Assert.IsNotNull(_right, "no RightStick");
        }

        // Screen position of the stick's base; on an overlay canvas world space is screen space.
        private static Vector2 StickCenter(VirtualStick stick)
        {
            return stick.transform.Find("Base").position;
        }

        private static PointerEventData Pointer(int id, Vector2 position)
        {
            return new PointerEventData(EventSystem.current) { pointerId = id, position = position };
        }

        // Corners exactly on the safe area edge count as inside.
        private static bool Inside(Rect rect, Vector3 point)
        {
            const float Tolerance = 0.5f;
            return point.x >= rect.xMin - Tolerance && point.x <= rect.xMax + Tolerance
                && point.y >= rect.yMin - Tolerance && point.y <= rect.yMax + Tolerance;
        }
    }
}
