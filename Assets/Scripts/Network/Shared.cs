using LiteNetLib;
using LiteNetLib.Utils;
using static LiteNetLib.EventBasedNetListener;
using System;
using UniRx;

static class EventBasedNetListenerRx {
    public static IObservable<ConnectionRequest> ConnectionRequestObservable(this EventBasedNetListener listener) {
        return Observable.FromEvent<OnConnectionRequest, ConnectionRequest>(
            h => (connRequest) => h(connRequest),
            h => listener.ConnectionRequestEvent += h,
            h => listener.ConnectionRequestEvent -= h
        );
    }

    public static IObservable<Tuple<NetPeer, NetPacketReader, DeliveryMethod>> NetworkReceiveObservable(this EventBasedNetListener listener) {
        return Observable.FromEvent<EventBasedNetListener.OnNetworkReceive, Tuple<NetPeer, NetPacketReader, DeliveryMethod>>(
            h => (peer, reader, deliveryMethod) => h(Tuple.Create(peer, reader, deliveryMethod)),
            h => listener.NetworkReceiveEvent += h,
            h => listener.NetworkReceiveEvent -= h
        );
    }
}

static class NetPacketProcessorRx {
    public static IObservable<T> NetSerializableObservable<T>(this NetPacketProcessor netPacketProcessor) where T : INetSerializable, new() {
        return Observable.FromEvent<T>(
            h => netPacketProcessor.SubscribeNetSerializable(h),
            h => netPacketProcessor.RemoveSubscription<T>()
        );
    }
}

class Shared {
    public static string CONNECTION_KEY = "abc123";
    public static int POLL_RATE_MS = 15;

    public static NetPacketProcessor GetNetPacketProcessor() {
        var netPacketProcessor = new NetPacketProcessor();
        netPacketProcessor.RegisterNestedType<PlayerInputPacket>();
        netPacketProcessor.RegisterNestedType<PlayerSpawnPacket>();
        netPacketProcessor.RegisterNestedType<PlayerStatePacket>();
        return netPacketProcessor;
    }
}
