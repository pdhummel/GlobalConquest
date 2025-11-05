namespace GlobalConquest.Units;

public class SubUnitType : UnitType
{

    public SubUnitType()
    {

    }

    public UnitType defineSub()
    {
        UnitType unitType = new UnitType();
        unitType.Name = "sub";
        unitType.LandOrSea = "sea";

        // can't become dug-in infantry in swamp
        unitType.AttritionByTerrain["swamp"] = 0;

        // column
        unitType.BattleDamageFromAttacker["infantry"] = 25;
        unitType.BattleDamageFromAttacker["armor"] = 25;
        unitType.BattleDamageFromAttacker["tank"] = 25;
        unitType.BattleDamageFromAttacker["dug-in-infantry"] = 25;
        unitType.BattleDamageFromAttacker["transport-infantry"] = 5;
        unitType.BattleDamageFromAttacker["transport-armor"] = 5;
        unitType.BattleDamageFromAttacker["transport-tank"] = 5;
        unitType.BattleDamageFromAttacker["submarine"] = 25;
        unitType.BattleDamageFromAttacker["sub"] = 25;
        unitType.BattleDamageFromAttacker["battleship"] = 34;
        unitType.BattleDamageFromAttacker["carrier"] = 25;
        unitType.BattleDamageFromAttacker["spy"] = 0;
        unitType.BattleDamageFromAttacker["com"] = 20;
        unitType.BattleDamageFromAttacker["ComCen"] = 20;
        unitType.BattleDamageFromAttacker["comcen"] = 20;
        unitType.BattleDamageFromAttacker["CommandCenter"] = 20;

        // row
        unitType.BattleDamageToDefender["infantry"] = 0;
        unitType.BattleDamageToDefender["armor"] = 0;
        unitType.BattleDamageToDefender["tank"] = 0;
        unitType.BattleDamageToDefender["dug-in-infantry"] = 0;
        unitType.BattleDamageToDefender["transport-infantry"] = 100;
        unitType.BattleDamageToDefender["transport-armor"] = 100;
        unitType.BattleDamageToDefender["transport-tank"] = 100;
        unitType.BattleDamageToDefender["submarine"] = 25;
        unitType.BattleDamageToDefender["sub"] = 25;
        unitType.BattleDamageToDefender["battleship"] = 34;
        unitType.BattleDamageToDefender["carrier"] = 34;
        unitType.BattleDamageToDefender["spy"] = 34;
        unitType.BattleDamageToDefender["com"] = 10;
        unitType.BattleDamageToDefender["ComCen"] = 10;
        unitType.BattleDamageToDefender["comcen"] = 10;
        unitType.BattleDamageToDefender["CommandCenter"] = 10;

        unitType.NormalStepsAddedPerRound = 20;
        unitType.BlitzStepsAddedPerRound = 28;
        unitType.SneakStepsAddedPerRound = 10;

        unitType.Cost = 25;

        unitType.DamageReductionForDefenderByTerrain["burb"] = 0;
        unitType.DamageReductionForDefenderByTerrain["mountain"] = 0;

        unitType.DiscoveryRange = 1;
        // Sub range is reduced to 3 if target not moving. 
        // Subs can only be spotted at a range of 1 if they are stationary or if the scanning unit 
        // is moving regardless of unit's normal range.
        unitType.ScanningRange = 4;

        // column
        unitType.FiringRangeFromAttacker["infantry"] = 1;
        unitType.FiringRangeFromAttacker["armor"] = 1;
        unitType.FiringRangeFromAttacker["tank"] = 1;
        unitType.FiringRangeFromAttacker["dug-in-infantry"] = 1;
        unitType.FiringRangeFromAttacker["transport-infantry"] = 1;
        unitType.FiringRangeFromAttacker["transport-armor"] = 1;
        unitType.FiringRangeFromAttacker["transport-tank"] = 1;
        unitType.FiringRangeFromAttacker["submarine"] = 2;
        unitType.FiringRangeFromAttacker["sub"] = 2;
        unitType.FiringRangeFromAttacker["battleship"] = 3;
        unitType.FiringRangeFromAttacker["carrier"] = 4;
        unitType.FiringRangeFromAttacker["spy"] = 0;
        unitType.FiringRangeFromAttacker["com"] = 2;
        unitType.FiringRangeFromAttacker["ComCen"] = 2;
        unitType.FiringRangeFromAttacker["comcen"] = 2;
        unitType.FiringRangeFromAttacker["CommandCenter"] = 2;

        // row
        unitType.FiringRangeToDefender["infantry"] = 0;
        unitType.FiringRangeToDefender["armor"] = 0;
        unitType.FiringRangeToDefender["tank"] = 0;
        unitType.FiringRangeToDefender["dug-in-infantry"] = 0;
        unitType.FiringRangeToDefender["transport-infantry"] = 2;
        unitType.FiringRangeToDefender["transport-armor"] = 2;
        unitType.FiringRangeToDefender["transport-tank"] = 2;
        unitType.FiringRangeToDefender["submarine"] = 2;
        unitType.FiringRangeToDefender["sub"] = 2;
        unitType.FiringRangeToDefender["battleship"] = 2;
        unitType.FiringRangeToDefender["carrier"] = 2;
        unitType.FiringRangeToDefender["spy"] = 2;
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

}