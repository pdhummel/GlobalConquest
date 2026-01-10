using System.Text.Json;
using LiteNetLib;
using static GameConstants;
namespace GlobalConquest.Actions;

public class PlanningReadyAction : PlayerAction
{
    Server? server;


    public new void deserializeAndExecute(NetPeer peer, Object serverObj)
    {
        //Globals.Log("deserializeAndExecute()");
        if (MessageAsJson != null)
        {
            PlanningReadyAction? action =
                    JsonSerializer.Deserialize<PlanningReadyAction>(this.MessageAsJson);
            action?.execute(peer, serverObj);
        }
    }

    public new void execute(NetPeer peer, Object serverObj)
    {
        Globals.Log("execute()");
        Server server = (Server)serverObj;
        GameState gameState = server.gameState;
        if (ClientIdentifier != null)
        {
            gameState.PlayerPlanningReady[ClientIdentifier] = true;
            Player player = gameState.Players.playerNameToPlayer[ClientIdentifier];
            Faction faction = gameState.Factions.ColorToFaction[player.FactionColor];
            faction.Status = FACTION_STATUS_PLANNING;
            server.sendGameState();
        }
        GameLogic gameLogic = server.GameLogic;
        //gameLogic.checkPlayersReadyForTimedPlanning(gameState.PlayerPlanningReady);
        gameLogic.checkPlayersReadyForTimedPlanning();
    }

}
