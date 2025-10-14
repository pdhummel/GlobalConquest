using System.Text.Json;

namespace GlobalConquest;

public class Map
{
    public MapHex[] FlattenedHexes { get; set; }
    public MapHex[,] Hexes { get; set; }
    public int Y { get; set; }
    public int X { get; set; }

    public Map()
    {
    }

    public Map(int y, int x)
    {
        Y = y;
        X = x;
        Hexes = generateMap(y, x);
        FlattenedHexes = Utilities.FlattenArray(Hexes);
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
        Hexes[y, x].setUnit(unit);
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

}
