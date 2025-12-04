using System.Text.Json;
using LiteNetLib;

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
        Faction faction = gameState.Factions.NameToFaction[JoinGameValues.FactionName];
        if (gameState.Players.playerNameToPlayer.ContainsKey(JoinGameValues.Name))
        {
            Globals.Log("execute(): Player with that name is already used:" + JoinGameValues.Name);
            return;
        }
        List<string> playerNames = gameState.Players.playerNameToPlayer.Keys.ToList<string>();
        for (int i = 0; i < gameState.Players.playerNameToPlayer.Count; i++)
        {
            Player player = gameState.Players.playerNameToPlayer[playerNames[i]];
            if (player.FactionColor.Equals(faction.Color))
            {
                Globals.Log("execute(): Faction has already been chosen: " + faction.Color);
                return;
            }
        }
        server.PlayerNameToPeer[JoinGameValues.Name] = peer;
        server.PeerToPlayerName[peer] = JoinGameValues.Name;
        Player newPlayer = gameState.Players.AddPlayer(gameState, JoinGameValues.Name, faction.Color, true);
        if (gameState.PlayerExecutionReady.ContainsKey(newPlayer.Name))
        {
            if (gameState.PlayerExecutionReady[newPlayer.Name])
            {
                faction.Status = "ready";
            }
            else
            {
                faction.Status = "planning";
            }
        }
        else
        {
            faction.Status = "planning";
        }
        server.sendGameState();

        gameState.PlayerPlanningReady[newPlayer.Name] = true;
        GameLogic gameLogic = server.GameLogic;
        gameLogic.checkPlayersReadyForTimedPlanning();

    }
}
