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

    // If this unit is a plane, it may have a ParentUnit such as a comcen or carrier.
    // When a plane is a child, the X,Y are inherited from the ParentUnit.
    // Alternatively, a plane could reside on a burb maphex and have a meaningful X and Y.
    public string? ParentUnitId { get; set; }

    // if this unit is a carrier or comcen, it may have an Airplane associated with it.
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

    // TODO: Add option, wait till repaired
    public int RoundsToWait { get; set; } = 0;

    public int MoveSteps { get; set; } = 0;

    // After flying a mission, a plane is unavailable for one turn 
    // (for short range missions and medium range transfers) 
    // or two turns (for the medium missions and long transfers). 
    // Kamikaze missions kill your plane.
    //
    // While resting, a plane cannot scan for enemy units 
    // (thus "seen" units may disappear)
    //
    // If your opponents attempt an air strike against your forces and the strike 
    // is within 10 spaces of your planes, 
    // your planes will automatically defend against the attack. 
    // If your plane survives this defense, it will need even more rest than usual. 
    // Planes need an additional 1/2 turn of rest (i.e., are unavailable) per attack they defend against.
    public float TurnsUnavailable {get; set; }


    public Dictionary<string, bool> Visibility { get; set; } = new Dictionary<string, bool>();
    public Dictionary<string, int> RoundsToBeSeen { get; set; } = new Dictionary<string, int>();

    public List<UnitAction> ActionQueue { get; set; } = new List<UnitAction>();
    public List<UnitAction> Patrol { get; set; } = new List<UnitAction>();

    public Vector2 lastTargetUnitVector { get; set; } = new Vector2(-1, -1);

    public bool IsUnloading { get; set; } = false;
    public bool IsLoading { get; set; } = false;

    public bool IsBlitzing { get; set; } = false;

    public bool IsSneaking { get; set; } = false;

    public bool IsAttacked {get;set;} = false;
    public bool IsAttacking {get; set;}

    public bool IsDefending {get; set;} = true;

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
        RoundsToBeSeen["amber"] = 0;
        RoundsToBeSeen["magenta"] = 0;
        RoundsToBeSeen["ocher"] = 0;
        RoundsToBeSeen["cyan"] = 0;
        RoundsToBeSeen["grey"] = 0;

        if (Color != null)
        {
            Visibility[Color] = true;
        }
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

    public Unit clone()
    {
        Unit unit = new Unit();

        unit.Color = Color;
        unit.X = X;
        unit.Y = Y;
        unit.UnitType = UnitType;
        unit.Airplane = Airplane;
        unit.ParentUnitId = ParentUnitId;
        unit.Id = Id;
        unit.HomeBurbX = HomeBurbX;
        unit.HomeBurbY = HomeBurbY;
        unit.Owner = Owner;
        unit.StrengthPoints = StrengthPoints;
        return unit;
    }

    public void updateGameStatePlane(GameState gameState, Unit planeWithNewValues)
    {
        Map map = gameState.Map;
        MapHex planeHex = map.Hexes[planeWithNewValues.Y, planeWithNewValues.X];
        if (planeWithNewValues.ParentUnitId != null)
        {
            if (map.UnitIdToUnit.ContainsKey(planeWithNewValues.ParentUnitId))
            {
                Unit parentUnit = map.UnitIdToUnit[planeWithNewValues.ParentUnitId];
                if (parentUnit != null && parentUnit.Airplane != null)
                    parentUnit.Airplane.copyPlaneValues(parentUnit.Airplane, planeWithNewValues);
            }
        }
        else if (planeHex.Airplane != null)
        {
            if (planeHex.Airplane != null)
                planeHex.Airplane.copyPlaneValues(planeHex.Airplane, planeWithNewValues);
        }
    }

    private void copyPlaneValues(Unit planeToUpdate, Unit planeWithNewValues)
    {
        //Id = unit.Id;
        //Color = unit.Color;
        //UnitType = unit.UnitType;
        //Owner = unit.Owner;
        //HomeBurbX = unit.HomeBurbX;
        //HomeBurbY = unit.HomeBurbY;
        //if (planeToUpdate.Airplane == null)
        //    Airplane = planeWithNewValues.Airplane;
        if (planeToUpdate.ParentUnitId == null)
            planeToUpdate.ParentUnitId = planeWithNewValues.ParentUnitId;
        planeToUpdate.X = planeWithNewValues.X;
        planeToUpdate.Y = planeWithNewValues.Y;
        planeToUpdate.StrengthPoints = planeWithNewValues.StrengthPoints;
        planeToUpdate.TurnsUnavailable = planeWithNewValues.TurnsUnavailable;
    }

    private void copyValues(Unit unit)
    {
        //Id = unit.Id;
        //Color = unit.Color;
        //UnitType = unit.UnitType;
        //Owner = unit.Owner;
        //HomeBurbX = unit.HomeBurbX;
        //HomeBurbY = unit.HomeBurbY;
        if (Airplane == null)
            Airplane = unit.Airplane;
        if (ParentUnitId == null)
            ParentUnitId = unit.ParentUnitId;
        X = unit.X;
        Y = unit.Y;
        StrengthPoints = unit.StrengthPoints;
        TurnsUnavailable = unit.TurnsUnavailable;
        IsBlitzing = unit.IsBlitzing;
        IsSneaking = unit.IsSneaking;
        IsLoading = unit.IsLoading;
        IsUnloading = unit.IsUnloading;
        //Visibility = unit.Visibility;
        //ActionQueue = unit.ActionQueue;
        UnitIdToPursue = unit.UnitIdToPursue;
        UnitToPursueX = unit.UnitToPursueX;
        UnitToPursueY = unit.UnitToPursueY;
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
        //Globals.Log("Unit.Equals(): false");
        return false;
    }

    public override int GetHashCode()
    {
        // Combine hash codes of relevant properties
        return HashCode.Combine(Owner + Color, UnitType, Y, X, Visibility);
    }

}
