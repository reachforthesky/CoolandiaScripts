using System.Collections.Generic;
using UnityEngine;

public class CaveGenerator : MonoBehaviour
{
    public virtual int[,] GetMap()
    {
        return new int[0, 0]; // Default implementation returns an empty map
    }
    public virtual int[,] GetMap(int seed)
    {
        return new int[0, 0]; // Default implementation returns an empty map
    }
    protected static List<Vector2Int> FloodFill(int[,] map, int[,] regionMap, int startX, int startY, int regionId)
    {
        List<Vector2Int> region = new();
        Queue<Vector2Int> queue = new();
        queue.Enqueue(new Vector2Int(startX, startY));
        regionMap[startX, startY] = regionId;

        Vector2Int[] directions = {
        Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
    };

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            region.Add(current);

            foreach (var dir in directions)
            {
                int nx = current.x + dir.x;
                int ny = current.y + dir.y;

                if (nx >= 0 && ny >= 0 && nx < map.GetLength(0) && ny < map.GetLength(1))
                {
                    if (map[nx, ny] == 0 && regionMap[nx, ny] == 0)
                    {
                        regionMap[nx, ny] = regionId;
                        queue.Enqueue(new Vector2Int(nx, ny));
                    }
                }
            }
        }

        return region;
    }
    protected static void CarveTunnel(ref int[,] map, Vector2Int from, Vector2Int to)
    {
        Vector2Int current = from;

        while (current.x != to.x)
        {
            map[current.x, current.y] = 0;
            current.x += to.x > current.x ? 1 : -1;
        }

        while (current.y != to.y)
        {
            map[current.x, current.y] = 0;
            current.y += to.y > current.y ? 1 : -1;
        }

        map[current.x, current.y] = 0;
    }

}
