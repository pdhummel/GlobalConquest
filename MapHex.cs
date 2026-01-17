using static UnitConstants;
using System.Text.Json;
using GlobalConquest.Units;
using static GameConstants;
using System.Text.Json.Serialization;
namespace GlobalConquest;

public class MapHex
{
    public string Terrain { get; set; } // sea, grass, mountain, swamp, forest
    public int Y { get; set; }
    public int X { get; set; }

    [JsonPropertyName("V")]
    public Dictionary<string, bool> Visibility { get; set; } = new Dictionary<string, bool>();
    [JsonPropertyName("TSV")]
    public Dictionary<string, bool> TemporarySpyVisibility { get; set; } = new Dictionary<string, bool>();

    [JsonPropertyName("U")]
    public Unit Unit { get; set; }
    [JsonPropertyName("A")]
    public Unit Airplane { get; set; }

    [JsonPropertyName("B")]
    public Burb? Burb { get; set; }
    [JsonPropertyName("iH")]
    public bool IsHighlighted {get; set;}

    public MapHex()
    {
        Visibility[AMBER] = false;
        Visibility[MAGENTA] = false;
        Visibility[OCHER] = false;
        Visibility[CYAN] = false;
    }

    public void makeVisibleToAll()
    {
        Visibility[AMBER] = true;
        Visibility[MAGENTA] = true;
        Visibility[OCHER] = true;
        Visibility[CYAN] = true;
    }

    public Unit getUnit()
    {
        return this.Unit;
    }

    public void setUnit(Unit unit)
    {
        this.Unit = unit;
    }

    public Unit getAnyAirplaneAtHex()
    {
        if (Airplane != null)
            return Airplane;
        Unit unit = getUnit();
        if (AIRPLANE.Equals(unit.UnitType))
        {
            return unit;
        }
        if (unit != null)
            return unit.Airplane;
        return null;
    }

    public override bool Equals(object obj)
    {
        if (obj is MapHex other)
        {
            return Terrain == other.Terrain &&
                Y == other.Y &&
                X == other.X;
        }
        //Globals.Log("MapHex.Equals(): false");
        return false;
    }

    public override int GetHashCode()
    {
        // Combine hash codes of relevant properties
        Unit unit = getUnit();
        int unitHashCode = 0;
        if (unit != null)
            unitHashCode = unit.GetHashCode();
        return HashCode.Combine(Terrain, Y, X, Visibility, unitHashCode);
    }

    public override string ToString()
    {
        string returnString = "MapHex " + Terrain + ";" + X + "," + Y;
        return returnString;
    }

    public void copyMapHexValues(MapHex mapHex)
    {
        if (mapHex == null)
            return;
        this.Airplane = mapHex.Airplane;
        if (this.Burb == null)
            this.Burb = mapHex.Burb;
        this.IsHighlighted = mapHex.IsHighlighted;
        this.Terrain = mapHex.Terrain;
        this.Visibility = mapHex.Visibility;
        this.TemporarySpyVisibility = mapHex.TemporarySpyVisibility;
        this.Unit = mapHex.Unit;
        this.X = mapHex.X;
        this.Y = mapHex.Y;
    }

    public bool IsVisibleToColor(string color)
    {
        bool isVisible = false;
        if (Visibility.ContainsKey(color))
            isVisible = Visibility[color];
        if (!isVisible && TemporarySpyVisibility.ContainsKey(color))
            isVisible = TemporarySpyVisibility[color];
        return isVisible;
    }

}
