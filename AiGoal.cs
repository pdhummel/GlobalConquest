using GlobalConquest;
using GlobalConquest.Units;

public class AiGoal
{
    public MapHex TargetMapHex { get; set; }
    public HashSet<AiUnit> DesiredUnits = new HashSet<AiUnit>();
    public HashSet<AiUnit> ActualUnits = new HashSet<AiUnit>();

    public bool ShouldMoveToTarget { get; set; } = false;
    public bool IsOngoingGoal { get; set; } = false;

    public bool UseRandomMovement { get; set; } = false;


    public AiGoal()
    {
    }

    public AiUnit getNextUnitToBuild()
    {
        AiUnit nextUnit = null;
        return nextUnit;
    }
}