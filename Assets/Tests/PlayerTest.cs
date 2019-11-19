using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TestTools;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace Tests {
    public class PlayerTest : InputTestFixture {
        private Player player;
        private Keyboard keyboard;

        [SetUp]
        public new void Setup() {
            // Spawn main camera.
            var cam = GameObject.Instantiate(
                new GameObject("Camera"),
                new Vector3(5, 0, 0),
                Quaternion.identity
            );
            cam.AddComponent<Camera>();
            cam.tag = "MainCamera";

            // Spawn UI.
            GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/UI"));

            // Spawn floor plane.
            var plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            plane.transform.localScale = new Vector3(10, 10, 10);

            // Load player prefab.
            var playerPrefab = Resources.Load<GameObject>("Prefabs/Player/Player");
            var playerObj = GameObject.Instantiate(playerPrefab, new Vector3(0, 1, 0), Quaternion.identity);
            player = playerObj.GetComponent<Player>();

            // Initialize keyboard input
            keyboard = InputSystem.AddDevice<Keyboard>();
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
            var ui = GameObject.FindGameObjectWithTag("UI");
            var aliasText = ui.transform.Find("Alias(Clone)").GetComponent<Text>();
            Assert.AreEqual("Jimmy Testerino", aliasText.text);

            // Assert text position is correct.
            var expectedAliasPos = player.transform.position + Camera.main.transform.up;
            var expectedScreenPos = Camera.main.WorldToScreenPoint(expectedAliasPos);
            var differenceToExpected = Vector3.Distance(expectedScreenPos, aliasText.rectTransform.position);
            Assert.Less(differenceToExpected, 0.01f); // close enough...
        }

        private IEnumerator MoveWithKey(KeyControl key) {
            Time.timeScale = 10f;
            Press(key);
            yield return new WaitForSeconds(2);
            Release(key);
            Time.timeScale = 1f;
        }

        [UnityTest]
        public IEnumerator PlayerMovesLeft() {
            var oldPos = player.transform.position.x;
            yield return MoveWithKey(keyboard.leftArrowKey);
            var newPos = player.transform.position.x;
            Assert.Less(newPos - oldPos, -0.1);
        }

        [UnityTest]
        public IEnumerator PlayerMovesRight() {
            var oldPos = player.transform.position.x;
            yield return MoveWithKey(keyboard.rightArrowKey);
            var newPos = player.transform.position.x;
            Assert.Greater(newPos - oldPos, 0.1);
        }

        [UnityTest]
        public IEnumerator PlayerMovesDown() {
            var oldPos = player.transform.position.z;
            yield return MoveWithKey(keyboard.downArrowKey);
            var newPos = player.transform.position.z;
            Assert.Less(newPos - oldPos, -0.1);
        }

        [UnityTest]
        public IEnumerator PlayerMovesUp() {
            var oldPos = player.transform.position.z;
            yield return MoveWithKey(keyboard.upArrowKey);
            var newPos = player.transform.position.z;
            Assert.Greater(newPos - oldPos, 0.1);
        }
    }
}
