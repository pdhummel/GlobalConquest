namespace GlobalConquest.Units;
using static UnitTypeConstants;

public class SpyUnitType : UnitType
{

// Spies are former politicians who, not being trusted with live ammunition, 
// have been recruited for the dirty work needed to be done in Conquest. These units 
// cannot carry weapons; the spy is used to gain information about the location of the 
// enemy's units. The best feature about spies is that only an enemy spy can "see" 
// another spy: to all other units the spy is invisible.
// Typical to the politician's profession, the spy can also "steal." Unlike today's 
// politicians, however, spies mostly steal information. If a spy ends its turn in an 
// enemy burb, all enemy units within 25 spaces will be visible and the status of units 
// being made in the enemy burb will be accessible. If a spy ends its turn next to an 
// enemy Comcen, info on all enemy units and burbs is available. In both cases this 
// information is temporary and will vanish after the orders phase ends. Finally, 
// spies ending their turn in an enemy burb sabotage that burb's production (for 
// specifics see the "Economics" section).

// Spies have a unique set of characteristics. They can't be spotted except by 
// other spies, and they can't be destroyed unless they are spotted. Therefore, it takes 
// a destroying unit (such as a Comcen or infantry) working with an allied spy to track 
// down and destroy an enemy spy. (That, or the player can obtain a court order, 
// which is extraordinarily difficult.) Also, spies specialize in personnel, not discovery: 
// they "see" units at a range of 10, but only "discover" the area of the world that 
// they physically pass over.

// Spies and Production Sabotage
// It is perhaps in this area where Conquest most closely resembles life as we 
// know it: the spy (former politician) will sabotage the income of a burb if it ends its 
// turn in an enemy-held burb. It will add eight bucks per turn to the cost of the unit 
// currently under production. For example, if, after supporting existing units, a burb 
// is earning ten bucks/turn toward production of new units, the spy will in effect 
// "cancel" eight of those bucks, and the burb will end up only two bucks closer to 
// producing the unit. Multiple spies bring about results which closely simulate long 
// court proceedings: they are devastatingly cumulative. A burb with enough spies 
// in it may never be able to produce new units.

// TODO: If a spy ends its turn in an enemy burb, 
// all enemy units within 25 spaces will be visible and the status of units 
// being made in the enemy burb will be accessible.
// Information is temporary and will vanish after the orders phase ends. 

// TODO: If a spy ends its turn next to an enemy Comcen, info on all enemy units and burbs is available. 
// Information is temporary and will vanish after the orders phase ends. 

// TODO: the spy will sabotage the income of a burb if it ends its 
// turn in an enemy-held burb. 
// It will add eight bucks per turn to the cost of the unit currently under production.

    public SpyUnitType()
    {

    }

    public UnitType defineSpy()
    {
        UnitType unitType = new UnitType();
        unitType.Name = SPY;
        unitType.LandOrSea = "both";

        // can't become dug-in infantry in swamp
        unitType.AttritionByTerrain["swamp"] = 0;

        // column
        unitType.BattleDamageFromAttacker[INFANTRY] = 34;
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
        unitType.BattleDamageFromAttacker[SPY] = 0;
        unitType.BattleDamageFromAttacker["decoy-comcen"] = 0;
        unitType.BattleDamageFromAttacker["com"] = 34;
        unitType.BattleDamageFromAttacker["ComCen"] = 34;
        unitType.BattleDamageFromAttacker["comcen"] = 34;
        unitType.BattleDamageFromAttacker["CommandCenter"] = 34;

        // row
        unitType.BattleDamageToDefender[INFANTRY] = 0;
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
        unitType.BattleDamageToDefender[SPY] = 0;
        unitType.BattleDamageToDefender["decoy-comcen"] = 0;
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

        unitType.DiscoveryRange = 0;
        unitType.ScanningRange = 10;
        unitType.PointsPerHit = 12;

        // column
        unitType.FiringRangeFromAttacker[INFANTRY] = 2;
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
        unitType.FiringRangeFromAttacker[SPY] = 0;
        unitType.FiringRangeFromAttacker["decoy-comcen"] = 0;
        unitType.FiringRangeFromAttacker["com"] = 2;
        unitType.FiringRangeFromAttacker["ComCen"] = 2;
        unitType.FiringRangeFromAttacker["comcen"] = 2;
        unitType.FiringRangeFromAttacker["CommandCenter"] = 2;

        // row
        unitType.FiringRangeToDefender[INFANTRY] = 0;
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