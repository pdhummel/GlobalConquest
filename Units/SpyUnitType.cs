namespace GlobalConquest.Units;
using static UnitConstants;
using static GameConstants;
using static GlobalConquest.Burbs;

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

        unitType.Cost = 85;

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