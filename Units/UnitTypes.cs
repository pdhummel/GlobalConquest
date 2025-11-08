using GlobalConquest.Units;

public class UnitTypes
{
    public Dictionary<string, UnitType> UnitTypeMap { get; set; } = new Dictionary<string, UnitType>();
    public UnitTypes()
    {
        defineUnitTypes();
    }
    public void defineUnitTypes()
    {
        InfantryUnitType infantryUnitType = new InfantryUnitType();
        UnitType unitTypeInfantry = infantryUnitType.defineInfantry();
        UnitTypeMap[unitTypeInfantry.Name] = unitTypeInfantry;

        UnitType unitTypeTransportInfantry = infantryUnitType.defineTransportInfantry();
        UnitTypeMap[unitTypeTransportInfantry.Name] = unitTypeTransportInfantry;

        UnitType unitTypeDugInInfantry = infantryUnitType.defineDugInInfantry();
        UnitTypeMap[unitTypeDugInInfantry.Name] = unitTypeDugInInfantry;


        ArmorUnitType armorUnitType = new ArmorUnitType();
        UnitType unitTypeArmor = armorUnitType.defineArmor();
        UnitTypeMap[unitTypeArmor.Name] = unitTypeArmor;
        UnitTypeMap["armor"] = unitTypeArmor;
        UnitTypeMap["tank"] = unitTypeArmor;

        UnitType unitTypeTransportArmor = armorUnitType.defineTransportArmor();
        UnitTypeMap[unitTypeTransportArmor.Name] = unitTypeTransportArmor;
        UnitTypeMap["transport-armor"] = unitTypeTransportArmor;
        UnitTypeMap["transport-tank"] = unitTypeTransportArmor;


        SubUnitType subUnitType = new SubUnitType();
        UnitType unitTypeSub = subUnitType.defineSub();
        UnitTypeMap[unitTypeSub.Name] = unitTypeSub;
        UnitTypeMap["sub"] = unitTypeSub;
        UnitTypeMap["submarine"] = unitTypeSub;

        BattleshipUnitType battleshipUnitType = new BattleshipUnitType();
        UnitType unitTypeBattleship = battleshipUnitType.defineBattleship();
        UnitTypeMap[unitTypeBattleship.Name] = unitTypeBattleship;

        CarrierUnitType carrierUnitType = new CarrierUnitType();
        UnitType unitTypeCarrier = carrierUnitType.defineCarrier();
        UnitTypeMap[unitTypeCarrier.Name] = unitTypeCarrier;

        SpyUnitType spyUnitType = new SpyUnitType();
        UnitType unitTypeSpy = spyUnitType.defineSpy();
        UnitTypeMap[unitTypeSpy.Name] = unitTypeSpy;

        ComCenUnitType comCenUnitType = new ComCenUnitType();
        UnitType unitTypeComCen = comCenUnitType.defineComCen();
        UnitTypeMap[unitTypeComCen.Name] = unitTypeComCen;
        UnitTypeMap["com"] = unitTypeComCen;
        UnitTypeMap["comcen"] = unitTypeComCen;
        UnitTypeMap["ComCen"] = unitTypeComCen;
        UnitTypeMap["CommandCenter"] = unitTypeComCen;

        PlaneUnitType planeUnitType = new PlaneUnitType();
        UnitType unitTypePlane = planeUnitType.definePlane();
        UnitTypeMap[unitTypePlane.Name] = unitTypePlane;

    }

}



