using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CaveLayoutGenerator: CaveGenerator
{
    public int width = 30;
    public int height = 30;
    public int[,] map;

    public int roomAttempts = 50;
    public int minRoomSize = 3;
    public int maxRoomSize = 7;

    private List<RectInt> rooms = new();

    void Awake()
    {
        map = new int[width, height];

        // Fill entire map with walls
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                map[x, y] = 1;
    }

    public override int[,] GetMap()
    {
        rooms.Clear();
        GenerateRooms();
        ConnectRooms();
        return map;
    }

    void GenerateRooms()
    {
        for (int i = 0; i < roomAttempts; i++)
        {
            int w = Random.Range(minRoomSize, maxRoomSize + 1);
            int h = Random.Range(minRoomSize, maxRoomSize + 1);
            int x = Random.Range(1, width - w - 1);
            int y = Random.Range(1, height - h - 1);

            RectInt newRoom = new RectInt(x, y, w, h);

            bool overlaps = false;
            foreach (var room in rooms)
            {
                if (newRoom.Overlaps(room))
                {
                    overlaps = true;
                    break;
                }
            }

            if (!overlaps)
            {
                rooms.Add(newRoom);
                CarveRoom(newRoom);
            }
        }
    }

    void CarveRoom(RectInt room)
    {
        for (int x = room.xMin; x < room.xMax; x++)
        {
            for (int y = room.yMin; y < room.yMax; y++)
            {
                map[x, y] = 0;
            }
        }
    }

    void ConnectRooms()
    {
        for (int i = 1; i < rooms.Count; i++)
        {
            Vector2Int a = RoomCenter(rooms[i - 1]);
            Vector2Int b = RoomCenter(rooms[i]);

            // Connect horizontally, then vertically
            if (Random.value < 0.5f)
            {
                CarveHorizontalTunnel(a.x, b.x, a.y);
                CarveVerticalTunnel(a.y, b.y, b.x);
            }
            else
            {
                CarveVerticalTunnel(a.y, b.y, a.x);
                CarveHorizontalTunnel(a.x, b.x, b.y);
            }
        }
    }

    void CarveHorizontalTunnel(int x1, int x2, int y)
    {
        for (int x = Mathf.Min(x1, x2); x <= Mathf.Max(x1, x2); x++)
        {
            if (InBounds(x, y)) map[x, y] = 0;
        }
    }

    void CarveVerticalTunnel(int y1, int y2, int x)
    {
        for (int y = Mathf.Min(y1, y2); y <= Mathf.Max(y1, y2); y++)
        {
            if (InBounds(x, y)) map[x, y] = 0;
        }
    }

    Vector2Int RoomCenter(RectInt room)
    {
        return new Vector2Int(
            room.xMin + room.width / 2,
            room.yMin + room.height / 2
        );
    }

    bool InBounds(int x, int y)
    {
        return x >= 0 && y >= 0 && x < width && y < height;
    }
}
