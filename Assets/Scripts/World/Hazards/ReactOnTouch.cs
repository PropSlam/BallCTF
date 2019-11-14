using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReactOnTouch : MonoBehaviour
{
    public LayerMask reactLayer;


    private void OnTriggerEnter(Collider other) {
        if(other.CompareTag("Player")) {
            other.GetComponent<Player>().Death(this);
        }
    }
}
