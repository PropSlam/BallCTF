using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TestTools;

namespace Tests {
    public class PlayerTest {
        private Player player;

        [SetUp]
        public void Setup() {
            // Spawn main camera.
            var cam = GameObject.Instantiate(
                new GameObject("Camera"),
                new Vector3(5, 0, 0),
                Quaternion.identity
            );
            cam.AddComponent<Camera>();
            cam.tag = "MainCamera";

            // Load player prefab.
            var playerPrefab = Resources.Load<GameObject>("Prefabs/Player");
            var playerObj = GameObject.Instantiate(playerPrefab);
            player = playerObj.GetComponent<Player>();
        }

        // Tests material is set to Yellow when team is Yellow.
        [UnityTest]
        public IEnumerator SetsYellowTeamMaterial() {
            // Set to Yellow team.
            player.team.Value = Team.Yellow;

            // Advance one tick.
            yield return new WaitForFixedUpdate();

            // Assert player material contains "Yellow".
            StringAssert.Contains(
                "Yellow",
                player.GetComponent<Renderer>().material.ToString()
            );
        }

        // Tests material is set to Purple when team is Purple.
        [UnityTest]
        public IEnumerator SetsPurpleTeamMaterial() {
            // Set to Purple team.
            player.team.Value = Team.Purple;

            // Advance one tick.
            yield return new WaitForFixedUpdate();

            // Assert player material contains "Purple".
            StringAssert.Contains(
                "Purple",
                player.GetComponent<Renderer>().material.ToString()
            );
        }

        // Tests alias is rendered correctly.
        [UnityTest]
        public IEnumerator RendersAlias() {
            // Set player alias and move the player transform.
            player.alias.Value = "Jimmy Testerino";
            player.transform.position = new Vector3(0, 0, 5);

            yield return new WaitForEndOfFrame();

            // Assert text content is correct.
            var aliasText = player.transform.Find("Canvas/Alias").GetComponent<Text>();
            Assert.AreEqual("Jimmy Testerino", aliasText.text);

            // Assert text position is correct.
            var expectedAliasPos = player.transform.position + Camera.main.transform.up;
            var expectedScreenPos = Camera.main.WorldToScreenPoint(expectedAliasPos);
            var differenceToExpected = Vector3.Distance(expectedScreenPos, aliasText.rectTransform.position);
            Assert.Less(differenceToExpected, 0.01f); // close enough...
        }
    }
}
