using System.Collections;
using System.Text.Json;
using GlobalConquest.Actions;

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

    public bool IsVisibleToAmber { get; set; } = true;
    public bool IsVisibleToOchre { get; set; } = true;
    public bool IsVisibleToMagenta { get; set; } = true;
    public bool IsVisibleToCyan { get; set; } = true;

    public ArrayList ActionQueue { get; set; } = new ArrayList();


    public Unit()
    {

    }

    public UnitAction getNextAction()
    {
        if (ActionQueue.Count < 1)
        {
            return null;
        }
        if (ActionQueue[0].GetType().Equals(new UnitAction().GetType()))
            return (UnitAction)ActionQueue[0];

        UnitAction unitAction =
            JsonSerializer.Deserialize<UnitAction>(ActionQueue[0].ToString());
        return unitAction;
    }

    public override bool Equals(object obj)
    {
        if (obj is Unit other)
        {
            return Owner == other.Owner &&
                UnitType == other.UnitType &&
                Color == other.Color &&
                Y == other.Y &&
                X == other.X &&
                IsVisibleToAmber == other.IsVisibleToAmber &&
                IsVisibleToCyan == other.IsVisibleToCyan &&
                IsVisibleToMagenta == other.IsVisibleToMagenta &&
                IsVisibleToOchre == other.IsVisibleToOchre;
        }
        //Console.WriteLine("Unit.Equals(): false");
        return false;
    }

    public override int GetHashCode()
    {
        // Combine hash codes of relevant properties
        return HashCode.Combine(Owner+Color, UnitType, Y, X, IsVisibleToAmber, IsVisibleToCyan, IsVisibleToMagenta, IsVisibleToOchre); 
    }

}