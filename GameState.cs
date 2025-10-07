using System.Text.Json.Serialization;

namespace GlobalConquest;

public class GameState
{

    public GameSettings GameSettings { get; set; }
    public Factions Factions { get; set; }

    [JsonIgnore]
    public Map Map { get; set; }

    public MapHex MapHex { get; set; }

    public GameState()
    {
        Factions = new Factions();
    }

}