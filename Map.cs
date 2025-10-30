using System.Text.Json;

namespace GlobalConquest;

public class Map
{
    public MapHex[] FlattenedHexes { get; set; }
    public MapHex[,] Hexes { get; set; }
    public int Y { get; set; }
    public int X { get; set; }
    public string VisibilityMode { get; set; }

    public Map()
    {
    }

    public Map(int y, int x)
    {
        Y = y;
        X = x;
        Hexes = generateMap(y, x);
        Burb amberMetro = new Burb();
        amberMetro.Name = "Amber Array";
        amberMetro.Type = "metro";
        amberMetro.Color = "amber";
        Burb ocherMetro = new Burb();
        ocherMetro.Name = "Ocher Order";
        ocherMetro.Type = "metro";
        ocherMetro.Color = "ocher";
        Burb magentaMetro = new Burb();
        magentaMetro.Name = "Magenta Mob";
        magentaMetro.Type = "metro";
        magentaMetro.Color = "magenta";
        Burb cyanMetro = new Burb();
        cyanMetro.Name = "Cyan Circle";
        cyanMetro.Type = "metro";
        cyanMetro.Color = "cyan";
        Burb capital = new Burb();
        capital.Name = "Washington";
        capital.Type = "capital";

        FlattenedHexes = Utilities.FlattenArray(Hexes);
        Hexes[0, 1].Burb = amberMetro;
        Hexes[0, 1].makeVisibleToAll();
        Hexes[1, x - 2].Burb = ocherMetro;
        Hexes[1, x - 2].makeVisibleToAll();
        Hexes[y - 2, 1].Burb = magentaMetro;
        Hexes[y - 2, 1].makeVisibleToAll();
        Hexes[y - 1, x - 2].Burb = cyanMetro;
        Hexes[y - 1, x - 2].makeVisibleToAll();
        Hexes[y / 2, x / 2].Burb = capital;
        
    }

    public MapHex[,] unflattenHexes()
    {
        return Utilities.UnflattenArray(FlattenedHexes, Y, X);
    }

    public MapHex[,] unflattenHexes(int rows, int cols)
    {
        return Utilities.UnflattenArray(FlattenedHexes, rows, cols);
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

    public void placeUnit(Unit unit, int x, int y)
    {
        if (x >= 0 && x < X && y >= 0 && y < Y)
        {
            Hexes[y, x].setUnit(unit);
            unit.X = x;
            unit.Y = y;
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
            targetMapHex.setUnit(unit);
            MapHex sourceMapHex = Hexes[unit.Y, unit.X];
            if (sourceMapHex.Units.Count > 0)
            {
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

}
