using LiteNetLib.Utils;
using UnityEngine;
using System.Runtime.InteropServices;

public struct PlayerSpawnPacket : INetSerializable {
    public int id;
    public bool local;
    public Vector3 pos;
    public Team team;
    public string alias;

    public void Serialize(NetDataWriter writer) {
        writer.Put(id);
        writer.Put(local);
        writer.Put(pos);
        writer.Put((byte)team);
        writer.Put(alias);
    }

    public void Deserialize(NetDataReader reader) {
        id = reader.GetInt();
        local = reader.GetBool();
        pos = reader.GetVector3();
        team = (Team)reader.GetByte();
        alias = reader.GetString();
    }

    public static PlayerSpawnPacket FromPlayer(Player player, bool local) {
        return new PlayerSpawnPacket {
            id = player.GetComponent<NetworkedEntity>().Id,
            local = local,
            pos = player.transform.position,
            team = player.team.Value,
            alias = player.alias.Value
        };
    }
}

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

public struct PlayerStatePacket : INetSerializable {
    public int id;
    public Vector3 pos;
    public Vector3 vel;
    public Quaternion rot;

    public void Serialize(NetDataWriter writer) {
        writer.Put(id);
        writer.Put(pos);
        writer.Put(vel);
        writer.Put(rot);
    }

    public void Deserialize(NetDataReader reader) {
        id = reader.GetInt();
        pos = reader.GetVector3();
        vel = reader.GetVector3();
        rot = reader.GetQuaternion();
    }
}
