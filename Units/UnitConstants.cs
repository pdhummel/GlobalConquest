using System.Data;
using GlobalConquest.Units;

public static class UnitConstants
{
    public const string INFANTRY = "infantry";
    public const string TRANSPORT_INFANTRY = "transport-infantry";
    public const string DUG_IN_INFANTRY = "dug-in-infantry";
    public const string ARMOR = "tank";
    public const string TRANSPORT_ARMOR = "transport-tank";
    public const string SPY = "spy";
    public const string COMMAND_CENTER = "comcen";
    public const string DECOY_COMMAND_CENTER = "decoy-comcen";
    public const string BATTLESHIP = "battleship";
    public const string AIRCRAFT_CARRIER = "carrier";
    public const string SUBMARINE = "sub";
    public const string AIRPLANE = "plane";
    public const string TRANSPORT = "transport";

    public const string UNIT_PALETTE_NAME_FULL = "Full-set";
    public const string UNIT_PALETTE_NAME_COMCEN = "ComCen";
    public const string UNIT_PALETTE_NAME_WW2 = "WW2";
    public const string UNIT_PALETTE_NAME_BASIC = "Basic-set";
    public const string UNIT_PALETTE_NAME_CHQ1918 = "CHQ 1918";
    public const string UNIT_PALETTE_NAME_INFANTRY = "Infantry";

    public static readonly List<string> UNIT_PALETTE_FULL = [SPY, COMMAND_CENTER, AIRPLANE, AIRCRAFT_CARRIER, BATTLESHIP, SUBMARINE, ARMOR, INFANTRY];
    public static readonly List<string> UNIT_PALETTE_COMCEN = [COMMAND_CENTER, AIRPLANE, AIRCRAFT_CARRIER, BATTLESHIP, SUBMARINE, ARMOR, INFANTRY];
    public static readonly List<string> UNIT_PALETTE_WW2 = [AIRPLANE, AIRCRAFT_CARRIER, BATTLESHIP, SUBMARINE, ARMOR, INFANTRY];
    public static readonly List<string> UNIT_PALETTE_BASIC = [BATTLESHIP, SUBMARINE, ARMOR, INFANTRY];
    public static readonly List<string> UNIT_PALETTE_CHQ1918 = [BATTLESHIP, SUBMARINE, INFANTRY];
    public static readonly List<string> UNIT_PALETTE_INFANTRY = [INFANTRY];

    public static readonly Dictionary<string, List<string>> UNIT_PALETTES = new  Dictionary<string, List<string>>()
    {
        [UNIT_PALETTE_NAME_FULL] = UNIT_PALETTE_FULL,
        [UNIT_PALETTE_NAME_COMCEN] = UNIT_PALETTE_COMCEN,
        [UNIT_PALETTE_NAME_WW2] = UNIT_PALETTE_WW2,
        [UNIT_PALETTE_NAME_BASIC] = UNIT_PALETTE_BASIC,
        [UNIT_PALETTE_NAME_CHQ1918] = UNIT_PALETTE_CHQ1918,
        [UNIT_PALETTE_NAME_INFANTRY] = UNIT_PALETTE_INFANTRY
    };
}




