using System.Text.Json;

namespace GlobalConquest.Actions;

public class JoinGameAction : PlayerAction
{
    public JoinGameValues? JoinGameValues { get; set; }

    public new void deserializeAndExecute(Object serverObj)
    {
        //Console.WriteLine("JoinGameAction.deserializeAndExecute()");
        if (MessageAsJson != null)
        {
            JoinGameAction? action =
                    JsonSerializer.Deserialize<JoinGameAction>(this.MessageAsJson);
            action?.execute(serverObj);
        }
    }
    
    public new void execute(Object serverObj)
    {
        Console.WriteLine("JoinGameAction.execute()");
        Server server = (Server)serverObj;
        GameState gameState = server.gameState;
        Faction faction = gameState.Factions.nameToFaction[JoinGameValues.FactionName];
        if (gameState.Players.playerNameToPlayer.ContainsKey(JoinGameValues.Name))
        {
            return;
        }
        List<string> playerNames = gameState.Players.playerNameToPlayer.Keys.ToList<string>();
        for (int i=0; i< gameState.Players.playerNameToPlayer.Count; i++)
        {
            Player player = gameState.Players.playerNameToPlayer[playerNames[i]];
            if (player.FactionColor.Equals(faction.Color))
            {
                return;
            }
        }
        Player newPlayer = gameState.Players.AddPlayer(gameState, JoinGameValues.Name, faction.Color, true);
        gameState.placeInitialUnit(newPlayer);
    }
}