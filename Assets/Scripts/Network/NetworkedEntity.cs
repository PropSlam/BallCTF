using System.Collections.Generic;
using UnityEngine;

class NetworkedEntity : MonoBehaviour {
    static Dictionary<int, NetworkedEntity> entities = new Dictionary<int, NetworkedEntity>();
    static Queue<int> availableIds = new Queue<int>();
    static int nextId = 0;

    public int id;

    void Awake() {
        id = availableIds.Count == 0 ? nextId++ : availableIds.Dequeue();
        entities[id] = this;
    }

    void OnDestroy() {
        availableIds.Enqueue(id);
        entities.Remove(id);
    }
}
