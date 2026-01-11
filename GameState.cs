using static UnitTypeConstants;
using System.Text.Json;
using System.Text.Json.Serialization;
using GlobalConquest.Units;
using static GlobalConquest.Map;
using static GlobalConquest.Burbs;
using static GameConstants;
namespace GlobalConquest;

public class GameState
{

    public GameSettings GameSettings { get; set; }
    public Factions Factions { get; set; }

    public Players Players { get; set; }

    [JsonIgnore]
    public Map Map { get; set; }

    public MapHex MapHex { get; set; }

    public Dictionary<string, bool> PlayerExecutionReady { get; set; } = new Dictionary<string, bool>();
    public Dictionary<string, bool> PlayerPlanningReady { get; set; } = new Dictionary<string, bool>();
    public Dictionary<string, bool> PlayerJoined { get; set; } = new Dictionary<string, bool>();
    public int CurrentTurn { get; set; } = 0;
    public string CurrentPhase { get; set; } = GAME_PHASE_PLAN;  // plan, execution, gameOver
    public int CurrentRound { get; set; } = 0;
    public UnitTypes UnitTypes { get; set; }
    public Burbs Burbs { get; set; }

    public string VictoriousColor { get; set; } = NATIVE_COLOR;
    public long Ticks { get; set; } = 0;

    public int SecondsRemainingUntilExecution {get; set;}

    // if any of the data elements in the entities change above, then this version should be bumped.
    public string Version { get; set; } = "v0.7.4";
    private Random rand = new System.Random();


    public GameState()
    {
        Factions = new Factions();
        UnitTypes = new UnitTypes();
        Players = new Players();
        Burbs = new Burbs();
    }

    public void updateTicks()
    {
        Ticks = DateTime.Now.Ticks;
    }

    public void placeInitialUnits()
    {
        foreach (string color in FACTION_COLORS)
        {
            placeInitialUnits(color);
        }
        if (GameSettings.HasNatives)
            placeNatives();
    }
    public void placeInitialUnits(Player player)
    {
        String color = player.FactionColor;
        placeInitialUnits(color);
    }
    public void placeInitialUnits(string color)
    {
        int width = GameSettings.Width;
        int height = GameSettings.Height;

        Unit comcen = new Unit();
        comcen.UnitType = COMMAND_CENTER;
        comcen.Color = color;
        Unit tank1 = new Unit();
        tank1.UnitType = ARMOR;
        tank1.Color = color;
        Unit tank2 = new Unit();
        tank2.UnitType = ARMOR;
        tank2.Color = color;
        Unit infantry = new Unit();
        infantry.UnitType = INFANTRY;
        infantry.Color = color;
        Unit spy = new Unit();
        spy.UnitType = SPY;
        spy.Color = color;

        Unit plane1 = new Unit();
        plane1.UnitType = AIRPLANE;
        plane1.Color = color;

        if (VISIBILITY_OMNISCIENT.Equals(GameSettings.Visibility))
        {
            comcen.setOmniVisibility();
            plane1.setOmniVisibility();
            spy.setOmniVisibility();
        }
        else
        {
            comcen.setBaseVisibility();
            plane1.setBaseVisibility();
            spy.setBaseVisibility();
        }
        if (color.Equals(AMBER))
        {
            MapHex metroHex = Map.getMetroHex(AMBER);
            List<string> directions = [DIRECTION_NORTH_WEST, DIRECTION_NORTH_EAST, DIRECTION_SOUTH_WEST, DIRECTION_SOUTH_EAST];
            placeUnit(metroHex, directions, comcen);
            Map.placeNewUnit(spy, metroHex);
            Faction faction = Factions.ColorToFaction[comcen.Color];
            faction.HasComCen = true;

            // Test with some large quantity of units
            if (1 == 0)
            {
                Unit decoyComcen = new Unit();
                decoyComcen.UnitType = DECOY_COMMAND_CENTER;
                decoyComcen.Color = color;
                decoyComcen.setBaseVisibility();
                placeUnit(Map.Hexes[5,5], directions, decoyComcen);

                for (int i=0; i< 100; i++)
                {
                    int x = rand.Next(5, 20);
                    int y = rand.Next(5, 20);
                    MapHex mapHex = Map.Hexes[y, x];
                    if (mapHex.getUnit() == null)
                    {
                        infantry = new Unit();
                        infantry.UnitType = INFANTRY;
                        infantry.Color = color;
                        //infantry.setOmniVisibility();
                        infantry.setBaseVisibility();
                        placeUnit(mapHex, infantry);
                    }    
                }
            }
        }
        else if (color.Equals(OCHER))
        {
            MapHex metroHex = Map.getMetroHex(OCHER);
            List<string> directions = [DIRECTION_NORTH_EAST, DIRECTION_NORTH_WEST, DIRECTION_SOUTH_WEST, DIRECTION_SOUTH_EAST];
            placeUnit(metroHex, directions, comcen);
            Map.placeNewUnit(spy, metroHex);
            Faction faction = Factions.ColorToFaction[comcen.Color];
            faction.HasComCen = true;
        }
        else if (color.Equals(CYAN))
        {
            MapHex metroHex = Map.getMetroHex(CYAN);
            List<string> directions = [DIRECTION_SOUTH_EAST, DIRECTION_NORTH_WEST, DIRECTION_SOUTH_WEST, DIRECTION_NORTH_EAST];
            placeUnit(metroHex, directions, comcen);
            Map.placeNewUnit(spy, metroHex);
            Faction faction = Factions.ColorToFaction[comcen.Color];
            faction.HasComCen = true;
        }
        else if (color.Equals(MAGENTA))
        {
            MapHex metroHex = Map.getMetroHex(MAGENTA);
            List<string> directions = [DIRECTION_SOUTH_WEST, DIRECTION_NORTH_WEST, DIRECTION_SOUTH_EAST, DIRECTION_NORTH_EAST];
            placeUnit(metroHex, directions, comcen);
            Map.placeNewUnit(spy, metroHex);
            Faction faction = Factions.ColorToFaction[comcen.Color];
            faction.HasComCen = true;
        }

        comcen.Airplane = plane1;
        plane1.ParentUnitId = comcen.Id;

    }

    private void placeUnit(MapHex metroHex, List<string> directions, Unit unit)
    {
        Dictionary<string, MapHex> surroundingHexes = Map.getSurroundingHexes(metroHex);
        foreach (string direction in directions)
        {
            if (surroundingHexes.ContainsKey(direction))
            {
                MapHex mapHex = surroundingHexes[direction];
                Map.placeNewUnit(unit, mapHex);
                break;
            }
        }
    }

    private void placeUnit(MapHex mapHex, Unit unit)
    {
        if (mapHex.getUnit() == null)
            Map.placeNewUnit(unit, mapHex);
    }

    private Unit createNativeInfantry(MapHex mapHex)
    {
        Unit unit = new Unit();
        unit.Color = NATIVE_COLOR;
        unit.UnitType = INFANTRY;
        if (VISIBILITY_OMNISCIENT.Equals(GameSettings.Visibility))
        {
            unit.setOmniVisibility();
        }
        else
        {
            unit.setBaseVisibility();
        }
        return unit;
    }

    private void placeNatives()
    {
        foreach (string key in Burbs.NameToBurb.Keys)
        {
            Burb burb = Burbs.NameToBurb[key];
            MapHex mapHex = Map.Hexes[burb.Y, burb.X];
            // villages 50% with a native
            // towns    100% with a native
            // cities   center + random surrounding natives
            // capital  natives in center and all surrounding spaces
            if (BURB_VILLAGE.Equals(burb.Type))
            {
                bool hasUnit = rand.NextDouble() >= 0.5;
                if (hasUnit)
                {
                    Unit unit = createNativeInfantry(mapHex);
                    placeUnit(mapHex, unit);
                }

            }
            else if (BURB_TOWN.Equals(burb.Type))
            {
                Unit unit = createNativeInfantry(mapHex);
                placeUnit(mapHex, unit);
            }
            else if (BURB_CITY.Equals(burb.Type))
            {
                Unit unit = createNativeInfantry(mapHex);
                placeUnit(mapHex, unit);
                List<MapHex> neighbors = Map.getSurroundingHexesList(mapHex);
                foreach (MapHex neighbor in neighbors)
                {
                    bool hasUnit = rand.NextDouble() >= 0.5;
                    if (neighbor.Burb != null && hasUnit)
                    {
                        Unit neighborUnit = createNativeInfantry(mapHex);
                        if (BURB_DOCK.Equals(neighbor.Burb.Type))
                            neighborUnit.UnitType = TRANSPORT_INFANTRY;
                        placeUnit(neighbor, neighborUnit);
                    }
                }
            }
            else if (BURB_CAPITAL.Equals(burb.Type))
            {
                Unit unit = createNativeInfantry(mapHex);
                placeUnit(mapHex, unit);
                List<MapHex> neighbors = Map.getSurroundingHexesList(mapHex);
                foreach (MapHex neighbor in neighbors)
                {
                    Unit neighborUnit = createNativeInfantry(mapHex);
                    if (neighbor.Burb != null && BURB_DOCK.Equals(neighbor.Burb.Type))
                    {
                        neighborUnit.UnitType = TRANSPORT_INFANTRY;
                    }
                    placeUnit(neighbor, neighborUnit);
                }
            }
        }
    }

    public void copyTransferredGameState(GameState newGameState)
    {
        if (newGameState == null)
            return;
        this.CurrentPhase = newGameState.CurrentPhase;
        this.CurrentRound = newGameState.CurrentRound;
        this.CurrentTurn = newGameState.CurrentTurn;
        if ((this.Burbs == null || Burbs.NameToBurb.Count <= 0) && newGameState.Burbs.NameToBurb.Count > 0)
        {
            this.Burbs = newGameState.Burbs;
        }
        this.Factions = newGameState.Factions;
        this.GameSettings = newGameState.GameSettings;
        this.Players = newGameState.Players;
        this.PlayerExecutionReady = newGameState.PlayerExecutionReady;
        this.PlayerJoined = newGameState.PlayerJoined;
        this.PlayerPlanningReady = newGameState.PlayerPlanningReady;
        this.SecondsRemainingUntilExecution = newGameState.SecondsRemainingUntilExecution;
        if ((this.UnitTypes == null || UnitTypes.UnitTypeMap.Count <= 0) && newGameState.UnitTypes.UnitTypeMap.Count > 0)
        {
            this.UnitTypes = newGameState.UnitTypes;
        }
        else
        {
            this.UnitTypes = new UnitTypes();
        }
        this.VictoriousColor = newGameState.VictoriousColor;

    }

    public override string ToString()
    {
        string returnString = "GameState: " + ToJson();
        return returnString;
    }
    public string ToJson()
    {
        return JsonSerializer.Serialize(this);
    }

}
