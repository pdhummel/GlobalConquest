using System.Text.Json;
using LiteNetLib;
using GlobalConquest;
namespace GlobalConquest.Actions;

public class ResignAction : PlayerAction
{

    public new void deserializeAndExecute(NetPeer peer, Object serverObj)
    {
        //Globals.Log("deserializeAndExecute()");
        if (MessageAsJson != null)
        {
            ResignAction? action =
                    JsonSerializer.Deserialize<ResignAction>(this.MessageAsJson);
            action?.execute(peer, serverObj);
        }
    }

    public new void execute(NetPeer peer, Object serverObj)
    {
        Globals.Log("execute()");
        Server server = (Server)serverObj;
        GameState gameState = server.gameState;
        if (gameState.Players.playerNameToPlayer.ContainsKey(ClientIdentifier))
        {
            Player player = gameState.Players.playerNameToPlayer[ClientIdentifier];
            if (player != null && player.IsHuman)
            {
                player.IsHuman = false;
                if (gameState.PlayerPlanningReady.ContainsKey(player.Name))
                    gameState.PlayerPlanningReady.Remove(player.Name);
                player.Name = "AI-" + player.Name;
                Faction faction = gameState.Factions.ColorToFaction[player.FactionColor];
                faction.Player = player;
                server.PlayerNameToPeer[player.Name] = peer;
                server.PeerToPlayerName[peer] = player.Name;
                
            }
            gameState.GameSettings.NumberOfHumans -= 1;
            server.sendGameState();
        }
        
    }
}
