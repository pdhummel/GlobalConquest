namespace GlobalConquest.Units;

public class PlaneUnitType : UnitType
{

    public int shortRangeHexes = 3;
    public int mediumRangeHexes = 6;
    public int longRangeHexes = 12;



    private Random rand = new System.Random();

    public PlaneUnitType()
    {

    }

    // Note that all missions have a chance of failure.
    // Missions executed in the outer, "medium range" circle are half as effective 
    // and/or twice as dangerous as the inner, "short range" circle.
    // Recon missions uncover any terrain within a radius of 8 spaces from the chosen spot 
    // and any units within 12 spaces.
    //
    // If your target is on top of or next to an enemy plane, a dogfight ensues. 
    // If the opposition's plane is unavailable (resting, grounded) then the chances are: 
    // 25% chance of completing the mission.
    // 40% chance of mission failure; 
    // 10% chance of your plane being shot down; 
    // and 25% chance that your foe's plane is shot down.
    //
    // If the opposition's plane is available, your chances are: 
    // 50% chance of mission failure; 
    // 25% chance your plane is shot down; 
    // and 25% your opponent's plane is shot down.
    //
    // If your target is between two and 10 spaces from an enemy plane, 
    // the probability of your air mission failing ranges anywhere from 10% to 50%; 
    // one third of those failures will end up resulting in a lost plane.
    //
    // Even if there are no enemy planes within 10 spaces, 
    // all air missions still have a 10% chance of failure.
    public AirplaneMissionOutcome determineMissionOutcome(GameState gameState, Unit plane, MapHex targetMapHex)
    {
        Map map = gameState.Map;
        AirplaneMissionOutcome outcome = new AirplaneMissionOutcome();
        bool isDogfight = false;
        bool isEnemyGrounded = true;
        HashSet<MapHex> dogFightHexes = map.getMapHexesInRange(targetMapHex, 1);
        Unit enemyPlane = getEnemyPlaneForDogfight(gameState, targetMapHex);
        if (enemyPlane != null)
        {
            isDogfight = true;
            if (enemyPlane.turnsUnavailable > 0)
                isEnemyGrounded = true;
            else
                isEnemyGrounded = false;
        }

        if (!isDogfight)
        {
            enemyPlane = getNearbyEnemyPlane(gameState, targetMapHex);
        }

        if (enemyPlane != null)
        {
            enemyPlane.turnsUnavailable += 0.5f;
        }
        int multiplier = 1;
        if (isMediumRangeMission(gameState, targetMapHex))
            multiplier = 2;
        outcome = resolveMission(gameState, plane, isDogfight, isEnemyGrounded, enemyPlane, multiplier);
        return outcome;
    }

    AirplaneMissionOutcome resolveMission(GameState gameState, Unit plane, 
      bool isDogfight, bool isEnemyGrounded, Unit? enemyPlane, int multiplier)
    {
        AirplaneMissionOutcome outcome = new AirplaneMissionOutcome();
        int chance = rand.Next(0, 100);

        if (isDogfight && isEnemyGrounded)
        {
            outcome.EnemyPlane = enemyPlane;
            
            if (chance < 10)
            {
                outcome.IsPlaneShotDown = true;
                outcome.IsMissionSuccessful = false;
                handlePlaneShotDown(gameState, plane);
            }
            else if (chance < 35)
            {
                outcome.IsEnemyPlaneShotDown = true;
                outcome.IsMissionSuccessful = false;
                handlePlaneShotDown(gameState, enemyPlane);
            }
            else if (chance < 75)
            {
                outcome.IsMissionSuccessful = false;
            }
            else
            {
                outcome.IsMissionSuccessful = true;
            }
        }

        else if (isDogfight && !isEnemyGrounded)
        {
            outcome.EnemyPlane = enemyPlane;
            if (chance < 25)
            {
                outcome.IsPlaneShotDown = true;
                outcome.IsMissionSuccessful = false;
                handlePlaneShotDown(gameState, plane);
            }
            else if (chance < 50)
            {
                outcome.IsEnemyPlaneShotDown = true;
                outcome.IsMissionSuccessful = false;
                handlePlaneShotDown(gameState, enemyPlane);
            }
            else
            {
                outcome.IsMissionSuccessful = true;
            }
        }
        else if (enemyPlane != null)
        {
            outcome.EnemyPlane = enemyPlane;
            int randomness = rand.Next(10, 50+1);
            if (chance < randomness)
            {
                outcome.IsMissionSuccessful = false;
                int planeLostChance = rand.Next(0, 100);
                if (planeLostChance < 33)
                {
                    outcome.IsPlaneShotDown = true;
                    handlePlaneShotDown(gameState, plane);
                }
            }
            else
            {
                outcome.IsMissionSuccessful = true;
            }
        }
        else
        {
            if (chance < 10)
            {
                outcome.IsMissionSuccessful = false;
            }
            else
                outcome.IsMissionSuccessful = true;
        }
        return outcome;
    }

    void handlePlaneShotDown(GameState gameState, Unit plane)
    {
        Map map = gameState.Map;
        MapHex planeHex = map.Hexes[plane.Y, plane.X];
        plane.StrengthPoints = 0;
        if (plane.ParentUnitId != null)
        {
            if (map.UnitIdToUnit.ContainsKey(plane.ParentUnitId))
            {
                Unit parentUnit = map.UnitIdToUnit[plane.ParentUnitId];
                parentUnit.Airplane = null;
            }
            plane.ParentUnitId = null;
        }
        else if (planeHex.Airplane != null)
        {
            planeHex.Airplane = null;
        }
        
    }

    Unit getEnemyPlaneForDogfight(GameState gameState, MapHex targetMapHex)
    {
        Map map = gameState.Map;
        HashSet<MapHex> dogFightHexes = map.getMapHexesInRange(targetMapHex, 1);
        Unit enemyPlane = null;
        foreach (MapHex mapHex in dogFightHexes)
        {
            Unit hexUnit = mapHex.getUnit();
            if (mapHex.Airplane != null && enemyPlane == null && mapHex.Airplane.turnsUnavailable <= 0)
            {
                enemyPlane = mapHex.Airplane;
                break;
            }
            else if (hexUnit != null && hexUnit.Airplane != null && enemyPlane == null && hexUnit.Airplane.turnsUnavailable <= 0)
            {
                enemyPlane = hexUnit.Airplane;
                break;
            }
            else if (mapHex.Airplane != null && enemyPlane == null)
            {
                enemyPlane = mapHex.Airplane;
            }
            else if (hexUnit != null && hexUnit.Airplane != null && enemyPlane == null)
            {
                enemyPlane = hexUnit.Airplane;
            }
        }

        return enemyPlane;
    }

    Unit getNearbyEnemyPlane(GameState gameState, MapHex targetMapHex)
    {
        Unit enemyPlane = null;
        Map map = gameState.Map;
        HashSet<MapHex> enemyPlaneHexes = map.getMapHexesInRange(targetMapHex, 10);
        foreach (MapHex mapHex in enemyPlaneHexes)
        {
            Unit hexUnit = mapHex.getUnit();
            if (mapHex.Airplane != null)
            {
                enemyPlane = mapHex.Airplane;
                break;
            }
            else if (hexUnit != null && hexUnit.Airplane != null)
            {
                enemyPlane = hexUnit.Airplane;
                break;
            }
        }
        return enemyPlane;
    }

    bool isShortRangeMission(GameState gameState, MapHex targetMapHex)
    {
        Map map = gameState.Map;
        bool isShortRange = false;
        PlaneUnitType planeType = new PlaneUnitType();

        HashSet<MapHex> shortRangeHexes = map.getMapHexesInRange(targetMapHex, planeType.shortRangeHexes);
        if (shortRangeHexes.Contains(targetMapHex))
            isShortRange = true;
        return isShortRange;
    }

    bool isMediumRangeMission(GameState gameState, MapHex targetMapHex)
    {
        Map map = gameState.Map;
        bool isMediumRange = false;
        PlaneUnitType planeType = new PlaneUnitType();

        HashSet<MapHex> mediumRangeHexes = map.getMapHexesInRange(targetMapHex, planeType.mediumRangeHexes);
        if (mediumRangeHexes.Contains(targetMapHex))
            isMediumRange = true;

        return isMediumRange;
    }

    public UnitType definePlane()
    {
        UnitType unitType = new UnitType();
        unitType.Name = "plane";
        unitType.LandOrSea = "neither";



        // can't become dug-in infantry in swamp
        unitType.AttritionByTerrain["swamp"] = 0;

        // column
        unitType.BattleDamageFromAttacker["infantry"] = 0;
        unitType.BattleDamageFromAttacker["armor"] = 0;
        unitType.BattleDamageFromAttacker["tank"] = 0;
        unitType.BattleDamageFromAttacker["dug-in-infantry"] = 0;
        unitType.BattleDamageFromAttacker["transport-infantry"] = 0;
        unitType.BattleDamageFromAttacker["transport-armor"] = 0;
        unitType.BattleDamageFromAttacker["transport-tank"] = 0;
        unitType.BattleDamageFromAttacker["submarine"] = 0;
        unitType.BattleDamageFromAttacker["sub"] = 0;
        unitType.BattleDamageFromAttacker["battleship"] = 0;
        unitType.BattleDamageFromAttacker["carrier"] = 0;
        unitType.BattleDamageFromAttacker["spy"] = 0;
        unitType.BattleDamageFromAttacker["com"] = 0;
        unitType.BattleDamageFromAttacker["ComCen"] = 0;
        unitType.BattleDamageFromAttacker["comcen"] = 0;
        unitType.BattleDamageFromAttacker["CommandCenter"] = 0;

        // row
        unitType.BattleDamageToDefender["infantry"] = 0;
        unitType.BattleDamageToDefender["armor"] = 0;
        unitType.BattleDamageToDefender["tank"] = 0;
        unitType.BattleDamageToDefender["dug-in-infantry"] = 0;
        unitType.BattleDamageToDefender["transport-infantry"] = 0;
        unitType.BattleDamageToDefender["transport-armor"] = 0;
        unitType.BattleDamageToDefender["transport-tank"] = 0;
        unitType.BattleDamageToDefender["submarine"] = 0;
        unitType.BattleDamageToDefender["sub"] = 0;
        unitType.BattleDamageToDefender["battleship"] = 0;
        unitType.BattleDamageToDefender["carrier"] = 0;
        unitType.BattleDamageToDefender["spy"] = 0;
        unitType.BattleDamageToDefender["com"] = 0;
        unitType.BattleDamageToDefender["ComCen"] = 0;
        unitType.BattleDamageToDefender["comcen"] = 0;
        unitType.BattleDamageToDefender["CommandCenter"] = 0;

        unitType.NormalStepsAddedPerRound = 0;
        unitType.BlitzStepsAddedPerRound = 0;
        unitType.SneakStepsAddedPerRound = 0;

        unitType.Cost = 35;

        unitType.DamageReductionForDefenderByTerrain["burb"] = 0;
        unitType.DamageReductionForDefenderByTerrain["mountain"] = 0;

        // Airplanes automatically scan for units and terrain within 12 spaces.
        // Recon missions uncover any terrain within a radius of 8 spaces 
        // from the chosen spot and any units within 12 spaces.
        unitType.DiscoveryRange = 12;
        unitType.ScanningRange = 12;
        unitType.PointsPerHit = 10;

        // column
        unitType.FiringRangeFromAttacker["infantry"] = 0;
        unitType.FiringRangeFromAttacker["armor"] = 0;
        unitType.FiringRangeFromAttacker["tank"] = 0;
        unitType.FiringRangeFromAttacker["dug-in-infantry"] = 0;
        unitType.FiringRangeFromAttacker["transport-infantry"] = 0;
        unitType.FiringRangeFromAttacker["transport-armor"] = 0;
        unitType.FiringRangeFromAttacker["transport-tank"] = 0;
        unitType.FiringRangeFromAttacker["submarine"] = 0;
        unitType.FiringRangeFromAttacker["sub"] = 0;
        unitType.FiringRangeFromAttacker["battleship"] = 0;
        unitType.FiringRangeFromAttacker["carrier"] = 0;
        unitType.FiringRangeFromAttacker["spy"] = 0;
        unitType.FiringRangeFromAttacker["com"] = 0;
        unitType.FiringRangeFromAttacker["ComCen"] = 0;
        unitType.FiringRangeFromAttacker["comcen"] = 0;
        unitType.FiringRangeFromAttacker["CommandCenter"] = 0;

        // row
        unitType.FiringRangeToDefender["infantry"] = 0;
        unitType.FiringRangeToDefender["armor"] = 0;
        unitType.FiringRangeToDefender["tank"] = 0;
        unitType.FiringRangeToDefender["dug-in-infantry"] = 0;
        unitType.FiringRangeToDefender["transport-infantry"] = 0;
        unitType.FiringRangeToDefender["transport-armor"] = 0;
        unitType.FiringRangeToDefender["transport-tank"] = 0;
        unitType.FiringRangeToDefender["submarine"] = 0;
        unitType.FiringRangeToDefender["sub"] = 0;
        unitType.FiringRangeToDefender["battleship"] = 0;
        unitType.FiringRangeToDefender["carrier"] = 0;
        unitType.FiringRangeToDefender["spy"] = 0;
        unitType.FiringRangeToDefender["com"] = 0;
        unitType.FiringRangeToDefender["ComCen"] = 0;
        unitType.FiringRangeToDefender["comcen"] = 0;
        unitType.FiringRangeToDefender["CommandCenter"] = 0;

        // only applies to infantry
        unitType.CanDigInByTerrainYorN["ocean"] = "N";
        unitType.CanDigInByTerrainYorN["sea"] = "N";
        unitType.CanDigInByTerrainYorN["dock"] = "N";
        unitType.CanDigInByTerrainYorN["burb"] = "N";
        unitType.CanDigInByTerrainYorN["village"] = "N";
        unitType.CanDigInByTerrainYorN["town"] = "N";
        unitType.CanDigInByTerrainYorN["city"] = "N";
        unitType.CanDigInByTerrainYorN["capital"] = "N";
        unitType.CanDigInByTerrainYorN["metro"] = "N";
        unitType.CanDigInByTerrainYorN["resource"] = "N";
        unitType.CanDigInByTerrainYorN["plain"] = "N";
        unitType.CanDigInByTerrainYorN["grass"] = "N";
        unitType.CanDigInByTerrainYorN["forest"] = "N";
        unitType.CanDigInByTerrainYorN["mountain"] = "N";
        unitType.CanDigInByTerrainYorN["swamp"] = "N";
        unitType.CanDigInByTerrainYorN["marsh"] = "N";

        // not used by planes
        unitType.RepairRateByFacility["resource"] = 0;
        unitType.RepairRateByFacility["village"] = 0;
        unitType.RepairRateByFacility["town"] = 0;
        unitType.RepairRateByFacility["city"] = 0;
        unitType.RepairRateByFacility["metro"] = 0;
        unitType.RepairRateByFacility["capital"] = 0;

        // not used by planes
        unitType.StepsUsedByTerrain["ocean"] = 0;
        unitType.StepsUsedByTerrain["sea"] = 0;
        unitType.StepsUsedByTerrain["dock"] = 0;
        unitType.StepsUsedByTerrain["burb"] = 0;
        unitType.StepsUsedByTerrain["village"] = 0;
        unitType.StepsUsedByTerrain["town"] = 0;
        unitType.StepsUsedByTerrain["city"] = 0;
        unitType.StepsUsedByTerrain["capital"] = 0;
        unitType.StepsUsedByTerrain["metro"] = 0;
        unitType.StepsUsedByTerrain["resource"] = 0;
        unitType.StepsUsedByTerrain["plain"] = 0;
        unitType.StepsUsedByTerrain["grass"] = 0;
        unitType.StepsUsedByTerrain["forest"] = 0;
        unitType.StepsUsedByTerrain["mountain"] = 0;
        unitType.StepsUsedByTerrain["swamp"] = 0;
        unitType.StepsUsedByTerrain["marsh"] = 0;

        return unitType;
    }
}
