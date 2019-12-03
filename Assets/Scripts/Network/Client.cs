using LiteNetLib;
using LiteNetLib.Utils;
using MoreLinq;
using System;
using UniRx;
using UnityEngine;

class Client : MonoBehaviour {
    private readonly NetPacketProcessor netPacketProcessor = Shared.GetNetPacketProcessor();
    private NetManager client;

    void Awake() {
        var listener = new EventBasedNetListener();
        client = new NetManager(listener);
        client.Start();
        client.Connect("localhost", 9696, Shared.CONNECTION_KEY);
        Debug.Log("Client started.");

        listener.NetworkReceiveObservable()
            .Subscribe(receiveParams => {
                var (fromPeer, dataReader, deliveryMethod) = receiveParams;
                netPacketProcessor.ReadAllPackets(dataReader, fromPeer);
            });
        
        listener.ConnectionRequestObservable()
            .Subscribe(peer => Debug.Log($"Connected to {peer}"));

        Observable.Interval(TimeSpan.FromMilliseconds(Shared.POLL_RATE_MS))
            .Subscribe(_ => client.PollEvents());

        // Susbcribe to packet receives.
        netPacketProcessor.NetSerializableObservable<PlayerSpawnPacket>()
            .Subscribe(ReceivePlayerSpawn);
        
        netPacketProcessor.NetSerializableObservable<PlayerStatePacket>()
            .Subscribe(ReceivePlayerState);
    }

    private void ReceivePlayerSpawn(PlayerSpawnPacket playerSpawn) {
        Debug.Log($"[CLIENT] Player spawned (local: {playerSpawn.local}).");
        PlayerManager.SpawnClient(
            playerSpawn.id,
            playerSpawn.local,
            playerSpawn.pos,
            playerSpawn.team,
            playerSpawn.alias
        );
    }

    private void ReceivePlayerState(PlayerStatePacket playerState) {
        var player = NetworkedEntity.entities[playerState.id].GetComponent<Player>();
        player.SyncState(playerState.pos, playerState.vel, playerState.rot);
    }

    public void SendPlayerInput(int id, Vector2 inputVec) {
        var playerInputPacket = new PlayerInputPacket {
            id = id,
            inputVec = inputVec
        };
        netPacketProcessor.SendNetSerializable(client.FirstPeer, playerInputPacket, DeliveryMethod.ReliableOrdered);
    }
}
