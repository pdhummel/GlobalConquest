using static GameConstants;
namespace GlobalConquest;

public class Burb
{
    public string Type { get; set; } // village, town, city, capital, metro, suburb, dock
    public string Name { get; set; }
    public string? Color { get; set; } = NATIVE_COLOR;
    public string? OwnerColor { get; set; } = NATIVE_COLOR;
    public string? ParentBurbName { get; set; }
    public string? DirectionFromParent {get;set;}
    public int X { get; set; }
    public int Y { get; set; }
    public int Money {get; set;}

    public Burb()
    {

    }

    public HashSet<MapHex> getHexesInBurb(Map map)
    {
        HashSet<MapHex> hexesInBurb = new HashSet<MapHex>();
        MapHex centerHex = map.Hexes[Y, X];
        hexesInBurb.Add(centerHex);
        List<MapHex> burbHexes = map.getSurroundingHexesList(centerHex);
        foreach (MapHex burbHex in burbHexes)
        {
            hexesInBurb.Add(burbHex);
        }
        return hexesInBurb;
    }

    public bool IsBurbCenter()
    {
        if (BURB_VILLAGE.Equals(Type) || BURB_TOWN.Equals(Type) || BURB_CITY.Equals(Type) || BURB_METROPLEX.Equals(Type) || BURB_CAPITAL.Equals(Type))
            return true;
        return false;
    }
}