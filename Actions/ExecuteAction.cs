using System.Text.Json;
using LiteNetLib;

namespace GlobalConquest.Actions;

public class ExecuteAction : PlayerAction
{
    Server? server;


    public new void deserializeAndExecute(NetPeer peer, Object serverObj)
    {
        //Console.WriteLine("ExecuteAction.deserializeAndExecute()");
        if (MessageAsJson != null)
        {
            ExecuteAction? action =
                    JsonSerializer.Deserialize<ExecuteAction>(this.MessageAsJson);
            action?.execute(peer, serverObj);
        }
    }

    public new void execute(NetPeer peer, Object serverObj)
    {
        Console.WriteLine("ExecuteAction.execute()");
        Server server = (Server)serverObj;
        GameState gameState = server.gameState;
        if (ClientIdentifier != null)
        {
            bool first = true;
            foreach (string key in gameState.PlayerExecutionReady.Keys)
            {
                if (gameState.PlayerExecutionReady[key])
                    first = false;
            }
            gameState.PlayerExecutionReady[ClientIdentifier] = true;
            Player player = gameState.Players.playerNameToPlayer[ClientIdentifier];
            Faction faction = gameState.Factions.ColorToFaction[player.FactionColor];
            faction.Status = "ready";
            // first player to execute gets a $5 reward.
            if (first)
            {
                faction.Money += 5;
            }
            //gameState.updateTicks();
            server.sendGameState();
        }


        bool startExecution = false;
        if ("Immediate".Equals(gameState.GameSettings.ExecutionMode))
        {
            startExecution = true;
        }
        if ("Quorum".Equals(gameState.GameSettings.ExecutionMode))
        {
            int readyCount = 0;
            foreach (string key in gameState.PlayerExecutionReady.Keys)
            {
                if (gameState.PlayerExecutionReady[key])
                {
                    readyCount += 1;
                }
            }
            if (readyCount >= gameState.GameSettings.NumberOfHumans)
                startExecution = true;
        }

        if (startExecution)
        {
            GameLogic gameLogic = new GameLogic();
            gameLogic.server = server;
            Thread executionPhaseThread = new Thread(new ThreadStart(gameLogic.doExecutionPhase))
            {
                IsBackground = true // Ensures thread closes with the main app
            };
            executionPhaseThread.Start();
        }
    }




    

}