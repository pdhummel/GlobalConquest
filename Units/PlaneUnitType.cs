using static UnitTypeConstants;
using static GameConstants;
using static GlobalConquest.Burbs;
namespace GlobalConquest.Units;

public class PlaneUnitType : UnitType
{

    public int shortRangeHexes = 4;
    public int mediumRangeHexes = 8;
    public int longRangeHexes = 20;



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
        MapHex planeHex = getPlaneMapHex(map, plane);
        AirplaneMissionOutcome outcome = new AirplaneMissionOutcome();
        if (isShortRangeMission(gameState, planeHex, targetMapHex))
        {
            Globals.Log("determineMissionOutcome(): short range mission");
            outcome.IsShortRangeMission = true;
        }
        else if (isMediumRangeMission(gameState, planeHex, targetMapHex))
        {
            Globals.Log("determineMissionOutcome(): medium range mission");
            outcome.IsMediumRangeMission = true;
        }
        else if (isLongRangeMission(gameState, planeHex, targetMapHex))
        {
            Globals.Log("determineMissionOutcome(): long range mission");
            outcome.IsLongRangeMission = true;
        }
        else
        {
            return outcome;
        }
        bool isDogfight = false;
        bool isEnemyGrounded = true;
        Unit enemyPlane = getEnemyPlaneForDogfight(gameState, targetMapHex, plane.Color);
        if (enemyPlane != null)
        {
            isDogfight = true;
            if (enemyPlane.TurnsUnavailable > 0)
                isEnemyGrounded = true;
            else
                isEnemyGrounded = false;
        }

        if (!isDogfight)
        {
            enemyPlane = getNearbyEnemyPlane(gameState, targetMapHex, plane.Color);
        }

        if (enemyPlane != null)
        {
            // If your opponents attempt an air strike against your forces and the strike 
            // is within 10 spaces of your planes, 
            // your planes will automatically defend against the attack. 
            // If your plane survives this defense, it will need even more rest than usual. 
            // Planes need an additional 1/2 turn of rest (i.e., are unavailable) per attack they defend against.
            enemyPlane.TurnsUnavailable += 0.5f;
        }
        int multiplier = 1;
        if (outcome.IsShortRangeMission)
        {
            multiplier = 1;
            plane.TurnsUnavailable += 1;
        }
        else if (outcome.IsMediumRangeMission)
        {
            multiplier = 2;
            plane.TurnsUnavailable += 2;
        }
        else if (outcome.IsLongRangeMission)
        {
            // Only applies for Transfer missions.
            plane.TurnsUnavailable += 2;
        }
        resolveMission(outcome, gameState, plane, isDogfight, isEnemyGrounded, enemyPlane, multiplier);
        outcome.Plane = plane;
        return outcome;
    }

    AirplaneMissionOutcome resolveMission(AirplaneMissionOutcome outcome, 
      GameState gameState, Unit plane, 
      bool isDogfight, bool isEnemyGrounded, Unit? enemyPlane, int multiplier)
    {
        int chance = rand.Next(0, 100);

        if (isDogfight && isEnemyGrounded)
        {
            outcome.EnemyPlane = enemyPlane;
            int shotDownProbability = 10 * multiplier;
            int enemyShotDownProbability = 25;
            int missionFailedProbability = 50;
            if (chance < shotDownProbability)
            {
                outcome.IsPlaneShotDown = true;
                outcome.IsMissionSuccessful = false;
                handlePlaneShotDown(gameState, plane);
            }
            else if (chance < shotDownProbability + enemyShotDownProbability)
            {
                outcome.IsEnemyPlaneShotDown = true;
                outcome.IsMissionSuccessful = false;
                handlePlaneShotDown(gameState, enemyPlane);
            }
            else if (chance < shotDownProbability + enemyShotDownProbability + missionFailedProbability)
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
            int shotDownProbability = 25 * multiplier;
            int enemyShotDownProbability = 25;
            if (chance < shotDownProbability)
            {
                outcome.IsPlaneShotDown = true;
                outcome.IsMissionSuccessful = false;
                handlePlaneShotDown(gameState, plane);
            }
            else if (chance < shotDownProbability + enemyShotDownProbability)
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
            // If your target is between two and 10 spaces from an enemy plane, 
            // the probability of your air mission failing ranges anywhere from 10% to 50%; 
            // one third of those failures will end up resulting in a lost plane.
            int missionFailedProbability = 10 * multiplier;
            outcome.EnemyPlane = enemyPlane;
            int randomness = rand.Next(missionFailedProbability, 50+1);
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
            int missionFailed = 10 * multiplier;
            if (chance < missionFailed)
            {
                outcome.IsMissionSuccessful = false;
            }
            else
                outcome.IsMissionSuccessful = true;
        }
        return outcome;
    }

    public void handlePlaneShotDown(GameState gameState, Unit plane)
    {
        Map map = gameState.Map;
        MapHex planeHex = map.Hexes[plane.Y, plane.X];
        plane.StrengthPoints = 0;
        Unit parentUnit = getParentUnit(map, plane);
        plane.ParentUnitId = null;
        if (parentUnit != null)
        {
            parentUnit.Airplane = null;
        }
        else if (planeHex.Airplane != null)
        {
            planeHex.Airplane = null;
        }
    }

    public Unit getEnemyPlaneForDogfight(GameState gameState, MapHex targetMapHex, string color)
    {
        Map map = gameState.Map;
        HashSet<MapHex> dogFightHexes = map.getMapHexesInRange(targetMapHex, 1);
        Unit enemyPlane = null;
        foreach (MapHex mapHex in dogFightHexes)
        {
            Unit hexUnit = mapHex.getUnit();
            if (mapHex.Airplane != null && enemyPlane == null && mapHex.Airplane != null && !color.Equals(mapHex.Airplane.Color) && 
                mapHex.Airplane.TurnsUnavailable <= 0)
            {
                enemyPlane = mapHex.Airplane;
                break;
            }
            else if (hexUnit != null && hexUnit.Airplane != null && enemyPlane == null && 
                    mapHex.Airplane != null &&
                    !color.Equals(mapHex.Airplane.Color) && hexUnit.Airplane.TurnsUnavailable <= 0)
            {
                enemyPlane = hexUnit.Airplane;
                break;
            }
            else if (mapHex.Airplane != null && enemyPlane == null && !color.Equals(mapHex.Airplane.Color))
            {
                enemyPlane = mapHex.Airplane;
            }
            else if (hexUnit != null && hexUnit.Airplane != null && enemyPlane == null && mapHex.Airplane != null && 
                    !color.Equals(mapHex.Airplane.Color))
            {
                enemyPlane = hexUnit.Airplane;
            }
        }
        if (enemyPlane != null)
            {
                MapHex enemyPlaneHex = getPlaneMapHex(map, enemyPlane);
                enemyPlane.X = enemyPlaneHex.X;
                enemyPlane.Y = enemyPlaneHex.Y;
                Globals.Log("getEnemyPlaneForDogfight(): enemyPlane=" + enemyPlaneHex.X + "," + enemyPlaneHex.Y);
            }
        return enemyPlane;
    }

    Unit getNearbyEnemyPlane(GameState gameState, MapHex targetMapHex, string color)
    {
        Unit enemyPlane = null;
        Map map = gameState.Map;
        HashSet<MapHex> enemyPlaneHexes = map.getMapHexesInRange(targetMapHex, 10);
        foreach (MapHex mapHex in enemyPlaneHexes)
        {
            Unit hexUnit = mapHex.getUnit();
            if (mapHex.Airplane != null && !color.Equals(mapHex.Airplane.Color) && mapHex.Airplane.IsDefending)
            {
                enemyPlane = mapHex.Airplane;
                break;
            }
            else if (hexUnit != null && hexUnit.Airplane != null && !color.Equals(hexUnit.Airplane.Color)  && hexUnit.Airplane.IsDefending)
            {
                enemyPlane = hexUnit.Airplane;
                break;
            }
        }
        if (enemyPlane != null)
        {
            MapHex enemyPlaneHex = getPlaneMapHex(map, enemyPlane);
            enemyPlane.X = enemyPlaneHex.X;
            enemyPlane.Y = enemyPlaneHex.Y;
            Globals.Log("getNearbyEnemyPlane(): enemyPlane=" + enemyPlaneHex.X + "," + enemyPlaneHex.Y);
        }
        return enemyPlane;
    }

    public bool isShortRangeMission(GameState gameState, MapHex sourceMapHex, MapHex targetMapHex)
    {
        Map map = gameState.Map;
        bool isShortRange = false;
        PlaneUnitType planeType = new PlaneUnitType();

        HashSet<MapHex> shortRangeHexes = map.getMapHexesInRange(sourceMapHex, planeType.shortRangeHexes);
        if (shortRangeHexes.Contains(targetMapHex))
            isShortRange = true;
        return isShortRange;
    }

    public bool isMediumRangeMission(GameState gameState, MapHex sourceMapHex, MapHex targetMapHex)
    {
        Map map = gameState.Map;
        bool isMediumRange = false;
        PlaneUnitType planeType = new PlaneUnitType();

        HashSet<MapHex> mediumRangeHexes = map.getMapHexesInRange(sourceMapHex, planeType.mediumRangeHexes);
        if (mediumRangeHexes.Contains(targetMapHex))
            isMediumRange = true;

        return isMediumRange;
    }

    bool isLongRangeMission(GameState gameState, MapHex sourceMapHex, MapHex targetMapHex)
    {
        Map map = gameState.Map;
        bool isLongRange = false;

        PlaneUnitType planeType = new PlaneUnitType();
        //HashSet<MapHex> longRangeHexes = map.getMapHexesInRange(sourceMapHex, planeType.longRangeHexes);
        //if (longRangeHexes.Contains(targetMapHex))
        //    isLongRange = true;
        float distance = map.calculateDistance(sourceMapHex, targetMapHex);
        if (distance < planeType.longRangeHexes)
            isLongRange = true;

        return isLongRange;
    }

    public MapHex getPlaneMapHex(Map map, Unit plane)
    {
        MapHex planeHex = null;
        Unit parentUnit = getParentUnit(map, plane);
        if (parentUnit != null)
        {
            planeHex = map.Hexes[parentUnit.Y, parentUnit.X];
        }
        else
        {
            planeHex = map.Hexes[plane.Y, plane.X];
        }
        return planeHex;
    }

    public Unit getExistingPlane(Map map, Unit plane)
    {
        Unit existingPlane = null;
        Unit parentUnit = getParentUnit(map, plane);
        if (parentUnit != null)
        {
            existingPlane = parentUnit.Airplane;
            if (existingPlane != null)
            {
                existingPlane.X = parentUnit.X;
                existingPlane.Y = parentUnit.Y;
            }
        }
        else if (parentUnit == null)
        {
            MapHex planeHex = this.getPlaneMapHex(map, plane);
            if (planeHex != null && planeHex.Airplane != null)
            {
                existingPlane = planeHex.Airplane;
                existingPlane.X = planeHex.X;
                existingPlane.Y = planeHex.Y;
            }
        }
        return existingPlane;
    }

    public Unit getPlane(MapHex mapHex, Unit parentUnit)
    {
        Unit plane = null;
        if (parentUnit != null && parentUnit.Airplane != null)
        {
            plane = parentUnit.Airplane;
            plane.X = parentUnit.X;
            plane.Y = parentUnit.Y;
        }
        else if (mapHex != null && mapHex.Airplane != null)
        {
            plane = mapHex.Airplane;
            plane.X = mapHex.X;
            plane.Y = mapHex.Y;
        } else if (mapHex != null && mapHex.getUnit() != null && mapHex.getUnit().Airplane != null)
        {
            plane = mapHex.getUnit().Airplane;
            plane.X = mapHex.X;
            plane.Y = mapHex.Y;
        }
        return plane;
    }


    public Unit getParentUnit(Map map, Unit plane)
    {
        Unit parentUnit = null;
        if (plane.ParentUnitId != null)
        {
            if (map.UnitIdToUnit.ContainsKey(plane.ParentUnitId))
            {
                parentUnit = map.UnitIdToUnit[plane.ParentUnitId];
            }
            else
            {
                Globals.Log("getParentUnit(): parentUnitId not found " + plane.ParentUnitId);
            }
        }
        return parentUnit;
    }

    public UnitType definePlane()
    {
        UnitType unitType = new UnitType();
        unitType.Name = AIRPLANE;
        unitType.LandOrSea = "neither";



        // can't become dug-in infantry in swamp
        unitType.AttritionByTerrain[TERRAIN_SWAMP] = 0;

        // column
        unitType.BattleDamageFromAttacker[INFANTRY] = 0;
        unitType.BattleDamageFromAttacker[ARMOR] = 0;
        unitType.BattleDamageFromAttacker[ARMOR] = 0;
        unitType.BattleDamageFromAttacker[DUG_IN_INFANTRY] = 0;
        unitType.BattleDamageFromAttacker[TRANSPORT_INFANTRY] = 0;
        unitType.BattleDamageFromAttacker[TRANSPORT_ARMOR] = 0;
        unitType.BattleDamageFromAttacker[TRANSPORT_ARMOR] = 0;
        unitType.BattleDamageFromAttacker["submarine"] = 0;
        unitType.BattleDamageFromAttacker[SUBMARINE] = 0;
        unitType.BattleDamageFromAttacker[BATTLESHIP] = 0;
        unitType.BattleDamageFromAttacker[AIRCRAFT_CARRIER] = 0;
        unitType.BattleDamageFromAttacker[SPY] = 0;
        unitType.BattleDamageFromAttacker[DECOY_COMMAND_CENTER] = 0;
        unitType.BattleDamageFromAttacker["com"] = 0;
        unitType.BattleDamageFromAttacker[COMMAND_CENTER] = 0;
        unitType.BattleDamageFromAttacker[COMMAND_CENTER] = 0;
        unitType.BattleDamageFromAttacker["CommandCenter"] = 0;

        // row
        unitType.BattleDamageToDefender[INFANTRY] = 0;
        unitType.BattleDamageToDefender[ARMOR] = 0;
        unitType.BattleDamageToDefender[ARMOR] = 0;
        unitType.BattleDamageToDefender[DUG_IN_INFANTRY] = 0;
        unitType.BattleDamageToDefender[TRANSPORT_INFANTRY] = 0;
        unitType.BattleDamageToDefender[TRANSPORT_ARMOR] = 0;
        unitType.BattleDamageToDefender[TRANSPORT_ARMOR] = 0;
        unitType.BattleDamageToDefender["submarine"] = 0;
        unitType.BattleDamageToDefender[SUBMARINE] = 0;
        unitType.BattleDamageToDefender[BATTLESHIP] = 0;
        unitType.BattleDamageToDefender[AIRCRAFT_CARRIER] = 0;
        unitType.BattleDamageToDefender[SPY] = 0;
        unitType.BattleDamageToDefender[DECOY_COMMAND_CENTER] = 0;
        unitType.BattleDamageToDefender["com"] = 0;
        unitType.BattleDamageToDefender[COMMAND_CENTER] = 0;
        unitType.BattleDamageToDefender[COMMAND_CENTER] = 0;
        unitType.BattleDamageToDefender["CommandCenter"] = 0;

        unitType.NormalStepsAddedPerRound = 0;
        unitType.BlitzStepsAddedPerRound = 0;
        unitType.SneakStepsAddedPerRound = 0;

        unitType.Cost = 35;

        unitType.DamageReductionForDefenderByTerrain["burb"] = 0;
        unitType.DamageReductionForDefenderByTerrain[TERRAIN_MOUNTAIN] = 0;

        // Airplanes automatically scan for units and terrain within 12 spaces.
        // Recon missions uncover any terrain within a radius of 8 spaces 
        // from the chosen spot and any units within 12 spaces.
        unitType.DiscoveryRange = 12;
        unitType.ScanningRange = 12;
        unitType.PointsPerHit = 10;

        // column
        unitType.FiringRangeFromAttacker[INFANTRY] = 0;
        unitType.FiringRangeFromAttacker[ARMOR] = 0;
        unitType.FiringRangeFromAttacker[ARMOR] = 0;
        unitType.FiringRangeFromAttacker[DUG_IN_INFANTRY] = 0;
        unitType.FiringRangeFromAttacker[TRANSPORT_INFANTRY] = 0;
        unitType.FiringRangeFromAttacker[TRANSPORT_ARMOR] = 0;
        unitType.FiringRangeFromAttacker[TRANSPORT_ARMOR] = 0;
        unitType.FiringRangeFromAttacker["submarine"] = 0;
        unitType.FiringRangeFromAttacker[SUBMARINE] = 0;
        unitType.FiringRangeFromAttacker[BATTLESHIP] = 0;
        unitType.FiringRangeFromAttacker[AIRCRAFT_CARRIER] = 0;
        unitType.FiringRangeFromAttacker[SPY] = 0;
        unitType.FiringRangeFromAttacker[DECOY_COMMAND_CENTER] = 0;
        unitType.FiringRangeFromAttacker["com"] = 0;
        unitType.FiringRangeFromAttacker[COMMAND_CENTER] = 0;
        unitType.FiringRangeFromAttacker[COMMAND_CENTER] = 0;
        unitType.FiringRangeFromAttacker["CommandCenter"] = 0;

        // row
        unitType.FiringRangeToDefender[INFANTRY] = 0;
        unitType.FiringRangeToDefender[ARMOR] = 0;
        unitType.FiringRangeToDefender[ARMOR] = 0;
        unitType.FiringRangeToDefender[DUG_IN_INFANTRY] = 0;
        unitType.FiringRangeToDefender[TRANSPORT_INFANTRY] = 0;
        unitType.FiringRangeToDefender[TRANSPORT_ARMOR] = 0;
        unitType.FiringRangeToDefender[TRANSPORT_ARMOR] = 0;
        unitType.FiringRangeToDefender["submarine"] = 0;
        unitType.FiringRangeToDefender[SUBMARINE] = 0;
        unitType.FiringRangeToDefender[BATTLESHIP] = 0;
        unitType.FiringRangeToDefender[AIRCRAFT_CARRIER] = 0;
        unitType.FiringRangeToDefender[SPY] = 0;
        unitType.FiringRangeToDefender[DECOY_COMMAND_CENTER] = 0;
        unitType.FiringRangeToDefender["com"] = 0;
        unitType.FiringRangeToDefender[COMMAND_CENTER] = 0;
        unitType.FiringRangeToDefender[COMMAND_CENTER] = 0;
        unitType.FiringRangeToDefender["CommandCenter"] = 0;

        // only applies to infantry
        unitType.CanDigInByTerrainYorN["ocean"] = "N";
        unitType.CanDigInByTerrainYorN[TERRAIN_SEA] = "N";
        unitType.CanDigInByTerrainYorN[BURB_DOCK] = "N";
        unitType.CanDigInByTerrainYorN["burb"] = "N";
        unitType.CanDigInByTerrainYorN[BURB_VILLAGE] = "N";
        unitType.CanDigInByTerrainYorN[BURB_TOWN] = "N";
        unitType.CanDigInByTerrainYorN[BURB_CITY] = "N";
        unitType.CanDigInByTerrainYorN[BURB_CAPITAL] = "N";
        unitType.CanDigInByTerrainYorN[BURB_METROPLEX] = "N";
        unitType.CanDigInByTerrainYorN["resource"] = "N";
        unitType.CanDigInByTerrainYorN["plain"] = "N";
        unitType.CanDigInByTerrainYorN[TERRAIN_GRASS] = "N";
        unitType.CanDigInByTerrainYorN[TERRAIN_FOREST] = "N";
        unitType.CanDigInByTerrainYorN[TERRAIN_MOUNTAIN] = "N";
        unitType.CanDigInByTerrainYorN[TERRAIN_SWAMP] = "N";
        unitType.CanDigInByTerrainYorN["marsh"] = "N";

        // not used by planes
        unitType.RepairRateByFacility["resource"] = 0;
        unitType.RepairRateByFacility[BURB_VILLAGE] = 0;
        unitType.RepairRateByFacility[BURB_TOWN] = 0;
        unitType.RepairRateByFacility[BURB_CITY] = 0;
        unitType.RepairRateByFacility[BURB_METROPLEX] = 0;
        unitType.RepairRateByFacility[BURB_CAPITAL] = 0;

        // not used by planes
        unitType.StepsUsedByTerrain["ocean"] = 0;
        unitType.StepsUsedByTerrain[TERRAIN_SEA] = 0;
        unitType.StepsUsedByTerrain[BURB_DOCK] = 0;
        unitType.StepsUsedByTerrain["burb"] = 0;
        unitType.StepsUsedByTerrain[BURB_VILLAGE] = 0;
        unitType.StepsUsedByTerrain[BURB_TOWN] = 0;
        unitType.StepsUsedByTerrain[BURB_CITY] = 0;
        unitType.StepsUsedByTerrain[BURB_CAPITAL] = 0;
        unitType.StepsUsedByTerrain[BURB_METROPLEX] = 0;
        unitType.StepsUsedByTerrain["resource"] = 0;
        unitType.StepsUsedByTerrain["plain"] = 0;
        unitType.StepsUsedByTerrain[TERRAIN_GRASS] = 0;
        unitType.StepsUsedByTerrain[TERRAIN_FOREST] = 0;
        unitType.StepsUsedByTerrain[TERRAIN_MOUNTAIN] = 0;
        unitType.StepsUsedByTerrain[TERRAIN_SWAMP] = 0;
        unitType.StepsUsedByTerrain["marsh"] = 0;

        return unitType;
    }


}
