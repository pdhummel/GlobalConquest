using LiteNetLib;
using LiteNetLib.Utils;
using System.DirectoryServices.ActiveDirectory;
using System.Reflection;
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

    public void StartAsHost(GameSettings gameSettings, string key)
    {
        this.maxPeers = gameSettings.NumberOfHumans;
        this.key = key;
        gameState.GameSettings = gameSettings;
        Map map = new Map(gameSettings.Height, gameSettings.Width);
        gameState.Map = map;
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
        server.Start(gameSettings.Port);
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
            Thread.Sleep(1000); // Adjust sleep time to control CPU usage.

            NetDataWriter writer = new NetDataWriter();
            if (server != null)
            {
                foreach (NetPeer peer in server.ConnectedPeerList)
                {
                    for (int liY = 0; liY < gameState.Map.Y; liY++)
                    {
                        for (int liX = 0; liX < gameState.Map.X; liX++)
                        {
                            gameState.MapHex = gameState.Map.Hexes[liY, liX];
                            //Console.WriteLine("ServerLoop(): hex=" + gameState.MapHex.Terrain);
                            string jsonString = JsonSerializer.Serialize(this.gameState);
                            //Console.WriteLine("ServerLoop(): jsonString=" + jsonString);
                            writer.Put(jsonString);
                            peer.Send(writer, DeliveryMethod.ReliableOrdered);
                            writer.Reset();
                        }
                    }
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
        //Type type = Type.GetType(action.ClassType);
        //dynamic subClassAction = Convert.ChangeType(action, type);
        PlayerAction subClassAction = action.makeSubclass();
        subClassAction.MessageAsJson = jsonString;
        MethodInfo executeMethod = subClassAction.GetType().GetMethod("deserializeAndExecute");
        object[] parameters = new object[] { gameState };
        executeMethod?.Invoke(subClassAction, parameters);

    }

    private void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        Console.WriteLine($"OnPeerDisconnected(): Peer disconnected: {peer.Address} from Server. Reason: {disconnectInfo.Reason}");
    }

}


