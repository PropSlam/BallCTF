using UnityEngine;

class Bootstrapper : MonoBehaviour {
    void Awake() {
        if (Application.isBatchMode) {
            Object.Instantiate(Resources.Load<Server>("Prefabs/Network/Server"));
        } else {
            Object.Instantiate(Resources.Load<Client>("Prefabs/Network/Client"));
        }
    }
}
