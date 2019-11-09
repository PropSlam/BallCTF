using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLib;
using LiteNetLib.Utils;

[RequireComponent(typeof(NetworkedObject))]
public class NetworkedComponent : MonoBehaviour {
    NetworkedObject _networkedObject;
    public NetworkedObject networkedObject {
        get {
            if (!_networkedObject) {
                _networkedObject = GetComponent<NetworkedObject>();
            }
            return _networkedObject;
        }
    }
    bool _dirty = true;
    public bool dirty {
        get {
            return _dirty;
        }
        set {
            _dirty = value;
            if (value) {
                networkedObject.SetDirty();
            }
        }
    }

    protected NetDataWriter GetWriter() {
        return networkedObject.GetComponentWriter(this);
    }

    protected void SendNetworkMessage(NetDataWriter writer) {
        if (GameServer.Active) {
            GameServer.SendNetworkMessage(writer);
        } else if (GameClient.Active) {
            GameClient.SendNetworkMessage(writer);
        }
    }

    public void PrepareSerialize(NetDataWriter writer, bool forceSerialize = false) {
        writer.Put(dirty || forceSerialize);
        if (dirty || forceSerialize) {
            Serialize(writer);
            if (!forceSerialize) {
                dirty = false;
            }
        }
    }

    public void PrepareDeserialize(NetDataReader reader) {
        if (reader.GetBool()) {
            Deserialize(reader);
        }
    }

    public virtual void HandleMessage(NetDataReader reader, int peerId) { }

    public virtual void Serialize(NetDataWriter writer) { }

    public virtual void Deserialize(NetDataReader reader) { }
}
