namespace GlobalConquest;
public class Burbs
{
    public Dictionary<string, int> IncomeMap { get; set; } = new Dictionary<string, int>();
    public Dictionary<string, Burb> NameToBurb { get; set; } = new Dictionary<string, Burb>();
    public Dictionary<string, Burb> HexXyToBurb { get; set; } = new Dictionary<string, Burb>();
    HashSet<string> cities = new HashSet<string>
    {
        "New York", "Philadelphia", "Los Angelos", "San Francisco", "San Jose", "Oakland", "Columbus", "Richmond", "Hartford", "Boston", "Atlanta", "Tampa", "Miami", "Pittsburg", "Austin", "Dallas", "St Louis"
    };
    HashSet<string> towns = new HashSet<string>
    {
        "Watsonville", "Salinas", "Monterey", "Warminster", "Mechanicsville", "Cheltenham", "Tappahannock", "Winchester", "Willow Grove", "Charleston", "State College", "Altoona", "Gilroy", "Morgan Hill", "Holister", "Castorville", "Monterey", "Carmel"
    };
    HashSet<string> villages = new HashSet<string>
    {
        "Prunedale", "Stephens Church", "Aylett", "Berryville", "Westerville", "Dublin", "Glenside", "Hartsville", "Mineola", "Arlington", "Chapel Hill", "Pacific Grove", "Spreckels"
    };

    public Burbs()
    {
        IncomeMap["village"] = 3;
        IncomeMap["town"] = 5;
        IncomeMap["city"] = 8;
        IncomeMap["metro"] = 10;
        IncomeMap["capital"] = 10;
        IncomeMap["suburb"] = 0;
        IncomeMap["dock"] = 0;
    }

    public Burb addBurb(Random random, string type, Map map, MapHex mapHex)
    {
        HashSet<string> names = new HashSet<string>();
        if ("village".Equals(type))
            names = villages;
        if ("town".Equals(type))
            names = towns;
        if ("city".Equals(type))
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
        burb.Name = name;
        burb.Type = type;
        burb.Color = color;
        burb.OwnerColor = ownerColor;
        mapHex.Burb = burb;
        HexXyToBurb[mapHex.X + "," + mapHex.Y] = burb;
        NameToBurb[name] = burb;
        if ("metro".Equals(type))
        {
            map.MetroLocations[color] = mapHex;
            mapHex.makeVisibleToAll();
        }
        if ("metro".Equals(type) || "capital".Equals(type) || "city".Equals(type))
        {
            Dictionary<string, MapHex> surroundingHexes = map.getSurroundingHexes(mapHex);
            List<string> directions = ["northWest", "northEast", "southWest", "southEast"];
            foreach (string direction in directions)
            {
                if (surroundingHexes.ContainsKey(direction))
                {
                    MapHex suburbHex = surroundingHexes[direction];
                    Burb suburb = new Burb();
                    if ("sea".Equals(suburbHex.Terrain) || "ocean".Equals(suburbHex.Terrain) || "swamp".Equals(suburbHex.Terrain) || "marsh".Equals(suburbHex.Terrain))
                    {
                        suburb.Type = "dock";
                    }
                    else
                    {
                        suburb.Type = "suburb";
                    }                    
                    suburbHex.Burb = suburb;
                }                
            }            
        }
        return burb;
    }
}