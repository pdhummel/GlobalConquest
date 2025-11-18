using LiteNetLib;
using LiteNetLib.Utils;
using System.Text.Json;
using GlobalConquest.Units;
using GlobalConquest.Actions;

namespace GlobalConquest;

public class Client
{
    private NetManager? netmanagerclient;
    private EventBasedNetListener? listener;
    private Thread? clientThread;
    public string? ClientIdentifier { get; set; }   // this is the player name
    private NetPeer? serverPeer;

    public GlobalConquestGame? GlobalConquestGame { get; set; }

    public bool isLoadContentComplete { get; set; } = false;

    public GameState GameState { get; set; } = new GameState();
    public JoinGameValues JoinGameValues { get; set; }

    public Client(GlobalConquestGame gcGame)
    {
        GlobalConquestGame = gcGame;
    }

    public void Connect(JoinGameValues joinGameValues, string key)
    {
        ClientIdentifier = joinGameValues.Name;
        listener = new EventBasedNetListener();
        listener.PeerConnectedEvent += OnPeerConnected;
        listener.NetworkReceiveEvent += OnNetworkReceive;
        listener.PeerDisconnectedEvent += OnPeerDisconnected;

        netmanagerclient = new NetManager(listener)
        {
            UnconnectedMessagesEnabled = true,
            UnsyncedEvents = true
        };
        netmanagerclient.Start();
        string host = joinGameValues.HostIp;
        int port = joinGameValues.Port;
        serverPeer = netmanagerclient.Connect(host, port, key); // Use the same key as the server
        Console.WriteLine($"Connect(): Client attempting to connect to {host}:{port}");
        // Create and start the new thread for the client's polling loop
        clientThread = new Thread(new ThreadStart(ClientLoop))
        {
            IsBackground = true // Ensures thread closes with the main app
        };
        clientThread.Start();
        PlayerAction action = new(serverPeer, ClientIdentifier, "connect");
    }


    private void OnDestroy()
    {
        netmanagerclient?.Stop();
    }

    private void ClientLoop()
    {
        Console.WriteLine("ClientLoop(): Client polling");
        // This is the client's polling loop, which runs continuously on its own thread.
        while (true)
        {
            netmanagerclient?.PollEvents();
            Thread.Sleep(15); // Adjust sleep time to control CPU usage.
        }
    }

    public void Stop()
    {
        netmanagerclient?.Stop();
        Console.WriteLine("Stop(): Client stopped.");
    }

    public void SendData(string peerIdentifier, string data)
    {
        NetDataWriter writer = new();
        writer.Put(data); // Add your data
        serverPeer?.Send(writer, DeliveryMethod.ReliableOrdered);
        Console.WriteLine("SendData(): " + peerIdentifier + " Client sent data " + data);
    }

    public void SendAction(string peerIdentifier, PlayerAction action)
    {
        Type type = Type.GetType(action.ClassType);
        dynamic subClassAction = Convert.ChangeType(action, type);

        if (peerIdentifier == null)
        {
            peerIdentifier = subClassAction.ClientIdentifier;
        }
        if (subClassAction.ClientIdentifier == null)
        {
            subClassAction.ClientIdentifier = peerIdentifier;
        }
        String data = JsonSerializer.Serialize(subClassAction);
        SendData(peerIdentifier, data);
    }


    // --- LiteNetLib Event Handlers ---
    private void OnPeerConnected(NetPeer peer)
    {
        Console.WriteLine($"OnPeerConnected(): Client peer connected: {peer.Address}");
        JoinGameAction joinGameAction = new JoinGameAction();
        joinGameAction.JoinGameValues = JoinGameValues;
        joinGameAction.ClassType = "GlobalConquest.Actions.JoinGameAction";
        joinGameAction.ClientIdentifier = JoinGameValues.Name;
        SendAction(JoinGameValues.Name, joinGameAction);
    }

    private void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
    {
        var jsonString = reader.GetString();
        //GameState oldGameState = GameState;
        GameEvent? gameEvent = JsonSerializer.Deserialize<GameEvent>(jsonString);
        if (gameEvent != null)
            Console.WriteLine("OnNetworkReceive(): gameEvent=" + gameEvent.EventType);
        if (gameEvent != null && "mapUpdate".Equals(gameEvent.EventType))
        {
            updateMap(gameEvent);
            return;
        }
        else if (gameEvent != null && "gameStateUpdate".Equals(gameEvent.EventType))
        {
            GameState? newGameState = gameEvent.GameState;
            if (GameState.Map != null && GameState.Map.IsMapReady)
                newGameState.Map = GameState.Map;
            GameState = newGameState;
        }
        else if (gameEvent != null && "gameStateAndMapUpdate".Equals(gameEvent.EventType))
        {
            GameState? newGameState = gameEvent.GameState;
            if (GameState.Map != null && GameState.Map.IsMapReady)
                newGameState.Map = GameState.Map;

            if (gameEvent.MapHex != null)
                GameState.Map.Hexes[gameEvent.MapHex.Y, gameEvent.MapHex.X] = gameEvent.MapHex;
            // else if (GameState.Map != null && GameState.Map.Hexes != null && gameEvent.GameState != null && gameEvent.GameState.MapHex != null)
            else if (gameEvent.GameState != null && gameEvent.GameState.MapHex != null)
                GameState.Map.Hexes[gameEvent.GameState.MapHex.Y, gameEvent.GameState.MapHex.X] = gameEvent.GameState.MapHex;
            GameState = newGameState;

        }

        if ("plan".Equals(GameState.CurrentPhase) && GameState.PlayerPlanningReady.ContainsKey(ClientIdentifier) && GameState.PlayerPlanningReady[ClientIdentifier] == false)
        {
            PlanningReadyAction action = new PlanningReadyAction();
            action.ClassType = "GlobalConquest.Actions.PlanningReadyAction";  //executeAction.GetType().FullName
            action.ClientIdentifier = ClientIdentifier;
            SendAction(ClientIdentifier, action);
            GameState.PlayerPlanningReady[ClientIdentifier] = true;
        }

        reader.Recycle(); // Free up the data reader
    }

    private void updateMap(GameEvent gameEvent)
    {
        Console.WriteLine("updateMap(): gameEvent mapHexBuffer=" + gameEvent.MapHexBuffer.Count);
        if (GameState != null)
        {
            if (GameState.Map == null)
            {
                Console.WriteLine("updateMap(): new Map");
                GameState.Map = new Map();
                Map map = GameState.Map;
                GameSettings gameSettings = GameState.GameSettings;
                map.Y = gameSettings.Height;
                map.X = gameSettings.Width;
                map.Hexes = new MapHex[gameSettings.Height, gameSettings.Width];
            }
            if (GameState.Map.Hexes == null)
            {
                GameState.Map.Hexes = new MapHex[GameState.GameSettings.Height, GameState.GameSettings.Width];
            }
                
            for (int liY = 0; liY < GameState.GameSettings.Height; liY++)
            {
                for (int liX = 0; liX < GameState.GameSettings.Width; liX++)
                {
                    if (GameState.Map.Hexes[liY, liX] == null)
                    {
                        //Console.WriteLine("OnNetworkReceive(): new MapHex");
                        MapHex mapHex = new MapHex();
                        mapHex.Y = liY;
                        mapHex.X = liX;
                        mapHex.Terrain = "sea";     // this is temporary so should not matter
                        GameState.Map.Hexes[liY, liX] = mapHex;
                    }
                }
            }


            if (gameEvent.MapHex != null)
            {
                Console.WriteLine("updateMap(): sync mapHex");
                GameState.Map.Hexes[gameEvent.MapHex.Y, gameEvent.MapHex.X] = gameEvent.MapHex;
            }
            if (gameEvent.MapHexBuffer != null)
            {
                Console.WriteLine("updateMap(): sync mapHexBuffer, IsLastMapHexBufferUpdate=" + gameEvent.IsLastMapHexBufferUpdate);
                foreach (MapHex mapHex in gameEvent.MapHexBuffer)
                {
                    GameState.Map.Hexes[mapHex.Y, mapHex.X] = mapHex;
                }

                if (!isLoadContentComplete && gameEvent.IsLastMapHexBufferUpdate)
                {
                    GameState.Map.IsMapReady = true;
                    Console.WriteLine("updateMap(): Loading map content into client hexMapEngineAdapter");
                    GlobalConquestGame?.HexMapLoadContent();
                    isLoadContentComplete = true;
                }

            }
        }

    }

    private void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        Console.WriteLine($"OnPeerDisconnected(): Client peer disconnected: {peer.Address}. Reason: {disconnectInfo.Reason}");
        GameState.CurrentPhase = "disconnected";
        Thread localThread = new Thread(new ThreadStart(ReConnect))
        {
            IsBackground = true // Ensures thread closes with the main app
        };
        localThread.Start();

    }

    private void ReConnect()
    {
        long originalMilliseconds = DateTime.Now.Ticks;
        long retryUntil = 3000;
        while (DateTime.Now.Ticks < originalMilliseconds + retryUntil)
        {
            if ("disconnected".Equals(GameState.CurrentPhase))
            {
                Console.WriteLine("ReConnect(): retry");
                Connect(JoinGameValues, "GlobalConquest");
                Thread.Sleep(300);
            }
            else
            {
                break;
            }
        }
        Console.WriteLine("ReConnect(): exit");
    }

}
