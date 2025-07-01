using UnityEngine;

[CreateAssetMenu(fileName = "HeightMapGenerator", menuName = "Procedural/HeightMapGenerator")]
public class HeightMapGenerator : ScriptableObject
{
    [Header("Heightmap Settings")]
    public int width = 10;
    public int length = 10;
    public int maxSteps = 4;
    public float noiseScale = 0.1f;

    [Header("Seed")]
    public int seed = 0;

    [HideInInspector]
    public int[,] map;

    public void GenerateHeightMap()
    {
        map = new int[width, length];
        var prng = new System.Random(seed);
        int noiseSeed = prng.Next(1000);

        for (int z = 0; z < length; z++)
        {
            for (int x = 0; x < width; x++)
            {
                float noise = Mathf.PerlinNoise(
                    (x + noiseSeed) * noiseScale,
                    (z + noiseSeed) * noiseScale);

                int stepped = Mathf.FloorToInt(noise * maxSteps);
                map[x, z] = stepped;
            }
        }
    }
}
