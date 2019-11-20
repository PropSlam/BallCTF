using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TestTools;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Animations;

namespace Tests {
    public class FlagTest : InputTestFixture {
        private Flag flag;
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

            // Spawn flag.
            var flagPrefab = Resources.Load<GameObject>("Prefabs/Flag");
            var flagObj = GameObject.Instantiate(flagPrefab, new Vector3(2, 0, 0), Quaternion.identity);
            flag = flagObj.GetComponent<Flag>();

            // Spawn player.
            var playerPrefab = Resources.Load<GameObject>("Prefabs/Player/Player");
            var playerObj = GameObject.Instantiate(playerPrefab, new Vector3(0, 1, 0), Quaternion.identity);
            player = playerObj.GetComponent<Player>();

            // Initialize keyboard input
            keyboard = InputSystem.AddDevice<Keyboard>();
        }

        // Tests flag can be grabbed by other team.
        [UnityTest]
        public IEnumerator GrabbableByOtherTeam() {
            // Set player to Yellow team, flag to Purple team.
            player.team.Value = Team.Yellow;
            flag.team.Value = Team.Purple;

            // Move player to flag.
            Time.timeScale = 10f;
            Press(keyboard.rightArrowKey);
            yield return new WaitForSeconds(2);
            Release(keyboard.rightArrowKey);
            Time.timeScale = 1f;

            // Assert flag is held by player.
            Assert.AreEqual(player, flag.heldBy.Value);

            // Assert flag position is set to player.
            var positionConstraint = flag.GetComponent<PositionConstraint>();
            Assert.AreEqual(player.transform, positionConstraint.GetSource(0).sourceTransform);
            Assert.IsTrue(positionConstraint.constraintActive);
            Assert.Less(Vector3.Distance(player.transform.position, flag.transform.position), 0.1);
        }

        // Tests flag cannot be grabbed by same team.
        [UnityTest]
        public IEnumerator NotGrabbableBySameTeam() {
            // Set player to Yellow team, flag to Yellow team.
            player.team.Value = Team.Yellow;
            flag.team.Value = Team.Yellow;

            // Move player to flag.
            Time.timeScale = 10f;
            Press(keyboard.rightArrowKey);
            yield return new WaitForSeconds(2);
            Release(keyboard.rightArrowKey);
            Time.timeScale = 1f;

            // Assert flag is not held by anyone.
            Assert.IsNull(flag.heldBy.Value);

            // Assert flag position is not near player.
            var positionConstraint = flag.GetComponent<PositionConstraint>();
            Assert.AreEqual(0, positionConstraint.sourceCount);
            Assert.IsFalse(positionConstraint.constraintActive);
            Assert.Greater(Vector3.Distance(player.transform.position, flag.transform.position), 0.1);
        }

        // Tests flag is dropped when player dies.
        [UnityTest]
        public IEnumerator DroppedOnPlayerDeath() {
            // Set player to Yellow team, flag to Purple team.
            player.team.Value = Team.Yellow;
            flag.team.Value = Team.Purple;

            flag.heldBy.Value = player;

            // Move player to the right.
            Time.timeScale = 10f;
            Press(keyboard.rightArrowKey);
            yield return new WaitForSeconds(2);
            Release(keyboard.rightArrowKey);
            Time.timeScale = 1f;

            // Kill player.
            player.alive.Value = false;
            
            // Advance one tick.
            yield return new WaitForFixedUpdate();

            // Assert flag is not held by anyone.
            Assert.IsTrue(flag.heldBy.Value == null);

            // Assert flag position is reset.
            var positionConstraint = flag.GetComponent<PositionConstraint>();
            Assert.AreEqual(0, positionConstraint.sourceCount);
            Assert.IsFalse(positionConstraint.constraintActive);
            Assert.Less(Vector3.Distance(flag.transform.position, new Vector3(2, 0, 0)), 0.01);
        }
    }
}
