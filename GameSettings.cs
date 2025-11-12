namespace GlobalConquest;

public class GameSettings
{
    public int Port { get; set;  }

    public int NumberOfHumans { get; set;  }

    public int Height { get; set; }

    public int Width { get; set; }

    public int NumberOfBurbs { get; set; } = 0;
    public int NumberOfTurnsForGame { get; set; } = -1;
    // Each turn has eight rounds (each round gives each unit a chance to move and/or fire).
    public int NumberOfRoundsPerTurn { get; set; } = 8;

    public string Visibility { get; set; } = "Fog of War";

    public string ExecutionMode { get; set; } = "Quorum";
    public int StartingMoney { get; set; } = 0;
    public string ScoringOption { get; set; } = "Combined"; // Income, Capital, Head-Count, Combined
    public bool HasNatives { get; set; } = false;

    public GameSettings()
    {

    }
}