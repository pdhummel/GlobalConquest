using System.Collections;

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
        if (Units.Count > 0)
        {
            return (Unit)Units[0];
        }
        else
        {
            return null;
        }
    }

    public void setUnit(Unit unit)
    {
        Units[0] = unit;
    }

}