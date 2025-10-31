using System.Collections;
using System.Runtime.InteropServices;
using System.Text.Json;
using GlobalConquest.Actions;

namespace GlobalConquest;

public class Unit
{
    public Faction Owner { get; set; }
    public string UnitType { get; set; }
    public string Color { get; set; }

    public int Y { get; set; }
    public int X { get; set; }

    // Global Conquest Manual - Technical Notes - p79-83
    // Units start with 100 strength points. When strength equals zero the unit is dead.
    public int StrengthPoints { get; set; } = 100;

    // When an infantry or armor unit moves from land to sea,
    // it will pause for four rounds to load into its transports and to have a 
    // marshmallow roast there on the beach. 
    // When going from transport to land (unloading), it will take eight rounds. 
    // If the beach square has a friendly dug-in infantry unit squatting in it, 
    // this loading/unloading takes only one round.
    public int RoundsToPause { get; set; } = 0;



    public int NormalSteps { get; set; } = 0;
    public int BlitzSteps { get; set; } = 0;
    public int SneakSteps { get; set; } = 0;
    public Dictionary<string, int> MovementCostThroughTerrain { get; set; } = new Dictionary<string, int>();
    public int AttackDamagePoints { get; set; }
    public int RemainingDamagePoints { get; set; }


    public Dictionary<string, bool> Visibility { get; set; } = new Dictionary<string, bool>();
    public Dictionary<string, int> RoundsToBeSeen { get; set; } = new Dictionary<string, int>();

    public List<UnitAction> ActionQueue { get; set; } = new List<UnitAction>();


    public Unit()
    {
    }

    public void setBaseVisibility()
    {
        Visibility["amber"] = false;
        Visibility["magenta"] = false;
        Visibility["ocher"] = false;
        Visibility["cyan"] = false;
        if (Color != null)
            Visibility[Color] = true;
    }

    public void setOmniVisibility()
    {
        Visibility["amber"] = true;
        Visibility["magenta"] = true;
        Visibility["ocher"] = true;
        Visibility["cyan"] = true;
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

    public void setUnitAction(UnitAction unitAction)
    {
        if (ActionQueue.Count < 1)
            ActionQueue.Add(unitAction);
        else
            ActionQueue[0] = unitAction;
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
                Visibility["amber"] == other.Visibility["amber"] &&
                Visibility["cyan"] == other.Visibility["cyan"] &&
                Visibility["magenta"] == other.Visibility["magenta"] &&
                Visibility["ocher"] == other.Visibility["ocher"];
        }
        //Console.WriteLine("Unit.Equals(): false");
        return false;
    }

    public override int GetHashCode()
    {
        // Combine hash codes of relevant properties
        return HashCode.Combine(Owner+Color, UnitType, Y, X, Visibility); 
    }

}