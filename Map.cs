using static UnitTypeConstants;
using GlobalConquest.Actions;
using GlobalConquest.Units;
using static GameConstants;
using Microsoft.Xna.Framework;
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
    public Dictionary<string, Point> MetroLocationPoints { get; set; } = new Dictionary<string, Point>();
    public Dictionary<string, MapHex> LeftMetro { get; set; } = new Dictionary<string, MapHex>();
    public Dictionary<string, MapHex> RightMetro { get; set; } = new Dictionary<string, MapHex>();
    public Dictionary<string, MapHex> DiagonalMetro { get; set; } = new Dictionary<string, MapHex>();

    public Dictionary<string, Unit> UnitIdToUnit { get; set; } = new Dictionary<string, Unit>();
    public Dictionary<string, HashSet<string>> ColorToUnitIds { get; set; } = new Dictionary<string, HashSet<string>>();
    public bool IsMapReady { get; set; } = false;
    Random random = new Random();

    public Map()
    {
        positionMetros();
    }

    public Map(int y, int x, int numberOfIslands=1)
    {
        Y = y;
        X = x;
        positionMetros();
        Hexes = generateMap(y, x, numberOfIslands);
        buildNodesForShortestPath();
        IsMapReady = true;
        foreach (string color in NATIVE_AND_FACTION_COLORS)
        {
            ColorToUnitIds[color] = new HashSet<string>();
        }
    }

    private void positionMetros()
    {
        bool isEven = false;
        if (X % 2 == 0)
            isEven = true;
        MetroLocationPoints[AMBER] = new Point(1, 0);
        MetroLocationPoints[MAGENTA] = new Point(1, Y-2);

        if (isEven)
        {
            MetroLocationPoints[OCHER] = new Point(X-2, 1);
            MetroLocationPoints[CYAN] = new Point(X-2, Y-1);
        }
        else
        {
            MetroLocationPoints[OCHER] = new Point(X-2, 0);
            MetroLocationPoints[CYAN] = new Point(X-2, Y-2);
        }        
    }

    public void addBurbs(Burbs burbs, int desiredBurbCount)
    {
        addFixedBurbs(burbs);
        Globals.Log("addBurbs(): desiredBurbCount=" + desiredBurbCount);
        int numberOfBurbs = 0;
        int tries = 0;
        while (numberOfBurbs < desiredBurbCount && tries < (desiredBurbCount * 10))
        {
            int x = random.Next(0, X);
            int y = random.Next(0, Y);
            MapHex mapHex = Hexes[y, x];
            bool burbConflictFound = false;
            if (mapHex.Burb == null && (mapHex.Terrain.Equals(TERRAIN_GRASS) || mapHex.Terrain.Equals(TERRAIN_MOUNTAIN) || 
                                        mapHex.Terrain.Equals(TERRAIN_FOREST)) )
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
                string type = BURB_VILLAGE;
                int typeRange = random.Next(0, 100);
                if (typeRange < 50)      // 50%
                    type = BURB_VILLAGE;
                else if (typeRange < 80) // 30%
                    type = BURB_TOWN;
                else
                    type = BURB_CITY;       // 20%
                Burb burb = burbs.addBurb(random, type, this, mapHex);
                Globals.Log("addBurbs(): added burb " + burb.Name + " at " + mapHex.X + "," + mapHex.Y);
                numberOfBurbs += 1;
            }
            tries += 1;
        }
        Globals.Log("addBurbs(): numberOfBurbs=" + numberOfBurbs);
    }

    public void addFixedBurbs(Burbs burbs)
    {
        positionMetros();
        Globals.Log("addFixedBurbs(): height=" + Hexes.GetLength(0) + ", width=" + Hexes.GetLength(1));
        Globals.Log("addFixedBurbs(): " + MetroLocationPoints[MAGENTA].X + "," + MetroLocationPoints[MAGENTA].Y);
        burbs.addBurb(FACTION_AMBER_ARRAY, BURB_METROPLEX, this, Hexes[MetroLocationPoints[AMBER].Y, MetroLocationPoints[AMBER].X], AMBER);
        burbs.addBurb(FACTION_MAGENTA_MOB, BURB_METROPLEX, this, Hexes[MetroLocationPoints[MAGENTA].Y, MetroLocationPoints[MAGENTA].X], MAGENTA);
        burbs.addBurb(FACTION_OCHER_ORDER, BURB_METROPLEX, this, Hexes[MetroLocationPoints[OCHER].Y, MetroLocationPoints[OCHER].X], OCHER);
        burbs.addBurb(FACTION_CYAN_CIRCLE, BURB_METROPLEX, this, Hexes[MetroLocationPoints[CYAN].Y, MetroLocationPoints[CYAN].X], CYAN);
        burbs.addBurb("Washington", BURB_CAPITAL, this, Hexes[Y / 2, X / 2]);
        LeftMetro[AMBER] = MetroLocations[OCHER];
        RightMetro[AMBER] = MetroLocations[MAGENTA];
        DiagonalMetro[AMBER] = MetroLocations[CYAN];
        LeftMetro[OCHER] = MetroLocations[CYAN];
        RightMetro[OCHER] = MetroLocations[AMBER];
        DiagonalMetro[OCHER] = MetroLocations[MAGENTA];
        LeftMetro[CYAN] = MetroLocations[MAGENTA];
        RightMetro[CYAN] = MetroLocations[OCHER];
        DiagonalMetro[CYAN] = MetroLocations[AMBER];
        LeftMetro[MAGENTA] = MetroLocations[AMBER];
        RightMetro[MAGENTA] = MetroLocations[CYAN];
        DiagonalMetro[MAGENTA] = MetroLocations[OCHER];
    }

    public MapHex getCapitalHex()
    {
        return Hexes[Y / 2, X / 2];
    }
    public MapHex getMetroHex(string color)
    {
        return MetroLocations[color];
    }

    public bool IsMetroHex(MapHex mapHex)
    {
        foreach (string color in FACTION_COLORS)
        {
            if (mapHex.X == MetroLocations[color].X && mapHex.Y == MetroLocations[color].Y)
                return true;
        }
        return false;
    }

    public MapHex[,] generateMap(int height, int width, int numberOfIslands)
    {
        MapHex[,] hexes = new MapHex[height, width];
        long milliseconds = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        for (int liY = 0; liY < height; liY++)
        {
            for (int liX = 0; liX < width; liX++)
            {
                int x = liX;
                int y = liY;
                int h = height;
                int w = width;
                //int textureIndex = rnd.Next(1, 7);
                float elevationNoise = OpenSimplex2S.Noise2(milliseconds, liX, liY);
                float moistureNoise = OpenSimplex2S.Noise2(milliseconds, liX, liY);
                //float elevationNoise = OpenSimplex2.Noise2(milliseconds, liX, liY);
                //float moistureNoise = OpenSimplex2.Noise2(milliseconds, liX, liY);
                string biome = determineBiome(elevationNoise, moistureNoise);

                // 1 balanced
                // 2 vertical or horizontal
                // 3 vertical or horizontal
                // 4 balanced
                // 5 balanced
                int islands = numberOfIslands;
                string orientation = MAP_ORIENTATION_BALANCED; // balanced, horizontal, vertical
                if (islands == 2 || islands == 3)
                {
                    if (X >= Y)
                        orientation  = MAP_ORIENTATION_HORIZONTAL;
                    else
                        orientation = MAP_ORIENTATION_VERTICAL;
                }

                if ((islands == 2 || islands == 4) && (MAP_ORIENTATION_HORIZONTAL.Equals(orientation) || MAP_ORIENTATION_BALANCED.Equals(orientation)))
                {
                    // 2 horizontal islands
                    if (liX <= width/2)
                        w = width/2;
                    else
                    {
                        x = liX-width/2;
                        w = width/2;
                    }
                }

                if ((islands == 2 || islands == 4) && (MAP_ORIENTATION_VERTICAL.Equals(orientation) || MAP_ORIENTATION_BALANCED.Equals(orientation)))
                {
                    // 2 vertical islands
                    if (liY <= height/2)
                        h = height/2;
                    else
                    {
                        y = liY-height/2;
                        h = height/2;
                    }
                }

                if ((islands == 3) && (MAP_ORIENTATION_VERTICAL.Equals(orientation)))
                {
                    // 3 vertical islands
                    if (liY < height/3)
                        h = height/3;
                    else if (liY >= height/3 && liY < height - height/3)
                    {
                        y = liY - height/3;
                        h = height/3;
                    }
                    else
                    {
                        y = liY - (2 * height/3);
                        h = height/3;
                    }
                }
                if ((islands == 3) && (MAP_ORIENTATION_HORIZONTAL.Equals(orientation)))
                {
                    // 3 horizontal islands
                    if (liX < width/3)
                        w = width/3;
                    else if (liX >= width/3 && liX < width - width/3)
                    {
                        x = liX - width/3;
                        w = width/3;
                    }
                    else
                    {
                        x = liX - (2 * width/3);
                        w = width/3;
                    }
                }

                if (islands == 5)
                {
                    if (liY <= height/2)
                        h = height/2;
                    else
                    {
                        y = liY - (height/2);
                        h = height/2;
                    }

                    if (liX <= width/2)
                        w = width/2;
                    else
                    {
                        x = liX - (width/2);
                        w = width/2;
                    }

                    if (liY >= height/3 && liY < height - height/3 &&
                        liX >= width/3 && liX < width - width/3)
                    {
                        y = liY - height/3;
                        h = height/3;
                        x = liX - width/3;
                        w = width/3;                        
                    }
                    
                }

                elevationNoise = shapeForIsland(biome, elevationNoise, x, y, w, h);
                elevationNoise = makeSeaBorder(biome, elevationNoise, liX, liY, width, height, islands);
                string newBiome = determineBiome(elevationNoise, moistureNoise);
                if (!newBiome.Equals(biome))
                {
                    //Globals.Log("changed biome from " + biome + " to " + newBiome);
                }
                MapHex mapHex = new MapHex();
                mapHex.X = liX;
                mapHex.Y = liY;
                mapHex.Terrain = newBiome;
                hexes[liY, liX] = mapHex;
            }
        }
        Globals.Log("generateMap(): hexes=" + hexes.GetLength(0) + "," + hexes.GetLength(1));
        return hexes;
    }

    public void placeNewUnit(Unit unit, MapHex mapHex)
    {
        placeNewUnit(unit, mapHex.X, mapHex.Y);
    }

    public void placeNewPlane(Unit unit, MapHex mapHex)
    {
        placeNewUnit(unit, mapHex.X, mapHex.Y);
    }

    public void placeNewUnit(Unit unit, int x, int y)
    {
        if (unit != null && x >= 0 && x < X && y >= 0 && y < Y)
        {
            unit.HomeBurbX = x;
            unit.HomeBurbY = y;
            unit.OriginalBurbX = x;
            unit.OriginalBurbY = y;
            unit.X = x;
            unit.Y = y;
            string id = unit.generateId();

            if (AIRPLANE.Equals(unit.UnitType))
                Hexes[y, x].Airplane = unit;
            else
            {
                MapHex mapHex = Hexes[y, x];
                mapHex.setUnit(unit);
                if (mapHex.Burb != null && mapHex.Burb.Type.Equals(BURB_DOCK))
                {
                    if (unit.UnitType.Equals(INFANTRY))
                        unit.UnitType = TRANSPORT_INFANTRY;
                    if (unit.UnitType.Equals(ARMOR))
                        unit.UnitType = TRANSPORT_ARMOR;
                }
            }
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

    public bool moveUnit(Unit unit, int destinationX, int destinationY)
    {
        bool hasUnitMoved = false;
        if (unit.X != destinationX || unit.Y != destinationY)
        {
            MapHex targetMapHex = Hexes[destinationY, destinationX];
            MapHex sourceMapHex = Hexes[unit.Y, unit.X];
            if (sourceMapHex.Unit != null)
            {
                targetMapHex.setUnit(unit);
                if (Hexes[unit.Y, unit.X].Unit != null)
                    Hexes[unit.Y, unit.X].Unit = null;
                hasUnitMoved = true;
            }
            unit.X = destinationX;
            unit.Y = destinationY;
        }
        return hasUnitMoved;
    }


    private string determineBiome(float elevation, float moisture)
    {
        // these thresholds will need tuning to match your generator
        if (elevation < 0.1F)
        {
            return TERRAIN_SEA;
        }
        if (elevation < 0.12F)
        {
            return TERRAIN_SWAMP;
        }

        if (elevation > 0.85F)
        {
            return TERRAIN_MOUNTAIN;
        }

        if (elevation > 0.6F)
        {
            if (moisture < 0.02F)
            {
                //return "desert";
                return TERRAIN_GRASS;
            }
            if (moisture < 0.66F)
            {
                return TERRAIN_GRASS;
            }
            return TERRAIN_FOREST;
        }

        if (elevation > 0.3F)
        {
            //if (moisture < 0.05F)
            //{
            //    return "desert";
            //}
            if (moisture < 0.50F)
            {
                return TERRAIN_GRASS;
            }
            if (moisture < 0.83F)
            {
                return TERRAIN_FOREST;
            }
            return TERRAIN_FOREST;
        }

        //if (moisture < 0.05F)
        //{
        //    return "desert";
        //}
        if (moisture < 0.33F)
        {
            return TERRAIN_GRASS;
        }
        if (moisture < 0.66F)
        {
            return TERRAIN_FOREST;
        }
        return TERRAIN_FOREST;
    }

    private float makeSeaBorder(string biome, float elevation, int x, int y, int width, int height, 
                                int islands)
    {
        float newElevation = elevation;
        int xBorder = 1;
        int yBorder = 1;
        if (width >= 50 || islands > 1)
            xBorder = 2;
        if (height >= 50 || islands > 1)
            yBorder = 2;
        if (x<=xBorder || y<=yBorder || x>=width-1-xBorder || y>=height-1-yBorder)
        {
            if (TERRAIN_SEA.Equals(biome))
                newElevation = 0.0F;
            else if (TERRAIN_SWAMP.Equals(biome))
                newElevation = 0.11F;
            else
            {
                int seaOrSwamp = random.Next(2);
                if (seaOrSwamp == 0)
                    newElevation = 0.0F;
                else
                    newElevation = 0.11F;
            }
            //Globals.Log("makeSeaBorder(): mapValue=" + mapValue + ", biome=" + biome + ", elevation=" + elevation + ", x=" + x + ", y=" + y + ", xd=" + xDistance + ", yd=" + yDistance + ", distance=" + distance + ", newE=" + newElevation);
        }
        return newElevation;
    }

    // https://www.redblobgames.com/maps/terrain-from-noise/
    private float shapeForIsland(string biome, float elevation, int x, int y, int width, int height)
    {
        float newElevation = elevation;
        // nx = 2*x/width - 1 and ny = 2*y/height - 1
        // square bump: d = 1 - (1-nx²) * (1-ny²)
        // euclidian^2: d = min(1, (nx² + ny²) / sqrt(2))
        float nWidth = 0;
        if (x != 0)
            nWidth = (2.0F * x / width) - 1.0F;
        float nHeight = 0;
        if (y != 0)
            nHeight = (2.0F * y / height) - 1.0F;
        float nX = nWidth;
        float nY = nHeight;
        float mix = 0.5F;
        //float distance = 1.0F - ((1.0F - (nWidth * (x ^ 2))) * ((1.0F - (nHeight * (y ^ 2)))));
        // distance from center
        int xDistance = Math.Abs((width / 2) - x);
        int yDistance = Math.Abs((height / 2) - y);
        float distance = (float)Math.Sqrt((xDistance * xDistance) + (yDistance * yDistance));
        float mapValue = (float)Math.Sqrt((width * width) + (height * height)) / 2;
        float euclidianDistance = (float)Math.Sqrt((nX * nX) + (nY * nY)) / 2;
        float squareBumpDistance = 1 - (1 - (nX*nX)) * (1 - (nY*nY));
        mapValue = 1.0f;
        distance = squareBumpDistance;
    
        // Lerp(a, b, t) is defined as a + (b — a) * t.
        // e = lerp(e, 1-d, mix)
        // float newElevation = elevation + (1.0F - distance - elevation) * mix;

        // 20% of the hexes don't change
        int leaveAlone = random.Next(5);
        if (leaveAlone == 0)
            return newElevation;

        if (distance < (mapValue * .2F) &&
            (biome.Equals(TERRAIN_SEA) || biome.Equals(TERRAIN_SWAMP)))
        {
            newElevation = elevation + 01.0F;
        }
        else if (distance < (mapValue * .3F) &&
            (biome.Equals(TERRAIN_SEA) || biome.Equals(TERRAIN_SWAMP)))
        {
            newElevation = elevation + 0.75F;
        }
        else if ((distance > (mapValue * .8F) ||
                xDistance >= width / 2 - 1 || yDistance >= height / 2 - 1) &&
                !(biome.Equals(TERRAIN_SEA) || biome.Equals(TERRAIN_SWAMP)))
        {
            //newElevation = elevation - 0.75F;
            newElevation = .09F;
            //Globals.Log("shapeForIsland(): diagonal=" + diagonal + ", biome=" + biome + ", elevation=" + elevation + ", x=" + x + ", y=" + y + ", xd=" + xDistance + ", yd=" + yDistance + ", distance=" + distance + ", newE=" + newElevation);
        }
        else if ((distance > (mapValue * .7F) ||
                xDistance >= width / 2 - 1 || yDistance >= height / 2 - 1) &&
                !(biome.Equals(TERRAIN_SEA) || biome.Equals(TERRAIN_SWAMP)))
        {
            newElevation = elevation - 0.75F;
            //newElevation = .09F;
            //Globals.Log("shapeForIsland(): diagonal=" + diagonal + ", biome=" + biome + ", elevation=" + elevation + ", x=" + x + ", y=" + y + ", xd=" + xDistance + ", yd=" + yDistance + ", distance=" + distance + ", newE=" + newElevation);
        }
        else if ((xDistance >= width / 2 - 1 || yDistance >= height / 2 - 1) &&
                (!(biome.Equals(TERRAIN_SEA) || biome.Equals(TERRAIN_SWAMP))))
        {
            newElevation = .11F;
            //Globals.Log("shapeForIsland(): mapValue=" + mapValue + ", biome=" + biome + ", elevation=" + elevation + ", x=" + x + ", y=" + y + ", xd=" + xDistance + ", yd=" + yDistance + ", distance=" + distance + ", newE=" + newElevation);

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
        //Globals.Log("Map.Equals(): false");
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
        if (mapHex == null)
            return hexes;
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
            hexes[DIRECTION_NORTH] = northHex;
        }
        if (mapHex.Y + 1 < Y)
        {
            MapHex southHex = Hexes[mapHex.Y + 1, mapHex.X];
            hexes[DIRECTION_SOUTH] = southHex;
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
                hexes[DIRECTION_NORTH_EAST] = northEastHex;
            }
            if (mapHex.Y + 1 < Y && mapHex.X + 1 < X)
            {
                southEastHex = Hexes[mapHex.Y + 1, mapHex.X + 1];
                hexes[DIRECTION_SOUTH_EAST] = southEastHex;
            }
            if (mapHex.Y + 1 < Y && mapHex.X - 1 >= 0)
            {
                southWestHex = Hexes[mapHex.Y + 1, mapHex.X - 1];
                hexes[DIRECTION_SOUTH_WEST] = southWestHex;
            }
            if (mapHex.X - 1 >= 0)
            {
                northWestHex = Hexes[mapHex.Y, mapHex.X - 1];
                hexes[DIRECTION_NORTH_WEST] = northWestHex;
            }
        }
        else
        {
            if (mapHex.Y - 1 >= 0 && mapHex.X + 1 < X)
            {
                northEastHex = Hexes[mapHex.Y - 1, mapHex.X + 1];
                hexes[DIRECTION_NORTH_EAST] = northEastHex;
            }
            if (mapHex.X + 1 < X)
            {
                southEastHex = Hexes[mapHex.Y, mapHex.X + 1];
                hexes[DIRECTION_SOUTH_EAST] = southEastHex;
            }
            if (mapHex.X - 1 >= 0)
            {
                southWestHex = Hexes[mapHex.Y, mapHex.X - 1];
                hexes[DIRECTION_SOUTH_WEST] = southWestHex;
            }
            if (mapHex.Y - 1 >= 0 && mapHex.X - 1 >= 0)
            {
                northWestHex = Hexes[mapHex.Y - 1, mapHex.X - 1];
                hexes[DIRECTION_NORTH_WEST] = northWestHex;
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
        if (hexesMap.ContainsKey(DIRECTION_NORTH))
            hexes.Add(hexesMap[DIRECTION_NORTH]);
        if (hexesMap.ContainsKey(DIRECTION_SOUTH))
            hexes.Add(hexesMap[DIRECTION_SOUTH]);
        if (hexesMap.ContainsKey(DIRECTION_NORTH_EAST))
            hexes.Add(hexesMap[DIRECTION_NORTH_EAST]);
        if (hexesMap.ContainsKey(DIRECTION_NORTH_WEST))
            hexes.Add(hexesMap[DIRECTION_NORTH_WEST]);
        if (hexesMap.ContainsKey(DIRECTION_SOUTH_EAST))
            hexes.Add(hexesMap[DIRECTION_SOUTH_EAST]);
        if (hexesMap.ContainsKey(DIRECTION_SOUTH_WEST))
            hexes.Add(hexesMap[DIRECTION_SOUTH_WEST]);
        return hexes;
    }

    public HashSet<MapHex> getMapHexesAtDistance(MapHex mapHex, int distance)
    {
        HashSet<MapHex> rangeHexes = getMapHexesInRange(mapHex, distance);
        if (distance > 1)
        {
            HashSet<MapHex> rangeMinusOneHexes = getMapHexesInRange(mapHex, distance-1);
            rangeHexes.ExceptWith(rangeMinusOneHexes);
        }
        HashSet<MapHex> finalRangeHexes = rangeHexes;
        if (finalRangeHexes.Contains(mapHex))
            finalRangeHexes.Remove(mapHex);
        return finalRangeHexes;
    }

    public MapHex getClosestUnoccupiedHexAtDistance(MapHex sourceHex, MapHex targetHex, int distance)
    {
        if (sourceHex == null || targetHex == null || distance <= 1)
            return null;
        MapHex closestHex = null;
        float closestCalculatedDistance = -1.0f;
        HashSet<MapHex> hexesAtDistance = getMapHexesAtDistance(targetHex, distance);
        foreach(MapHex mapHex in hexesAtDistance)
        {
            float calculatedDistance = calculateDistance(sourceHex, mapHex);
            if (mapHex.getUnit() == null && (closestCalculatedDistance == -1.0f || calculatedDistance < closestCalculatedDistance))
            {
                closestHex = mapHex;
                closestCalculatedDistance = calculatedDistance;
            }
        }
        return closestHex;
    }

    public HashSet<MapHex> getMapHexesInRange(MapHex mapHex, int range, bool useOriginalLogic=true, bool shouldLog=false)
    {
        HashSet<MapHex> hexes = new HashSet<MapHex>();
        if (range > 5)
            useOriginalLogic = false;
        Dictionary<int, HashSet<MapHex>> checkedHexes = new Dictionary<int, HashSet<MapHex>>();
        hexes = getMapHexesInRange(hexes, checkedHexes, mapHex, range, useOriginalLogic, shouldLog);
        if (shouldLog)
            Globals.Log("getMapHexesInRange(): count=" + hexes.Count);
        return hexes;
    }

    // TODO: refactor this method
    public HashSet<MapHex> getMapHexesInRange(HashSet<MapHex> hexes, Dictionary<int, HashSet<MapHex>> checkedHexes, 
           MapHex mapHex, int range, bool useOriginalLogic, bool shouldLog)
    {
        //Globals.Log("getMapHexesInRange(): mapHex=" + mapHex.X + "," + mapHex.Y + ", range=" + range + ", count=" + hexes.Count);

        if (range > 0 && useOriginalLogic)
        {
            if (!checkedHexes.ContainsKey(range))
            {
                HashSet<MapHex> setHexes = new HashSet<MapHex>();
                checkedHexes[range] = setHexes;
            }
            List<MapHex> surroundingHexes = getSurroundingHexesList(mapHex);
            //Globals.Log("getMapHexesInRange(): surroundingHexes=" + surroundingHexes.Count);
            foreach (MapHex nextHex in surroundingHexes)
            {
                //Globals.Log("getMapHexesInRange(): surroundingHex=" + nextHex.X + "," + nextHex.Y);
                if (!checkedHexes[range].Contains(nextHex))
                {
                    HashSet<MapHex> newHexes = getMapHexesInRange(hexes, checkedHexes, nextHex, range - 1, useOriginalLogic, shouldLog);
                    hexes.Add(nextHex);
                }
            }
            checkedHexes[range].Add(mapHex);
            hexes.Add(mapHex);
        }

        // TODO: This is less precise than the above but much quicker.
        else if (range > 0)
        {
            hexes.Add(mapHex);
            int x = mapHex.X;
            int y = mapHex.Y;
            int minX = x - range;
            int maxX = x + range;
            int minY = y - range;
            int maxY = y + range;
            if (minX < 0)
                minX = 0;
            if (maxX > X)
                maxX = X;
            if (minY < 0)
                minY = 0;
            if (maxY > Y)
                maxY = Y;

            for (int liY = minY; liY < maxY; liY++)
            {
                for (int liX = minX; liX < maxX; liX++)
                {
                    float distance = -1;
                    //if (liX == minX || liX >= maxX-2 || liY == minY || liY >= maxY-2)
                    //    distance = Math.Abs(calculateDistance(mapHex, Hexes[liY, liX]));
                    if (Math.Round(distance) <= range)
                    {
                        //if (shouldLog && distance != -1)
                        //    Globals.Log("getMapHexesInRange(): from " + mapHex.X + "," + mapHex.Y + " to " + liX + "," + liY + ": " + distance);
                        hexes.Add(Hexes[liY, liX]);
                    }
                }
            }
        }
        else if (range == 0)
            hexes.Add(mapHex);

        return hexes;
    }

    public void checkBurbsForOwner(Server server)
    {

        for (int liY = 0; liY < Y; liY++)
        {
            for (int liX = 0; liX < X; liX++)
            {
                MapHex mapHex = Hexes[liY, liX];
                updateBurbOwners(server, mapHex);
            }
        }
    }
    public void updateBurbOwners(Server server, MapHex mapHex)
    {
        bool burbCaptured = false;
        if (mapHex.Burb != null)
        {
            string previousOwnerColor = mapHex.Burb.OwnerColor;
            string newOwnerColor = mapHex.Burb.OwnerColor;
            List<string> directions = [];
            if (BURB_DOCK.Equals(mapHex.Burb.Type) || BURB_SUBURB.Equals(mapHex.Burb.Type))
                return;
            if (BURB_CAPITAL.Equals(mapHex.Burb.Type) || BURB_METROPLEX.Equals(mapHex.Burb.Type) || BURB_CITY.Equals(mapHex.Burb.Type) ||
                BURB_CAPITAL.Equals(mapHex.Burb.Type) || BURB_METROPLEX.Equals(mapHex.Burb.Type) || BURB_CITY.Equals(mapHex.Burb.Type))
                directions = [DIRECTION_NORTH, DIRECTION_SOUTH, DIRECTION_NORTH_WEST, DIRECTION_SOUTH_WEST, DIRECTION_NORTH_EAST, DIRECTION_SOUTH_EAST];
            else if (BURB_TOWN.Equals(mapHex.Burb.Type))
                directions = [DIRECTION_NORTH, DIRECTION_SOUTH];
            if (BURB_CAPITAL.Equals(mapHex.Burb.Type) || BURB_METROPLEX.Equals(mapHex.Burb.Type) || BURB_CITY.Equals(mapHex.Burb.Type) ||
                BURB_CAPITAL.Equals(mapHex.Burb.Type) || BURB_METROPLEX.Equals(mapHex.Burb.Type) || BURB_CITY.Equals(mapHex.Burb.Type) ||
                BURB_TOWN.Equals(mapHex.Burb.Type))
            {
                string color = null;
                Unit unit = mapHex.getUnit();
                if (unit != null && !SPY.Equals(unit.UnitType))
                {
                    color = unit.Color;
                }
                Dictionary<string, MapHex> surroundingHexes = getSurroundingHexes(mapHex);
                foreach (string direction in directions)
                {
                    if (surroundingHexes.ContainsKey(direction))
                    {
                        MapHex neighborHex = surroundingHexes[direction];
                        unit = neighborHex.getUnit();
                        if (unit != null && !SPY.Equals(unit.UnitType))
                        {
                            if (color == null)
                                color = unit.Color;
                            if (!color.Equals(unit.Color))
                                return;
                        }
                    }
                }
                if (color != null)
                {
                    mapHex.Burb.OwnerColor = color;
                    if (previousOwnerColor != null && !previousOwnerColor.Equals(color))
                    {
                        burbCaptured = true;
                        newOwnerColor = color;
                    }
                }
                foreach (string direction in directions)
                {
                    if (surroundingHexes.ContainsKey(direction))
                    {
                        MapHex neighborHex = surroundingHexes[direction];
                        if (neighborHex.Burb != null)
                        {
                            neighborHex.Burb.OwnerColor = mapHex.Burb.OwnerColor;
                        }
                    }
                }
            }
            else
            {
                Unit unit = mapHex.getUnit();
                if (previousOwnerColor != null && unit != null && !SPY.Equals(unit.UnitType))
                {
                    mapHex.Burb.OwnerColor = unit.Color;
                    if (!previousOwnerColor.Equals(unit.Color))
                    {
                        burbCaptured = true;
                        newOwnerColor = unit.Color;
                    }
                }
            }
            if (burbCaptured)
            {
                mapHex.Airplane = null;
                server.sendGameStateAndMapHex(mapHex.X, mapHex.Y);
                GameEvent gameEvent = new GameEvent("burbCaptured");
                gameEvent.EnemyColor = previousOwnerColor;
                gameEvent.MapHex = mapHex;
                server.sendGamePlayEvent(newOwnerColor, gameEvent);
                gameEvent.EventType = "burbLost";
                gameEvent.EnemyColor = newOwnerColor;
                server.sendGamePlayEvent(previousOwnerColor, gameEvent);
            }

        }
    }

    // TODO: This is not precise, as "hex math" does not match the coord system we are using.
    // A row snakes up and down so the vertical distance to a hex in a row changes.
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

    public List<UnitAction> determinePath(MapHex origin, MapHex destination)
    {
        return determinePath(allNodesGraph, origin, destination);
    }


    public List<UnitAction> determinePath(Dictionary<string, Node> graph, MapHex origin, MapHex destination)
    {
        Globals.Log("determinePath(): from " + origin.X + "," + origin.Y + " to " + destination.X + "," + destination.Y);
        List<UnitAction> path = new List<UnitAction>();
        Node originNode = new Node(origin);
        Node destinationNode = new Node(destination);
        Dictionary<string, string> previousNodes = DijkstraAlgorithm.FindShortestPaths(graph, originNode.Name);
        List<string> nodesInPath = DijkstraAlgorithm.ReconstructPath(previousNodes, originNode.Name, destinationNode.Name);
        foreach (string nodeName in nodesInPath)
        {
            //Globals.Log("determinePath(): " + nodeName);
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
        Globals.Log("determinePath(): path count=" + path.Count + " from " + origin.X + "," + origin.Y + " to " + destination.X + "," + destination.Y);
        return path;
    }

    private void buildNodesForShortestPath()
    {
        buildNodesForShortestPath(false, this.allNodesGraph, this.seaNodesGraph, this.landNodesGraph, null);
    }

    public void buildNodesForShortestPath(bool shouldAvoidUnits, Dictionary<string, Node> graph,
                                           Dictionary<string, Node> seaGraph, Dictionary<string, Node> landGraph,
                                           MapHex destinationHex)
    {
        Globals.Log("buildNodesForShortestPath(): enter");
        int swampCount = 0;
        int burbCount = 0;
        for (int y = 0; y < Y; y++)
        {
            for (int x = 0; x < X; x++)
            {
                MapHex mapHex = Hexes[y, x];

                Node node = new Node(mapHex);
                List<MapHex> neighbors = getSurroundingHexesList(mapHex);
                if (graph != null)
                    graph[node.Name] = node;
                List<Edge> edges = new List<Edge>();
                foreach (MapHex neighbor in neighbors)
                {
                    if (shouldAvoidUnits && neighbor.getUnit() != null && destinationHex != null &&
                        !(neighbor.X == destinationHex.X && neighbor.Y == destinationHex.Y))
                        continue;
                    Node targetNode = new Node(neighbor);
                    Edge edge = new Edge(targetNode);
                    edges.Add(edge);
                }
                node.Edges = edges;

                if (TERRAIN_SWAMP.Equals(mapHex.Terrain) || "marsh".Equals(mapHex.Terrain))
                    swampCount += 1;
                if (mapHex.Burb != null)
                    burbCount += 1;
                if ((TERRAIN_SEA.Equals(mapHex.Terrain) || TERRAIN_SWAMP.Equals(mapHex.Terrain) || "marsh".Equals(mapHex.Terrain)) &&
                    (mapHex.Burb == null || BURB_DOCK.Equals(mapHex.Burb.Type) || BURB_METROPLEX.Equals(mapHex.Burb.Type)))
                {
                    Node seaNode = new Node(mapHex);
                    if (seaGraph != null)
                        seaGraph[seaNode.Name] = seaNode;
                    List<Edge> seaEdges = new List<Edge>();
                    foreach (MapHex neighbor in neighbors)
                    {
                        if (shouldAvoidUnits && neighbor.getUnit() != null)
                            continue;

                        Node targetNode = new Node(neighbor);
                        if ((TERRAIN_SEA.Equals(neighbor.Terrain) || TERRAIN_SWAMP.Equals(neighbor.Terrain) || "marsh".Equals(neighbor.Terrain)) &&
                            (neighbor.Burb == null || BURB_DOCK.Equals(neighbor.Burb.Type)))
                        {
                            Edge edge = new Edge(targetNode);
                            seaEdges.Add(edge);
                        }
                    }
                    seaNode.Edges = seaEdges;
                }

                if ((!TERRAIN_SEA.Equals(mapHex.Terrain)))
                {
                    Node landNode = new Node(mapHex);
                    if (landGraph != null)
                        landGraph[node.Name] = node;
                    List<Edge> landEdges = new List<Edge>();
                    foreach (MapHex neighbor in neighbors)
                    {
                        if (shouldAvoidUnits && neighbor.getUnit() != null)
                            continue;

                        Node targetNode = new Node(neighbor);
                        if ((!TERRAIN_SEA.Equals(neighbor.Terrain)) && (neighbor.Burb == null))
                        {
                            Edge edge = new Edge(targetNode);
                            landEdges.Add(edge);
                        }
                    }
                    landNode.Edges = landEdges;
                }

            }
        }
        int graphCount = 0;
        if (graph != null)
            graphCount = graph.Count;
        int seaCount = 0;
        if (seaGraph != null)
            seaCount = seaGraph.Count;
        int landCount = 0;
        if (landGraph != null)
            landCount = landGraph.Count;
        Globals.Log("buildNodesForShortestPath(): allNodes=" + graphCount + ", seaNodes=" + seaCount +
                          ", landNodes=" + landCount + ", burbCount=" + burbCount + ", swampCount=" + swampCount);
    }

    public void restoreMap(Burbs burbs)
    {
        foreach (string color in NATIVE_AND_FACTION_COLORS)
        {
            ColorToUnitIds[color] = new HashSet<string>();
        }

        addFixedBurbs(burbs);
        buildNodesForShortestPath();
        for (int y = 0; y < Y; y++)
        {
            for (int x = 0; x < X; x++)
            {
                MapHex mapHex = Hexes[y, x];
                if (mapHex == null)
                {
                    Globals.Log("restoreMap(): map is corrupted at " + x + "," + y);
                    continue;
                }
                Unit unit = mapHex.getUnit();
                if (unit != null)
                {
                    ColorToUnitIds[unit.Color].Add(unit.Id);
                    UnitIdToUnit[unit.Id] = unit;
                }

                Burb burb = mapHex.Burb;
                if (burb != null  && burb.Name != null && !BURB_SUBURB.Equals(burb.Type) && !BURB_DOCK.Equals(burb.Type))
                {
                    burbs.NameToBurb[burb.Name] = burb;
                }

            }
        }
    }

    public void outputDataStructureUse()
    {
        Globals.Log("outputDataStructureUse(): allNodesGraph=" + allNodesGraph.Count);
        Globals.Log("outputDataStructureUse(): seaNodesGraph=" + seaNodesGraph.Count);
        Globals.Log("outputDataStructureUse(): landNodesGraph=" + landNodesGraph.Count);

    }

}
