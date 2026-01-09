using static UnitTypeConstants;
using static GameConstants;
namespace GlobalConquest.Units;

public class ComCenUnitType : UnitType
{

    public ComCenUnitType()
    {

    }

    public UnitType defineComCen()
    {
        UnitType unitType = new UnitType();
        unitType.Name = COMMAND_CENTER;
        unitType.LandOrSea = "both";

        // can't become dug-in infantry in swamp
        unitType.AttritionByTerrain[TERRAIN_SWAMP] = 0;

        // column
        unitType.BattleDamageFromAttacker[INFANTRY] = 10;
        unitType.BattleDamageFromAttacker[ARMOR] = 10;
        unitType.BattleDamageFromAttacker[ARMOR] = 10;
        unitType.BattleDamageFromAttacker[DUG_IN_INFANTRY] = 10;
        unitType.BattleDamageFromAttacker[TRANSPORT_INFANTRY] = 10;
        unitType.BattleDamageFromAttacker[TRANSPORT_ARMOR] = 10;
        unitType.BattleDamageFromAttacker[TRANSPORT_ARMOR] = 10;
        unitType.BattleDamageFromAttacker["submarine"] = 10;
        unitType.BattleDamageFromAttacker[SUBMARINE] = 10;
        unitType.BattleDamageFromAttacker[BATTLESHIP] = 10;
        unitType.BattleDamageFromAttacker[AIRCRAFT_CARRIER] = 10;
        unitType.BattleDamageFromAttacker[SPY] = 0;
        unitType.BattleDamageFromAttacker[DECOY_COMMAND_CENTER] = 0;
        unitType.BattleDamageFromAttacker["com"] = 10;
        unitType.BattleDamageFromAttacker[COMMAND_CENTER] = 10;
        unitType.BattleDamageFromAttacker[COMMAND_CENTER] = 10;
        unitType.BattleDamageFromAttacker["CommandCenter"] = 10;

        // row
        unitType.BattleDamageToDefender[INFANTRY] = 20;
        unitType.BattleDamageToDefender[ARMOR] = 20;
        unitType.BattleDamageToDefender[ARMOR] = 20;
        unitType.BattleDamageToDefender[DUG_IN_INFANTRY] = 20;
        unitType.BattleDamageToDefender[TRANSPORT_INFANTRY] = 20;
        unitType.BattleDamageToDefender[TRANSPORT_ARMOR] = 20;
        unitType.BattleDamageToDefender[TRANSPORT_ARMOR] = 20;
        unitType.BattleDamageToDefender["submarine"] = 20;
        unitType.BattleDamageToDefender[SUBMARINE] = 20;
        unitType.BattleDamageToDefender[BATTLESHIP] = 20;
        unitType.BattleDamageToDefender[AIRCRAFT_CARRIER] = 20;
        unitType.BattleDamageToDefender[SPY] = 34;
        unitType.BattleDamageToDefender[DECOY_COMMAND_CENTER] = 34;
        unitType.BattleDamageToDefender["com"] = 10;
        unitType.BattleDamageToDefender[COMMAND_CENTER] = 10;
        unitType.BattleDamageToDefender[COMMAND_CENTER] = 10;
        unitType.BattleDamageToDefender["CommandCenter"] = 10;

        unitType.NormalStepsAddedPerRound = 20;
        unitType.BlitzStepsAddedPerRound = 28;
        unitType.SneakStepsAddedPerRound = 10;

        unitType.Cost = 0;  // cannot be purchased

        unitType.DamageReductionForDefenderByTerrain["burb"] = 0;
        unitType.DamageReductionForDefenderByTerrain[TERRAIN_MOUNTAIN] = 0;

        unitType.DiscoveryRange = 8;
        unitType.ScanningRange = 6;
        unitType.PointsPerHit = 16;

        // column
        unitType.FiringRangeFromAttacker[INFANTRY] = 2;
        unitType.FiringRangeFromAttacker[ARMOR] = 2;
        unitType.FiringRangeFromAttacker[ARMOR] = 2;
        unitType.FiringRangeFromAttacker[DUG_IN_INFANTRY] = 2;
        unitType.FiringRangeFromAttacker[TRANSPORT_INFANTRY] = 1;
        unitType.FiringRangeFromAttacker[TRANSPORT_ARMOR] = 1;
        unitType.FiringRangeFromAttacker[TRANSPORT_ARMOR] = 1;
        unitType.FiringRangeFromAttacker["submarine"] = 2;
        unitType.FiringRangeFromAttacker[SUBMARINE] = 2;
        unitType.FiringRangeFromAttacker[BATTLESHIP] = 3;
        unitType.FiringRangeFromAttacker[AIRCRAFT_CARRIER] = 4;
        unitType.FiringRangeFromAttacker[SPY] = 0;
        unitType.FiringRangeFromAttacker[DECOY_COMMAND_CENTER] = 0;
        unitType.FiringRangeFromAttacker["com"] = 2;
        unitType.FiringRangeFromAttacker[COMMAND_CENTER] = 2;
        unitType.FiringRangeFromAttacker[COMMAND_CENTER] = 2;
        unitType.FiringRangeFromAttacker["CommandCenter"] = 2;

        // row
        unitType.FiringRangeToDefender[INFANTRY] = 2;
        unitType.FiringRangeToDefender[ARMOR] = 2;
        unitType.FiringRangeToDefender[ARMOR] = 2;
        unitType.FiringRangeToDefender[DUG_IN_INFANTRY] = 2;
        unitType.FiringRangeToDefender[TRANSPORT_INFANTRY] = 2;
        unitType.FiringRangeToDefender[TRANSPORT_ARMOR] = 2;
        unitType.FiringRangeToDefender[TRANSPORT_ARMOR] = 2;
        unitType.FiringRangeToDefender["submarine"] = 2;
        unitType.FiringRangeToDefender[SUBMARINE] = 2;
        unitType.FiringRangeToDefender[BATTLESHIP] = 3;
        unitType.FiringRangeToDefender[AIRCRAFT_CARRIER] = 4;
        unitType.FiringRangeToDefender[SPY] = 2;
        unitType.FiringRangeToDefender[DECOY_COMMAND_CENTER] = 2;
        unitType.FiringRangeToDefender["com"] = 2;
        unitType.FiringRangeToDefender[COMMAND_CENTER] = 2;
        unitType.FiringRangeToDefender[COMMAND_CENTER] = 2;
        unitType.FiringRangeToDefender["CommandCenter"] = 2;

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

    public UnitType defineDecoyComCen()
    {
        UnitType unitType = new UnitType();
        unitType.Name = DECOY_COMMAND_CENTER;
        unitType.LandOrSea = "both";

        // can't become dug-in infantry in swamp
        unitType.AttritionByTerrain[TERRAIN_SWAMP] = 0;

        // column
        unitType.BattleDamageFromAttacker[INFANTRY] = 34;
        unitType.BattleDamageFromAttacker[ARMOR] = 34;
        unitType.BattleDamageFromAttacker[ARMOR] = 34;
        unitType.BattleDamageFromAttacker[DUG_IN_INFANTRY] = 34;
        unitType.BattleDamageFromAttacker[TRANSPORT_INFANTRY] = 34;
        unitType.BattleDamageFromAttacker[TRANSPORT_ARMOR] = 34;
        unitType.BattleDamageFromAttacker[TRANSPORT_ARMOR] = 34;
        unitType.BattleDamageFromAttacker["submarine"] = 34;
        unitType.BattleDamageFromAttacker[SUBMARINE] = 34;
        unitType.BattleDamageFromAttacker[BATTLESHIP] = 34;
        unitType.BattleDamageFromAttacker[AIRCRAFT_CARRIER] = 34;
        unitType.BattleDamageFromAttacker[SPY] = 0;
        unitType.BattleDamageFromAttacker[DECOY_COMMAND_CENTER] = 0;
        unitType.BattleDamageFromAttacker["com"] = 34;
        unitType.BattleDamageFromAttacker[COMMAND_CENTER] = 34;
        unitType.BattleDamageFromAttacker[COMMAND_CENTER] = 34;
        unitType.BattleDamageFromAttacker["CommandCenter"] = 34;

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

        unitType.NormalStepsAddedPerRound = 20;
        unitType.BlitzStepsAddedPerRound = 28;
        unitType.SneakStepsAddedPerRound = 10;

        unitType.Cost = 15;

        unitType.DamageReductionForDefenderByTerrain["burb"] = 0;
        unitType.DamageReductionForDefenderByTerrain[TERRAIN_MOUNTAIN] = 0;

        unitType.DiscoveryRange = 0;
        unitType.ScanningRange = 10;
        unitType.PointsPerHit = 12;

        // column
        unitType.FiringRangeFromAttacker[INFANTRY] = 2;
        unitType.FiringRangeFromAttacker[ARMOR] = 2;
        unitType.FiringRangeFromAttacker[ARMOR] = 2;
        unitType.FiringRangeFromAttacker[DUG_IN_INFANTRY] = 2;
        unitType.FiringRangeFromAttacker[TRANSPORT_INFANTRY] = 2;
        unitType.FiringRangeFromAttacker[TRANSPORT_ARMOR] = 2;
        unitType.FiringRangeFromAttacker[TRANSPORT_ARMOR] = 2;
        unitType.FiringRangeFromAttacker["submarine"] = 2;
        unitType.FiringRangeFromAttacker[SUBMARINE] = 2;
        unitType.FiringRangeFromAttacker[BATTLESHIP] = 3;
        unitType.FiringRangeFromAttacker[AIRCRAFT_CARRIER] = 4;
        unitType.FiringRangeFromAttacker[SPY] = 0;
        unitType.FiringRangeFromAttacker[DECOY_COMMAND_CENTER] = 0;
        unitType.FiringRangeFromAttacker["com"] = 2;
        unitType.FiringRangeFromAttacker[COMMAND_CENTER] = 2;
        unitType.FiringRangeFromAttacker[COMMAND_CENTER] = 2;
        unitType.FiringRangeFromAttacker["CommandCenter"] = 2;

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