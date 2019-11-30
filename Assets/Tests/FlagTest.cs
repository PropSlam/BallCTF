using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.Animations;

namespace Tests {
    public class FlagTest {
        private Flag enemyFlag;
        private Flag friendlyFlag;
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

            // Spawn UI.
            GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/UI"));

            // Spawn floor plane.
            var plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            plane.transform.localScale = new Vector3(10, 10, 10);

            var flagPrefab = Resources.Load<GameObject>("Prefabs/Flag");

            // Spawn enemy flag.
            var enemyFlagObj = GameObject.Instantiate(flagPrefab, new Vector3(2, 0, 0), Quaternion.identity);
            enemyFlag = enemyFlagObj.GetComponent<Flag>();
            enemyFlag.team.Value = Team.Purple;

            // Spawn friendly flag.
            var friendlyFlagObj = GameObject.Instantiate(flagPrefab, new Vector3(-2, 0, 0), Quaternion.identity);
            friendlyFlag = friendlyFlagObj.GetComponent<Flag>();
            friendlyFlag.team.Value = Team.Yellow;

            // Spawn player.
            var playerPrefab = Resources.Load<GameObject>("Prefabs/Player/Player");
            var playerObj = GameObject.Instantiate(playerPrefab, new Vector3(0, 1, 0), Quaternion.identity);
            player = playerObj.GetComponent<Player>();
            player.team.Value = Team.Yellow;
        }

        // Tests flag can be grabbed by other team.
        [UnityTest]
        public IEnumerator GrabbableByOtherTeam() {
            // Move player to enemy flag.
            player.transform.position = enemyFlag.transform.position;
            yield return new WaitForFixedUpdate();

            // Move player somewhere else.
            player.transform.position += new Vector3(10, 0, 0);
            yield return new WaitForEndOfFrame();

            // Assert flag is held by player.
            Assert.AreEqual(player, enemyFlag.heldBy.Value);
            Assert.NotNull(player.heldFlag.Value);

            // Assert flag position is set to player.
            var positionConstraint = enemyFlag.GetComponent<PositionConstraint>();
            Assert.AreEqual(player.transform, positionConstraint.GetSource(0).sourceTransform);
            Assert.IsTrue(positionConstraint.constraintActive);
            Assert.Less(Vector3.Distance(player.transform.position, enemyFlag.transform.position), 0.1);
        }

        // Tests flag cannot be grabbed by same team.
        [UnityTest]
        public IEnumerator NotGrabbableBySameTeam() {
            // Move player to friendly flag.
            player.transform.position = friendlyFlag.transform.position;
            yield return new WaitForFixedUpdate();

            // Move player away from friendly flag.
            player.transform.position += new Vector3(10, 0, 0);
            yield return new WaitForFixedUpdate();

            // Assert flag is not held by anyone.
            Assert.IsNull(friendlyFlag.heldBy.Value);
            Assert.IsNull(player.heldFlag.Value);

            // Assert flag position is not near player.
            var positionConstraint = friendlyFlag.GetComponent<PositionConstraint>();
            Assert.AreEqual(0, positionConstraint.sourceCount);
            Assert.IsFalse(positionConstraint.constraintActive);
            Assert.Greater(Vector3.Distance(player.transform.position, friendlyFlag.transform.position), 0.1);
        }

        // Tests flag is dropped when player dies.
        [UnityTest]
        public IEnumerator DroppedOnPlayerDeath() {
            enemyFlag.GrabBy(player);

            // Teleport player far away.
            player.transform.position = new Vector3(10, 0, 0);

            // Advance one tick.
            yield return new WaitForFixedUpdate();

            // Kill player.
            player.alive.Value = false;

            // Advance one tick.
            yield return new WaitForFixedUpdate();

            // Assert flag is not held by anyone.
            Assert.IsTrue(enemyFlag.heldBy.Value == null);
            Assert.IsTrue(player.heldFlag.Value == null);

            // Assert flag position is reset.
            var positionConstraint = enemyFlag.GetComponent<PositionConstraint>();
            Assert.AreEqual(0, positionConstraint.sourceCount);
            Assert.IsFalse(positionConstraint.constraintActive);
            Assert.Less(Vector3.Distance(enemyFlag.transform.position, new Vector3(2, 0, 0)), 0.01);
        }

        // Tests flag can be captured.
        [UnityTest]
        public IEnumerator CanBeCaptured() {
            enemyFlag.GrabBy(player);

            var captured = false;
            enemyFlag.captured.AddListener(_ => captured = true);

            // Move player to friendly flag.
            player.transform.position = friendlyFlag.transform.position;
            yield return new WaitForFixedUpdate();

            // Assert flag is not held by anyone.
            Assert.IsTrue(enemyFlag.heldBy.Value == null);
            Assert.IsTrue(player.heldFlag.Value == null);

            // Assert flag was captured.
            Assert.IsTrue(captured);

            // Assert flag position is reset.
            var positionConstraint = enemyFlag.GetComponent<PositionConstraint>();
            Assert.AreEqual(0, positionConstraint.sourceCount);
            Assert.IsFalse(positionConstraint.constraintActive);
            Assert.Less(Vector3.Distance(enemyFlag.transform.position, new Vector3(2, 0, 0)), 0.01);
        }
    }
}
