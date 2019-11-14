using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathHazard : MonoBehaviour {
    public LayerMask touchLayer;

    void OnTriggerEnter(Collider other) {
        if((1 << other.gameObject.layer) == touchLayer.value) {
            other.GetComponent<Player>().Death();
        }
    }
}