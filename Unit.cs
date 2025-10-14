namespace GlobalConquest;

public class Unit
{
    public Faction Owner { get; set; }
    public string UnitType { get; set; } = "infantry"; // infantry, tank, plane, ComCen, carrier, battleship, spy
    public string Color { get; set; }

    public int Y { get; set; }
    public int X { get; set; }

    public int ActionPoints { get; set; }
    public Dictionary<string, int> MovementCostThroughTerrain = new Dictionary<string, int>();
    public int AttackDamagePoints { get; set; }
    public int RemainingDamagePoints { get; set; }


    public Unit()
    {

    }
}