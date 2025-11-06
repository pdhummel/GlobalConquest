namespace GlobalConquest.Units;

public class PlaneUnitType : UnitType
{

    public PlaneUnitType()
    {

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

        unitType.DiscoveryRange = 0;
        unitType.ScanningRange = 0;
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
