namespace GlobalConquest;

public class GameSettings
{
    public int Port { get; set;  }

    public int NumberOfHumans { get; set;  }

    public int Height { get; set; }

    public int Width { get; set; }
    public int NumberOfTurnsForGame { get; set; } = -1;
    // Each turn has eight rounds (each round gives each unit a chance to move and/or fire).
    public int NumberOfRoundsPerTurn { get; set; } = 8;

    public string Visibility { get; set; }

    public string ExecutionMode { get; set; }

    public GameSettings()
    {

    }
}