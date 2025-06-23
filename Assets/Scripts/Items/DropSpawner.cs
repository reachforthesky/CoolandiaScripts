using Unity.Netcode;
using UnityEngine;

public class DropSpawner : NetworkBehaviour
{
    public static DropSpawner Instance;

    public GameObject dropPrefab; // Assign in inspector
    private int dropPrefabId = -1;

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    private void Start()
    {
        // Find index of dropPrefab in PEM's list
        if (PersistentEntityManager.Instance != null && dropPrefab != null)
        {
            var prefabs = PersistentEntityManager.Instance.entityPrefabs;
            dropPrefabId = System.Array.IndexOf(prefabs, dropPrefab);

            if (dropPrefabId == -1)
                Debug.LogError("[DropSpawner] Drop prefab not found in PEM entityPrefabs list!");
        }
    }

    public void SpawnDrop(ItemData item, Vector3 position)
    {

        if (!IsServer) return;
        if (dropPrefabId == -1 || item == null) return;

        var data = new PersistentEntityData
        {
            prefabId = dropPrefabId,
            position = position + Vector3.up * 0.5f,
            rotation = Quaternion.identity,
            isDestroyed = false
        };

        var go = PersistentEntityManager.Instance.RegisterEntity(data);
        go.GetComponent<ItemDrop>().itemId.Value = item.itemId;
    }
}