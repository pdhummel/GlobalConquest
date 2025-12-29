using static UnitTypeConstants;
namespace GlobalConquest.Units;

public class InfantryUnitType : UnitType
{

    public InfantryUnitType()
    {

    }

    public UnitType defineInfantry()
    {
        UnitType unitType = new UnitType();
        unitType.Name = INFANTRY;
        unitType.LandOrSea = "land";

        unitType.AttritionByTerrain["swamp"] = 3;

        // column
        unitType.BattleDamageFromAttacker[INFANTRY] = 20;
        unitType.BattleDamageFromAttacker[ARMOR] = 26;
        unitType.BattleDamageFromAttacker[ARMOR] = 26;
        unitType.BattleDamageFromAttacker[DUG_IN_INFANTRY] = 20;
        unitType.BattleDamageFromAttacker[TRANSPORT_INFANTRY] = 18;
        unitType.BattleDamageFromAttacker[TRANSPORT_ARMOR] = 9;
        unitType.BattleDamageFromAttacker[TRANSPORT_ARMOR] = 9;
        unitType.BattleDamageFromAttacker["submarine"] = 0;
        unitType.BattleDamageFromAttacker[SUBMARINE] = 0;
        unitType.BattleDamageFromAttacker[BATTLESHIP] = 12;
        unitType.BattleDamageFromAttacker[AIRCRAFT_CARRIER] = 10;
        unitType.BattleDamageFromAttacker[SPY] = 0;
        unitType.BattleDamageFromAttacker[DECOY_COMMAND_CENTER] = 0;
        unitType.BattleDamageFromAttacker["com"] = 20;
        unitType.BattleDamageFromAttacker[COMMAND_CENTER] = 20;
        unitType.BattleDamageFromAttacker[COMMAND_CENTER] = 20;
        unitType.BattleDamageFromAttacker["CommandCenter"] = 20;

        // row
        unitType.BattleDamageToDefender[INFANTRY] = 20;
        unitType.BattleDamageToDefender[ARMOR] = 17;
        unitType.BattleDamageToDefender[ARMOR] = 17;
        unitType.BattleDamageToDefender[DUG_IN_INFANTRY] = 15;
        unitType.BattleDamageToDefender[TRANSPORT_INFANTRY] = 25;
        unitType.BattleDamageToDefender[TRANSPORT_ARMOR] = 25;
        unitType.BattleDamageToDefender[TRANSPORT_ARMOR] = 25;
        unitType.BattleDamageToDefender["submarine"] = 25;
        unitType.BattleDamageToDefender[SUBMARINE] = 25;
        unitType.BattleDamageToDefender[BATTLESHIP] = 25;
        unitType.BattleDamageToDefender[AIRCRAFT_CARRIER] = 25;
        unitType.BattleDamageToDefender[SPY] = 34;
        unitType.BattleDamageToDefender[DECOY_COMMAND_CENTER] = 34;
        unitType.BattleDamageToDefender["com"] = 10;
        unitType.BattleDamageToDefender[COMMAND_CENTER] = 10;
        unitType.BattleDamageToDefender[COMMAND_CENTER] = 10;
        unitType.BattleDamageToDefender["CommandCenter"] = 10;

        unitType.NormalStepsAddedPerRound = 6;
        unitType.BlitzStepsAddedPerRound = 14;
        unitType.SneakStepsAddedPerRound = 3;

        unitType.Cost = 25;

        unitType.DamageReductionForDefenderByTerrain["burb"] = 1 / 3;
        unitType.DamageReductionForDefenderByTerrain["mountain"] = 1 / 4;

        unitType.DiscoveryRange = 3;
        unitType.ScanningRange = 5;
        unitType.PointsPerHit = 2;

        // column
        unitType.FiringRangeFromAttacker[INFANTRY] = 2;
        unitType.FiringRangeFromAttacker[ARMOR] = 2;
        unitType.FiringRangeFromAttacker[ARMOR] = 2;
        unitType.FiringRangeFromAttacker[DUG_IN_INFANTRY] = 2;
        unitType.FiringRangeFromAttacker[TRANSPORT_INFANTRY] = 2;
        unitType.FiringRangeFromAttacker[TRANSPORT_ARMOR] = 2;
        unitType.FiringRangeFromAttacker[TRANSPORT_ARMOR] = 2;
        unitType.FiringRangeFromAttacker["submarine"] = 0;
        unitType.FiringRangeFromAttacker[SUBMARINE] = 0;
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
        unitType.FiringRangeToDefender["submarine"] = 1;
        unitType.FiringRangeToDefender[SUBMARINE] = 1;
        unitType.FiringRangeToDefender[BATTLESHIP] = 1;
        unitType.FiringRangeToDefender[AIRCRAFT_CARRIER] = 1;
        unitType.FiringRangeToDefender[SPY] = 2;
        unitType.FiringRangeToDefender[DECOY_COMMAND_CENTER] = 2;
        unitType.FiringRangeToDefender["com"] = 2;
        unitType.FiringRangeToDefender[COMMAND_CENTER] = 2;
        unitType.FiringRangeToDefender[COMMAND_CENTER] = 2;
        unitType.FiringRangeToDefender["CommandCenter"] = 2;

        // only applies to infantry
        unitType.CanDigInByTerrainYorN["ocean"] = "N";
        unitType.CanDigInByTerrainYorN["sea"] = "N";
        unitType.CanDigInByTerrainYorN["dock"] = "Y";
        unitType.CanDigInByTerrainYorN["burb"] = "Y";
        unitType.CanDigInByTerrainYorN["village"] = "Y";
        unitType.CanDigInByTerrainYorN["town"] = "Y";
        unitType.CanDigInByTerrainYorN["city"] = "Y";
        unitType.CanDigInByTerrainYorN["capital"] = "Y";
        unitType.CanDigInByTerrainYorN["metro"] = "Y";
        unitType.CanDigInByTerrainYorN["resource"] = "Y";
        unitType.CanDigInByTerrainYorN["plain"] = "Y";
        unitType.CanDigInByTerrainYorN["grass"] = "Y";
        unitType.CanDigInByTerrainYorN["forest"] = "Y";
        unitType.CanDigInByTerrainYorN["mountain"] = "Y";
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

    public UnitType defineTransportInfantry()
    {
        UnitType unitType = new UnitType();
        unitType.Name = TRANSPORT_INFANTRY;
        unitType.LandOrSea = "sea";

        unitType.AttritionByTerrain["forest"] = 0;
        unitType.AttritionByTerrain["mountain"] = 0;

        // column
        unitType.BattleDamageFromAttacker[INFANTRY] = 25;
        unitType.BattleDamageFromAttacker[ARMOR] = 25;
        unitType.BattleDamageFromAttacker[ARMOR] = 25;
        unitType.BattleDamageFromAttacker[DUG_IN_INFANTRY] = 25;
        unitType.BattleDamageFromAttacker[TRANSPORT_INFANTRY] = 10;
        unitType.BattleDamageFromAttacker[TRANSPORT_ARMOR] = 10;
        unitType.BattleDamageFromAttacker[TRANSPORT_ARMOR] = 10;
        unitType.BattleDamageFromAttacker["submarine"] = 100;
        unitType.BattleDamageFromAttacker[SUBMARINE] = 100;
        unitType.BattleDamageFromAttacker[BATTLESHIP] = 50;
        unitType.BattleDamageFromAttacker[AIRCRAFT_CARRIER] = 50;
        unitType.BattleDamageFromAttacker[SPY] = 0;
        unitType.BattleDamageFromAttacker[DECOY_COMMAND_CENTER] = 0;
        unitType.BattleDamageFromAttacker["com"] = 20;
        unitType.BattleDamageFromAttacker[COMMAND_CENTER] = 20;
        unitType.BattleDamageFromAttacker[COMMAND_CENTER] = 20;
        unitType.BattleDamageFromAttacker["CommandCenter"] = 20;

        // row
        unitType.BattleDamageToDefender[INFANTRY] = 18;
        unitType.BattleDamageToDefender[ARMOR] = 16;
        unitType.BattleDamageToDefender[ARMOR] = 16;
        unitType.BattleDamageToDefender[DUG_IN_INFANTRY] = 14;
        unitType.BattleDamageToDefender[TRANSPORT_INFANTRY] = 10;
        unitType.BattleDamageToDefender[TRANSPORT_ARMOR] = 10;
        unitType.BattleDamageToDefender[TRANSPORT_ARMOR] = 10;
        unitType.BattleDamageToDefender["submarine"] = 5;
        unitType.BattleDamageToDefender[SUBMARINE] = 5;
        unitType.BattleDamageToDefender[BATTLESHIP] = 5;
        unitType.BattleDamageToDefender[AIRCRAFT_CARRIER] = 5;
        unitType.BattleDamageToDefender[SPY] = 34;
        unitType.BattleDamageToDefender[DECOY_COMMAND_CENTER] = 34;
        unitType.BattleDamageToDefender["com"] = 10;
        unitType.BattleDamageToDefender[COMMAND_CENTER] = 10;
        unitType.BattleDamageToDefender[COMMAND_CENTER] = 10;
        unitType.BattleDamageToDefender["CommandCenter"] = 10;

        unitType.NormalStepsAddedPerRound = 18;
        unitType.BlitzStepsAddedPerRound = 26;
        unitType.SneakStepsAddedPerRound = 9;

        unitType.Cost = 25;

        unitType.DamageReductionForDefenderByTerrain["burb"] = 0;
        unitType.DamageReductionForDefenderByTerrain["mountain"] = 0;

        unitType.DiscoveryRange = 2;
        unitType.ScanningRange = 3;
        unitType.PointsPerHit = 2;

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
        unitType.FiringRangeToDefender[INFANTRY] = 2;
        unitType.FiringRangeToDefender[ARMOR] = 2;
        unitType.FiringRangeToDefender[ARMOR] = 2;
        unitType.FiringRangeToDefender[DUG_IN_INFANTRY] = 2;
        unitType.FiringRangeToDefender[TRANSPORT_INFANTRY] = 2;
        unitType.FiringRangeToDefender[TRANSPORT_ARMOR] = 2;
        unitType.FiringRangeToDefender[TRANSPORT_ARMOR] = 2;
        unitType.FiringRangeToDefender["submarine"] = 1;
        unitType.FiringRangeToDefender[SUBMARINE] = 1;
        unitType.FiringRangeToDefender[BATTLESHIP] = 1;
        unitType.FiringRangeToDefender[AIRCRAFT_CARRIER] = 1;
        unitType.FiringRangeToDefender[SPY] = 2;
        unitType.FiringRangeToDefender[DECOY_COMMAND_CENTER] = 2;
        unitType.FiringRangeToDefender["com"] = 1;
        unitType.FiringRangeToDefender[COMMAND_CENTER] = 1;
        unitType.FiringRangeToDefender[COMMAND_CENTER] = 1;
        unitType.FiringRangeToDefender["CommandCenter"] = 1;

        // Only applies to infantry
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

    public UnitType defineDugInInfantry()
    {
        UnitType unitType = new UnitType();
        unitType.Name = DUG_IN_INFANTRY;
        unitType.LandOrSea = "land";

        // can't become dug-in infantry in swamp
        unitType.AttritionByTerrain["swamp"] = 3;

        // column
        unitType.BattleDamageFromAttacker[INFANTRY] = 15;
        unitType.BattleDamageFromAttacker[ARMOR] = 20;
        unitType.BattleDamageFromAttacker[ARMOR] = 20;
        unitType.BattleDamageFromAttacker[DUG_IN_INFANTRY] = 15;
        unitType.BattleDamageFromAttacker[TRANSPORT_INFANTRY] = 14;
        unitType.BattleDamageFromAttacker[TRANSPORT_ARMOR] = 7;
        unitType.BattleDamageFromAttacker[TRANSPORT_ARMOR] = 7;
        unitType.BattleDamageFromAttacker["submarine"] = 0;
        unitType.BattleDamageFromAttacker[SUBMARINE] = 0;
        unitType.BattleDamageFromAttacker[BATTLESHIP] = 6;
        unitType.BattleDamageFromAttacker[AIRCRAFT_CARRIER] = 5;
        unitType.BattleDamageFromAttacker[SPY] = 0;
        unitType.BattleDamageFromAttacker[DECOY_COMMAND_CENTER] = 0;
        unitType.BattleDamageFromAttacker["com"] = 20;
        unitType.BattleDamageFromAttacker[COMMAND_CENTER] = 20;
        unitType.BattleDamageFromAttacker[COMMAND_CENTER] = 20;
        unitType.BattleDamageFromAttacker["CommandCenter"] = 20;

        // row
        unitType.BattleDamageToDefender[INFANTRY] = 20;
        unitType.BattleDamageToDefender[ARMOR] = 17;
        unitType.BattleDamageToDefender[ARMOR] = 17;
        unitType.BattleDamageToDefender[DUG_IN_INFANTRY] = 15;
        unitType.BattleDamageToDefender[TRANSPORT_INFANTRY] = 25;
        unitType.BattleDamageToDefender[TRANSPORT_ARMOR] = 25;
        unitType.BattleDamageToDefender[TRANSPORT_ARMOR] = 25;
        unitType.BattleDamageToDefender["submarine"] = 25;
        unitType.BattleDamageToDefender[SUBMARINE] = 25;
        unitType.BattleDamageToDefender[BATTLESHIP] = 25;
        unitType.BattleDamageToDefender[AIRCRAFT_CARRIER] = 25;
        unitType.BattleDamageToDefender[SPY] = 34;
        unitType.BattleDamageToDefender[DECOY_COMMAND_CENTER] = 34;
        unitType.BattleDamageToDefender["com"] = 10;
        unitType.BattleDamageToDefender[COMMAND_CENTER] = 10;
        unitType.BattleDamageToDefender[COMMAND_CENTER] = 10;
        unitType.BattleDamageToDefender["CommandCenter"] = 10;

        unitType.NormalStepsAddedPerRound = 6;
        unitType.BlitzStepsAddedPerRound = 14;
        unitType.SneakStepsAddedPerRound = 3;

        unitType.Cost = 25;

        unitType.DamageReductionForDefenderByTerrain["burb"] = 1 / 3;
        unitType.DamageReductionForDefenderByTerrain["mountain"] = 1 / 4;

        unitType.DiscoveryRange = 3;
        unitType.ScanningRange = 5;
        unitType.PointsPerHit = 2;

        // column
        unitType.FiringRangeFromAttacker[INFANTRY] = 2;
        unitType.FiringRangeFromAttacker[ARMOR] = 2;
        unitType.FiringRangeFromAttacker[ARMOR] = 2;
        unitType.FiringRangeFromAttacker[DUG_IN_INFANTRY] = 2;
        unitType.FiringRangeFromAttacker[TRANSPORT_INFANTRY] = 2;
        unitType.FiringRangeFromAttacker[TRANSPORT_ARMOR] = 2;
        unitType.FiringRangeFromAttacker[TRANSPORT_ARMOR] = 2;
        unitType.FiringRangeFromAttacker["submarine"] = 0;
        unitType.FiringRangeFromAttacker[SUBMARINE] = 0;
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
        unitType.FiringRangeToDefender["submarine"] = 1;
        unitType.FiringRangeToDefender[SUBMARINE] = 1;
        unitType.FiringRangeToDefender[BATTLESHIP] = 1;
        unitType.FiringRangeToDefender[AIRCRAFT_CARRIER] = 1;
        unitType.FiringRangeToDefender[SPY] = 2;
        unitType.FiringRangeToDefender[DECOY_COMMAND_CENTER] = 2;
        unitType.FiringRangeToDefender["com"] = 2;
        unitType.FiringRangeToDefender[COMMAND_CENTER] = 2;
        unitType.FiringRangeToDefender[COMMAND_CENTER] = 2;
        unitType.FiringRangeToDefender["CommandCenter"] = 2;

        // only applies to infantry
        unitType.CanDigInByTerrainYorN["ocean"] = "N";
        unitType.CanDigInByTerrainYorN["sea"] = "N";
        unitType.CanDigInByTerrainYorN["dock"] = "Y";
        unitType.CanDigInByTerrainYorN["burb"] = "Y";
        unitType.CanDigInByTerrainYorN["village"] = "Y";
        unitType.CanDigInByTerrainYorN["town"] = "Y";
        unitType.CanDigInByTerrainYorN["city"] = "Y";
        unitType.CanDigInByTerrainYorN["capital"] = "Y";
        unitType.CanDigInByTerrainYorN["metro"] = "Y";
        unitType.CanDigInByTerrainYorN["resource"] = "Y";
        unitType.CanDigInByTerrainYorN["plain"] = "Y";
        unitType.CanDigInByTerrainYorN["grass"] = "Y";
        unitType.CanDigInByTerrainYorN["forest"] = "Y";
        unitType.CanDigInByTerrainYorN["mountain"] = "Y";
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