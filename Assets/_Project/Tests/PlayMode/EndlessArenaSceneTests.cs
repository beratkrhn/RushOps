using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using RushOps.Cameras;
using RushOps.Enemies;
using RushOps.Levels;
using RushOps.Navigation;
using RushOps.Player;
using RushOps.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.Tilemaps;

namespace RushOps.Tests.PlayMode
{
    /// <summary>The endless arena (issue #206): round colosseum with fountain, four gates and weapon slots.</summary>
    public class EndlessArenaSceneTests
    {
        private const string SceneName = "Endless_Arena";
        private static readonly Vector2 Center = new Vector2(16f, 16f);
        private const float FloorRadius = 12.5f;
        private const float FountainRadius = 1.9f;

        private PlayerMovement _player;

        [UnitySetUp]
        public IEnumerator LoadScene()
        {
            SaveGameIsolation.Reset();
            yield return SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);
            yield return null;
            _player = Object.FindAnyObjectByType<PlayerMovement>();
            Assert.IsNotNull(_player, "no player in the arena");
            _player.GetComponent<PlayerMoveInput>().enabled = false;
        }

        [UnityTest]
        public IEnumerator Scene_HasArenaPiecesAndTheCampaignHud()
        {
            yield return null;

            Assert.IsNotNull(Object.FindAnyObjectByType<CameraFollow>(), "camera");
            Assert.IsNotNull(Object.FindAnyObjectByType<PauseController>(), "pause");
            Assert.IsNotNull(Object.FindAnyObjectByType<MissionTracker>(), "HUD");
            Assert.IsNotNull(Object.FindAnyObjectByType<TouchControlsVisibility>(), "touch controls");
            Assert.IsNotEmpty(Object.FindObjectsByType<SafeAreaFitter>(FindObjectsSortMode.None), "safe area");
            Assert.IsNull(Object.FindAnyObjectByType<LevelExit>(), "the arena has no exit");
            Assert.IsNull(Object.FindAnyObjectByType<Gate>(), "the arena gates stay closed and are no level gates");

            Zone[] zones = Object.FindObjectsByType<Zone>(FindObjectsSortMode.None);
            Assert.AreEqual(1, zones.Length);
            Assert.IsNull(zones[0].Waves, "the rounds feed the zone (issue #207)");
            foreach (string gate in new[] { "N", "E", "S", "W" })
            {
                Assert.IsNotNull(GameObject.Find($"Gate_{gate}"), $"gate {gate}");
                int spawns = 0;
                foreach (SpawnPoint point in zones[0].SpawnPoints)
                {
                    spawns += point.name.StartsWith($"SpawnPoint_{gate}_") ? 1 : 0;
                }

                Assert.That(spawns, Is.InRange(2, 3), $"spawn points at gate {gate}");
            }

            Assert.AreEqual(3, GameObject.Find("WeaponSlots").transform.childCount);
            Assert.Less(_player.transform.position.y, Center.y - 2f, "the player starts in the south");
        }

        [UnityTest]
        public IEnumerator Player_CanMove()
        {
            Vector2 start = _player.transform.position;
            _player.MoveInput = Vector2.left;
            yield return new WaitForSeconds(0.4f);
            _player.MoveInput = Vector2.zero;

            Assert.Less(_player.transform.position.x, start.x - 0.5f);
        }

        [UnityTest]
        public IEnumerator Fountain_BlocksPlayerBulletsAndTheFlowField()
        {
            Teleport(Center + new Vector2(0f, -3.5f));
            _player.MoveInput = Vector2.up;
            yield return new WaitForSeconds(1.2f);
            _player.MoveInput = Vector2.zero;

            var body = _player.GetComponent<Rigidbody2D>();
            Collider2D own = _player.GetComponent<Collider2D>();
            float distance = Vector2.Distance(own.bounds.center, Center);
            Assert.Greater(distance, FountainRadius, "the player walked into the fountain");
            Assert.Less(body.position.y, Center.y, "the player passed through the fountain");

            int walls = LayerMask.GetMask("Walls");
            Collider2D hit = Physics2D.OverlapPoint(Center + new Vector2(1.5f, 0f), walls);
            Assert.IsNotNull(hit, "bullets would fly through the fountain");
            Assert.AreEqual("Fountain", hit.name);

            FlowFieldNavigator navigator = Object.FindAnyObjectByType<FlowFieldNavigator>();
            navigator.Rebuild();
            Assert.IsFalse(navigator.Field.IsWalkable(navigator.CellOf(Center + new Vector2(0.5f, 0.5f))));
            Assert.IsFalse(navigator.Field.IsWalkable(navigator.CellOf(Center + new Vector2(-1.5f, 0.5f))));
            Assert.IsTrue(navigator.Field.IsWalkable(navigator.CellOf(Center + new Vector2(0f, 3.5f))));
        }

        [UnityTest]
        public IEnumerator Gates_AreSolid()
        {
            yield return null;

            int walls = LayerMask.GetMask("Walls");
            foreach (Vector2 cell in new[] { new Vector2(15.5f, 28.5f), new Vector2(28.5f, 16.5f), new Vector2(16.5f, 3.5f), new Vector2(3.5f, 15.5f) })
            {
                Assert.IsNotNull(Physics2D.OverlapPoint(cell, walls), $"gate cell {cell} lets bodies through");
            }
        }

        [UnityTest]
        public IEnumerator SpawnPointsWeaponSlotsAndFloor_AreReachableForATwoByTwoBody()
        {
            yield return null;

            var goals = new List<(string name, Vector3 position)>();
            foreach (SpawnPoint point in Object.FindAnyObjectByType<Zone>().SpawnPoints)
            {
                goals.Add((point.name, point.transform.position));
            }

            foreach (Transform slot in GameObject.Find("WeaponSlots").transform)
            {
                goals.Add((slot.name, slot.position + Vector3.down));
            }

            Tilemap walls = GameObject.Find("Walls").GetComponent<Tilemap>();
            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 32; x++)
                {
                    var world = new Vector2(x + 0.5f, y + 0.5f);
                    if (Vector2.Distance(world, Center) < FloorRadius && !walls.HasTile(new Vector3Int(x, y, 0)))
                    {
                        goals.Add(($"floor ({x}, {y})", world));
                    }
                }
            }

            Assert.Greater(goals.Count, 400);
            LevelReachability.AssertReachable(goals);
        }

        private void Teleport(Vector2 position)
        {
            var body = _player.GetComponent<Rigidbody2D>();
            body.position = position;
            body.transform.position = position;
        }
    }
}
