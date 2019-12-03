using LiteNetLib.Utils;

class Shared {
    public static string CONNECTION_KEY = "abc123";
    public static int POLL_RATE_MS = 15;

    public static NetPacketProcessor GetNetPacketProcessor() {
        var netPacketProcessor = new NetPacketProcessor();
        // netPacketProcessor.RegisterNestedType<PlayerInputPacket>();
        netPacketProcessor.RegisterNestedType<PlayerSpawnPacket>();
        netPacketProcessor.RegisterNestedType<PlayerState>();
        return netPacketProcessor;
    }
}
