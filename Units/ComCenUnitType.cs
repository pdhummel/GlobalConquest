using static UnitTypeConstants;
namespace GlobalConquest.Units;

public class ComCenUnitType : UnitType
{

    public ComCenUnitType()
    {

    }

    public UnitType defineComCen()
    {
        UnitType unitType = new UnitType();
        unitType.Name = "comcen";
        unitType.LandOrSea = "both";

        // can't become dug-in infantry in swamp
        unitType.AttritionByTerrain["swamp"] = 0;

        // column
        unitType.BattleDamageFromAttacker[INFANTRY] = 10;
        unitType.BattleDamageFromAttacker[ARMOR] = 10;
        unitType.BattleDamageFromAttacker[ARMOR] = 10;
        unitType.BattleDamageFromAttacker[DUG_IN_INFANTRY] = 10;
        unitType.BattleDamageFromAttacker[TRANSPORT_INFANTRY] = 10;
        unitType.BattleDamageFromAttacker[TRANSPORT_ARMOR] = 10;
        unitType.BattleDamageFromAttacker[TRANSPORT_ARMOR] = 10;
        unitType.BattleDamageFromAttacker["submarine"] = 10;
        unitType.BattleDamageFromAttacker["sub"] = 10;
        unitType.BattleDamageFromAttacker["battleship"] = 10;
        unitType.BattleDamageFromAttacker["carrier"] = 10;
        unitType.BattleDamageFromAttacker[SPY] = 0;
        unitType.BattleDamageFromAttacker["decoy-comcen"] = 0;
        unitType.BattleDamageFromAttacker["com"] = 10;
        unitType.BattleDamageFromAttacker["ComCen"] = 10;
        unitType.BattleDamageFromAttacker["comcen"] = 10;
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
        unitType.BattleDamageToDefender["sub"] = 20;
        unitType.BattleDamageToDefender["battleship"] = 20;
        unitType.BattleDamageToDefender["carrier"] = 20;
        unitType.BattleDamageToDefender[SPY] = 34;
        unitType.BattleDamageToDefender["decoy-comcen"] = 34;
        unitType.BattleDamageToDefender["com"] = 10;
        unitType.BattleDamageToDefender["ComCen"] = 10;
        unitType.BattleDamageToDefender["comcen"] = 10;
        unitType.BattleDamageToDefender["CommandCenter"] = 10;

        unitType.NormalStepsAddedPerRound = 20;
        unitType.BlitzStepsAddedPerRound = 28;
        unitType.SneakStepsAddedPerRound = 10;

        unitType.Cost = 0;  // cannot be purchased

        unitType.DamageReductionForDefenderByTerrain["burb"] = 0;
        unitType.DamageReductionForDefenderByTerrain["mountain"] = 0;

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
        unitType.FiringRangeFromAttacker["sub"] = 2;
        unitType.FiringRangeFromAttacker["battleship"] = 3;
        unitType.FiringRangeFromAttacker["carrier"] = 4;
        unitType.FiringRangeFromAttacker[SPY] = 0;
        unitType.FiringRangeFromAttacker["decoy-comcen"] = 0;
        unitType.FiringRangeFromAttacker["com"] = 2;
        unitType.FiringRangeFromAttacker["ComCen"] = 2;
        unitType.FiringRangeFromAttacker["comcen"] = 2;
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
        unitType.FiringRangeToDefender["sub"] = 2;
        unitType.FiringRangeToDefender["battleship"] = 3;
        unitType.FiringRangeToDefender["carrier"] = 4;
        unitType.FiringRangeToDefender[SPY] = 2;
        unitType.FiringRangeToDefender["decoy-comcen"] = 2;
        unitType.FiringRangeToDefender["com"] = 2;
        unitType.FiringRangeToDefender["ComCen"] = 2;
        unitType.FiringRangeToDefender["comcen"] = 2;
        unitType.FiringRangeToDefender["CommandCenter"] = 2;

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

    public UnitType defineDecoyComCen()
    {
        UnitType unitType = new UnitType();
        unitType.Name = "decoy-comcen";
        unitType.LandOrSea = "both";

        // can't become dug-in infantry in swamp
        unitType.AttritionByTerrain["swamp"] = 0;

        // column
        unitType.BattleDamageFromAttacker[INFANTRY] = 34;
        unitType.BattleDamageFromAttacker[ARMOR] = 34;
        unitType.BattleDamageFromAttacker[ARMOR] = 34;
        unitType.BattleDamageFromAttacker[DUG_IN_INFANTRY] = 34;
        unitType.BattleDamageFromAttacker[TRANSPORT_INFANTRY] = 34;
        unitType.BattleDamageFromAttacker[TRANSPORT_ARMOR] = 34;
        unitType.BattleDamageFromAttacker[TRANSPORT_ARMOR] = 34;
        unitType.BattleDamageFromAttacker["submarine"] = 34;
        unitType.BattleDamageFromAttacker["sub"] = 34;
        unitType.BattleDamageFromAttacker["battleship"] = 34;
        unitType.BattleDamageFromAttacker["carrier"] = 34;
        unitType.BattleDamageFromAttacker[SPY] = 0;
        unitType.BattleDamageFromAttacker["decoy-comcen"] = 0;
        unitType.BattleDamageFromAttacker["com"] = 34;
        unitType.BattleDamageFromAttacker["ComCen"] = 34;
        unitType.BattleDamageFromAttacker["comcen"] = 34;
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
        unitType.BattleDamageToDefender["sub"] = 0;
        unitType.BattleDamageToDefender["battleship"] = 0;
        unitType.BattleDamageToDefender["carrier"] = 0;
        unitType.BattleDamageToDefender[SPY] = 0;
        unitType.BattleDamageToDefender["decoy-comcen"] = 0;
        unitType.BattleDamageToDefender["com"] = 0;
        unitType.BattleDamageToDefender["ComCen"] = 0;
        unitType.BattleDamageToDefender["comcen"] = 0;
        unitType.BattleDamageToDefender["CommandCenter"] = 0;

        unitType.NormalStepsAddedPerRound = 20;
        unitType.BlitzStepsAddedPerRound = 28;
        unitType.SneakStepsAddedPerRound = 10;

        unitType.Cost = 15;

        unitType.DamageReductionForDefenderByTerrain["burb"] = 0;
        unitType.DamageReductionForDefenderByTerrain["mountain"] = 0;

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
        unitType.FiringRangeFromAttacker["sub"] = 2;
        unitType.FiringRangeFromAttacker["battleship"] = 3;
        unitType.FiringRangeFromAttacker["carrier"] = 4;
        unitType.FiringRangeFromAttacker[SPY] = 0;
        unitType.FiringRangeFromAttacker["decoy-comcen"] = 0;
        unitType.FiringRangeFromAttacker["com"] = 2;
        unitType.FiringRangeFromAttacker["ComCen"] = 2;
        unitType.FiringRangeFromAttacker["comcen"] = 2;
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
        unitType.FiringRangeToDefender["sub"] = 0;
        unitType.FiringRangeToDefender["battleship"] = 0;
        unitType.FiringRangeToDefender["carrier"] = 0;
        unitType.FiringRangeToDefender[SPY] = 0;
        unitType.FiringRangeToDefender["decoy-comcen"] = 0;
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