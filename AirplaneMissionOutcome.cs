using GlobalConquest.Units;

namespace GlobalConquest;

public class AirplaneMissionOutcome
{
    public bool IsMissionSuccessful {get; set; }
    public bool IsPlaneShotDown {get; set; }
    public bool IsEnemyPlaneShotDown {get; set; }

    public bool IsShortRangeMission {get;set;}
    public bool IsMediumRangeMission {get;set;}
    public bool IsLongRangeMission {get;set;}

    public Unit? EnemyPlane {get; set;}

    public Unit Plane {get; set;}

    public AirplaneMissionOutcome()
    {}

}