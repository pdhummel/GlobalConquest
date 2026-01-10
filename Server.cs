using LiteNetLib;
using LiteNetLib.Utils;
using System.Reflection;
using System.Text.Json;
using GlobalConquest.Units;
using GlobalConquest.Actions;
using static GameConstants;

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
    Random random = new Random();
    int lastQueueSize = 0;

    public void StartAsHost(GameSettings gameSettings, string key)
    {
        Globals.Log("StartAsHost(): enter");
        this.maxPeers = 8; // gameSettings.NumberOfHumans;
        this.key = key;
        gameState.GameSettings = gameSettings;
        Map map = new Map(gameSettings.Height, gameSettings.Width, gameSettings.NumberOfIslands);
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
                sendGamePlayEvent(new GameEvent("joinedGame"));
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
                if (VISIBILITY_OMNISCIENT.Equals(gameState.GameSettings.Visibility) || "Command HQ".Equals(gameState.GameSettings.Visibility))
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
                    try
                    {
                        NetPeer peer = server.ConnectedPeerList[i];
                        sendGameState(peer);
                    }
                    catch (Exception ex)
                    {
                        Globals.Log("sendGameState(): Exception:" + ex +
                        ", Count=" + server.ConnectedPeerList.Count + ", i=" + i);
                    }
                }
                else
                {
                    Globals.Log("sendGameState(): Count=" + server.ConnectedPeerList.Count + ", i=" + i);
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
                    try
                    {
                        NetPeer peer = server.ConnectedPeerList[i];
                        sendGameStateAndMapHex(peer, x, y);
                    }
                    catch (Exception ex)
                    {
                        Globals.Log("sendGameStateAndMapHex(): Exception:" + ex +
                        ", Count=" + server.ConnectedPeerList.Count + ", i=" + i);
                    }
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
        GameEvent gameEvent = new GameEvent();
        gameEvent.EventType = EVENT_TYPE_GAME_STATE_AND_MAP_UPDATE;
        gameEvent.GameState = gameState;
        gameEvent.MapHex = gameState.Map.Hexes[y, x];
        gameState.MapHex = gameState.Map.Hexes[y, x];
        string jsonString = JsonSerializer.Serialize(gameEvent);
        sendJsonString(peer, jsonString);
    }

    public void sendGameStateAndMapHex(string color, int x, int y)
    {
        if (server != null)
        {
            if (gameState.Players.colorToPlayer.ContainsKey(color))
            {
                if (gameState.Players.colorToPlayer.ContainsKey(color))
                {
                    Player player = gameState.Players.colorToPlayer[color];
                    if (PlayerNameToPeer.ContainsKey(player.Name))
                    {
                        NetPeer peer = PlayerNameToPeer[player.Name];
                        sendGameStateAndMapHex(peer, x, y);
                    }
                }
            }
            else
            {
                //Globals.Log("sendGameStateAndMapHex(): NetPeer not found for " + color);
            }
        }
    }


    public void sendGameState(NetPeer peer)
    {
        GameEvent gameEvent = new GameEvent();
        gameEvent.EventType = EVENT_TYPE_GAME_STATE_UPDATE;
        gameEvent.GameState = gameState;
        gameState.MapHex = null;
        string jsonString = JsonSerializer.Serialize(gameEvent);
        sendJsonString(peer, jsonString);
    }

    public void sendMap(NetPeer? peer)
    {
        Globals.Log("sendMap(): peer=" + peer);
        List<MapHex> mapHexBuffer = new List<MapHex>();
        Map map = gameState.Map;
        // 200 - Server sendMapBuffer(): Exception:System.OverflowException: Arithmetic operation resulted in an overflow.
        int bufferSize = 175;
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
        Globals.Log("sendMap(): hexes=" + map.Hexes.GetLength(0) + "," + map.Hexes.GetLength(1));
    }

    public void sendMapBuffer(List<MapHex> mapHexBuffer, bool isLast)
    {
        //Globals.Log("sendMapBuffer(): mapHexBuffer=" + mapHexBuffer.Count);
        int count = server.ConnectedPeerList.Count;
        for (int i = 0; i < count; i++)
        {
            if (i < server.ConnectedPeerList.Count)
            {
                try
                {
                    NetPeer peer = server.ConnectedPeerList[i];
                    sendMapBuffer(peer, mapHexBuffer, isLast);
                }
                catch (Exception ex)
                {
                    Globals.Log("sendMapBuffer(): Exception:" + ex +
                    ", Count=" + server.ConnectedPeerList.Count + ", i=" + i);
                }
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
        GameEvent gameEvent = new GameEvent();
        gameEvent.EventType = EVENT_TYPE_MAP_UPDATE;
        gameEvent.MapHexBuffer = mapHexBuffer;
        gameEvent.GameState = null;
        gameEvent.IsLastMapHexBufferUpdate = isLast;
        string jsonString = JsonSerializer.Serialize(gameEvent);
        sendJsonString(peer, jsonString);
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
                    try
                    {
                        NetPeer peer = server.ConnectedPeerList[i];
                        sendGamePlayEvent(peer, gameEvent);
                    }
                    catch (Exception ex)
                    {
                        Globals.Log("sendGamePlayEvent(): Exception:" + ex +
                        ", Count=" + server.ConnectedPeerList.Count + ", i=" + i);
                    }
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
        if (server != null)
        {
            if (gameState.Players.colorToPlayer.ContainsKey(color))
            {
                Player player = gameState.Players.colorToPlayer[color];
                if (PlayerNameToPeer.ContainsKey(player.Name))
                {
                    NetPeer peer = PlayerNameToPeer[player.Name];
                    sendGamePlayEvent(peer, gameEvent);
                }
            }
            else
            {
                //Globals.Log("sendGamePlayEvent(): NetPeer not found for " + color);
            }
        }
    }

    public void sendGamePlayEvent(NetPeer peer, GameEvent gameEvent)
    {
        string jsonString = JsonSerializer.Serialize(gameEvent);
        sendJsonString(peer, jsonString);
    }

    public void sendJsonString(NetPeer peer, String jsonString)
    {
        int value = random.Next(0, 60);
        if (value == 0)
            checkQueueCount(peer);
        NetDataWriter writer = new NetDataWriter();
        if (server != null)
        {
            writer.Put(jsonString);
            peer.Send(writer, DeliveryMethod.ReliableOrdered);
            writer.Reset();
        }
    }

    private void checkQueueCount()
    {
        if (server.ConnectedPeerList.Count > 0)
        {
            int peerIndex = random.Next(0, server.ConnectedPeerList.Count);
            NetPeer peer = server.ConnectedPeerList[peerIndex];
            checkQueueCount(peer);
        }
    }

    private void checkQueueCount(NetPeer peer)
    {
        byte channelId = 0;
        bool isOrdered = true;
        int queueCount = peer.GetPacketsCountInReliableQueue(channelId, isOrdered);
        Globals.Log("sendJsonString(): Packets in queue: " + queueCount);
        // Throttle server processing until the network message queue is caught up a little.
        while (queueCount > 50000)
        {
            Thread.Sleep(1000);
            queueCount = peer.GetPacketsCountInReliableQueue(channelId, isOrdered);
        }
        Globals.Log("sendJsonString(): Packets in queue: " + queueCount);
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
        if (GAME_PHASE_PLAN.Equals(gameState.CurrentPhase))
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
                faction.Status = FACTION_STATUS_DISCONNECTED;
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
