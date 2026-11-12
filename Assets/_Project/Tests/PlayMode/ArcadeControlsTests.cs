using System.Collections;
using NUnit.Framework;
using RushOps.Arcade;
using RushOps.Player;
using RushOps.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;

namespace RushOps.Tests.PlayMode
{
    /// <summary>The control panel of the cabinet (issue #322, F-ARC-04): joysticks move and aim, the button dashes.</summary>
    public class ArcadeControlsTests
    {
        private const int MovePointer = 11;
        private const int DashPointer = 12;

        [UnitySetUp]
        public IEnumerator LoadScene()
        {
            yield return ArcadeRunTests.Load(true);
        }

        [TearDown]
        public void TearDown()
        {
            RushOps.Endless.EndlessStart.Arcade = false;
            SaveGameIsolation.Reset();
        }

        [UnityTest]
        public IEnumerator NormalTouchControls_AreOffInTheArcade()
        {
            var visibility = Object.FindAnyObjectByType<TouchControlsVisibility>(FindObjectsInactive.Include);
            Assert.IsFalse(visibility.gameObject.activeInHierarchy);
            yield break;
        }

        [UnityTest]
        public IEnumerator LeftJoystick_DragRight_MovesThePlayerRightAndTilts()
        {
            ArcadeStickView left = StickView("StickLeft");
            var player = Object.FindAnyObjectByType<PlayerMovement>();

            GameObject zone = left.Stick.gameObject;
            Vector2 press = Center(zone.transform as RectTransform);
            PointerEventData pointer = Pointer(MovePointer, press);
            ExecuteEvents.Execute(zone, pointer, ExecuteEvents.pointerDownHandler);
            pointer.position = press + new Vector2(200f, 0f);
            ExecuteEvents.Execute(zone, pointer, ExecuteEvents.dragHandler);

            yield return null;
            yield return null;

            Assert.Greater(left.Stick.Value.x, 0.9f);
            Assert.AreEqual(1, left.ShownSprite, "tilted east");
            Assert.Greater(player.MoveInput.x, 0.9f, "Player/Move reads the left joystick");

            ExecuteEvents.Execute(zone, pointer, ExecuteEvents.pointerUpHandler);
            yield return null;
            Assert.AreEqual(5, left.ShownSprite, "swings back to the west first");
            Assert.AreEqual(Vector2.zero, player.MoveInput, "released joystick stops the player");
            yield return new WaitForSecondsRealtime(0.2f);
            Assert.AreEqual(0, left.ShownSprite, "rests again");
        }

        [UnityTest]
        public IEnumerator RightJoystick_IsTheAimStick()
        {
            ArcadeStickView right = StickView("StickRight");
            GameObject zone = right.Stick.gameObject;
            Vector2 press = Center(zone.transform as RectTransform);
            PointerEventData pointer = Pointer(MovePointer, press);
            ExecuteEvents.Execute(zone, pointer, ExecuteEvents.pointerDownHandler);
            pointer.position = press + new Vector2(0f, 200f);
            ExecuteEvents.Execute(zone, pointer, ExecuteEvents.dragHandler);
            yield return null;
            yield return null;

            Assert.Greater(right.Stick.Value.y, 0.9f);
            Assert.AreEqual(3, right.ShownSprite, "tilted north");
            ExecuteEvents.Execute(zone, pointer, ExecuteEvents.pointerUpHandler);
        }

        [UnityTest]
        public IEnumerator DashButton_Dashes_PressesIn_AndDimsDuringTheCooldown()
        {
            var view = Object.FindAnyObjectByType<ArcadeDashButtonView>();
            var dash = Object.FindAnyObjectByType<PlayerDash>();
            int dashes = 0;
            dash.Dashed += _ => dashes++;
            yield return null;
            Assert.IsTrue(view.ShowsReady);

            PointerEventData pointer = Pointer(DashPointer, Center((RectTransform)view.transform));
            ExecuteEvents.Execute(view.gameObject, pointer, ExecuteEvents.pointerDownHandler);
            for (int i = 0; i < 5 && dashes == 0; i++)
            {
                yield return null;
            }

            Assert.AreEqual(1, dashes, "the button triggers a dash");
            Assert.IsTrue(view.IsHeld);
            var image = view.GetComponent<UnityEngine.UI.Image>();
            Assert.AreSame(Object.FindAnyObjectByType<ArcadeCabinet>().Config.DashPressedSprite, image.sprite);

            ExecuteEvents.Execute(view.gameObject, pointer, ExecuteEvents.pointerUpHandler);
            yield return null;
            Assert.IsFalse(view.IsHeld);
            Assert.AreSame(Object.FindAnyObjectByType<ArcadeCabinet>().Config.DashSprite, image.sprite);
            Assert.IsFalse(view.ShowsReady, "dimmed during the cooldown");
        }

        private static ArcadeStickView StickView(string name)
        {
            foreach (ArcadeStickView view in Object.FindObjectsByType<ArcadeStickView>(FindObjectsSortMode.None))
            {
                if (view.name == name)
                {
                    return view;
                }
            }

            Assert.Fail("no stick view " + name);
            return null;
        }

        private static Vector2 Center(RectTransform rect)
        {
            var corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            return (corners[0] + corners[2]) * 0.5f;
        }

        private static PointerEventData Pointer(int id, Vector2 position)
        {
            return new PointerEventData(EventSystem.current) { pointerId = id, position = position };
        }
    }
}
