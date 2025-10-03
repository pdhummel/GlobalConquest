using LiteNetLib;
using LiteNetLib.Utils;
using System.Text.Json;

namespace GlobalConquest;

public class Client
{
    private NetManager? client;
    private EventBasedNetListener? listener;
    private Thread? clientThread;
    public string? ClientIdentifier { get; set; }
    private NetPeer? serverPeer;

    public GameState GameState { get; set; } = new GameState();


    public void Connect(string host, int port, string key, string clientIdentifier)
    {
        ClientIdentifier = clientIdentifier;
        listener = new EventBasedNetListener();
        listener.PeerConnectedEvent += OnPeerConnected;
        listener.NetworkReceiveEvent += OnNetworkReceive;
        listener.PeerDisconnectedEvent += OnPeerDisconnected;

        client = new NetManager(listener)
        {
            UnconnectedMessagesEnabled = true,
            UnsyncedEvents = true
        };
        client.Start();
        serverPeer = client.Connect(host, port, key); // Use the same key as the server
        Console.WriteLine($"Connect(): Client attempting to connect to {host}:{port}");
        // Create and start the new thread for the client's polling loop
        clientThread = new Thread(new ThreadStart(ClientLoop))
        {
            IsBackground = true // Ensures thread closes with the main app
        };
        clientThread.Start();
        PlayerAction action = new(clientIdentifier, "connect");
    }


    private void OnDestroy()
    {
        client?.Stop();
    }

    private void ClientLoop()
    {
        Console.WriteLine("ClientLoop(): Client polling");
        // This is the client's polling loop, which runs continuously on its own thread.
        while (true)
        {
            client?.PollEvents();
            Thread.Sleep(15); // Adjust sleep time to control CPU usage.
        }
    }

    public void Stop()
    {
        client?.Stop();
        Console.WriteLine("Stop(): Client stopped.");
    }

    public void SendData(string peerIdentifier, string data)
    {
        NetDataWriter writer = new();
        writer.Put(data); // Add your data
        serverPeer?.Send(writer, DeliveryMethod.ReliableOrdered);
        //Console.WriteLine("SendData(): " + peerIdentifier + "Client sent data " + data);
    }


    // --- LiteNetLib Event Handlers ---
    private void OnPeerConnected(NetPeer peer)
    {
        Console.WriteLine($"OnPeerConnected(): Client peer connected: {peer.Address}");
    }

    private void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
    {
        //Console.WriteLine($"OnNetworkReceive(): Client [Received] from {peer.EndPoint}: {message}");
        var jsonString = reader.GetString();
        GameState? newGameState = JsonSerializer.Deserialize<GameState>(jsonString);
        if (newGameState != null)
            GameState = newGameState;
        reader.Recycle(); // Free up the data reader
    }

    private void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        Console.WriteLine($"OnPeerDisconnected(): Client peer disconnected: {peer.Address}. Reason: {disconnectInfo.Reason}");
    }

}