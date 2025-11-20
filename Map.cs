using System.Text.Json;
using GlobalConquest.Actions;
using GlobalConquest.Units;
namespace GlobalConquest;

public class Map
{
    public MapHex[,] Hexes { get; set; }
    Dictionary<string, Node> allNodesGraph = new Dictionary<string, Node>();
    Dictionary<string, Node> seaNodesGraph = new Dictionary<string, Node>();
    Dictionary<string, Node> landNodesGraph = new Dictionary<string, Node>();
    public int Y { get; set; }
    public int X { get; set; }
    public string VisibilityMode { get; set; }
    public Dictionary<string, MapHex> MetroLocations { get; set; } = new Dictionary<string, MapHex>();
    public Dictionary<string, MapHex> LeftMetro { get; set; } = new Dictionary<string, MapHex>();
    public Dictionary<string, MapHex> RightMetro { get; set; } = new Dictionary<string, MapHex>();
    public Dictionary<string, MapHex> DiagonalMetro { get; set; } = new Dictionary<string, MapHex>();
    public Dictionary<string, Unit> UnitIdToUnit { get; set; } = new Dictionary<string, Unit>();
    public Dictionary<string, HashSet<string>> ColorToUnitIds { get; set; } = new Dictionary<string, HashSet<string>>();
    public bool IsMapReady { get; set; } = false;

    public Map()
    {
    }

    public Map(int y, int x)
    {
        Y = y;
        X = x;
        Hexes = generateMap(y, x);
        buildNodesForShortestPath();
        IsMapReady = true;
        List<string> colors = ["amber", "ocher", "magenta", "cyan", "grey"];
        foreach (string color in colors)
        {
            ColorToUnitIds[color] = new HashSet<string>();
        }
    }

    public void addBurbs(Burbs burbs, int desiredBurbCount)
    {
        addFixedBurbs(burbs);
        Console.WriteLine("addBurbs(): desiredBurbCount=" + desiredBurbCount);
        Random random = new Random();
        int numberOfBurbs = 0;
        int tries = 0;
        while (numberOfBurbs < desiredBurbCount && tries < 1000)
        {
            int x = random.Next(0, X);
            int y = random.Next(0, Y);
            MapHex mapHex = Hexes[y, x];
            bool burbConflictFound = false;
            if (mapHex.Burb == null && mapHex.Terrain.Equals("grass"))
            {
                HashSet<MapHex> neighborHexes = getMapHexesInRange(mapHex, 5);
                foreach (MapHex hex in neighborHexes)
                {
                    if (hex.Burb != null)
                    {
                        burbConflictFound = true;
                        break;
                    }
                }
            }
            else
            {
                burbConflictFound = true;
            }
            if (!burbConflictFound)
            {
                string type = "village";
                int typeRange = random.Next(0, 100);
                if (typeRange < 50)      // 50%
                    type = "village";
                else if (typeRange < 80) // 30%
                    type = "town";
                else
                    type = "city";       // 20%
                Burb burb = burbs.addBurb(random, type, this, mapHex);
                Console.WriteLine("addBurbs(): added burb " + burb.Name + " at " + mapHex.X + "," + mapHex.Y);
                numberOfBurbs += 1;
            }
            tries += 1;
        }
        Console.WriteLine("addBurbs(): numberOfBurbs=" + numberOfBurbs);
    }

    public void addFixedBurbs(Burbs burbs)
    {
        bool isEven = false;
        if (X % 2 == 0)
            isEven = true;
        burbs.addBurb("Amber Array", "metro", this, Hexes[0, 1], "amber");
        burbs.addBurb("Magenta Mob", "metro", this, Hexes[Y - 2, 1], "magenta");
        if (isEven)
        {
            burbs.addBurb("Ocher Order", "metro", this, Hexes[1, X - 2], "ocher");
            burbs.addBurb("Cyan Circle", "metro", this, Hexes[Y - 1, X - 2], "cyan");
        }
        else
        {
            burbs.addBurb("Ocher Order", "metro", this, Hexes[0, X - 2], "ocher");
            burbs.addBurb("Cyan Circle", "metro", this, Hexes[Y - 2, X - 2], "cyan");
        }
        burbs.addBurb("Washington", "capital", this, Hexes[Y / 2, X / 2]);
        LeftMetro["amber"] = MetroLocations["ocher"];
        RightMetro["amber"] = MetroLocations["magenta"];
        DiagonalMetro["amber"] = MetroLocations["cyan"];
        LeftMetro["ocher"] = MetroLocations["cyan"];
        RightMetro["ocher"] = MetroLocations["ocher"];
        DiagonalMetro["ocher"] = MetroLocations["magenta"];
        LeftMetro["cyan"] = MetroLocations["magenta"];
        RightMetro["cyan"] = MetroLocations["ocher"];
        DiagonalMetro["cyan"] = MetroLocations["amber"];
        LeftMetro["magenta"] = MetroLocations["amber"];
        RightMetro["magenta"] = MetroLocations["cyan"];
        DiagonalMetro["magenta"] = MetroLocations["ocher"];
    }

    public MapHex getCapitalHex()
    {
        return Hexes[Y / 2, X / 2];
    }
    public MapHex getMetroHex(string color)
    {
        return MetroLocations[color];
    }

    public MapHex[,] generateMap(int height, int width)
    {
        MapHex[,] hexes = new MapHex[height, width];
        long milliseconds = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        for (int liY = 0; liY < height; liY++)
        {
            for (int liX = 0; liX < width; liX++)
            {
                //int textureIndex = rnd.Next(1, 7);
                float elevationNoise = OpenSimplex2S.Noise2(milliseconds, liX, liY);
                float moistureNoise = OpenSimplex2S.Noise2(milliseconds, liX, liY);
                //float elevationNoise = OpenSimplex2.Noise2(milliseconds, liX, liY);
                //float moistureNoise = OpenSimplex2.Noise2(milliseconds, liX, liY);
                //textures[liY, liX] = idToTerrain[textureIndex].TEXTURE2D_IMAGE_TILE;
                string biome = determineBiome(elevationNoise, moistureNoise);
                elevationNoise = shapeForIsland(biome, elevationNoise, liX, liY, width, height);
                string newBiome = determineBiome(elevationNoise, moistureNoise);
                if (!newBiome.Equals(biome))
                {
                    //Console.WriteLine("changed biome from " + biome + " to " + newBiome);
                }
                MapHex mapHex = new MapHex();
                mapHex.X = liX;
                mapHex.Y = liY;
                mapHex.Terrain = newBiome;
                hexes[liY, liX] = mapHex;
            }
        }
        return hexes;
    }

    public void placeNewUnit(Unit unit, MapHex mapHex)
    {
        placeNewUnit(unit, mapHex.X, mapHex.Y);
    }

    public void placeNewUnit(Unit unit, int x, int y)
    {
        if (x >= 0 && x < X && y >= 0 && y < Y)
        {
            Hexes[y, x].setUnit(unit);
            unit.HomeBurbX = x;
            unit.HomeBurbY = y;
            unit.OriginalBurbX = x;
            unit.OriginalBurbY = y;
            unit.X = x;
            unit.Y = y;
            string id = unit.generateId();
            UnitIdToUnit[id] = unit;
            ColorToUnitIds[unit.Color].Add(id);
        }
    }

    public Unit? getUnitAtXY(int x, int y)
    {
        Unit? unit = null;
        if (x >= 0 && x < X && y >= 0 && y < Y)
        {
            unit = Hexes[y, x].getUnit();
        }
        return unit;
    }

    public void moveUnit(Unit unit, int destinationX, int destinationY)
    {
        if (unit.X != destinationX || unit.Y != destinationY)
        {
            MapHex targetMapHex = Hexes[destinationY, destinationX];
            MapHex sourceMapHex = Hexes[unit.Y, unit.X];
            if (sourceMapHex.Units.Count > 0)
            {
                targetMapHex.setUnit(unit);
                if (Hexes[unit.Y, unit.X].Units.Count > 0)
                    Hexes[unit.Y, unit.X].Units.RemoveAt(0);
            }
        }
    }


    private string determineBiome(float elevation, float moisture)
    {
        // these thresholds will need tuning to match your generator
        if (elevation < 0.1F)
        {
            return "sea";
        }
        if (elevation < 0.12F)
        {
            return "swamp";
        }

        if (elevation > 0.85F)
        {
            return "mountain";
        }

        if (elevation > 0.6F)
        {
            if (moisture < 0.02F)
            {
                //return "desert";
                return "grass";
            }
            if (moisture < 0.66F)
            {
                return "grass";
            }
            return "forest";
        }

        if (elevation > 0.3F)
        {
            //if (moisture < 0.05F)
            //{
            //    return "desert";
            //}
            if (moisture < 0.50F)
            {
                return "grass";
            }
            if (moisture < 0.83F)
            {
                return "forest";
            }
            return "forest";
        }

        //if (moisture < 0.05F)
        //{
        //    return "desert";
        //}
        if (moisture < 0.33F)
        {
            return "grass";
        }
        if (moisture < 0.66F)
        {
            return "forest";
        }
        return "forest";
    }

    // https://www.redblobgames.com/maps/terrain-from-noise/
    private float shapeForIsland(string biome, float elevation, int x, int y, int width, int height)
    {
        // nx = 2*x/width - 1 and ny = 2*y/height - 1
        // square bump: d = 1 - (1-nx²) * (1-ny²)
        // euclidian^2: d = min(1, (nx² + ny²) / sqrt(2))
        float nWidth = 0;
        if (x != 0)
            nWidth = (2.0F / width) - (1.0F / x);
        float nHeight = 0;
        if (y != 0)
            nHeight = (2.0F / height) - (1.0F / y);
        float mix = 0.5F;
        //float distance = 1.0F - ((1.0F - (nWidth * (x ^ 2))) * ((1.0F - (nHeight * (y ^ 2)))));
        // distance from center
        int xDistance = Math.Abs((width / 2) - x);
        int yDistance = Math.Abs((height / 2) - y);
        float distance = (float)Math.Sqrt((xDistance * xDistance) + (yDistance * yDistance));
        float diagonal = (float)Math.Sqrt((width * width) + (height * height)) / 2;
        // Lerp(a, b, t) is defined as a + (b — a) * t.
        // e = lerp(e, 1-d, mix)
        // float newElevation = elevation + (1.0F - distance - elevation) * mix;
        float newElevation = elevation;
        if (distance < (diagonal * .2F) &&
            (biome.Equals("sea") || biome.Equals("swamp")))
        {
            newElevation = elevation + 01.0F;
        }
        else if (distance < (diagonal * .3F) &&
            (biome.Equals("sea") || biome.Equals("swamp")))
        {
            newElevation = elevation + 0.75F;
        }
        else if ((distance > (diagonal * .8F) ||
                xDistance >= width / 2 - 1 || yDistance >= height / 2 - 1) &&
                !(biome.Equals("sea") || biome.Equals("swamp")))
        {
            //newElevation = elevation - 0.75F;
            newElevation = .09F;
            //Console.WriteLine("shapeForIsland(): diagonal=" + diagonal + ", biome=" + biome + ", elevation=" + elevation + ", x=" + x + ", y=" + y + ", xd=" + xDistance + ", yd=" + yDistance + ", distance=" + distance + ", newE=" + newElevation);
        }
        else if ((distance > (diagonal * .7F) ||
                xDistance >= width / 2 - 1 || yDistance >= height / 2 - 1) &&
                !(biome.Equals("sea") || biome.Equals("swamp")))
        {
            newElevation = elevation - 0.75F;
            //newElevation = .09F;
            //Console.WriteLine("shapeForIsland(): diagonal=" + diagonal + ", biome=" + biome + ", elevation=" + elevation + ", x=" + x + ", y=" + y + ", xd=" + xDistance + ", yd=" + yDistance + ", distance=" + distance + ", newE=" + newElevation);
        }
        else if ((xDistance >= width / 2 - 1 || yDistance >= height / 2 - 1) &&
                (!(biome.Equals("sea") || biome.Equals("swamp"))))
        {
            newElevation = .11F;
            Console.WriteLine("shapeForIsland(): diagonal=" + diagonal + ", biome=" + biome + ", elevation=" + elevation + ", x=" + x + ", y=" + y + ", xd=" + xDistance + ", yd=" + yDistance + ", distance=" + distance + ", newE=" + newElevation);

        }
        if (newElevation < 0)
        {
            newElevation = 0;
        }
        return newElevation;
    }

    public override bool Equals(object obj)
    {
        if (obj is Map other)
        {
            if (Y == other.Y && X == other.X)
            {
                for (int y = 0; y < Y; y++)
                {
                    for (int x = 0; x < X; x++)
                    {
                        if (!Hexes[y, x].Equals(other.Hexes[y, x]))
                            return false;
                    }
                }
                return true;
            }
        }
        //Console.WriteLine("Map.Equals(): false");
        return false;
    }


    public override int GetHashCode()
    {
        // Combine hash codes of relevant properties
        int hashCode = 0;
        for (int y = 0; y < Y; y++)
        {
            for (int x = 0; x < X; x++)
            {
                hashCode = HashCode.Combine(hashCode, Hexes[y, x].GetHashCode());
            }
        }
        return hashCode;
    }


    public Dictionary<string, MapHex> getSurroundingHexes(MapHex mapHex)
    {
        Dictionary<string, MapHex> hexes = new Dictionary<string, MapHex>();
        // is nw/ne or sw/se on the same row?
        bool northEastAndWestSameRow = true;
        // 0,0->S; 1,0->N; 2,1->S; 3,1->N
        if (mapHex.X % 2 == 0)
        {
            northEastAndWestSameRow = false;
        }
        if (mapHex.Y - 1 >= 0)
        {
            MapHex northHex = Hexes[mapHex.Y - 1, mapHex.X];
            hexes["north"] = northHex;
        }
        if (mapHex.Y + 1 < Y)
        {
            MapHex southHex = Hexes[mapHex.Y + 1, mapHex.X];
            hexes["south"] = southHex;
        }
        MapHex northEastHex;
        MapHex southEastHex;
        MapHex southWestHex;
        MapHex northWestHex;
        if (northEastAndWestSameRow)
        {
            if (mapHex.X + 1 < X)
            {
                northEastHex = Hexes[mapHex.Y, mapHex.X + 1];
                hexes["northEast"] = northEastHex;
            }
            if (mapHex.Y + 1 < Y && mapHex.X + 1 < X)
            {
                southEastHex = Hexes[mapHex.Y + 1, mapHex.X + 1];
                hexes["southEast"] = southEastHex;
            }
            if (mapHex.Y + 1 < Y && mapHex.X - 1 >= 0)
            {
                southWestHex = Hexes[mapHex.Y + 1, mapHex.X - 1];
                hexes["southWest"] = southWestHex;
            }
            if (mapHex.X - 1 >= 0)
            {
                northWestHex = Hexes[mapHex.Y, mapHex.X - 1];
                hexes["northWest"] = northWestHex;
            }
        }
        else
        {
            if (mapHex.Y - 1 >= 0 && mapHex.X + 1 < X)
            {
                northEastHex = Hexes[mapHex.Y - 1, mapHex.X + 1];
                hexes["northEast"] = northEastHex;
            }
            if (mapHex.X + 1 < X)
            {
                southEastHex = Hexes[mapHex.Y, mapHex.X + 1];
                hexes["southEast"] = southEastHex;
            }
            if (mapHex.X - 1 >= 0)
            {
                southWestHex = Hexes[mapHex.Y, mapHex.X - 1];
                hexes["southWest"] = southWestHex;
            }
            if (mapHex.Y - 1 >= 0 && mapHex.X - 1 >= 0)
            {
                northWestHex = Hexes[mapHex.Y - 1, mapHex.X - 1];
                hexes["northWest"] = northWestHex;
            }
        }
        if (mapHex.X - 1 >= 0)
        {
            MapHex westHex = Hexes[mapHex.Y, mapHex.X - 1];
            hexes["west"] = westHex;
        }
        if (mapHex.X + 1 < X)
        {
            MapHex eastHex = Hexes[mapHex.Y, mapHex.X + 1];
            hexes["east"] = eastHex;
        }
        return hexes;
    }

    public List<MapHex> getSurroundingHexesList(MapHex mapHex)
    {
        List<MapHex> hexes = new List<MapHex>();
        Dictionary<string, MapHex> hexesMap = getSurroundingHexes(mapHex);
        return getSurroundingHexesList(hexesMap);
    }

    public List<MapHex> getSurroundingHexesList(Dictionary<string, MapHex> hexesMap)
    {
        List<MapHex> hexes = new List<MapHex>();
        if (hexesMap.ContainsKey("north"))
            hexes.Add(hexesMap["north"]);
        if (hexesMap.ContainsKey("south"))
            hexes.Add(hexesMap["south"]);
        if (hexesMap.ContainsKey("northEast"))
            hexes.Add(hexesMap["northEast"]);
        if (hexesMap.ContainsKey("northWest"))
            hexes.Add(hexesMap["northWest"]);
        if (hexesMap.ContainsKey("southEast"))
            hexes.Add(hexesMap["southEast"]);
        if (hexesMap.ContainsKey("southWest"))
            hexes.Add(hexesMap["southWest"]);
        return hexes;
    }

    public HashSet<MapHex> getMapHexesInRange(MapHex mapHex, int range)
    {
        HashSet<MapHex> hexes = new HashSet<MapHex>();
        Dictionary<int, HashSet<MapHex>> checkedHexes = new Dictionary<int, HashSet<MapHex>>();
        return getMapHexesInRange(hexes, checkedHexes, mapHex, range);
    }

    public HashSet<MapHex> getMapHexesInRange(HashSet<MapHex> hexes, Dictionary<int, HashSet<MapHex>> checkedHexes, MapHex mapHex, int range)
    {
        //Console.WriteLine("getMapHexesInRange(): mapHex=" + mapHex.X + "," + mapHex.Y + ", range=" + range + ", count=" + hexes.Count);

        if (range > 0)
        {
            if (!checkedHexes.ContainsKey(range))
            {
                HashSet<MapHex> setHexes = new HashSet<MapHex>();
                checkedHexes[range] = setHexes;
            }
            List<MapHex> surroundingHexes = getSurroundingHexesList(mapHex);
            //Console.WriteLine("getMapHexesInRange(): surroundingHexes=" + surroundingHexes.Count);
            foreach (MapHex nextHex in surroundingHexes)
            {
                //Console.WriteLine("getMapHexesInRange(): surroundingHex=" + nextHex.X + "," + nextHex.Y);
                if (!checkedHexes[range].Contains(nextHex))
                {
                    HashSet<MapHex> newHexes = getMapHexesInRange(hexes, checkedHexes, nextHex, range - 1);
                    hexes.Add(nextHex);
                }
            }
            checkedHexes[range].Add(mapHex);
            hexes.Add(mapHex);
        }

        return hexes;
    }

    public void checkBurbsForOwner(Server server)
    {

        for (int liY = 0; liY < Y; liY++)
        {
            for (int liX = 0; liX < X; liX++)
            {
                MapHex mapHex = Hexes[liY, liX];
                checkBurbOwner(server, mapHex);
            }
        }
    }
    public void checkBurbOwner(Server server, MapHex mapHex)
    {
        //MapHex mapHex = Hexes[y, x];
        if (mapHex.Burb != null)
        {
            if ("Capital".Equals(mapHex.Burb.Type) || "Metro".Equals(mapHex.Burb.Type) || "City".Equals(mapHex.Burb.Type) ||
                "capital".Equals(mapHex.Burb.Type) || "metro".Equals(mapHex.Burb.Type) || "city".Equals(mapHex.Burb.Type))
            {
                string color = null;
                Unit unit = mapHex.getUnit();
                if (unit != null)
                {
                    color = unit.Color;
                }
                Dictionary<string, MapHex> surroundingHexes = getSurroundingHexes(mapHex);
                if (surroundingHexes.ContainsKey("northWest"))
                {
                    MapHex northWestHex = surroundingHexes["northWest"];
                    unit = northWestHex.getUnit();
                    if (unit != null)
                    {
                        if (color == null)
                            color = unit.Color;
                        if (!color.Equals(unit.Color))
                            return;
                    }
                }
                if (surroundingHexes.ContainsKey("southWest"))
                {
                    MapHex southWestHex = surroundingHexes["southWest"];
                    unit = southWestHex.getUnit();
                    if (unit != null)
                    {
                        if (color == null)
                            color = unit.Color;
                        if (!color.Equals(unit.Color))
                            return;
                    }
                }
                if (surroundingHexes.ContainsKey("northEast"))
                {
                    MapHex northEastHex = surroundingHexes["northEast"];
                    unit = northEastHex.getUnit();
                    if (unit != null)
                    {
                        if (color == null)
                            color = unit.Color;
                        if (!color.Equals(unit.Color))
                            return;
                    }
                }
                if (surroundingHexes.ContainsKey("southEast"))
                {
                    MapHex southEastHex = surroundingHexes["southEast"];
                    unit = southEastHex.getUnit();
                    if (unit != null)
                    {
                        if (color == null)
                            color = unit.Color;
                        if (!color.Equals(unit.Color))
                            return;
                    }
                }
                if (mapHex.Burb != null && color != null)
                {
                    string previousOwnerColor = mapHex.Burb.OwnerColor;
                    mapHex.Burb.OwnerColor = color;
                    if (previousOwnerColor != null && ! previousOwnerColor.Equals(color))
                    {
                        server.sendGameStateAndMapHex(mapHex.X, mapHex.Y);
                        GameEvent gameEvent = new GameEvent("burbCaptured");
                        gameEvent.EnemyColor = previousOwnerColor;
                        gameEvent.MapHex = mapHex;
                        server.sendGamePlayEvent(color, gameEvent);
                        gameEvent.EventType = "burbLost";
                        gameEvent.EnemyColor = color;
                        server.sendGamePlayEvent(previousOwnerColor, gameEvent);
                    }
                }
            }
            else
            {
                string previousOwnerColor = mapHex.Burb.OwnerColor;
                Unit unit = mapHex.getUnit();
                if (previousOwnerColor != null && unit != null)
                {
                    mapHex.Burb.OwnerColor = unit.Color;
                    if (! previousOwnerColor.Equals(unit.Color))
                    {
                        server.sendGameStateAndMapHex(mapHex.X, mapHex.Y);
                        GameEvent gameEvent = new GameEvent("burbCaptured");
                        gameEvent.EnemyColor = previousOwnerColor;
                        gameEvent.MapHex = mapHex;
                        server.sendGamePlayEvent(unit.Color, gameEvent);
                        gameEvent.EventType = "burbLost";
                        gameEvent.EnemyColor = unit.Color;
                        server.sendGamePlayEvent(previousOwnerColor, gameEvent);

                    }
                }
            }
        }
    }

    public float calculateDistance(MapHex mapHex1, MapHex mapHex2)
    {
        float distance = (float)Math.Sqrt((Math.Pow(mapHex1.X - mapHex2.X, 2) + Math.Pow(mapHex1.Y - mapHex2.Y, 2)));
        return distance;
    }

    public List<UnitAction> determineSeaPath(MapHex origin, MapHex destination)
    {
        return determinePath(seaNodesGraph, origin, destination);
    }

    public List<UnitAction> determineLandPath(MapHex origin, MapHex destination)
    {
        return determinePath(landNodesGraph, origin, destination);
    }

    public List<UnitAction> determinePath(Dictionary<string, Node> graph, MapHex origin, MapHex destination)
    {
        //Console.WriteLine("determinePath(): from " + origin.X + "," + origin.Y + " to " + destination.X + "," + destination.Y);
        List<UnitAction> path = new List<UnitAction>();
        Node originNode = new Node(origin);
        Node destinationNode = new Node(destination);
        Dictionary<string, string> previousNodes = DijkstraAlgorithm.FindShortestPaths(graph, originNode.Name);
        Node node = destinationNode;
        List<string> nodesInPath = DijkstraAlgorithm.ReconstructPath(previousNodes, originNode.Name, destinationNode.Name);
        foreach (string nodeName in nodesInPath)
        {
            //Console.WriteLine("determinePath(): " + nodeName);
            UnitAction unitAction = new UnitAction();
            unitAction.Action = "move";
            string[] parts = nodeName.Split(",");
            int x = Int32.Parse(parts[0]);
            int y = Int32.Parse(parts[1]);
            if (x != origin.X || y != origin.Y)
            {
                unitAction.TargetX = x;
                unitAction.TargetY = y;
                unitAction.Ticks = DateTime.Now.Ticks;
                path.Add(unitAction);
            }
        }
        Console.WriteLine("determinePath(): path count=" + path.Count + " from " + origin.X + "," + origin.Y + " to " + destination.X + "," + destination.Y);
        return path;
    }

    private void buildNodesForShortestPath()
    {
        Console.WriteLine("buildNodesForShortestPath(): enter");
        int swampCount = 0;
        int burbCount = 0;
        for (int y = 0; y < Y; y++)
        {
            for (int x = 0; x < X; x++)
            {
                MapHex mapHex = Hexes[y, x];
                Node node = new Node(mapHex);
                List<MapHex> neighbors = getSurroundingHexesList(mapHex);
                allNodesGraph[node.Name] = node;
                List<Edge> edges = new List<Edge>();
                foreach (MapHex neighbor in neighbors)
                {
                    Node targetNode = new Node(neighbor);
                    Edge edge = new Edge(targetNode);
                    edges.Add(edge);
                }
                node.Edges = edges;

                if ("swamp".Equals(mapHex.Terrain) || "marsh".Equals(mapHex.Terrain))
                    swampCount += 1;
                if (mapHex.Burb != null)
                    burbCount += 1;
                if (("sea".Equals(mapHex.Terrain) || "swamp".Equals(mapHex.Terrain) || "marsh".Equals(mapHex.Terrain)) &&
                    (mapHex.Burb == null))
                {
                    Node seaNode = new Node(mapHex);
                    seaNodesGraph[seaNode.Name] = seaNode;
                    List<Edge> seaEdges = new List<Edge>();
                    foreach (MapHex neighbor in neighbors)
                    {
                        Node targetNode = new Node(neighbor);
                        if (("sea".Equals(neighbor.Terrain) || "swamp".Equals(neighbor.Terrain) || "marsh".Equals(neighbor.Terrain)) &&
                            (neighbor.Burb == null))
                        {
                            Edge edge = new Edge(targetNode);
                            seaEdges.Add(edge);
                        }
                    }
                    seaNode.Edges = seaEdges;
                }

                if ((!"sea".Equals(mapHex.Terrain)))
                {
                    Node landNode = new Node(mapHex);
                    landNodesGraph[node.Name] = node;
                    List<Edge> landEdges = new List<Edge>();
                    foreach (MapHex neighbor in neighbors)
                    {
                        Node targetNode = new Node(neighbor);
                        if ((!"sea".Equals(neighbor.Terrain)) && (neighbor.Burb == null))
                        {
                            Edge edge = new Edge(targetNode);
                            landEdges.Add(edge);
                        }
                    }
                    landNode.Edges = landEdges;
                }

            }
        }
        Console.WriteLine("buildNodesForShortestPath(): allNodes=" + allNodesGraph.Count + ", seaNodes=" + seaNodesGraph.Count +
                          ", landNodes=" + landNodesGraph.Count + ", burbCount=" + burbCount + ", swampCount=" + swampCount);
    }


}
