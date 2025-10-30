using System.Collections;
using System.Text.Json;

namespace GlobalConquest;

public class MapHex
{
    public string Terrain { get; set; }
    public int Y { get; set; }
    public int X { get; set; }

    public Dictionary<string, bool> Visibility { get; set; } = new Dictionary<string, bool>();

    public List<Unit> Units { get; set; } = new List<Unit>();

    public Burb? Burb { get; set; }

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
        if (Units.Count > 0)
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
        //Console.WriteLine("MapHex.Equals(): false");
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

}