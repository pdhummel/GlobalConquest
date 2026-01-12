using System.Text.Json;
using LiteNetLib;
using static GameConstants;
namespace GlobalConquest.Actions;

public class SetProposedTreatyAction : PlayerAction
{
    public string FactionColor { get; set; }
    public string OtherFactionColor { get; set; }
    public string ProposedTreaty { get; set; }

    public new void deserializeAndExecute(NetPeer peer, Object serverObj)
    {
        //Globals.Log("deserializeAndExecute()");
        if (MessageAsJson != null)
        {
            SetProposedTreatyAction? action =
                    JsonSerializer.Deserialize<SetProposedTreatyAction>(this.MessageAsJson);
            action?.execute(peer, serverObj);
        }
    }

    public new void execute(NetPeer peer, Object serverObj)
    {
        Globals.Log("execute(): SetProposedTreatyAction enter");
        Server server = (Server)serverObj;
        GameState gameState = server.gameState;
        
        if (ClientIdentifier != null && gameState.Players.playerNameToPlayer.ContainsKey(ClientIdentifier))
        {
            Player player = gameState.Players.playerNameToPlayer[ClientIdentifier];
            // Verify the player is setting a treaty for their own faction
            if (player.FactionColor.Equals(FactionColor) && gameState.Factions.ColorToFaction.ContainsKey(FactionColor))
            {
                Faction faction = gameState.Factions.ColorToFaction[FactionColor];
                Faction otherFaction = gameState.Factions.ColorToFaction[OtherFactionColor];
                // Verify the other faction color is valid
                if (FACTION_COLORS.Contains(OtherFactionColor) && !OtherFactionColor.Equals(FactionColor))
                {
                    // Set the proposed treaty
                    faction.ColorToProposedTreaty[OtherFactionColor] = ProposedTreaty;
                    gameState.Factions.ColorToFaction[FactionColor] = faction;
                    Globals.Log($"execute(): Set proposed treaty from {FactionColor} to {OtherFactionColor} to {ProposedTreaty}");

                    string currentTreaty = gameState.Factions.GetCurrentTreaty(FactionColor, OtherFactionColor);
                    string newTreaty = gameState.Factions.DetermineNewTreaty(FactionColor, OtherFactionColor);
                    if (currentTreaty.Equals(TREATY_TEAM_MATES) && 
                        faction.GetProposedTreatyForColor(OtherFactionColor).Equals(otherFaction.GetProposedTreatyForColor(FactionColor)) &&
                        newTreaty.Equals(TREATY_ALLIANCE))
                    {
                        gameState.Factions.SetCurrentTreaty(FactionColor, OtherFactionColor, newTreaty);
                        faction.SetProposedTreatyForColor(OtherFactionColor, newTreaty);
                        otherFaction.SetProposedTreatyForColor(FactionColor, newTreaty);
                    }
                    else if (currentTreaty.Equals(TREATY_ALLIANCE) && 
                        faction.GetProposedTreatyForColor(OtherFactionColor).Equals(otherFaction.GetProposedTreatyForColor(FactionColor)) &&
                        newTreaty.Equals(TREATY_CEASE_FIRE))
                    {
                        gameState.Factions.SetCurrentTreaty(FactionColor, OtherFactionColor, newTreaty);
                        faction.SetProposedTreatyForColor(OtherFactionColor, newTreaty);
                        otherFaction.SetProposedTreatyForColor(FactionColor, newTreaty);
                    }
                    else if (currentTreaty.Equals(TREATY_CEASE_FIRE) && 
                        faction.GetProposedTreatyForColor(OtherFactionColor).Equals(otherFaction.GetProposedTreatyForColor(FactionColor)) &&
                        newTreaty.Equals(TREATY_AT_WAR))
                    {
                        gameState.Factions.SetCurrentTreaty(FactionColor, OtherFactionColor, newTreaty);
                        faction.SetProposedTreatyForColor(OtherFactionColor, newTreaty);
                        otherFaction.SetProposedTreatyForColor(FactionColor, newTreaty);
                    }
                    // Send updated game state to all clients
                    server.sendGameState();
                }
            }
        }
    }
}
