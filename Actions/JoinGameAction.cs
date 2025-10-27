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
        gameState.Players.AddPlayer(gameState, JoinGameValues.Name, faction.Color, true);
        //server.sendGameState();
    }
}