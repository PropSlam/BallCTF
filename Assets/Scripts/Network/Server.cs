using System;
using System.Collections.Generic;
using System.Linq;
using LiteNetLib;
using LiteNetLib.Utils;
using MoreLinq;
using UniRx;
using UnityEngine;

class Server : MonoBehaviour {
    public static int MAX_PLAYERS = 16;

    private NetManager server;
    private EventBasedNetListener listener;
    private readonly NetPacketProcessor netPacketProcessor = Shared.GetNetPacketProcessor();
    private List<NetPeer> peers = new List<NetPeer>();

    void Awake() {
        listener = new EventBasedNetListener();
        server = new NetManager(listener);

        server.Start(9696);
        Debug.Log("Started server.");

        listener.ConnectionRequestObservable()
            .Where(_ => peers.Count < MAX_PLAYERS)
            .Select(request => request.AcceptIfKey(Shared.CONNECTION_KEY))
            .Where(peer => peer != null)
            .Subscribe(HandlePlayerConnected);

        listener.NetworkReceiveObservable()
            .Subscribe(receiveParams => {
                var(fromPeer, dataReader, deliveryMethod) = receiveParams;
                netPacketProcessor.ReadAllPackets(dataReader);
            });

        Observable.Interval(TimeSpan.FromMilliseconds(Shared.POLL_RATE_MS))
            .Subscribe(_ => server.PollEvents());

        // Periodically send server state syncs.
        Observable.Interval(TimeSpan.FromMilliseconds(100))
            .Subscribe(_ => SendServerState());

        // Susbcribe to packet receives.
        netPacketProcessor.NetSerializableObservable<PlayerInputPacket>()
            .Subscribe(ReceivePlayerInput);
    }

    private void HandlePlayerConnected(NetPeer newPeer) {
        // Send PlayerSpawn packet for all existing players to new peer.
        PlayerManager.players.ForEach(existingPlayer => {
            var playerSpawnPacket = PlayerSpawnPacket.FromPlayer(existingPlayer, false);
            netPacketProcessor.SendNetSerializable(newPeer, playerSpawnPacket, DeliveryMethod.ReliableOrdered);
        });

        peers.Add(newPeer);

        // Assign team.
        var team = (Team)((peers.Count - 1) % 2);

        // Find spawnpoint.
        var spawnpoint = FindObjectsOfType<Spawnpoint>()
            .Where(sp => sp.team == team)
            .Shuffle()
            .First();

        // Spawn player on server.
        var player = PlayerManager.SpawnServer(spawnpoint.transform.position, team);
        Debug.Log($"[SERVER] Player connected: #{player.GetComponent<NetworkedEntity>().Id}");

        // Handle player leave.
        listener.PeerDisconnectedObservable()
            .Where(disconnectedParams => disconnectedParams.Item1 == newPeer)
            .Subscribe(_ => HandlePlayerLeave(player, newPeer));

        // Send PlayerSpawn packet for new peer to all connected peers.
        peers.ForEach(peer => {
            var playerSpawnPacket = PlayerSpawnPacket.FromPlayer(player, peer == newPeer);
            netPacketProcessor.SendNetSerializable(peer, playerSpawnPacket, DeliveryMethod.ReliableOrdered);
        });
    }

    private void HandlePlayerLeave(Player player, NetPeer playerPeer) {
        var playerId = player.GetComponent<NetworkedEntity>().Id;
        Debug.Log($"[SERVER] Player left: #{playerId}");
        var playerLeavePacket = new PlayerLeavePacket { id = playerId };
        peers.Remove(playerPeer);
        peers.ForEach(peer => netPacketProcessor.Send(peer, playerLeavePacket, DeliveryMethod.ReliableOrdered));
        PlayerManager.HandlePlayerLeave(player);
    }

    private void ReceivePlayerInput(PlayerInputPacket playerInput) {
        Debug.Log($"[SERVER] Got input from #{playerInput.id}: {playerInput.inputVec}");
        NetworkedEntity.entities[playerInput.id].GetComponent<Player>().inputVector.Value = playerInput.inputVec;
    }

    private void SendServerState() {
        var playerStates = PlayerManager.players.Select(player => new PlayerState {
            id = player.GetComponent<NetworkedEntity>().Id,
            pos = player.rigidbody.position,
            vel = player.rigidbody.velocity,
            rot = player.rigidbody.rotation
        }).ToArray();

        var serverStatePacket = new ServerStatePacket {
            playerStates = playerStates
        };

        peers.ForEach(peer => netPacketProcessor.SendNetSerializable(
            peer, serverStatePacket, DeliveryMethod.ReliableOrdered));
    }
}
