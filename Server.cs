using LiteNetLib;
using LiteNetLib.Utils;
using System.Text.Json;

namespace GlobalConquest;

public class Server
{
    private NetManager? server;

    //Dictionary<string, NetPeer> peers = new Dictionary<string, NetPeer>();

    private EventBasedNetListener? listener;
    private Thread? serverThread;
    private bool isRunning = false;
    private string? key;
    private int maxPeers;
    long lastMilliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
    private readonly GameState gameState = new();

    public void StartAsHost(int port, int maxPeers, string key)
    {
        this.maxPeers = maxPeers;
        this.key = key;
        listener = new EventBasedNetListener();

        // Set up event handlers for connection/data
        listener.ConnectionRequestEvent += OnConnectionRequest;
        listener.PeerConnectedEvent += OnPeerConnected;
        listener.NetworkReceiveEvent += OnNetworkReceive;
        listener.PeerDisconnectedEvent += OnPeerDisconnected;

        server = new NetManager(listener)
        {
            UnsyncedEvents = true
        };

        // Start the server manager
        server.Start(port);
        isRunning = true;

        // Create and start the new thread for the server's polling loop
        serverThread = new Thread(new ThreadStart(ServerLoop))
        {
            IsBackground = true // Ensures thread closes with the main app
        };
        serverThread.Start();
    }

    private void ServerLoop()
    {
        Console.WriteLine("ServerLoop(): Server polling");
        // This is the server's polling loop, which runs continuously on its own thread.
        while (isRunning)
        {
            server?.PollEvents();
            Thread.Sleep(15); // Adjust sleep time to control CPU usage.

            NetDataWriter writer = new NetDataWriter();
            string jsonString = JsonSerializer.Serialize(this.gameState);
            writer.Put(jsonString);
            if (server != null)
            {
                foreach (NetPeer peer in server.ConnectedPeerList)
                {
                    peer.Send(writer, DeliveryMethod.ReliableOrdered);
                }
            }
        }
    }



    private void StopServer()
    {
        isRunning = false;
        if (serverThread != null && serverThread.IsAlive)
        {
            serverThread.Join(); // Wait for the server thread to finish gracefully
        }
        server?.Stop();
    }

    private void Update()
    {
        server?.PollEvents();
    }

    private void OnDestroy()
    {
        server?.Stop();
    }

    // --- LiteNetLib Event Handlers ---
    private void OnConnectionRequest(ConnectionRequest request)
    {
        Console.WriteLine($"OnConnectionRequest(): Incoming connection request to Server from: {request.RemoteEndPoint}");
        // In a real application, you would add validation here.
        if (server?.ConnectedPeersCount < maxPeers)
        {
            request.AcceptIfKey(this.key);
            Console.WriteLine("OnConnectionRequest(): connection accepted by Server");
        }
        else
        {
            request.Reject();
            Console.WriteLine("OnConnectionRequest(): connection rejected by Server");
        }
    }

    private void OnPeerConnected(NetPeer peer)
    {
        Console.WriteLine($"OnPeerConnected(): Peer connected to Server: {peer.Address}");
    }

    private void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
    {
        var jsonString = reader.GetString();
        reader.Recycle(); // Free up the data reader
        PlayerAction? action =
                JsonSerializer.Deserialize<PlayerAction>(jsonString);
    }

    private void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        Console.WriteLine($"OnPeerDisconnected(): Peer disconnected: {peer.Address} from Server. Reason: {disconnectInfo.Reason}");
    }

}


