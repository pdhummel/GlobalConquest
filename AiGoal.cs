using GlobalConquest;
using GlobalConquest.Units;

public class AiGoal
{
    public string Type { get; set; } // explore, defend, conquer
    public bool IsComplete { get; set; } = false;
    public bool IsGoalStarted { get; set; } = false;    // builds have begun
    public MapHex TargetMapHex { get; set; }
    public HashSet<AiUnit> DesiredUnits = new HashSet<AiUnit>();
    public HashSet<AiUnit> ActualUnits = new HashSet<AiUnit>();

    public int Enemies { get; set; }
    public bool ShouldMoveToTarget { get; set; } = false;
    public bool IsOngoingGoal { get; set; } = false;

    public bool UseRandomMovement { get; set; } = false;
    public int DifficultyScore {get; set; }
    Random random = new Random();


    public AiGoal()
    {
    }

    public AiUnit getNextUnitToBuild(bool IsLog=false)
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
        Dictionary<string, int> desiredUnitTypeCount = new Dictionary<string, int>();
        Dictionary<string, int> actualUnitTypeCount = new Dictionary<string, int>();
        List<string> desiredUnitTypes = new List<string>();
        foreach (AiUnit aiUnit in DesiredUnits)
        {
            if (!desiredUnitTypeCount.ContainsKey(aiUnit.UnitType))
                desiredUnitTypeCount[aiUnit.UnitType] = 1;
            else
                desiredUnitTypeCount[aiUnit.UnitType] += 1;
        }
        foreach (AiUnit aiUnit in ActualUnits)
        {
            if (!actualUnitTypeCount.ContainsKey(aiUnit.UnitType))
                actualUnitTypeCount[aiUnit.UnitType] = 1;
            else
                actualUnitTypeCount[aiUnit.UnitType] += 1;
        }
        foreach (string key in desiredUnitTypeCount.Keys)
        {
            int desiredCount = desiredUnitTypeCount[key];
            if (!actualUnitTypeCount.ContainsKey(key) || actualUnitTypeCount[key] < desiredCount)
            {
                desiredUnitTypes.Add(key);
                if (IsLog)
                    Globals.Log("getNextUnitToBuild(): need " + key + " for " + this);
            }
        }
        if (desiredUnitTypes.Count > 0)
        {
            int index = random.Next(desiredUnitTypes.Count);
            string desiredUnitType = desiredUnitTypes[index];
            foreach (AiUnit aiUnit in DesiredUnits)
            {
                if (aiUnit.UnitType.Equals(desiredUnitType) && (aiUnit.Unit == null || aiUnit.Unit.StrengthPoints <= 0))
                {
                    nextUnit = aiUnit;
                    break;
                }
            }
        }

        if (IsLog)
        {
            Globals.Log("getNextUnitToBuild(): " + this + ": nextUnit=" + nextUnit + ", DesiredUnits=" + DesiredUnits.Count + ", ActualUnits=" + ActualUnits.Count);
        }
        return nextUnit;
    }

    public int GetDesiredCountForUnitType(string unitType)
    {
        int count = 0;
        foreach (AiUnit aiUnit in DesiredUnits)
        {
            if ("transport-infantry".Equals(unitType) || "dug-in-infantry".Equals(unitType))
                unitType = "infantry";
            if (aiUnit.UnitType.Equals(unitType))
                count += 1;
        }
        return count;
    }

    public string GoalName()
    {
        string name = Type;
        if (TargetMapHex != null)
            name += " " + TargetMapHex.X + "," + TargetMapHex.Y;
        return name;
    }

    public override string ToString()
    {
        // Use string interpolation for a clean, readable format
        string stringValue = GoalName();
        if ("conquer".Equals(Type))
            stringValue += ", difficulty=" + DifficultyScore;
        return stringValue;
    }
}
