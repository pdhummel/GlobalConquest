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

    public AiUnit()
    {
        Ticks = DateTime.Now.Ticks;
    }
}