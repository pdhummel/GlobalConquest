namespace GlobalConquest.Units;
using static UnitConstants;
using static GameConstants;
using static GlobalConquest.Burbs;

public class BattleshipUnitType : UnitType
{

    public BattleshipUnitType()
    {

    }

    public UnitType defineBattleship()
    {
        UnitType unitType = new UnitType();
        unitType.Name = BATTLESHIP;
        unitType.LandOrSea = TERRAIN_SEA;

        // can't become dug-in infantry in swamp
        unitType.AttritionByTerrain[TERRAIN_SWAMP] = 0;

        // column
        unitType.BattleDamageFromAttacker[INFANTRY] = 25;
        unitType.BattleDamageFromAttacker[ARMOR] = 25;
        unitType.BattleDamageFromAttacker[ARMOR] = 25;
        unitType.BattleDamageFromAttacker[DUG_IN_INFANTRY] = 25;
        unitType.BattleDamageFromAttacker[TRANSPORT_INFANTRY] = 5;
        unitType.BattleDamageFromAttacker[TRANSPORT_ARMOR] = 5;
        unitType.BattleDamageFromAttacker[TRANSPORT_ARMOR] = 5;
        unitType.BattleDamageFromAttacker["submarine"] = 25;
        unitType.BattleDamageFromAttacker[SUBMARINE] = 34;
        unitType.BattleDamageFromAttacker[BATTLESHIP] = 34;
        unitType.BattleDamageFromAttacker[AIRCRAFT_CARRIER] = 25;
        unitType.BattleDamageFromAttacker[SPY] = 0;
        unitType.BattleDamageFromAttacker[DECOY_COMMAND_CENTER] = 0;
        unitType.BattleDamageFromAttacker["com"] = 20;
        unitType.BattleDamageFromAttacker[COMMAND_CENTER] = 20;
        unitType.BattleDamageFromAttacker[COMMAND_CENTER] = 20;
        unitType.BattleDamageFromAttacker["CommandCenter"] = 20;

        // row
        unitType.BattleDamageToDefender[INFANTRY] = 12;
        unitType.BattleDamageToDefender[ARMOR] = 10;
        unitType.BattleDamageToDefender[ARMOR] = 10;
        unitType.BattleDamageToDefender[DUG_IN_INFANTRY] = 6;
        unitType.BattleDamageToDefender[TRANSPORT_INFANTRY] = 50;
        unitType.BattleDamageToDefender[TRANSPORT_ARMOR] = 50;
        unitType.BattleDamageToDefender[TRANSPORT_ARMOR] = 50;
        unitType.BattleDamageToDefender["submarine"] = 34;
        unitType.BattleDamageToDefender[SUBMARINE] = 34;
        unitType.BattleDamageToDefender[BATTLESHIP] = 25;
        unitType.BattleDamageToDefender[AIRCRAFT_CARRIER] = 34;
        unitType.BattleDamageToDefender[SPY] = 34;
        unitType.BattleDamageToDefender[DECOY_COMMAND_CENTER] = 34;
        unitType.BattleDamageToDefender["com"] = 10;
        unitType.BattleDamageToDefender[COMMAND_CENTER] = 10;
        unitType.BattleDamageToDefender[COMMAND_CENTER] = 10;
        unitType.BattleDamageToDefender["CommandCenter"] = 10;

        unitType.NormalStepsAddedPerRound = 20;
        unitType.BlitzStepsAddedPerRound = 28;
        unitType.SneakStepsAddedPerRound = 10;

        unitType.Cost = 35;

        unitType.DamageReductionForDefenderByTerrain["burb"] = 0;
        unitType.DamageReductionForDefenderByTerrain[TERRAIN_MOUNTAIN] = 0;

        unitType.DiscoveryRange = 4;
        unitType.ScanningRange = 5;
        unitType.PointsPerHit = 10;

        // column
        unitType.FiringRangeFromAttacker[INFANTRY] = 1;
        unitType.FiringRangeFromAttacker[ARMOR] = 1;
        unitType.FiringRangeFromAttacker[ARMOR] = 1;
        unitType.FiringRangeFromAttacker[DUG_IN_INFANTRY] = 1;
        unitType.FiringRangeFromAttacker[TRANSPORT_INFANTRY] = 1;
        unitType.FiringRangeFromAttacker[TRANSPORT_ARMOR] = 1;
        unitType.FiringRangeFromAttacker[TRANSPORT_ARMOR] = 1;
        unitType.FiringRangeFromAttacker["submarine"] = 2;
        unitType.FiringRangeFromAttacker[SUBMARINE] = 2;
        unitType.FiringRangeFromAttacker[BATTLESHIP] = 3;
        unitType.FiringRangeFromAttacker[AIRCRAFT_CARRIER] = 4;
        unitType.FiringRangeFromAttacker[SPY] = 0;
        unitType.FiringRangeFromAttacker[DECOY_COMMAND_CENTER] = 0;
        unitType.FiringRangeFromAttacker["com"] = 3;
        unitType.FiringRangeFromAttacker[COMMAND_CENTER] = 3;
        unitType.FiringRangeFromAttacker[COMMAND_CENTER] = 3;
        unitType.FiringRangeFromAttacker["CommandCenter"] = 3;

        // row
        unitType.FiringRangeToDefender[INFANTRY] = 3;
        unitType.FiringRangeToDefender[ARMOR] = 3;
        unitType.FiringRangeToDefender[ARMOR] = 3;
        unitType.FiringRangeToDefender[DUG_IN_INFANTRY] = 3;
        unitType.FiringRangeToDefender[TRANSPORT_INFANTRY] = 3;
        unitType.FiringRangeToDefender[TRANSPORT_ARMOR] = 3;
        unitType.FiringRangeToDefender[TRANSPORT_ARMOR] = 3;
        unitType.FiringRangeToDefender["submarine"] = 3;
        unitType.FiringRangeToDefender[SUBMARINE] = 3;
        unitType.FiringRangeToDefender[BATTLESHIP] = 3;
        unitType.FiringRangeToDefender[AIRCRAFT_CARRIER] = 3;
        unitType.FiringRangeToDefender[SPY] = 3;
        unitType.FiringRangeToDefender[DECOY_COMMAND_CENTER] = 3;
        unitType.FiringRangeToDefender["com"] = 3;
        unitType.FiringRangeToDefender[COMMAND_CENTER] = 3;
        unitType.FiringRangeToDefender[COMMAND_CENTER] = 3;
        unitType.FiringRangeToDefender["CommandCenter"] = 3;

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

        // same for all unit types
        unitType.RepairRateByFacility["resource"] = 2;
        unitType.RepairRateByFacility[BURB_VILLAGE] = 4;
        unitType.RepairRateByFacility[BURB_TOWN] = 6;
        unitType.RepairRateByFacility[BURB_CITY] = 8;
        unitType.RepairRateByFacility[BURB_METROPLEX] = 10;
        unitType.RepairRateByFacility[BURB_CAPITAL] = 10;

        // same for all unit types
        unitType.StepsUsedByTerrain["ocean"] = 10;
        unitType.StepsUsedByTerrain[TERRAIN_SEA] = 10;
        unitType.StepsUsedByTerrain[BURB_DOCK] = 10;
        unitType.StepsUsedByTerrain["burb"] = 10;
        unitType.StepsUsedByTerrain[BURB_VILLAGE] = 10;
        unitType.StepsUsedByTerrain[BURB_TOWN] = 10;
        unitType.StepsUsedByTerrain[BURB_CITY] = 10;
        unitType.StepsUsedByTerrain[BURB_CAPITAL] = 10;
        unitType.StepsUsedByTerrain[BURB_METROPLEX] = 10;
        unitType.StepsUsedByTerrain["resource"] = 10;
        unitType.StepsUsedByTerrain["plain"] = 10;
        unitType.StepsUsedByTerrain[TERRAIN_GRASS] = 10;
        unitType.StepsUsedByTerrain[TERRAIN_FOREST] = 15;
        unitType.StepsUsedByTerrain[TERRAIN_MOUNTAIN] = 20;
        unitType.StepsUsedByTerrain[TERRAIN_SWAMP] = 30;
        unitType.StepsUsedByTerrain["marsh"] = 30;

        return unitType;
    }

}