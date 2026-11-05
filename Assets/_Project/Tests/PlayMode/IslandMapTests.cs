using System.Collections;
using System.Linq;
using NUnit.Framework;
using RushOps.Levels;
using RushOps.Save;
using RushOps.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace RushOps.Tests.PlayMode
{
    public class IslandMapTests
    {
        private IslandMap _map;

        private IEnumerator Load()
        {
            yield return SceneManager.LoadSceneAsync(IslandMap.SceneName, LoadSceneMode.Single);
            yield return null;
            _map = Object.FindFirstObjectByType<IslandMap>();
            Assert.IsNotNull(_map, "IslandMap scene has no IslandMap");
        }

        [TearDown]
        public void ForgetProgress()
        {
            SaveGameIsolation.Reset();
        }

        private LevelNode Node(int island, int level)
        {
            return _map.Nodes.Single(n => n.Island == island && n.Level == level);
        }

        private static GameObject Cover(int island)
        {
            return GameObject.Find($"Island_{island + 1}").transform.Find("LockedCover").gameObject;
        }

        [UnityTest]
        public IEnumerator NewGame_ShowsTheBeach_WithOnlyLevel1_1Open()
        {
            SaveGameIsolation.Reset();
            yield return Load();

            Assert.AreEqual(3, _map.IslandCount);
            Assert.AreEqual(9, _map.Nodes.Length);
            Assert.AreEqual(0, _map.Current);
            Assert.AreEqual("Insel 1: Strand", GameObject.Find("Title").GetComponent<Text>().text);
            Assert.AreEqual(LevelNodeState.Open, Node(0, 0).State);
            Assert.AreEqual(LevelNodeState.Locked, Node(0, 1).State, "level 1-2 opens after 1-1");
            Assert.AreEqual(LevelNodeState.Locked, Node(0, 2).State);
            Assert.AreEqual("Strandbad", Node(0, 0).NameShown, "decision C11: number and short name");
            Assert.AreEqual("Strandpromenade", Node(0, 1).NameShown);
            Assert.AreEqual("Leuchtturm", Node(0, 2).NameShown);
            Assert.AreEqual("Hinterhöfe", Node(1, 0).NameShown, "decision C11");
            Assert.AreEqual("U-Bahn", Node(1, 1).NameShown);
            Assert.AreEqual("Kanalisation", Node(1, 2).NameShown);
            Assert.AreEqual("Aschefeld", Node(2, 0).NameShown);
            Assert.AreEqual("Lavastrom", Node(2, 1).NameShown);
            Assert.IsFalse(Cover(0).activeSelf);
            Assert.IsTrue(Cover(1).activeSelf, "island 2 not locked");
            Assert.IsTrue(Cover(2).activeSelf, "island 3 not locked");
            StringAssert.Contains("Strand", Cover(1).GetComponentInChildren<Text>().text);
            Assert.IsNull(GameObject.Find("DownArrow"), "no island below the first");
            Assert.IsNotNull(GameObject.Find("UpArrow"));
        }

        [UnityTest]
        public IEnumerator FinishedLevel_ShowsItsStars()
        {
            SaveGameService save = SaveGameIsolation.Reset();
            ProgressRules.RecordCompletion(save.Data.Progress, "Level_1-1", 2, 1800);
            yield return Load();

            Assert.AreEqual(LevelNodeState.Done, Node(0, 0).State);
            Assert.AreEqual(2, Node(0, 0).StarsShown);
            Assert.AreEqual(LevelNodeState.Open, Node(0, 1).State, "finishing 1-1 opens 1-2");
        }

        [UnityTest]
        public IEnumerator Arrows_SlideBetweenTheIslands()
        {
            SaveGameIsolation.Reset();
            yield return Load();

            _map.Next();
            Assert.AreEqual(1, _map.Current);
            Assert.AreEqual("Insel 2: Düstere Stadt", GameObject.Find("Title").GetComponent<Text>().text);
            float start = Time.realtimeSinceStartup;
            while (!_map.IsSettled && Time.realtimeSinceStartup - start < 2f)
            {
                yield return null;
            }

            Assert.IsTrue(_map.IsSettled, "map did not slide to island 2");
            _map.Next();
            _map.Next();
            Assert.AreEqual(2, _map.Current, "slid past the last island");
            _map.Previous();
            Assert.AreEqual(1, _map.Current);
        }

        [UnityTest]
        public IEnumerator TappingAnOpenLevel_StartsIt()
        {
            SaveGameIsolation.Reset();
            yield return Load();

            Node(0, 0).GetComponent<Button>().onClick.Invoke();
            yield return null;
            yield return null;

            Assert.AreEqual("Level_1-1", SceneManager.GetActiveScene().name);
        }

        [UnityTest]
        public IEnumerator LockedAndMissingLevels_CannotBeStarted()
        {
            SaveGameIsolation.Reset();
            yield return Load();

            Assert.IsFalse(Node(0, 1).GetComponent<Button>().interactable);
            Assert.IsFalse(Node(1, 0).GetComponent<Button>().interactable);
            _map.Play(1, 0);
            yield return null;

            Assert.AreEqual(IslandMap.SceneName, SceneManager.GetActiveScene().name);
        }

        [UnityTest]
        public IEnumerator Back_LeadsToTheMainMenu()
        {
            SaveGameIsolation.Reset();
            yield return Load();

            GameObject.Find("BackButton").GetComponent<Button>().onClick.Invoke();
            yield return null;
            yield return null;

            Assert.AreEqual(MainMenu.SceneName, SceneManager.GetActiveScene().name);
        }
    }
}
