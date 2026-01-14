using System.Text.Json.Serialization;
using static GameConstants;
using static GlobalConquest.Burbs;
namespace GlobalConquest;

public class Burb
{
    [JsonPropertyName("T")]
    public string Type { get; set; } // village, town, city, capital, metro, suburb, dock
    [JsonPropertyName("N")]
    public string Name { get; set; }
    [JsonPropertyName("C")]
    public string? Color { get; set; } = NATIVE_COLOR;
    [JsonPropertyName("OC")]
    public string? OwnerColor { get; set; } = NATIVE_COLOR;
    [JsonPropertyName("PBN")]
    public string? ParentBurbName { get; set; }
    [JsonPropertyName("DFP")]
    public string? DirectionFromParent {get;set;}
    public int X { get; set; }
    public int Y { get; set; }
    [JsonPropertyName("M")]
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