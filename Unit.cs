namespace GlobalConquest;

public class Unit
{
    Faction Owner { get; set; }
    string UnitType { get; set; } = "infantry";

    int Y { get; set; }
    int X { get; set; }

    public Unit()
    {

    }
}