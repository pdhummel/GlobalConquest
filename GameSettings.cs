namespace GlobalConquest;

public class GameSettings
{
    public int Port { get; set;  }

    public int NumberOfHumans { get; set;  }

    public int Height { get; set; }

    public int Width { get; set; }
    public int NumberOfTurnsForGame { get; set; } = -1;
    public int NumberOfRoundsPerTurn { get; set; } = 10;

    public string Visibility { get; set; }

    public string ExecutionMode { get; set; }

    public GameSettings()
    {

    }
}