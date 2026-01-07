using static UnitTypeConstants;
using System.Text.Json;
using GlobalConquest.Units;
namespace GlobalConquest;

public class MapHex
{
    public string Terrain { get; set; } // sea, grass, mountain, swamp, forest
    public int Y { get; set; }
    public int X { get; set; }

    public Dictionary<string, bool> Visibility { get; set; } = new Dictionary<string, bool>();

    public List<Unit> Units { get; set; } = new List<Unit>();
    public Unit Airplane { get; set; }

    public Burb? Burb { get; set; }
    public bool IsHighlighted {get; set;}

    public MapHex()
    {
        Visibility["amber"] = false;
        Visibility["magenta"] = false;
        Visibility["ocher"] = false;
        Visibility["cyan"] = false;
    }

    public void makeVisibleToAll()
    {
        Visibility["amber"] = true;
        Visibility["magenta"] = true;
        Visibility["ocher"] = true;
        Visibility["cyan"] = true;
    }

    public Unit getUnit()
    {
        Unit tmpUnit = new Unit();
        if (Units.Count > 0 && Units[0] != null)
        {
            if (Units[0].GetType().Equals(tmpUnit.GetType()))
                return (Unit)Units[0];

            Unit unit =
                JsonSerializer.Deserialize<Unit>(Units[0].ToString());
            return unit;
        }
        else
        {
            return null;
        }
    }

    public void setUnit(Unit unit)
    {
        if (Units.Count > 0)
            Units[0] = unit;
        else
            Units.Add(unit);
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
                X == other.X &&
                Visibility["amber"] == other.Visibility["amber"] &&
                Visibility["cyan"] == other.Visibility["cyan"] &&
                Visibility["magenta"] == other.Visibility["magenta"] &&
                Visibility["ocher"] == other.Visibility["ocher"];
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
        if (this.Units == null)
            this.Units = mapHex.Units;
        if (Units != null && Units.Count < 1 && mapHex.Units != null && mapHex.Units.Count > 0)
            this.Units.Add(mapHex.Units[0]);
        else if (Units != null && Units.Count > 0 && mapHex.Units != null && mapHex.Units.Count > 0)
            this.Units[0] = mapHex.Units[0];
        else if (Units != null && mapHex.Units != null && mapHex.Units.Count <= 0)
            this.Units.Clear();
        this.X = mapHex.X;
        this.Y = mapHex.Y;
    }

}
