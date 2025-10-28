namespace GlobalConquest;

public class Players
{
    public Dictionary<string, Player> playerNameToPlayer { get; set; } = new Dictionary<string, Player>();

    public Dictionary<string, Player> colorToPlayer { get; set; } = new Dictionary<string, Player>();
    public Dictionary<string, Player> factionNameToPlayer { get; set; } = new Dictionary<string, Player>();

    public Players()
    {

    }
    
    public Player AddPlayer(GameState gameState, String name, string color, bool isHuman)
    {
        Console.WriteLine("AddPlayer(): " + name + ", " + color);
        Player player = new();
        player.Name = name;
        player.FactionColor = color;
        player.IsHuman = isHuman;
        playerNameToPlayer[name] = player;
        colorToPlayer[color] = player;
        factionNameToPlayer[gameState.Factions.colorToFaction[color].Name] = player;
        gameState.PlayerJoined[name] = true;
        return player;
    }
}