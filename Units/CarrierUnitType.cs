using static UnitTypeConstants;
namespace GlobalConquest.Units;

public class CarrierUnitType : UnitType
{

    public CarrierUnitType()
    {

    }

    public UnitType defineCarrier()
    {
        UnitType unitType = new UnitType();
        unitType.Name = AIRCRAFT_CARRIER;
        unitType.LandOrSea = "sea";

        // can't become dug-in infantry in swamp
        unitType.AttritionByTerrain["swamp"] = 0;

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
        unitType.BattleDamageToDefender[INFANTRY] = 10;
        unitType.BattleDamageToDefender[ARMOR] = 8;
        unitType.BattleDamageToDefender[ARMOR] = 8;
        unitType.BattleDamageToDefender[DUG_IN_INFANTRY] = 5;
        unitType.BattleDamageToDefender[TRANSPORT_INFANTRY] = 50;
        unitType.BattleDamageToDefender[TRANSPORT_ARMOR] = 50;
        unitType.BattleDamageToDefender[TRANSPORT_ARMOR] = 50;
        unitType.BattleDamageToDefender["submarine"] = 25;
        unitType.BattleDamageToDefender[SUBMARINE] = 25;
        unitType.BattleDamageToDefender[BATTLESHIP] = 20;
        unitType.BattleDamageToDefender[AIRCRAFT_CARRIER] = 25;
        unitType.BattleDamageToDefender[SPY] = 34;
        unitType.BattleDamageToDefender[DECOY_COMMAND_CENTER] = 34;
        unitType.BattleDamageToDefender["com"] = 10;
        unitType.BattleDamageToDefender[COMMAND_CENTER] = 10;
        unitType.BattleDamageToDefender[COMMAND_CENTER] = 10;
        unitType.BattleDamageToDefender["CommandCenter"] = 10;

        unitType.NormalStepsAddedPerRound = 20;
        unitType.BlitzStepsAddedPerRound = 28;
        unitType.SneakStepsAddedPerRound = 10;

        unitType.Cost = 45;

        unitType.DamageReductionForDefenderByTerrain["burb"] = 0;
        unitType.DamageReductionForDefenderByTerrain["mountain"] = 0;

        unitType.DiscoveryRange = 5;
        unitType.ScanningRange = 6;
        unitType.PointsPerHit = 12;

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
        unitType.FiringRangeFromAttacker["com"] = 4;
        unitType.FiringRangeFromAttacker[COMMAND_CENTER] = 4;
        unitType.FiringRangeFromAttacker[COMMAND_CENTER] = 4;
        unitType.FiringRangeFromAttacker["CommandCenter"] = 4;

        // row
        unitType.FiringRangeToDefender[INFANTRY] = 4;
        unitType.FiringRangeToDefender[ARMOR] = 4;
        unitType.FiringRangeToDefender[ARMOR] = 4;
        unitType.FiringRangeToDefender[DUG_IN_INFANTRY] = 4;
        unitType.FiringRangeToDefender[TRANSPORT_INFANTRY] = 4;
        unitType.FiringRangeToDefender[TRANSPORT_ARMOR] = 4;
        unitType.FiringRangeToDefender[TRANSPORT_ARMOR] = 4;
        unitType.FiringRangeToDefender["submarine"] = 4;
        unitType.FiringRangeToDefender[SUBMARINE] = 4;
        unitType.FiringRangeToDefender[BATTLESHIP] = 4;
        unitType.FiringRangeToDefender[AIRCRAFT_CARRIER] = 4;
        unitType.FiringRangeToDefender[SPY] = 4;
        unitType.FiringRangeToDefender[DECOY_COMMAND_CENTER] = 4;
        unitType.FiringRangeToDefender["com"] = 4;
        unitType.FiringRangeToDefender[COMMAND_CENTER] = 4;
        unitType.FiringRangeToDefender[COMMAND_CENTER] = 4;
        unitType.FiringRangeToDefender["CommandCenter"] = 4;

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

        // same for all unit types
        unitType.RepairRateByFacility["resource"] = 2;
        unitType.RepairRateByFacility["village"] = 4;
        unitType.RepairRateByFacility["town"] = 6;
        unitType.RepairRateByFacility["city"] = 8;
        unitType.RepairRateByFacility["metro"] = 10;
        unitType.RepairRateByFacility["capital"] = 10;

        // same for all unit types
        unitType.StepsUsedByTerrain["ocean"] = 10;
        unitType.StepsUsedByTerrain["sea"] = 10;
        unitType.StepsUsedByTerrain["dock"] = 10;
        unitType.StepsUsedByTerrain["burb"] = 10;
        unitType.StepsUsedByTerrain["village"] = 10;
        unitType.StepsUsedByTerrain["town"] = 10;
        unitType.StepsUsedByTerrain["city"] = 10;
        unitType.StepsUsedByTerrain["capital"] = 10;
        unitType.StepsUsedByTerrain["metro"] = 10;
        unitType.StepsUsedByTerrain["resource"] = 10;
        unitType.StepsUsedByTerrain["plain"] = 10;
        unitType.StepsUsedByTerrain["grass"] = 10;
        unitType.StepsUsedByTerrain["forest"] = 15;
        unitType.StepsUsedByTerrain["mountain"] = 20;
        unitType.StepsUsedByTerrain["swamp"] = 30;
        unitType.StepsUsedByTerrain["marsh"] = 30;

        return unitType;
    }


}