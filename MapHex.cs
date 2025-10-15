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

    public ArrayList Units { get; set; } = new ArrayList();

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

}