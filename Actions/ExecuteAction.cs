using System.Text.Json;
using LiteNetLib;
using static GameConstants;
namespace GlobalConquest.Actions;

public class ExecuteAction : PlayerAction
{
    Server? server;


    public new void deserializeAndExecute(NetPeer peer, Object serverObj)
    {
        //Globals.Log("deserializeAndExecute()");
        if (MessageAsJson != null)
        {
            ExecuteAction? action =
                    JsonSerializer.Deserialize<ExecuteAction>(this.MessageAsJson);
            action?.execute(peer, serverObj);
        }
    }

    public new void execute(NetPeer peer, Object serverObj)
    {
        Globals.Log("execute(): enter");
        Server server = (Server)serverObj;
        GameState gameState = server.gameState;
        bool IsFactionActive = true;
        if (ClientIdentifier != null)
        {
            Player player = gameState.Players.playerNameToPlayer[ClientIdentifier];
            Faction faction = gameState.Factions.ColorToFaction[player.FactionColor];
            if (!faction.HasComCen  && !gameState.GameSettings.CanLoseComCen)
                IsFactionActive = false;
            bool first = true;
            foreach (string key in gameState.PlayerExecutionReady.Keys)
            {
                if (gameState.PlayerExecutionReady[key])
                    first = false;
            }
            gameState.PlayerExecutionReady[ClientIdentifier] = true;
            faction.Status = "ready";
            // first player to execute gets a $5 reward.
            if (first)
            {
                faction.Money += 5;
            }
            server.sendGameState();
        }
        else
        {
            IsFactionActive = false;
        }


        bool startExecution = false;
        if (EXECUTION_QUORUM.Equals(gameState.GameSettings.ExecutionMode))
        {
            int readyCount = 0;
            foreach (string key in gameState.PlayerExecutionReady.Keys)
            {
                if (gameState.PlayerExecutionReady[key])
                {
                    readyCount += 1;
                }
            }
            // TODO: Consider players that have been defeated.
            if (readyCount >= gameState.GameSettings.NumberOfHumans)
                startExecution = true;
        }

        if (IsFactionActive && EXECUTION_IMMEDIATE.Equals(gameState.GameSettings.ExecutionMode))
        {
            startExecution = true;
        }
        else if (IsFactionActive && EXECUTION_GRACE.Equals(gameState.GameSettings.ExecutionMode))
        {
            GameEvent gameEvent = new GameEvent();
            gameEvent.EventType = "gracePeriodStarted";
            server.sendGamePlayEvent(gameEvent);
            server.GameLogic.startExecutionTimer();
        }

        if (startExecution)
        {
            Globals.Log("execute(): new thread for doExecutionPhase");
            GameLogic gameLogic = server.GameLogic;
            Thread executionPhaseThread = new Thread(new ThreadStart(gameLogic.doExecutionPhase))
            {
                IsBackground = true // Ensures thread closes with the main app
            };
            executionPhaseThread.Start();
        }
    }

}
