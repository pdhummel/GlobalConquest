using System.Text.Json;
using LiteNetLib;
using GlobalConquest;
using static GameConstants;
using static GlobalConquest.GameEvent;
namespace GlobalConquest.Actions;

public class JoinGameAction : PlayerAction
{
    public JoinGameValues? JoinGameValues { get; set; }

    public new void deserializeAndExecute(NetPeer peer, Object serverObj)
    {
        //Globals.Log("deserializeAndExecute()");
        if (MessageAsJson != null)
        {
            JoinGameAction? action =
                    JsonSerializer.Deserialize<JoinGameAction>(this.MessageAsJson);
            action?.execute(peer, serverObj);
        }
    }

    public new void execute(NetPeer peer, Object serverObj)
    {
        Globals.Log("execute()");
        Server server = (Server)serverObj;
        GameState gameState = server.gameState;
        if (gameState.Players.playerNameToPlayer.ContainsKey(JoinGameValues.Name))
        {
            Globals.Log("execute(): Player with that name is already used:" + JoinGameValues.Name);
            GameEvent gameEvent = new GameEvent();
            gameEvent.EventType = GAME_EVENT_SERVER_MESSAGE;
            gameEvent.TargetScreenId = "JoinGameScreen";
            gameEvent.EventString = "Player with that name is already used: " + JoinGameValues.Name;
            server.sendGamePlayEvent(peer, gameEvent);
            return;
        }
        List<string> playerNames = gameState.Players.playerNameToPlayer.Keys.ToList<string>();
        server.PlayerNameToPeer[JoinGameValues.Name] = peer;
        server.PeerToPlayerName[peer] = JoinGameValues.Name;
        if (JoinGameValues.IsObserverOnly)
        {
            server.sendGameState(peer);
            server.sendMap(peer);
            return;
        }
        Faction faction = gameState.Factions.NameToFaction[JoinGameValues.FactionName];
        int factionsWithHumanPlayer = 0;
        for (int i = 0; i < gameState.Players.playerNameToPlayer.Count; i++)
        {
            Player player = gameState.Players.playerNameToPlayer[playerNames[i]];
            if (player.FactionColor.Equals(faction.Color))
            {
                Globals.Log("execute(): Faction has already been chosen: " + faction.Color);
                GameEvent gameEvent = new GameEvent();
                gameEvent.EventType = GAME_EVENT_SERVER_MESSAGE;
                gameEvent.TargetScreenId = "JoinGameScreen";
                gameEvent.EventString = "Faction has already been chosen: " + faction.Color;
                server.sendGamePlayEvent(peer, gameEvent);
                return;
            }
            if (player.FactionColor != null)
            {
                factionsWithHumanPlayer += 1;
            }
        }
        if (factionsWithHumanPlayer >= gameState.GameSettings.NumberOfHumans)
        {
            Globals.Log("execute(): Exceeds max number of human players: " + gameState.GameSettings.NumberOfHumans);
            GameEvent gameEvent = new GameEvent();
            gameEvent.EventType = GAME_EVENT_SERVER_MESSAGE;
            gameEvent.TargetScreenId = "JoinGameScreen";
            gameEvent.EventString = "Exceeds allowed number of human players. Please increase the setting.";
            server.sendGamePlayEvent(peer, gameEvent);
            return;            
        }
        Player newPlayer = gameState.Players.AddPlayer(gameState, JoinGameValues.Name, faction.Color, true);
        if (gameState.PlayerExecutionReady.ContainsKey(newPlayer.Name))
        {
            if (gameState.PlayerExecutionReady[newPlayer.Name])
            {
                faction.Status = "ready";
            }
            else
            {
                faction.Status = FACTION_STATUS_PLANNING;
            }
        }
        else
        {
            faction.Status = FACTION_STATUS_PLANNING;
        }
        server.sendGameState();
        if (gameState.PlayerJoined.Count >= gameState.GameSettings.NumberOfHumans)
            server.sendMap(peer);
        else
        {
            GameEvent gameEvent = new GameEvent();
            gameEvent.EventType = GAME_EVENT_SERVER_MESSAGE;
            gameEvent.EventString = "Waiting for other players: " + gameState.PlayerJoined.Count + " of " + 
                                    gameState.GameSettings.NumberOfHumans + " have joined.";
            server.sendGamePlayEvent(gameEvent);
        }
            

        gameState.PlayerPlanningReady[newPlayer.Name] = true;
        GameLogic gameLogic = server.GameLogic;
        gameLogic.checkPlayersReadyForTimedPlanning();

    }
}
