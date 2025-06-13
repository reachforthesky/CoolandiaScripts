using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class TileMapData
{
    public int width;
    public int length;
    public TileData[,] tiles;
    public List<PlateauData> plateaus = new();

    public TileMapData(int width, int length)
    {
        this.width = width;
        this.length = length;
        tiles = new TileData[width, length];
    }
}