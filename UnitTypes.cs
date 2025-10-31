public class UnitTypes
{
    public Dictionary<string, UnitType> UnitTypeMap = new Dictionary<string, UnitType>();
    public UnitTypes()
    {
        defineInfantry();
        defineArmor();
        defineTransportInfantry();
        defineTransportArmor();
        defineDugInInfantry();
        defineSub();
        defineBattleship();
        defineCarrier();
        defineSpy();
        defineComCen();
    }

    private void defineInfantry()
    {
        UnitType unitType = new UnitType();
        unitType.Name = "infantry";
        unitType.LandOrSea = "land";

        unitType.AttritionByTerrain["swamp"] = 3;

        // column
        unitType.BattleDamageFromAttacker["infantry"] = 20;
        unitType.BattleDamageFromAttacker["armor"] = 26;
        unitType.BattleDamageFromAttacker["tank"] = 26;
        unitType.BattleDamageFromAttacker["dug-in-infantry"] = 20;
        unitType.BattleDamageFromAttacker["transport-infantry"] = 18;
        unitType.BattleDamageFromAttacker["transport-armor"] = 9;
        unitType.BattleDamageFromAttacker["transport-tank"] = 9;
        unitType.BattleDamageFromAttacker["submarine"] = 0;
        unitType.BattleDamageFromAttacker["sub"] = 0;
        unitType.BattleDamageFromAttacker["battleship"] = 12;
        unitType.BattleDamageFromAttacker["carrier"] = 10;
        unitType.BattleDamageFromAttacker["spy"] = 0;
        unitType.BattleDamageFromAttacker["com"] = 20;
        unitType.BattleDamageFromAttacker["ComCen"] = 20;
        unitType.BattleDamageFromAttacker["comcen"] = 20;
        unitType.BattleDamageFromAttacker["CommandCenter"] = 20;

        // row
        unitType.BattleDamageToDefender["infantry"] = 20;
        unitType.BattleDamageToDefender["armor"] = 17;
        unitType.BattleDamageToDefender["tank"] = 17;
        unitType.BattleDamageToDefender["dug-in-infantry"] = 15;
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

        unitType.NormalStepsAddedPerRound = 6;
        unitType.BlitzStepsAddedPerRound = 14;
        unitType.SneakStepsAddedPerRound = 3;

        unitType.Cost = 25;

        unitType.DamageReductionForDefenderByTerrain["burb"] = 1 / 3;
        unitType.DamageReductionForDefenderByTerrain["mountain"] = 1 / 4;

        unitType.DiscoveryRange = 3;
        unitType.ScanningRange = 5;

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
        unitType.FiringRangeToDefender["comcen"] = 2;
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

        UnitTypeMap[unitType.Name] = unitType;
    }

    private void defineArmor()
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

        UnitTypeMap[unitType.Name] = unitType;
        UnitTypeMap["armor"] = unitType;
        UnitTypeMap["tank"] = unitType;
    }


    private void defineTransportInfantry()
    {
        UnitType unitType = new UnitType();
        unitType.Name = "transport-infantry";
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
        unitType.BattleDamageToDefender["infantry"] = 18;
        unitType.BattleDamageToDefender["armor"] = 16;
        unitType.BattleDamageToDefender["tank"] = 16;
        unitType.BattleDamageToDefender["dug-in-infantry"] = 14;
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

        unitType.Cost = 25;

        unitType.DamageReductionForDefenderByTerrain["burb"] = 0;
        unitType.DamageReductionForDefenderByTerrain["mountain"] = 0;

        unitType.DiscoveryRange = 2;
        unitType.ScanningRange = 3;

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

        UnitTypeMap[unitType.Name] = unitType;
    }

    private void defineTransportArmor()
    {
        UnitType unitType = new UnitType();
        unitType.Name = "transport-armor";
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

        UnitTypeMap[unitType.Name] = unitType;
    }


    private void defineDugInInfantry()
    {
        UnitType unitType = new UnitType();
        unitType.Name = "dug-in-infantry";
        unitType.LandOrSea = "land";

        // can't become dug-in infantry in swamp
        unitType.AttritionByTerrain["swamp"] = 3;

        // column
        unitType.BattleDamageFromAttacker["infantry"] = 15;
        unitType.BattleDamageFromAttacker["armor"] = 20;
        unitType.BattleDamageFromAttacker["tank"] = 20;
        unitType.BattleDamageFromAttacker["dug-in-infantry"] = 15;
        unitType.BattleDamageFromAttacker["transport-infantry"] = 14;
        unitType.BattleDamageFromAttacker["transport-armor"] = 7;
        unitType.BattleDamageFromAttacker["transport-tank"] = 7;
        unitType.BattleDamageFromAttacker["submarine"] = 0;
        unitType.BattleDamageFromAttacker["sub"] = 0;
        unitType.BattleDamageFromAttacker["battleship"] = 6;
        unitType.BattleDamageFromAttacker["carrier"] = 5;
        unitType.BattleDamageFromAttacker["spy"] = 0;
        unitType.BattleDamageFromAttacker["com"] = 20;
        unitType.BattleDamageFromAttacker["ComCen"] = 20;
        unitType.BattleDamageFromAttacker["comcen"] = 20;
        unitType.BattleDamageFromAttacker["CommandCenter"] = 20;

        // row
        unitType.BattleDamageToDefender["infantry"] = 20;
        unitType.BattleDamageToDefender["armor"] = 17;
        unitType.BattleDamageToDefender["tank"] = 17;
        unitType.BattleDamageToDefender["dug-in-infantry"] = 15;
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

        unitType.NormalStepsAddedPerRound = 6;
        unitType.BlitzStepsAddedPerRound = 14;
        unitType.SneakStepsAddedPerRound = 3;

        unitType.Cost = 25;

        unitType.DamageReductionForDefenderByTerrain["burb"] = 1 / 3;
        unitType.DamageReductionForDefenderByTerrain["mountain"] = 1 / 4;

        unitType.DiscoveryRange = 3;
        unitType.ScanningRange = 5;

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

        UnitTypeMap[unitType.Name] = unitType;
    }

    private void defineSub()
    {
        UnitType unitType = new UnitType();
        unitType.Name = "submarine";
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

        UnitTypeMap[unitType.Name] = unitType;
        UnitTypeMap["sub"] = unitType;
    }

    private void defineBattleship()
    {
        UnitType unitType = new UnitType();
        unitType.Name = "battleship";
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
        unitType.BattleDamageFromAttacker["sub"] = 34;
        unitType.BattleDamageFromAttacker["battleship"] = 34;
        unitType.BattleDamageFromAttacker["carrier"] = 25;
        unitType.BattleDamageFromAttacker["spy"] = 0;
        unitType.BattleDamageFromAttacker["com"] = 20;
        unitType.BattleDamageFromAttacker["ComCen"] = 20;
        unitType.BattleDamageFromAttacker["comcen"] = 20;
        unitType.BattleDamageFromAttacker["CommandCenter"] = 20;

        // row
        unitType.BattleDamageToDefender["infantry"] = 12;
        unitType.BattleDamageToDefender["armor"] = 10;
        unitType.BattleDamageToDefender["tank"] = 10;
        unitType.BattleDamageToDefender["dug-in-infantry"] = 6;
        unitType.BattleDamageToDefender["transport-infantry"] = 50;
        unitType.BattleDamageToDefender["transport-armor"] = 50;
        unitType.BattleDamageToDefender["transport-tank"] = 50;
        unitType.BattleDamageToDefender["submarine"] = 34;
        unitType.BattleDamageToDefender["sub"] = 34;
        unitType.BattleDamageToDefender["battleship"] = 25;
        unitType.BattleDamageToDefender["carrier"] = 34;
        unitType.BattleDamageToDefender["spy"] = 34;
        unitType.BattleDamageToDefender["com"] = 10;
        unitType.BattleDamageToDefender["ComCen"] = 10;
        unitType.BattleDamageToDefender["comcen"] = 10;
        unitType.BattleDamageToDefender["CommandCenter"] = 10;

        unitType.NormalStepsAddedPerRound = 20;
        unitType.BlitzStepsAddedPerRound = 28;
        unitType.SneakStepsAddedPerRound = 10;

        unitType.Cost = 35;

        unitType.DamageReductionForDefenderByTerrain["burb"] = 0;
        unitType.DamageReductionForDefenderByTerrain["mountain"] = 0;

        unitType.DiscoveryRange = 4;
        unitType.ScanningRange = 5;

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
        unitType.FiringRangeFromAttacker["com"] = 3;
        unitType.FiringRangeFromAttacker["ComCen"] = 3;
        unitType.FiringRangeFromAttacker["comcen"] = 3;
        unitType.FiringRangeFromAttacker["CommandCenter"] = 3;

        // row
        unitType.FiringRangeToDefender["infantry"] = 3;
        unitType.FiringRangeToDefender["armor"] = 3;
        unitType.FiringRangeToDefender["tank"] = 3;
        unitType.FiringRangeToDefender["dug-in-infantry"] = 3;
        unitType.FiringRangeToDefender["transport-infantry"] = 3;
        unitType.FiringRangeToDefender["transport-armor"] = 3;
        unitType.FiringRangeToDefender["transport-tank"] = 3;
        unitType.FiringRangeToDefender["submarine"] = 3;
        unitType.FiringRangeToDefender["sub"] = 3;
        unitType.FiringRangeToDefender["battleship"] = 3;
        unitType.FiringRangeToDefender["carrier"] = 3;
        unitType.FiringRangeToDefender["spy"] = 3;
        unitType.FiringRangeToDefender["com"] = 3;
        unitType.FiringRangeToDefender["ComCen"] = 3;
        unitType.FiringRangeToDefender["comcen"] = 3;
        unitType.FiringRangeToDefender["CommandCenter"] = 3;

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

        UnitTypeMap[unitType.Name] = unitType;
    }

    private void defineCarrier()
    {
        UnitType unitType = new UnitType();
        unitType.Name = "carrier";
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
        unitType.BattleDamageFromAttacker["sub"] = 34;
        unitType.BattleDamageFromAttacker["battleship"] = 34;
        unitType.BattleDamageFromAttacker["carrier"] = 25;
        unitType.BattleDamageFromAttacker["spy"] = 0;
        unitType.BattleDamageFromAttacker["com"] = 20;
        unitType.BattleDamageFromAttacker["ComCen"] = 20;
        unitType.BattleDamageFromAttacker["comcen"] = 20;
        unitType.BattleDamageFromAttacker["CommandCenter"] = 20;

        // row
        unitType.BattleDamageToDefender["infantry"] = 10;
        unitType.BattleDamageToDefender["armor"] = 8;
        unitType.BattleDamageToDefender["tank"] = 8;
        unitType.BattleDamageToDefender["dug-in-infantry"] = 5;
        unitType.BattleDamageToDefender["transport-infantry"] = 50;
        unitType.BattleDamageToDefender["transport-armor"] = 50;
        unitType.BattleDamageToDefender["transport-tank"] = 50;
        unitType.BattleDamageToDefender["submarine"] = 25;
        unitType.BattleDamageToDefender["sub"] = 25;
        unitType.BattleDamageToDefender["battleship"] = 20;
        unitType.BattleDamageToDefender["carrier"] = 25;
        unitType.BattleDamageToDefender["spy"] = 34;
        unitType.BattleDamageToDefender["com"] = 10;
        unitType.BattleDamageToDefender["ComCen"] = 10;
        unitType.BattleDamageToDefender["comcen"] = 10;
        unitType.BattleDamageToDefender["CommandCenter"] = 10;

        unitType.NormalStepsAddedPerRound = 20;
        unitType.BlitzStepsAddedPerRound = 28;
        unitType.SneakStepsAddedPerRound = 10;

        unitType.Cost = 45;

        unitType.DamageReductionForDefenderByTerrain["burb"] = 0;
        unitType.DamageReductionForDefenderByTerrain["mountain"] = 0;

        unitType.DiscoveryRange = 5;
        unitType.ScanningRange = 6;

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
        unitType.FiringRangeFromAttacker["com"] = 4;
        unitType.FiringRangeFromAttacker["ComCen"] = 4;
        unitType.FiringRangeFromAttacker["comcen"] = 4;
        unitType.FiringRangeFromAttacker["CommandCenter"] = 4;

        // row
        unitType.FiringRangeToDefender["infantry"] = 4;
        unitType.FiringRangeToDefender["armor"] = 4;
        unitType.FiringRangeToDefender["tank"] = 4;
        unitType.FiringRangeToDefender["dug-in-infantry"] = 4;
        unitType.FiringRangeToDefender["transport-infantry"] = 4;
        unitType.FiringRangeToDefender["transport-armor"] = 4;
        unitType.FiringRangeToDefender["transport-tank"] = 4;
        unitType.FiringRangeToDefender["submarine"] = 4;
        unitType.FiringRangeToDefender["sub"] = 4;
        unitType.FiringRangeToDefender["battleship"] = 4;
        unitType.FiringRangeToDefender["carrier"] = 4;
        unitType.FiringRangeToDefender["spy"] = 4;
        unitType.FiringRangeToDefender["com"] = 4;
        unitType.FiringRangeToDefender["ComCen"] = 4;
        unitType.FiringRangeToDefender["comcen"] = 4;
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

        UnitTypeMap[unitType.Name] = unitType;
    }


    private void defineSpy()
    {
        UnitType unitType = new UnitType();
        unitType.Name = "spy";
        unitType.LandOrSea = "both";

        // can't become dug-in infantry in swamp
        unitType.AttritionByTerrain["swamp"] = 0;

        // column
        unitType.BattleDamageFromAttacker["infantry"] = 34;
        unitType.BattleDamageFromAttacker["armor"] = 34;
        unitType.BattleDamageFromAttacker["tank"] = 34;
        unitType.BattleDamageFromAttacker["dug-in-infantry"] = 34;
        unitType.BattleDamageFromAttacker["transport-infantry"] = 34;
        unitType.BattleDamageFromAttacker["transport-armor"] = 34;
        unitType.BattleDamageFromAttacker["transport-tank"] = 34;
        unitType.BattleDamageFromAttacker["submarine"] = 34;
        unitType.BattleDamageFromAttacker["sub"] = 34;
        unitType.BattleDamageFromAttacker["battleship"] = 34;
        unitType.BattleDamageFromAttacker["carrier"] = 34;
        unitType.BattleDamageFromAttacker["spy"] = 0;
        unitType.BattleDamageFromAttacker["com"] = 34;
        unitType.BattleDamageFromAttacker["ComCen"] = 34;
        unitType.BattleDamageFromAttacker["comcen"] = 34;
        unitType.BattleDamageFromAttacker["CommandCenter"] = 34;

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

        unitType.NormalStepsAddedPerRound = 20;
        unitType.BlitzStepsAddedPerRound = 28;
        unitType.SneakStepsAddedPerRound = 10;

        unitType.Cost = 85;

        unitType.DamageReductionForDefenderByTerrain["burb"] = 0;
        unitType.DamageReductionForDefenderByTerrain["mountain"] = 0;

        unitType.DiscoveryRange = 1;
        unitType.ScanningRange = 10;

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

        UnitTypeMap[unitType.Name] = unitType;
    }

    private void defineComCen()
    {
        UnitType unitType = new UnitType();
        unitType.Name = "comcen";
        unitType.LandOrSea = "both";

        // can't become dug-in infantry in swamp
        unitType.AttritionByTerrain["swamp"] = 0;

        // column
        unitType.BattleDamageFromAttacker["infantry"] = 10;
        unitType.BattleDamageFromAttacker["armor"] = 10;
        unitType.BattleDamageFromAttacker["tank"] = 10;
        unitType.BattleDamageFromAttacker["dug-in-infantry"] = 10;
        unitType.BattleDamageFromAttacker["transport-infantry"] = 10;
        unitType.BattleDamageFromAttacker["transport-armor"] = 10;
        unitType.BattleDamageFromAttacker["transport-tank"] = 10;
        unitType.BattleDamageFromAttacker["submarine"] = 10;
        unitType.BattleDamageFromAttacker["sub"] = 10;
        unitType.BattleDamageFromAttacker["battleship"] = 10;
        unitType.BattleDamageFromAttacker["carrier"] = 10;
        unitType.BattleDamageFromAttacker["spy"] = 0;
        unitType.BattleDamageFromAttacker["com"] = 10;
        unitType.BattleDamageFromAttacker["ComCen"] = 10;
        unitType.BattleDamageFromAttacker["CommandCenter"] = 10;

        // row
        unitType.BattleDamageToDefender["infantry"] = 20;
        unitType.BattleDamageToDefender["armor"] = 20;
        unitType.BattleDamageToDefender["tank"] = 20;
        unitType.BattleDamageToDefender["dug-in-infantry"] = 20;
        unitType.BattleDamageToDefender["transport-infantry"] = 20;
        unitType.BattleDamageToDefender["transport-armor"] = 20;
        unitType.BattleDamageToDefender["transport-tank"] = 20;
        unitType.BattleDamageToDefender["submarine"] = 20;
        unitType.BattleDamageToDefender["sub"] = 20;
        unitType.BattleDamageToDefender["battleship"] = 20;
        unitType.BattleDamageToDefender["carrier"] = 20;
        unitType.BattleDamageToDefender["spy"] = 34;
        unitType.BattleDamageToDefender["com"] = 10;
        unitType.BattleDamageToDefender["ComCen"] = 10;
        unitType.BattleDamageToDefender["CommandCenter"] = 10;

        unitType.NormalStepsAddedPerRound = 20;
        unitType.BlitzStepsAddedPerRound = 28;
        unitType.SneakStepsAddedPerRound = 10;

        unitType.Cost = 0;  // cannot be purchased

        unitType.DamageReductionForDefenderByTerrain["burb"] = 0;
        unitType.DamageReductionForDefenderByTerrain["mountain"] = 0;

        unitType.DiscoveryRange = 8;
        unitType.ScanningRange = 6;

        // column
        unitType.FiringRangeFromAttacker["infantry"] = 2;
        unitType.FiringRangeFromAttacker["armor"] = 2;
        unitType.FiringRangeFromAttacker["tank"] = 2;
        unitType.FiringRangeFromAttacker["dug-in-infantry"] = 2;
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
        unitType.FiringRangeFromAttacker["CommandCenter"] = 2;

        // row
        unitType.FiringRangeToDefender["infantry"] = 2;
        unitType.FiringRangeToDefender["armor"] = 2;
        unitType.FiringRangeToDefender["tank"] = 2;
        unitType.FiringRangeToDefender["dug-in-infantry"] = 2;
        unitType.FiringRangeToDefender["transport-infantry"] = 2;
        unitType.FiringRangeToDefender["transport-armor"] = 2;
        unitType.FiringRangeToDefender["transport-tank"] = 2;
        unitType.FiringRangeToDefender["submarine"] = 2;
        unitType.FiringRangeToDefender["sub"] = 2;
        unitType.FiringRangeToDefender["battleship"] = 3;
        unitType.FiringRangeToDefender["carrier"] = 4;
        unitType.FiringRangeToDefender["spy"] = 2;
        unitType.FiringRangeToDefender["com"] = 2;
        unitType.FiringRangeToDefender["ComCen"] = 2;
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

        UnitTypeMap[unitType.Name] = unitType;
        UnitTypeMap["com"] = unitType;
        UnitTypeMap["comcen"] = unitType;
        UnitTypeMap["ComCen"] = unitType;
        UnitTypeMap["CommandCenter"] = unitType;
    }

    private void definePlane()
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
        unitType.BattleDamageToDefender["CommandCenter"] = 0;

        unitType.NormalStepsAddedPerRound = 0;
        unitType.BlitzStepsAddedPerRound = 0;
        unitType.SneakStepsAddedPerRound = 0;

        unitType.Cost = 35;

        unitType.DamageReductionForDefenderByTerrain["burb"] = 0;
        unitType.DamageReductionForDefenderByTerrain["mountain"] = 0;

        unitType.DiscoveryRange = 0;
        unitType.ScanningRange = 0;

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

        UnitTypeMap[unitType.Name] = unitType;
    }
}