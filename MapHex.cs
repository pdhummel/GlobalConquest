using System.Collections;
using System.Text.Json;

namespace GlobalConquest;

public class MapHex
{
    public string Terrain { get; set; }
    public int Y { get; set; }
    public int X { get; set; }

    public bool IsVisibleToAmber { get; set; } = true;
    public bool IsVisibleToOchre { get; set; } = true;
    public bool IsVisibleToMagenta { get; set; } = true;
    public bool IsVisibleToCyan { get; set; } = true;

    public List<Unit> Units { get; set; } = new List<Unit>();

    public Burb? Burb { get; set; }

    public MapHex()
    {

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
                IsVisibleToAmber == other.IsVisibleToAmber &&
                IsVisibleToCyan == other.IsVisibleToCyan &&
                IsVisibleToMagenta == other.IsVisibleToMagenta &&
                IsVisibleToOchre == other.IsVisibleToOchre;
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
        return HashCode.Combine(Terrain, Y, X, IsVisibleToAmber, IsVisibleToCyan, IsVisibleToMagenta, IsVisibleToOchre, unitHashCode); 
    }

}