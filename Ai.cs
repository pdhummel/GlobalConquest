using GlobalConquest.Actions;
using GlobalConquest.HexMapEngine.Structures;
using GlobalConquest.Units;
using SharpDX.Direct2D1;
using static UnitConstants;
using static GameConstants;
using static GlobalConquest.Burbs;

namespace GlobalConquest;

public class Ai
{
    public Server Server { get; set; }
    GameState gameState;
    GameSettings gameSettings;
    Map map;
    public Faction Faction { get; set; }

    Dictionary<string, MapHex> metroSurroundingHexes;
    List<MapHex> metroSurroundingHexesList;
    List<MapHex> dockList = new List<MapHex>();
    List<AiGoal> goals = new List<AiGoal>();
    List<AiGoal> conquestGoals = new List<AiGoal>();
    List<AiGoal> exploreGoals = new List<AiGoal>();
    Dictionary<string, AiGoal> targetXyToGoal = new Dictionary<string, AiGoal>();
    Dictionary<string, HashSet<AiUnit>> unitTypeToAvailableUnits = new Dictionary<string, HashSet<AiUnit>>();
    Dictionary<string, AiUnit> unitIdToAiUnit = new Dictionary<string, AiUnit>();
    Unit comcen;

    Unit spy;
    MapHex myMetroHex;
    MapHex leftMetroHex;
    MapHex rightMetroHex;
    MapHex diagonalMetroHex;
    Random random = new Random();

    AiGoal defaultGoal;

    public Ai()
    {
    }

    public void initialize(Server server)
    {
        Server = server;
        gameState = server.gameState;
        map = gameState.Map;
        gameSettings = gameState.GameSettings;
        myMetroHex = map.MetroLocations[Faction.Color];
        metroSurroundingHexes = map.getSurroundingHexes(myMetroHex);
        metroSurroundingHexesList = map.getSurroundingHexesList(metroSurroundingHexes);
        leftMetroHex = map.LeftMetro[Faction.Color];
        rightMetroHex = map.RightMetro[Faction.Color];
        diagonalMetroHex = map.DiagonalMetro[Faction.Color];
        foreach (MapHex mapHex in metroSurroundingHexesList)
        {
            if (mapHex.Burb != null && BURB_DOCK.Equals(mapHex.Burb.Type))
                dockList.Add(mapHex);
        }
        Unit unit = myMetroHex.getUnit();
        if (unit != null && SPY.Equals(unit.UnitType))
            spy = unit;

        List<MapHex> metroNeighbors = map.getSurroundingHexesList(myMetroHex);
        foreach (MapHex neighbor in metroNeighbors)
        {
            Unit neighborUnit = neighbor.getUnit();
            if (neighborUnit != null && COMMAND_CENTER.Equals(neighborUnit.UnitType))
            {
                comcen = neighborUnit;
                break;
            }
        }

        createInitialGoals();
        // Let the AI know about their comcen.
        if (targetXyToGoal.ContainsKey(myMetroHex.X + "," + myMetroHex.Y))
        {
            AiGoal metroGoal = targetXyToGoal[myMetroHex.X + "," + myMetroHex.Y];
            AiUnit aiUnit = new AiUnit();
            aiUnit.GoalTargetXy = myMetroHex.X + "," + myMetroHex.Y;
            MapHex comcenHex = map.Hexes[comcen.Y, comcen.X];
            aiUnit.InitialPosition = comcenHex;
            aiUnit.UnitType = COMMAND_CENTER;
            aiUnit.Unit = comcen;
            unitIdToAiUnit[comcen.Id] = aiUnit;
            aiUnit.ShouldMoveToTarget = false;
            metroGoal.ActualUnits.Add(aiUnit);
        }
    }

    public void planTurn()
    {
        Globals.Log("Ai.planTurn(): faction=" + Faction.Color);
        if (!Faction.HasComCen && !gameSettings.CanLoseComCen)
            return;
        acceptTreaties();
        checkAvailableUnits();
        addGoals();
        processGoals();
        checkForStuckUnits();
        // Don't move units away from metro if AI has TREATY_TEAM_MATES with a human player
        if (!hasTeamMatesTreatyWithHuman())
        {
            moveUnitsAwayFromMetro();
            moveSpy();
        }
    }


    /*
    TODO:
    if not playing with preferred AI team mates
        if active faction count == 4
            if the first place faction is human and the other factions are AI
                The second place faction will randomally offer cease fire to the other AIs, which they would accept
    */
    public void offerTreaties()
    {
        int activeFactionCount = gameState.GetActiveFactionCount();
        int secondPlaceScoreIndex = 1;
        int lastPlaceScoreIndex = 3;
        int secondToLastPlaceScoreIndex = 2;
        if (activeFactionCount == 3)
        {
            lastPlaceScoreIndex = 2;
            secondToLastPlaceScoreIndex = 1;
        }
        // If active faction count == 3 or 4, an AI faction with the last place score will offer a cease fire treaty 
        // to an AI faction with the 2nd-to-last place score
        if (activeFactionCount >= 3)
        {
            // Get all active factions sorted by score
            List<(string color, int score, bool isAi)> activeFactions = new List<(string, int, bool)>();
            foreach (string color in FACTION_COLORS)
            {
                Faction faction = gameState.Factions.ColorToFaction[color];
                // Check if faction is active
                bool isActive = faction.HasComCen || gameState.GameSettings.CanLoseComCen;
                bool isAi = faction.Player == null || !faction.Player.IsHuman;
                
                if (isActive)
                {
                    activeFactions.Add((color, faction.CombinedScore, isAi));
                }
            }
            
            // Sort by score descending (highest first)
            activeFactions.Sort((a, b) => b.score.CompareTo(a.score));
            
            // Check if 2nd-to-last place is AI and last place is AI
            string secondToLastPlaceColor = activeFactions[secondToLastPlaceScoreIndex].color;
            bool secondToLastPlaceIsAi = activeFactions[secondToLastPlaceScoreIndex].isAi;
            string lastPlaceColor = activeFactions[lastPlaceScoreIndex].color;
            bool lastPlaceIsAi = activeFactions[lastPlaceScoreIndex].isAi;
            string secondPlaceColor = activeFactions[secondPlaceScoreIndex].color;
            bool secondPlaceIsAi = activeFactions[secondPlaceScoreIndex].isAi;
            
            // First, check if this AI has a treaty with another AI that should be downgraded
            // (if either faction is no longer in 2nd-to-last or last place)
            foreach (string color in FACTION_COLORS)
            {
                if (color.Equals(Faction.Color))
                    continue;
                    
                Faction otherFaction = gameState.Factions.ColorToFaction[color];
                bool otherIsAi = otherFaction.Player == null || !otherFaction.Player.IsHuman;
                
                // Only check treaties between AI factions
                if (otherIsAi)
                {
                    string currentTreaty = gameState.Factions.GetCurrentTreaty(Faction.Color, color);
                    string currentProposed = Faction.GetProposedTreatyForColor(color);
                    
                    // If we have a treaty (not at war), check if either faction is no longer in 2nd-to-last or last place
                    if (!currentTreaty.Equals(TREATY_AT_WAR))
                    {
                        bool thisInLastORSecondToLast = Faction.Color.Equals(secondToLastPlaceColor) || Faction.Color.Equals(lastPlaceColor);
                        bool otherInLastOrSecondToLast = color.Equals(secondToLastPlaceColor) || color.Equals(lastPlaceColor);
                        
                        // If either faction is no longer in last or 2nd-to-last 3rd place, downgrade the treaty
                        if (!thisInLastORSecondToLast || !otherInLastOrSecondToLast)
                        {
                            string previousTreaty = getPreviousTreatyLevel(currentTreaty);
                            // Only propose if we haven't already proposed this level or lower
                            if (!currentProposed.Equals(previousTreaty) && !currentProposed.Equals(TREATY_AT_WAR))
                            {
                                Faction.SetProposedTreatyForColor(color, previousTreaty);
                                Globals.Log($"offerTreaties(): AI {Faction.Color} downgrading treaty with {color} from {currentTreaty} to {previousTreaty} (no longer in last/2nd-to-last place)");
                            }
                        }
                    }
                }
            }
            
            // If this AI is the last place AI and 2nd-to-last place is also AI
            if (Faction.Color.Equals(lastPlaceColor) && lastPlaceIsAi && secondToLastPlaceIsAi)
            {
                string currentTreaty = gameState.Factions.GetCurrentTreaty(Faction.Color, secondToLastPlaceColor);
                string currentProposed = Faction.GetProposedTreatyForColor(secondToLastPlaceColor);
                
                // If they already have a cease fire, offer alliance
                if (currentTreaty.Equals(TREATY_CEASE_FIRE))
                {
                    // Only offer if we haven't already proposed alliance or better
                    if (!currentProposed.Equals(TREATY_ALLIANCE) && !currentProposed.Equals(TREATY_TEAM_MATES))
                    {
                        Faction.SetProposedTreatyForColor(secondToLastPlaceColor, TREATY_ALLIANCE);
                        Globals.Log($"offerTreaties(): last place AI {Faction.Color} offering alliance to 2nd-to-last place AI {secondToLastPlaceColor}");
                    }
                }
                // Otherwise, offer cease fire if currently at war
                else if (currentProposed.Equals(TREATY_AT_WAR) || currentTreaty.Equals(TREATY_AT_WAR))
                {
                    Faction.SetProposedTreatyForColor(secondToLastPlaceColor, TREATY_CEASE_FIRE);
                    Globals.Log($"offerTreaties(): last place AI {Faction.Color} offering cease fire to 2nd-to-last place AI {secondToLastPlaceColor}");
                }
            }
        }
        // If active faction count == 2, AI will offer all factions a previous treaty proposal until it is at war with every other faction
        else if (activeFactionCount == 2)
        {
            foreach (string color in FACTION_COLORS)
            {
                if (color.Equals(Faction.Color))
                    continue;
                    
                Faction otherFaction = gameState.Factions.ColorToFaction[color];
                string currentTreaty = gameState.Factions.GetCurrentTreaty(Faction.Color, color);
                string currentProposed = Faction.GetProposedTreatyForColor(color);
                
                // If not already at war, propose the previous (lower) treaty level
                if (!currentTreaty.Equals(TREATY_AT_WAR))
                {
                    string previousTreaty = getPreviousTreatyLevel(currentTreaty);
                    // Only propose if we haven't already proposed this level or lower
                    if (!currentProposed.Equals(previousTreaty) && !currentProposed.Equals(TREATY_AT_WAR))
                    {
                        Faction.SetProposedTreatyForColor(color, previousTreaty);
                        Globals.Log($"offerTreaties(): AI {Faction.Color} downgrading treaty with {color} from {currentTreaty} to {previousTreaty}");
                    }
                }
                else if (!currentProposed.Equals(TREATY_AT_WAR))
                {
                    // Current treaty is war but proposed is not, set proposed to war
                    Faction.SetProposedTreatyForColor(color, TREATY_AT_WAR);
                    Globals.Log($"offerTreaties(): AI {Faction.Color} setting proposed treaty with {color} to war");
                }
            }
        }
    }

    private void acceptTreaties()
    {
        int activeFactionCount = gameState.GetActiveFactionCount();
        int secondPlaceIndex = 1;
        int secondToLastPlaceScoreIndex = 2;
        int lastPlaceScoreIndex = 3;
        bool mustBeAiOffer = false;
        if (activeFactionCount == 3)
        {
            secondToLastPlaceScoreIndex = 1;
            lastPlaceScoreIndex = 2;
            mustBeAiOffer = true;
        }

        // If active faction count == 3, an AI faction with the 2nd-to-last place score will accept a treaty from an AI faction with the last place score
        // If active faction count == 4, an AI faction with the 2nd-to-last place score will accept a treaty from an AI faction with the last place score
        if (activeFactionCount >= 3)
        {
            // Get all active factions sorted by score
            List<(string color, int score, bool isAi)> activeFactions = new List<(string, int, bool)>();
            foreach (string color in FACTION_COLORS)
            {
                Faction faction = gameState.Factions.ColorToFaction[color];
                // Check if faction is active
                bool isActive = faction.HasComCen || gameState.GameSettings.CanLoseComCen;
                bool isAi = faction.Player == null || !faction.Player.IsHuman;
                
                if (isActive)
                {
                    activeFactions.Add((color, faction.CombinedScore, isAi));
                }
            }
            
            // Sort by score descending (highest first)
            activeFactions.Sort((a, b) => b.score.CompareTo(a.score));
            
            // Check if 2nd-to-last place is AI and last place is AI
            string secondToLastPlaceColor = activeFactions[secondToLastPlaceScoreIndex].color;
            bool secondToLastPlaceIsAi = activeFactions[secondToLastPlaceScoreIndex].isAi;
            string lastPlaceColor = activeFactions[lastPlaceScoreIndex].color;
            bool lastPlaceIsAi = activeFactions[lastPlaceScoreIndex].isAi;
            string secondPlaceColor = activeFactions[secondPlaceIndex].color;
            bool secondPlaceIsAi = activeFactions[secondPlaceIndex].isAi;
            
            // If this AI is the 2nd-to-last place AI, accept any treaty proposal from the last place
            if (Faction.Color.Equals(secondToLastPlaceColor) && secondToLastPlaceIsAi && 
                (!mustBeAiOffer || (mustBeAiOffer && lastPlaceIsAi)))
            {
                Faction lastPlaceFaction = gameState.Factions.ColorToFaction[lastPlaceColor];
                string proposedTreaty = lastPlaceFaction.GetProposedTreatyForColor(Faction.Color);
                
                // If there's a proposed treaty (not at war) and it's not already matched, match it
                if (!proposedTreaty.Equals(TREATY_AT_WAR))
                {
                    string currentProposed = Faction.GetProposedTreatyForColor(lastPlaceColor);
                    if (!currentProposed.Equals(proposedTreaty))
                    {
                        Faction.SetProposedTreatyForColor(lastPlaceColor, proposedTreaty);
                        Globals.Log($"acceptTreaties(): 2nd-to-last place AI {Faction.Color} accepting treaty {proposedTreaty} from last place {lastPlaceColor}");
                    }
                }
            }

            // If this AI is the last place AI, accept any treaty proposal from the 2nd-to-last place
            if (Faction.Color.Equals(lastPlaceColor) && lastPlaceIsAi &&
                (!mustBeAiOffer || (mustBeAiOffer && secondToLastPlaceIsAi)))
            {
                Faction secondToLastPlaceFaction = gameState.Factions.ColorToFaction[secondToLastPlaceColor];
                string proposedTreaty = secondToLastPlaceFaction.GetProposedTreatyForColor(Faction.Color);
                
                // If there's a proposed treaty (not at war) and it's not already matched, match it
                if (!proposedTreaty.Equals(TREATY_AT_WAR))
                {
                    string currentProposed = Faction.GetProposedTreatyForColor(secondToLastPlaceColor);
                    if (!currentProposed.Equals(proposedTreaty))
                    {
                        Faction.SetProposedTreatyForColor(secondToLastPlaceColor, proposedTreaty);
                        Globals.Log($"acceptTreaties(): 2nd-to-last place AI {Faction.Color} accepting treaty {proposedTreaty} from last place {secondToLastPlaceColor}");
                    }
                }
            }

            if (activeFactionCount == 4)
            {
                // If this AI is the 2nd-to-last place AI or last place AI accept any treaty proposal from any other AI
                if (((Faction.Color.Equals(secondToLastPlaceColor) && secondToLastPlaceIsAi) ||
                    (Faction.Color.Equals(lastPlaceColor) && lastPlaceIsAi)) && 
                    secondPlaceIsAi)
                {
                    Faction secondPlaceFaction = gameState.Factions.ColorToFaction[secondPlaceColor];
                    string proposedTreaty = secondPlaceFaction.GetProposedTreatyForColor(Faction.Color);
                    
                    // If there's a proposed treaty (not at war) and it's not already matched, match it
                    if (!proposedTreaty.Equals(TREATY_AT_WAR))
                    {
                        string currentProposed = Faction.GetProposedTreatyForColor(secondPlaceColor);
                        if (!currentProposed.Equals(proposedTreaty))
                        {
                            Faction.SetProposedTreatyForColor(secondPlaceColor, proposedTreaty);
                            Globals.Log($"acceptTreaties(): AI {Faction.Color} accepting treaty {proposedTreaty} from second place AI {secondPlaceColor}");
                        }
                    }
                }                
            }

        }

        // If active faction count > 2, a preferred AI team-mate will match any proposed treaty from its human team-mate
        if (activeFactionCount > 2)
        {
            // Check all factions to see if any human has this AI as their preferred team-mate
            foreach (string color in FACTION_COLORS)
            {
                if (color.Equals(Faction.Color))
                    continue;
                    
                Faction otherFaction = gameState.Factions.ColorToFaction[color];
                
                // Check if this other faction is human and has this AI as preferred team-mate
                bool isHumanFaction = otherFaction.Player != null && otherFaction.Player.IsHuman;
                bool isPreferredTeamMate = otherFaction.PreferredTeamMateColor != null && 
                                           otherFaction.PreferredTeamMateColor.Equals(Faction.Color);
                
                if (isHumanFaction && isPreferredTeamMate)
                {
                    // Get the proposed treaty from the human
                    string proposedTreaty = otherFaction.GetProposedTreatyForColor(Faction.Color);
                    
                    // If there's a proposed treaty (not at war) and it's not already matched, match it
                    if (!proposedTreaty.Equals(TREATY_AT_WAR))
                    {
                        string currentProposed = Faction.GetProposedTreatyForColor(color);
                        if (!currentProposed.Equals(proposedTreaty))
                        {
                            Faction.SetProposedTreatyForColor(color, proposedTreaty);
                            Globals.Log($"acceptTreaties(): Preferred AI team-mate {Faction.Color} automatically matched treaty {proposedTreaty} from human {color}");
                        }
                    }
                }
            }
        }
    }

    private string getPreviousTreatyLevel(string currentTreaty)
    {
        switch (currentTreaty)
        {
            case TREATY_TEAM_MATES:
                return TREATY_ALLIANCE;
            case TREATY_ALLIANCE:
                return TREATY_CEASE_FIRE;
            case TREATY_CEASE_FIRE:
                return TREATY_AT_WAR;
            default:
                return TREATY_AT_WAR; // fallback
        }
    }

    private bool hasTeamMatesTreatyWithHuman()
    {
        // Check if this AI faction has TREATY_TEAM_MATES with any human player
        foreach (string color in FACTION_COLORS)
        {
            if (color.Equals(Faction.Color))
                continue;
                
            Faction otherFaction = gameState.Factions.ColorToFaction[color];
            
            // Check if the other faction is human
            bool isHumanFaction = otherFaction.Player != null && otherFaction.Player.IsHuman;
            
            if (isHumanFaction)
            {
                // Check if we have TREATY_TEAM_MATES with this human
                string currentTreaty = gameState.Factions.GetCurrentTreaty(Faction.Color, color);
                if (currentTreaty.Equals(TREATY_TEAM_MATES))
                {
                    return true;
                }
            }
        }
        return false;
    }

    private void addGoals()
    {
        if (gameState == null)
        {
            Globals.Log("addGoals(): gameState is null");
            return;
        }
        if (gameState.Burbs == null)
        {
            Globals.Log("addGoals(): gameState.Burbs is null");
            return;
        }
        if (gameState == null)
        {
            Globals.Log("addGoals(): gameState.Burbs.NameToBurb is null");
            return;
        }

        foreach (string key in gameState.Burbs.NameToBurb.Keys)
        {
            Burb burb = gameState.Burbs.NameToBurb[key];
            MapHex mapHex = map.Hexes[burb.Y, burb.X];
            if (mapHex.Visibility[Faction.Color] && !burb.OwnerColor.Equals(Faction.Color))
            {
                createConquerBurbGoal(mapHex);
            }
        }

        foreach (Resource resource in gameState.Resources)
        {
            if (resource.IsVisibleToColor(Faction.Color) && !resource.OwnerColor.Equals(Faction.Color))
            {
                createConquerResource(resource);
            }
        }
    }

    public void processGoals()
    {
        List<AiGoal> goalsToKeep = new List<AiGoal>();

        // Prioritize conquest goals and sort them.
        conquestGoals.Clear();
        List<AiGoal> sortedConquestGoalsAsc = prioritizeConquestGoals();

        AiGoal bestConquestGoal = null;
        AiGoal nextBestConquestGoal = null;
        AiGoal randomGoal = null;
        if (sortedConquestGoalsAsc.Count > 0)
        {
            bestConquestGoal = sortedConquestGoalsAsc[0];
            Globals.Log("processGoals(): best conquest goal for " + Faction.Color + " is " + bestConquestGoal);
        }
        if (sortedConquestGoalsAsc.Count > 1)
        {
            nextBestConquestGoal = sortedConquestGoalsAsc[1];
            Globals.Log("processGoals(): next best goal for " + Faction.Color + " is " + nextBestConquestGoal);
        }
        // Pick a random goal
        if (goals.Count > 0)
        {
            int index = random.Next(0, goals.Count);
            randomGoal = goals[index];
            // If we pick a conquest goal, switch to the next best goal or best goal.
            if (AI_GOAL_CONQUER.Equals(randomGoal.Type))
            {
                if (nextBestConquestGoal != null)
                    randomGoal = nextBestConquestGoal;
                else if (bestConquestGoal != null)
                    randomGoal = bestConquestGoal;
            }
            else if (AI_GOAL_EXPLORE.Equals(randomGoal.Type))
            {
                index = random.Next(0, exploreGoals.Count);
                randomGoal = exploreGoals[index];
            }
            Globals.Log("processGoals(): random goal for " + Faction.Color + " is " + randomGoal + ", goalCount=" + goals.Count);
        }

        if (randomGoal != null)
            processGoal(goalsToKeep, randomGoal, true);

        if (bestConquestGoal != null)
        {
            assignAvailableUnitsToGoal(bestConquestGoal);
            processGoal(goalsToKeep, bestConquestGoal, true);
        }


        // Finally loop through goals and see what can be done.
        goalsToKeep.Clear();
        List<AiGoal> goalsToProcess = new List<AiGoal>(goals);
        foreach (AiGoal goal in goalsToProcess)
        {
            if (Faction.Money > 70)
            {
                processGoal(goalsToKeep, goal, true, true);
            }
            else
            {
                processGoal(goalsToKeep, goal, false, false);
            }
        }
        goals = goalsToKeep;
    }

    private List<AiGoal> prioritizeConquestGoals()
    {
        List<AiGoal> sortedConquestGoalsAsc = new List<AiGoal>();
        //HashSet<string> conquestGoalsInProgress = new HashSet<string>();

        // Prioritize conquest goals and sort them.
        conquestGoals.Clear();
        foreach (AiGoal goal in goals)
        {
            if (AI_GOAL_CONQUER.Equals(goal.Type) && !goal.IsComplete)
            {
                conquestGoals.Add(goal);
                float goalDistance = map.calculateDistance(myMetroHex, goal.TargetMapHex);
                float difficulty = goalDistance * 10;
                HashSet<MapHex> hexesInRange = map.getMapHexesInRange(goal.TargetMapHex, 2);
                foreach (MapHex hex in hexesInRange)
                {
                    Unit unit = hex.getUnit();
                    if (unit != null)
                    {
                        UnitType unitType = gameState.UnitTypes.UnitTypeMap[unit.UnitType];
                        difficulty += 1;
                        difficulty += (unit.StrengthPoints / 10);
                        difficulty += (25 - unitType.BattleDamageFromAttacker[INFANTRY]);
                        difficulty += unitType.BattleDamageToDefender[INFANTRY];
                    }
                }
                foreach (AiUnit aiUnit in goal.DesiredUnits)
                {
                    UnitType unitType = gameState.UnitTypes.UnitTypeMap[aiUnit.UnitType];
                    {
                        difficulty += unitType.Cost;
                    }
                }
                if (goal.IsGoalStarted)
                {
                    //conquestGoalsInProgress.Add(goal.GoalName());
                    foreach (AiUnit aiUnit in goal.ActualUnits)
                    {
                        UnitType unitType = gameState.UnitTypes.UnitTypeMap[aiUnit.UnitType];
                        if (aiUnit.Unit != null)
                        {
                            difficulty -= 1;
                            difficulty -= (aiUnit.Unit.StrengthPoints / 10);
                            difficulty -= unitType.Cost;
                        }
                    }
                }
                // Add 1000 to difficulty if target is a burb owned by a faction with a non-war treaty
                if (goal.TargetMapHex.Burb != null && goal.TargetMapHex.Burb.OwnerColor != null)
                {
                    string burbOwnerColor = goal.TargetMapHex.Burb.OwnerColor;
                    if (!burbOwnerColor.Equals(Faction.Color) && !burbOwnerColor.Equals(NATIVE_COLOR))
                    {
                        string currentTreaty = gameState.Factions.GetCurrentTreaty(Faction.Color, burbOwnerColor);
                        if (!currentTreaty.Equals(TREATY_AT_WAR))
                        {
                            difficulty += 1000;
                            Globals.Log("prioritizeConquestGoals(): Added 1000 to difficulty for burb with non-war treaty: " + goal);
                        }
                    }
                }
                goal.DifficultyScore = (int)Math.Round(difficulty);
                Globals.Log("prioritizeConquestGoals(): calculated difficulty for goal=" + goal);
            }
        }
        sortedConquestGoalsAsc = conquestGoals.OrderBy(g => g.DifficultyScore).ToList();
        return sortedConquestGoalsAsc;
    }

    // Make sure the available units pool doesn't have dead units in it.
    private void checkAvailableUnits()
    {
        foreach (string key in unitTypeToAvailableUnits.Keys)
        {
            HashSet<AiUnit> availableAiUnits = unitTypeToAvailableUnits[key];
            HashSet<AiUnit> availableAiUnitsCopy = new HashSet<AiUnit>(availableAiUnits);
            foreach (AiUnit aiUnit in availableAiUnitsCopy)
            {
                if (aiUnit.Unit == null || aiUnit.Unit.StrengthPoints <= 0)
                {
                    availableAiUnits.Remove(aiUnit);
                }
            }
            Globals.Log("checkAvailableUnits(): " + key + ": " + availableAiUnitsCopy.Count);
        }
    }

    private void assignAvailableUnitsToGoal(AiGoal goal)
    {
        bool availableUnits = true;
        AiUnit aiUnit = goal.getNextUnitToBuild(true);
        while (aiUnit != null && availableUnits)
        {
            //UnitType unitType = gameState.UnitTypes.UnitTypeMap[aiUnit.UnitType];
            Unit availableUnit = getUnitFromAvailableUnits(aiUnit.UnitType);
            if (availableUnit != null && availableUnit.StrengthPoints > 0)
            {
                goal.IsGoalStarted = true;
                aiUnit.Unit = availableUnit;
                unitIdToAiUnit[availableUnit.Id] = aiUnit;
                aiUnit.UnitType = availableUnit.UnitType;
                goal.ActualUnits.Add(aiUnit);
                Globals.Log("assignAvailableUnitsToGoal(): assigned " + availableUnit.Id + " for " + goal);
            }
            else
            {
                availableUnits = false;
            }

            aiUnit = goal.getNextUnitToBuild(true);
        }

    }

    private void moveUnitsAwayFromMetro()
    {
        Globals.Log("moveUnitsAwayFromMetro(): enter");
        int count = 0;

        // HashSet<MapHex> metroNeighborHexes = map.getMapHexesInRange(myMetroHex, 2);
        // foreach (MapHex mapHex in metroNeighborHexes)
        // {
        //     Unit unit = mapHex.getUnit();
        //     if (unit != null && unit.Color.Equals(myMetroHex.Burb.Color) &&
        //         !unit.UnitType.Equals(COMMAND_CENTER) &&
        //         !unit.UnitType.Equals(SPY) &&
        //         !(unit.X == myMetroHex.X && unit.Y == myMetroHex.Y))
        //     {
        //         if (unit.ActionQueue.Count <= 0)
        //         {
        //             randomMovement(unit);
        //             count += 1;
        //         }
        //     }
        // }

        HashSet<MapHex> metroNeighborHexes = map.getMapHexesInRange(myMetroHex, 3);
        foreach (MapHex mapHex in metroNeighborHexes)
        {
            Unit unit = mapHex.getUnit();
            if (unit != null && unit.Color.Equals(myMetroHex.Burb.Color) &&
                !unit.UnitType.Equals(COMMAND_CENTER) &&
                !unit.UnitType.Equals(SPY) &&
                !unit.UnitType.Equals(AIRCRAFT_CARRIER) &&
                !(unit.X == myMetroHex.X && unit.Y == myMetroHex.Y))
            {
                if (unit.ActionQueue.Count <= 0)
                {
                    randomMovement(unit);
                    count += 1;
                }
            }
        }
        Globals.Log("moveUnitsAwayFromMetro(): exit: count=" + count);
    }


    private void checkForStuckUnits()
    {
        Globals.Log("checkForStuckUnits(): enter");
        int count = 0;
        for (int y = 0; y < map.Y; y++)
        {
            for (int x = 0; x < map.X; x++)
            {
                MapHex mapHex = map.Hexes[y, x];
                Unit unit = mapHex.getUnit();
                if (unit != null &&
                    unit.Color.Equals(Faction.Color) &&
                    !unit.UnitType.Equals(COMMAND_CENTER) &&
                    !unit.UnitType.Equals(SPY))
                {
                    if (unitIdToAiUnit.ContainsKey(unit.Id))
                    {
                        AiUnit aiUnit = unitIdToAiUnit[unit.Id];
                        if (unit.X != mapHex.X || unit.Y != mapHex.Y)
                        {
                            Globals.Log("checkForStuckUnits(): warning x,y mismatch: " + unit.X + "," + unit.Y + " vs " + mapHex.X + "," + mapHex.Y);
                            unit.X = mapHex.X;
                            unit.Y = mapHex.Y;
                        }
                        checkForLazyUnit(aiUnit);
                        if (checkForBlockedUnit(aiUnit))
                        {
                            //Globals.Log("checkForStuckUnits(): aiUnit was stuck: " + aiUnit + " " + x + "," + y);
                            count += 1;
                        }
                        else
                        {
                            //Globals.Log("checkForStuckUnits(): aiUnit was not stuck: " + aiUnit + " " + x + "," + y);
                        }
                    }
                    else
                    {
                        Globals.Log("checkForStuckUnits(): could not find aiUnit for " + unit.Id + " at " + x + "," + y);
                    }
                }
            }
        }
        Globals.Log("checkForStuckUnits(): exit: count=" + count);
    }

    public void processGoal(List<AiGoal> goalsToKeep, AiGoal aiGoal, bool IsLog = false, bool spendMoney = true)
    {
        bool isFinished = evaluateGoal(aiGoal, IsLog);
        aiGoal.IsComplete = isFinished;
        if (!isFinished)
        {
            Unit unit = null;
            if (spendMoney)
                unit = buildUnits(aiGoal, IsLog);
            // Don't move units if AI has TREATY_TEAM_MATES with a human player
            int moveCount = 0;
            if (!hasTeamMatesTreatyWithHuman())
            {
                moveCount = moveUnits(aiGoal);
            }
            goalsToKeep.Add(aiGoal);
            if (unit != null || moveCount > 0)
                Globals.Log("processGoal(): remaining goal for " + Faction.Color + " is " + aiGoal);
        }
    }

    private bool evaluateGoal(AiGoal goal, bool IsLog = false)
    {
        //Globals.Log("evaluateGoal(): " + goal + ": desiredUnits=" + goal.DesiredUnits.Count + ", actualUnits=" + goal.ActualUnits);
        if (goal.IsOngoingGoal)
            return false;

        // goal is complete
        if (AI_GOAL_CONQUER.Equals(goal.Type) && goal.TargetMapHex.Burb != null && goal.TargetMapHex.Burb.OwnerColor.Equals(Faction.Color))
        {
            Globals.Log("Ai.evaluateGoal(): goal complete: " + goal);
            if (targetXyToGoal.ContainsKey(goal.TargetMapHex.X + "," + goal.TargetMapHex.Y))
                targetXyToGoal.Remove(goal.TargetMapHex.X + "," + goal.TargetMapHex.Y);
            createDefendBurbGoal(goal.TargetMapHex);
            HashSet<AiUnit> availableUnits = new HashSet<AiUnit>(goal.ActualUnits);
            foreach (AiUnit availableAiUnit in availableUnits)
            {
                Unit unit = availableAiUnit.Unit;
                // If the unit is in the burb, leave it there and don't add it to the available units pool
                if (unit != null && unit.X == goal.TargetMapHex.X && unit.Y == goal.TargetMapHex.Y)
                {
                    string unitType = unit.UnitType;
                    if (TRANSPORT_INFANTRY.Equals(unitType) || DUG_IN_INFANTRY.Equals(unitType))
                        unitType = INFANTRY;
                    if (!unitTypeToAvailableUnits.ContainsKey(unitType))
                        unitTypeToAvailableUnits[unitType] = new HashSet<AiUnit>();
                    continue;
                }

                // Remove the unit from the goal and add it to the available units pool
                if (unit != null)
                {
                    string unitType = unit.UnitType;
                    if (TRANSPORT_INFANTRY.Equals(unitType) || DUG_IN_INFANTRY.Equals(unitType))
                        unitType = INFANTRY;
                    if (!unitTypeToAvailableUnits.ContainsKey(unitType))
                        unitTypeToAvailableUnits[unitType] = new HashSet<AiUnit>();
                    goal.ActualUnits.Remove(availableAiUnit);
                    MapHex unitHex = map.Hexes[unit.Y, unit.X];
                    bool isBurbCenter = false;
                    if (unitHex.Burb != null)
                        isBurbCenter = unitHex.Burb.IsBurbCenter();
                    if (unit.StrengthPoints > 0 && !isBurbCenter)
                        unitTypeToAvailableUnits[unitType].Add(availableAiUnit);
                    else
                        unit.ActionQueue.Clear();
                }
            }
            return true;
        }

        // Expand DesiredUnits if enemy count increases.
        if (AI_GOAL_CONQUER.Equals(goal.Type))
        {
            if (IsBurbCoastal(goal.TargetMapHex))
            {
                updateDesiredUnitsForCoastalBurbGoal(goal);
            }
            else
            {
                updateDesiredUnitsForInteriorBurbGoal(goal);
            }
        }
        AiUnit aiUnit = goal.getNextUnitToBuild(IsLog);

        // if (AI_GOAL_BUILD_CARRIER.Equals(goal.Type) && (aiUnit == null || (AIRPLANE.Equals(aiUnit.UnitType) && myMetroHex.Airplane != null)))
        // {
        //     AiUnit aiCarrier = goal.GetActualUnit(AIRCRAFT_CARRIER);
        //     if (aiCarrier == null || aiCarrier.Unit == null)
        //         return false;
        //     Unit plane = myMetroHex.Airplane;
        //     if (plane == null)
        //         return false;
        //     if (plane != null && this.unitIdToAiUnit.ContainsKey(plane.Id))
        //     {
        //         AiUnit aiPlane = this.unitIdToAiUnit[plane.Id];
        //         // Planes behave the same no matter what their goals.
        //         // Remove the plane from the defend metro goal, so another one gets built.
        //         if (this.targetXyToGoal.ContainsKey(aiPlane.GoalTargetXy))
        //         {
        //             AiGoal otherGoal = targetXyToGoal[aiPlane.GoalTargetXy];
        //             otherGoal.ActualUnits.Remove(aiPlane);
        //         }
        //         // Add the plane to the default goal.
        //         defaultGoal.ActualUnits.Add(aiPlane);
        //     }
        //     if (aiCarrier.Unit.Airplane == null)
        //     {
        //         TransferAction action = new TransferAction();
        //         action.ClassType = "GlobalConquest.Actions.TransferAction";
        //         action.ClientIdentifier = Faction.Color;
        //         action.Plane = plane;
        //         action.DestinationX = aiCarrier.Unit.X;
        //         action.DestinationY = aiCarrier.Unit.Y;
        //         action.execute(Server);
        //     }
        //     goal.IsComplete = true;
        //     if (!unitTypeToAvailableUnits.ContainsKey(AIRCRAFT_CARRIER))
        //         unitTypeToAvailableUnits[AIRCRAFT_CARRIER] = new HashSet<AiUnit>();
        //     unitTypeToAvailableUnits[AIRCRAFT_CARRIER].Add(aiCarrier);
        //     Globals.Log("evaluateGoal(): Carrier with plane available for goals.");
        //     createBuildCarrierGoal();
        //     return true;
        // }

        // Build is complete for goal b/c there is nothing needed from above.
        if (aiUnit == null && AI_GOAL_CONQUER.Equals(goal.Type))
        {
            Globals.Log("Ai.evaluateGoal(): build ready for : " + goal);
            bool isInPosition = true;
            foreach (AiUnit builtAiUnit in goal.ActualUnits)
            {
                if (!IsUnitInPosition(goal, builtAiUnit))
                {
                    isInPosition = false;
                    break;
                }
            }
            // randomGo=25%
            int randomGo = random.Next(0, 4);
            if (isInPosition || randomGo < 1)
            {
                // Check if target is a burb owned by a faction with a non-war treaty
                bool shouldBlockMove = false;
                if (goal.TargetMapHex.Burb != null && goal.TargetMapHex.Burb.OwnerColor != null)
                {
                    string burbOwnerColor = goal.TargetMapHex.Burb.OwnerColor;
                    if (!burbOwnerColor.Equals(Faction.Color) && !burbOwnerColor.Equals(NATIVE_COLOR))
                    {
                        string currentTreaty = gameState.Factions.GetCurrentTreaty(Faction.Color, burbOwnerColor);
                        if (!currentTreaty.Equals(TREATY_AT_WAR))
                        {
                            shouldBlockMove = true;
                            Globals.Log("Ai.evaluateGoal(): Blocking move to target burb owned by faction with non-war treaty: " + goal);
                        }
                    }
                }
                
                if (!shouldBlockMove)
                {
                    goal.ShouldMoveToTarget = true;
                    foreach (AiUnit builtAiUnit in goal.ActualUnits)
                    {
                        builtAiUnit.ShouldMoveToTarget = true;
                    }
                    if (goal.ActualUnits.Count > 0)
                        Globals.Log("Ai.evaluateGoal(): ShouldMoveToTarget, attack ready for " + goal);
                }
            }
        }
        else if (goal.ActualUnits.Count + 3 < goal.DesiredUnits.Count && goal.IsGoalStarted)
        {
            goal.ShouldMoveToTarget = false;
            bool IsMoveToTarget = true;
            // Attack already in progress, but failed and AI needs to mass troops again.
            foreach (AiUnit builtAiUnit in goal.ActualUnits)
            {
                if (!builtAiUnit.ShouldMoveToTarget)
                    IsMoveToTarget = false;
                builtAiUnit.ShouldMoveToTarget = false;
            }
            if (IsMoveToTarget)
                Globals.Log("Ai.evaluateGoal(): reset for new assault: " + goal);
        }
        return false;
    }

    private Unit buildUnits(AiGoal goal, bool IsLog = false)
    {
        int shouldBuild = 1;
        if (Faction.Money < 45)
            shouldBuild = random.Next(0, 20);
        else if (Faction.Money < 35)
            shouldBuild = random.Next(0, 10);
        if (shouldBuild == 0)
        {
            Globals.Log("buildUnits(): skipping to save money");
            return null;
        }
        AiUnit aiUnit = goal.getNextUnitToBuild(IsLog);
        if (aiUnit == null)
            return null;
        UnitType unitType = gameState.UnitTypes.UnitTypeMap[aiUnit.UnitType];
        Unit newUnit = null;
        if (AI_GOAL_DEFEND.Equals(goal.Type) && aiUnit.InitialPosition != null && aiUnit.InitialPosition.X == myMetroHex.X && aiUnit.InitialPosition.Y == myMetroHex.Y)
        {
            // I think this block is only used to place an infantry in the center.
            if (TERRAIN_SEA.Equals(unitType.LandOrSea))
                newUnit = purchaseUnitAtMetroDock(aiUnit.UnitType);
            else
                newUnit = purchaseUnitAtMetro(aiUnit.UnitType);
            if (newUnit != null)
                Globals.Log("Ai.buildUnits(): " + newUnit.Id + " built to defend " + Faction.Color + " metro");
        }
        else if (AI_GOAL_DEFEND.Equals(goal.Type) && aiUnit.InitialPosition != null && aiUnit.InitialPosition.X == goal.TargetMapHex.X && aiUnit.InitialPosition.Y == goal.TargetMapHex.Y)
        {
            // Initially captured burbs will not have any offensive capbilities.
            if (TERRAIN_SEA.Equals(unitType.LandOrSea))
                newUnit = purchaseUnitAtBurbDock(aiUnit.InitialPosition, aiUnit.UnitType);
            else
                newUnit = purchaseUnitAtBurb(aiUnit.InitialPosition, aiUnit.UnitType);
            if (newUnit != null)
                Globals.Log("Ai.buildUnits(): Burb-InitialPosition " + newUnit.Id + " built for " + goal);
        }
        else if (AI_GOAL_DEFEND.Equals(goal.Type) && aiUnit.InitialPosition != null)
        {
            // Initially captured burbs will not have any offensive capbilities.
            newUnit = purchaseUnitAtBurbDock(aiUnit.InitialPosition, aiUnit.UnitType);
            if (newUnit != null)
            {
                Globals.Log("Ai.buildUnits(): InitialPosition " + newUnit.Id + " built for " + goal);
                moveUnit(unitType, newUnit, aiUnit.InitialPosition);
            }
        }
        else if (AI_GOAL_DEFEND.Equals(goal.Type) && aiUnit.InitialPosition == null && aiUnit.DistanceFromTarget > 1)
        {
            Unit availableUnit = getUnitFromAvailableUnits(aiUnit.UnitType);
            if (availableUnit != null && availableUnit.StrengthPoints > 0)
            {
                newUnit = availableUnit;
            }
            else
            {
                newUnit = purchaseUnitAtMetroDock(aiUnit.UnitType);
            }
            MapHex foundMapHex = findHexAroundBurb(goal, aiUnit);
            if (newUnit != null && foundMapHex != null)
            {
                Globals.Log("Ai.buildUnits(): " + newUnit.Id + " built around hex for " + goal);
                moveUnit(unitType, newUnit, foundMapHex);
            }
        }
        else if (AI_GOAL_BUILD_PLANE.Equals(goal.Type))
        {
            if (myMetroHex.Airplane == null)
            {
                newUnit = purchaseUnitAtMetro(aiUnit.UnitType);
                Globals.Log("Ai.buildUnits(): " + newUnit.Id + " built for " + goal);
            }
        }

        // else if (AI_GOAL_BUILD_CARRIER.Equals(goal.Type) && aiUnit.InitialPosition == null && aiUnit.DistanceFromTarget > 1)
        // {
        //     newUnit = purchaseUnitAtMetroDock(aiUnit.UnitType);

        //     MapHex foundMapHex = findHexAroundBurb(goal, aiUnit);
        //     if (newUnit != null && foundMapHex != null)
        //     {
        //         Globals.Log("Ai.buildUnits(): " + newUnit.Id + " built around hex for " + goal);
        //         moveUnit(unitType, newUnit, foundMapHex);
        //     }
        // }
        // else if (AI_GOAL_BUILD_CARRIER.Equals(goal.Type) && AIRPLANE.Equals(aiUnit.UnitType))
        // {
        //     if (myMetroHex.Airplane == null)
        //     {
        //         newUnit = purchaseUnitAtMetro(aiUnit.UnitType);
        //         Globals.Log("Ai.buildUnits(): " + newUnit.Id + " built for " + goal);
        //     }
        // }
                
        else if (AI_GOAL_CONQUER.Equals(goal.Type) && aiUnit.InitialPosition != null)
        {
            Unit availableUnit = getUnitFromAvailableUnits(aiUnit.UnitType);
            if (availableUnit != null && availableUnit.StrengthPoints > 0)
            {
                newUnit = availableUnit;
            }
            else
            {
                newUnit = purchaseUnitAtMetroDock(aiUnit.UnitType);
            }
            if (newUnit != null)
            {
                Globals.Log("Ai.buildUnits(): " + newUnit.Id + " used for goal:" + goal);
                moveUnit(unitType, newUnit, aiUnit.InitialPosition);
            }
        }
        else if (AI_GOAL_CONQUER.Equals(goal.Type) && aiUnit.InitialPosition == null && aiUnit.DistanceFromTarget > 1)
        {
            Unit availableUnit = getUnitFromAvailableUnits(aiUnit.UnitType);
            if (availableUnit != null && availableUnit.StrengthPoints > 0)
            {
                newUnit = availableUnit;
            }
            else
            {
                newUnit = purchaseUnitAtMetroDock(aiUnit.UnitType);
            }
            if (newUnit != null)
            {
                Globals.Log("Ai.buildUnits(): " + newUnit.Id + " used for goal:" + goal);
                MapHex foundMapHex = findHexAroundBurb(goal, aiUnit);
                moveUnit(unitType, newUnit, foundMapHex);
            }
        }
        else if (AI_GOAL_EXPLORE.Equals(goal.Type))
        {
            Unit availableUnit = getUnitFromAvailableUnits(aiUnit.UnitType);
            if (availableUnit != null && availableUnit.StrengthPoints > 0)
            {
                newUnit = availableUnit;
            }
            else
            {
                newUnit = purchaseUnitAtMetroDock(aiUnit.UnitType);
            }
            if (newUnit != null)
            {
                Globals.Log("Ai.buildUnits(): " + newUnit.Id + " used for goal: " + goal);
                MapHex targetMapHex = map.Hexes[goal.TargetMapHex.Y, goal.TargetMapHex.X];
                moveUnit(unitType, newUnit, targetMapHex);
            }
        }

        if (newUnit != null)
        {
            goal.IsGoalStarted = true;
            aiUnit.Unit = newUnit;
            unitIdToAiUnit[newUnit.Id] = aiUnit;
            string newUnitType = newUnit.UnitType;
            if (TRANSPORT_INFANTRY.Equals(newUnitType))
                newUnitType = INFANTRY;
            if (AIRCRAFT_CARRIER.Equals(newUnitType))
            {
                if (newUnit.Airplane == null && myMetroHex.Airplane != null)
                {
                    Unit plane = myMetroHex.Airplane;
                    TransferAction action = new TransferAction();
                    action.ClassType = "GlobalConquest.Actions.TransferAction";
                    action.ClientIdentifier = Faction.Color;
                    action.Plane = plane;
                    action.DestinationX = aiUnit.Unit.X;
                    action.DestinationY = aiUnit.Unit.Y;
                    action.execute(Server);

                    if (plane != null && this.unitIdToAiUnit.ContainsKey(plane.Id))
                    {
                        AiUnit aiPlane = this.unitIdToAiUnit[plane.Id];
                        // Planes behave the same no matter what their goals.
                        // Remove the plane from the defend metro goal, so another one gets built.
                        if (this.targetXyToGoal.ContainsKey(aiPlane.GoalTargetXy))
                        {
                            AiGoal otherGoal = targetXyToGoal[aiPlane.GoalTargetXy];
                            otherGoal.ActualUnits.Remove(aiPlane);
                        }
                        // Add the plane to the default goal.
                        defaultGoal.ActualUnits.Add(aiPlane);
                    }

                }
            }
            aiUnit.UnitType = newUnitType;
            if (!AIRPLANE.Equals(aiUnit.UnitType) || (AI_GOAL_DEFEND.Equals(goal.Type) && 
                goal.TargetMapHex != null && (goal.TargetMapHex.X != myMetroHex.X || goal.TargetMapHex.Y != myMetroHex.Y)))
                goal.ActualUnits.Add(aiUnit);
            else
                defaultGoal.ActualUnits.Add(aiUnit);
            Globals.Log("buildUnits(): unit " + newUnit.Id + " being used for " + goal);
            Globals.Log("buildUnits(): defaultGoalUnits=" + defaultGoal.ActualUnits.Count);
        }

        return newUnit;
    }

    private Unit getUnitFromAvailableUnits(string unitType)
    {
        Unit unit = null;
        AiUnit availableAiUnit = null;
        if (TRANSPORT_INFANTRY.Equals(unitType) || DUG_IN_INFANTRY.Equals(unitType))
            unitType = INFANTRY;
        if (unitTypeToAvailableUnits.ContainsKey(unitType))
        {
            List<AiUnit> availableAiUnits = unitTypeToAvailableUnits[unitType].ToList<AiUnit>();
            if (availableAiUnits.Count > 0)
            {
                if (availableAiUnits[0].Unit != null)
                {
                    availableAiUnit = availableAiUnits[0];
                    unitTypeToAvailableUnits[unitType].Remove(availableAiUnit);
                }
            }
        }
        if (availableAiUnit != null && availableAiUnit.Unit != null && availableAiUnit.Unit.StrengthPoints > 0)
        {
            unit = availableAiUnit.Unit;
            availableAiUnit.Unit = null;
        }
        if (unit != null)
            Globals.Log("getUnitFromAvailableUnits(): Found availableUnit " + unit.Id);
        return unit;
    }

    // Returns number of units moved.
    private int checkForBruteForceAssault(AiGoal goal)
    {
        int count = 0;
        // Brute force assault on burb.
        if (AI_GOAL_CONQUER.Equals(goal.Type) && goal.TargetMapHex != null && goal.TargetMapHex.Burb != null &&
            goal.ShouldMoveToTarget && !goal.IsComplete)
        {
            HashSet<MapHex> nearbyHexes = map.getMapHexesInRange(goal.TargetMapHex, 4);
            foreach (MapHex nearbyHex in nearbyHexes)
            {
                Unit unit = nearbyHex.getUnit();
                if (unit == null)
                    continue;
                unit.IsSneaking = false;
                UnitType unitType = gameState.UnitTypes.UnitTypeMap[unit.UnitType];
                if (unit.Color.Equals(Faction.Color) && (INFANTRY.Equals(unit.UnitType) || DUG_IN_INFANTRY.Equals(unit.UnitType) || TRANSPORT_INFANTRY.Equals(unit.UnitType)))
                {
                    moveUnit(unitType, unit, goal.TargetMapHex);
                    count += 1;
                    Globals.Log("Ai.moveUnits(): request assault by " + unit.Id + " for " + goal);
                }
                else if (unit.Color.Equals(Faction.Color) && TERRAIN_SEA.Equals(unitType.LandOrSea) && !TRANSPORT_INFANTRY.Equals(unit.UnitType))
                {
                    int distance = 3;
                    if (BURB_METROPLEX.Equals(goal.TargetMapHex.Burb.Type) && BATTLESHIP.Equals(unit.UnitType))
                        distance = 2;
                    else if (BURB_METROPLEX.Equals(goal.TargetMapHex.Burb.Type) && AIRCRAFT_CARRIER.Equals(unit.UnitType))
                        distance = 3;
                    MapHex nearbySeaHex = findHexAroundBurb(goal.TargetMapHex, unit, distance);
                    if (nearbySeaHex != null)
                    {
                        moveUnit(unitType, unit, nearbySeaHex);
                        count += 1;
                        Globals.Log("Ai.moveUnits(): request sea assault by " + unit.Id + " for " + goal);
                    }
                }
            }
        }
        return count;
    }

    // Returns number of units moved.
    private int moveUnits(AiGoal goal)
    {
        int count = checkForBruteForceAssault(goal);

        HashSet<AiUnit> actualUnitsCopy = new HashSet<AiUnit>(goal.ActualUnits);
        foreach (AiUnit aiUnit in actualUnitsCopy)
        {
            if (aiUnit == null)
                continue;
            if (aiUnit.Unit != null && aiUnit.Unit.StrengthPoints <= 0)
            {
                if (unitIdToAiUnit.ContainsKey(aiUnit.Unit.Id))
                    unitIdToAiUnit.Remove(aiUnit.Unit.Id);
                aiUnit.Unit = null;
                goal.ActualUnits.Remove(aiUnit);
            }
            if (aiUnit.Unit == null)
            {
                goal.ActualUnits.Remove(aiUnit);
                continue;
            }
            if (aiUnit.Unit != null)
            {
                MapHex unitHex = map.Hexes[aiUnit.Unit.Y, aiUnit.Unit.X];
                bool isBurbCenter = false;
                if (unitHex.Burb != null)
                    isBurbCenter = unitHex.Burb.IsBurbCenter();
                // If a unit is in the burb center, make it stay
                if (isBurbCenter && !SPY.Equals(aiUnit.Unit) && unitHex.Burb != null && 
                    unitHex.Burb.OwnerColor.Equals(aiUnit.Unit.Color))
                {
                    goal.ActualUnits.Remove(aiUnit);
                    aiUnit.Unit.ActionQueue.Clear();
                    continue;
                }
            }
            if (AIRPLANE.Equals(aiUnit.UnitType) && aiUnit.Unit != null)
            {
                flyMission(goal, aiUnit.Unit);
                continue;
            }
            if (aiUnit.Unit != null && aiUnit.Unit.Airplane != null)
            {
                flyMission(goal, aiUnit.Unit.Airplane);
            }
            UnitType unitType = gameState.UnitTypes.UnitTypeMap[aiUnit.UnitType];
            if (checkForBlockedUnit(aiUnit))
                continue;

            // TODO: figure out if there is only 1 enemy which has less than 40 strength -- 2 infantry
            //                                                     less than 30 strength -- 1 infantry
            if (AI_GOAL_CONQUER.Equals(goal.Type) && (goal.ShouldMoveToTarget || goal.Enemies == 0))
            {
                if (!TERRAIN_SEA.Equals(unitType.LandOrSea))
                {
                    Globals.Log("moveUnits(): ShouldMoveToTarget " + aiUnit.Unit.Id + " to " + goal.TargetMapHex.X + "," + goal.TargetMapHex.Y);
                    aiUnit.Unit.IsSneaking = false;
                    moveUnit(unitType, aiUnit.Unit, goal.TargetMapHex);
                }
                else
                {
                    int distance = 3;
                    Unit unit = aiUnit.Unit;
                    aiUnit.Unit.IsSneaking = false;
                    if (goal.TargetMapHex.Burb != null && BURB_METROPLEX.Equals(goal.TargetMapHex.Burb.Type) && 
                        BATTLESHIP.Equals(unit.UnitType))
                        distance = 2;
                    else if (goal.TargetMapHex.Burb != null && BURB_METROPLEX.Equals(goal.TargetMapHex.Burb.Type) && 
                             AIRCRAFT_CARRIER.Equals(unit.UnitType))
                        distance = 3;
                    MapHex nearbyHex = findHexAroundBurb(goal.TargetMapHex, aiUnit, distance);
                    if (nearbyHex != null)
                        moveUnit(unitType, aiUnit.Unit, nearbyHex);
                }
                count += 1;
            }
            else if (aiUnit.InitialPosition != null)
            {
                Globals.Log("moveUnits(): InitialPosition " + aiUnit.Unit.Id + " to " + aiUnit.InitialPosition.X + "," + aiUnit.InitialPosition.Y);
                if (!TERRAIN_SEA.Equals(unitType.LandOrSea))
                {
                    if (AI_GOAL_CONQUER.Equals(goal.Type) || TRANSPORT_INFANTRY.Equals(unitType.Name))
                        aiUnit.Unit.IsSneaking = true;
                    moveUnit(unitType, aiUnit.Unit, aiUnit.InitialPosition);
                }
                else
                {
                    if (TERRAIN_SEA.Equals(aiUnit.InitialPosition.Terrain) || TERRAIN_SWAMP.Equals(aiUnit.InitialPosition.Terrain) || "marsh".Equals(aiUnit.InitialPosition.Terrain))
                        moveUnit(unitType, aiUnit.Unit, aiUnit.InitialPosition);
                    else
                    {
                        int distance = 2;
                        MapHex nearbyHex = findHexAroundBurb(aiUnit.InitialPosition, aiUnit, distance);
                        if (nearbyHex != null && (TERRAIN_SEA.Equals(nearbyHex.Terrain) || TERRAIN_SWAMP.Equals(nearbyHex.Terrain) || "marsh".Equals(nearbyHex.Terrain)))
                            moveUnit(unitType, aiUnit.Unit, nearbyHex);
                    }
                }
                count += 1;
            }
            else if (aiUnit.InitialPosition == null && aiUnit.DistanceFromTarget > 1)
            {
                if (IsUnitInPosition(goal, aiUnit))
                {
                    if (AI_GOAL_CONQUER.Equals(goal.Type) && AIRCRAFT_CARRIER.Equals(unitType.Name))
                    {
                        aiUnit.Unit.IsSneaking = false;
                    }
                    continue;
                }
                //if (AI_GOAL_CONQUER.Equals(goal.Type) && AIRCRAFT_CARRIER.Equals(unitType.Name) && aiUnit.Unit.StrengthPoints < 100)
                //{
                //    aiUnit.Unit.IsSneaking = false;
                //}
                MapHex foundMapHex = findHexAroundBurb(goal, aiUnit);
                if (foundMapHex != null && aiUnit.Unit != null)
                {
                    Globals.Log("moveUnits(): DistanceFromTarget=" + aiUnit.DistanceFromTarget + ", " +
                        aiUnit.Unit.Id + " to " + foundMapHex.X + "," + foundMapHex.Y);
                    //if (AI_GOAL_CONQUER.Equals(goal.Type) &&
                    //    (AIRCRAFT_CARRIER.Equals(unitType.Name) && aiUnit.Unit.StrengthPoints == 100))
                    //{
                    //    aiUnit.Unit.IsSneaking = true;
                    //}
                    if (AI_GOAL_CONQUER.Equals(goal.Type) && (!TERRAIN_SEA.Equals(unitType.LandOrSea) ||
                        TRANSPORT_INFANTRY.Equals(unitType.Name)))
                    {
                        aiUnit.Unit.IsSneaking = true;
                    }
                    moveUnit(unitType, aiUnit.Unit, foundMapHex);
                    count += 1;
                }
            }
        }
        return count;
    }

    private bool checkForLazyUnit(AiUnit aiUnit)
    {
        if (aiUnit.Unit == null)
            return false;
        bool wasLazy = false;
        MapHex previousMapHex = aiUnit.LastMapHex;
        aiUnit.LastMapHex = map.Hexes[aiUnit.Unit.Y, aiUnit.Unit.X];
        UnitType unitType = gameState.UnitTypes.UnitTypeMap[aiUnit.UnitType];
        if (aiUnit.Unit != null && aiUnit.LastMapHex != null && previousMapHex != null &&
            aiUnit.Unit.ActionQueue.Count == 0)
        {
            if (previousMapHex.X != aiUnit.LastMapHex.X || previousMapHex.Y != aiUnit.LastMapHex.Y)
                return false;
            if (aiUnit.LastMapHex.Burb != null && aiUnit.LastMapHex.Burb.Name != null)
                return false;

            // Find out if the aiUnit is part of a goal.
            string goalTargetXy = aiUnit.GoalTargetXy;
            bool partOfOpenGoal = false;
            if (goalTargetXy != null && targetXyToGoal.ContainsKey(goalTargetXy))
            {
                AiGoal goal = targetXyToGoal[goalTargetXy];
                // If it is a defend goal, being lazy is fine.
                if (AI_GOAL_DEFEND.Equals(goal.Type))
                    return false;
                if (goal.IsComplete)
                    partOfOpenGoal = false;
                else
                    partOfOpenGoal = true;
            }
            // If it is not a part of a goal or the goal has completed,
            // make sure the aiUnit is part of the availableUnits pool.
            if (!partOfOpenGoal)
            {
                string unitTypeString = aiUnit.UnitType;
                if (TRANSPORT_INFANTRY.Equals(unitTypeString) || DUG_IN_INFANTRY.Equals(unitTypeString))
                    unitTypeString = INFANTRY;

                if (!unitTypeToAvailableUnits.ContainsKey(unitTypeString))
                {
                    unitTypeToAvailableUnits[unitTypeString] = new HashSet<AiUnit>();
                }
                HashSet<AiUnit> availableUnits = unitTypeToAvailableUnits[unitTypeString];
                if (!availableUnits.Contains(aiUnit))
                {
                    availableUnits.Add(aiUnit);
                    Globals.Log("checkForLazyUnit(): Added " + aiUnit + " to availableUnits.");
                }
            }

        }
        return wasLazy;

    }

    private bool checkForBlockedUnit(AiUnit aiUnit)
    {
        if (aiUnit.Unit == null)
            return false;
        bool wasBlocked = false;
        MapHex previousMapHex = aiUnit.LastMapHex;
        aiUnit.LastMapHex = map.Hexes[aiUnit.Unit.Y, aiUnit.Unit.X];
        UnitType unitType = gameState.UnitTypes.UnitTypeMap[aiUnit.UnitType];
        if (aiUnit.Unit != null && aiUnit.LastMapHex != null && previousMapHex != null &&
            aiUnit.Unit.ActionQueue.Count > 0 && aiUnit.Unit.MoveSteps > 0 &&
            !(aiUnit.Unit.X == aiUnit.Unit.ActionQueue[aiUnit.Unit.ActionQueue.Count - 1].TargetX && aiUnit.Unit.Y == aiUnit.Unit.ActionQueue[aiUnit.Unit.ActionQueue.Count - 1].TargetY))
        {
            if (previousMapHex.X != aiUnit.LastMapHex.X || previousMapHex.Y != aiUnit.LastMapHex.Y)
            {
                aiUnit.BlockedRounds = 0;
                return false;
            }
            else
            {
                aiUnit.BlockedRounds += 1;
            }

            if (previousMapHex.X == aiUnit.LastMapHex.X && previousMapHex.Y == aiUnit.LastMapHex.Y &&
                (!aiUnit.Unit.IsLoading && !aiUnit.Unit.IsUnloading || aiUnit.BlockedRounds >= 12))
            {
                //if (aiUnit.BlockedRounds >= 8)
                //{
                //    randomMovement(aiUnit.Unit);
                //    Globals.Log("checkForBlockedUnit(): Unblocking unit " + aiUnit.Unit.Id + " at " + aiUnit.Unit.X + "," + aiUnit.Unit.Y + " with random move");
                //    wasBlocked = true;
                //    aiUnit.BlockedRounds = 0;
                //}
                if (aiUnit.BlockedRounds >= 4)
                {
                    Globals.Log("checkForBlockedUnit(): Unblocking unit " + aiUnit.Unit.Id + " at " + aiUnit.Unit.X + "," + aiUnit.Unit.Y);
                    UnitAction firstMoveAction = null;
                    UnitAction lastMoveAction = null;
                    if (aiUnit.Unit.ActionQueue.Count > 0)
                    {
                        firstMoveAction = aiUnit.Unit.ActionQueue[0];
                        lastMoveAction = aiUnit.Unit.ActionQueue[aiUnit.Unit.ActionQueue.Count - 1];
                        aiUnit.Unit.ActionQueue.Clear();
                        MapHex destinationHex = map.Hexes[lastMoveAction.TargetY, lastMoveAction.TargetX];
                        moveUnit(unitType, aiUnit.Unit, destinationHex);
                        if (aiUnit.Unit.ActionQueue.Count > 0)
                        {
                            UnitAction newMoveAction = aiUnit.Unit.ActionQueue[0];
                            if (newMoveAction.TargetX == firstMoveAction.TargetX && newMoveAction.TargetY == firstMoveAction.TargetY)
                            {
                                aiUnit.Unit.ActionQueue.Clear();
                            }
                            else
                            {
                                Globals.Log("checkForBlockedUnit(): Unblocking unit " + aiUnit.Unit.Id + " at " + aiUnit.Unit.X + "," + aiUnit.Unit.Y + " with new path");
                            }
                        }
                    }
                    string goalTargetXy = aiUnit.GoalTargetXy;
                    AiGoal aiGoal = null;
                    if (goalTargetXy != null && targetXyToGoal.ContainsKey(goalTargetXy))
                    {
                        aiGoal = targetXyToGoal[goalTargetXy];
                        if (aiGoal.IsComplete)
                        {
                            aiUnit.GoalTargetXy = null;
                            targetXyToGoal.Remove(goalTargetXy);
                            aiGoal = null;
                        }
                    }
                    if (aiUnit.Unit.ActionQueue.Count <= 0 && aiGoal != null &&
                        (INFANTRY.Equals(unitType.Name) || DUG_IN_INFANTRY.Equals(unitType.Name) || TRANSPORT_INFANTRY.Equals(unitType.Name)))
                    {
                        List<MapHex> surroundingHexes = map.getSurroundingHexesList(aiUnit.LastMapHex);
                        for (int i = 0; i < surroundingHexes.Count; i++)
                        {
                            int index = random.Next(surroundingHexes.Count);
                            MapHex surroundingHex = surroundingHexes[index];
                            if (surroundingHex.getUnit() == null && surroundingHex.Burb == null)
                            {
                                moveUnit(unitType, aiUnit.Unit, surroundingHex);
                                break;
                            }
                        }
                        if (aiUnit.Unit.ActionQueue.Count > 0)
                        {
                            Globals.Log("checkForBlockedUnit(): Unblocking unit " + aiUnit.Unit.Id + " at " + aiUnit.Unit.X + "," + aiUnit.Unit.Y + " with hex move");
                        }
                    }
                    if (aiUnit.Unit.ActionQueue.Count <= 0 && aiGoal != null)
                    {
                        randomMovement(aiUnit.Unit);
                        Globals.Log("checkForBlockedUnit(): Unblocking unit " + aiUnit.Unit.Id + " at " + aiUnit.Unit.X + "," + aiUnit.Unit.Y + " with random move");
                    }
                    wasBlocked = true;
                }
            }
        }
        string previous = "";
        if (previousMapHex != null)
            previous = ", previousHex=" + previousMapHex.X + "," + previousMapHex.Y;
        string latest = "";
        if (aiUnit.LastMapHex != null)
            latest = ", lastHex=" + aiUnit.LastMapHex.X + "," + aiUnit.LastMapHex.Y;
        string unitInfo = "";
        if (aiUnit.Unit != null)
            unitInfo = ", moves=" + aiUnit.Unit.ActionQueue.Count;
        if (wasBlocked)
            Globals.Log("checkForBlockedUnit(): " + aiUnit + " wasBlocked=" + wasBlocked + unitInfo + previous + latest);
        return wasBlocked;
    }

    private void moveUnit(UnitType unitType, Unit unit, MapHex toHex)
    {
        if (AIRPLANE.Equals(unitType.Name) || !unit.Color.Equals(Faction.Color))
        {
            return;
        }
        Dictionary<string, Node> graph = new Dictionary<string, Node>();
        Dictionary<string, Node> seaGraph = new Dictionary<string, Node>();
        Dictionary<string, Node> landGraph = new Dictionary<string, Node>();
        if (unit == null || unit.StrengthPoints <= 0 || toHex == null)
            return;
        Globals.Log("moveUnit(): enter: " + unit.Id + " to " + toHex.X + "," + toHex.Y);
        MapHex fromHex = map.Hexes[unit.Y, unit.X];
        if (TERRAIN_SEA.Equals(unitType.LandOrSea) && !TRANSPORT_INFANTRY.Equals(unit.UnitType))
        {
            Globals.Log("moveUnit(): trying to find path by sea for " + unit.Id + " to " + toHex.X + "," + toHex.Y);
            gameState.Map.buildNodesForShortestPath(true, null, seaGraph, null, toHex);
            List<UnitAction> path = gameState.Map.determinePath(seaGraph, fromHex, toHex);
            //List<UnitAction> path = gameState.Map.determineSeaPath(fromHex, toHex);
            if (path != null && path.Count > 0)
            {
                unit.ActionQueue.Clear();
                foreach (UnitAction moveAction in path)
                {
                    unit.addUnitAction(moveAction);
                }
                Globals.Log("moveUnit(): " + unit.Id + " from " + unit.X + "," + unit.Y + " to " + toHex.X + "," + toHex.Y + ", paths=" + path.Count);
            }
        }
        else if ((!TERRAIN_SEA.Equals(unitType.LandOrSea)) &&
                (TERRAIN_GRASS.Equals(fromHex.Terrain) || TERRAIN_FOREST.Equals(fromHex.Terrain) || TERRAIN_MOUNTAIN.Equals(fromHex.Terrain) || TERRAIN_SWAMP.Equals(fromHex.Terrain)) &&
                (TERRAIN_GRASS.Equals(toHex.Terrain) || TERRAIN_FOREST.Equals(toHex.Terrain) || TERRAIN_MOUNTAIN.Equals(toHex.Terrain) || TERRAIN_SWAMP.Equals(toHex.Terrain)))
        {
            Globals.Log("moveUnit(): trying to find path by land for " + unit.Id + " to " + toHex.X + "," + toHex.Y);
            gameState.Map.buildNodesForShortestPath(true, null, null, landGraph, toHex);
            List<UnitAction> path = gameState.Map.determinePath(landGraph, fromHex, toHex);
            //List<UnitAction> path = gameState.Map.determineLandPath(fromHex, toHex);
            if (path != null && path.Count > 0)
            {
                unit.ActionQueue.Clear();
                foreach (UnitAction moveAction in path)
                {
                    unit.addUnitAction(moveAction);
                }
                Globals.Log("moveUnit(): " + unit.Id + " from " + unit.X + "," + unit.Y + " to " + toHex.X + "," + toHex.Y + ", paths=" + path.Count);
            }
        }
        if (unit.ActionQueue.Count <= 0)
        {
            Globals.Log("moveUnit(): trying to find path for " + unit.Id + " to " + toHex.X + "," + toHex.Y);
            gameState.Map.buildNodesForShortestPath(true, graph, null, null, toHex);
            List<UnitAction> path = gameState.Map.determinePath(graph, fromHex, toHex);
            //List<UnitAction> path = gameState.Map.determinePath(fromHex, toHex);
            if (path != null && path.Count > 0)
            {
                unit.ActionQueue.Clear();
                foreach (UnitAction moveAction in path)
                {
                    unit.addUnitAction(moveAction);
                }
                Globals.Log("moveUnit(): " + unit.Id + " from " + unit.X + "," + unit.Y + " to " + toHex.X + "," + toHex.Y + ", paths=" + path.Count);
            }
        }
        if (unit.ActionQueue.Count <= 0)
        {
            UnitAction unitAction = new UnitAction();
            unitAction.Action = "move";
            unitAction.TargetX = toHex.X;
            unitAction.TargetY = toHex.Y;
            unit.setUnitAction(unitAction);
            Globals.Log("moveUnit(): single unitAction used to move " + unit.Id + " from " + unit.X + "," + unit.Y + " to " + toHex.X + "," + toHex.Y);
        }
    }

    private void flyMission(AiGoal goal, Unit plane)
    {
        PlaneUnitType planeType = new PlaneUnitType();
        Unit parentUnit = planeType.getParentUnit(map, plane);
        if (parentUnit != null)
        {
            Globals.Log("flyMission(): goal=" + goal.Type + ", plane=" + parentUnit.X + "," + parentUnit.Y);
            plane.X = parentUnit.X;
            plane.Y = parentUnit.Y;
        }
        else
            Globals.Log("flyMission(): goal=" + goal.Type + ", plane=" + plane.X + "," + plane.Y);
        // Look for desirable short range targets in order:
        // Comcens
        // armor units
        // non-dug-in infantry units
        // carriers
        // transports
        // subs
        // battleships
        // dug-in infantry
        MapHex planeHex = planeType.getPlaneMapHex(map, plane);
        Unit priorityTargetUnit = null;
        HashSet<MapHex> rangeHexes = map.getMapHexesInRange(planeHex, 4);
        foreach (MapHex mapHex in rangeHexes)
        {
            Unit targetUnit = mapHex.getUnit();
            if (targetUnit == null || targetUnit.Color.Equals(plane.Color))
                continue;

            if (priorityTargetUnit == null)
            {
                priorityTargetUnit = targetUnit;
            }
            if (COMMAND_CENTER.Equals(targetUnit.UnitType))
            {
                priorityTargetUnit = targetUnit;
                break;
            }
            else if (ARMOR.Equals(targetUnit.UnitType) &&
                     new HashSet<string>() { INFANTRY, TRANSPORT_INFANTRY, TRANSPORT_ARMOR, SUBMARINE, BATTLESHIP, DUG_IN_INFANTRY }
                     .Contains(priorityTargetUnit.UnitType))
            {
                priorityTargetUnit = targetUnit;
            }
            else if (INFANTRY.Equals(targetUnit.UnitType) &&
                     new HashSet<string>() { TRANSPORT_ARMOR, TRANSPORT_INFANTRY, SUBMARINE, BATTLESHIP, DUG_IN_INFANTRY }
                     .Contains(priorityTargetUnit.UnitType))
            {
                priorityTargetUnit = targetUnit;
            }
            else if (TRANSPORT_ARMOR.Equals(targetUnit.UnitType) &&
                     new HashSet<string>() { TRANSPORT_INFANTRY, SUBMARINE, BATTLESHIP, DUG_IN_INFANTRY }
                     .Contains(priorityTargetUnit.UnitType))
            {
                priorityTargetUnit = targetUnit;
            }
            else if (TRANSPORT_INFANTRY.Equals(targetUnit.UnitType) &&
                     new HashSet<string>() { SUBMARINE, BATTLESHIP, DUG_IN_INFANTRY }
                     .Contains(priorityTargetUnit.UnitType))
            {
                priorityTargetUnit = targetUnit;
            }
            else if (SUBMARINE.Equals(targetUnit.UnitType) &&
                     new HashSet<string>() { BATTLESHIP, DUG_IN_INFANTRY }
                     .Contains(priorityTargetUnit.UnitType))
            {
                priorityTargetUnit = targetUnit;
            }
            else if (BATTLESHIP.Equals(targetUnit.UnitType) &&
                     new HashSet<string>() { DUG_IN_INFANTRY }
                     .Contains(priorityTargetUnit.UnitType))
            {
                priorityTargetUnit = targetUnit;
            }
        }
        if (priorityTargetUnit != null)
        {
            Globals.Log("flyMission(): priorityTargetUnit=" + priorityTargetUnit.UnitType + " at " + priorityTargetUnit.X + "," + priorityTargetUnit.Y);
            AirstrikeAction action = new AirstrikeAction();
            action.ClientIdentifier = Faction.Color;
            action.ClassType = "GlobalConquest.Actions.AirstrikeAction";
            action.Plane = plane;
            action.StrikeX = priorityTargetUnit.X;
            action.StrikeY = priorityTargetUnit.Y;
            action.execute(Server);
        }
    }

    private bool IsUnitInPosition(AiGoal goal, AiUnit aiUnit)
    {
        bool isUnitInPosition = false;
        HashSet<MapHex> rangeMinusOneHexes = map.getMapHexesInRange(goal.TargetMapHex, aiUnit.DistanceFromTarget - 1);
        HashSet<MapHex> rangeHexes = map.getMapHexesInRange(goal.TargetMapHex, aiUnit.DistanceFromTarget);
        rangeHexes.ExceptWith(rangeMinusOneHexes);
        HashSet<MapHex> finalRangeHexes = rangeHexes;
        if (aiUnit.Unit == null)
            return false;

        // Unit is already in position
        MapHex mapHex = map.Hexes[aiUnit.Unit.Y, aiUnit.Unit.X];
        if (finalRangeHexes.Contains(mapHex))
            isUnitInPosition = true;

        return isUnitInPosition;

    }

    private MapHex findHexAroundBurb(MapHex burbHex, AiUnit aiUnit, int distance)
    {

        if (aiUnit == null || aiUnit.Unit == null)
            return null;
        return findHexAroundBurb(burbHex, aiUnit.Unit, distance);
    }

    private MapHex findHexAroundBurb(MapHex burbHex, Unit unit, int distance)
    {
        if (unit == null || burbHex == null || burbHex.Burb == null)
            return null;

        HashSet<MapHex> rangeHexes = map.getMapHexesAtDistance(burbHex, distance);
        HashSet<MapHex> finalRangeHexes = rangeHexes;

        // Unit is already in position
        MapHex mapHex = map.Hexes[unit.Y, unit.X];
        if (finalRangeHexes.Contains(mapHex))
            return null;

        MapHex foundMapHex = null;
        //int index = random.Next(0, finalRangeHexes.Count);
        //MapHex candidateHex = finalRangeHexes.ToList<MapHex>()[index];
        MapHex candidateHex = map.getClosestUnoccupiedHexAtDistance(mapHex, burbHex, distance);
        UnitType unitType = gameState.UnitTypes.UnitTypeMap[unit.UnitType];
        if (candidateHex != null && candidateHex.getUnit() == null && ((!TERRAIN_SEA.Equals(unitType.LandOrSea)) ||
            (TERRAIN_SEA.Equals(unitType.LandOrSea) &&
            (TERRAIN_SEA.Equals(candidateHex.Terrain) || TERRAIN_SWAMP.Equals(candidateHex.Terrain) || "marsh".Equals(candidateHex.Terrain)))))
        {
            foundMapHex = candidateHex;
        }
        else
        {
            foreach (MapHex searchMapHex in finalRangeHexes)
            {
                if (searchMapHex.getUnit() == null && ((!TERRAIN_SEA.Equals(unitType.LandOrSea)) ||
                    (TERRAIN_SEA.Equals(unitType.LandOrSea) &&
                    (TERRAIN_SEA.Equals(searchMapHex.Terrain) || TERRAIN_SWAMP.Equals(searchMapHex.Terrain) || "marsh".Equals(searchMapHex.Terrain)))))
                {
                    foundMapHex = searchMapHex;
                    break;
                }
            }
        }

        if (foundMapHex == null)
        {
            Globals.Log("Ai.findHexAroundBurb(): could not find hex around " + burbHex.X + "," + burbHex.Y);
        }
        return foundMapHex;
    }

    private MapHex findHexAroundBurb(AiGoal goal, AiUnit aiUnit)
    {
        return findHexAroundBurb(goal.TargetMapHex, aiUnit, aiUnit.DistanceFromTarget);
    }


    private Unit purchaseUnitAtMetro(string unitTypeString)
    {
        return purchaseUnitAtBurb(myMetroHex, unitTypeString);
    }


    private Unit purchaseUnitAtBurb(MapHex burbHex, string unitTypeString)
    {
        UnitType unitType = gameState.UnitTypes.UnitTypeMap[unitTypeString];
        Unit unit = null;
        if (AIRPLANE.Equals(unitTypeString) && burbHex.Airplane == null)
        {
            unit = new Unit();
            unit.UnitType = unitTypeString;
            unit.Color = Faction.Color;
            unit.X = burbHex.X;
            unit.Y = burbHex.Y;
            if (VISIBILITY_OMNISCIENT.Equals(gameSettings.Visibility))
                unit.setOmniVisibility();
            else
                unit.setBaseVisibility();
            map.placeNewPlane(unit, burbHex);
            Faction.Money -= unitType.Cost;
        }
        else if (burbHex.getUnit() == null && Faction.Money >= unitType.Cost)
        {
            unit = new Unit();
            unit.UnitType = unitTypeString;
            unit.Color = Faction.Color;
            unit.X = burbHex.X;
            unit.Y = burbHex.Y;
            if (VISIBILITY_OMNISCIENT.Equals(gameSettings.Visibility))
                unit.setOmniVisibility();
            else
                unit.setBaseVisibility();
            map.placeNewUnit(unit, burbHex);
            Faction.Money -= unitType.Cost;
        }
        if (unit != null)
            Globals.Log("Ai.purchaseUnitAtBurb(): " + unit.Id);
        return unit;
    }

    private Unit purchaseUnitAtMetroDock(string unitTypeString)
    {
        return purchaseUnitAtBurbDock(myMetroHex, unitTypeString);
    }

    private Unit purchaseUnitAtBurbDock(MapHex burbHex, string unitTypeString)
    {
        UnitType unitType = gameState.UnitTypes.UnitTypeMap[unitTypeString];
        MapHex dock = null;
        Unit unit = null;
        if (Faction.Money >= unitType.Cost)
        {
            foreach (MapHex dockHex in map.getSurroundingHexesList(burbHex))
            {
                if (dockHex.Burb != null && (BURB_DOCK.Equals(dockHex.Burb.Type) || TERRAIN_SEA.Equals(dockHex.Terrain)) && dockHex.getUnit() == null && Faction.Money >= unitType.Cost)
                {
                    unit = new Unit();
                    unit.UnitType = unitTypeString;
                    unit.Color = Faction.Color;
                    unit.X = dockHex.X;
                    unit.Y = dockHex.Y;
                    if (VISIBILITY_OMNISCIENT.Equals(gameSettings.Visibility))
                        unit.setOmniVisibility();
                    else
                        unit.setBaseVisibility();
                    map.placeNewUnit(unit, dockHex);
                    Faction.Money -= unitType.Cost;
                    break;
                }
            }
        }
        if (unit != null)
            Globals.Log("Ai.purchaseUnitAtBurbDock(): " + unit.Id);
        return unit;
    }

    private Unit purchaseUnitAtSuburb(MapHex burbHex, string unitTypeString)
    {
        UnitType unitType = gameState.UnitTypes.UnitTypeMap[unitTypeString];
        Unit unit = null;
        if (Faction.Money >= unitType.Cost)
        {
            foreach (MapHex suburbHex in map.getSurroundingHexesList(burbHex))
            {
                if (AIRPLANE.Equals(unitTypeString) && suburbHex.Burb != null && (BURB_SUBURB.Equals(suburbHex.Burb.Type)) && suburbHex.Airplane == null)
                {
                    unit = new Unit();
                    unit.UnitType = unitTypeString;
                    unit.Color = Faction.Color;
                    unit.X = suburbHex.X;
                    unit.Y = suburbHex.Y;
                    if (VISIBILITY_OMNISCIENT.Equals(gameSettings.Visibility))
                        unit.setOmniVisibility();
                    else
                        unit.setBaseVisibility();
                    map.placeNewPlane(unit, suburbHex);
                    Faction.Money -= unitType.Cost;

                }
                else if (suburbHex.Burb != null && (BURB_SUBURB.Equals(suburbHex.Burb.Type)) && suburbHex.getUnit() == null)
                {
                    unit = new Unit();
                    unit.UnitType = unitTypeString;
                    unit.Color = Faction.Color;
                    unit.X = suburbHex.X;
                    unit.Y = suburbHex.Y;
                    if (VISIBILITY_OMNISCIENT.Equals(gameSettings.Visibility))
                        unit.setOmniVisibility();
                    else
                        unit.setBaseVisibility();
                    map.placeNewUnit(unit, suburbHex);
                    Faction.Money -= unitType.Cost;
                }
            }
        }
        if (unit != null)
            Globals.Log("Ai.purchaseUnitAtSuburb(): " + unit.Id);
        return unit;
    }

    private void moveSpy()
    {
        // TODO: we need something smarter and more cautious.
        randomMovement(spy);
    }

    private void randomMovement(Unit unit)
    {
        if (unit != null && unit.StrengthPoints > 0)
        {
            UnitType unitType = gameState.UnitTypes.UnitTypeMap[unit.UnitType];
            //Globals.Log("Ai.randomMovement(): " + unit.UnitType);
            // 0=capital, 1=left, 2=right, 3=diagonal
            MapHex targetHex = null;
            int randomNumber = random.Next(0, 4);
            // Sea units can't go to the capital
            if (TERRAIN_SEA.Equals(unitType.LandOrSea) && randomNumber == 0 && !unitType.Name.Contains(TRANSPORT))
                randomNumber = 3;
            if (randomNumber == 0)
                targetHex = Server.gameState.Map.getCapitalHex();
            else if (randomNumber == 1)
                targetHex = Server.gameState.Map.LeftMetro[Faction.Color];
            else if (randomNumber == 2)
                targetHex = Server.gameState.Map.RightMetro[Faction.Color];
            else if (randomNumber == 3)
                targetHex = Server.gameState.Map.DiagonalMetro[Faction.Color];
            if (targetHex != null)
            {
                moveUnit(unitType, unit, targetHex);
                Globals.Log("randomMovement(): " + unit.Id + " to " + targetHex.X + "," + targetHex.Y);
            }
        }
    }

    private bool freeBurb(Unit unit)
    {
        return freeBurb(unit, 3);
    }

    private bool freeBurb(Unit unit, int range)
    {
        bool isBurbToFree = false;
        if (unit != null && unit.StrengthPoints > 0)
        {
            UnitType unitType = gameState.UnitTypes.UnitTypeMap[unit.UnitType];
            if (TERRAIN_SEA.Equals(unitType.LandOrSea))
                return false;
            MapHex unitHex = map.Hexes[unit.Y, unit.X];
            foreach (string burbKey in gameState.Burbs.HexXyToBurb.Keys)
            {
                Burb burb = gameState.Burbs.HexXyToBurb[burbKey];
                MapHex burbHex = map.Hexes[burb.Y, burb.X];
                List<MapHex> neighbors = map.getSurroundingHexesList(burbHex);
                int enemies = 0;
                if (burbHex.getUnit() != null)
                    enemies = 1;
                foreach (MapHex neighbor in neighbors)
                {
                    Unit enemyUnit = neighbor.getUnit();
                    if (enemyUnit != null && !enemyUnit.Color.Equals(Faction.Color))
                        enemies += 1;
                }
                if (enemies == 0)
                {
                    float distance = map.calculateDistance(unitHex, burbHex);
                    if (distance <= range && !burb.OwnerColor.Equals(unit.Color))
                    {
                        moveUnit(unitType, unit, burbHex);
                        isBurbToFree = true;
                        Globals.Log("Ai.freeBurb(): " + unit.Id + " to " + burbHex.X + "," + burbHex.Y);
                        break;
                    }
                }
            }
        }
        return isBurbToFree;
    }


    private void createInitialGoals()
    {
        createDefendMetroGoal();
        createExploreMetroGoal(leftMetroHex);
        createExploreMetroGoal(rightMetroHex);
        AiGoal exploreMetro = createExploreMetroGoal(diagonalMetroHex);
        exploreMetro.UseRandomMovement = true;
        createExploreCapitalGoal();
        AiGoal topLevelExploreGoal = new AiGoal();
        topLevelExploreGoal.Type = AI_GOAL_EXPLORE;
        goals.Add(topLevelExploreGoal);
        //createBuildCarrierGoal();
        createBuildMetroPlaneGoal();
        createDefaultGoal();
    }


    private void createDefaultGoal()
    {
        defaultGoal = new AiGoal();
        defaultGoal.Type = AI_GOAL_DEFEND;
        defaultGoal.TargetMapHex = myMetroHex;
        defaultGoal.IsOngoingGoal = true;
        goals.Add(defaultGoal);
    }

    private void createBuildMetroPlaneGoal()
    {
        AiGoal goal = new AiGoal();
        goal.IsOngoingGoal = true;
        goal.Type = AI_GOAL_BUILD_PLANE;
        goal.TargetMapHex = myMetroHex;
        AiUnit plane = new AiUnit();
        plane.GoalTargetXy = myMetroHex.X + "," + myMetroHex.Y;
        plane.InitialPosition = myMetroHex;
        plane.UnitType = AIRPLANE;
        goal.DesiredUnits.Add(plane);
        goals.Add(goal);
    }

    private void createBuildCarrierGoal()
    {
        AiGoal goal = new AiGoal();
        goal.Type = AI_GOAL_BUILD_CARRIER;
        goal.TargetMapHex = myMetroHex;
        AiUnit carrier = new AiUnit();
        carrier.GoalTargetXy = myMetroHex.X + "," + myMetroHex.Y;
        carrier.DistanceFromTarget = 5;
        carrier.UnitType = AIRCRAFT_CARRIER;
        goal.DesiredUnits.Add(carrier);
        AiUnit plane = new AiUnit();
        plane.GoalTargetXy = myMetroHex.X + "," + myMetroHex.Y;
        plane.InitialPosition = myMetroHex;
        plane.UnitType = AIRPLANE;
        goal.DesiredUnits.Add(plane);
        goals.Add(goal);
    }

    private void createDefendMetroGoal()
    {
        AiGoal defendMetro = new AiGoal();
        defendMetro.Type = AI_GOAL_DEFEND;
        defendMetro.TargetMapHex = myMetroHex;
        defendMetro.IsOngoingGoal = true;
        // 3 subs, 1 carrier, 1 battleship, 1 infantry
        AiUnit sub1 = new AiUnit();
        sub1.GoalTargetXy = myMetroHex.X + "," + myMetroHex.Y;
        sub1.DistanceFromTarget = 5;
        sub1.UnitType = SUBMARINE;
        defendMetro.DesiredUnits.Add(sub1);
        AiUnit sub2 = new AiUnit();
        sub2.GoalTargetXy = myMetroHex.X + "," + myMetroHex.Y;
        sub2.DistanceFromTarget = 5;
        sub2.UnitType = SUBMARINE;
        defendMetro.DesiredUnits.Add(sub2);
        AiUnit sub3 = new AiUnit();
        sub3.GoalTargetXy = myMetroHex.X + "," + myMetroHex.Y;
        sub3.DistanceFromTarget = 5;
        sub3.UnitType = SUBMARINE;
        defendMetro.DesiredUnits.Add(sub3);
        AiUnit infantry = new AiUnit();
        infantry.GoalTargetXy = myMetroHex.X + "," + myMetroHex.Y;
        infantry.InitialPosition = myMetroHex;
        infantry.DistanceFromTarget = 0;
        infantry.UnitType = INFANTRY;
        defendMetro.DesiredUnits.Add(infantry);
        AiUnit plane = new AiUnit();
        plane.GoalTargetXy = myMetroHex.X + "," + myMetroHex.Y;
        plane.InitialPosition = myMetroHex;
        plane.UnitType = AIRPLANE;
        defendMetro.DesiredUnits.Add(plane);
        AiUnit battleship = new AiUnit();
        battleship.GoalTargetXy = myMetroHex.X + "," + myMetroHex.Y;
        battleship.DistanceFromTarget = 4;
        battleship.UnitType = BATTLESHIP;
        defendMetro.DesiredUnits.Add(battleship);
        AiUnit carrier = new AiUnit();
        carrier.GoalTargetXy = myMetroHex.X + "," + myMetroHex.Y;
        carrier.DistanceFromTarget = 3;
        carrier.UnitType = AIRCRAFT_CARRIER;
        defendMetro.DesiredUnits.Add(carrier);
        goals.Add(defendMetro);
    }

    private AiGoal createExploreMetroGoal(MapHex metro)
    {
        AiGoal exploreMetro = new AiGoal();
        exploreMetro.Type = AI_GOAL_EXPLORE;
        exploreMetro.TargetMapHex = metro;
        exploreMetro.IsOngoingGoal = true;
        // 1 sub, 1 infantry
        AiUnit sub1 = new AiUnit();
        sub1.GoalTargetXy = metro.X + "," + metro.Y;
        sub1.DistanceFromTarget = 3;
        sub1.UnitType = SUBMARINE;
        exploreMetro.DesiredUnits.Add(sub1);
        AiUnit infantry = new AiUnit();
        infantry.GoalTargetXy = metro.X + "," + metro.Y;
        infantry.InitialPosition = metro;
        infantry.UnitType = INFANTRY;
        exploreMetro.DesiredUnits.Add(infantry);
        exploreGoals.Add(exploreMetro);
        return exploreMetro;
    }

    private void createExploreCapitalGoal()
    {
        AiGoal exploreGoal = new AiGoal();
        exploreGoal.Type = AI_GOAL_EXPLORE;
        exploreGoal.UseRandomMovement = true;
        exploreGoal.TargetMapHex = Server.gameState.Map.getCapitalHex();
        AiUnit infantry = new AiUnit();
        infantry.GoalTargetXy = exploreGoal.TargetMapHex.X + "," + exploreGoal.TargetMapHex.Y;
        infantry.UnitType = INFANTRY;
        infantry.DistanceFromTarget = 4;
        exploreGoal.DesiredUnits.Add(infantry);
        exploreGoals.Add(exploreGoal);
    }

    private void createDefendBurbGoal(MapHex burbHex)
    {
        AiGoal defendGoal = new AiGoal();
        defendGoal.Type = AI_GOAL_DEFEND;
        defendGoal.IsOngoingGoal = true;
        defendGoal.TargetMapHex = burbHex;
        AiUnit infantry = new AiUnit();
        infantry.GoalTargetXy = defendGoal.TargetMapHex.X + "," + defendGoal.TargetMapHex.Y;
        infantry.InitialPosition = burbHex;
        infantry.UnitType = INFANTRY;
        defendGoal.DesiredUnits.Add(infantry);
        AiUnit plane = new AiUnit();
        plane.GoalTargetXy = defendGoal.TargetMapHex.X + "," + defendGoal.TargetMapHex.Y;
        plane.InitialPosition = burbHex;
        plane.UnitType = AIRPLANE;
        defendGoal.DesiredUnits.Add(plane);
        Globals.Log("createDefendBurbGoal(): " + burbHex.Burb.Type);
        if (BURB_VILLAGE.Equals(burbHex.Burb.Type))
        {
        }
        else if (BURB_TOWN.Equals(burbHex.Burb.Type) || BURB_CITY.Equals(burbHex.Burb.Type) || BURB_METROPLEX.Equals(burbHex.Burb.Type) || BURB_CAPITAL.Equals(burbHex.Burb.Type))
        {
            List<MapHex> neighbors = map.getSurroundingHexesList(burbHex);
            bool hasDock = false;
            foreach (MapHex mapHex in neighbors)
            {
                if (mapHex.Burb != null && BURB_DOCK.Equals(mapHex.Burb.Type))
                {
                    hasDock = true;
                    break;
                }
            }
            if (hasDock)
            {
                AiUnit sub1 = new AiUnit();
                sub1.GoalTargetXy = defendGoal.TargetMapHex.X + "," + defendGoal.TargetMapHex.Y;
                sub1.DistanceFromTarget = 3;
                sub1.UnitType = SUBMARINE;
                defendGoal.DesiredUnits.Add(sub1);
                if (!BURB_TOWN.Equals(burbHex.Burb.Type))
                {
                    AiUnit sub2 = new AiUnit();
                    sub2.GoalTargetXy = defendGoal.TargetMapHex.X + "," + defendGoal.TargetMapHex.Y;
                    sub2.DistanceFromTarget = 4;
                    sub2.UnitType = SUBMARINE;
                    defendGoal.DesiredUnits.Add(sub2);
                }
            }
        }
        goals.Add(defendGoal);
    }

    private void createConquerBurbGoal(MapHex burbHex)
    {
        if (targetXyToGoal.ContainsKey(burbHex.X + "," + burbHex.Y))
            return;
        bool isCoastal = IsBurbCoastal(burbHex);
        if (isCoastal)
            conquerCoastalBurbGoal(burbHex);
        else
            conquerInteriorBurbGoal(burbHex);
    }

    private void createConquerResource(Resource resource)
    {
        if (targetXyToGoal.ContainsKey(resource.X + "," + resource.Y))
            return;
        AiGoal attackGoal = new AiGoal();
        attackGoal.Type = AI_GOAL_CONQUER;
        MapHex mapHex = map.Hexes[resource.Y, resource.X];
        attackGoal.TargetMapHex = mapHex;
        attackGoal.ShouldMoveToTarget = true;
        attackGoal.IsOngoingGoal = false;
        AiUnit infantry = new AiUnit();
        infantry.InitialPosition = mapHex;
        infantry.GoalTargetXy = attackGoal.TargetMapHex.X + "," + attackGoal.TargetMapHex.Y;
        infantry.UnitType = INFANTRY;
        attackGoal.DesiredUnits.Add(infantry);
        goals.Add(attackGoal);
        Globals.Log("createConquerResource(): added conquer goal for " + mapHex.X + "," + mapHex.Y);
        targetXyToGoal[mapHex.X + "," + mapHex.Y] = attackGoal;

    }

    private bool IsBurbCoastal(MapHex burbHex)
    {
        bool isCoastal = false;
        List<MapHex> neighbors = map.getSurroundingHexesList(burbHex);
        foreach (MapHex neighbor in neighbors)
        {
            if (TERRAIN_SEA.Equals(neighbor.Terrain) || (neighbor.Burb != null && BURB_DOCK.Equals(neighbor.Burb.Type)))
            {
                isCoastal = true;
                break;
            }
        }
        return isCoastal;
    }

    private void conquerInteriorBurbGoal(MapHex burbHex)
    {
        if (targetXyToGoal.ContainsKey(burbHex.X + "," + burbHex.Y))
            return;
        AiGoal attackGoal = new AiGoal();
        attackGoal.Type = AI_GOAL_CONQUER;
        attackGoal.TargetMapHex = burbHex;
        attackGoal.ShouldMoveToTarget = false;
        attackGoal.IsOngoingGoal = false;
        updateDesiredUnitsForInteriorBurbGoal(attackGoal);
        goals.Add(attackGoal);
        Globals.Log("Ai.conquerInteriorBurbGoal(): added conquer goal for " + burbHex.X + "," + burbHex.Y);
        targetXyToGoal[burbHex.X + "," + burbHex.Y] = attackGoal;
    }

    private void updateDesiredUnitsForInteriorBurbGoal(AiGoal attackGoal)
    {
        MapHex burbHex = attackGoal.TargetMapHex;
        if (burbHex.Burb == null)
            return;
        List<MapHex> neighbors = map.getSurroundingHexesList(burbHex);
        int enemies = 0;
        if (burbHex.getUnit() != null)
            enemies = 1;
        foreach (MapHex neighbor in neighbors)
        {
            Unit unit = neighbor.getUnit();
            if (unit != null && !unit.Color.Equals(Faction.Color))
                enemies += 1;
        }

        attackGoal.Enemies = enemies;
        int desiredInfantry = 0;
        if (attackGoal.Enemies == 0)
            desiredInfantry = 1;
        else
            desiredInfantry = attackGoal.Enemies + 2;
        int count = 0;
        int currentDesire = attackGoal.GetDesiredCountForUnitType(INFANTRY);
        if (currentDesire >= desiredInfantry)
            count = 0;
        else
            count = desiredInfantry - currentDesire;

        for (int i = 0; i < count; i++)
        {
            AiUnit infantry = new AiUnit();
            infantry.GoalTargetXy = attackGoal.TargetMapHex.X + "," + attackGoal.TargetMapHex.Y;
            infantry.UnitType = INFANTRY;
            if (BURB_VILLAGE.Equals(burbHex.Burb.Type) || BURB_TOWN.Equals(burbHex.Burb.Type))
                infantry.DistanceFromTarget = 3;
            else
                infantry.DistanceFromTarget = 4;
            attackGoal.DesiredUnits.Add(infantry);
        }
    }

    private void conquerCoastalBurbGoal(MapHex burbHex)
    {
        if (targetXyToGoal.ContainsKey(burbHex.X + "," + burbHex.Y))
            return;
        AiGoal attackGoal = new AiGoal();
        attackGoal.Type = AI_GOAL_CONQUER;
        attackGoal.TargetMapHex = burbHex;
        attackGoal.ShouldMoveToTarget = false;
        attackGoal.IsOngoingGoal = false;
        updateDesiredUnitsForCoastalBurbGoal(attackGoal);
        goals.Add(attackGoal);
        //createBuildCarrierGoal();
        Globals.Log("Ai.conquerCoastalBurbGoal(): added conquer goal for " + burbHex.X + "," + burbHex.Y);
        targetXyToGoal[burbHex.X + "," + burbHex.Y] = attackGoal;

    }

    private void updateDesiredUnitsForCoastalBurbGoal(AiGoal attackGoal)
    {
        MapHex burbHex = attackGoal.TargetMapHex;
        if (burbHex.Burb == null)
            return;
        List<MapHex> neighbors = map.getSurroundingHexesList(burbHex);
        int enemies = 0;
        if (burbHex.getUnit() != null)
            enemies = 1;
        foreach (MapHex neighbor in neighbors)
        {
            Unit unit = neighbor.getUnit();
            if (unit != null && !unit.Color.Equals(Faction.Color))
                enemies += 1;
        }
        attackGoal.Enemies = enemies;
        int desiredInfantry = 0;
        if (attackGoal.Enemies == 0)
            desiredInfantry = 1;
        else
            desiredInfantry = attackGoal.Enemies + 2;
        int count = 0;
        int currentInfantryDesire = attackGoal.GetDesiredCountForUnitType(INFANTRY);
        if (currentInfantryDesire >= desiredInfantry)
            count = 0;
        else
            count = desiredInfantry - currentInfantryDesire;

        int desiredBattleships = 1;
        if (map.IsMetroHex(burbHex))
            desiredBattleships = 3;

        if (attackGoal.Enemies > 0)
        {
            bool needsCarrier = true;
            bool needsBattleship = true;
            int actualBattleships = 0;
            foreach (AiUnit actualAiUnit in attackGoal.ActualUnits)
            {
                if (AIRCRAFT_CARRIER.Equals(actualAiUnit.Unit.UnitType))
                    needsCarrier = false;
                if (BATTLESHIP.Equals(actualAiUnit.Unit.UnitType))
                    actualBattleships += 1;
            }
            if (actualBattleships < desiredBattleships)
                needsBattleship = true;
            if (needsCarrier && attackGoal.GetDesiredCountForUnitType(AIRCRAFT_CARRIER) < 1)
            {
                AiUnit carrier = new AiUnit();
                carrier.GoalTargetXy = attackGoal.TargetMapHex.X + "," + attackGoal.TargetMapHex.Y;
                carrier.UnitType = AIRCRAFT_CARRIER;
                carrier.DistanceFromTarget = 4;
                attackGoal.DesiredUnits.Add(carrier);
            }

            int currentDesiredBattleships = attackGoal.GetDesiredCountForUnitType(BATTLESHIP);
            if (needsBattleship && attackGoal.GetDesiredCountForUnitType(BATTLESHIP) < desiredBattleships)
            {
                for (int i = 0; i < (desiredBattleships - currentDesiredBattleships); i++)
                {
                    AiUnit battleship = new AiUnit();
                    battleship.GoalTargetXy = attackGoal.TargetMapHex.X + "," + attackGoal.TargetMapHex.Y;
                    battleship.UnitType = BATTLESHIP;
                    if (BURB_VILLAGE.Equals(burbHex.Burb.Type) || BURB_TOWN.Equals(burbHex.Burb.Type))
                        battleship.DistanceFromTarget = 3;
                    else
                        battleship.DistanceFromTarget = 3;
                    attackGoal.DesiredUnits.Add(battleship);
                }
            }
        }

        for (int i = 0; i < count; i++)
        {
            AiUnit infantry = new AiUnit();
            infantry.GoalTargetXy = attackGoal.TargetMapHex.X + "," + attackGoal.TargetMapHex.Y;
            infantry.UnitType = INFANTRY;
            if (BURB_VILLAGE.Equals(burbHex.Burb.Type) || BURB_TOWN.Equals(burbHex.Burb.Type))
                infantry.DistanceFromTarget = 3;
            else
                infantry.DistanceFromTarget = 4;
            attackGoal.DesiredUnits.Add(infantry);
        }

    }


    public void outputDataStructureUse()
    {
        Globals.Log("outputDataStructureUse(): " + Faction.Color +  ": dockList=" + dockList.Count);
        Globals.Log("outputDataStructureUse(): " + Faction.Color +  ": goals=" + goals.Count);
        Globals.Log("outputDataStructureUse(): " + Faction.Color +  ": conquestGoals=" + conquestGoals.Count);
        Globals.Log("outputDataStructureUse(): " + Faction.Color +  ": exploreGoals=" + exploreGoals.Count);
        Globals.Log("outputDataStructureUse(): " + Faction.Color +  ": targetXyToGoal=" + targetXyToGoal.Count);
        Globals.Log("outputDataStructureUse(): " + Faction.Color +  ": unitIdToAiUnit=" + unitIdToAiUnit.Count);

        Globals.Log("outputDataStructureUse(): unitTypeToAvailableUnits=" + unitTypeToAvailableUnits.Count);
        int totalAvailableUnits = 0;
        foreach (string key in unitTypeToAvailableUnits.Keys)
        {
            totalAvailableUnits += unitTypeToAvailableUnits[key].Count;
        }
        Globals.Log("outputDataStructureUse(): " + Faction.Color +  ": totalAvailableUnits=" + totalAvailableUnits);

        int goalUnits = 0;
        foreach (AiGoal aiGoal in goals)
        {
            goalUnits += aiGoal.countDataStructureUse();            
        }
        Globals.Log("outputDataStructureUse(): " + Faction.Color +  ": goalUnits=" + goalUnits);
    }

}
