using static UnitConstants;
using System.Text.Json;
using System.Text.Json.Serialization;
using GlobalConquest.Units;
using static GlobalConquest.Map;
using static GlobalConquest.Burbs;
using static GameConstants;
namespace GlobalConquest;

public class GameState
{
    [JsonPropertyName("GS")]
    public GameSettings GameSettings { get; set; }
    [JsonPropertyName("F")]
    public Factions Factions { get; set; }

    [JsonPropertyName("P")]
    public Players Players { get; set; }

    [JsonIgnore]
    public Map Map { get; set; }

    [JsonPropertyName("MP")]
    public MapHex MapHex { get; set; }

    [JsonPropertyName("PER")]
    public Dictionary<string, bool> PlayerExecutionReady { get; set; } = new Dictionary<string, bool>();
    [JsonPropertyName("PPR")]
    public Dictionary<string, bool> PlayerPlanningReady { get; set; } = new Dictionary<string, bool>();
    [JsonPropertyName("PJ")]
    public Dictionary<string, bool> PlayerJoined { get; set; } = new Dictionary<string, bool>();
    [JsonPropertyName("CT")]
    public int CurrentTurn { get; set; } = 0;
    [JsonPropertyName("CP")]
    public string CurrentPhase { get; set; } = GAME_PHASE_PLAN;  // plan, execution, gameOver
    [JsonPropertyName("CR")]
    public int CurrentRound { get; set; } = 0;
    [JsonPropertyName("UT")]
    public UnitTypes UnitTypes { get; set; }
    public Burbs Burbs { get; set; }

    [JsonPropertyName("VC")]
    public string VictoriousColor { get; set; } = NATIVE_COLOR;
    public long Ticks { get; set; } = 0;

    [JsonPropertyName("SRUE")]
    public int SecondsRemainingUntilExecution {get; set;}

    // if any of the data elements in the entities change above, then this version should be bumped.
    public string Version { get; set; } = "v0.9.0";
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

        MapHex metroHex = Map.getMetroHex(color);
        List<string> directions = [];
        if (color.Equals(AMBER))
        {
            directions = [DIRECTION_NORTH_WEST, DIRECTION_NORTH_EAST, DIRECTION_SOUTH_WEST, DIRECTION_SOUTH_EAST];
            // testPlacement(color, directions)
        }
        else if (color.Equals(OCHER))
        {
            directions = [DIRECTION_NORTH_EAST, DIRECTION_NORTH_WEST, DIRECTION_SOUTH_WEST, DIRECTION_SOUTH_EAST];
        }
        else if (color.Equals(CYAN))
        {
            directions = [DIRECTION_SOUTH_EAST, DIRECTION_NORTH_WEST, DIRECTION_SOUTH_WEST, DIRECTION_NORTH_EAST];
        }
        else if (color.Equals(MAGENTA))
        {
            directions = [DIRECTION_SOUTH_WEST, DIRECTION_NORTH_WEST, DIRECTION_SOUTH_EAST, DIRECTION_NORTH_EAST];
        }

        Unit comcen = new Unit();
        comcen.UnitType = COMMAND_CENTER;
        comcen.Color = color;
        Unit spy = new Unit();
        spy.UnitType = SPY;
        spy.Color = color;
        Unit plane = new Unit();
        plane.UnitType = AIRPLANE;
        plane.Color = color;
        Unit infantry = new Unit();
        infantry.UnitType = INFANTRY;
        infantry.Color = color;
        Unit tank = new Unit();
        tank.UnitType = ARMOR;
        tank.Color = color;
        Unit carrier = new Unit();
        carrier.UnitType = AIRCRAFT_CARRIER;
        carrier.Color = color;
        Unit battleship = new Unit();
        battleship.UnitType = BATTLESHIP;
        battleship.Color = color;

        Faction faction = Factions.ColorToFaction[color];
        faction.HasComCen = true;
        if (GameSettings.UnitPalette.Equals(UNIT_PALETTE_NAME_ORIGINAL_GC) || GameSettings.UnitPalette.Equals(UNIT_PALETTE_NAME_EXTENDED))
        {
            placeUnit(metroHex, directions, comcen);
            Map.placeNewUnit(spy, metroHex);
            comcen.Airplane = plane;
            plane.ParentUnitId = comcen.Id;
            List<Unit> units = [comcen, spy, plane];
            setInitialUnitsVisibility(units);
        }
        else if (GameSettings.UnitPalette.Equals(UNIT_PALETTE_NAME_COMCEN))
        {
            placeUnit(metroHex, directions, comcen);
            Map.placeNewUnit(infantry, metroHex);
            comcen.Airplane = plane;
            plane.ParentUnitId = comcen.Id;
            List<Unit> units = [comcen, infantry, plane];
            setInitialUnitsVisibility(units);
        }
        else if (GameSettings.UnitPalette.Equals(UNIT_PALETTE_NAME_WW2))
        {
            placeUnit(metroHex, directions, infantry);
            Map.placeNewUnit(carrier, metroHex);
            carrier.Airplane = plane;
            plane.ParentUnitId = carrier.Id;
            List<Unit> units = [carrier, infantry, plane];
            setInitialUnitsVisibility(units);
        }
        else if (GameSettings.UnitPalette.Equals(UNIT_PALETTE_NAME_BASIC))
        {
            placeUnit(metroHex, directions, infantry);
            Map.placeNewUnit(battleship, metroHex);
            List<Unit> units = [battleship, infantry];
            setInitialUnitsVisibility(units);
        }
        else if (GameSettings.UnitPalette.Equals(UNIT_PALETTE_NAME_CHQ1918))
        {
            placeUnit(metroHex, directions, infantry);
            Map.placeNewUnit(battleship, metroHex);
            List<Unit> units = [battleship, infantry];
            setInitialUnitsVisibility(units);
        }
        else if (GameSettings.UnitPalette.Equals(UNIT_PALETTE_NAME_INFANTRY))
        {
            Map.placeNewUnit(infantry, metroHex);
            List<Unit> units = [infantry];
            setInitialUnitsVisibility(units);
        }
    }

    private void setInitialUnitsVisibility(List<Unit> units)
    {
        foreach (Unit unit in units)
        {
            if (VISIBILITY_OMNISCIENT.Equals(GameSettings.Visibility))
            {
                unit.setOmniVisibility();
            }
            else
            {
                unit.setBaseVisibility();
            }
        }
    }

    private void testPlacement(string color, List<string> directions)
    {
        Unit infantry = new Unit();
        infantry.UnitType = INFANTRY;
        infantry.Color = color;

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

    public int GetActiveFactionCount()
    {
        int activeCount = 0;
        foreach (string color in FACTION_COLORS)
        {
            Faction faction = Factions.ColorToFaction[color];
            // If CanLoseComCen is true, all factions are active regardless of HasComCen
            // Otherwise, only factions with HasComCen are active
            if (GameSettings.CanLoseComCen || faction.HasComCen)
            {
                activeCount++;
            }
        }
        return activeCount;
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
