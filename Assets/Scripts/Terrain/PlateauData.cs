using System.Collections.Generic;
using UnityEngine;

public class PlateauData
{
    public int heightLevel;
    public List<Vector2Int> surfaceTiles = new ();
    public List<Vector2Int> edgeTiles = new();
    public Color gizmoColor;

    public PlateauData(int height)
    {
        this.heightLevel = height;
        gizmoColor = GetRandomColor();
    }

    Color GetRandomColor()
    {
        return new Color(Random.value, Random.value, Random.value, 1f);
    }
}