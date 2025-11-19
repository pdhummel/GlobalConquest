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

    public AiUnit()
    {
        Ticks = DateTime.Now.Ticks;
    }


    public override bool Equals(object obj)
    {
        if (obj is MapHex other)
        {
            return Ticks == Ticks;
        }
        return false;
    }

    public override int GetHashCode()
    {
        // Combine hash codes of relevant properties
        return HashCode.Combine(Ticks, UnitType);
    }

}
