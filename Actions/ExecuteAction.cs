using System.Text.Json;

namespace GlobalConquest.Actions;

public class ExecuteAction : PlayerAction
{
    Server? server;


    public new void deserializeAndExecute(Object serverObj)
    {
        //Console.WriteLine("ExecuteAction.deserializeAndExecute()");
        if (MessageAsJson != null)
        {
            ExecuteAction? action =
                    JsonSerializer.Deserialize<ExecuteAction>(this.MessageAsJson);
            action?.execute(serverObj);
        }
    }

    public new void execute(Object serverObj)
    {
        Console.WriteLine("ExecuteAction.execute()");
        Server server = (Server)serverObj;
        GameState gameState = server.gameState;
        // TODO: is ClientIdentifier the right key? color? faction?
        if (ClientIdentifier != null)
        {
            gameState.PlayerExecutionReady[ClientIdentifier] = true;
        }

        // TODO: evaluate whether the execution phase should occur.
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