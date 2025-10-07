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

    public GlobalConquestGame GlobalConquestGame { get; set; }

    public bool isLoadContentComplete { get; set; } = false;

    public GameState GameState { get; set; } = new GameState();


    public void Connect(JoinGameValues joinGameValues, string key)
    {
        ClientIdentifier = joinGameValues.Name;
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
        string host = joinGameValues.HostIp;
        int port = joinGameValues.Port;
        serverPeer = client.Connect(host, port, key); // Use the same key as the server
        Console.WriteLine($"Connect(): Client attempting to connect to {host}:{port}");
        // Create and start the new thread for the client's polling loop
        clientThread = new Thread(new ThreadStart(ClientLoop))
        {
            IsBackground = true // Ensures thread closes with the main app
        };
        clientThread.Start();
        PlayerAction action = new(ClientIdentifier, "connect");
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
        Console.WriteLine("SendData(): " + peerIdentifier + "Client sent data " + data);
    }


    // --- LiteNetLib Event Handlers ---
    private void OnPeerConnected(NetPeer peer)
    {
        Console.WriteLine($"OnPeerConnected(): Client peer connected: {peer.Address}");
    }

    private void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
    {
        var jsonString = reader.GetString();
        //Console.WriteLine($"OnNetworkReceive(): Client [Received] from {peer.Address}: {jsonString}");
        GameState oldGameState = GameState;
        GameState? newGameState = JsonSerializer.Deserialize<GameState>(jsonString);
        if (newGameState != null)
        {
            if (newGameState.Map == null)
            {
                newGameState.Map = new Map();
                newGameState.Map.Y = newGameState.GameSettings.Height;
                newGameState.Map.X = newGameState.GameSettings.Width;

            }
            if (newGameState.Map.Hexes == null)
            {
                newGameState.Map.Y = newGameState.GameSettings.Height;
                newGameState.Map.X = newGameState.GameSettings.Width;
                newGameState.Map.Hexes = new MapHex[newGameState.GameSettings.Height, newGameState.GameSettings.Width];
                for (int liY = 0; liY < newGameState.GameSettings.Height; liY++)
                {
                    for (int liX = 0; liX < newGameState.GameSettings.Width; liX++)
                    {
                        if (newGameState.MapHex.Y.Equals(liY) && newGameState.MapHex.X.Equals(liX))
                        {
                            newGameState.Map.Hexes[newGameState.MapHex.Y, newGameState.MapHex.X] = newGameState.MapHex;
                        }
                        else if (oldGameState.Map != null && oldGameState.Map.Hexes != null && oldGameState.Map.Hexes[liY, liX] != null)
                        {
                            newGameState.Map.Hexes[liY, liX] = oldGameState.Map.Hexes[liY, liX];
                        }
                        else
                        {
                            MapHex mapHex = new MapHex();
                            mapHex.Y = liY;
                            mapHex.X = liX;
                            // TODO: unknnown terrain
                            // mapHex.Terrain = "unknown";
                            mapHex.Terrain = "sea";
                            newGameState.Map.Hexes[liY, liX] = mapHex;
                        }
                    }
                }
            }
            newGameState.Map.Hexes[newGameState.MapHex.Y, newGameState.MapHex.X] = newGameState.MapHex;
            GameState = newGameState;
            if (!isLoadContentComplete)
            {
                GlobalConquestGame.HexMapLoadContent();
                isLoadContentComplete = true;
            }
            GlobalConquestGame.updateMap();
        }
        reader.Recycle(); // Free up the data reader
    }

    private void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        Console.WriteLine($"OnPeerDisconnected(): Client peer disconnected: {peer.Address}. Reason: {disconnectInfo.Reason}");
    }

}