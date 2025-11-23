using System.Collections;
using System.Runtime.InteropServices;
using System.Text.Json;
using GlobalConquest.Actions;
using Microsoft.Xna.Framework;

namespace GlobalConquest.Units;

public class Unit
{
    public string Id { get; set; }
    public Faction Owner { get; set; }
    public string UnitType { get; set; }
    public string Color { get; set; }
    public int OriginalBurbX { get; set; }
    public int OriginalBurbY { get; set; }
    public int HomeBurbX { get; set; }
    public int HomeBurbY { get; set; }
    public int UnitToPursueX { get; set; } = -1;
    public int UnitToPursueY { get; set; } = -1;
    public string UnitIdToPursue { get; set; }

    public int Y { get; set; }
    public int X { get; set; }

    public Unit? ParentUnit { get; set; }
    public Unit? Airplane { get; set; }

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
    public int RoundsToWait { get; set; } = 0;

    public int MoveSteps { get; set; } = 0;


    public Dictionary<string, bool> Visibility { get; set; } = new Dictionary<string, bool>();
    public Dictionary<string, int> RoundsToBeSeen { get; set; } = new Dictionary<string, int>();

    public List<UnitAction> ActionQueue { get; set; } = new List<UnitAction>();
    public List<UnitAction> Patrol { get; set; } = new List<UnitAction>();

    public Vector2 lastTargetUnitVector { get; set; } = new Vector2(-1, -1);

    public bool IsUnloading { get; set; } = false;
    public bool IsLoading { get; set; } = false;

    public bool IsBlitzing { get; set; } = false;

    public bool IsSneaking { get; set; } = false;

    public Unit()
    {
    }

    public string generateId()
    {
        string newId = Color + "." + UnitType + "." + OriginalBurbX + "." + OriginalBurbY + "." + DateTime.Now.Ticks;
        this.Id = newId;
        return newId;
    }

    public void setBaseVisibility()
    {
        Visibility["amber"] = false;
        Visibility["magenta"] = false;
        Visibility["ocher"] = false;
        Visibility["cyan"] = false;
        Visibility["grey"] = false;
        if (Color != null)
            Visibility[Color] = true;
    }

    public void setOmniVisibility()
    {
        Visibility["amber"] = true;
        Visibility["magenta"] = true;
        Visibility["ocher"] = true;
        Visibility["cyan"] = true;
        Visibility["grey"] = true;
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

    public void DeleteMoveUnitActions()
    {
        ActionQueue.Clear();
    }

    public void setUnitAction(UnitAction unitAction)
    {
        if (ActionQueue.Count < 1 || unitAction.Ticks > ActionQueue[0].Ticks)
        {
            ActionQueue.Clear();
            ActionQueue.Add(unitAction);
        }
    }

    public void addUnitAction(UnitAction unitAction)
    {
        if (ActionQueue.Count > 0)
        {
            UnitAction lastAction = ActionQueue[ActionQueue.Count - 1];
            if (unitAction.Ticks >= lastAction.Ticks)
            {
                ActionQueue.Add(unitAction);
            }
        }
        else
        {
            ActionQueue.Add(unitAction);
        }
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
        return HashCode.Combine(Owner + Color, UnitType, Y, X, Visibility);
    }

}
