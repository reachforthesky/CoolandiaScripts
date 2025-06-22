using System.Collections.Generic;
using UnityEngine;

public class CaveGeneratorCellularAutomata: CaveGenerator
{
    [Header("Generation Settings")]
    public int width = 50;
    public int height = 50;
    [Range(0f, 1f)]
    public float initialWallChance = 0.45f;
    public int smoothingIterations = 5;
    //[SerializeField] private CaveMeshGenerator meshGen;


    [Header("Tile Settings")]
    public float tileSize = 4f;

    private int[,] map;

    public override int[,] GetMap()
    {
        if (map == null)
        {
            GenerateMap();
            for (int i = 0; i < smoothingIterations; i++)
                SmoothMap();
            ConnectCaveRegions();
        }
        return map;
    }
    public override int[,] GetMap(int seed)
    {
        if (map == null)
        {
            GenerateMap(seed);
            for (int i = 0; i < smoothingIterations; i++)
                SmoothMap();
            ConnectCaveRegions();
        }
        return map;
    }
    void GenerateMap()
    {
        map = new int[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                bool isEdge = (x == 0 || y == 0 || x == width - 1 || y == height - 1);
                map[x, y] = isEdge ? 1 : (Random.value < initialWallChance ? 1 : 0);
            }
        }
    }
    void GenerateMap(int seed)
    {
        map = new int[width, height];

        System.Random rng = new System.Random(seed);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                bool isEdge = (x == 0 || y == 0 || x == width - 1 || y == height - 1);

                map[x, y] = isEdge ? 1 : (rng.NextDouble() < initialWallChance ? 1 : 0);
            }
        }
    }

    void SmoothMap()
    {
        int[,] newMap = new int[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int neighbors = CountWallNeighbors(x, y);
                if (neighbors > 4)
                    newMap[x, y] = 1;
                else if (neighbors < 4)
                    newMap[x, y] = 0;
                else
                    newMap[x, y] = map[x, y];
            }
        }

        map = newMap;
    }

    int CountWallNeighbors(int x, int y)
    {
        int count = 0;
        for (int nx = x - 1; nx <= x + 1; nx++)
        {
            for (int ny = y - 1; ny <= y + 1; ny++)
            {
                if (nx == x && ny == y) continue;

                if (nx < 0 || ny < 0 || nx >= width || ny >= height)
                    count++;
                else if (map[nx, ny] == 1)
                    count++;
            }
        }
        return count;
    }
    public void ConnectCaveRegions()
    {
        int width = map.GetLength(0);
        int height = map.GetLength(1);
        int[,] regionMap = new int[width, height];
        int currentRegion = 2; // Start labeling from 2 to avoid 0 and 1

        List<List<Vector2Int>> regions = new();

        // Step 1: Flood fill regions
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (map[x, y] == 0 && regionMap[x, y] == 0)
                {
                    var region = FloodFill(map, regionMap, x, y, currentRegion);
                    regions.Add(region);
                    currentRegion++;
                }
            }
        }

        if (regions.Count <= 1) return;

        // Step 2: Find largest region
        regions.Sort((a, b) => b.Count.CompareTo(a.Count)); // largest first
        var mainRegion = regions[0];

        // Step 3: Connect each other region to the main region
        for (int i = 1; i < regions.Count; i++)
        {
            Vector2Int bestA = Vector2Int.zero;
            Vector2Int bestB = Vector2Int.zero;
            float bestDist = float.MaxValue;

            foreach (var a in mainRegion)
            {
                foreach (var b in regions[i])
                {
                    float dist = Vector2Int.Distance(a, b);
                    if (dist < bestDist)
                    {
                        bestDist = dist;
                        bestA = a;
                        bestB = b;
                    }
                }
            }

            // Carve tunnel between bestA and bestB
            CarveTunnel(ref map, bestA, bestB);
            mainRegion.AddRange(regions[i]);
        }
    }
}
