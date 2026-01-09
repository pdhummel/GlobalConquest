using static GameConstants;
namespace GlobalConquest;
public class Burbs
{
    public Dictionary<string, int> IncomeMap { get; set; } = new Dictionary<string, int>();
    public Dictionary<string, int> PointMap  { get; set; } = new Dictionary<string, int>();
    public Dictionary<string, Burb> NameToBurb { get; set; } = new Dictionary<string, Burb>();
    public Dictionary<string, Burb> HexXyToBurb { get; set; } = new Dictionary<string, Burb>();
    HashSet<string> cities = new HashSet<string>
    {
        "New York", "Philadelphia", "Los Angeles", "San Francisco", "San Jose", "Oakland", "Columbus", 
        "Richmond", "Hartford", "Boston", "Atlanta", "Tampa", "Miami", "Pittsburg", "Austin", "Dallas", 
        "St Louis"
    };
    HashSet<string> towns = new HashSet<string>
    {
        "Watsonville", "Salinas", "Monterey", "Warminster", "Mechanicsville", "Cheltenham", "Tappahannock", 
        "Winchester", "Willow Grove", "Charleston", "State College", "Altoona", "Gilroy", "Morgan Hill", 
        "Holister", "Castorville", "Monterey", "Carmel"
    };
    HashSet<string> villages = new HashSet<string>
    {
        "Prunedale", "Stephens Church", "Aylett", "Berryville", "Westerville", "Dublin", "Glenside", "Hartsville", 
        "Mineola", "Arlington", "Chapel Hill", "Pacific Grove", "Spreckels", "Lambertville"
    };

    public Burbs()
    {
        IncomeMap[BURB_VILLAGE] = 3;
        IncomeMap[BURB_TOWN] = 5;
        IncomeMap[BURB_CITY] = 8;
        IncomeMap[BURB_METROPLEX] = 10;
        IncomeMap[BURB_CAPITAL] = 10;
        IncomeMap[BURB_SUBURB] = 0;
        IncomeMap[BURB_DOCK] = 0;

        PointMap[BURB_VILLAGE] = 20;
        PointMap[BURB_TOWN] = 30;
        PointMap[BURB_CITY] = 40;
        PointMap[BURB_METROPLEX] = 50;
        PointMap[BURB_CAPITAL] = 250;  // Combined Scoring = 250; Capital Scoring == 2500
        PointMap[BURB_SUBURB] = 0;
        PointMap[BURB_DOCK] = 0;
    }

    public Burb addBurb(Random random, string type, Map map, MapHex mapHex)
    {
        HashSet<string> names = new HashSet<string>();
        if (BURB_VILLAGE.Equals(type))
            names = villages;
        if (BURB_TOWN.Equals(type))
            names = towns;
        if (BURB_CITY.Equals(type))
            names = cities;
        int randomNumber = random.Next(0, 1000);
        int nameIndex = random.Next(0, names.Count);
        string name = type + "-" + randomNumber;
        if (nameIndex < names.Count)
        {
            name = names.ToList()[nameIndex];
            names.Remove(name);            
        }
        return addBurb(name, type, map, mapHex, "grey");
    }

    public Burb addBurb(string name, string type, Map map, MapHex mapHex)
    {
        return addBurb(name, type, map, mapHex, "grey");
    }

    public Burb addBurb(string name, string type, Map map, MapHex mapHex, string color)
    {
        return addBurb(name, type, map, mapHex, color, color);
    }

    public Burb addBurb(string name, string type, Map map, MapHex mapHex, string color, string ownerColor)
    {
        Burb burb = new Burb();
        burb.X = mapHex.X;
        burb.Y = mapHex.Y;
        burb.Name = name;
        burb.Type = type;
        burb.Color = color;
        burb.OwnerColor = ownerColor;
        mapHex.Burb = burb;
        HexXyToBurb[mapHex.X + "," + mapHex.Y] = burb;
        NameToBurb[name] = burb;
        if (BURB_METROPLEX.Equals(type))
        {
            map.MetroLocations[color] = mapHex;
            mapHex.makeVisibleToAll();
        }

        List<string> directions = [];
        Dictionary<string, MapHex> surroundingHexes = map.getSurroundingHexes(mapHex);
        if (BURB_METROPLEX.Equals(type) || BURB_CAPITAL.Equals(type) || BURB_CITY.Equals(type))
            directions = ["north", "south", "northWest", "northEast", "southWest", "southEast"];
            //directions = ["northWest", "northEast", "southWest", "southEast"];
        else if (BURB_TOWN.Equals(type))
            directions = ["north", "south"];
            //directions = [];

        if (BURB_METROPLEX.Equals(type))
            mapHex.Terrain = TERRAIN_SWAMP;
        if (BURB_METROPLEX.Equals(type) || BURB_CAPITAL.Equals(type) || BURB_CITY.Equals(type) || BURB_TOWN.Equals(type))
        {
            foreach (string direction in directions)
            {
                if (surroundingHexes.ContainsKey(direction))
                {
                    MapHex suburbHex = surroundingHexes[direction];
                    Burb suburb = new Burb();
                    if (TERRAIN_SEA.Equals(suburbHex.Terrain) || "ocean".Equals(suburbHex.Terrain) || TERRAIN_SWAMP.Equals(suburbHex.Terrain) || "marsh".Equals(suburbHex.Terrain))
                    {
                        suburb.Type = BURB_DOCK;
                    }
                    else
                    {
                        suburb.Type = BURB_SUBURB;
                    }
                    suburb.ParentBurbName = mapHex.Burb.Name;
                    suburb.X = suburbHex.X;
                    suburb.Y = suburbHex.Y;
                    suburb.DirectionFromParent = direction;
                    suburb.OwnerColor = burb.OwnerColor;
                    suburbHex.Burb = suburb;
                }                
            }            
        }

        return burb;
    }

}