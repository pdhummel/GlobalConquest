using GlobalConquest;
using GlobalConquest.Units;

public class AiGoal
{
    public string Type { get; set;} // explore, defend, conquer
    public bool IsComplete {get; set; } = false;
    public MapHex TargetMapHex { get; set; }
    public HashSet<AiUnit> DesiredUnits = new HashSet<AiUnit>();
    public HashSet<AiUnit> ActualUnits = new HashSet<AiUnit>();

    public int Enemies { get; set;}
    public bool ShouldMoveToTarget { get; set; } = false;
    public bool IsOngoingGoal { get; set; } = false;

    public bool UseRandomMovement { get; set; } = false;


    public AiGoal()
    {
    }

    public AiUnit getNextUnitToBuild()
    {
        HashSet<AiUnit> newActualUnits = new HashSet<AiUnit>();
        foreach (AiUnit aiUnit in ActualUnits)
        {
            Unit? unit = aiUnit.Unit;
            if (unit != null && unit.StrengthPoints > 0)
            {
                newActualUnits.Add(aiUnit);
            }
        }
        ActualUnits = newActualUnits;

        AiUnit nextUnit = null;
        HashSet<AiUnit> tempSet = new HashSet<AiUnit>(DesiredUnits);
        tempSet.ExceptWith(ActualUnits);
        if (tempSet.Count > 0)
            nextUnit = tempSet.ToList()[0];
        return nextUnit;
    }
}
