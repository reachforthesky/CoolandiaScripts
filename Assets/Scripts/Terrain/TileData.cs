using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Rendering;

public enum TileType { Grass, Water, Sand }
public class TileData
{
    public Vector2Int gridPosition;
    public float averageHeight;
    public float[] cornerHeights;
    public float minHeight;
    public float maxHeight;
    public TileType tileType;
    public float slope { get { return maxHeight - minHeight; } }
    public bool isRamp;
}
