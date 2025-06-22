using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class CaveMeshGenerator : MonoBehaviour, ITerrainGenerator
{

    [SerializeField] private CaveGenerator caveGenerator; // Reference to the cave generator for map generation
    [SerializeField] private BotiExitPlacer exitPlacer;
    [Header("Dummy Map Settings")]
    public int width = 10;
    public int height = 10;
    [Header("Mesh Settings")]
    public float tileSize = 2f;
    public float wallHeight = 5f;
    public float vertexSpacing = 0.33f; // Spacing multiplier inside each tile (1/3rd)

    [Header("Generation")]
    public bool generateOnStart = true;
    public bool raiseCorners = true;

    private int[,] map;


    void Start()
    {
        if (generateOnStart)
        {
            Generate();
        }
    }

    public void Generate()
    {
        if (caveGenerator != null)
        {
            map = caveGenerator.GetMap();
        }
        if (map != null)
            GenerateMesh();
        exitPlacer.PlaceExit(map, this);
    }
    public void Generate(int seed)
    {
        if (caveGenerator != null)
        {
            map = caveGenerator.GetMap(seed);
        }
        if (map != null)
            GenerateMesh();
        exitPlacer.PlaceExit(map, this);
    }
    public void GenerateDummyMap()
    {
        map = new int[width, height];
        // Randomly fill with walls (for testing)
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                bool edge = x == 0 || y == 0 || x == width - 1 || y == height - 1;
                map[x, y] = edge ? 1 : (Random.value < 0.4f ? 1 : 0);
            }
        }
    }
    public void GenerateMesh()
    {
        width = map.GetLength(0);
        height = map.GetLength(1);

        int vertCountX = width * 3 + 1;
        int vertCountY = height * 3 + 1;

        Vector3[] vertices = new Vector3[vertCountX * vertCountY];
        Vector2[] uvs = new Vector2[vertices.Length];
        List<int> triangles = new();

        // Step 1: Calculate vertex positions
        for (int y = 0; y < vertCountY; y++)
        {
            for (int x = 0; x < vertCountX; x++)
            {
                float worldX = x * tileSize * vertexSpacing;
                float worldZ = y * tileSize * vertexSpacing;

                float height = GetVertexHeight(x, y);
                vertices[y * vertCountX + x] = new Vector3(worldX, height, worldZ);
                uvs[y * vertCountX + x] = new Vector2((float)x / vertCountX, (float)y / vertCountY);
            }
        }

        // Step 2: Create triangles
        for (int y = 0; y < vertCountY - 1; y++)
        {
            for (int x = 0; x < vertCountX - 1; x++)
            {
                int i00 = y * vertCountX + x;
                int i10 = y * vertCountX + x + 1;
                int i01 = (y + 1) * vertCountX + x;
                int i11 = (y + 1) * vertCountX + x + 1;

                // Get vertex positions
                Vector3 vA = vertices[i00];
                Vector3 vB = vertices[i10];
                Vector3 vC = vertices[i01];
                Vector3 vD = vertices[i11];

                float flatnessThreshold = 0.01f;
                float maxDelta = Mathf.Max(
                    Mathf.Abs(vA.y - vB.y),
                    Mathf.Abs(vA.y - vC.y),
                    Mathf.Abs(vA.y - vD.y)
                );

                if (maxDelta < flatnessThreshold)
                {
                    // Consistent split for visually flat quads (e.g., top-left to bottom-right)
                    triangles.Add(i00); triangles.Add(i11); triangles.Add(i10);
                    triangles.Add(i00); triangles.Add(i01); triangles.Add(i11);
                }
                else
                {
                    float diagAC = Mathf.Abs(vA.y - vD.y);
                    float diagBD = Mathf.Abs(vB.y - vC.y);

                    if (diagAC <= diagBD)
                    {
                        triangles.Add(i00); triangles.Add(i11); triangles.Add(i10);
                        triangles.Add(i00); triangles.Add(i01); triangles.Add(i11);
                    }
                    else
                    {
                        triangles.Add(i00); triangles.Add(i01); triangles.Add(i10);
                        triangles.Add(i10); triangles.Add(i01); triangles.Add(i11);
                    }

                }
            }
        }

        // Step 3: Build mesh
        Mesh mesh = new Mesh();
        mesh.name = "CaveMesh";
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        // Step 4: Apply to components
        var mf = GetComponent<MeshFilter>();
        var mr = GetComponent<MeshRenderer>();
        var mc = GetComponent<MeshCollider>();

        mf.sharedMesh = mesh;
        mc.sharedMesh = mesh;

        // Assign a default material if none is set
        if (mr.sharedMaterial == null)
        {
            mr.sharedMaterial = new Material(Shader.Find("Standard"));
        }
    }

    private float GetVertexHeight(int vx, int vy)
    {
        // Translate vertex into logical tile and sub-tile index
        int tileX = vx / 3;
        int tileY = vy / 3;

        if (tileX < 0 || tileY < 0 || tileX >= width || tileY >= height)
            return wallHeight;

        int localX = vx % 3;
        int localY = vy % 3;

        bool isWall = map[tileX, tileY] == 1;
        float h = 0f;

        // Middle vertices (center 4) – always raised if wall
        if (isWall && localX >= 1 && localY >= 1)
            return wallHeight;

        // Edge vertices – raise if adjacent wall
        if (localX >= 1 && (localY == 0))
        {
            int dy = -1;
            if ((InMap(tileX, tileY + dy) && map[tileX, tileY + dy] == 1 && isWall) ||
                (!InMap(tileX, tileY + dy) && isWall))
                return wallHeight;
        }
        if (localY >= 1 && (localX == 0))
        {
            int dx = -1;
            if ((InMap(tileX + dx, tileY) && map[tileX + dx, tileY] == 1 && isWall) ||
                (!InMap(tileX + dx, tileY) && isWall))
                return wallHeight;
        }

        // Corner vertices – raise only if surrounded by walls
        if ((localX == 0) && (localY == 0))
        {
            int dx = -1;
            int dy = -1;

            int surroundingWalls = 0;
            if ((InMap(tileX + dx, tileY) && map[tileX + dx, tileY] == 1) || !InMap(tileX + dx, tileY)) surroundingWalls++;
            if ((InMap(tileX, tileY + dy) && map[tileX, tileY + dy] == 1) || !InMap(tileX, tileY + dy)) surroundingWalls++;
            if ((InMap(tileX + dx, tileY + dy) && map[tileX + dx, tileY + dy] == 1) || !InMap(tileX + dx, tileY + dy)) surroundingWalls++;
            if (isWall) surroundingWalls++;

            if (surroundingWalls > 2 && raiseCorners || surroundingWalls == 4)
                return wallHeight;
        }

        return h;
    }

    private bool InMap(int x, int y)
    {
        return x >= 0 && y >= 0 && x < width && y < height;
    }
    public Vector3 GetTileCenterWorldPosition(int tileX, int tileY)
    {
        float x = tileX * tileSize + tileSize / 2f;
        float z = tileY * tileSize + tileSize / 2f;
        return new Vector3(x, 0, z) + transform.position;
    }
}
