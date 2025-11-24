using GlobalConquest.Units;

namespace GlobalConquest;

public class AirplaneMissionOutcome
{
    public bool IsMissionSuccessful {get; set; }
    public bool IsPlaneShotDown {get; set; }
    public bool IsEnemyPlaneShotDown {get; set; }

    public Unit? EnemyPlane {get; set;}

    public AirplaneMissionOutcome()
    {}

}