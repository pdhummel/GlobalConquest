public class UnitType
{
    // infantry, tank, plane, ComCen, carrier, battleship, spy
    public string Name { get; set; }
    public string LandOrSea { get; set; }

    // Global Conquest Manual - Technical Notes - p79-83
    public int Cost { get; set; }

    // Subs have special scanning rules. They can't be spotted by planes, spies or any other unit until they attack. 
    // However, once a sub is spotted it stays "seen" at the normal range of the "seeing" unit 
    // (e.g., 6 for carriers and Comcens, 5 for battleships) but for a shorter period of time 
    // (only 2 rounds, which is considerably shorter than the 8 rounds for all other units). 
    // Sub range is reduced to 3 if target not moving. Subs can only be spotted 
    // at a range of 1 if they are stationary or if the scanning unit is moving regardless of unit's normal range.
    public int ScanningRange { get; set; }

    public int DiscoveryRange { get; set; }


    // Units can accumulate steps as they are moving (up to a maximum of 100).
    // Infantry and armor when on land may move only once per round 
    // while sea units (including infantry and armor transports) may move as 
    // many times as their accumulated steps will allow when they are 
    // outside the range of enemy units (usually twice per round). 
    // Spies and Comcens move on land like they do at sea.
    // Infantry units lose steps equal to the damage done when either attacking or defending. 
    // Armor lose steps equal to 1/2 the damage. 
    // This effect can reduce the steps to a deficit of -25 (when steps are negative the unit is "pinned.0)
    // When not moving, a land unit's accumulation of steps returns to 0 
    // while a ship's value returns to its steps available per round 
    // (thus ships are quick to make an initial move while land units are not).
    public int NormalStepsAddedPerRound { get; set; }
    // Blitz movement causes a unit to incur damage at a 2% rate (?two points of damage?) per round to the unit.
    //  Units will cease blitz mode when their strength reaches 20% or they reach their destination. 
    public int BlitzStepsAddedPerRound { get; set; }
    //  Sneak movement requires the scanner to be three times closer than normal to spot the sneaking unit.
    public int SneakStepsAddedPerRound { get; set; }

    public Dictionary<string, float> DamageReductionForDefenderByTerrain = new Dictionary<string, float>();
    public Dictionary<string, int> BattleDamageToDefender = new Dictionary<string, int>();
    public Dictionary<string, int> BattleDamageFromAttacker = new Dictionary<string, int>();
    public Dictionary<string, int> FiringRangeToDefender = new Dictionary<string, int>();
    public Dictionary<string, int> FiringRangeFromAttacker = new Dictionary<string, int>();

    // Move Steps Used: A certain number of "steps" are used by a unit to move out of each type of terrain.
    public Dictionary<string, int> StepsUsedByTerrain = new Dictionary<string, int>();

    // Attrition causes a unit to suffer damage in certain terrains but cannot lower strength below 20%.
    // Armor, when moving into forests, sustains 1% attrition per terrain square moved into. 
    // In the same way, mountains cause a 2% attrition rate.
    public Dictionary<string, int> AttritionByTerrain = new Dictionary<string, int>();

    // facility = dock or resource (oil, minerals) or burb (village, town, city, metro, capital)
    // docks repair land or sea units.
    // burbs and resources only repair land units.
    //  The rate units repair is based on the site they are on 
    // (2% for resources, 4% for villages, 6% for towns, 8% for cities and 10% for metros and the capital). 
    // This amount is added to the unit's strength every other round.
    public Dictionary<string, int> RepairRateByFacility = new Dictionary<string, int>();

    public Dictionary<string, string> CanDigInByTerrainYorN = new Dictionary<string, string>();

    public UnitType()
    {

    }

}