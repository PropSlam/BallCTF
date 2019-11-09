using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLib;
using LiteNetLib.Utils;

public class NetworkedObject : MonoBehaviour {
    const bool KEEP_ABOVE = true;
    const float KEEP_ABOVE_ELEVATION = -5f;
    const float KEEP_ABOVE_ADJUSTMENT = 10f;
    public List<NetworkedComponent> networkedComponents;
    public static Dictionary<int, NetworkedObject> objDict = new Dictionary<int, NetworkedObject>();
    public static List<NetworkedObject> objList = new List<NetworkedObject>();
    //public static Dictionary<int, bool> clientDestroyedObjects = new Dictionary<int, bool>();
    public static List<int> destroyedObjects = new List<int>();
    public static List<NetworkedObject> dirtyObjects = new List<NetworkedObject>();
    public const int SERVER_OWNER_ID = -1; // Objects not owned by clients should have this ID
    public const int NO_OWNER_ID = -2; // The default owner of a client, not even the server "owns" it
    public int ownerId = SERVER_OWNER_ID;
    public short prefab; // ID of prefab in GameServer prefab list
    public int sceneId; // Scene ID of object (if it was spawned as part of a scene)
    int parentId; // ID of parent object
    NetworkedObject parent; // Parent object, this is not always reliable
    Vector3 position;
    Quaternion rotation;
    public byte layerDepth; // How many child layers to set automatically; default 0
    public bool syncTransform = true;
    bool sleep;
    bool gravity;
    bool kinematic;
    Vector3 velocity;
    Vector3 angularVelocity;
    public bool useLocalPosition;
    Vector3 prevPosition; // For interpolation
    Quaternion prevRotation; // For interpoaltion

    public int debugId;

    float lastUpdateTime;
    bool lerp;

    bool dirty;

    Vector3 dirtyCheckPosition;
    Quaternion dirtyCheckRotation;

    public Rigidbody body { get; private set; }

    int _id = 0;
    public int id {
        get {
            return _id;
        }
        set {
            if (_id != 0 && objDict.ContainsKey(_id)) {
                objDict.Remove(_id);
                objList.Remove(this);
            }
            _id = value;
            objDict.Add(_id, this);
            objList.Add(this);
            SetDirty();
        }
    }


    void Awake() {
        if (sceneId < 0 && !GameServer.Active && !GameClient.Active) {
            id = sceneId;
        }
        if (id == 0) {
            id = GetNewId();
        }
        body = GetComponent<Rigidbody>();
        if (networkedComponents.Count == 0) {
            networkedComponents = new List<NetworkedComponent>(GetComponents<NetworkedComponent>());
        }
    }

    void OnDestroy() {
        objDict.Remove(id);
        objList.Remove(this); // TODO Consider doing this elsewhere
        if (dirtyObjects.Contains(this)) {
            dirtyObjects.Remove(this);
        }
        if (GameServer.Active) {
            destroyedObjects.Add(id);
        }
    }

    public static int GetNewId() {
        int key = Random.Range(1, int.MaxValue);
        while (objDict.ContainsKey(key)) {
            key = Random.Range(1, int.MaxValue);
        }
        return key;
    }

    public static int NetworkWriteAll(NetDataWriter writer, int start, List<NetworkedObject> objectList, bool allComponents = false) {
        int length = objectList.Count - start;
        if (length > GameServer.OBJECT_BATCH_SIZE) {
            length = GameServer.OBJECT_BATCH_SIZE;
        }
        writer.Put(GameServer.MESSAGE_OBJECT_BATCH); // This is the type of message
        writer.Put(length); // How many objects are being sent
        int i = 0;
        while (i < length) {
            objectList[i + start].NetworkWrite(writer, allComponents);
            i++;
            if (i >= length) {
                return i + start;
            }
        }
        return 0;
    }

    void NetworkWrite(NetDataWriter writer, bool allComponents = false) {
        writer.Put(id); // -2
        writer.Put(prefab); // -1
        writer.Put(ownerId); // 0
        if (parent != null) {
            writer.Put(parent.id); // 1 parent
        } else {
            writer.Put((int)0); // 1 parent
        }
        writer.Put(useLocalPosition); // 2 useLocal
        writer.Put((byte)layerDepth); // 3 layerDepth
        writer.Put((byte)gameObject.layer); // 4 layer
        writer.Put(transform.position); // 5 position
        writer.Put(transform.rotation.eulerAngles); // 6 angle
        writer.Put(syncTransform); // 7 syncEnabled
        if (body) {
            writer.Put(body.velocity); // 8 velocity
            writer.Put(body.angularVelocity); // 9 angVelocity
            writer.Put(body.isKinematic); // 10 kinematic
            writer.Put(body.useGravity); // 11 gravity 
            writer.Put(body.IsSleeping()); // 12 sleep
        }
        foreach (NetworkedComponent comp in networkedComponents) {
            comp.PrepareSerialize(writer, allComponents);
        }
    }

    void NetworkRead(NetDataReader reader, bool ownerChanged = false, bool firstSync = false) { // id, prefab, ownerId have already been read in
        if (!firstSync && ((GameServer.Active && ownerId == SERVER_OWNER_ID) || (GameClient.Active && ownerId == GameClient.ownerId)) && !ownerChanged) {
            reader.GetInt(); // 1 parent
            reader.GetBool(); // 2 useLocal
            reader.GetByte(); // 3  layerDepth
            reader.GetByte(); // 4  layer
            reader.GetVector3(); // 5 position
            reader.GetVector3(); // 6 angle
            reader.GetBool(); // 7 syncEnabled
            if (body) {
                reader.GetVector3(); // 8 velocity
                reader.GetVector3(); // 9 angVelocity
                reader.GetBool(); // 10 kinematic
                reader.GetBool(); // 11 gravity
                reader.GetBool(); // 12 sleep
            }
            position = transform.position;
            rotation = transform.rotation;
            if (body) {
                velocity = body.velocity;
                angularVelocity = body.angularVelocity;
            }
            prevPosition = transform.position;
            prevRotation = transform.rotation;
            lastUpdateTime = Time.time;
            lerp = false;



            foreach (NetworkedComponent comp in networkedComponents) {
                comp.PrepareDeserialize(reader);
            }
        } else {
            parentId = reader.GetInt(); // 1 parent
            useLocalPosition = reader.GetBool(); // 2 useLocal
            layerDepth = reader.GetByte(); // 3 layerDepth
            gameObject.layer = reader.GetByte(); // 4  layer // Layers only use the first 5 bits (max layer 31)
            if (layerDepth > 0) {
                SetChildrenLayer(gameObject.layer, transform, layerDepth - 1);
            }

            position = reader.GetVector3(); // 5 position
            rotation = Quaternion.Euler(reader.GetVector3()); // 6 angle
            syncTransform = reader.GetBool(); // 7 syncEnabled
            if (body) {
                velocity = reader.GetVector3(); // 8 velocity
                angularVelocity = reader.GetVector3(); // 9 angVelocity
                kinematic = reader.GetBool(); // 10 kinematic
                gravity = reader.GetBool(); // 11 gravity
                sleep = reader.GetBool(); // 12 sleep
            }
            prevPosition = transform.position;
            prevRotation = transform.rotation;
            lastUpdateTime = Time.time;
            lerp = true;
            TrySetParent();

            if (firstSync) {
                transform.position = position;
                transform.rotation = rotation;
                prevPosition = transform.position;
                prevRotation = transform.rotation;
                if (body) {
                    body.velocity = velocity;
                    body.angularVelocity = angularVelocity;
                    body.isKinematic = kinematic;
                    body.useGravity = gravity;
                    if (sleep) {
                        body.Sleep();
                    }
                }
            }

            foreach (NetworkedComponent comp in networkedComponents) {
                comp.PrepareDeserialize(reader);
            }
            if (GameServer.Active) {
                SetDirty();
            }
        }
    }

    public static void SendNetworkedObjectsDirty(List<NetPeer> peers, NetDataWriter writer) {
        if (peers.Count != 0) {
            int cutoff = 0; // Updates have to be batched to fit in UDP packet size limits
            do {
                writer.Reset();
                cutoff = NetworkWriteAll(writer, cutoff, dirtyObjects);
                foreach (NetPeer peer in peers) {
                    peer.Send(writer, DeliveryMethod.ReliableOrdered);
                }
            } while (cutoff != 0);
            ClearDirtyObjects();
        }
    }

    public static void SendNetworkedObjects(List<NetPeer> peers, NetDataWriter writer) {
        if (peers.Count != 0) {
            int cutoff = 0; // Updates have to be batched to fit in UDP packet size limits
            do {
                writer.Reset();
                cutoff = NetworkWriteAll(writer, cutoff, objList, allComponents: true);
                foreach (NetPeer peer in peers) {
                    peer.Send(writer, DeliveryMethod.ReliableOrdered);
                }
            } while (cutoff != 0);
        }
    }

    public static void SendOwnedNetworkedObjects(List<NetPeer> peers, NetDataWriter writer) {
        if (peers.Count > 0) {
            List<NetworkedObject> ownedDirtyObjects = new List<NetworkedObject>();
            foreach (NetworkedObject obj in dirtyObjects) {
                if (obj.ownerId == GameClient.ownerId) {
                    ownedDirtyObjects.Add(obj);
                }
            }
            int cutoff = 0; // Updates have to be batched to fit in UDP packet size limits
            do {
                writer.Reset();
                cutoff = NetworkWriteAll(writer, cutoff, ownedDirtyObjects);
                foreach (NetPeer peer in peers) {
                    peer.Send(writer, DeliveryMethod.ReliableOrdered);
                }
            } while (cutoff != 0);
            ClearDirtyObjects();
        }
    }

    public static void NetworkReadAll(NetDataReader reader, int peerOwnerId = SERVER_OWNER_ID) {
        int length = reader.GetInt();
        for (int i = 0; i < length; i++) {
            int id = reader.GetInt();
            short prefab = reader.GetShort();
            int ownerId = reader.GetInt();
            if (objDict.ContainsKey(id)) {
                if ((GameServer.Active && objDict[id].ownerId == peerOwnerId) || GameClient.Active) {
                    objDict[id].prefab = prefab;
                    bool ownerChanged = objDict[id].ownerId != ownerId;
                    objDict[id].ownerId = ownerId;
                    objDict[id].NetworkRead(reader, ownerChanged);
                } else {
                    objDict[id].NetworkRead(reader);
                }

            } else if (!GameServer.Active) { // && (!clientDestroyedObjects.ContainsKey(id) ||  !clientDestroyedObjects[id])
                GameObject newObj = GameObject.Instantiate(GameServer.singleton.prefabs[prefab]);
                NetworkedObject newNetObj = newObj.GetComponent<NetworkedObject>();
                if (newNetObj) {
                    newNetObj.id = id;
                    newNetObj.prefab = prefab;
                    newNetObj.ownerId = ownerId;
                    newNetObj.NetworkRead(reader, firstSync: true);
                }
            }
        }
    }

    void Update() {
        debugId = id;
        if (GameServer.Active || (ownerId == GameClient.ownerId && GameClient.Active)) {
            if (KEEP_ABOVE && transform.position.y < KEEP_ABOVE_ELEVATION) {
                transform.position = new Vector3(transform.position.x, KEEP_ABOVE_ELEVATION + KEEP_ABOVE_ADJUSTMENT, transform.position.z);
                if (body) {
                    body.velocity = new Vector3(body.velocity.x, 0f, body.velocity.y);
                }
            }
            if (body && GameClient.Active) {
                body.isKinematic = kinematic;
                body.useGravity = gravity;
                body.WakeUp();
            }
            if (syncTransform && ((body && !body.IsSleeping()) || (dirtyCheckPosition != transform.position || dirtyCheckRotation != transform.rotation))) {
                SetDirty();
                dirtyCheckPosition = transform.position;
                dirtyCheckRotation = transform.rotation;
            }
            if (transform.parent == null) {
                if (parent != null) {
                    parent = null;
                    parentId = 0;
                    SetDirty();
                }
            } else {
                if (parent == null) {
                    NetworkedObject parentObj = transform.parent.gameObject.GetComponent<NetworkedObject>();
                    if (parentObj) {
                        parent = parentObj;
                        parentId = parentObj.id;
                        SetDirty();
                    }
                }
            }

        }

        if (id == 0) {
            return;
        }
        TrySetParent();
        if (!syncTransform) {
            lerp = false;
        }
        if (lerp) {
            float lerpProgress = (Time.time - lastUpdateTime) / GameServer.UPDATE_TIME;
            if (lerpProgress >= 1) {
                transform.position = position;
                transform.rotation = rotation;
                if (body) {
                    body.velocity = velocity;
                    body.angularVelocity = angularVelocity;
                    if (sleep) {
                        body.isKinematic = true;
                        body.useGravity = false;
                        body.velocity = Vector3.zero;
                        body.angularVelocity = Vector3.zero;
                        body.Sleep();
                    } else {
                        body.isKinematic = kinematic;
                        body.useGravity = gravity;
                    }
                }
                lerp = false;
            } else {
                transform.position = Vector3.Lerp(prevPosition, position, lerpProgress);
                transform.rotation = Quaternion.Slerp(prevRotation, rotation, lerpProgress);
                if (body) {
                    body.velocity = velocity;
                    body.angularVelocity = angularVelocity;
                }
            }
        }
    }

    void TrySetParent() {
        if (parentId != 0 && parent == null && objDict.ContainsKey(parentId)) {
            parent = objDict[parentId];
            transform.parent = parent.transform;
        } else if (parentId == 0 && parent) {
            parent = null;
            transform.parent = null;
        }
    }

    public void SetDirty() {
        if (!dirty && !dirtyObjects.Contains(this)) {
            dirtyObjects.Add(this);
            dirty = true;
        }
    }

    public static void ClearDirtyObjects() {
        foreach (NetworkedObject obj in dirtyObjects) {
            obj.dirty = false;
            foreach (NetworkedComponent comp in obj.networkedComponents) {
                comp.dirty = false;
            }
        }
        dirtyObjects = new List<NetworkedObject>();
    }

    public NetDataWriter GetComponentWriter(NetworkedComponent component) {
        if (!networkedComponents.Contains(component)) {
            return null;
        }
        NetDataWriter writer = new NetDataWriter();
        writer.Put(GameServer.MESSAGE_COMPONENT_CUSTOM);
        writer.Put(id);
        writer.Put((byte)networkedComponents.IndexOf(component));
        return writer;
    }

    public static void HandleComponentMessage(NetDataReader reader, int peerId) {
        int id = reader.GetInt();
        byte compIndex = reader.GetByte();
        if (objDict.ContainsKey(id) && objDict[id].networkedComponents.Count > compIndex) {
            objDict[id].networkedComponents[compIndex].HandleMessage(reader, peerId);
        }
    }

    public static void SetChildrenLayer(int value, Transform transform, int depth) {
        foreach (Transform child in transform) {
            child.gameObject.layer = value;
            if (depth > 0) {
                SetChildrenLayer(value, child, depth - 1);
            }
        }
    }

    public static void SendDestroyedObjects(List<NetPeer> peers, NetDataWriter writer) {
        writer.Reset();
        writer.Put(GameServer.MESSAGE_OBJECT_DESTROY);
        writer.PutArray(destroyedObjects.ToArray());
        foreach (NetPeer peer in peers) {
            peer.Send(writer, DeliveryMethod.ReliableOrdered);
        }
        destroyedObjects.Clear();
    }

    public static void HandleDestroyMessage(NetDataReader reader, int peerId) {
        foreach (int i in reader.GetIntArray()) {
            if (objDict.ContainsKey(i)) {
                if (objDict[i] != null) {
                    Destroy(objDict[i].gameObject);
                }
                objDict.Remove(i);
                //clientDestroyedObjects.Add(i, true);
            }
        }
    }
}
