using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathHazard : MonoBehaviour {
    void OnTriggerEnter(Collider other) {
        var player = other.GetComponent<Player>();
        if (player) {
            player.alive.Value = false;
        }
    }
}