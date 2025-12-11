using System.Text.Json;
using LiteNetLib;
using GlobalConquest.Units;
namespace GlobalConquest.Actions;

public class ChangeGameSettingsAction : PlayerAction
{
    Server? server;
    public GameSettings GameSettings {get; set;}

    public new void deserializeAndExecute(NetPeer peer, Object serverObj)
    {
        //Globals.Log("deserializeAndExecute()");
        if (MessageAsJson != null)
        {
            ChangeGameSettingsAction? action =
                    JsonSerializer.Deserialize<ChangeGameSettingsAction>(this.MessageAsJson);
            action?.execute(peer, serverObj);
        }
    }

    public new void execute(NetPeer peer, Object serverObj)
    {
        Globals.Log("execute(): enter");
        Server server = (Server)serverObj;
        GameState gameState = server.gameState;
        gameState.GameSettings.ExecutionMode = GameSettings.ExecutionMode;
        gameState.GameSettings.TimedSeconds = GameSettings.TimedSeconds;
        if (GameSettings.NumberOfTurnsForGame == -1 || gameState.CurrentTurn < GameSettings.NumberOfTurnsForGame)
            gameState.GameSettings.NumberOfTurnsForGame = GameSettings.NumberOfTurnsForGame;
        //gameState.GameSettings.ScoringOption = GameSettings.ScoringOption;
        server.sendGameState();
        server.GameLogic.checkPlayersReadyForTimedPlanning();
    }
}
