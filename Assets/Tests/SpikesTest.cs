using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TestTools;

namespace Tests {

    public class SpikeTest {
        private Player player;
        private GameObject spikes;

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
            playerObj.transform.SetPositionAndRotation(new Vector3(0, 2, 5), Quaternion.identity);

            var spikesPrefab = Resources.Load<GameObject>("Prefabs/Spikes");
            var spikesObj = GameObject.Instantiate(spikesPrefab);
            spikes = spikesObj;
            spikes.transform.SetPositionAndRotation(new Vector3(0, 0, 5), Quaternion.Euler(-90, 0, 0));
        }

        // Tests player gameobject is destroyed when spikes are touched
        [UnityTest]
        public IEnumerator DestroyPlayerOnSpikesTrigger() {
            // Increase Time scale to quicken test
            Time.timeScale = 10;

            // Advance one second
            yield return new WaitForSeconds(1);
            Time.timeScale = 1;

            // Assert player is null to confirm destroyed
            // Use IsTrue (and compare) instead of IsNull/Null because Unity Destroy causes pseudo-null situation
            Assert.IsTrue(player == null);
        }
    }
}