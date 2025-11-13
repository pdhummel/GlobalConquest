using LiteNetLib;
using LiteNetLib.Utils;
using System.DirectoryServices.ActiveDirectory;
using System.Reflection;
using System.Text.Json;
using GlobalConquest.Units;
using GlobalConquest.Actions;

namespace GlobalConquest;

public class Server
{
    private NetManager? server;

    public Dictionary<NetPeer, string> PeerToPlayerName { get; set; } = new Dictionary<NetPeer, string>();
    public Dictionary<string, NetPeer> PlayerNameToPeer { get; set; } = new Dictionary<string, NetPeer>();

    private EventBasedNetListener? listener;
    private Thread? serverThread;
    private bool isRunning = false;
    private string? key;
    private int maxPeers;
    long lastMilliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
    public GameState gameState { get; set; } = new();
    private bool initialSync = false;
    public GameLogic? GameLogic { get; set; }

    public void StartAsHost(GameSettings gameSettings, string key)
    {
        Console.WriteLine("StartAsHost(): enter");
        this.maxPeers = gameSettings.NumberOfHumans;
        this.key = key;
        gameState.GameSettings = gameSettings;
        Map map = new Map(gameSettings.Height, gameSettings.Width);
        map.addBurbs(gameState.Burbs, gameState.GameSettings.NumberOfBurbs);
        map.VisibilityMode = gameSettings.Visibility;
        gameState.Map = map;
        gameState.placeInitialUnits();
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

    public void RestoreHost(GameSettings gameSettings, string key)
    {
        Console.WriteLine("RestoreHost(): enter");
        this.maxPeers = gameSettings.NumberOfHumans;
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
        GameLogic = new GameLogic();
        GameLogic.server = this;
        GameLogic.startGame(this);

        int sleepTime = 1000;
        Console.WriteLine("ServerLoop(): Server polling");
        // This is the server's polling loop, which runs continuously on its own thread.
        while (isRunning)
        {

            server?.PollEvents();
            if (!initialSync && gameState.PlayerJoined.Count >= gameState.GameSettings.NumberOfHumans)
            {
                syncAllMapHexes();
                initialSync = true;
            }
            Thread.Sleep(sleepTime); // Adjust sleep time to control CPU usage.
        }
    }

    public void syncAllMapHexes()
    {
        for (int liY = 0; liY < gameState.Map.Y; liY++)
        {
            for (int liX = 0; liX < gameState.Map.X; liX++)
            {
                if ("Omniscient".Equals(gameState.GameSettings.Visibility) || "Command HQ".Equals(gameState.GameSettings.Visibility))
                {
                    gameState.Map.Hexes[liY, liX].makeVisibleToAll();
                }
                sendGameStateAndMapHex(liX, liY);
            }
        }
    }

    public void sendGameState()
    {
        if (server != null)
        {
            gameState.updateTicks();
            int count = server.ConnectedPeerList.Count;
            for (int i = 0; i < count; i++)
            {
                if (i <= server.ConnectedPeerList.Count)
                {
                    NetPeer peer = server.ConnectedPeerList[i];
                    sendGameState(peer);
                }
                else
                {
                    Console.WriteLine("sendGameStateAndMapHex(): Count=" + server.ConnectedPeerList.Count + ", i=" + i);
                }

            }
        }
    }


    public void sendGameStateAndMapHex(int x, int y)
    {
        if (server != null)
        {
            gameState.updateTicks();
            int count = server.ConnectedPeerList.Count;
            for (int i = 0; i < count; i++)
            {
                if (i < server.ConnectedPeerList.Count)
                {
                    NetPeer peer = server.ConnectedPeerList[i];
                    sendGameStateAndMapHex(peer, x, y);
                }
                else
                {
                    Console.WriteLine("sendGameStateAndMapHex(): Count=" + server.ConnectedPeerList.Count + ", i=" + i);
                }
            }
        }
    }

    public void sendGameStateAndMapHex(NetPeer peer, int x, int y)
    {
        NetDataWriter writer = new NetDataWriter();
        if (server != null)
        {
            GameEvent gameEvent = new GameEvent();
            gameEvent.EventType = "gameStateAndMapUpdate";
            gameEvent.GameState = gameState;
            gameState.MapHex = gameState.Map.Hexes[y, x];
            string jsonString = JsonSerializer.Serialize(gameEvent);
            writer.Put(jsonString);
            peer.Send(writer, DeliveryMethod.ReliableOrdered);
            writer.Reset();
        }
    }

    public void sendGameStateAndMapHex(string color, int x, int y)
    {
        NetDataWriter writer = new NetDataWriter();
        if (server != null)
        {
            if (gameState.Players.colorToPlayer.ContainsKey(color))
            {
                Player player = gameState.Players.colorToPlayer[color];
                NetPeer peer = PlayerNameToPeer[player.Name];
                sendGameStateAndMapHex(peer, x, y);
            }
            else
            {
                //Console.WriteLine("sendGameStateAndMapHex(): NetPeer not found for " + color);
            }
        }
    }

    public void sendGameState(NetPeer peer)
    {
        NetDataWriter writer = new NetDataWriter();
        if (server != null)
        {
            GameEvent gameEvent = new GameEvent();
            gameEvent.EventType = "gameStateUpdate";
            gameEvent.GameState = gameState;
            gameState.MapHex = null;
            string jsonString = JsonSerializer.Serialize(gameEvent);
            writer.Put(jsonString);
            peer.Send(writer, DeliveryMethod.ReliableOrdered);
            writer.Reset();
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
        Console.WriteLine($"OnConnectionRequest(): Incoming connection request to Server from: {request.RemoteEndPoint}, data=" + request.Data);
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
        PlayerAction subClassAction = action.makeSubclass();
        subClassAction.MessageAsJson = jsonString;
        MethodInfo executeMethod = subClassAction.GetType().GetMethod("deserializeAndExecute");
        object[] parameters = new object[] { peer, this };
        if ("plan".Equals(gameState.CurrentPhase))
        {
            executeMethod?.Invoke(subClassAction, parameters);
        }
        else
        {
            Console.WriteLine("Skipping action, currentPhase=" + gameState.CurrentPhase);
        }


    }

    private void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        Console.WriteLine($"OnPeerDisconnected(): Peer disconnected: {peer.Address} from Server. Reason: {disconnectInfo.Reason}");
        if (PeerToPlayerName.ContainsKey(peer))
        {
            string playerName = PeerToPlayerName[peer];

            Player player = gameState.Players.playerNameToPlayer[playerName];
            Faction faction = gameState.Factions.ColorToFaction[player.FactionColor];
            faction.Status = "disconnected";

            Console.WriteLine("Player " + playerName + " disconnected");
            PeerToPlayerName.Remove(peer);
            PlayerNameToPeer.Remove(playerName);
            gameState.Players.RemovePlayer(gameState, playerName);
            initialSync = false;
            sendGameState();
        }
    }



}
