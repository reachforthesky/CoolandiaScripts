using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;
using System;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;
using System.Linq;
using static UnityEditor.Searcher.SearcherWindow.Alignment;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using Unity.Netcode;
using System.Collections;









#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class TerrainGenerator : NetworkBehaviour
{
    public enum gizmoType { standard, plateau, ramp, none }

    [Header("GameObjects")]
    public List<EntitySpawner> entitySpawners = new List<EntitySpawner>();
    //public EntitySpawner TreeSpawner;
    //public EntitySpawner RockSpawner;

    [Header("Tile Settings")]
    public float tileSize = 1f; // default 1m x 1m tiles

    [Header("Terrain Settings")]
    public int seed = 0;
    public int mapWidth = 100;
    public int mapLength = 100;
    public float noiseScale = 0.1f;
    public int maxSteps = 4;
    [Range(1f, 10f)] public float hillSharpness = 4f;
    [Range(0f, 5f)] public float baseHeight = 0.3f;
    public float slopeSensitivity = 1f;
    public float maxHeight = 5f;

    [Header("Ramp Carving")]
    public int rampFrequency = 2;
    public int rampLength = 3; 
    public int sideSmoothRadius = 3; // Number of tiles to each side to smooth
    public float sideFalloff = 0.25f; // Smoothing strength for side tiles
    public float rampWidthFactor = 3f;

    [ButtonInvoke("GenerateTerrain", displayIn:ButtonInvoke.DisplayIn.PlayAndEditModes)] public bool generateTerrain;

    [Header("Gizmo Settings")]
    public gizmoType currentType = TerrainGenerator.gizmoType.standard;

    private TileMapData mapData;
    private float stepHeight { get { return maxHeight/maxSteps; } }
    private System.Random seededRandom;
    private Vector3[] vertices;

    void Start()
    {
    }

    private void OnEnable()
    {
    #if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            GenerateTerrain(); // regenerate for editor gizmos
        }
    #endif
    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        GenerateTerrain();
    }
    
    public void GenerateTerrain()
    {
        if (!IsServer) return;
        seededRandom = new System.Random(seed);
        mapData = new TileMapData(mapWidth, mapLength);

        vertices = new Vector3[(mapWidth + 1) * (mapLength + 1)];
        int[] triangles = new int[mapWidth * mapLength * 6];
        Color[] colors = new Color[vertices.Length];
        int[] colorWriteCounts = new int[triangles.Length];
        Vector2[] uvs = new Vector2[vertices.Length];
        Dictionary<Vector2Int, PlateauData> tileToPlateau = new();

        // Generate vertices
        GenerateVertices(triangles, uvs);

        //Calculate tile data
        CalulateTileData();

        CalculatePlateaus(tileToPlateau);
        CarveRampsBetweenPlateaus(mapData.plateaus, tileToPlateau);

        //Recalculate tile data
        CalulateTileData();
        Color[] finalColors = ColorVertices(colors, colorWriteCounts);

        Mesh mesh = new Mesh
        {
            vertices = vertices,
            triangles = triangles,
            uv = uvs,
            colors = finalColors,
        };
        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;

        StartCoroutine(WaitForPEMThenSpawn());
    }
    private IEnumerator WaitForPEMThenSpawn()
    {
        // Wait until PEM is spawned
        while (PersistentEntityManager.Instance == null || !PersistentEntityManager.Instance.IsSpawned)
        {
            yield return null;
        }

        // Now safe to spawn scenery
        foreach (var spawner in entitySpawners)
        {
            spawner.SpawnEntities(mapData);
        }
    }
    Color[] ColorVertices(Color[] colors, int[] colorWriteCounts)
    {
        for (int z = 0; z < mapLength; z++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                int index = x + z * (mapWidth + 1);

                float slope = mapData.tiles[x, z].slope;
                // Normalize slope (tweak slopeSensitivity to control visual output)
                float normalSlope = Mathf.Clamp01(slope * slopeSensitivity);

                // Map to color (e.g., green = flat, black = steep)
                Color slopeColor = Color.Lerp(new Color(0f,0f,0f), new Color(0f, 1f, 0f), normalSlope);

                if (mapData.tiles[x, z].isRamp)
                    slopeColor = new Color(0f, 0.5f, 0f);

                // Apply to 4 corner vertices of this tile

                AddColor(index, slopeColor);
                AddColor(index + 1, slopeColor);
                AddColor(index + mapWidth + 1, slopeColor);
                AddColor(index + mapWidth + 2, slopeColor);
            }
        }

        Color[] finalColors = new Color[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            finalColors[i] = colorWriteCounts[i] > 0 ? colors[i] / colorWriteCounts[i] : Color.white;
            Debug.Log($"Vertex {i} color: {finalColors[i].g} (writes: {colorWriteCounts[i]})");
        }

        return finalColors;

        void AddColor(int i, Color c)
        {
            colors[i] += c;
            colorWriteCounts[i]++;
        }
    }

    private void CalulateTileData()
    {
        for (int z = 0; z < mapLength; z++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                TileData tile = mapData.tiles[x, z];
                if (tile == null)
                    tile = new TileData();

                int index = x + z * (mapWidth + 1);

                float bl = vertices[index].y;
                float br = vertices[index + 1].y;
                float tl = vertices[index + mapWidth + 1].y;
                float tr = vertices[index + mapWidth + 2].y;

                float avg = (bl + br + tl + tr) / 4f;

                tile.gridPosition = new Vector2Int(x, z);
                tile.averageHeight = avg;
                tile.cornerHeights = new float[] { bl, br, tl, tr };
                tile.minHeight = Mathf.Min(tile.cornerHeights);
                tile.maxHeight = Mathf.Max(tile.cornerHeights);

                mapData.tiles[x, z] = tile;
            }
        }
    }

    private void CalculatePlateaus(Dictionary<Vector2Int, PlateauData> tileToPlateau)
    {
        bool[,] visited = new bool[mapWidth, mapLength];
        var plateaus = mapData.plateaus;

        for (int z = 0; z < mapLength; z++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                if (visited[x, z]) continue;

                var tile = mapData.tiles[x, z];
                int level = GetQuantizedHeight(tile); // i.e., Mathf.RoundToInt(tile.averageHeight / stepHeight)

                PlateauData plateau = new PlateauData(level);

                Queue<Vector2Int> queue = new();
                queue.Enqueue(new Vector2Int(x, z));

                while (queue.Count > 0)
                {
                    Vector2Int pos = queue.Dequeue();
                    int px = pos.x;
                    int pz = pos.y;

                    if (visited[px, pz]) continue;
                    visited[px, pz] = true;

                    plateau.surfaceTiles.Add(pos);

                    foreach (Vector2Int dir in FourDirections())
                    {
                        int nx = px + dir.x;
                        int nz = pz + dir.y;

                        if (!InBounds(nx, nz)) continue;

                        var neighbor = mapData.tiles[nx, nz];
                        int neighborLevel = GetQuantizedHeight(neighbor);

                        if (neighborLevel == level)
                        {
                            if (!visited[nx, nz])
                                queue.Enqueue(new Vector2Int(nx, nz));
                        }
                        else
                        {
                            // This is a border tile — we’ll carve ramps here later
                            plateau.edgeTiles.Add(new Vector2Int(px, pz));
                        }
                    }
                }
                foreach (var tilePos in plateau.surfaceTiles)
                {
                    tileToPlateau[tilePos] = plateau;
                }
                plateaus.Add(plateau);
            }
        }
    }
    void CarveRampsBetweenPlateaus(List<PlateauData> plateaus, Dictionary<Vector2Int, PlateauData> tileToPlateau)
    {
        foreach (var plateau in plateaus)
        {
            int rampsCreated = 0;
            int rampCount = Mathf.Max(Mathf.RoundToInt(plateau.surfaceTiles.Count / 500), 1) * rampFrequency;

            List<Vector2Int> edgeShuffled = plateau.edgeTiles.OrderBy(_ => seededRandom.Next()).ToList();
            HashSet<PlateauData> connectedLowerPlateaus = new();

            foreach (var tilePos in edgeShuffled)
            {
                if (rampsCreated >= rampCount && connectedLowerPlateaus.Count >= 1)
                    break;

                foreach (Vector2Int dir in FourDirections())
                {
                    Vector2Int neighborPos = tilePos + dir;

                    if (!InBounds(neighborPos.x, neighborPos.y)) continue;

                    var neighborTile = mapData.tiles[neighborPos.x, neighborPos.y];
                    int tileHeight = GetQuantizedHeight(mapData.tiles[tilePos.x, tilePos.y]);
                    int neighborHeight = GetQuantizedHeight(neighborTile);

                    if (tileHeight - neighborHeight == 1)
                    {
                        // Get the neighboring plateau
                        PlateauData neighborPlateau;
                        if (!tileToPlateau.TryGetValue(neighborPos, out neighborPlateau))
                            continue;

                        bool isRequiredRamp = !connectedLowerPlateaus.Contains(neighborPlateau);

                        if (rampsCreated < rampCount || isRequiredRamp)
                        {
                            // Carve the ramp
                            CarveRamp(tilePos, dir, rampLength, tileHeight, neighborHeight);

                            Vector2 startXZ = new Vector2(tilePos.x * tileSize + tileSize / 2f, tilePos.y * tileSize + tileSize / 2f);
                            Vector2 endXZ = new Vector2(
                                (tilePos + dir * rampLength).x * tileSize + tileSize / 2f,
                                (tilePos + dir * rampLength).y * tileSize + tileSize / 2f
                            );

                            float startH = plateau.heightLevel * stepHeight;
                            float endH = (plateau.heightLevel - 1) * stepHeight;
                            float rampWidth = tileSize * rampWidthFactor;

                            SmoothRampVertices(startXZ, endXZ, startH, endH, rampWidth);

                            connectedLowerPlateaus.Add(neighborPlateau);
                            rampsCreated++;
                            break;
                        }
                    }
                }
            }
        }
    }
    void CarveRamp(Vector2Int start, Vector2Int dir, int length, int fromLevel, int toLevel)
    {
        float startHeight = fromLevel * stepHeight;
        float endHeight = toLevel * stepHeight;

        Vector2Int left = new Vector2Int(-dir.y, dir.x);   // Perpendicular left

        for (int i = 0; i <= length; i++)
        {
            float t = (float)i / length;
            float centerHeight = Mathf.Lerp(startHeight, endHeight, t);

            Vector2Int center = start + dir * i;

            for (int offset = -sideSmoothRadius; offset <= sideSmoothRadius; offset++)
            {
                Vector2Int offsetPos = center + left * offset;

                if (!InBounds(offsetPos.x, offsetPos.y)) continue;

                float sideFactor = 1f - Mathf.Abs(offset) * sideFalloff;
                float smoothedHeight = Mathf.Lerp(GetQuantizedHeight(mapData.tiles[offsetPos.x, offsetPos.y]) * stepHeight, centerHeight, sideFactor);

                var tile = mapData.tiles[offsetPos.x, offsetPos.y];
                tile.averageHeight = smoothedHeight;
                tile.isRamp = true;

                // Update mesh vertices
                int vIndex = offsetPos.x + offsetPos.y * (mapData.width + 1);

                // Optional: protect against out-of-bounds
                if (vIndex < 0 || vIndex + mapData.width + 2 >= vertices.Length) continue;

                float finalHeight = smoothedHeight;
                vertices[vIndex].y = finalHeight;
                vertices[vIndex + 1].y = finalHeight;
                vertices[vIndex + mapData.width + 1].y = finalHeight;
                vertices[vIndex + mapData.width + 2].y = finalHeight;
            }
        }
    }
    void SmoothRampVertices(Vector2 startXZ, Vector2 endXZ, float startHeight, float endHeight, float rampWidthWorld)
    {
        Vector2 rampDir = (endXZ - startXZ).normalized;
        Vector2 rampPerp = new Vector2(-rampDir.y, rampDir.x); // perpendicular vector
        float rampLength = Vector2.Distance(startXZ, endXZ);

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 v = vertices[i];
            Vector2 vertexXZ = new Vector2(v.x, v.z);

            Vector2 toVertex = vertexXZ - startXZ;

            float along = Vector2.Dot(toVertex, rampDir);
            float across = Vector2.Dot(toVertex, rampPerp);

            if (along < 0f || along > rampLength || Mathf.Abs(across) > rampWidthWorld)
                continue; // Outside ramp zone

            float t = along / rampLength;
            float baseHeight = Mathf.Lerp(startHeight, endHeight, t);

            float sideFalloff = 1f - Mathf.Clamp01(Mathf.Abs(across) / rampWidthWorld); // 1 at center, 0 at edge
            float smoothed = Mathf.Lerp(v.y, baseHeight, sideFalloff);
            bool isNaN = float.IsNaN(smoothed);
            bool isInfinity = float.IsInfinity(smoothed);
            if (isNaN || isInfinity)
            {
                smoothed = baseHeight; // Fallback to safe value
            }

            v.y = smoothed;
            vertices[i] = v;
        }
    }
    private void GenerateVertices(int[] triangles, Vector2[] uvs)
    {
        var noiseSeed = seededRandom.Next(1000);
        for (int z = 0; z <= mapLength; z++)
        {
            for (int x = 0; x <= mapWidth; x++)
            {
                float rawNoise = Mathf.PerlinNoise((x + noiseSeed) * noiseScale, (z + noiseSeed) * noiseScale);
                rawNoise = Mathf.Clamp01(rawNoise);
                float powNoise = Mathf.Pow(rawNoise, hillSharpness);
                float stepped = Mathf.Floor(powNoise * maxSteps) / maxSteps;
                float height = baseHeight + stepped * (maxHeight - baseHeight);

                //bool isNaN = float.IsNaN(height);
                //bool isInfinity = float.IsInfinity(height);
                //if (isNaN || isInfinity)
                //{
                //    height = baseHeight; // Fallback to safe value
                //}

                int index = x + z * (mapWidth + 1);
                vertices[index] = new Vector3(x * tileSize, height, z * tileSize);
                uvs[index] = new Vector2((float)x / mapWidth, (float)z / mapLength);

                if (x < mapWidth && z < mapLength)
                {

                    int triIndex = (x + z * mapWidth) * 6;

                    triangles[triIndex + 0] = index;
                    triangles[triIndex + 1] = index + mapWidth + 1;
                    triangles[triIndex + 2] = index + 1;

                    triangles[triIndex + 3] = index + 1;
                    triangles[triIndex + 4] = index + mapWidth + 1;
                    triangles[triIndex + 5] = index + mapWidth + 2;
                }
            }
        }
    }

    bool InBounds(int x, int z)
    {
        return x >= 0 && x < mapData.width && z >= 0 && z < mapData.length;
    }

    IEnumerable<Vector2Int> FourDirections()
    {
        yield return Vector2Int.up;
        yield return Vector2Int.down;
        yield return Vector2Int.left;
        yield return Vector2Int.right;
    }

    int GetQuantizedHeight(TileData tile)
    {
        return Mathf.RoundToInt(tile.maxHeight / (maxHeight/maxSteps));
    }

    void OnDrawGizmos()
    {
        if (currentType == gizmoType.none)
        {
            return;
        }

        if (currentType == gizmoType.ramp)
        {
            drawRampGizmos();
        }
        if (mapData == null || mapData.tiles == null)
            return;

        List<TileData> allTiles = new();

        for (int z = 0; z < mapData.length; z++)
        {
            for (int x = 0; x < mapData.width; x++)
            {
                var tile = mapData.tiles[x, z];
                if (tile != null)
                    allTiles.Add(tile);
            }
        }

        // Sort so lower tiles are drawn first, higher ones later
        allTiles.Sort((a, b) => a.maxHeight.CompareTo(b.maxHeight));

        if (currentType == gizmoType.standard)
            drawStandardGizmos(allTiles);
        else if (currentType == gizmoType.plateau)
            drawPlateauGizmos();
    }

    private void drawRampGizmos()
    {
        for (int z = 0; z < mapData.length; z++)
        {
            for (int x = 0; x < mapData.width; x++)
            {
                var tile = mapData.tiles[x, z];

                if(tile == null)
                    continue;

                // Draw a flat cube slightly above the terrain to avoid z-fighting
                Vector3 center = new Vector3(
                    tile.gridPosition.x * tileSize + tileSize / 2f,
                    tile.maxHeight + 0.05f,
                    tile.gridPosition.y * tileSize + tileSize / 2f
                );
                Vector3 size = new Vector3(tileSize, 0.1f, tileSize);
                
                if(tile.isRamp)
                {
                    Gizmos.color = Color.black;
                    Gizmos.DrawCube(center, size);
                }
            }
        }
    }

    private void drawPlateauGizmos()
    {
        foreach( var plateau in mapData.plateaus)
        {

            foreach(var surfaceTile in plateau.surfaceTiles)
            {
                var tile = mapData.tiles[surfaceTile.x, surfaceTile.y];

                // Draw a flat cube slightly above the terrain to avoid z-fighting
                Vector3 center = new Vector3(
                    tile.gridPosition.x * tileSize + tileSize / 2f,
                    tile.maxHeight + 0.05f,
                    tile.gridPosition.y * tileSize + tileSize / 2f
                );
                Vector3 size = new Vector3(tileSize, 0.1f, tileSize);
                if (!plateau.edgeTiles.Contains(surfaceTile))
                {
                    Gizmos.color = plateau.gizmoColor;
                    Gizmos.DrawCube(center, size);
                }
                else
                {
                    Gizmos.color = Color.black;
                    Gizmos.DrawCube(center, size);
                }
            }
        }
    }

    private void drawStandardGizmos(List<TileData> allTiles)
    {
        foreach (var tile in allTiles)
        {
            if (tile == null)
                continue;

            // Choose color based on tile properties
            if (tile.slope > 0)
                Gizmos.color = Color.yellow;
            else if (tile.averageHeight < 1f)
                Gizmos.color = Color.blue;
            else if (tile.averageHeight < 5f)
                Gizmos.color = Color.green;
            else
                Gizmos.color = Color.red;


            // Draw a flat cube slightly above the terrain to avoid z-fighting
            Vector3 center = new Vector3(
                tile.gridPosition.x * tileSize + tileSize / 2f,
                tile.maxHeight + 0.05f,
                tile.gridPosition.y * tileSize + tileSize / 2f
            );
            Vector3 size = new Vector3(tileSize, 0.1f, tileSize);
            Gizmos.DrawCube(center, size);
        }
    }
}