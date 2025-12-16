using GlobalConquest;
using GlobalConquest.Units;

public class AiUnit
{
    public long Ticks { get; set; }
    public string UnitType { get; set; }
    public MapHex? InitialPosition { get; set; }
    public int DistanceFromTarget { get; set; }
    public bool ShouldMoveToTarget { get; set; } = false;
    public Unit? Unit { get; set; }
    public MapHex? LastMapHex { get; set; }
    public int BlockedRounds { get; set; } = 0;
    public string GoalTargetXy {get; set; }

    public AiUnit()
    {
        Ticks = DateTime.Now.Ticks;
    }


    public override bool Equals(object obj)
    {
        if (obj is AiUnit other)
        {
            if (Ticks == Ticks && UnitType == UnitType)
                return true;
        }
        return false;
    }

    public override int GetHashCode()
    {
        // Combine hash codes of relevant properties
        return HashCode.Combine(Ticks, UnitType);
    }

    public override string ToString()
    {
        string returnString = "AiUnit " + UnitType;
        if (Unit != null)
            returnString += ": " + Unit.Id + "; " + Unit.X + "," + Unit.Y;
        return returnString;
    }
}

