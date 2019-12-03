using LiteNetLib;
using LiteNetLib.Utils;
using static LiteNetLib.EventBasedNetListener;
using System;
using UniRx;
using UnityEngine;

public static class Extensions {
    // Vector2
    public static void Put(this NetDataWriter writer, Vector2 vector) {
        writer.Put(vector.x);
        writer.Put(vector.y);
    }

    public static Vector2 GetVector2(this NetDataReader reader) {
        Vector2 v;
        v.x = reader.GetFloat();
        v.y = reader.GetFloat();
        return v;
    }

    // Vector3
    public static void Put(this NetDataWriter writer, Vector3 vector) {
        writer.Put(vector.x);
        writer.Put(vector.y);
        writer.Put(vector.z);
    }

    public static Vector3 GetVector3(this NetDataReader reader) {
        Vector3 v;
        v.x = reader.GetFloat();
        v.y = reader.GetFloat();
        v.z = reader.GetFloat();
        return v;
    }

    // Quaternion
    public static void Put(this NetDataWriter writer, Quaternion quat) {
        writer.Put(quat.x);
        writer.Put(quat.y);
        writer.Put(quat.z);
        writer.Put(quat.w);
    }

    public static Quaternion GetQuaternion(this NetDataReader reader) {
        Quaternion q;
        q.x = reader.GetFloat();
        q.y = reader.GetFloat();
        q.z = reader.GetFloat();
        q.w = reader.GetFloat();
        return q;
    }

    // EventBasedNetListener
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

    // NetPacketProcessor
    public static IObservable<T> NetSerializableObservable<T>(this NetPacketProcessor netPacketProcessor) where T : INetSerializable, new() {
        return Observable.FromEvent<T>(
            h => netPacketProcessor.SubscribeNetSerializable(h),
            h => netPacketProcessor.RemoveSubscription<T>()
        );
    }
}