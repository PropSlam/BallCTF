using UnityEngine;
using LiteNetLib;
using LiteNetLib.Utils;
using UniRx;
using UniRx.Triggers;
using System;
using System.Threading;

class GameServer : MonoBehaviour {

    void Awake() {
        var netPacketProcessor = new NetPacketProcessor();
        netPacketProcessor.RegisterNestedType<PlayerInputPacket>();
        netPacketProcessor.RegisterNestedType<PlayerSpawnPacket>();
        netPacketProcessor.RegisterNestedType<PlayerState>();
        netPacketProcessor.RegisterNestedType<ServerState>();

        var listener = new EventBasedNetListener();
        var server = new NetManager(listener);

        Debug.Log("started");
        server.Start(9696);

        Debug.Log("started");


        listener.ConnectionRequestEvent += request => {
            Debug.Log("....");

            if (server.PeersCount < 10 /* max connections */)
                request.AcceptIfKey("hello");
            else
                request.Reject();
        };

        var netReceive = Observable.FromEvent<EventBasedNetListener.OnNetworkReceive, Tuple<NetPeer, NetPacketReader, DeliveryMethod>>(
            h => (peer, reader, deliveryMethod) => h(Tuple.Create(peer, reader, deliveryMethod)),
            h => listener.NetworkReceiveEvent += h,
            h => listener.NetworkReceiveEvent -= h
        );

        netReceive.Subscribe(asdf => {
            Debug.Log(asdf);
        });

        var connectionRequest = Observable.FromEvent<EventBasedNetListener.OnConnectionRequest, ConnectionRequest>(
            h => (connRequest) => h(connRequest),
            h => listener.ConnectionRequestEvent += h,
            h => listener.ConnectionRequestEvent -= h
        );

        listener.ConnectionRequestEvent += conn => {
            Debug.Log(conn);
        };

        connectionRequest.Subscribe(r => Debug.Log(r));

        NetManager client = new NetManager(listener);
        client.Start();
        client.Connect("localhost", 9696, "hello");

        Debug.Log("go");

        while (!Console.KeyAvailable) {
            server.PollEvents();
            Thread.Sleep(15);
        }
    }
}