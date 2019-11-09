using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLib;
using LiteNetLib.Utils;

public class GameClient : MonoBehaviour, INetEventListener
{
    private NetManager netClient;

    static GameClient _singleton;
    public static GameClient singleton {
        get{
            if( _singleton == null ){
                _singleton = GameObject.FindObjectOfType<GameClient>();
            }
            return _singleton;
        }
    }
    public static bool Active { get; private set; }
    public static int ownerId = NetworkedObject.NO_OWNER_ID;
    private NetDataWriter _dataWriter;
    private List<NetPeer> peers = new List<NetPeer>();

    float serverSyncTime;
    float clientSyncTime;

    float lastUpdateTime;
    public const float UPDATE_TIME = 0.05f;

    void Start()
    {
        
    }

    void Update()
    {
        
        if( Input.GetKeyDown(KeyCode.P) && netClient == null && !GameServer.Active ){
            netClient = new NetManager(this);
            netClient.Start();
            netClient.UpdateTime = 15;
            Debug.Log("Client starting using localhost");
            netClient.Connect("localhost", GameServer.PORT, GameServer.APPID);
        }
        if( Input.GetKeyDown(KeyCode.M) && netClient == null && !GameServer.Active) {
            netClient = new NetManager(this);
            netClient.Start();
            netClient.UpdateTime = 15;
            Debug.Log("Client starting using IP");
            netClient.Connect("100.42.240.88", GameServer.PORT, GameServer.APPID);
        }
        if( netClient == null ){
            return;
        }
        else {
            Active = true;
        }
        netClient.PollEvents();

        var peer = netClient.FirstPeer;
        if (peer != null && peer.ConnectionState == ConnectionState.Connected) {
            if (Time.time > lastUpdateTime + UPDATE_TIME) {
                _dataWriter = new NetDataWriter();
                NetworkedObject.SendOwnedNetworkedObjects(peers, _dataWriter);
                lastUpdateTime = Time.time;
            }
        }
        else {
            //netClient.SendDiscoveryRequest(new byte[] {1}, GameServer.PORT);
            //netClient.Connect()
        }
    }

    void OnDestroy()
    {
        if (netClient != null)
            netClient.Stop();
    }

    public void OnPeerConnected(NetPeer peer)
    {
        Debug.Log("[CLIENT] We connected to " + peer.EndPoint);
        peers = new List<NetPeer>();
        peers.Add(peer);
    }

    public void OnNetworkError(IPEndPoint endPoint, SocketError socketErrorCode)
    {
        Debug.Log("[CLIENT] We received error " + socketErrorCode);
    }

    public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
    {
        byte messageType = reader.GetByte();
        if (messageType == GameServer.MESSAGE_OBJECT_BATCH ) {
            NetworkedObject.NetworkReadAll(reader);
        }
        else if( messageType == GameServer.MESSAGE_CLIENT_INFO ) {
            ownerId = reader.GetInt();
            serverSyncTime = reader.GetFloat();
            clientSyncTime = Time.time;
            Debug.Log("Set client ID to " + ownerId);
        }
        else if( messageType == GameServer.MESSAGE_OBJECT_DESTROY) {
            NetworkedObject.HandleDestroyMessage(reader, peer.Id);
        }
        else if( messageType == GameServer.MESSAGE_COMPONENT_CUSTOM) {
            NetworkedObject.HandleComponentMessage(reader, peer.Id );
        }
    }

    public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
    {
        if (messageType == UnconnectedMessageType.DiscoveryResponse && netClient.PeersCount == 0)
        {
            Debug.Log("[CLIENT] Received discovery response. Connecting to: " + remoteEndPoint);
            netClient.Connect(remoteEndPoint, "sample_app");
        }
    }

    public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
    {

    }

    public void OnConnectionRequest(ConnectionRequest request)
    {
        
    }

    public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        Debug.Log("[CLIENT] We disconnected because " + disconnectInfo.Reason);
    }
    public static void SendNetworkMessage( NetDataWriter writer) {
        singleton.netClient.SendToAll(writer, DeliveryMethod.ReliableOrdered);
    }
}