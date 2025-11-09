using LiteNetLib;
using LiteNetLib.Utils;
using System.Text.Json;
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
    public JoinGameValues JoinGameValues {get; set; }

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
        //Console.WriteLine($"OnNetworkReceive(): Client [Received] from {peer.Address}: {jsonString}");
        GameState oldGameState = GameState;
        // Note that the full Map is never sent from the server to the client b/c the content is too large.
        GameState? newGameState = JsonSerializer.Deserialize<GameState>(jsonString);
        if (newGameState != null && oldGameState != null && newGameState.Ticks > oldGameState.Ticks)
        {
            if (newGameState.MapHex == null)
                Console.WriteLine("OnNetworkReceive(): Updating game state");
            if (GameState.Map != null && GameState.Map.IsMapReady)
                newGameState.Map = GameState.Map;
            if (newGameState.Map == null)
            {
                //Console.WriteLine("OnNetworkReceive(): Creating map in new game state");
                newGameState.Map = new Map();
                newGameState.Map.Y = newGameState.GameSettings.Height;
                newGameState.Map.X = newGameState.GameSettings.Width;

            }
            if (newGameState.Map.Hexes == null)
            {
                //Console.WriteLine("OnNetworkReceive(): Updating map hexes in new game state");
                newGameState.Map.Y = newGameState.GameSettings.Height;
                newGameState.Map.X = newGameState.GameSettings.Width;
                newGameState.Map.Hexes = new MapHex[newGameState.GameSettings.Height, newGameState.GameSettings.Width];
                for (int liY = 0; liY < newGameState.GameSettings.Height; liY++)
                {
                    for (int liX = 0; liX < newGameState.GameSettings.Width; liX++)
                    {
                        if (newGameState.MapHex != null && newGameState.MapHex.Y.Equals(liY) && newGameState.MapHex.X.Equals(liX))
                        {
                            if (newGameState.Map.Hexes[newGameState.MapHex.Y, newGameState.MapHex.X] == null || newGameState.Map.Hexes[newGameState.MapHex.Y, newGameState.MapHex.X].Ticks < newGameState.MapHex.Ticks)
                                newGameState.Map.Hexes[newGameState.MapHex.Y, newGameState.MapHex.X] = newGameState.MapHex;
                        }
                        else if (oldGameState.Map != null && oldGameState.Map.Hexes != null && oldGameState.Map.Hexes[liY, liX] != null)
                        {
                            newGameState.Map.Hexes[liY, liX] = oldGameState.Map.Hexes[liY, liX];
                        }
                        else
                        {
                            //Console.WriteLine("OnNetworkReceive(): new MapHex");
                            MapHex mapHex = new MapHex();
                            mapHex.Y = liY;
                            mapHex.X = liX;
                            mapHex.Terrain = "sea";     // this is temporary so should not matter
                            newGameState.Map.Hexes[liY, liX] = mapHex;
                        }
                    }
                }
                //newGameState.Map.IsMapReady = true;
            }
            if (newGameState.MapHex != null)
            {
                //Console.WriteLine("OnNetworkReceive(): updating MapHex " + newGameState.MapHex.X + "," + newGameState.MapHex.Y);
                newGameState.Map.Hexes[newGameState.MapHex.Y, newGameState.MapHex.X] = newGameState.MapHex;
            }

            GameState = newGameState;
            if (!isLoadContentComplete)
            {
                Console.WriteLine("OnNetworkReceive(): Loading map content");
                GlobalConquestGame?.HexMapLoadContent();
                isLoadContentComplete = true;
            }
            if (oldGameState != null && oldGameState.Map != null && !newGameState.Map.Equals(oldGameState.Map))
            {
                //Console.WriteLine("OnNetworkReceive(): Updating map display");
                GlobalConquestGame?.updateMap();
            }
        }
        else
        {
            Console.WriteLine("OnNetworkReceive(): Discarding message b/c Ticks are older");
            Player player = GlobalConquestGame.identifySelf();
            RefreshGameStateAction action = new RefreshGameStateAction();
            action.ClassType = "GlobalConquest.Actions.RefreshGameStateAction";
            action.ClientIdentifier = player.Name;

            if (newGameState != null && newGameState.MapHex != null)
            {
                action.X = newGameState.MapHex.X;
                action.Y = newGameState.MapHex.Y;
            }
            SendAction(player.Name, action);
        }
        reader.Recycle(); // Free up the data reader
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