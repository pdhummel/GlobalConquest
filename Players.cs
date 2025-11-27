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
        Globals.Log("AddPlayer(): " + name + ", " + color);
        Player player = new();
        player.Name = name;
        player.FactionColor = color;
        player.IsHuman = isHuman;
        playerNameToPlayer[name] = player;
        colorToPlayer[color] = player;
        factionNameToPlayer[gameState.Factions.ColorToFaction[color].Name] = player;
        gameState.PlayerJoined[name] = true;
        return player;
    }
    
    public void RemovePlayer(GameState gameState, String name)
    {
        if (playerNameToPlayer.ContainsKey(name))
        {
            Player player = playerNameToPlayer[name];
            playerNameToPlayer.Remove(name);
            if (colorToPlayer.ContainsKey(player.FactionColor))
            {
                colorToPlayer.Remove(player.FactionColor);
            }
            if (gameState.PlayerJoined.ContainsKey(name))
            {
                gameState.PlayerJoined.Remove(name);
            }
        }
    }
}