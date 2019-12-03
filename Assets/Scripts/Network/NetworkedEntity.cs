using System.Collections.Generic;
using UnityEngine;

class NetworkedEntity : MonoBehaviour {
    public readonly static Dictionary<int, NetworkedEntity> entities = new Dictionary<int, NetworkedEntity>();
    static readonly Queue<int> availableIds = new Queue<int>();
    static int nextId = 0;

    private int _id = -1;
    public int Id {
        get {
            if (_id < 0) {
                _id = availableIds.Count == 0 ? nextId++ : availableIds.Dequeue();
                entities[_id] = this;
            }
            return _id;
        }
        set {
            if (entities.ContainsKey(_id)) Free();
            _id = value;
            entities[_id] = this;
        }
    }

    void OnDestroy() {
        Free();
    }

    void Free() {
        availableIds.Enqueue(Id);
        entities.Remove(Id);
    }
}
