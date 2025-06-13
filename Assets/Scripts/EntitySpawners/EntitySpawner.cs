using UnityEngine;
using System.Collections.Generic;

public class EntitySpawner : MonoBehaviour
{
    [Header("Entity Settings")]
    public GameObject entityPrefab; // Billboard sprite prefab
    public TerrainGenerator terrainGenerator;
    public float entityNoiseScale = 0.05f;
    public float entityThreshold = 0.6f;
    public int seed = 42;
    public float clusterNoiseScale = 0.01f; // For clustered patches
    public float clusterStrength = 0.4f;    // Mix-in strength for secondary clustering
    public float spriteYOffset = 0.5f;

    private System.Random rng;

    [ButtonInvoke("GenerateTerrain", displayIn: ButtonInvoke.DisplayIn.PlayAndEditModes)] public bool generateTerrain;
    public void SpawnEntities(TileMapData mapData)
    {
        ClearExistingEntities();
        rng = new System.Random(seed);
        TileMapData map = mapData;
        var tileSize = terrainGenerator.tileSize;

        for (int z = 0; z < map.length; z++)
        {
            for (int x = 0; x < map.width; x++)
            {
                TileData tile = map.tiles[x, z];

                if (tile == null || tile.slope > 0.05f) continue; // Only on flat(ish) tiles

                float baseNoise = Mathf.PerlinNoise((x + seed) * entityNoiseScale, (z + seed) * entityNoiseScale);
                float clusterNoise = Mathf.PerlinNoise((x + seed) * clusterNoiseScale, (z + seed) * clusterNoiseScale);

                float combinedNoise = Mathf.Lerp(baseNoise, clusterNoise, clusterStrength);

                if (combinedNoise > entityThreshold)
                {
                    float randX01 = (float)rng.NextDouble();
                    float randZ01 = (float)rng.NextDouble();

                    float randOffsetX = Mathf.Lerp(-tileSize / 2f + 0.1f, tileSize / 2f - 0.1f, randX01);
                    float randOffsetZ = Mathf.Lerp(-tileSize / 2f + 0.1f, tileSize / 2f - 0.1f, randZ01);

                    Vector3 pos = new Vector3(
                        x * tileSize + tileSize / 2f + randOffsetX,
                        tile.maxHeight + spriteYOffset,
                        z * tileSize + tileSize / 2f + randOffsetZ
                    );

                    GameObject entity = Instantiate(entityPrefab, pos, Quaternion.identity, this.transform);
                }
            }
        }
    }

    public void GenerateTerrain()
    {
        terrainGenerator.GenerateTerrain();
    }

    void ClearExistingEntities()
    {
        // Cache children before modifying the hierarchy
        List<GameObject> toDestroy = new List<GameObject>();
        foreach (Transform child in transform)
        {
            toDestroy.Add(child.gameObject);
        }

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            foreach (GameObject go in toDestroy)
            {
                DestroyImmediate(go);
            }
        }
        else
        {
            foreach (GameObject go in toDestroy)
            {
                Destroy(go);
            }
        }
#else
    foreach (GameObject go in toDestroy)
    {
        Destroy(go);
    }
#endif
    }
}
