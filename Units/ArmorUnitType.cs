namespace GlobalConquest.Units;

public class ArmorUnitType : UnitType
{

    public ArmorUnitType()
    {

    }

    public UnitType defineArmor()
    {
        UnitType unitType = new UnitType();
        unitType.Name = "armor";
        unitType.LandOrSea = "land";

        unitType.AttritionByTerrain["forest"] = 1;
        unitType.AttritionByTerrain["mountain"] = 2;

        // column
        unitType.BattleDamageFromAttacker["infantry"] = 17;
        unitType.BattleDamageFromAttacker["armor"] = 20;
        unitType.BattleDamageFromAttacker["tank"] = 20;
        unitType.BattleDamageFromAttacker["dug-in-infantry"] = 17;
        unitType.BattleDamageFromAttacker["transport-infantry"] = 16;
        unitType.BattleDamageFromAttacker["transport-armor"] = 8;
        unitType.BattleDamageFromAttacker["transport-tank"] = 8;
        unitType.BattleDamageFromAttacker["submarine"] = 0;
        unitType.BattleDamageFromAttacker["sub"] = 0;
        unitType.BattleDamageFromAttacker["battleship"] = 10;
        unitType.BattleDamageFromAttacker["carrier"] = 8;
        unitType.BattleDamageFromAttacker["spy"] = 0;
        unitType.BattleDamageFromAttacker["com"] = 20;
        unitType.BattleDamageFromAttacker["ComCen"] = 20;
        unitType.BattleDamageFromAttacker["comcen"] = 20;
        unitType.BattleDamageFromAttacker["CommandCenter"] = 20;

        // row
        unitType.BattleDamageToDefender["infantry"] = 26;
        unitType.BattleDamageToDefender["armor"] = 20;
        unitType.BattleDamageToDefender["tank"] = 20;
        unitType.BattleDamageToDefender["dug-in-infantry"] = 20;
        unitType.BattleDamageToDefender["transport-infantry"] = 25;
        unitType.BattleDamageToDefender["transport-armor"] = 25;
        unitType.BattleDamageToDefender["transport-tank"] = 25;
        unitType.BattleDamageToDefender["submarine"] = 25;
        unitType.BattleDamageToDefender["sub"] = 25;
        unitType.BattleDamageToDefender["battleship"] = 25;
        unitType.BattleDamageToDefender["carrier"] = 25;
        unitType.BattleDamageToDefender["spy"] = 34;
        unitType.BattleDamageToDefender["com"] = 10;
        unitType.BattleDamageToDefender["ComCen"] = 10;
        unitType.BattleDamageToDefender["comcen"] = 10;
        unitType.BattleDamageToDefender["CommandCenter"] = 10;

        unitType.NormalStepsAddedPerRound = 12;
        unitType.BlitzStepsAddedPerRound = 20;
        unitType.SneakStepsAddedPerRound = 6;

        unitType.Cost = 35;

        unitType.DamageReductionForDefenderByTerrain["burb"] = 1 / 5;
        unitType.DamageReductionForDefenderByTerrain["mountain"] = 1 / 6;

        unitType.DiscoveryRange = 3;
        unitType.ScanningRange = 5;
        unitType.PointsPerHit = 5;

        // column
        unitType.FiringRangeFromAttacker["infantry"] = 2;
        unitType.FiringRangeFromAttacker["armor"] = 2;
        unitType.FiringRangeFromAttacker["tank"] = 2;
        unitType.FiringRangeFromAttacker["dug-in-infantry"] = 2;
        unitType.FiringRangeFromAttacker["transport-infantry"] = 2;
        unitType.FiringRangeFromAttacker["transport-armor"] = 2;
        unitType.FiringRangeFromAttacker["transport-tank"] = 2;
        unitType.FiringRangeFromAttacker["submarine"] = 0;
        unitType.FiringRangeFromAttacker["sub"] = 0;
        unitType.FiringRangeFromAttacker["battleship"] = 3;
        unitType.FiringRangeFromAttacker["carrier"] = 4;
        unitType.FiringRangeFromAttacker["spy"] = 0;
        unitType.FiringRangeFromAttacker["com"] = 2;
        unitType.FiringRangeFromAttacker["ComCen"] = 2;
        unitType.FiringRangeFromAttacker["comcen"] = 2;
        unitType.FiringRangeFromAttacker["CommandCenter"] = 2;

        // row
        unitType.FiringRangeToDefender["infantry"] = 2;
        unitType.FiringRangeToDefender["armor"] = 2;
        unitType.FiringRangeToDefender["tank"] = 2;
        unitType.FiringRangeToDefender["dug-in-infantry"] = 2;
        unitType.FiringRangeToDefender["transport-infantry"] = 2;
        unitType.FiringRangeToDefender["transport-armor"] = 2;
        unitType.FiringRangeToDefender["transport-tank"] = 2;
        unitType.FiringRangeToDefender["submarine"] = 1;
        unitType.FiringRangeToDefender["sub"] = 1;
        unitType.FiringRangeToDefender["battleship"] = 1;
        unitType.FiringRangeToDefender["carrier"] = 1;
        unitType.FiringRangeToDefender["spy"] = 2;
        unitType.FiringRangeToDefender["com"] = 2;
        unitType.FiringRangeToDefender["ComCen"] = 2;
        unitType.FiringRangeToDefender["comcen"] = 2;
        unitType.FiringRangeToDefender["CommandCenter"] = 2;

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



    public UnitType defineTransportArmor()
    {
        UnitType unitType = new UnitType();
        unitType.Name = "transport-tank";
        unitType.LandOrSea = "sea";

        unitType.AttritionByTerrain["forest"] = 0;
        unitType.AttritionByTerrain["mountain"] = 0;

        // column
        unitType.BattleDamageFromAttacker["infantry"] = 25;
        unitType.BattleDamageFromAttacker["armor"] = 25;
        unitType.BattleDamageFromAttacker["tank"] = 25;
        unitType.BattleDamageFromAttacker["dug-in-infantry"] = 25;
        unitType.BattleDamageFromAttacker["transport-infantry"] = 10;
        unitType.BattleDamageFromAttacker["transport-armor"] = 10;
        unitType.BattleDamageFromAttacker["transport-tank"] = 10;
        unitType.BattleDamageFromAttacker["submarine"] = 100;
        unitType.BattleDamageFromAttacker["sub"] = 100;
        unitType.BattleDamageFromAttacker["battleship"] = 50;
        unitType.BattleDamageFromAttacker["carrier"] = 50;
        unitType.BattleDamageFromAttacker["spy"] = 0;
        unitType.BattleDamageFromAttacker["com"] = 20;
        unitType.BattleDamageFromAttacker["ComCen"] = 20;
        unitType.BattleDamageFromAttacker["comcen"] = 20;
        unitType.BattleDamageFromAttacker["CommandCenter"] = 20;

        // row
        unitType.BattleDamageToDefender["infantry"] = 9;
        unitType.BattleDamageToDefender["armor"] = 8;
        unitType.BattleDamageToDefender["tank"] = 8;
        unitType.BattleDamageToDefender["dug-in-infantry"] = 7;
        unitType.BattleDamageToDefender["transport-infantry"] = 10;
        unitType.BattleDamageToDefender["transport-armor"] = 10;
        unitType.BattleDamageToDefender["transport-tank"] = 10;
        unitType.BattleDamageToDefender["submarine"] = 5;
        unitType.BattleDamageToDefender["sub"] = 5;
        unitType.BattleDamageToDefender["battleship"] = 5;
        unitType.BattleDamageToDefender["carrier"] = 5;
        unitType.BattleDamageToDefender["spy"] = 34;
        unitType.BattleDamageToDefender["com"] = 10;
        unitType.BattleDamageToDefender["ComCen"] = 10;
        unitType.BattleDamageToDefender["comcen"] = 10;
        unitType.BattleDamageToDefender["CommandCenter"] = 10;

        unitType.NormalStepsAddedPerRound = 18;
        unitType.BlitzStepsAddedPerRound = 26;
        unitType.SneakStepsAddedPerRound = 9;

        unitType.Cost = 23;

        unitType.DamageReductionForDefenderByTerrain["burb"] = 0;
        unitType.DamageReductionForDefenderByTerrain["mountain"] = 0;

        unitType.DiscoveryRange = 2;
        unitType.ScanningRange = 3;
        unitType.PointsPerHit = 5;

        // column
        unitType.FiringRangeFromAttacker["infantry"] = 2;
        unitType.FiringRangeFromAttacker["armor"] = 2;
        unitType.FiringRangeFromAttacker["tank"] = 2;
        unitType.FiringRangeFromAttacker["dug-in-infantry"] = 2;
        unitType.FiringRangeFromAttacker["transport-infantry"] = 2;
        unitType.FiringRangeFromAttacker["transport-armor"] = 2;
        unitType.FiringRangeFromAttacker["transport-tank"] = 2;
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
        unitType.FiringRangeToDefender["infantry"] = 2;
        unitType.FiringRangeToDefender["armor"] = 2;
        unitType.FiringRangeToDefender["tank"] = 2;
        unitType.FiringRangeToDefender["dug-in-infantry"] = 2;
        unitType.FiringRangeToDefender["transport-infantry"] = 2;
        unitType.FiringRangeToDefender["transport-armor"] = 2;
        unitType.FiringRangeToDefender["transport-tank"] = 2;
        unitType.FiringRangeToDefender["submarine"] = 1;
        unitType.FiringRangeToDefender["sub"] = 1;
        unitType.FiringRangeToDefender["battleship"] = 1;
        unitType.FiringRangeToDefender["carrier"] = 1;
        unitType.FiringRangeToDefender["spy"] = 2;
        unitType.FiringRangeToDefender["com"] = 1;
        unitType.FiringRangeToDefender["ComCen"] = 1;
        unitType.FiringRangeToDefender["comcen"] = 1;
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


}