using LiteNetLib;
using LiteNetLib.Utils;
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
        Globals.Log("StartAsHost(): enter");
        this.maxPeers = 8; // gameSettings.NumberOfHumans;
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
        Globals.Log("RestoreHost(): enter");
        this.maxPeers = 8; //gameSettings.NumberOfHumans;
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
        Globals.Log("ServerLoop(): Server polling");
        // This is the server's polling loop, which runs continuously on its own thread.
        while (isRunning)
        {

            server?.PollEvents();
            if (!initialSync && gameState.PlayerJoined.Count >= gameState.GameSettings.NumberOfHumans)
            {
                Globals.Log("ServerLoop(): all clients joined");
                syncAllMapHexes();
                initialSync = true;
            }
            Thread.Sleep(sleepTime); // Adjust sleep time to control CPU usage.
        }
    }

    public void syncAllMapHexes()
    {
        Globals.Log("syncAllMapHexes(): enter");
        for (int liY = 0; liY < gameState.Map.Y; liY++)
        {
            for (int liX = 0; liX < gameState.Map.X; liX++)
            {
                if ("Omniscient".Equals(gameState.GameSettings.Visibility) || "Command HQ".Equals(gameState.GameSettings.Visibility))
                {
                    gameState.Map.Hexes[liY, liX].makeVisibleToAll();
                }
            }
        }
        sendGameState();
        sendMap(null);
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
                    Globals.Log("sendGameStateAndMapHex(): Count=" + server.ConnectedPeerList.Count + ", i=" + i);
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
                    Globals.Log("sendGameStateAndMapHex(): Count=" + server.ConnectedPeerList.Count + ", i=" + i);
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
            gameEvent.MapHex = gameState.Map.Hexes[y, x];
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
                //Globals.Log("sendGameStateAndMapHex(): NetPeer not found for " + color);
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

    public void sendMap(NetPeer? peer)
    {
        Globals.Log("sendMap(): peer=" + peer);
        List<MapHex> mapHexBuffer = new List<MapHex>();
        Map map = gameState.Map;
        int bufferSize = 50;
        for (int y = 0; y < map.Y; y++)
        {
            for (int x = 0; x < map.X; x++)
            {
                mapHexBuffer.Add(map.Hexes[y, x]);
                if (mapHexBuffer.Count >= bufferSize)
                {
                    if (peer != null)
                    {
                        sendMapBuffer(peer, mapHexBuffer, false);
                    }
                    else
                    {
                        sendMapBuffer(mapHexBuffer, false);
                    }
                    mapHexBuffer.Clear();
                }
            }
        }
        if (mapHexBuffer.Count <= 0)
            mapHexBuffer.Add(map.Hexes[0, 0]);
        if (mapHexBuffer.Count > 0)
        {
            if (peer != null)
            {
                sendMapBuffer(peer, mapHexBuffer, true);
            }
            else
            {
                sendMapBuffer(mapHexBuffer, true);
            }
            mapHexBuffer.Clear();
        }
    }

    public void sendMapBuffer(List<MapHex> mapHexBuffer, bool isLast)
    {
        //Globals.Log("sendMapBuffer(): mapHexBuffer=" + mapHexBuffer.Count);
        int count = server.ConnectedPeerList.Count;
        for (int i = 0; i < count; i++)
        {
            if (i < server.ConnectedPeerList.Count)
            {
                NetPeer peer = server.ConnectedPeerList[i];
                sendMapBuffer(peer, mapHexBuffer, isLast);
            }
            else
            {
                //Globals.Log("sendMapBuffer(): Count=" + server.ConnectedPeerList.Count + ", i=" + i);
            }
        }
    }

    public void sendMapBuffer(NetPeer peer, List<MapHex> mapHexBuffer, bool isLast)
    {
        Globals.Log("sendMapBuffer(): peer=" + peer + ", mapHexBuffer=" + mapHexBuffer.Count);
        NetDataWriter writer = new NetDataWriter();
        if (server != null)
        {
            GameEvent gameEvent = new GameEvent();
            gameEvent.EventType = "mapUpdate";
            gameEvent.MapHexBuffer = mapHexBuffer;
            gameEvent.GameState = null;
            gameEvent.IsLastMapHexBufferUpdate = isLast;
            string jsonString = JsonSerializer.Serialize(gameEvent);
            writer.Put(jsonString);
            peer.Send(writer, DeliveryMethod.ReliableOrdered);
            writer.Reset();
        }
    }

    public void sendGamePlayEvent(GameEvent gameEvent)
    {
        if (server != null)
        {
            int count = server.ConnectedPeerList.Count;
            for (int i = 0; i < count; i++)
            {
                if (i < server.ConnectedPeerList.Count)
                {
                    NetPeer peer = server.ConnectedPeerList[i];
                    sendGamePlayEvent(peer, gameEvent);
                }
                else
                {
                    Globals.Log("sendGamePlayEvent(): Count=" + server.ConnectedPeerList.Count + ", i=" + i);
                }
            }
        }

    }


    public void sendGamePlayEvent(string color, GameEvent gameEvent)
    {
        NetDataWriter writer = new NetDataWriter();
        if (server != null)
        {
            if (gameState.Players.colorToPlayer.ContainsKey(color))
            {
                Player player = gameState.Players.colorToPlayer[color];
                NetPeer peer = PlayerNameToPeer[player.Name];
                sendGamePlayEvent(peer, gameEvent);
            }
            else
            {
                //Globals.Log("sendGamePlayEvent(): NetPeer not found for " + color);
            }
        }        
    }

    public void sendGamePlayEvent(NetPeer peer, GameEvent gameEvent)
    {
        NetDataWriter writer = new NetDataWriter();
        if (server != null)
        {
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
        Globals.Log($"OnConnectionRequest(): Incoming connection request to Server from: {request.RemoteEndPoint}, data=" + request.Data);
        // In a real application, you would add validation here.
        if (server?.ConnectedPeersCount < maxPeers)
        {
            request.AcceptIfKey(this.key);
            Globals.Log("OnConnectionRequest(): connection accepted by Server");
        }
        else
        {
            request.Reject();
            Globals.Log("OnConnectionRequest(): connection rejected by Server b/c limit to connected peers. (player count)");
        }
    }

    private void OnPeerConnected(NetPeer peer)
    {
        Globals.Log($"OnPeerConnected(): Peer connected to Server: {peer.Address}");
    }

    private void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
    {
        var jsonString = reader.GetString();
        Globals.Log("Server.OnNetworkReceive(): " + jsonString);
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
            Globals.Log("OnNetworkReceive(): invoked method for " + subClassAction.GetType());
        }
        else
        {
            Globals.Log("OnNetworkReceive(): Skipping action, currentPhase=" + gameState.CurrentPhase);
        }

    }

    private void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        Globals.Log($"OnPeerDisconnected(): Peer disconnected: {peer.Address} from Server. Reason: {disconnectInfo.Reason}");
        if (PeerToPlayerName.ContainsKey(peer))
        {
            string playerName = PeerToPlayerName[peer];
            if (gameState.Players.playerNameToPlayer.ContainsKey(playerName))
            {
                Player player = gameState.Players.playerNameToPlayer[playerName];
                Faction faction = gameState.Factions.ColorToFaction[player.FactionColor];
                faction.Status = "disconnected";
                Globals.Log("Player " + playerName + " disconnected");
            }
            else
            {
                Globals.Log(playerName + " disconnected");
            }
            if (PeerToPlayerName.ContainsKey(peer))
                PeerToPlayerName.Remove(peer);
            if (PlayerNameToPeer.ContainsKey(playerName))
                PlayerNameToPeer.Remove(playerName);
            gameState.Players.RemovePlayer(gameState, playerName);
            initialSync = false;
            sendGameState();
        }
    }



}
