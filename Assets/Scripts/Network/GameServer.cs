using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLib;
using LiteNetLib.Utils;

public class GameServer : MonoBehaviour, INetEventListener, INetLogger {
    public const string APPID = "sample_app";
    public const int PORT = 65021;
    public const int OBJECT_BATCH_SIZE = 300; // How many objects to send in 1 packet
    // Message types -------------------------------------
    public const byte MESSAGE_OBJECT_BATCH = 0;
    public const byte MESSAGE_OBJECT_DESTROY = 1;
    public const byte MESSAGE_CLIENT_INFO = 2;
    public const byte MESSAGE_CHAT = 3;
    public const byte MESSAGE_COMPONENT_CUSTOM = 4;
    // ---------------------------------------------------
    public GameObject playerPrefab;
    public GameObject[] spawnPoints;
    public GameObject[] prefabs;
    private NetManager netServer;
    private List<NetPeer> peers = new List<NetPeer>();
    private NetDataWriter _dataWriter;
    static GameServer _singleton;
    public static GameServer singleton {
        get {
            if (_singleton == null) {
                _singleton = GameObject.FindObjectOfType<GameServer>();
            }
            return _singleton;
        }
    }
    public static bool Active { get; private set; }

    float lastUpdateTime;
    public const float UPDATE_TIME = 0.05f;

    void Awake() {
        for (short i = 0; i < prefabs.Length; i++) {
            prefabs[i].GetComponent<NetworkedObject>().prefab = i;
        }
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.O) && !GameServer.Active) {
            NetDebug.Logger = this;
            _dataWriter = new NetDataWriter();
            netServer = new NetManager(this);
            netServer.Start(PORT);
            netServer.DiscoveryEnabled = true;
            netServer.UpdateTime = 15;
            Active = true;
            Debug.Log("Server starting");
        }
        if (netServer == null) {
            return;
        }
        netServer.PollEvents();
        if (Time.time > lastUpdateTime + UPDATE_TIME) {
            NetworkedObject.SendNetworkedObjectsDirty(peers, _dataWriter);
            NetworkedObject.SendDestroyedObjects(peers, _dataWriter);
            lastUpdateTime = Time.time;
        }
    }

    void OnDestroy() {
        NetDebug.Logger = null;
        if (netServer != null)
            netServer.Stop();
    }

    public void OnPeerConnected(NetPeer peer) {
        Debug.Log("[SERVER] We have new peer " + peer.EndPoint);
        peers.Add(peer);
        if (playerPrefab) {
            GameObject playerObject = GameObject.Instantiate(playerPrefab);
            playerObject.GetComponent<NetworkedObject>().ownerId = peer.Id;
            if (spawnPoints.Length > 0) {
                playerObject.transform.position = spawnPoints[0].transform.position;
                playerObject.transform.parent = spawnPoints[0].transform.parent;
            }
        }
        _dataWriter = new NetDataWriter();
        _dataWriter.Put(MESSAGE_CLIENT_INFO);
        _dataWriter.Put(peer.Id);
        _dataWriter.Put(Time.time); // Server sync time
        peer.Send(_dataWriter, DeliveryMethod.ReliableOrdered);
        _dataWriter.Reset();
        List<NetPeer> newPeers = new List<NetPeer>();
        newPeers.Add(peer);
        NetworkedObject.SendNetworkedObjects(newPeers, _dataWriter);
    }

    public void OnNetworkError(IPEndPoint endPoint, SocketError socketErrorCode) {
        Debug.Log("[SERVER] error " + socketErrorCode);
    }

    public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader,
        UnconnectedMessageType messageType) {
        if (messageType == UnconnectedMessageType.DiscoveryRequest) {
            Debug.Log("[SERVER] Received discovery request. Send discovery response");
            netServer.SendDiscoveryResponse(new byte[] { 1 }, remoteEndPoint);
        }
    }

    public void OnNetworkLatencyUpdate(NetPeer peer, int latency) {
    }

    public void OnConnectionRequest(ConnectionRequest request) {
        request.AcceptIfKey(APPID);
    }

    public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo) {
        Debug.Log("[SERVER] peer disconnected " + peer.EndPoint + ", info: " + disconnectInfo.Reason);
        if (peers.Contains(peer)) {
            peers.Remove(peer);
        }
    }

    public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod) {
        byte messageType = reader.GetByte();
        if (messageType == MESSAGE_OBJECT_BATCH) {
            NetworkedObject.NetworkReadAll(reader, peer.Id);
        } else if (messageType == MESSAGE_COMPONENT_CUSTOM) {
            NetworkedObject.HandleComponentMessage(reader, peer.Id);
        }
    }

    public void WriteNet(NetLogLevel level, string str, params object[] args) {
        Debug.LogFormat(str, args);
    }

    public static void SendNetworkMessage(NetDataWriter writer) { // For custom component messages
        singleton.netServer.SendToAll(writer, DeliveryMethod.ReliableOrdered);
    }
}
