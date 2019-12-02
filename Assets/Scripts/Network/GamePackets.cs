using LiteNetLib.Utils;
using UnityEngine;

public struct PlayerInputPacket : INetSerializable {
    public int id;
    public Vector2 inputVec;

    public void Serialize(NetDataWriter writer) {
        writer.Put(id);
        writer.Put(inputVec);
    }

    public void Deserialize(NetDataReader reader) {
        id = reader.GetInt();
        inputVec = reader.GetVector2();
    }
}

public struct PlayerSpawnPacket : INetSerializable {
    public int id;
    public Vector3 pos;
    public Team team;
    public string alias;

    public void Serialize(NetDataWriter writer) {
        writer.Put(id);
        writer.Put(pos);
        writer.Put((byte)team);
        writer.Put(alias);
    }

    public void Deserialize(NetDataReader reader) {
        id = reader.GetInt();
        pos = reader.GetVector3();
        team = (Team)reader.GetByte();
        alias = reader.GetString();
    }
}

public struct PlayerState : INetSerializable {
    public int id;
    public Vector3 pos;
    public Vector3 vel;
    public Quaternion rot;

    public const int Size = sizeof(int) +
        sizeof(float) * 3 +
        sizeof(float) * 3 +
        sizeof(float) * 4;

    public void Serialize(NetDataWriter writer) {
        writer.Put(id);
        writer.Put(pos);
        writer.Put(vel);
        writer.Put(rot);
    }

    public void Deserialize(NetDataReader reader) {
        id = reader.GetByte();
        pos = reader.GetVector3();
        vel = reader.GetVector3();
        rot = reader.GetQuaternion();
    }
}

public struct ServerState : INetSerializable {
    public PlayerState[] playerStates;

    public void Serialize(NetDataWriter writer) {
        foreach (var state in playerStates)
            state.Serialize(writer);
    }

    public void Deserialize(NetDataReader reader) {
        playerStates = new PlayerState[reader.AvailableBytes / PlayerState.Size];
        for (int i = 0; i < playerStates.Length; i++)
            playerStates[i].Deserialize(reader);
    }
}
