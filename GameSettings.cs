using static GameConstants;
namespace GlobalConquest;

public class GameSettings
{
    public int Port { get; set; }

    public int NumberOfHumans { get; set; }

    public int Height { get; set; }

    public int Width { get; set; }

    public int NumberOfBurbs { get; set; } = 0;
    public int NumberOfTurnsForGame { get; set; } = -1;
    // Each turn has eight rounds (each round gives each unit a chance to move and/or fire).
    public int NumberOfRoundsPerTurn { get; set; } = 8;

    public string Visibility { get; set; } = "Fog of War";

    public string ExecutionMode { get; set; } = EXECUTION_QUORUM;
    public int TimedSeconds { get; set; } = 180;
    public int StartingMoney { get; set; } = 0;
    public string ScoringOption { get; set; } = VICTORY_COMBINED; // Income, Capital, Head-Count, Combined
    public bool HasNatives { get; set; } = false;
    public bool CanLoseComCen {get; set;}
    public bool IsAdvancedEconomics {get;set;}
    public int NumberOfIslands {get; set;} = 1;
    public bool IsStandaloneServer {get; set;}
    public string UnitPalette {get; set;}
    public String ResourceMode {get;set;}

    public GameSettings()
    {

    }
}
