using System.Collections.Generic;
using UnityEngine;

public class DFSMazeGenerator : CaveGenerator
{
    public int width = 30;
    public int height = 30;
    public int seed = 123;
    public override int[,] GetMap()
    {
        System.Random rng = new System.Random(seed);
        // Ensure odd dimensions for proper maze
        if (width % 2 == 0) width += 1;
        if (height % 2 == 0) height += 1;

        int[,] map = new int[width, height];

        // Fill map with walls
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                map[x, y] = 1;

        Stack<Vector2Int> stack = new();
        Vector2Int start = new Vector2Int(1, 1);
        map[start.x, start.y] = 0;
        stack.Push(start);

        Vector2Int[] directions = {
            new Vector2Int(0, 2),
            new Vector2Int(2, 0),
            new Vector2Int(0, -2),
            new Vector2Int(-2, 0),
        };

        while (stack.Count > 0)
        {
            Vector2Int current = stack.Peek(); // Don't pop yet
            Shuffle(directions, rng);

            bool moved = false;
            foreach (var dir in directions)
            {
                Vector2Int next = current + dir;
                if (IsInBounds(next, width, height) && map[next.x, next.y] == 1)
                {
                    Vector2Int between = current + dir / 2;
                    map[between.x, between.y] = 0;
                    map[next.x, next.y] = 0;
                    stack.Push(next);
                    moved = true;
                    break; // Only move to one neighbor
                }
            }
            if (!moved)
            {
                stack.Pop(); // Backtrack if no moves
            }
        }
        Debug.Log(MapToString(map));
        return map;
    }

    private bool IsInBounds(Vector2Int pos, int width, int height)
    {
        return pos.x > 0 && pos.x < width - 1 && pos.y > 0 && pos.y < height - 1;
    }

    private void Shuffle(Vector2Int[] array, System.Random rng)
    {
        for (int i = 0; i < array.Length; i++)
        {
            int j = rng.Next(i, array.Length);
            (array[i], array[j]) = (array[j], array[i]);
        }
    }

    private string MapToString(int[,] map)
    {
        string result = "";
        for (int y = 0; y < map.GetLength(1); y++)
        {
            for (int x = 0; x < map.GetLength(0); x++)
            {
                result += map[x, y] == 1 ? "#" : "O";
            }
            result += "\n";
        }
        return result;
    }
}
