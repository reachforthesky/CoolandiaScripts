using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class BotiExitPlacer : MonoBehaviour
{
    [SerializeField] private GameObject exitPrefab;
    [SerializeField] private PlacementMode placementMode = PlacementMode.CenterOfMap;
    private int prefabIndex = -1;


    void Start()
    {
        prefabIndex = PersistentEntityManager.Instance.FindIndex(exitPrefab);
    }
    public enum PlacementMode
    {
        FirstOpenTile,
        CenterOfMap,
        FurthestFromOrigin,
    }
    public void PlaceExit(int[,] map, CaveMeshGenerator meshGenerator)
    {
        Vector2Int? tilePos = placementMode switch
        {
            PlacementMode.FirstOpenTile => FindFirstOpenTile(map),
            PlacementMode.CenterOfMap => FindCenterOpenTile(map),
            PlacementMode.FurthestFromOrigin => FindFurthestFromOrigin(map, Vector2Int.zero),
            _ => null
        };

        if (tilePos == null)
        {
            Debug.LogWarning("No valid tile found for exit placement.");
            return;
        }

        Vector3 worldPos = meshGenerator.GetTileCenterWorldPosition(tilePos.Value.x, tilePos.Value.y);

        StartCoroutine(SpawnExitCoroutine(worldPos + Vector3.up * 1f, (trigger) =>
        {
            Debug.Log("Exit trigger spawned: " + trigger.name);
        }));
    }

    private Vector2Int? FindFirstOpenTile(int[,] map)
    {
        for (int x = 0; x < map.GetLength(0); x++)
            for (int y = 0; y < map.GetLength(1); y++)
                if (map[x, y] == 0) return new Vector2Int(x, y);
        return null;
    }

    private Vector2Int? FindCenterOpenTile(int[,] map)
    {
        int cx = map.GetLength(0) / 2;
        int cy = map.GetLength(1) / 2;

        // Spiral out from center until floor found
        for (int radius = 0; radius < Mathf.Max(cx, cy); radius++)
        {
            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    int x = cx + dx;
                    int y = cy + dy;
                    if (x >= 0 && y >= 0 && x < map.GetLength(0) && y < map.GetLength(1) && map[x, y] == 0)
                        return new Vector2Int(x, y);
                }
            }
        }

        return null;
    }

    private Vector2Int? FindFurthestFromOrigin(int[,] map, Vector2Int origin)
    {
        Vector2Int furthest = origin;
        float maxDist = -1f;

        for (int x = 0; x < map.GetLength(0); x++)
        {
            for (int y = 0; y < map.GetLength(1); y++)
            {
                if (map[x, y] == 0)
                {
                    float dist = Vector2Int.Distance(origin, new Vector2Int(x, y));
                    if (dist > maxDist)
                    {
                        maxDist = dist;
                        furthest = new Vector2Int(x, y);
                    }
                }
            }
        }

        return maxDist > 0 ? furthest : (Vector2Int?)null;
    }


    public IEnumerator SpawnExitCoroutine(Vector3 position, System.Action<GameObject> onSpawned)
    {
        // Wait until PEM is ready
        yield return new WaitUntil(() => PersistentEntityManager.Instance != null && PersistentEntityManager.Instance.IsSpawned);

        var data = new PersistentEntityData
        {
            prefabId = prefabIndex,
            position = position,
            rotation = Quaternion.identity,
            isDestroyed = false
        };

        var exitTrigger = PersistentEntityManager.Instance.RegisterEntity(data, transform);

        if (exitTrigger != null)
            //exitTrigger.GetComponent<NetworkObject>().TrySetParent(transform, false);

            // Return via callback
            onSpawned?.Invoke(exitTrigger);
    }
}
