using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class SubdividedTerrainMeshGenerator : MonoBehaviour
{
    [Header("Heightmap Source")]
    public HeightMapGenerator heightMapGenerator;

    [Header("Tile Settings")]
    public float tileSize = 1f;
    [Range(1, 11)] public int segmentsPerTileSide = 3; // Must be odd!

    [Header("Height Settings")]
    public float baseHeight = 0f;
    public float maxHeight = 5f;

    private void Start()
    {
        if (heightMapGenerator == null)
        {
            Debug.LogError("No HeightMapGenerator assigned!");
            return;
        }

        heightMapGenerator.GenerateHeightMap();
        GenerateMesh();
    }

    void GenerateMesh()
    {
        int[,] map = heightMapGenerator.map;
        int width = heightMapGenerator.width;
        int length = heightMapGenerator.length;
        int maxSteps = heightMapGenerator.maxSteps;

        int vertCountX = width * segmentsPerTileSide + 1;
        int vertCountZ = length * segmentsPerTileSide + 1;

        Vector3[] vertices = new Vector3[vertCountX * vertCountZ];
        Vector2[] uvs = new Vector2[vertices.Length];

        for (int z = 0; z < vertCountZ; z++)
        {
            for (int x = 0; x < vertCountX; x++)
            {
                float worldX = (x / (float)segmentsPerTileSide) * tileSize;
                float worldZ = (z / (float)segmentsPerTileSide) * tileSize;

                float height = CalculateVertexHeight(x, z, map, width, length, maxSteps);
                vertices[z * vertCountX + x] = new Vector3(worldX, height, worldZ);

                uvs[z * vertCountX + x] = new Vector2((float)x / vertCountX, (float)z / vertCountZ);
            }
        }

        List<int> triangles = new List<int>();

        for (int z = 0; z < vertCountZ - 1; z++)
        {
            for (int x = 0; x < vertCountX - 1; x++)
            {
                int i00 = z * vertCountX + x;
                int i10 = z * vertCountX + x + 1;
                int i01 = (z + 1) * vertCountX + x;
                int i11 = (z + 1) * vertCountX + x + 1;

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
                    // If very flat, use consistent split
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


        Mesh mesh = new Mesh();
        mesh.name = "SubdividedTerrain";
        mesh.vertices = vertices;
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs;
        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().sharedMesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    float CalculateVertexHeight(int vx, int vz, int[,] map, int width, int length, int maxSteps)
    {
        float stepSize = maxHeight / maxSteps;
        int tileX = vx / segmentsPerTileSide;
        int tileZ = vz / segmentsPerTileSide;

        // Clamp to make sure we don't run off the map edges
        tileX = Mathf.Clamp(tileX, 0, width - 1);
        tileZ = Mathf.Clamp(tileZ, 0, length - 1);

        int localX = vx % segmentsPerTileSide;
        int localZ = vz % segmentsPerTileSide;

        int center = map[tileX, tileZ];
        float trueHeight = baseHeight + (center / (float)maxSteps) * maxHeight;

        if (localX >= 1 && localZ >= 1)
        {
            // CENTER vertex
            return trueHeight;
        }
        else if (localX == 0 && localZ == 0)
        {
            // CORNER vertex — check 3 neighbors
            int adjX = tileX - 1;
            int adjZ = tileZ - 1;

            int adj1 = center, adj2 = center, adj3 = center, adj4 = center;
            if (InMap(tileX, adjZ, width, length)) adj1 = map[tileX, adjZ];
            if (InMap(adjX, tileZ, width, length)) adj2 = map[adjX, tileZ];
            if (InMap(adjX, adjZ, width, length)) adj3 = map[adjX, adjZ];
            if (InMap(tileX, tileZ, width, length)) adj4 = map[tileX, tileZ];

            int matches = 0;
            if (adj1 >= center) matches++;
            if (adj2 >= center) matches++;
            if (adj3 >= center) matches++;
            if (adj4 >= center) matches++;

            return matches >= 3 ? trueHeight : trueHeight - stepSize;
        }
        else
        {
            // EDGE vertex — check 2 neighbors
            int adjX = tileX;
            int adjZ = tileZ;

            if (localX == 0) adjX -= 1;  // on X edge -> neighbor left
            if (localZ == 0) adjZ -= 1;  // on Z edge -> neighbor up

            int adj = center;
            if (InMap(adjX, adjZ, width, length))
            {
                if (localX == 0) adj = map[adjX, tileZ];
                if (localZ == 0) adj = map[tileX, adjZ];
            }

            return adj >= center ? trueHeight : trueHeight - stepSize;
        }
    }


    bool InMap(int x, int z, int width, int length)
    {
        return x >= 0 && z >= 0 && x < width && z < length;
    }
}
