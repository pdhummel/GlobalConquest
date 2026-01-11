using LiteNetLib;
using LiteNetLib.Utils;
using System.Text.Json;
using System.Collections.Concurrent;
using GlobalConquest.Units;
using GlobalConquest.Actions;
using static GameConstants;
using Microsoft.Xna.Framework;

namespace GlobalConquest;

public class Client
{
    private NetManager? netmanagerclient;
    private EventBasedNetListener? listener;
    private Thread? clientThread;
    private Thread? processGameEventQueueThread;
    public string? ClientIdentifier { get; set; }   // this is the player name
    private NetPeer? serverPeer;

    public GlobalConquestGame? GlobalConquestGame { get; set; }

    public bool IsObserverOnly {get; set;} = false;

    public GameState GameState { get; set; } = new GameState();
    public JoinGameValues JoinGameValues { get; set; }
    ConcurrentQueue<GameEvent> gameEventExecutionQueue = new ConcurrentQueue<GameEvent>();

    //public List<GameEvent> GamePlayEvents { get; set; } = new List<GameEvent>();

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
        Globals.Log($"Connect(): Client attempting to connect to {host}:{port}");
        // Create and start the new thread for the client's polling loop
        clientThread = new Thread(new ThreadStart(ClientLoop))
        {
            IsBackground = true // Ensures thread closes with the main app
        };
        clientThread.Start();
        processGameEventQueueThread = new Thread(new ThreadStart(processGameEventQueue))
        {
            IsBackground = true // Ensures thread closes with the main app
        };
        processGameEventQueueThread.Start();

        PlayerAction action = new(serverPeer, ClientIdentifier, "connect");
    }


    private void OnDestroy()
    {
        netmanagerclient?.Stop();
    }

    private void ClientLoop()
    {
        Globals.Log("ClientLoop(): Client polling");
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
        Globals.Log("Stop(): Client stopped.");
    }

    public void SendData(string peerIdentifier, string data)
    {
        NetDataWriter writer = new();
        writer.Put(data); // Add your data
        serverPeer?.Send(writer, DeliveryMethod.ReliableOrdered);
        Globals.Log("SendData(): " + peerIdentifier + " Client sent data " + data);
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
        Globals.Log($"OnPeerConnected(): Client peer connected: {peer.Address}");
        JoinGameAction joinGameAction = new JoinGameAction();
        joinGameAction.JoinGameValues = JoinGameValues;
        joinGameAction.ClassType = "GlobalConquest.Actions.JoinGameAction";
        joinGameAction.ClientIdentifier = JoinGameValues.Name;
        SendAction(JoinGameValues.Name, joinGameAction);
    }

    private void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
    {
        var jsonString = reader.GetString();
        GameEvent gameEvent = null;
        try 
        {
            gameEvent = JsonSerializer.Deserialize<GameEvent>(jsonString);
        }
        catch(Exception ex)
        {
            Globals.Log("OnNetworkReceive(): Could not deserialize gameEvent: " + ex);
        }
        if (gameEvent != null)
        {
            if ("execution".Equals(GameState.CurrentPhase))
            {
                gameEventExecutionQueue.Enqueue(gameEvent);
            }
            else
            {
                processGameEvent(gameEvent);
            }
        }    
        reader.Recycle(); // Free up the data reader
    }

    private void processGameEventQueue()
    {
        int currentRound = GameState.CurrentRound;
        while (true)
        {
            GameEvent gameEvent;
            if (gameEventExecutionQueue.Count > 0)
            {
                //Globals.Log("processGameEventQueue():" + gameEventExecutionQueue.Count);
                gameEventExecutionQueue.TryDequeue(out gameEvent);
                {
                processGameEvent(gameEvent);
                    if (currentRound != GameState.CurrentRound && gameEventExecutionQueue.Count < 500)
                    {
                        Thread.Sleep(GlobalConquestGame.MyJoinGameValues.GameExecutionSpeed);
                    }
                    else if (gameEventExecutionQueue.Count > 1000)
                        Globals.Log("processGameEventQueue(): queueSize=" + gameEventExecutionQueue.Count);
                }
            }
            else
            {
                Thread.Sleep(10);
            }    
        }
    }

    private void processGameEvent(GameEvent gameEvent)
    {
        handleGamePlayEvent(gameEvent);

        if (gameEvent != null && EVENT_TYPE_MAP_UPDATE.Equals(gameEvent.EventType))
        {
            updateMap(gameEvent);
            return;
        }
        else if (gameEvent != null && EVENT_TYPE_GAME_STATE_UPDATE.Equals(gameEvent.EventType))
        {
            GameState? newGameState = gameEvent.GameState;
            //if (GameState.Map != null && GameState.Map.IsMapReady)
            //    newGameState.Map = GameState.Map;
            //GameState = newGameState;
            GameState.copyTransferredGameState(newGameState);
            handleGameOverForClient();
        }
        else if (gameEvent != null && EVENT_TYPE_GAME_STATE_AND_MAP_UPDATE.Equals(gameEvent.EventType))
        {
            GameState? newGameState = gameEvent.GameState;
            GameState.copyTransferredGameState(newGameState);
            bool isHighlighted = false;
            if (GameState.Map != null && GameState.Map.IsMapReady)
            {
                if (gameEvent.MapHex != null) // && !GAME_PHASE_PLAN.Equals(GameState.CurrentPhase))
                    isHighlighted = GameState.Map.Hexes[gameEvent.MapHex.Y, gameEvent.MapHex.X].IsHighlighted;
                //else
                //    GameState.Map.Hexes[gameEvent.MapHex.Y, gameEvent.MapHex.X].IsHighlighted = false;
                //newGameState.Map = GameState.Map;
            }

            if (gameEvent != null && gameEvent.MapHex != null && GameState.Map != null)
            {
                gameEvent.MapHex.IsHighlighted = isHighlighted;    
                GameState.Map.Hexes[gameEvent.MapHex.Y, gameEvent.MapHex.X] = gameEvent.MapHex;
            }
            // else if (GameState.Map != null && GameState.Map.Hexes != null && gameEvent.GameState != null && gameEvent.GameState.MapHex != null)
            else if (gameEvent.GameState != null && gameEvent.GameState.MapHex != null)
            {
                gameEvent.GameState.MapHex.IsHighlighted = isHighlighted;
                if (GameState != null && GameState.Map != null && GameState.Map.Hexes != null)
                    GameState.Map.Hexes[gameEvent.GameState.MapHex.Y, gameEvent.GameState.MapHex.X] = gameEvent.GameState.MapHex;
            }
            //GameState = newGameState;
            handleGameOverForClient();
        }

        if (GAME_PHASE_PLAN.Equals(GameState.CurrentPhase) && GameState.PlayerPlanningReady.ContainsKey(ClientIdentifier) && GameState.PlayerPlanningReady[ClientIdentifier] == false)
        {
            PlanningReadyAction action = new PlanningReadyAction();
            action.ClassType = "GlobalConquest.Actions.PlanningReadyAction";  //executeAction.GetType().FullName
            action.ClientIdentifier = ClientIdentifier;
            SendAction(ClientIdentifier, action);
            GameState.PlayerPlanningReady[ClientIdentifier] = true;
        }

    }


    private void handleGameOverForClient()
    {
        if (GameState.VictoriousColor != null && !GameState.VictoriousColor.Equals(NATIVE_COLOR))
        {
            IsObserverOnly = true;
            return;
        }
        Player player = GlobalConquestGame.identifySelf();
        if (player == null || player.FactionColor.Equals(NATIVE_COLOR))
        {
            IsObserverOnly = true;
            return;
        }
        if (player != null)
        {
            Faction faction = GameState.Factions.ColorToFaction[player.FactionColor];
            if (!faction.HasComCen && !GameState.GameSettings.CanLoseComCen)
            {
                IsObserverOnly = true;
                return;
            }
        }
    } 

    private void handleGamePlayEvent(GameEvent gameEvent)
    {
        GlobalConquestGame.handleGamePlayEvent(gameEvent);
    }

    private void updateMap(GameEvent gameEvent)
    {
        GlobalConquestGame.clientUpdateMap(gameEvent);
    }

    private void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        if (peer != null)
            Globals.Log($"OnPeerDisconnected(): Client peer disconnected: {peer.Address}. Reason: {disconnectInfo.Reason}");
        GameState.CurrentPhase = FACTION_STATUS_DISCONNECTED;
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
            if (FACTION_STATUS_DISCONNECTED.Equals(GameState.CurrentPhase))
            {
                Globals.Log("ReConnect(): retry");
                Connect(JoinGameValues, "GlobalConquest");
                Thread.Sleep(300);
            }
            else
            {
                break;
            }
        }
        Globals.Log("ReConnect(): exit");
    }

}
